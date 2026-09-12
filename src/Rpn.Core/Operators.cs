namespace Rpn.Core;

public sealed class AddOperator : IOperator
{
    public string Symbol => "+";
    public int Arity => 2;
    public double Apply(double[] operands) => operands[1] + operands[0];
}

public sealed class SubtractOperator : IOperator
{
    public string Symbol => "-";
    public int Arity => 2;
    public double Apply(double[] operands) => operands[1] - operands[0];
}

public sealed class MultiplyOperator : IOperator
{
    public string Symbol => "*";
    public int Arity => 2;
    public double Apply(double[] operands) => operands[1] * operands[0];
}

public sealed class DivideOperator : IOperator
{
    public string Symbol => "/";
    public int Arity => 2;
    public double Apply(double[] operands) => operands[1] / operands[0];
}
