using System.Text.Json.Serialization;

namespace Common.Entities;

public record ServerTagEntity
{
    public Guid Id { get; init; }
    public int ServerId { get; init; }
    public required string Key { get; init; }
    public required string Value { get; init; }

    [JsonIgnore]
    public ServerEntity? Server { get; init; }
}
