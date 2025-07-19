namespace NotesApp.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; set; }
    public required Guid UserId { get; set; }
    public required string Token { get; set; }
    public DateTime ExpiryDate { get; set; }
    public User User { get; set; } = null!;
}