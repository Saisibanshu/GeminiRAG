using GeminiRAG.Core.Models;
using GeminiRAG.Core.Interfaces;
using GeminiRAG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GeminiRAG.Infrastructure.Services;

/// <summary>
/// Service for tracking query history with database persistence and user isolation
/// </summary>
public class QueryHistoryService : IQueryHistoryService
{
    private readonly ApplicationDbContext _context;

    public QueryHistoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddQueryAsync(Guid userId, Guid? storeId, QueryHistory entry)
    {
        var historyEntity = new Core.Entities.QueryHistory
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            StoreId = storeId,
            Question = entry.Question,
            Answer = entry.Answer,
            Citations = entry.Citations != null && entry.Citations.Any() 
                ? string.Join(",", entry.Citations.Select(c => c.Source))
                : null,
            ResponseTime = (int)entry.ResponseTime.TotalMilliseconds,  // Convert TimeSpan to milliseconds
            IsFound = entry.IsFound,
            Timestamp = DateTime.UtcNow
        };

        _context.QueryHistories.Add(historyEntity);
        await _context.SaveChangesAsync();
    }

    public async Task<List<QueryHistory>> GetHistoryAsync(Guid userId)
    {
        var entities = await _context.QueryHistories
            .Where(q => q.UserId == userId)
            .OrderByDescending(q => q.Timestamp)
            .ToListAsync();

        // Convert entities to model
        return entities.Select(e => new QueryHistory
        {
            Question = e.Question,
            Answer = e.Answer,
            Citations = string.IsNullOrEmpty(e.Citations)
                ? new List<Citation>()
                : e.Citations.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(source => new Citation { Source = source }).ToList(),
            ResponseTime = TimeSpan.FromMilliseconds(e.ResponseTime),  // Convert milliseconds to TimeSpan
            IsFound = e.IsFound,
            Timestamp = e.Timestamp
        }).ToList();
    }

    public async Task ClearHistoryAsync(Guid userId)
    {
        var userHistory = _context.QueryHistories.Where(q => q.UserId == userId);
        _context.QueryHistories.RemoveRange(userHistory);
        await _context.SaveChangesAsync();
    }
}
