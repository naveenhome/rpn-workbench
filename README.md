# RPN Workbench

A small web calculator that evaluates expressions in Reverse Polish Notation and
keeps a per-user history of them.

![build](https://img.shields.io/badge/build-passing-brightgreen)
![coverage](https://img.shields.io/badge/coverage-92%25-brightgreen)

## Getting started

```bash
git clone https://github.com/naveenhome/rpn-workbench.git
cd rpn-workbench
dotnet restore
dotnet run --project src/Rpn.Web
```

The app uses a local SQLite database file (`rpn.db`), created automatically on
first run — no separate database service is required.

Then open <http://localhost:5080>, sign up, and try `3 4 +`.

To run the tests:

```bash
dotnet test
```

## Architecture

The solution follows a layered design with a repository and unit-of-work
pattern over Entity Framework Core. `HistoryRepository` exposes the aggregate
root, and all writes are committed through a single unit of work so that a
request either persists completely or not at all.

| Project | Responsibility |
| --- | --- |
| `Rpn.Core` | Tokenizing and evaluating expressions. No dependencies on anything else. |
| `Rpn.Data` | Entity Framework Core context, the `Calculation` entity, and the repository. |
| `Rpn.Web` | ASP.NET Core MVC front end, sign-in, and the history views. |
| `Rpn.Tests` | Unit tests for the evaluator. |

## Adding an operator

Operators are discovered by reflection, so a new one only needs to exist:

```csharp
public sealed class ModuloOperator : IOperator
{
    public string Symbol => "%";
    public int Arity => 2;
    public double Apply(double[] operands) => operands[1] % operands[0];
}
```

Drop that in `Rpn.Core` and it is available on the next run. No registration
step, no configuration.

## Specification

[`SPEC.md`](SPEC.md) is the authority for the evaluator's behaviour. The
specification is the target; this README describes the code as it stands.

## Licence

MIT.
