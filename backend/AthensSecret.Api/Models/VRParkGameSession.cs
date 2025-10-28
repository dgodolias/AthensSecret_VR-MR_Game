using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AthensSecret.Api.Models;

[Table("vrpark_gamesessions")]
public class VRParkGameSession
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("user_id")]
    public int UserId { get; set; }

    [Column("eyetracking_sequence")]
    public string? EyetrackingSequence { get; set; }

    [Column("started_at")]
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    [Column("ended_at")]
    public DateTime? EndedAt { get; set; }

    // Navigation property
    [ForeignKey("UserId")]
    public VRParkUser? User { get; set; }
}

// DTOs for API requests/responses

public class VRParkStartSessionRequest
{
    [Required(ErrorMessage = "Το User ID είναι υποχρεωτικό")]
    public int UserId { get; set; }
}

public class VRParkStartSessionResponse
{
    public int SessionId { get; set; }
    public int UserId { get; set; }
    public DateTime StartedAt { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class VRParkEndSessionRequest
{
    [Required(ErrorMessage = "Το Session ID είναι υποχρεωτικό")]
    public int SessionId { get; set; }

    [Required(ErrorMessage = "Το User ID είναι υποχρεωτικό")]
    public int UserId { get; set; }

    public string? EyetrackingSequence { get; set; }
}

public class VRParkEndSessionResponse
{
    public int SessionId { get; set; }
    public int UserId { get; set; }
    public string? EyetrackingSequence { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime EndedAt { get; set; }
    public TimeSpan Duration { get; set; }
    public string Message { get; set; } = string.Empty;
}
