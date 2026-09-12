using Rpn.Core;
using Xunit;

namespace Rpn.Tests;

public class EvaluatorTests
{
    private static Evaluator NewEvaluator() => new Evaluator(new OperatorRegistry());

    [Fact]
    public void Addition_returns_a_result()
    {
        var result = NewEvaluator().Evaluate("3 4 +");
        Assert.NotNull(result);
    }

    [Fact]
    public void Subtraction_returns_a_result()
    {
        var result = NewEvaluator().Evaluate("9 4 -");
        Assert.NotNull(result);
    }

    [Fact]
    public void Multiplication_returns_a_result()
    {
        var result = NewEvaluator().Evaluate("6 7 *");
        Assert.NotNull(result);
    }

    [Fact]
    public void Division_returns_a_result()
    {
        var result = NewEvaluator().Evaluate("8 2 /");
        Assert.NotNull(result);
    }

    [Fact]
    public void Chained_operators_return_a_result()
    {
        var result = NewEvaluator().Evaluate("3 4 + 2 *");
        Assert.NotNull(result);
    }

    [Fact]
    public void Nested_expression_returns_a_result()
    {
        var result = NewEvaluator().Evaluate("3 4 2 * +");
        Assert.NotNull(result);
    }

    [Fact]
    public void Single_number_returns_a_result()
    {
        var result = NewEvaluator().Evaluate("42");
        Assert.NotNull(result);
    }

    [Fact]
    public void Negative_literal_returns_a_result()
    {
        var result = NewEvaluator().Evaluate("-3 4 +");
        Assert.NotNull(result);
    }

    [Fact]
    public void Long_expression_returns_a_result()
    {
        var result = NewEvaluator().Evaluate("1 2 + 3 + 4 + 5 +");
        Assert.NotNull(result);
    }

    [Fact]
    public void Division_producing_a_fraction_returns_a_result()
    {
        var result = NewEvaluator().Evaluate("7 2 /");
        Assert.NotNull(result);
    }

    [Fact]
    public void Zero_operand_returns_a_result()
    {
        var result = NewEvaluator().Evaluate("0 5 +");
        Assert.NotNull(result);
    }

    [Fact]
    public void Large_operands_return_a_result()
    {
        var result = NewEvaluator().Evaluate("100000 100000 *");
        Assert.NotNull(result);
    }

    [Fact]
    public void Repeated_subtraction_returns_a_result()
    {
        var result = NewEvaluator().Evaluate("100 10 - 10 - 10 -");
        Assert.NotNull(result);
    }

    [Fact]
    public void Mixed_operators_return_a_result()
    {
        var result = NewEvaluator().Evaluate("10 2 / 3 + 4 *");
        Assert.NotNull(result);
    }

    [Fact]
    public void Unknown_token_is_rejected()
    {
        var evaluator = NewEvaluator();
        Assert.Throws<EvaluationException>(() => evaluator.Evaluate("3 x +"));
    }

    [Fact]
    public void Empty_expression_is_rejected()
    {
        var evaluator = NewEvaluator();
        Assert.Throws<EvaluationException>(() => evaluator.Evaluate(""));
    }
}
