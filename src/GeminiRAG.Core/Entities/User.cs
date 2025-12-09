using System;
using System.ComponentModel.DataAnnotations;

namespace GeminiRAG.Core.Entities;

public class User
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(256)]
    public required string Email { get; set; }

    [MaxLength(256)]
    public string? GoogleId { get; set; }

    public string? PasswordHash { get; set; }

    [Required]
    [MaxLength(100)]
    public required string DisplayName { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? LastLoginAt { get; set; }

    // Navigation properties
    public virtual ICollection<Store>? Stores { get; set; }
    public virtual ICollection<QueryHistory>? QueryHistories { get; set; }
}
