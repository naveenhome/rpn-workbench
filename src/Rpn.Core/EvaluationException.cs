namespace Rpn.Core;

/// <summary>
/// Raised when an expression cannot be evaluated. The message is shown to the
/// user, so it should say what is wrong in terms they can act on.
/// </summary>
public class EvaluationException : Exception
{
    public EvaluationException(string message) : base(message)
    {
    }
}
