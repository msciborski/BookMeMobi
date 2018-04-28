using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace BookMeMobi2.Migrations
{
    public partial class AlterBookTableDeleteStoragrPathColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cover",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "StoragePath",
                table: "Books");

            migrationBuilder.CreateIndex(
                name: "IX_Books_CoverId",
                table: "Books",
                column: "CoverId");

            migrationBuilder.AddForeignKey(
                name: "FK_Books_Covers_CoverId",
                table: "Books",
                column: "CoverId",
                principalTable: "Covers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Books_Covers_CoverId",
                table: "Books");

            migrationBuilder.DropIndex(
                name: "IX_Books_CoverId",
                table: "Books");

            migrationBuilder.AddColumn<byte[]>(
                name: "Cover",
                table: "Books",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StoragePath",
                table: "Books",
                nullable: true);
        }
    }
}
