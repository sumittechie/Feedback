using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

namespace Data.Migrations
{
    public partial class FeedbackTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Feedback",
                columns: table => new
                {
                    FeedbackId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Question = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedback", x => x.FeedbackId);
                });

            migrationBuilder.CreateTable(
                name: "FeedbackAssigned",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    FeedbackId = table.Column<int>(type: "int", nullable: false),
                    UsersId = table.Column<string>(type: "varchar(767)", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackAssigned", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeedbackAssigned_AspNetUsers_UsersId",
                        column: x => x.UsersId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FeedbackAssigned_Feedback_FeedbackId",
                        column: x => x.FeedbackId,
                        principalTable: "Feedback",
                        principalColumn: "FeedbackId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FeedbackReplys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    FeedbackAssignedId = table.Column<int>(type: "int", nullable: false),
                    Answer = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackReplys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeedbackReplys_FeedbackAssigned_FeedbackAssignedId",
                        column: x => x.FeedbackAssignedId,
                        principalTable: "FeedbackAssigned",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackAssigned_FeedbackId",
                table: "FeedbackAssigned",
                column: "FeedbackId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackAssigned_UsersId",
                table: "FeedbackAssigned",
                column: "UsersId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackReplys_FeedbackAssignedId",
                table: "FeedbackReplys",
                column: "FeedbackAssignedId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeedbackReplys");

            migrationBuilder.DropTable(
                name: "FeedbackAssigned");

            migrationBuilder.DropTable(
                name: "Feedback");
        }
    }
}
