using System.Text.Json.Serialization;

namespace Common.Entities;

public record NetworkInterfaceEntity
{
    public Guid Id { get; init; }
    public required Guid ServerId { get; init; }
    public required string Name { get; init; }
    public required string MacAddress { get; init; }
    public string? IpAddress { get; init; }
    public string? SubnetMask { get; init; }
    public int? VlanId { get; init; }
    public bool IsEnabled { get; init; } 

    [JsonIgnore]
    public ServerEntity? Server { get; init; }
}
