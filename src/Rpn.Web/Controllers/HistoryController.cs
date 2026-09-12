using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rpn.Web.Services;

namespace Rpn.Web.Controllers;

[Authorize]
public class HistoryController : Controller
{
    private readonly HistoryService _history;

    public HistoryController(HistoryService history)
    {
        _history = history;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        return View(_history.Recent(userId));
    }

    [HttpGet]
    public IActionResult Details(int id)
    {
        var calculation = _history.Single(id);

        if (calculation is null)
        {
            return NotFound();
        }

        return View(calculation);
    }
}
