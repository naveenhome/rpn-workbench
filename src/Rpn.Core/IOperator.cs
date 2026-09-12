namespace Rpn.Core;

/// <summary>
/// One operator in the calculator. Implement this and the registry will find it.
/// </summary>
public interface IOperator
{
    /// <summary>The token that selects this operator, e.g. "+".</summary>
    string Symbol { get; }

    /// <summary>How many values this operator takes off the stack.</summary>
    int Arity { get; }

    /// <summary>
    /// Applies the operator. Operands arrive in stack order: for "a b -",
    /// operands[0] is b and operands[1] is a.
    /// </summary>
    double Apply(double[] operands);
}
