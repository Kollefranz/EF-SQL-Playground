using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Common.Migrations
{
    /// <inheritdoc />
    public partial class DropColumnstoreIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IX_Servers_Columnstore ON [Servers];");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "CREATE NONCLUSTERED COLUMNSTORE INDEX IX_Servers_Columnstore ON [Servers] " +
                "([Id], [Hostname], [IpAddress], [OperatingSystem], [CpuCores], [MemoryMb], " +
                "[Status], [Environment], [ProvisionedAt], [DecommissionedAt]);"
            );
        }
    }
}
