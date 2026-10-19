namespace TaskManager.API.DTOs;

public record ProjectCreateDto(string Name, string? Description);

public record ProjectUpdateDto(string Name, string? Description);

public record AddMemberDto(int UserId);

public record ProjectSummaryDto(
    int Id,
    string Name,
    string? Description,
    UserDto Owner,
    int MemberCount,
    int TaskCount,
    int DoneCount,
    DateTime CreatedAt);

public record ProjectDetailDto(
    int Id,
    string Name,
    string? Description,
    UserDto Owner,
    List<UserDto> Members,
    DateTime CreatedAt);
