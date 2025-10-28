using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AthensSecret.Api.Models;

[Table("vrpark_users")]
public class VRParkUser
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("first_name")]
    [MaxLength(100)]
    public required string FirstName { get; set; }

    [Required]
    [Column("last_name")]
    [MaxLength(100)]
    public required string LastName { get; set; }

    [Column("email")]
    [MaxLength(255)]
    public string? Email { get; set; } // Προαιρετικό

    [Required]
    [Column("age")]
    public int Age { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// DTO για το signup request
public class VRParkSignupRequest
{
    [Required(ErrorMessage = "Το όνομα είναι υποχρεωτικό")]
    [MaxLength(100)]
    public required string FirstName { get; set; }

    [Required(ErrorMessage = "Το επίθετο είναι υποχρεωτικό")]
    [MaxLength(100)]
    public required string LastName { get; set; }

    [EmailAddress(ErrorMessage = "Μη έγκυρο email")]
    [MaxLength(255)]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Η ηλικία είναι υποχρεωτική")]
    [Range(1, 120, ErrorMessage = "Η ηλικία πρέπει να είναι μεταξύ 1 και 120")]
    public int Age { get; set; }
}

// DTO για το signup response
public class VRParkSignupResponse
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public int Age { get; set; }
    public string Message { get; set; } = string.Empty;
}
