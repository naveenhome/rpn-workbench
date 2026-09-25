using System.ComponentModel;
using ModelContextProtocol.Server;
using Rpn.Core;

namespace RpnMcp;

/// <summary>
/// One tool, one job.
///
/// The Description attribute is not documentation. It is what the model reads
/// when deciding whether to call this tool at all, so it is written the way a
/// skill description is written: what it does, and when it applies.
/// </summary>
[McpServerToolType]
public static class RpnTools
{
    [McpServerTool]
    [Description("Evaluate an RPN (postfix) expression such as \"3 4 +\" and return the result.")]
    public static string Evaluate(
        [Description("A postfix expression, tokens separated by spaces. For example: 3 4 + 2 *")]
        string expression)
    {
        try
        {
            var evaluator = new Evaluator(new OperatorRegistry());
            return evaluator.Evaluate(expression).ToString();
        }
        catch (EvaluationException ex)
        {
            // A failed evaluation is an answer, not a crash. Returning the
            // message lets the model tell the user what was wrong with their
            // expression; throwing would only tell it that something broke.
            return $"Could not evaluate \"{expression}\": {ex.Message}";
        }
    }
}
