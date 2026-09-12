namespace Rpn.Core;

/// <summary>
/// Evaluates an expression in Reverse Polish Notation.
///
/// Walks the tokens once, left to right. Numbers go on the stack; an operator
/// takes its operands off the stack and puts the result back.
/// </summary>
public sealed class Evaluator
{
    private readonly OperatorRegistry _registry;

    public Evaluator(OperatorRegistry registry)
    {
        _registry = registry;
    }

    public double Evaluate(string expression)
    {
        var tokens = Tokenizer.Tokenize(expression);

        if (tokens.Length == 0)
        {
            throw new EvaluationException("Enter an expression.");
        }

        var stack = new Stack<double>();

        foreach (var token in tokens)
        {
            if (_registry.IsOperator(token))
            {
                var op = _registry.Resolve(token);

                var operands = new double[op.Arity];
                for (var i = 0; i < op.Arity; i++)
                {
                    operands[i] = stack.Pop();
                }

                stack.Push(op.Apply(operands));
            }
            else if (int.TryParse(token, out var number))
            {
                stack.Push(number);
            }
            else
            {
                throw new EvaluationException($"\"{token}\" is not a number or an operator.");
            }
        }

        if (stack.Count > 1)
        {
            throw new EvaluationException(
                $"This expression leaves {stack.Count} values. Did you miss an operator?");
        }

        return stack.Pop();
    }
}
