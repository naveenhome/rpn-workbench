using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rpn.Core;
using Rpn.Data;
using Rpn.Web.Models;
using Rpn.Web.Services;

namespace Rpn.Web.Controllers;

[Authorize]
public class CalculatorController : Controller
{
    private readonly Evaluator _evaluator;
    private readonly HistoryRepository _history;
    private readonly OperatorRegistry _registry;

    public CalculatorController(
        Evaluator evaluator,
        HistoryRepository history,
        OperatorRegistry registry)
    {
        _evaluator = evaluator;
        _history = history;
        _registry = registry;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(new CalculatorViewModel { Symbols = _registry.Symbols.ToList() });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Index(CalculatorViewModel model)
    {
        model.Symbols = _registry.Symbols.ToList();

        var expression = Normalise(model.Expression ?? string.Empty);

        if (expression.Length == 0)
        {
            model.Error = "Enter an expression.";
            return View(model);
        }

        try
        {
            var result = _evaluator.Evaluate(expression);

            _history.Add(new Calculation
            {
                Expression = expression,
                Result = result,
                EvaluatedAt = DateTime.UtcNow,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty
            });

            model.Result = result;
        }
        catch (EvaluationException ex)
        {
            model.Error = ex.Message;
        }

        return View(model);
    }

    /// <summary>
    /// Tidies an expression before it is evaluated and stored, so that the same
    /// calculation typed two different ways is recorded once.
    /// </summary>
    private static string Normalise(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return string.Empty;
        }

        var trimmed = expression.Trim();

        return string.Join(" ", trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
