using System.ComponentModel.DataAnnotations;

namespace Rpn.Data;

/// <summary>
/// One evaluated expression, kept so a user can look back at what they worked out.
/// </summary>
public class Calculation
{
    public int Id { get; set; }

    [Required]
    public string Expression { get; set; } = string.Empty;

    public double Result { get; set; }

    public DateTime EvaluatedAt { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;
}
