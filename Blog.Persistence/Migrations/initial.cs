#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Blog.Persistence.Migrations;

public partial class initial : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            "Authors",
            table => new
            {
                AuthorId = table.Column<long>("bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy",
                        NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Username = table.Column<string>("text", nullable: false),
                EmailAddress = table.Column<string>("text", nullable: false),
                Description = table.Column<string>("text", nullable: false),
                PasswordHash = table.Column<string>("text", nullable: true),
                Roles = table.Column<int[]>("integer[]", nullable: false)
            },
            constraints: table => { table.PrimaryKey("PK_Authors", x => x.AuthorId); });

        migrationBuilder.CreateTable(
            "Categories",
            table => new
            {
                CategoryName = table.Column<string>("text", nullable: false)
            },
            constraints: table => { table.PrimaryKey("PK_Categories", x => x.CategoryName); });

        migrationBuilder.CreateTable(
            "BlogPosts",
            table => new
            {
                PostId = table.Column<long>("bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy",
                        NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Title = table.Column<string>("text", nullable: false),
                Summary = table.Column<string>("text", nullable: false),
                Body = table.Column<string>("text", nullable: false),
                Tags = table.Column<string[]>("text[]", nullable: false),
                CategoryName = table.Column<string>("text", nullable: false),
                AuthorId = table.Column<long>("bigint", nullable: false),
                Created = table.Column<DateTime>("timestamp with time zone", nullable: false),
                Updated = table.Column<DateTime>("timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BlogPosts", x => x.PostId);
                table.ForeignKey(
                    "FK_BlogPosts_Authors_AuthorId",
                    x => x.AuthorId,
                    "Authors",
                    "AuthorId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    "FK_BlogPosts_Categories_CategoryName",
                    x => x.CategoryName,
                    "Categories",
                    "CategoryName",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            "IX_Authors_EmailAddress",
            "Authors",
            "EmailAddress",
            unique: true);

        migrationBuilder.CreateIndex(
            "IX_BlogPosts_AuthorId",
            "BlogPosts",
            "AuthorId");

        migrationBuilder.CreateIndex(
            "IX_BlogPosts_CategoryName",
            "BlogPosts",
            "CategoryName");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            "BlogPosts");

        migrationBuilder.DropTable(
            "Authors");

        migrationBuilder.DropTable(
            "Categories");
    }
}