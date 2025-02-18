using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DailyLeetcodeReminder.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixTimestampColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Timestamp",
                table: "Challengers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Timestamp",
                table: "Challengers",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
