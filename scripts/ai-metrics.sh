#!/usr/bin/env bash
#
# ai-metrics.sh — lightweight engineering-signal report for the AI-config feedback loop.
#
# Pulls recently merged PRs from GitHub and prints directional metrics you can paste into
# docs/ai-config-log.md to gauge whether AI-config changes (Copilot instructions, etc.) are
# helping. See Copilot.md "Part 6 — Measuring effectiveness".
#
# These are DIRECTIONAL signals, not proof. Sample size is small and attribution is fuzzy
# (you improve at the codebase while you change config). Watch the basket, not one number.
#
# Metrics:
#   - PR count in the window
#   - Cycle time (open -> merge), median & mean hours
#   - PR size (additions + deletions), median
#   - Review comments per PR (proxy for back-and-forth)
#   - CI first-try-green rate (PRs whose pull_request CI runs never failed) — the headline
#     "are the gates catching less?" signal. Falls if the AI writes non-compliant code.
#
# Requirements: gh (authenticated), jq.
#
# Usage:
#   scripts/ai-metrics.sh                 # last 30 days
#   scripts/ai-metrics.sh --since 60      # last 60 days
#   scripts/ai-metrics.sh --limit 50      # cap number of PRs scanned (default 100)
#
set -euo pipefail

SINCE_DAYS=30
LIMIT=100

while [[ $# -gt 0 ]]; do
  case "$1" in
    --since) SINCE_DAYS="$2"; shift 2 ;;
    --limit) LIMIT="$2"; shift 2 ;;
    -h|--help) sed -n '2,30p' "$0"; exit 0 ;;
    *) echo "Unknown arg: $1" >&2; exit 2 ;;
  esac
done

command -v gh >/dev/null || { echo "error: gh CLI not found (https://cli.github.com)"; exit 1; }
command -v jq >/dev/null || { echo "error: jq not found"; exit 1; }

REPO="$(gh repo view --json nameWithOwner -q .nameWithOwner)"

# Cross-platform "N days ago" in ISO-8601 (BSD date on macOS, GNU date on Linux).
if date -v-1d >/dev/null 2>&1; then
  SINCE="$(date -u -v-"${SINCE_DAYS}"d +%Y-%m-%dT%H:%M:%SZ)"   # macOS / BSD
else
  SINCE="$(date -u -d "${SINCE_DAYS} days ago" +%Y-%m-%dT%H:%M:%SZ)"  # GNU
fi

echo "Repo:   $REPO"
echo "Window: merged since $SINCE (last ${SINCE_DAYS} days)"
echo

# --- Merged PRs in the window --------------------------------------------------------------
prs="$(gh pr list --repo "$REPO" --state merged --limit "$LIMIT" \
  --json number,title,createdAt,mergedAt,additions,deletions,comments,reviews \
  | jq --arg since "$SINCE" '[ .[] | select(.mergedAt >= $since) ]')"

count="$(jq 'length' <<<"$prs")"
if [[ "$count" -eq 0 ]]; then
  echo "No merged PRs in window. (New repo? Try --since 90.)"
  exit 0
fi

# --- CI workflow runs (pull_request event), mapped PR number -> conclusions -----------------
# pull_requests[] is populated for same-repo PRs, which is our case.
runs="$(gh api --paginate "repos/$REPO/actions/runs?event=pull_request&per_page=100" \
  -q '.workflow_runs[] | {n: (.pull_requests[0].number // null), c: .conclusion}' 2>/dev/null \
  | jq -s '.' || echo '[]')"

# --- Aggregate & print ---------------------------------------------------------------------
jq -rn --argjson prs "$prs" --argjson runs "$runs" '
  def median(arr): (arr | sort) as $s | ($s | length) as $n
    | if $n == 0 then 0
      elif ($n % 2) == 1 then $s[($n/2|floor)]
      else (($s[$n/2-1] + $s[$n/2]) / 2) end;

  def hours(a; b): ((b | fromdateiso8601) - (a | fromdateiso8601)) / 3600;

  ($prs | length) as $count
  | [ $prs[] | hours(.createdAt; .mergedAt) ] as $cycle
  | [ $prs[] | (.additions + .deletions) ] as $size
  | [ $prs[] | (.comments | length) ] as $comments

  # group CI runs by PR number; a PR is "first-try-green" if it had >=1 run and none failed/cancelled
  | ($runs | map(select(.n != null)) | group_by(.n)) as $byPr
  | ( $byPr
      | map({ n: .[0].n,
              green: (all(.[]; .c == "success")) })
      | map(select(.green)) | length ) as $greenPrs
  | ( $byPr | length ) as $prsWithRuns

  | "PRs merged in window:        \($count)",
    "Cycle time (median hours):   \(median($cycle) | .*10 | round / 10)",
    "Cycle time (mean hours):     \(($cycle | add / length) | .*10 | round / 10)",
    "PR size (median LOC changed): \(median($size))",
    "Review comments / PR (mean): \(($comments | add / length) | .*10 | round / 10)",
    "",
    "CI first-try-green rate:     \(if $prsWithRuns>0 then ($greenPrs*100/$prsWithRuns | round) else 0 end)% (\($greenPrs)/\($prsWithRuns) PRs with CI runs)",
    "  └─ higher = AI/devs produce passing code up front (gates catch less). Track this over time."
'

echo
echo "Tip: paste these into docs/ai-config-log.md alongside the config change you are evaluating."
