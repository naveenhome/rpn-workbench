using Microsoft.EntityFrameworkCore;

namespace Rpn.Data;

/// <summary>
/// Reads and writes calculation history.
/// </summary>
public class HistoryRepository
{
    private readonly DbContextOptions<AppDbContext> _options;

    public HistoryRepository(DbContextOptions<AppDbContext> options)
    {
        _options = options;
    }

    public void Add(Calculation calculation)
    {
        var context = new AppDbContext(_options);
        context.Calculations.Add(calculation);
        context.SaveChanges();
    }

    public List<Calculation> ForUser(string userId)
    {
        var context = new AppDbContext(_options);
        return context.Calculations
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.EvaluatedAt)
            .Take(50)
            .ToList();
    }

    public Calculation? Find(int id)
    {
        var context = new AppDbContext(_options);
        return context.Calculations.FirstOrDefault(c => c.Id == id);
    }

    public int CountForUser(string userId)
    {
        var context = new AppDbContext(_options);
        return context.Calculations.Count(c => c.UserId == userId);
    }
}
