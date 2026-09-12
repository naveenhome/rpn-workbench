namespace Rpn.Core;

/// <summary>
/// Splits an expression into tokens.
/// </summary>
public static class Tokenizer
{
    public static string[] Tokenize(string expression)
    {
        if (expression is null)
        {
            return Array.Empty<string>();
        }

        return expression.Trim().Split(' ');
    }
}
