
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoneyCare.Migrations
{
    public partial class AddTietKiem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TietKiem",
                columns: table => new
                {
                    MaTietKiem = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    UserId = table.Column<int>(type: "int", nullable: false),

                    TenMucTieu = table.Column<string>(
                        type: "nvarchar(150)",
                        maxLength: 150,
                        nullable: false),

                    SoTienMucTieu = table.Column<decimal>(
                        type: "decimal(18,2)",
                        nullable: false),

                    SoTienHienTai = table.Column<decimal>(
                        type: "decimal(18,2)",
                        nullable: false),

                    HanHoanThanh = table.Column<DateTime>(
                        type: "date",
                        nullable: false),

                    NgayTao = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_TietKiem",
                        x => x.MaTietKiem);

                    table.ForeignKey(
                        name: "FK_TietKiem_NguoiDung_UserId",
                        column: x => x.UserId,
                        principalTable: "NguoiDung",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TietKiem_UserId",
                table: "TietKiem",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TietKiem");
        }
    }
}
