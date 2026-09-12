namespace Rpn.Web.Models;

public class CalculatorViewModel
{
    public string? Expression { get; set; }
    public double? Result { get; set; }
    public string? Error { get; set; }
    public List<string> Symbols { get; set; } = new();
}
