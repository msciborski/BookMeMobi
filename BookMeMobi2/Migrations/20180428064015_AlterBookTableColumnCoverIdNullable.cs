using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace BookMeMobi2.Migrations
{
    public partial class AlterBookTableColumnCoverIdNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Books_Covers_CoverId",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "Image",
                table: "Covers");

            migrationBuilder.AddColumn<string>(
                name: "CoverName",
                table: "Covers",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CoverId",
                table: "Books",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddForeignKey(
                name: "FK_Books_Covers_CoverId",
                table: "Books",
                column: "CoverId",
                principalTable: "Covers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Books_Covers_CoverId",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "CoverName",
                table: "Covers");

            migrationBuilder.AddColumn<byte[]>(
                name: "Image",
                table: "Covers",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CoverId",
                table: "Books",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Books_Covers_CoverId",
                table: "Books",
                column: "CoverId",
                principalTable: "Covers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
