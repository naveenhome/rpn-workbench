# RPN Workbench — Evaluator Specification

**Version 1.0 · 12 September 2026**

This document defines the behaviour the evaluator is required to have. It is the
authority for every acceptance test in the repository.

It describes the **target**, not the current implementation. Section 9 says which
parts are built. Anything in this document that is not in section 9 is work to be
done — and anything the code does that contradicts this document is a bug,
whether or not a test covers it.

---

## 1. Scope

`rpn-workbench` is a web application that evaluates arithmetic expressions written
in Reverse Polish Notation, and keeps a per-user history of what each signed-in
user has evaluated.

This specification covers the **evaluator**: how an expression is read, how it is
computed, what a result is, and how failures are reported. It does not cover the
web interface, authentication, or storage beyond section 8.

---

## 2. Notation

Expressions are written in **postfix** form: operands first, then the operator.

```
3 4 +          → 7
3 4 + 2 *      → 14
3 4 2 * +      → 11
```

Postfix needs no parentheses and no precedence rules, because the order of
operations is fully determined by the order of the tokens. `3 4 + 2 *` and
`3 4 2 * +` are different expressions with different values, and neither is
ambiguous. This is the whole reason the notation is worth teaching: there is
nothing to get wrong about precedence, so every remaining bug is a real one.

---

## 3. The stack machine

Evaluation walks the token list once, left to right, maintaining a stack of
values:

1. A **number** token is pushed onto the stack.
2. An **operator** token pops the number of operands it requires (its *arity*),
   computes a single result, and pushes that result.
3. When the token list is exhausted, the stack **must** contain exactly one
   value. That value is the result of the expression.

Three failure points follow directly from those rules, and all three must be
reported rather than allowed to crash:

| Condition | Error |
|---|---|
| An operator requires more operands than the stack holds | `StackUnderflow` |
| The token list is exhausted with more than one value on the stack | `IncompleteExpression` |
| The token list is empty, or is entirely whitespace | `EmptyExpression` |

> **`3 +` must report `StackUnderflow`.** `+` has arity 2; the stack holds one
> value. This is the single most common defect in a hand-written RPN evaluator
> and it must not reach an unhandled exception.

---

## 4. Numbers

**The numeric type is `decimal`.**

`decimal` is required rather than `double`, for two reasons that matter to this
application:

- Results are shown to users and stored. `0.1 0.2 +` must display `0.3`, not
  `0.30000000000000004`.
- Division by zero must be a reported error, not a value. `decimal` division by
  zero raises; `double` division by zero yields `Infinity`, which is a number,
  which is storable, and which will therefore end up in the history table
  looking like a result.

Consequences:

- **Literals may contain a decimal point**: `3.5`, `0.25`, `-7.125`.
- **Parsing is culture-invariant.** `3.5` is three and a half in every locale the
  application runs in. An implementation that parses with the ambient culture
  will read `3.5` as `35` under a locale that uses `,` as the decimal separator.
  Use invariant parsing.
- **A leading `-` makes a negative literal**, not a subtraction: `-3 4 +` is `1`.
  Subtraction is only ever the `-` *operator*, which is always separated by
  whitespace from its operands.
- **Range.** Results outside the range of `decimal` are an `Overflow` error, not a
  wrapped or infinite value.

---

## 5. Operators

| Symbol | Arity | Reads as | Semantics | Errors |
|---|---|---|---|---|
| `+` | 2 | `a b +` | `a + b` | `Overflow` |
| `-` | 2 | `a b -` | `a - b` | `Overflow` |
| `*` | 2 | `a b *` | `a × b` | `Overflow` |
| `/` | 2 | `a b /` | `a ÷ b` | `DivisionByZero`, `Overflow` |
| `^` | 2 | `a b ^` | `a` raised to the power `b` | `DomainError`, `Overflow` |
| `!` | 1 | `a !` | factorial of `a` | `DomainError`, `Overflow` |
| `%` | 1 | `a %` | `a ÷ 100` | — |

Operands are popped in reverse order: for `a b -`, `b` is popped first, then `a`,
and the result is `a - b`. `7 3 -` is `4`, not `-4`.

### 5.1 `^` — exponentiation

- The exponent **must be integral**. `2 0.5 ^` is a `DomainError`. Fractional
  exponents are out of scope for version 1.0.
- Negative integral exponents are valid: `2 -1 ^` is `0.5`.
- `0 0 ^` is `1`.
- `0` raised to a negative power is a `DomainError`.
- The result must be **exact**. An implementation that routes through
  `Math.Pow` and back loses exactness for large or fractional results and does
  not conform; repeated decimal multiplication does.

### 5.2 `!` — factorial

- Defined for **non-negative integral** values only.
- `0 !` is `1`. This is not a special case to be argued about; it is the
  definition, and an implementation that returns `0` is wrong.
- `-1 !` is a `DomainError`.
- `3.5 !` is a `DomainError`. This case is reachable, because section 4 permits
  decimal literals.
- `27 !` is the largest factorial representable as a `decimal`. `28 !` must be an
  `Overflow` error — not a negative number, not infinity, not a silently wrapped
  value.

### 5.3 `%` — percent

**`%` is a unary percent operator. It is not modulo.**

`a %` pops one value and pushes `a ÷ 100`.

```
50 %           → 0.5
50 % 200 *     → 100
-20 %          → -0.2
```

This is a deliberate departure from the convention in most programming languages,
where `%` is a binary remainder operator. In this application `%` means *per
cent*, because the calculator is used to work out percentages and `7 3 %` meaning
`1` would be useless to that user.

An implementation of `%` with arity 2 that returns a remainder does not conform to
this specification, however reasonable it looks.

---

## 6. Tokenisation

- Tokens are separated by **one or more** whitespace characters. `3  4 +`, with
  two spaces, is the same expression as `3 4 +`.
- Leading and trailing whitespace is ignored.
- Tabs and newlines are whitespace.
- A token that is neither a valid number nor a known operator is an
  `UnknownToken` error, and the error must name the offending token.

> Splitting on a single space character without discarding empty entries produces
> a zero-length token from `3  4 +` and fails on a legal expression. Discard empty
> entries.

---

## 7. Errors

Every failure is one of the following. Each carries a message that names the
problem in terms the user can act on.

| Error | Raised when | Example message |
|---|---|---|
| `EmptyExpression` | Nothing to evaluate | `Enter an expression.` |
| `UnknownToken` | A token is neither number nor operator | `"x" is not a number or an operator.` |
| `StackUnderflow` | An operator has too few operands | `"+" needs 2 values but only 1 is available.` |
| `IncompleteExpression` | More than one value remains | `This expression leaves 2 values. Did you miss an operator?` |
| `DivisionByZero` | Divisor is zero | `Cannot divide by zero.` |
| `DomainError` | Operand outside the operator's domain | `Factorial is only defined for whole numbers of 0 or more.` |
| `Overflow` | Result outside the range of `decimal` | `That result is too large to represent.` |

Requirements:

- An invalid expression **never** produces a stored result.
- An invalid expression **never** produces an unhandled exception or a server
  error page. It returns to the form with the message.
- The expression the user typed is preserved in the form so they can correct it.

---

## 8. Persistence

A successful evaluation records one history row:

| Field | Value |
|---|---|
| `Expression` | The expression as entered, with runs of whitespace collapsed to single spaces and outer whitespace trimmed |
| `Result` | The computed value |
| `EvaluatedAt` | UTC timestamp |
| `UserId` | The signed-in user |

Failed evaluations are not recorded.

**History is private.** A user may read only their own rows. This applies to every
route that exposes a calculation, including any that addresses a row by its
identifier.

---

## 9. Conformance — what is built

| Req | Feature | Status |
|---|---|---|
| 1 | `+` `-` `*` `/` | **Implemented** |
| 2 | Decimal operands | Not implemented — the parser accepts whole numbers only |
| 3 | `^` | Not implemented |
| 4 | `%` | Not implemented |
| 5 | `!` | Not implemented |

Note the shape of requirement 2. The evaluator already computes in a
floating-point type, so `7 2 /` correctly yields `3.5` — but the *parser* rejects
`3.5` as input. The application can produce a decimal it cannot accept. Bringing
the parser, the stored column, the display formatting and the existing tests into
line with section 4 is what requirement 2 asks for.

Sections 1 through 8 are the target regardless of this table. Where the current
implementation of requirement 1 contradicts them, that is a defect to be found and
fixed, not a feature to be added.

---

## 10. Out of scope for version 1.0

- Infix input, or conversion between infix and postfix.
- Fractional exponents.
- Variables, memory registers, or named constants.
- Trigonometric, logarithmic and root operators. (`sqrt` is under consideration;
  see the change log.)
- Expression history beyond the per-user list: no sharing, no export.

---

## 11. Worked examples

These are the source for the acceptance tests. An implementation that satisfies
every row conforms.

| Expression | Result | Note |
|---|---|---|
| `3 4 +` | `7` | |
| `3 4 + 2 *` | `14` | |
| `3 4 2 * +` | `11` | Same tokens, different order, different value |
| `7 3 -` | `4` | Operand order |
| `7 2 /` | `3.5` | |
| `0.1 0.2 +` | `0.3` | Exact — not `0.30000000000000004` |
| `3  4 +` | `7` | Two spaces |
| `-3 4 +` | `1` | Negative literal, not subtraction |
| `2 10 ^` | `1024` | |
| `2 -1 ^` | `0.5` | Negative exponent |
| `0 0 ^` | `1` | |
| `2 0.5 ^` | `DomainError` | Fractional exponent |
| `0 !` | `1` | |
| `5 !` | `120` | |
| `27 !` | `10888869450418352160768000000` | Largest representable |
| `28 !` | `Overflow` | Not a wrapped value |
| `-1 !` | `DomainError` | |
| `3.5 !` | `DomainError` | |
| `50 %` | `0.5` | Percent, **not** modulo |
| `50 % 200 *` | `100` | Fifty per cent of two hundred |
| `-20 %` | `-0.2` | |
| `5 0 /` | `DivisionByZero` | Not infinity, and not stored |
| `3 +` | `StackUnderflow` | |
| `3 4` | `IncompleteExpression` | |
| `3 x +` | `UnknownToken` | Message names `x` |
| `` (empty) | `EmptyExpression` | |

---

## 12. Change log

| Version | Date | Change |
|---|---|---|
| 1.0 | 12 Sep 2026 | First issue. Requirements 1–5. |

---

*Questions about this specification go to the maintainer. Do not infer behaviour
from the implementation — the implementation is what is being measured.*
