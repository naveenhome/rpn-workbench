---
name: rpn-spec-conformance
description: Use when changing the evaluator, adding an operator, or reviewing either. Checks the change against SPEC.md sections 4 to 8, and against the worked examples in section 11.
license: MIT
---

# Checking a change against the specification

SPEC.md is the authority. The code is the current state. Where they disagree,
the code is wrong — whether or not a test covers it.

## The procedure

1. **Name the clause first.** Find the section that governs what changed.
   Numbers are section 4, operators are section 5, tokenisation is section 6,
   errors are section 7, persistence is section 8.

2. **Read the whole clause, not the heading.** Three of them contain a
   requirement that contradicts what a C# developer would assume:

   - `%` is **unary percent**, not binary modulo (5.3).
   - `^` requires an **integral** exponent, and must be exact — routing through
     `Math.Pow` does not conform (5.1).
   - Parsing is **culture-invariant**. `3.5` is three and a half in every
     locale (4).

3. **Check the worked examples.** Section 11 is the source for the acceptance
   tests. Every row it lists for the operator you touched must hold.

4. **Run the checker**, which compares the section 11 table against the tests
   that exist:

   ```
   python3 .github/skills/rpn-spec-conformance/check-spec.py
   ```

   It reports which worked examples have a test behind them and which do not.
   A row with no test is not a failure — it is a gap, and worth naming.

5. **Report clause by clause.** For each behaviour touched: the clause, and
   whether the change conforms, contradicts it, or is not covered by it.

## What this skill does not do

It does not decide whether a requirement is sensible. If a clause looks wrong,
say so and stop — changing SPEC.md is a decision for a person, and a change
that quietly brings the specification into line with the code has removed the
only thing that could have caught the code.
