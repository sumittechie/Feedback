using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class CorrectedDefaultValue : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdated",
                table: "FeedbackReplys",
                type: "datetime",
                nullable: false,
                defaultValueSql: "NOW()",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "now()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdated",
                table: "FeedbackAssigned",
                type: "datetime",
                nullable: false,
                defaultValueSql: "NOW()",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "now()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdated",
                table: "Feedback",
                type: "datetime",
                nullable: false,
                defaultValueSql: "NOW()",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "now()");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdated",
                table: "FeedbackReplys",
                type: "datetime",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "NOW()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdated",
                table: "FeedbackAssigned",
                type: "datetime",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "NOW()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdated",
                table: "Feedback",
                type: "datetime",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "NOW()");
        }
    }
}
