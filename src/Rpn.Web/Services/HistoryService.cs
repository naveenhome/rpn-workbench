using Rpn.Data;

namespace Rpn.Web.Services;

/// <summary>
/// Sits between the controllers and the repository so the history view and the
/// calculator agree on how an expression is stored and displayed.
/// </summary>
public class HistoryService
{
    private readonly HistoryRepository _repository;

    public HistoryService(HistoryRepository repository)
    {
        _repository = repository;
    }

    public List<Calculation> Recent(string userId) => _repository.ForUser(userId);

    public Calculation? Single(int id) => _repository.Find(id);

    public int Count(string userId) => _repository.CountForUser(userId);

    /// <summary>
    /// Tidies an expression for display. Users paste expressions in from all
    /// sorts of places, and a trailing "=" is common enough to be worth removing.
    /// </summary>
    public string Normalise(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return string.Empty;
        }

        var trimmed = expression.Trim();

        if (trimmed.EndsWith("="))
        {
            trimmed = trimmed.Substring(0, trimmed.Length - 1);
        }

        return string.Join(" ", trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
