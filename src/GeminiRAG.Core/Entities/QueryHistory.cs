using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeminiRAG.Core.Entities;

public class QueryHistory
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    public Guid? StoreId { get; set; }

    [Required]
    public required string Question { get; set; }

    [Required]
    public required string Answer { get; set; }

    public string? Citations { get; set; }  // JSON array of citation strings

    public int ResponseTime { get; set; }

    public bool IsFound { get; set; }

    public DateTime Timestamp { get; set; }

    // Navigation properties
    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [ForeignKey(nameof(StoreId))]
    public Store? Store { get; set; }
}
