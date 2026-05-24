# ---------------------------------------------
# HALF Local Model Benchmark Runner
# ---------------------------------------------

# --- Benchmark config ---
$defaultModel = "qwen3.5:4b"
$runCount = 3
$numCtx = 4096
$numPredict = 512
$temperature = 0.1
$ollamaUri = "http://localhost:11434/api/chat"

$selectedModel = Read-Host "Enter model name [$defaultModel]"
$model = if ([string]::IsNullOrWhiteSpace($selectedModel)) {
    $defaultModel
}
else {
    $selectedModel.Trim()
}

Write-Host "Using model: $model"

# Optional output files
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$safeModelName = $model -replace '[<>:"/\\|?*]', '-'
$outputDirectory = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\docs\benchmarks"))

if (-not (Test-Path -LiteralPath $outputDirectory)) {
    New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
}

$resultsCsv = Join-Path $outputDirectory "$safeModelName-benchmark-results-$timestamp.csv"
$summaryCsv = Join-Path $outputDirectory "$safeModelName-benchmark-summary-$timestamp.csv"
$responseLog = Join-Path $outputDirectory "$safeModelName-benchmark-responses-$timestamp.txt"

function Write-Section {
    param([string]$Title)

    Write-Host ""
    Write-Host "============================================="
    Write-Host $Title
    Write-Host "============================================="
}

function Invoke-OllamaBenchmarkPrompt {
    param(
        [string]$Model,
        [string]$SystemPrompt,
        [string]$UserPrompt,
        [int]$NumCtx,
        [int]$NumPredict,
        [double]$Temperature,
        [string]$Uri
    )

    $body = @{
        model    = $Model
        think    = $false
        stream   = $false
        format   = "json"
        messages = @(
            @{
                role    = "system"
                content = $SystemPrompt
            },
            @{
                role    = "user"
                content = $UserPrompt
            }
        )
        options  = @{
            num_ctx     = $NumCtx
            temperature = $Temperature
            num_predict = $NumPredict
        }
    } | ConvertTo-Json -Depth 8

    Invoke-RestMethod `
        -Uri $Uri `
        -Method Post `
        -Body $body `
        -ContentType "application/json"
}

function Invoke-BenchmarkRun {
    param(
        [hashtable]$Prompt,
        [int]$Run,
        [string]$Model,
        [int]$NumCtx,
        [int]$NumPredict,
        [double]$Temperature,
        [string]$Uri
    )

    $success = $true
    $notes = ""
    $responseText = $null
    $response = $null

    try {
        $response = Invoke-OllamaBenchmarkPrompt `
            -Model $Model `
            -SystemPrompt $Prompt.SystemPrompt `
            -UserPrompt $Prompt.UserPrompt `
            -NumCtx $NumCtx `
            -NumPredict $NumPredict `
            -Temperature $Temperature `
            -Uri $Uri

        $responseText = $response.message.content

        try {
            $null = $responseText | ConvertFrom-Json
            $notes = "Valid JSON"
        }
        catch {
            $success = $false
            $notes = "Invalid JSON response"
        }
    }
    catch {
        $success = $false
        $notes = "API error: $($_.Exception.Message)"
    }

    [pscustomobject]@{
        Row          = [pscustomobject]@{
            Run              = $Run
            PromptID         = $Prompt.PromptID
            Category         = $Prompt.Category
            Model            = $Model
            Think            = $false
            NumCtx           = $NumCtx
            NumPredict       = $NumPredict
            Temperature      = $Temperature
            LoadMs           = if ($response) { [math]::Round($response.load_duration / 1e6, 0) } else { $null }
            PromptMs         = if ($response) { [math]::Round($response.prompt_eval_duration / 1e6, 0) } else { $null }
            GenerationMs     = if ($response) { [math]::Round($response.eval_duration / 1e6, 0) } else { $null }
            TotalMs          = if ($response) { [math]::Round($response.total_duration / 1e6, 0) } else { $null }
            PromptTokens     = if ($response) { $response.prompt_eval_count } else { $null }
            CompletionTokens = if ($response) { $response.eval_count } else { $null }
            TokPerSec        = if ($response -and $response.eval_duration -gt 0) {
                [math]::Round($response.eval_count / ($response.eval_duration / 1e9), 2)
            }
            else {
                $null
            }
            Success          = if ($success) { "Yes" } else { "No" }
            Notes            = $notes
        }
        ResponseText = $responseText
    }
}

# ---------------------------------------------
# Prompt suite
# ---------------------------------------------

$prompts = @(
    @{
        PromptID     = "P00"
        Category     = "Warmup"
        SystemPrompt = "You are a benchmark warmup assistant. Return valid JSON only."
        UserPrompt   = "Return JSON with status, purpose, and confidence. The status should be ready. The purpose should be warmup."
    },
    @{
        PromptID     = "P01"
        Category     = "General reasoning"
        SystemPrompt = "You are a practical software architecture assistant. Return valid JSON only. Prefer simple, measurable designs over unnecessary complexity."
        UserPrompt   = "A small team is building a local development assistant for constrained hardware. The assistant should be observable, safe, and useful, but should avoid overengineering. Should version 1 start as a simple single-service prototype or a distributed multi-service architecture? Return JSON with decision, reasoning, tradeoffs, risk, and confidence."
    },
    @{
        PromptID     = "P02"
        Category     = "Code generation"
        SystemPrompt = "You are a C# code generation assistant. Return valid JSON only. Keep code concise, idiomatic, and dependency-free unless explicitly requested."
        UserPrompt   = "Generate a small C# utility class named RetryPolicy. Requirements: retry an asynchronous operation up to a maximum number of attempts, support CancellationToken, use exponential backoff, and do not use external packages. Return JSON with summary, code, notes, and confidence."
    },
    @{
        PromptID     = "P03"
        Category     = "Code editing"
        SystemPrompt = "You are a C# code repair assistant. Return valid JSON only. Prefer minimal edits and avoid unrelated changes."
        UserPrompt   = "The following C# method throws when input is null: public static int CountItems(IEnumerable<string> items) { return items.Count(); } Modify the code so null input returns 0. Return JSON with likely_bug, minimal_change, patch, and confidence. The patch field should contain a unified diff as a string."
    },
    @{
        PromptID     = "P04"
        Category     = "Tool-use planning"
        SystemPrompt = "You are a .NET/C# repair assistant. Return valid JSON only. Do not guess package facts. Prefer deterministic checks."
        UserPrompt   = "A project fails to build after a class was renamed. Available tools: search_files(query), inspect_file(path), run_build(), list_recent_changes(), search_symbols(symbol_name). Return JSON with likely_cause, first_tool_to_call, first_tool_arguments, followup_tools, stop_condition, and confidence. Rules: do not guess file contents, prefer inspection and search before proposing fixes, and use at most 4 tool calls."
    },
    @{
        PromptID     = "P05"
        Category     = "Summarization"
        SystemPrompt = "You are a technical summarization assistant. Return valid JSON only. Preserve caveats and avoid overclaiming."
        UserPrompt   = "Summarize these benchmark notes: Model A responded in 4.2 seconds on a warm run. Model A produced valid JSON for structured-output tasks. Model A sometimes made unsupported factual claims. Model B responded in 2.8 seconds but failed to follow the requested schema twice. Both models were tested with the same context length and output token limit. The benchmark prioritizes reliability and structured output over raw speed. Return JSON with summary, key_findings, recommendation, and caveats."
    }
)

# ---------------------------------------------
# Measured runs
# ---------------------------------------------

$results = [System.Collections.Generic.List[object]]::new()
$responses = [System.Collections.Generic.List[string]]::new()

foreach ($prompt in $prompts) {
    Write-Section "Running $($prompt.PromptID): $($prompt.Category)"

    for ($i = 1; $i -le $runCount; $i++) {
        $runResult = Invoke-BenchmarkRun `
            -Prompt $prompt `
            -Run $i `
            -Model $model `
            -NumCtx $numCtx `
            -NumPredict $numPredict `
            -Temperature $temperature `
            -Uri $ollamaUri

        $row = $runResult.Row
        $results.Add($row)
        $responses.Add("PromptID: $($row.PromptID) | Category: $($row.Category) | Run: $($row.Run) | Success: $($row.Success)")
        $responses.Add($runResult.ResponseText)
        $responses.Add("--------------------------------------------------")

        Write-Host ""
        Write-Host "Run $i response:"
        Write-Host $runResult.ResponseText
        Write-Host ""
        $row | Format-List
    }
}

# ---------------------------------------------
# Summary by prompt
# ---------------------------------------------

$summary = $results |
Group-Object PromptID |
ForEach-Object {
    $group = $_.Group
    $first = $group | Select-Object -First 1

    [pscustomobject]@{
        PromptID            = $first.PromptID
        Category            = $first.Category
        Runs                = $group.Count
        SuccessfulRuns      = ($group | Where-Object { $_.Success -eq "Yes" }).Count
        AvgLoadMs           = [math]::Round(($group | Measure-Object LoadMs -Average).Average, 0)
        AvgPromptMs         = [math]::Round(($group | Measure-Object PromptMs -Average).Average, 0)
        AvgGenerationMs     = [math]::Round(($group | Measure-Object GenerationMs -Average).Average, 0)
        AvgTotalMs          = [math]::Round(($group | Measure-Object TotalMs -Average).Average, 0)
        AvgPromptTokens     = [math]::Round(($group | Measure-Object PromptTokens -Average).Average, 2)
        AvgCompletionTokens = [math]::Round(($group | Measure-Object CompletionTokens -Average).Average, 2)
        AvgTokPerSec        = [math]::Round(($group | Measure-Object TokPerSec -Average).Average, 2)
    }
}

# ---------------------------------------------
# Output
# ---------------------------------------------

Write-Section "Response Log"
Write-Host ($responses -join "`n")

Write-Section "Run Results"
$results | Format-Table -AutoSize

Write-Section "Summary by Prompt"
$summary | Format-Table -AutoSize

$results | Export-Csv -Path $resultsCsv -NoTypeInformation
$summary | Export-Csv -Path $summaryCsv -NoTypeInformation
$responses | Out-File -FilePath $responseLog -Encoding utf8


Write-Host ""
Write-Host "Saved run results to: $resultsCsv"
Write-Host "Saved summary to:     $summaryCsv"
Write-Host "Saved responses to:   $responseLog"