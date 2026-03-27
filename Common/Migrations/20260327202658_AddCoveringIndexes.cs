using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Common.Migrations
{
    /// <inheritdoc />
    public partial class AddCoveringIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ServerTags_ServerId",
                table: "ServerTags");

            migrationBuilder.DropIndex(
                name: "IX_NetworkInterfaces_ServerId",
                table: "NetworkInterfaces");

            migrationBuilder.DropIndex(
                name: "IX_InstalledServices_ServerId",
                table: "InstalledServices");

            migrationBuilder.DropIndex(
                name: "IX_Disks_ServerId",
                table: "Disks");

            migrationBuilder.CreateIndex(
                name: "IX_ServerTags_ServerId",
                table: "ServerTags",
                column: "ServerId")
                .Annotation("SqlServer:Include", new[] { "Id", "Key", "Value" });

            migrationBuilder.CreateIndex(
                name: "IX_NetworkInterfaces_ServerId",
                table: "NetworkInterfaces",
                column: "ServerId")
                .Annotation("SqlServer:Include", new[] { "Id", "Name", "MacAddress", "IpAddress", "SubnetMask", "VlanId", "IsEnabled" });

            migrationBuilder.CreateIndex(
                name: "IX_InstalledServices_ServerId",
                table: "InstalledServices",
                column: "ServerId")
                .Annotation("SqlServer:Include", new[] { "Id", "Name", "Version", "Port", "Status", "InstalledAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Disks_ServerId",
                table: "Disks",
                column: "ServerId")
                .Annotation("SqlServer:Include", new[] { "Id", "MountPoint", "CapacityGb", "DiskType", "UsedGb" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ServerTags_ServerId",
                table: "ServerTags");

            migrationBuilder.DropIndex(
                name: "IX_NetworkInterfaces_ServerId",
                table: "NetworkInterfaces");

            migrationBuilder.DropIndex(
                name: "IX_InstalledServices_ServerId",
                table: "InstalledServices");

            migrationBuilder.DropIndex(
                name: "IX_Disks_ServerId",
                table: "Disks");

            migrationBuilder.CreateIndex(
                name: "IX_ServerTags_ServerId",
                table: "ServerTags",
                column: "ServerId");

            migrationBuilder.CreateIndex(
                name: "IX_NetworkInterfaces_ServerId",
                table: "NetworkInterfaces",
                column: "ServerId");

            migrationBuilder.CreateIndex(
                name: "IX_InstalledServices_ServerId",
                table: "InstalledServices",
                column: "ServerId");

            migrationBuilder.CreateIndex(
                name: "IX_Disks_ServerId",
                table: "Disks",
                column: "ServerId");
        }
    }
}
