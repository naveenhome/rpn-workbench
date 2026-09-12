using System.Reflection;

namespace Rpn.Core;

/// <summary>
/// Finds the operator for a token.
///
/// Operators are discovered by scanning the assembly for implementations of
/// IOperator, so a new operator only has to exist - it does not have to be
/// registered anywhere. Add the class and it works.
/// </summary>
public sealed class OperatorRegistry
{
    private readonly Dictionary<string, IOperator> _operators;

    public OperatorRegistry()
    {
        _operators = new Dictionary<string, IOperator>(StringComparer.Ordinal);

        var operatorTypes = typeof(OperatorRegistry).Assembly
            .GetTypes()
            .Where(t => typeof(IOperator).IsAssignableFrom(t)
                        && !t.IsInterface
                        && !t.IsAbstract);

        foreach (var type in operatorTypes)
        {
            var instance = (IOperator)Activator.CreateInstance(type)!;
            _operators[instance.Symbol] = instance;
        }
    }

    public bool IsOperator(string token) => _operators.ContainsKey(token);

    public IOperator Resolve(string token) => _operators[token];

    public IEnumerable<string> Symbols => _operators.Keys;
}
