namespace API.DTOs;

public record TagInfoDto
{
    public required Guid ServerId { get; init; }
    public required string TagName { get; init; }
    public required string TagValue { get; init; }
}
