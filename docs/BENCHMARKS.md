# Benchmark Log

## Purpose
This document captures manual benchmark evidence while evaluating local Ollama models for the initial HALF runtime.

The goal is to choose a default starting model based on measurable performance, output quality, and fit for constrained local hardware.

## Decision Criteria

| Criterion | What To Look For | Notes |
| :-------- | :--------------- | :---- |
| Task fit | Follows instructions, uses tools correctly, stays concise when needed | |
| Latency | Time to first token and total completion time | |
| Throughput | Tokens per second during steady generation | |
| Resource usage | RAM, VRAM, CPU, GPU utilization | |
| Stability | Repeated runs stay consistent and complete successfully | |
| Local viability | Performs acceptably on the target machine without excessive tuning | |

## Test Environment

| Field | Value |
| :---- | :---- |
| Host machine | Laptop |
| OS | Windows |
| CPU | i7-9750h |
| RAM | 32GB |
| GPU | RTX2060 6GB |
| Storage | 1TB |
| Ollama version | 0.24.0 |
| Context window | 4096 |
| Warm-up runs | 3 |
| Measured runs | 3 |

> Benchmark runs are collected with `scripts/Run-Benchmarks.ps1`.

## Prompt Suite

| Prompt ID | Category          | Description                                        | Expected Output                   |
| :-------- | :---------------- | :------------------------------------------------- | :-------------------------------- |
| P01       | General reasoning | Evaluate a practical tradeoff under constraints    | Structured recommendation         |
| P02       | Code generation   | Generate a small, self-contained function or class | Code block with brief explanation |
| P03       | Code editing      | Modify existing code with minimal changes          | Unified diff                      |
| P04       | Tool-use planning | Choose tools and execution order for a task        | JSON plan                         |
| P05       | Summarization     | Condense technical notes into a short summary      | Markdown summary                  |

### P01 — General reasoning

**Prompt:**

```ps
messages = @(
  @{
    role = "system"
    content = "You are a practical software architecture assistant. Return valid JSON only. Prefer simple, measurable designs over unnecessary complexity."
  },
  @{
    role = "user"
    content = "A small team is building a local development assistant for constrained hardware. The assistant should be observable, safe, and useful, but should avoid overengineering. Should version 1 start as a simple single-service prototype or a distributed multi-service architecture? Return JSON with decision, reasoning, tradeoffs, risk, and confidence."
  }
)
```

### P02 — Code generation

**Prompt:**

```ps
messages = @(
  @{
    role = "system"
    content = "You are a C# code generation assistant. Return valid JSON only. Keep code concise, idiomatic, and dependency-free unless explicitly requested."
  },
  @{
    role = "user"
    content = "Generate a small C# utility class named RetryPolicy. Requirements: retry an asynchronous operation up to a maximum number of attempts, support CancellationToken, use exponential backoff, and do not use external packages. Return JSON with summary, code, notes, and confidence."
  }
)
```

### P03 — Code editing

**Prompt:**

```ps
messages = @(
  @{
    role = "system"
    content = "You are a C# code repair assistant. Return valid JSON only. Prefer minimal edits and avoid unrelated changes."
  },
  @{
    role = "user"
    content = "The following C# method throws when input is null: public static int CountItems(IEnumerable<string> items) { return items.Count(); } Modify the code so null input returns 0. Return JSON with likely_bug, minimal_change, patch, and confidence. The patch field should contain a unified diff as a string."
  }
)
```

### P04 — Tool-use planning

**Prompt:**

```ps
messages = @(
  @{
    role = "system"
    content = "You are a .NET/C# repair assistant. Return valid JSON only. Do not guess package facts. Prefer deterministic checks."
  },
  @{
    role = "user"
    content = "A project fails to build after a class was renamed. Available tools: search_files(query), inspect_file(path), run_build(), list_recent_changes(), search_symbols(symbol_name). Return JSON with likely_cause, first_tool_to_call, first_tool_arguments, followup_tools, stop_condition, and confidence. Rules: do not guess file contents, prefer inspection and search before proposing fixes, and use at most 4 tool calls."
  }
)
```

### P05 — Summarization

**Prompt:**

```ps
messages = @(
  @{
    role = "system"
    content = "You are a technical summarization assistant. Return valid JSON only. Preserve caveats and avoid overclaiming."
  },
  @{
    role = "user"
    content = "Summarize these benchmark notes: Model A responded in 4.2 seconds on a warm run. Model A produced valid JSON for structured-output tasks. Model A sometimes made unsupported factual claims. Model B responded in 2.8 seconds but failed to follow the requested schema twice. Both models were tested with the same context length and output token limit. The benchmark prioritizes reliability and structured output over raw speed. Return JSON with summary, key_findings, recommendation, and caveats."
  }
)
```



## Metrics To Capture
These fields align with the current telemetry direction in HALF.

| Metric | Unit | Required | Notes |
| :----- | :--- | :------- | :---- |
| Model name | text | Yes | Exact Ollama tag |
| Quantization | text | Yes | Example: Q4_K_M |
| Request ID | UUID/text | Optional | If available |
| Load latency | ms | Yes | Time to load model into memory |
| Prompt latency | ms | Yes | Time to process prompt context |
| Generation latency | ms | Yes | Time spent generating output |
| Total latency | ms | Yes | End-to-end runtime |
| Prompt tokens | count | Yes | |
| Completion tokens | count | Yes | |
| Tokens per second | tok/s | Yes | Derived or reported |
| Peak RAM | GB | Yes | |
| Peak VRAM | GB | Optional | Leave blank if not applicable |
| CPU utilization | percent | Optional | |
| GPU utilization | percent | Optional | |
| Success | bool | Yes | |
| Error category | text | Optional | Only if failed |

## Results Summary
Use one row per model/configuration after completing the prompt suite.

| Model | Quant | Params | Avg latency (ms) | Avg tok/s | Peak RAM (GB) | Peak VRAM (GB)  | Overall rank |
| :---- | :---- | :----- | ---------------: | --------: | ------------: | -------------:  | -----------: |
| qwen3.5:2b | TBD | TBD | 4,818 | 64.85 | n/a | n/a | 1 |
| qwen3.5:4b | Q4_K_M | 4.66B | 10,950 | 23.16 | n/a | n/a | 2 |
| Phi-4-mini-instruct | TBD | TBD | TBD | TBD | TBD | TBD | TBD |
| TBD | TBD | TBD | TBD | TBD | TBD | TBD | TBD |

Current artifact set captures latency, token counts, throughput, success, and a single `ollama ps` snapshot. It does not include per-run peak RAM or VRAM measurements, so those cells remain `n/a` until resource telemetry is added to the benchmark script.

## Detailed Runs

### Model: Qwen 3.5:4B
#### Configuration
| Field | Value |
| :---- | :---- |
| Ollama tag | qwen3.5:4b |
| Parameters | think:false, stream:false, format:json |
| Quantization | Q4_K_M |
| Context window | 4096 |
| Temperature | 0.1 |
| Other overrides | num_predict=512 |
| Loaded model snapshot | 6.3 GB reported by `ollama ps`; processor split reported as 30%/70% CPU/GPU |

Warmup summary: P00 ran 3 times before the measured suite, averaging 1,912 ms total latency and 22.85 tok/s.

#### Run Results
| Run | Prompt ID | Load ms | Prompt ms | Generation ms | Total ms | Prompt tokens | Completion tokens | Tok/s | Peak RAM GB | Peak VRAM GB | Success | Notes |
| ---: | :-------- | ------: | --------: | -------------: | -------: | ------------: | ----------------: | ----: | ----------: | ------------: | :------ | :---- |
| 1 | P01 | 297 | 137 | 6,885 | 7,532 | 102 | 158 | 22.95 | n/a | n/a | Yes | Valid JSON |
| 2 | P01 | 286 | 142 | 7,170 | 7,820 | 102 | 166 | 23.15 | n/a | n/a | Yes | Valid JSON |
| 3 | P01 | 298 | 140 | 6,443 | 7,084 | 102 | 149 | 23.13 | n/a | n/a | Yes | Valid JSON |
| 1 | P02 | 305 | 138 | 22,163 | 23,105 | 97 | 512 | 23.10 | n/a | n/a | No | Invalid JSON response |
| 2 | P02 | 329 | 144 | 15,424 | 16,285 | 97 | 356 | 23.08 | n/a | n/a | No | Invalid JSON response |
| 3 | P02 | 302 | 140 | 16,365 | 17,187 | 97 | 376 | 22.98 | n/a | n/a | No | Invalid JSON response |
| 1 | P03 | 319 | 142 | 7,318 | 7,997 | 102 | 171 | 23.37 | n/a | n/a | Yes | Valid JSON |
| 2 | P03 | 307 | 140 | 7,469 | 8,132 | 102 | 173 | 23.16 | n/a | n/a | Yes | Valid JSON |
| 3 | P03 | 300 | 141 | 7,074 | 7,732 | 102 | 165 | 23.33 | n/a | n/a | Yes | Valid JSON |
| 1 | P04 | 290 | 155 | 5,412 | 6,054 | 127 | 127 | 23.47 | n/a | n/a | Yes | Valid JSON |
| 2 | P04 | 306 | 155 | 6,363 | 7,036 | 127 | 147 | 23.10 | n/a | n/a | Yes | Valid JSON |
| 3 | P04 | 308 | 153 | 6,167 | 6,831 | 127 | 143 | 23.19 | n/a | n/a | Yes | Valid JSON |
| 1 | P05 | 285 | 187 | 14,048 | 14,887 | 137 | 325 | 23.13 | n/a | n/a | Yes | Valid JSON |
| 2 | P05 | 287 | 187 | 13,232 | 14,035 | 137 | 306 | 23.13 | n/a | n/a | Yes | Valid JSON |
| 3 | P05 | 282 | 190 | 11,750 | 12,530 | 137 | 272 | 23.15 | n/a | n/a | Yes | Valid JSON |

#### Quality Notes
| Area | Notes |
| :--- | :---- |
| Instruction following | Strong on P01, P03, P04, and P05 across all three measured runs each: the model stayed on-task and returned the requested JSON shape. P02 was the consistent exception, with all three code-generation runs failing the valid-JSON requirement even though the high-level answer intent matched the prompt. |
| Code quality | Mixed. P03 edits were minimal and correct in all runs. P02 outputs showed the right retry/backoff structure, but one sample was truncated mid-note and others had API/design issues such as ignoring `CancellationToken` in the operation signature or describing a static method as an extension method. |
| Hallucination risk | Low to moderate in this prompt set. Outputs stayed anchored to the prompt and did not invent package facts, but P04 sometimes fell back to placeholder identifiers such as `OldClassName` or `old_class_name` instead of giving a more concrete diagnostic path. |
| Formatting discipline | Good overall except for code generation. 12 of 15 measured runs produced valid JSON; all three P02 runs failed schema validity because of malformed JSON in the response payload. |
| Tool-use potential | Moderate to good. P04 consistently chose deterministic search-first workflows and sensible stop conditions, which aligns with the task, but the plans were generic and underused the full tool budget. |
| Failure modes | Primary failure mode is malformed structured output on longer code-heavy responses, especially when emitting large code strings. Secondary issues are generic placeholder values in planning tasks and occasional generated-code defects even when the high-level approach is reasonable. |

### Model: Qwen 3.5:2B
#### Configuration
| Field | Value |
| :---- | :---- |
| Ollama tag | qwen3.5:2b |
| Parameters | think:false, stream:false, format:json |
| Quantization | TBD |
| Context window | 4096 |
| Temperature | 0.1 |
| Other overrides | num_predict=512 |
| Loaded model snapshot | 4.4 GB reported by `ollama ps`; processor split reported as 100% GPU |

Warmup summary: P00 ran 3 times before the measured suite, averaging 9,307 ms total latency and 66.38 tok/s. The first warmup incurred a cold-load penalty of 26,076 ms total, while warmup runs 2 and 3 completed in 945 ms and 899 ms.

#### Run Results
| Run | Prompt ID | Load ms | Prompt ms | Generation ms | Total ms | Prompt tokens | Completion tokens | Tok/s | Peak RAM GB | Peak VRAM GB | Success | Notes |
| ---: | :-------- | ------: | --------: | -------------: | -------: | ------------: | ----------------: | ----: | ----------: | ------------: | :------ | :---- |
| 1 | P01 | 310 | 57 | 4,223 | 4,890 | 102 | 278 | 65.83 | n/a | n/a | Yes | Valid JSON |
| 2 | P01 | 281 | 57 | 2,573 | 3,147 | 102 | 167 | 64.91 | n/a | n/a | Yes | Valid JSON |
| 3 | P01 | 304 | 57 | 2,772 | 3,386 | 102 | 183 | 66.02 | n/a | n/a | Yes | Valid JSON |
| 1 | P02 | 283 | 58 | 6,706 | 7,524 | 97 | 442 | 65.91 | n/a | n/a | Yes | Valid JSON |
| 2 | P02 | 286 | 58 | 6,906 | 7,748 | 97 | 441 | 63.86 | n/a | n/a | Yes | Valid JSON |
| 3 | P02 | 361 | 57 | 7,954 | 8,921 | 97 | 512 | 64.37 | n/a | n/a | No | Invalid JSON response |
| 1 | P03 | 366 | 58 | 2,429 | 3,073 | 102 | 154 | 63.41 | n/a | n/a | Yes | Valid JSON |
| 2 | P03 | 292 | 57 | 2,387 | 2,988 | 102 | 154 | 64.52 | n/a | n/a | Yes | Valid JSON |
| 3 | P03 | 272 | 56 | 2,399 | 2,965 | 102 | 154 | 64.20 | n/a | n/a | Yes | Valid JSON |
| 1 | P04 | 279 | 62 | 2,619 | 3,207 | 127 | 173 | 66.06 | n/a | n/a | Yes | Valid JSON |
| 2 | P04 | 281 | 64 | 3,836 | 4,460 | 127 | 247 | 64.38 | n/a | n/a | Yes | Valid JSON |
| 3 | P04 | 300 | 65 | 2,817 | 3,434 | 127 | 179 | 63.54 | n/a | n/a | Yes | Valid JSON |
| 1 | P05 | 282 | 76 | 4,686 | 5,441 | 137 | 306 | 65.31 | n/a | n/a | Yes | Valid JSON |
| 2 | P05 | 347 | 79 | 4,693 | 5,497 | 137 | 304 | 64.78 | n/a | n/a | Yes | Valid JSON |
| 3 | P05 | 290 | 77 | 4,869 | 5,590 | 137 | 320 | 65.73 | n/a | n/a | Yes | Valid JSON |

#### Quality Notes
| Area | Notes |
| :--- | :---- |
| Instruction following | Strong overall. P01, P03, P04, and P05 succeeded in all three measured runs, and P02 succeeded in two of three runs. The model consistently stayed on-task and usually produced the requested JSON shape. |
| Code quality | Mixed but improved over the 4B runs on formatting. P03 outputs were consistent and minimal. P02 responses generally matched the requested retry/backoff intent, but implementation quality still needed review: one sample misused task collection and exception state, another mixed incompatible delegate signatures, and the failed third run degraded into clearly broken code. |
| Hallucination risk | Low to moderate in this prompt set. Outputs stayed mostly grounded to the prompt, but P04 plans still leaned on generic diagnostic phrasing rather than concrete symbol names or repo-specific checks. |
| Formatting discipline | Better than the 4B runs. 14 of 15 measured runs produced valid JSON; the only schema failure was the third P02 code-generation run. |
| Tool-use potential | Moderate. P04 responses were usable and usually valid JSON, but they sometimes selected weaker first steps such as starting with `search_symbols` and using generic argument placeholders instead of the most deterministic repo-inspection path. |
| Failure modes | Primary failure mode is occasional degradation on longer code-generation outputs, where the model can still produce malformed JSON or broken code. Secondary issues are generic planning advice and code samples that compile only after manual correction. |