namespace NotesApp.Application.DTOs;

public class UserDto
{
    public Guid Id { get; set; }
    public required string Username { get; set; }
}