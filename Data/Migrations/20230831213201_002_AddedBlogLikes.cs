using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Blog.Data.Migrations
{
    /// <inheritdoc />
    public partial class _002_AddedBlogLikes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlogLikes",
                schema: "blog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BlogPostId = table.Column<int>(type: "integer", nullable: false),
                    IsLiked = table.Column<bool>(type: "boolean", nullable: false),
                    BlogUserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogLikes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlogLikes_AspNetUsers_BlogUserId",
                        column: x => x.BlogUserId,
                        principalSchema: "blog",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BlogLikes_BlogPosts_BlogPostId",
                        column: x => x.BlogPostId,
                        principalSchema: "blog",
                        principalTable: "BlogPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlogLikes_BlogPostId",
                schema: "blog",
                table: "BlogLikes",
                column: "BlogPostId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogLikes_BlogUserId",
                schema: "blog",
                table: "BlogLikes",
                column: "BlogUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlogLikes",
                schema: "blog");
        }
    }
}
