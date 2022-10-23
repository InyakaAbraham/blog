#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Blog.Persistence.Migrations;

public partial class Initial : Migration
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
                FirstName = table.Column<string>("text", nullable: false),
                LastName = table.Column<string>("text", nullable: true),
                Description = table.Column<string>("text", nullable: false),
                EmailAddress = table.Column<string>("text", nullable: false),
                PasswordHash = table.Column<string>("text", nullable: true),
                VerifiedAt = table.Column<DateTime>("timestamp with time zone", nullable: false),
                LastLogin = table.Column<DateTime>("timestamp with time zone", nullable: false),
                CreatedAt = table.Column<DateTime>("timestamp with time zone", nullable: false)
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
            "Roles",
            table => new
            {
                Id = table.Column<int>("integer", nullable: false)
            },
            constraints: table => { table.PrimaryKey("PK_Roles", x => x.Id); });

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

        migrationBuilder.CreateTable(
            "AuthorRole",
            table => new
            {
                AuthorsAuthorId = table.Column<long>("bigint", nullable: false),
                RolesId = table.Column<int>("integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AuthorRole", x => new { x.AuthorsAuthorId, x.RolesId });
                table.ForeignKey(
                    "FK_AuthorRole_Authors_AuthorsAuthorId",
                    x => x.AuthorsAuthorId,
                    "Authors",
                    "AuthorId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    "FK_AuthorRole_Roles_RolesId",
                    x => x.RolesId,
                    "Roles",
                    "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.InsertData(
            "Roles",
            "Id",
            new object[]
            {
                1,
                2,
                3
            });

        migrationBuilder.CreateIndex(
            "IX_AuthorRole_RolesId",
            "AuthorRole",
            "RolesId");

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
            "AuthorRole");

        migrationBuilder.DropTable(
            "BlogPosts");

        migrationBuilder.DropTable(
            "Roles");

        migrationBuilder.DropTable(
            "Authors");

        migrationBuilder.DropTable(
            "Categories");
    }
}