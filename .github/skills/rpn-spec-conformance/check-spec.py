#!/usr/bin/env python3
"""Which worked examples from SPEC.md section 11 have a test behind them?

Reads the section 11 table, then greps the test projects for each expression.
It proves nothing about correctness — a test that mentions an expression may
still assert nothing, which is the point module 6 makes. What it does show is
the gap between what the specification promises and what anything checks.

    python3 .github/skills/rpn-spec-conformance/check-spec.py
"""

import pathlib
import re
import sys

ROOT = pathlib.Path(__file__).resolve().parents[3]
SPEC = ROOT / 'SPEC.md'
TESTS = ROOT / 'tests'


def worked_examples(text):
    """Rows of the section 11 table: (expression, expected)."""
    try:
        section = text.split('## 11. Worked examples', 1)[1].split('\n## ', 1)[0]
    except IndexError:
        sys.exit('SPEC.md has no section 11 — has it been renumbered?')

    rows = []
    for line in section.splitlines():
        cells = [c.strip() for c in line.split('|')[1:-1]]
        if len(cells) < 2:
            continue
        expr, expected = cells[0], cells[1]
        if expr in ('Expression', '') or set(expr) <= set('-: '):
            continue
        rows.append((expr.strip('`'), expected.strip('`')))
    return rows


def main():
    if not SPEC.exists():
        sys.exit(f'no SPEC.md at {SPEC}')

    rows = worked_examples(SPEC.read_text(encoding='utf-8'))
    if not rows:
        sys.exit('section 11 parsed but produced no rows')

    sources = []
    if TESTS.exists():
        sources = [p.read_text(encoding='utf-8', errors='replace')
                   for p in TESTS.rglob('*.cs')]
    blob = '\n'.join(sources)

    covered, missing = [], []
    for expr, expected in rows:
        # The expression as it would appear inside a C# string literal.
        needle = expr.replace('\\', '\\\\').replace('"', '\\"')
        (covered if needle and needle in blob else missing).append((expr, expected))

    print(f'SPEC.md section 11: {len(rows)} worked examples')
    print(f'  mentioned in a test : {len(covered)}')
    print(f'  mentioned nowhere   : {len(missing)}\n')

    if missing:
        print('No test mentions these:')
        width = max(len(e) for e, _ in missing)
        for expr, expected in missing:
            print(f'  {expr.ljust(width)}   ->  {expected}')
        print()

    print('A mention is not a check. Read the assertion before you believe it.')
    return 0


if __name__ == '__main__':
    raise SystemExit(main())
