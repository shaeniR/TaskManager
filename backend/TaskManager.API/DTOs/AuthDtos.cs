namespace TaskManager.API.DTOs;

public record RegisterDto(string Name, string Email, string Password);

public record LoginDto(string Email, string Password);

public record UserDto(int Id, string Name, string Email, string Role);

public record AuthResponseDto(string Token, DateTime ExpiresAt, UserDto User);
