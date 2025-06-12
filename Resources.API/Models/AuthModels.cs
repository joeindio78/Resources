namespace Resources.API.Models;

public record LoginRequest(string Email, string Password);
public record LoginResponse(string Token);
public record User(string Email, string Password); 