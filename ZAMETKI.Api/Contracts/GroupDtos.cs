namespace ZAMETKI.Api.Contracts;

public record GroupDto(string Id, string Name, string TeacherId, DateTime CreatedDate);

public record CreateGroupRequest(string Name);
