using System.Data;
using System.Text.Json;
using Common.Entities;

namespace Common.Seeding;

/// <summary>
/// Streams ServerJsonEntity rows on demand — no full array allocation.
/// </summary>
public sealed class ServerDataReader(
    long count,
    Action<long>? onProgress = null,
    int progressEvery = 100_000
) : IDataReader
{
    static readonly string[] ColumnNames =
    [
        "Id",
        "Hostname",
        "IpAddress",
        "OperatingSystem",
        "CpuCores",
        "MemoryMb",
        "Status",
        "Environment",
        "ProvisionedAt",
        "DecommissionedAt",
        "Disks",
        "NetworkInterfaces",
        "InstalledServices",
        "Tags",
    ];

    long _index = -1;
    ServerJsonEntity? _current;

    public int FieldCount => ColumnNames.Length;

    public bool Read()
    {
        if (_index + 1 >= count)
        {
            return false;
        }
        _index++;
        _current = FastServerSeeder.GenerateOne(_index);
        if (onProgress != null && (_index + 1) % progressEvery == 0)
        {
            onProgress(_index + 1);
        }
        return true;
    }

    public object GetValue(int i)
    {
        var s = _current!;
        return i switch
        {
            0 => s.Id,
            1 => s.Hostname,
            2 => s.IpAddress,
            3 => s.OperatingSystem,
            4 => s.CpuCores,
            5 => s.MemoryMb,
            6 => s.Status,
            7 => s.Environment,
            8 => s.ProvisionedAt,
            9 => (object?)s.DecommissionedAt ?? DBNull.Value,
            10 => JsonSerializer.Serialize(s.Disks),
            11 => JsonSerializer.Serialize(s.NetworkInterfaces),
            12 => JsonSerializer.Serialize(s.InstalledServices),
            13 => JsonSerializer.Serialize(s.Tags),
            _ => throw new IndexOutOfRangeException($"Column index {i} out of range."),
        };
    }

    public string GetName(int i) => ColumnNames[i];

    public bool IsDBNull(int i) => GetValue(i) is DBNull;

    public object this[int i] => GetValue(i);

    // Not used by SqlBulkCopy
    public void Close() { }

    public void Dispose() { }

    public int Depth => 0;
    public bool IsClosed => false;
    public int RecordsAffected => -1;
    public bool NextResult() => false;
    public DataTable? GetSchemaTable() => null;
    public object this[string name] => throw new NotSupportedException();
    public bool GetBoolean(int i) => throw new NotSupportedException();
    public byte GetByte(int i) => throw new NotSupportedException();
    public long GetBytes(int i, long fieldOffset, byte[]? buffer, int bufferoffset, int length) =>
        throw new NotSupportedException();
    public char GetChar(int i) => throw new NotSupportedException();
    public long GetChars(int i, long fieldoffset, char[]? buffer, int bufferoffset, int length) =>
        throw new NotSupportedException();
    public IDataReader GetData(int i) => throw new NotSupportedException();
    public string GetDataTypeName(int i) => throw new NotSupportedException();
    public DateTime GetDateTime(int i) => throw new NotSupportedException();
    public decimal GetDecimal(int i) => throw new NotSupportedException();
    public double GetDouble(int i) => throw new NotSupportedException();
    public Type GetFieldType(int i) => throw new NotSupportedException();
    public float GetFloat(int i) => throw new NotSupportedException();
    public Guid GetGuid(int i) => throw new NotSupportedException();
    public short GetInt16(int i) => throw new NotSupportedException();
    public int GetInt32(int i) => throw new NotSupportedException();
    public long GetInt64(int i) => throw new NotSupportedException();
    public int GetOrdinal(string name) => throw new NotSupportedException();
    public string GetString(int i) => throw new NotSupportedException();
    public int GetValues(object[] values) => throw new NotSupportedException();
}
