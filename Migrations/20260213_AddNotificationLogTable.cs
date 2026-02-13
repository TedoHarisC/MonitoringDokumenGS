using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonitoringDokumenGS.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationLogTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SYS_NotificationLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NotificationType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ReferenceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RecipientEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ReminderLevel = table.Column<int>(type: "int", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SYS_NotificationLog", x => x.Id);
                });

            // Create index for better query performance
            migrationBuilder.CreateIndex(
                name: "IX_SYS_NotificationLog_Type_Ref_SentAt",
                table: "SYS_NotificationLog",
                columns: new[] { "NotificationType", "ReferenceId", "SentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SYS_NotificationLog_ReferenceId",
                table: "SYS_NotificationLog",
                column: "ReferenceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SYS_NotificationLog");
        }
    }
}
