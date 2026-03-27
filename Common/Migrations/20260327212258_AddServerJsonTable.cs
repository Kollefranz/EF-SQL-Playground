using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Common.Migrations
{
    /// <inheritdoc />
    public partial class AddServerJsonTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServersJson",
                columns: table => new
                {
                    RowId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    Hostname = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    OperatingSystem = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CpuCores = table.Column<int>(type: "int", nullable: false),
                    MemoryMb = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Environment = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProvisionedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DecommissionedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Disks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InstalledServices = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NetworkInterfaces = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tags = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServersJson", x => x.RowId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServersJson_Id",
                table: "ServersJson",
                column: "Id",
                unique: true);

            // Copy existing normalized data into JSON columns.
            migrationBuilder.Sql(@"
INSERT INTO [ServersJson] (
    [Id], [Hostname], [IpAddress], [OperatingSystem], [CpuCores], [MemoryMb],
    [Status], [Environment], [ProvisionedAt], [DecommissionedAt],
    [Disks], [NetworkInterfaces], [InstalledServices], [Tags]
)
SELECT
    s.[Id], s.[Hostname], s.[IpAddress], s.[OperatingSystem], s.[CpuCores], s.[MemoryMb],
    s.[Status], s.[Environment], s.[ProvisionedAt], s.[DecommissionedAt],
    ISNULL((SELECT d.[Id], d.[MountPoint], d.[DiskType], d.[CapacityGb], d.[UsedGb]
            FROM [Disks] d WHERE d.[ServerId] = s.[RowId] FOR JSON PATH), N'[]'),
    ISNULL((SELECT n.[Id], n.[Name], n.[MacAddress], n.[IpAddress], n.[SubnetMask], n.[VlanId], n.[IsEnabled]
            FROM [NetworkInterfaces] n WHERE n.[ServerId] = s.[RowId] FOR JSON PATH), N'[]'),
    ISNULL((SELECT i.[Id], i.[Name], i.[Version], i.[Port], i.[Status], i.[InstalledAt]
            FROM [InstalledServices] i WHERE i.[ServerId] = s.[RowId] FOR JSON PATH), N'[]'),
    ISNULL((SELECT t.[Id], t.[Key], t.[Value]
            FROM [ServerTags] t WHERE t.[ServerId] = s.[RowId] FOR JSON PATH), N'[]')
FROM [Servers] s;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServersJson");
        }
    }
}
