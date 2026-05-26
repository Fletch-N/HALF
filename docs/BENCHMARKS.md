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

| Model | Quant | Params | Avg latency (ms) | Avg tok/s | Peak RAM (GB) | Peak VRAM (GB)  | Overall rank |
| :---- | :---- | :----- | ---------------: | --------: | ------------: | -------------:  | -----------: |
| nemotron-3-nano:4b | TBD | 4B | 4,207 | 55.25 | n/a | n/a | 1 |
| qwen3.5:2b | TBD | TBD | 4,818 | 64.85 | n/a | n/a | 2 |
| granite4.1:3b | TBD | 3B | 9,581 | 48.07 | n/a | n/a | 3 |
| phi4-mini:3.8b | TBD | 3.8B | 16,897 | 47.23 | n/a | n/a | 4 |
| qwen3.5:4b | Q4_K_M | 4.66B | 10,950 | 23.16 | n/a | n/a | 5 |
| gemma4:e2b | TBD | TBD | 11,680 | 25.72 | n/a | n/a | 6 |
| deepseek-r1:1.5b | TBD | 1.5B | 9,434 | 59.13 | n/a | n/a | 7 |
| ministral-3:3b | TBD | 3B | 5,865 | 64.86 | n/a | n/a | 8 |

Current artifact set captures latency, token counts, throughput, success, and a single `ollama ps` snapshot. It does not include per-run peak RAM or VRAM measurements, so those cells remain `n/a` until resource telemetry is added to the benchmark script.

### Rationale

1. Nemotron 3 Nano 4B. Best overall balance. It is the fastest of the fully reliable models, went 15/15 on valid JSON, stayed concise, and was especially stable on reasoning, editing, and summarization. Its main weakness is semantic correctness inside code generation, but that is a narrower problem than schema failure.

2. Qwen 3.5 2B. Best practical small-model contender. It was very fast, had strong overall task adherence, and only dropped one structured-output run out of fifteen. I place it above Granite because the latency and general usefulness gain is large, while the reliability drop is small rather than systemic.

3. Granite 4.1 3B. Most structurally dependable alongside Nemotron, with 15/15 valid JSON, but weaker than Nemotron and Qwen 2B on practical usefulness because code-task quality still had meaningful defects and average latency is much higher. Safe protocol behavior, less convincing substance.

4. Phi-4-mini 3.8B. I would move it above the malformed-output cluster because 15/15 schema compliance matters for an agent runtime. That said, this is a reluctant fourth place: its code quality and planning quality were weak enough that the outputs still need heavy human review.

5. Qwen 3.5 4B. Better than the remaining low-ranked models because it was strong on four prompt classes and generally stayed grounded, but failing all three code-generation runs on valid JSON is too large a reliability penalty for a coding assistant. It is usable, but not dependable enough as a default.

6. Gemma4:E2B. Very similar profile to Qwen 4B, but slower and heavier with no compensating quality win. Strong on reasoning, editing, planning, and summarization, yet the consistent P02 schema failure keeps it out of the top half.

7. DeepSeek-R1 1.5B. Attractive on size and occasional speed, but too uneven. The benchmark notes show both schema breakdown and weak code-centric reasoning, which is a bad combination for an assistant expected to be observable and reliable.

8. Ministral 3 3B. Clear last place despite excellent throughput. It breaks the benchmark’s highest-priority requirement too often, with only 5/15 valid JSON runs and failures across reasoning, code generation, and summarization. Fast failure is still failure.

## Detailed Runs

Full log files can be found in `docs/benchmarks/$model-$size-*`

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

### Model: Nemotron 3 Nano 4B
#### Configuration
| Field | Value |
| :---- | :---- |
| Ollama tag | nemotron-3-nano:4b |
| Parameters | think:false, stream:false, format:json |
| Quantization | TBD |
| Context window | 4096 |
| Temperature | 0.1 |
| Other overrides | num_predict=512 |
| Loaded model snapshot | 5.2 GB reported by `ollama ps`; processor split reported as 100% GPU |

Warmup summary: P00 ran 3 times before the measured suite, averaging 10,611 ms total latency and 56.74 tok/s. The first warmup incurred a cold-load penalty of 29,968 ms total, while warmup runs 2 and 3 completed in 967 ms and 898 ms.

#### Run Results
| Run | Prompt ID | Load ms | Prompt ms | Generation ms | Total ms | Prompt tokens | Completion tokens | Tok/s | Peak RAM GB | Peak VRAM GB | Success | Notes |
| ---: | :-------- | ------: | --------: | -------------: | -------: | ------------: | ----------------: | ----: | ----------: | ------------: | :------ | :---- |
| 1 | P01 | 220 | 94 | 2,503 | 2,959 | 104 | 141 | 56.33 | n/a | n/a | Yes | Valid JSON |
| 2 | P01 | 229 | 94 | 2,255 | 2,706 | 104 | 126 | 55.89 | n/a | n/a | Yes | Valid JSON |
| 3 | P01 | 211 | 91 | 2,769 | 3,201 | 104 | 154 | 55.61 | n/a | n/a | Yes | Valid JSON |
| 1 | P02 | 201 | 88 | 8,807 | 9,377 | 99 | 489 | 55.52 | n/a | n/a | Yes | Valid JSON |
| 2 | P02 | 211 | 91 | 7,660 | 8,221 | 99 | 423 | 55.22 | n/a | n/a | Yes | Valid JSON |
| 3 | P02 | 202 | 88 | 8,960 | 9,547 | 99 | 490 | 54.69 | n/a | n/a | Yes | Valid JSON |
| 1 | P03 | 211 | 93 | 1,513 | 1,928 | 105 | 84 | 55.53 | n/a | n/a | Yes | Valid JSON |
| 2 | P03 | 207 | 92 | 1,521 | 1,916 | 105 | 84 | 55.24 | n/a | n/a | Yes | Valid JSON |
| 3 | P03 | 204 | 92 | 1,499 | 1,901 | 105 | 83 | 55.36 | n/a | n/a | Yes | Valid JSON |
| 1 | P04 | 206 | 131 | 2,069 | 2,525 | 136 | 114 | 55.11 | n/a | n/a | Yes | Valid JSON |
| 2 | P04 | 208 | 128 | 1,976 | 2,427 | 136 | 109 | 55.18 | n/a | n/a | Yes | Valid JSON |
| 3 | P04 | 256 | 130 | 1,967 | 2,475 | 136 | 107 | 54.39 | n/a | n/a | Yes | Valid JSON |
| 1 | P05 | 237 | 130 | 4,077 | 4,615 | 139 | 224 | 54.94 | n/a | n/a | Yes | Valid JSON |
| 2 | P05 | 206 | 130 | 4,266 | 4,775 | 139 | 234 | 54.86 | n/a | n/a | Yes | Valid JSON |
| 3 | P05 | 212 | 136 | 4,004 | 4,538 | 139 | 220 | 54.95 | n/a | n/a | Yes | Valid JSON |

#### Quality Notes
| Area | Notes |
| :--- | :---- |
| Instruction following | Strong overall. All 15 measured runs returned valid JSON and stayed aligned with the requested task shape. P01, P03, and P05 were especially consistent and concise across runs. |
| Code quality | Moderate. P03 outputs were consistently minimal and correct. P02 responses were structurally strong, but the generated retry helpers still had important implementation defects such as not awaiting async operations in some runs, invalid `TimeSpan` math, disposing a `CancellationToken`, and inconsistent use of `Task` versus `ValueTask`. |
| Hallucination risk | Low to moderate in this prompt set. The model stayed grounded to the prompt and avoided obvious package hallucinations, but it still presented flawed code and generic architectural claims with high confidence. |
| Formatting discipline | Excellent. All 15 measured runs produced valid JSON, and the model maintained stable schema compliance even on longer code-generation outputs. |
| Tool-use potential | Moderate. P04 responses were valid and coherent, but the plans were generic and sometimes chose weaker first steps such as broad symbol search for a rename issue instead of a more deterministic inspection path. |
| Failure modes | Primary failure mode is semantic correctness inside otherwise clean output. The model is fast and structurally reliable, but code-generation results still require review for compile-time and API-level correctness. |

### Model: DeepSeek-R1 1.5B
#### Configuration
| Field | Value |
| :---- | :---- |
| Ollama tag | deepseek-r1:1.5b |
| Parameters | think:false, stream:false, format:json |
| Quantization | TBD |
| Context window | 4096 |
| Temperature | 0.1 |
| Other overrides | num_predict=512 |
| Loaded model snapshot | 1.4 GB reported by `ollama ps`; processor split reported as 100% GPU |

Warmup summary: P00 ran 3 times before the measured suite, averaging 13,394 ms total latency and 59.64 tok/s. The first warmup incurred a cold-load penalty of 36,566 ms total, while warmup runs 2 and 3 completed in 1,798 ms and 1,818 ms.

#### Run Results
| Run | Prompt ID | Load ms | Prompt ms | Generation ms | Total ms | Prompt tokens | Completion tokens | Tok/s | Peak RAM GB | Peak VRAM GB | Success | Notes |
| ---: | :-------- | ------: | --------: | -------------: | -------: | ------------: | ----------------: | ----: | ----------: | ------------: | :------ | :---- |
| 1 | P01 | 169 | 38 | 1,425 | 6,024 | 92 | 89 | 62.47 | n/a | n/a | Yes | Valid JSON |
| 2 | P01 | 0 | 0 | 0 | 0 | n/a | n/a | n/a | n/a | n/a | No | Invalid JSON response |
| 3 | P01 | 149 | 11 | 996 | 4,163 | 92 | 61 | 61.21 | n/a | n/a | Yes | Valid JSON |
| 1 | P02 | 189 | 29 | 8,151 | 32,366 | 87 | 512 | 62.81 | n/a | n/a | No | Invalid JSON response |
| 2 | P02 | 151 | 12 | 7,705 | 33,447 | 87 | 512 | 66.45 | n/a | n/a | No | Invalid JSON response |
| 3 | P02 | 150 | 12 | 1,395 | 6,796 | 87 | 104 | 74.58 | n/a | n/a | Yes | Valid JSON |
| 1 | P03 | 147 | 40 | 955 | 3,949 | 92 | 60 | 62.84 | n/a | n/a | Yes | Valid JSON |
| 2 | P03 | 150 | 11 | 1,623 | 7,251 | 92 | 113 | 69.62 | n/a | n/a | Yes | Valid JSON |
| 3 | P03 | 168 | 11 | 1,400 | 6,634 | 92 | 102 | 72.84 | n/a | n/a | Yes | Valid JSON |
| 1 | P04 | 145 | 50 | 1,440 | 5,751 | 117 | 89 | 61.80 | n/a | n/a | Yes | Valid JSON |
| 2 | P04 | 150 | 11 | 1,371 | 5,232 | 117 | 83 | 60.53 | n/a | n/a | Yes | Valid JSON |
| 3 | P04 | 151 | 12 | 1,549 | 5,652 | 117 | 90 | 58.11 | n/a | n/a | Yes | Valid JSON |
| 1 | P05 | 149 | 39 | 2,373 | 8,619 | 127 | 135 | 56.89 | n/a | n/a | Yes | Valid JSON |
| 2 | P05 | 152 | 12 | 2,169 | 7,940 | 127 | 125 | 57.63 | n/a | n/a | Yes | Valid JSON |
| 3 | P05 | 160 | 11 | 2,031 | 7,684 | 127 | 120 | 59.08 | n/a | n/a | Yes | Valid JSON |

#### Quality Notes
| Area | Notes |
| :--- | :---- |
| Instruction following | Uneven. P03, P04, and P05 succeeded in all three measured runs, but P01 had one malformed response and P02 only succeeded once out of three runs. When the model stayed within the schema, it generally remained on-task and concise. |
| Code quality | Weak for generation and inconsistent for editing. P02 frequently produced unusable output, including non-C# syntax, nested JSON instead of code, and malformed or truncated payloads. P03 responses were valid JSON but still questionable on substance, with runs that claimed the original code already handled null input or suggested non-existent APIs such as `CountOrDefault`. |
| Hallucination risk | Moderate. The model stayed close to the benchmark prompts, but it confidently introduced unsupported APIs, confused language constructs, and generic assumptions in planning responses. |
| Formatting discipline | Mixed. 11 of 15 measured runs produced valid JSON, but failures were severe when they happened: one P01 run collapsed into truncated output, and two P02 runs produced invalid schema payloads despite long completions. |
| Tool-use potential | Limited to moderate. P04 outputs were valid JSON and roughly followed the requested shape, but they were generic, often named the wrong first tool, and did not show strong repo-aware troubleshooting. |
| Failure modes | Primary failure modes are schema breakdown and low-quality code-centric reasoning. The model can be very fast on successful runs, but it is less reliable on structured reasoning and code generation than the stronger alternatives in this benchmark set. |

### Model: Granite 4.1 3B
#### Configuration
| Field | Value |
| :---- | :---- |
| Ollama tag | granite4.1:3b |
| Parameters | think:false, stream:false, format:json |
| Quantization | TBD |
| Context window | 4096 |
| Temperature | 0.1 |
| Other overrides | num_predict=512 |
| Loaded model snapshot | 2.7 GB reported by `ollama ps`; processor split reported as 100% GPU |

Warmup summary: P00 ran 3 times before the measured suite, averaging 6,223 ms total latency and 50.02 tok/s. The first warmup incurred a cold-load penalty of 15,458 ms total, while warmup runs 2 and 3 completed in 1,617 ms and 1,594 ms.

#### Run Results
| Run | Prompt ID | Load ms | Prompt ms | Generation ms | Total ms | Prompt tokens | Completion tokens | Tok/s | Peak RAM GB | Peak VRAM GB | Success | Notes |
| ---: | :-------- | ------: | --------: | -------------: | -------: | ------------: | ----------------: | ----: | ----------: | ------------: | :------ | :---- |
| 1 | P01 | 106 | 1,051 | 4,264 | 12,403 | 98 | 196 | 45.97 | n/a | n/a | Yes | Valid JSON |
| 2 | P01 | 107 | 14 | 4,337 | 11,589 | 98 | 202 | 46.58 | n/a | n/a | Yes | Valid JSON |
| 3 | P01 | 107 | 20 | 3,599 | 10,433 | 98 | 176 | 48.90 | n/a | n/a | Yes | Valid JSON |
| 1 | P02 | 111 | 76 | 6,075 | 16,583 | 93 | 289 | 47.57 | n/a | n/a | Yes | Valid JSON |
| 2 | P02 | 132 | 20 | 6,084 | 17,058 | 93 | 296 | 48.66 | n/a | n/a | Yes | Valid JSON |
| 3 | P02 | 107 | 17 | 6,206 | 16,753 | 93 | 293 | 47.21 | n/a | n/a | Yes | Valid JSON |
| 1 | P03 | 113 | 81 | 710 | 2,281 | 98 | 37 | 52.12 | n/a | n/a | Yes | Valid JSON |
| 2 | P03 | 111 | 17 | 1,381 | 3,906 | 98 | 68 | 49.24 | n/a | n/a | Yes | Valid JSON |
| 3 | P03 | 105 | 19 | 1,362 | 4,280 | 98 | 72 | 52.85 | n/a | n/a | Yes | Valid JSON |
| 1 | P04 | 108 | 102 | 2,070 | 5,686 | 123 | 97 | 46.85 | n/a | n/a | Yes | Valid JSON |
| 2 | P04 | 109 | 18 | 1,966 | 5,592 | 123 | 97 | 49.35 | n/a | n/a | Yes | Valid JSON |
| 3 | P04 | 107 | 18 | 2,175 | 5,770 | 123 | 100 | 45.98 | n/a | n/a | Yes | Valid JSON |
| 1 | P05 | 100 | 96 | 3,965 | 10,644 | 133 | 184 | 46.41 | n/a | n/a | Yes | Valid JSON |
| 2 | P05 | 125 | 19 | 4,033 | 10,750 | 133 | 185 | 45.87 | n/a | n/a | Yes | Valid JSON |
| 3 | P05 | 103 | 19 | 3,664 | 9,986 | 133 | 174 | 47.49 | n/a | n/a | Yes | Valid JSON |

#### Quality Notes
| Area | Notes |
| :--- | :---- |
| Instruction following | Strong overall. All 15 measured runs returned valid JSON and stayed aligned with the requested task shape. P01, P04, and P05 were consistently on-spec, while P02 and P03 completed successfully but still needed substance review. |
| Code quality | Mixed. P02 outputs were well-formed and directionally correct, but the generated retry helpers still had implementation issues such as returning a value from a non-generic `Task` method, missing `System.Threading` imports for `CancellationToken`, using `Thread.Sleep` inside async flow, and not actually implementing exponential backoff. P03 improved on runs 2 and 3, but run 1 produced a broken patch that removed logic instead of fixing the null case. |
| Hallucination risk | Low to moderate in this prompt set. The model stayed anchored to the benchmark prompts and avoided wild factual invention, but it still produced questionable API usage and overconfident explanations for flawed code. |
| Formatting discipline | Excellent. All 15 measured runs produced valid JSON, making Granite 4.1 3B the most structurally reliable model in this benchmark set so far. |
| Tool-use potential | Moderate. P04 consistently selected a sensible search-first workflow and returned a usable schema, but the plans remained generic and did not exploit repo-specific evidence or the full tool budget. |
| Failure modes | Primary failure mode is semantic quality rather than schema compliance: answers are usually well-formed and fast, but code-oriented tasks can still contain compile or logic defects that require human review. |

### Model: Gemma4:E2B
#### Configuration
| Field | Value |
| :---- | :---- |
| Ollama tag | gemma4:e2b |
| Parameters | think:false, stream:false, format:json |
| Quantization | TBD |
| Context window | 4096 |
| Temperature | 0.1 |
| Other overrides | num_predict=512 |
| Loaded model snapshot | 7.8 GB reported by `ollama ps`; processor split reported as 74%/26% CPU/GPU |

Warmup summary: P00 ran 3 times before the measured suite, averaging 16,639 ms total latency and 25.04 tok/s. The first warmup incurred a cold-load penalty of 45,696 ms total, while warmup runs 2 and 3 completed in 2,183 ms and 2,037 ms.

#### Run Results
| Run | Prompt ID | Load ms | Prompt ms | Generation ms | Total ms | Prompt tokens | Completion tokens | Tok/s | Peak RAM GB | Peak VRAM GB | Success | Notes |
| ---: | :-------- | ------: | --------: | -------------: | -------: | ------------: | ----------------: | ----: | ----------: | ------------: | :------ | :---- |
| 1 | P01 | 446 | 92 | 12,446 | 13,603 | 100 | 325 | 26.11 | n/a | n/a | Yes | Valid JSON |
| 2 | P01 | 435 | 44 | 12,846 | 13,991 | 100 | 339 | 26.39 | n/a | n/a | Yes | Valid JSON |
| 3 | P01 | 396 | 41 | 12,646 | 13,743 | 100 | 325 | 25.70 | n/a | n/a | Yes | Valid JSON |
| 1 | P02 | 410 | 91 | 20,396 | 21,669 | 96 | 512 | 25.10 | n/a | n/a | No | Invalid JSON response |
| 2 | P02 | 383 | 43 | 20,352 | 21,540 | 96 | 512 | 25.16 | n/a | n/a | No | Invalid JSON response |
| 3 | P02 | 391 | 40 | 20,454 | 21,624 | 96 | 512 | 25.03 | n/a | n/a | No | Invalid JSON response |
| 1 | P03 | 394 | 89 | 5,246 | 6,292 | 104 | 133 | 25.35 | n/a | n/a | Yes | Valid JSON |
| 2 | P03 | 404 | 43 | 5,254 | 6,261 | 104 | 133 | 25.31 | n/a | n/a | Yes | Valid JSON |
| 3 | P03 | 438 | 42 | 4,625 | 5,556 | 104 | 119 | 25.73 | n/a | n/a | Yes | Valid JSON |
| 1 | P04 | 390 | 113 | 4,132 | 5,077 | 141 | 108 | 26.14 | n/a | n/a | Yes | Valid JSON |
| 2 | P04 | 410 | 43 | 4,105 | 5,017 | 141 | 109 | 26.55 | n/a | n/a | Yes | Valid JSON |
| 3 | P04 | 404 | 45 | 4,285 | 5,178 | 141 | 109 | 25.44 | n/a | n/a | Yes | Valid JSON |
| 1 | P05 | 428 | 107 | 10,928 | 12,095 | 131 | 282 | 25.81 | n/a | n/a | Yes | Valid JSON |
| 2 | P05 | 388 | 45 | 10,468 | 11,508 | 131 | 273 | 26.08 | n/a | n/a | Yes | Valid JSON |
| 3 | P05 | 384 | 42 | 11,020 | 12,049 | 131 | 286 | 25.95 | n/a | n/a | Yes | Valid JSON |

#### Quality Notes
| Area | Notes |
| :--- | :---- |
| Instruction following | Strong on P01, P03, P04, and P05 across all three measured runs each: the model stayed on-task and returned the requested JSON shape. P02 was the consistent exception, with all three code-generation runs failing the valid-JSON requirement even though the high-level answer intent matched the prompt. |
| Code quality | Mixed. P03 edits were minimal and correct in all runs. P02 outputs showed the right retry/backoff structure, but the responses had design issues and malformed payloads, including incomplete code strings and invalid exception construction, so the generated code would still need manual repair. |
| Hallucination risk | Low to moderate in this prompt set. Outputs stayed anchored to the prompt and did not invent package facts, but P04 sometimes fell back to generic placeholder arguments instead of producing a more repo-specific diagnostic path. |
| Formatting discipline | Good overall except for code generation. 12 of 15 measured runs produced valid JSON; all three P02 runs failed schema validity because of malformed JSON in the response payload. |
| Tool-use potential | Moderate to good. P04 consistently chose deterministic search-first workflows and sensible stop conditions, which aligns with the task, but the plans were generic and underused the available tool budget. |
| Failure modes | Primary failure mode is malformed structured output on longer code-heavy responses, especially when emitting large code strings. Secondary issues are generic placeholder values in planning tasks and generated-code defects even when the high-level approach is reasonable. |

### Model: Phi-4-mini 3.8B
#### Configuration
| Field | Value |
| :---- | :---- |
| Ollama tag | phi4-mini:3.8b |
| Parameters | think:false, stream:false, format:json |
| Quantization | TBD |
| Context window | 4096 |
| Temperature | 0.1 |
| Other overrides | num_predict=512 |
| Loaded model snapshot | 3.3 GB reported by `ollama ps`; processor split reported as 100% GPU |

Warmup summary: P00 ran 3 times before the measured suite, averaging 12,436 ms total latency and 49.28 tok/s. The first warmup incurred a cold-load penalty of 32,135 ms total, while warmup runs 2 and 3 completed in 2,563 ms and 2,611 ms.

#### Run Results
| Run | Prompt ID | Load ms | Prompt ms | Generation ms | Total ms | Prompt tokens | Completion tokens | Tok/s | Peak RAM GB | Peak VRAM GB | Success | Notes |
| ---: | :-------- | ------: | --------: | -------------: | -------: | ------------: | ----------------: | ----: | ----------: | ------------: | :------ | :---- |
| 1 | P01 | 198 | 82 | 5,928 | 26,250 | 89 | 268 | 45.21 | n/a | n/a | Yes | Valid JSON |
| 2 | P01 | 203 | 15 | 3,610 | 17,131 | 89 | 177 | 49.03 | n/a | n/a | Yes | Valid JSON |
| 3 | P01 | 204 | 20 | 5,758 | 26,904 | 89 | 275 | 47.76 | n/a | n/a | Yes | Valid JSON |
| 1 | P02 | 199 | 76 | 5,996 | 26,287 | 85 | 282 | 47.04 | n/a | n/a | Yes | Valid JSON |
| 2 | P02 | 205 | 19 | 7,990 | 35,338 | 85 | 385 | 48.19 | n/a | n/a | Yes | Valid JSON |
| 3 | P02 | 198 | 19 | 8,943 | 38,104 | 85 | 414 | 46.30 | n/a | n/a | Yes | Valid JSON |
| 1 | P03 | 244 | 68 | 875 | 4,068 | 89 | 41 | 46.83 | n/a | n/a | Yes | Valid JSON |
| 2 | P03 | 200 | 20 | 1,227 | 5,551 | 89 | 58 | 47.28 | n/a | n/a | Yes | Valid JSON |
| 3 | P03 | 206 | 15 | 1,274 | 5,549 | 89 | 58 | 45.52 | n/a | n/a | Yes | Valid JSON |
| 1 | P04 | 200 | 77 | 2,274 | 10,101 | 114 | 105 | 46.18 | n/a | n/a | Yes | Valid JSON |
| 2 | P04 | 199 | 20 | 1,934 | 8,919 | 114 | 94 | 48.61 | n/a | n/a | Yes | Valid JSON |
| 3 | P04 | 201 | 15 | 1,942 | 8,799 | 114 | 92 | 47.37 | n/a | n/a | Yes | Valid JSON |
| 1 | P05 | 204 | 89 | 2,890 | 13,171 | 124 | 139 | 48.10 | n/a | n/a | Yes | Valid JSON |
| 2 | P05 | 205 | 17 | 2,820 | 13,481 | 124 | 140 | 49.64 | n/a | n/a | Yes | Valid JSON |
| 3 | P05 | 203 | 15 | 3,169 | 13,806 | 124 | 144 | 45.44 | n/a | n/a | Yes | Valid JSON |

#### Quality Notes
| Area | Notes |
| :--- | :---- |
| Instruction following | Strong structurally. All 15 measured runs returned valid JSON and stayed within the requested task shapes. The model was verbose on P01 and especially long on P02, which contributed heavily to total latency. |
| Code quality | Weak to moderate. P03 outputs targeted the null case but were not reliably minimal or correct, and the P02 retry-policy answers contained major C# defects such as invalid syntax, mixed exception syntax from other languages, broken field naming, malformed control flow, and blocking waits inside async-oriented code. |
| Hallucination risk | Moderate. The model generally stayed on the benchmark prompt, but it introduced unsupported code constructs, odd metadata such as a generated date, and generic planning placeholders with high confidence. |
| Formatting discipline | Excellent. All 15 measured runs produced valid JSON, even when the payload content itself was low quality or excessively long. |
| Tool-use potential | Limited to moderate. P04 outputs were syntactically valid, but they were weak diagnostically: empty symbol names, placeholder paths, and premature boolean stop conditions made the plans much less actionable than the stronger models. |
| Failure modes | Primary failure mode is poor semantic quality hidden behind clean structure. Phi-4-mini stays schema-compliant, but it is slow on reasoning and code generation tasks and still requires close human review for correctness. |

### Model: Ministral 3 3B
#### Configuration
| Field | Value |
| :---- | :---- |
| Ollama tag | ministral-3:3b |
| Parameters | think:false, stream:false, format:json |
| Quantization | TBD |
| Context window | 4096 |
| Temperature | 0.1 |
| Other overrides | num_predict=512 |
| Loaded model snapshot | 5.0 GB reported by `ollama ps`; processor split reported as 100% GPU |

Warmup summary: P00 ran 3 times before the measured suite, averaging 3,065 ms total latency and 71.72 tok/s. The first warmup incurred a cold-load penalty of 6,555 ms total, while warmup runs 2 and 3 completed in 1,331 ms and 1,308 ms.

#### Run Results
| Run | Prompt ID | Load ms | Prompt ms | Generation ms | Total ms | Prompt tokens | Completion tokens | Tok/s | Peak RAM GB | Peak VRAM GB | Success | Notes |
| ---: | :-------- | ------: | --------: | -------------: | -------: | ------------: | ----------------: | ----: | ----------: | ------------: | :------ | :---- |
| 1 | P01 | 203 | 46 | 7,346 | 8,130 | 93 | 512 | 69.70 | n/a | n/a | No | Invalid JSON response |
| 2 | P01 | 245 | 14 | 7,419 | 8,216 | 93 | 512 | 69.02 | n/a | n/a | No | Invalid JSON response |
| 3 | P01 | 205 | 12 | 7,377 | 8,121 | 93 | 512 | 69.41 | n/a | n/a | No | Invalid JSON response |
| 1 | P02 | 226 | 45 | 7,504 | 8,425 | 88 | 512 | 68.23 | n/a | n/a | No | Invalid JSON response |
| 2 | P02 | 203 | 13 | 7,464 | 8,341 | 88 | 512 | 68.60 | n/a | n/a | No | Invalid JSON response |
| 3 | P02 | 216 | 12 | 7,438 | 8,210 | 88 | 512 | 68.84 | n/a | n/a | No | Invalid JSON response |
| 1 | P03 | 214 | 46 | 1,842 | 2,407 | 94 | 129 | 70.02 | n/a | n/a | Yes | Valid JSON |
| 2 | P03 | 208 | 13 | 1,799 | 2,365 | 94 | 129 | 71.70 | n/a | n/a | Yes | Valid JSON |
| 3 | P03 | 234 | 13 | 1,839 | 2,415 | 94 | 129 | 70.15 | n/a | n/a | Yes | Valid JSON |
| 1 | P04 | 0 | 0 | 0 | 0 | n/a | n/a | n/a | n/a | n/a | No | Invalid JSON response |
| 2 | P04 | 241 | 17 | 2,296 | 3,105 | 125 | 160 | 69.68 | n/a | n/a | Yes | Valid JSON |
| 3 | P04 | 209 | 13 | 2,277 | 3,076 | 125 | 160 | 70.25 | n/a | n/a | Yes | Valid JSON |
| 1 | P05 | 239 | 62 | 7,410 | 8,282 | 128 | 512 | 69.09 | n/a | n/a | No | Invalid JSON response |
| 2 | P05 | 207 | 11 | 7,387 | 8,639 | 128 | 512 | 69.31 | n/a | n/a | No | Invalid JSON response |
| 3 | P05 | 243 | 12 | 7,438 | 8,249 | 128 | 512 | 68.84 | n/a | n/a | No | Invalid JSON response |

#### Quality Notes
| Area | Notes |
| :--- | :---- |
| Instruction following | Weak overall despite strong raw speed. The model only succeeded consistently on P03 and partly on P04; it failed all measured runs for P01, P02, and P05 because it did not produce valid JSON under the requested schema. |
| Code quality | Mixed to weak. P03 outputs were the only consistently usable slice, but even there the patches shown in the response log were logically incorrect because they placed the null check after `items.Count()`. P02 outputs were long and fast, but the payloads did not remain valid JSON and the visible code had correctness issues such as invalid `catch`/`else` flow and incorrect `TimeSpan` math. |
| Hallucination risk | Moderate. The model stayed on the broad task, but it added irrelevant frameworks and unsupported implementation claims, especially in reasoning prompts that over-elaborated and then broke schema constraints. |
| Formatting discipline | Poor for benchmark purposes. Only 5 of 15 measured runs produced valid JSON, and every failure mode in this run set was a schema-validity failure rather than a transport or timeout issue. |
| Tool-use potential | Limited. The successful P04 runs were acceptable, but one run failed outright and the surviving plans did not stand out for determinism or repo awareness. |
| Failure modes | Primary failure mode is schema collapse at higher completion lengths: the model is fast and high-throughput, but it tends to run to the token limit and break structured-output reliability on reasoning, code generation, and summarization. |