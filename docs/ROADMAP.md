# Roadmap

```mermaid
gantt
    title HALF — Evolution to v1.0
    dateFormat  YYYY-MM-DD
    axisFormat  %d
    tickInterval 1d

    section v0.1: Lightweight Scaffolding
    Docs & Specs (Current)      :active, t1, 2026-05-22, 2d
    CLI Bootstrap               :t2, 2026-05-22, 2d

    section v0.2: Measurable Runtimes
    Observability & Metrics     :t3, after t2, 2d
    Ollama Local Integration    :t4, after t3, 2d

    section v0.3: Deterministic Tools
    Reusable Tool Surfaces      :milestone, t5, after t4, 0d
    Safe Execution Boundaries   :milestone, t6, after t5, 0d

    section v0.4: Agentic Core
    Small Context Window Logic  :milestone, t7, after t6, 0d
    Explicit Runtime Boundaries :milestone, t8, after t7, 0d

    section v0.5: Background Jobs
    Constrained Workflow Engine :milestone, t9, after t8, 0d
    Portable Linux/Win Workers  :milestone, t10, after t9, 0d

    section v0.6: Stable Foundation
    v1.0.0 Release Candidate    :milestone, t11, after t10, 0d
```

## v0.1: Scaffolding
- Establish the .NET 10 solution and project structure.
- Keep the initial code shallow and explicit.
- Deliver core project structure and CLI wiring.

## v0.2: Observability-First Runtime Integration
- Add the Ollama runtime adapter.
- Introduce run records, benchmark records, and per-run traces.
- Capture latency, token usage, and resource telemetry for normal model operation.
- Expose the first CLI workflows for `run`, `benchmark`, `trace`, and `status`.

## v0.3: Tools and Gateway
- Introduce tool contracts and a lightweight tool registry.
- Add deterministic local tools for files, search, shell, and validation.
- Route tool execution through policy and observability boundaries.

## v0.4: Agentic Core
- Add the bounded agent loop in `HALF.Agent`.
- Keep deterministic orchestration in control and introduce LLM planning behind explicit execution limits.

## v0.5: Background Jobs
- Add queued background work via `HALF.Jobs`.
- Expand data retention, structured exports, and longer-lived local state.

## v0.6: Platform Growth
- Promote the stack from local laptop development to a Linux VM deployment.
- Add richer log and trace backends only when the local-first baseline is proven.

## v1.0: TBD
- Stable, production-ready working release.