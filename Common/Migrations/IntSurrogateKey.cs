using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Common.Migrations
{
    /// <inheritdoc />
    public partial class IntSurrogateKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Existing data cannot be migrated (uniqueidentifier → int); truncate before schema change.
            migrationBuilder.Sql("DELETE FROM [InstalledServices];");
            migrationBuilder.Sql("DELETE FROM [NetworkInterfaces];");
            migrationBuilder.Sql("DELETE FROM [Disks];");
            migrationBuilder.Sql("DELETE FROM [ServerTags];");
            migrationBuilder.Sql("DELETE FROM [Servers];");

            migrationBuilder.DropForeignKey(
                name: "FK_Disks_Servers_ServerId",
                table: "Disks");

            migrationBuilder.DropForeignKey(
                name: "FK_InstalledServices_Servers_ServerId",
                table: "InstalledServices");

            migrationBuilder.DropForeignKey(
                name: "FK_NetworkInterfaces_Servers_ServerId",
                table: "NetworkInterfaces");

            migrationBuilder.DropForeignKey(
                name: "FK_ServerTags_Servers_ServerId",
                table: "ServerTags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Servers",
                table: "Servers");

            // SQL Server cannot ALTER COLUMN between uniqueidentifier and int;
            // drop covering indexes first, then drop+add the column.
            migrationBuilder.DropIndex(name: "IX_ServerTags_ServerId", table: "ServerTags");
            migrationBuilder.DropIndex(name: "IX_NetworkInterfaces_ServerId", table: "NetworkInterfaces");
            migrationBuilder.DropIndex(name: "IX_InstalledServices_ServerId", table: "InstalledServices");
            migrationBuilder.DropIndex(name: "IX_Disks_ServerId", table: "Disks");

            migrationBuilder.DropColumn(name: "ServerId", table: "ServerTags");
            migrationBuilder.DropColumn(name: "ServerId", table: "NetworkInterfaces");
            migrationBuilder.DropColumn(name: "ServerId", table: "InstalledServices");
            migrationBuilder.DropColumn(name: "ServerId", table: "Disks");

            migrationBuilder.AddColumn<int>(
                name: "ServerId",
                table: "ServerTags",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ServerId",
                table: "NetworkInterfaces",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ServerId",
                table: "InstalledServices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ServerId",
                table: "Disks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RowId",
                table: "Servers",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Servers",
                table: "Servers",
                column: "RowId");

            migrationBuilder.CreateIndex(
                name: "IX_Servers_Id",
                table: "Servers",
                column: "Id",
                unique: true);

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

            migrationBuilder.AddForeignKey(
                name: "FK_Disks_Servers_ServerId",
                table: "Disks",
                column: "ServerId",
                principalTable: "Servers",
                principalColumn: "RowId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InstalledServices_Servers_ServerId",
                table: "InstalledServices",
                column: "ServerId",
                principalTable: "Servers",
                principalColumn: "RowId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NetworkInterfaces_Servers_ServerId",
                table: "NetworkInterfaces",
                column: "ServerId",
                principalTable: "Servers",
                principalColumn: "RowId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ServerTags_Servers_ServerId",
                table: "ServerTags",
                column: "ServerId",
                principalTable: "Servers",
                principalColumn: "RowId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Disks_Servers_ServerId",
                table: "Disks");

            migrationBuilder.DropForeignKey(
                name: "FK_InstalledServices_Servers_ServerId",
                table: "InstalledServices");

            migrationBuilder.DropForeignKey(
                name: "FK_NetworkInterfaces_Servers_ServerId",
                table: "NetworkInterfaces");

            migrationBuilder.DropForeignKey(
                name: "FK_ServerTags_Servers_ServerId",
                table: "ServerTags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Servers",
                table: "Servers");

            migrationBuilder.DropIndex(
                name: "IX_Servers_Id",
                table: "Servers");

            migrationBuilder.DropColumn(
                name: "RowId",
                table: "Servers");

            migrationBuilder.DropIndex(name: "IX_ServerTags_ServerId", table: "ServerTags");
            migrationBuilder.DropIndex(name: "IX_NetworkInterfaces_ServerId", table: "NetworkInterfaces");
            migrationBuilder.DropIndex(name: "IX_InstalledServices_ServerId", table: "InstalledServices");
            migrationBuilder.DropIndex(name: "IX_Disks_ServerId", table: "Disks");

            migrationBuilder.DropColumn(name: "ServerId", table: "ServerTags");
            migrationBuilder.DropColumn(name: "ServerId", table: "NetworkInterfaces");
            migrationBuilder.DropColumn(name: "ServerId", table: "InstalledServices");
            migrationBuilder.DropColumn(name: "ServerId", table: "Disks");

            migrationBuilder.AddColumn<Guid>(
                name: "ServerId",
                table: "ServerTags",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ServerId",
                table: "NetworkInterfaces",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ServerId",
                table: "InstalledServices",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ServerId",
                table: "Disks",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Servers",
                table: "Servers",
                column: "Id");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Disks_Servers_ServerId",
                table: "Disks",
                column: "ServerId",
                principalTable: "Servers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InstalledServices_Servers_ServerId",
                table: "InstalledServices",
                column: "ServerId",
                principalTable: "Servers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NetworkInterfaces_Servers_ServerId",
                table: "NetworkInterfaces",
                column: "ServerId",
                principalTable: "Servers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ServerTags_Servers_ServerId",
                table: "ServerTags",
                column: "ServerId",
                principalTable: "Servers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
