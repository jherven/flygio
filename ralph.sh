#!/bin/bash
# Ralph Wiggum loop for Flygio.se
# Usage: ./ralph.sh [max_iterations]

MAX_ITERATIONS=${1:-30}
ITERATION=0

echo "Starting Ralph Wiggum loop for Flygio.se"
echo "Max iterations: $MAX_ITERATIONS"
echo "---"

while [ $ITERATION -lt $MAX_ITERATIONS ]; do
    ITERATION=$((ITERATION + 1))
    echo ""
    echo "=== Iteration $ITERATION / $MAX_ITERATIONS ==="
    echo "$(date '+%Y-%m-%d %H:%M:%S')"
    echo ""

    # Run Claude Code with the prompt
    claude --print -p "$(cat PROMPT.md)"

    # Check if EXIT_SIGNAL is set in activity.md
    if grep -q "EXIT_SIGNAL: true" activity.md 2>/dev/null; then
        echo ""
        echo "EXIT_SIGNAL detected! Project complete."
        echo "Completed in $ITERATION iterations."
        exit 0
    fi

    echo ""
    echo "--- Iteration $ITERATION complete, continuing... ---"
    sleep 5
done

echo ""
echo "Max iterations ($MAX_ITERATIONS) reached."
echo "Check activity.md and plan.md for current status."
exit 1
