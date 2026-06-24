# AI-config decision log

A running log of changes to AI-assistant configuration (Copilot instructions, prompts, AGENTS.md,
quality gates) and their measured effect. The point is a disciplined loop — **change one lever,
measure, decide** — not statistical proof. See `Copilot.md` "Part 6 — Measuring effectiveness".

How to use:
1. Before a change, run `scripts/ai-metrics.sh` to capture a **baseline** row.
2. Make **one** config change. Note the hypothesis and the threshold that would make you keep it.
3. After the time-box (≈4 weeks / N PRs), run `scripts/ai-metrics.sh` again and record the result + decision.

Metric reminders (directional only — small sample, fuzzy attribution):
- **First-try-green** = % of PRs whose CI never failed (the headline "gates catch less" signal).
- **Cycle time** = open→merge hours. **Size** = LOC changed. **Comments/PR** = review back-and-forth.

---

## Log

| Date | Change | Hypothesis | First-try-green | Cycle time (med h) | Comments/PR | Decision |
|------|--------|------------|-----------------|--------------------|-------------|----------|
| YYYY-MM-DD | _baseline (before AI config)_ | — | _%_ | _h_ | _n_ | baseline |
| 2026-06-24 | Land repo-wide `copilot-instructions.md` + path instructions + quality gates | Compliant code up front → fewer gate failures, fewer review nits | _tbd_ | _tbd_ | _tbd_ | measuring (review ~2026-07-22) |

<!--
Template row:
| 2026-07-22 | Added `add-endpoint` prompt | Faster, layering-correct endpoints | 78% | 9.1 | 1.2 | keep — first-try-green +12pp |
-->
