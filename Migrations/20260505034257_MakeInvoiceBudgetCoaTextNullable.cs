using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonitoringDokumenGS.Migrations
{
    /// <inheritdoc />
    public partial class MakeInvoiceBudgetCoaTextNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NoCoa",
                table: "VendorCategories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ParentBudgetCodeId",
                table: "VendorCategories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Activity",
                table: "MST_Budget",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "BudgetCodeId",
                table: "MST_Budget",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoCoa",
                table: "MST_Budget",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TypeBudget",
                table: "MST_Budget",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "BudgetCodeId",
                table: "Invoices",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BudgetId",
                table: "Invoices",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "CoaTextId",
                table: "Invoices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CoaVendorCategoryId",
                table: "Invoices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "GrandTotal",
                table: "Invoices",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceDescription",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoSAP",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AdvancedStatuses",
                columns: table => new
                {
                    AdvancedStatusesId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvancedStatuses", x => x.AdvancedStatusesId);
                });

            migrationBuilder.CreateTable(
                name: "BiayaRealisasiStatuses",
                columns: table => new
                {
                    BiayaRealisasiStatusesId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BiayaRealisasiStatuses", x => x.BiayaRealisasiStatusesId);
                });

            migrationBuilder.CreateTable(
                name: "BudgetCode",
                columns: table => new
                {
                    BudgetCodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetCode", x => x.BudgetCodeId);
                });

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

            migrationBuilder.CreateTable(
                name: "TemplateFile",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Permission = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileSize = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateFile", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "VendorPics",
                columns: table => new
                {
                    VendorPicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VendorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PicName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PicNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PicEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorPics", x => x.VendorPicId);
                    table.ForeignKey(
                        name: "FK_VendorPics_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "VendorId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UangMukas",
                columns: table => new
                {
                    UangMukaId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Jenis = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BudgetCodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CoaTextId = table.Column<int>(type: "int", nullable: true),
                    UangMukaRelatedId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NoSAP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AtasNama = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Deskripsi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StatusId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UangMukas", x => x.UangMukaId);
                    table.ForeignKey(
                        name: "FK_UangMukas_BudgetCode_BudgetCodeId",
                        column: x => x.BudgetCodeId,
                        principalTable: "BudgetCode",
                        principalColumn: "BudgetCodeId");
                    table.ForeignKey(
                        name: "FK_UangMukas_UangMukas_UangMukaRelatedId",
                        column: x => x.UangMukaRelatedId,
                        principalTable: "UangMukas",
                        principalColumn: "UangMukaId");
                    table.ForeignKey(
                        name: "FK_UangMukas_VendorCategories_CoaTextId",
                        column: x => x.CoaTextId,
                        principalTable: "VendorCategories",
                        principalColumn: "VendorCategoryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UangMukaBudgetCode",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UangMukaId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BudgetCodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UangMukaBudgetCode", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UangMukaBudgetCode_BudgetCode_BudgetCodeId",
                        column: x => x.BudgetCodeId,
                        principalTable: "BudgetCode",
                        principalColumn: "BudgetCodeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UangMukaBudgetCode_UangMukas_UangMukaId",
                        column: x => x.UangMukaId,
                        principalTable: "UangMukas",
                        principalColumn: "UangMukaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UangMukaCoaText",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UangMukaId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CoaTextId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UangMukaCoaText", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UangMukaCoaText_UangMukas_UangMukaId",
                        column: x => x.UangMukaId,
                        principalTable: "UangMukas",
                        principalColumn: "UangMukaId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UangMukaCoaText_VendorCategories_CoaTextId",
                        column: x => x.CoaTextId,
                        principalTable: "VendorCategories",
                        principalColumn: "VendorCategoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MST_Budget_BudgetCodeId",
                table: "MST_Budget",
                column: "BudgetCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_BudgetId",
                table: "Invoices",
                column: "BudgetId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CoaVendorCategoryId",
                table: "Invoices",
                column: "CoaVendorCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_UangMukaBudgetCode_BudgetCodeId",
                table: "UangMukaBudgetCode",
                column: "BudgetCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_UangMukaBudgetCode_UangMukaId",
                table: "UangMukaBudgetCode",
                column: "UangMukaId");

            migrationBuilder.CreateIndex(
                name: "IX_UangMukaCoaText_CoaTextId",
                table: "UangMukaCoaText",
                column: "CoaTextId");

            migrationBuilder.CreateIndex(
                name: "IX_UangMukaCoaText_UangMukaId",
                table: "UangMukaCoaText",
                column: "UangMukaId");

            migrationBuilder.CreateIndex(
                name: "IX_UangMukas_BudgetCodeId",
                table: "UangMukas",
                column: "BudgetCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_UangMukas_CoaTextId",
                table: "UangMukas",
                column: "CoaTextId");

            migrationBuilder.CreateIndex(
                name: "IX_UangMukas_UangMukaRelatedId",
                table: "UangMukas",
                column: "UangMukaRelatedId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorPics_VendorId",
                table: "VendorPics",
                column: "VendorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_MST_Budget_BudgetId",
                table: "Invoices",
                column: "BudgetId",
                principalTable: "MST_Budget",
                principalColumn: "BudgetId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_VendorCategories_CoaVendorCategoryId",
                table: "Invoices",
                column: "CoaVendorCategoryId",
                principalTable: "VendorCategories",
                principalColumn: "VendorCategoryId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MST_Budget_BudgetCode_BudgetCodeId",
                table: "MST_Budget",
                column: "BudgetCodeId",
                principalTable: "BudgetCode",
                principalColumn: "BudgetCodeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_MST_Budget_BudgetId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_VendorCategories_CoaVendorCategoryId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_MST_Budget_BudgetCode_BudgetCodeId",
                table: "MST_Budget");

            migrationBuilder.DropTable(
                name: "AdvancedStatuses");

            migrationBuilder.DropTable(
                name: "BiayaRealisasiStatuses");

            migrationBuilder.DropTable(
                name: "SYS_NotificationLog");

            migrationBuilder.DropTable(
                name: "TemplateFile");

            migrationBuilder.DropTable(
                name: "UangMukaBudgetCode");

            migrationBuilder.DropTable(
                name: "UangMukaCoaText");

            migrationBuilder.DropTable(
                name: "VendorPics");

            migrationBuilder.DropTable(
                name: "UangMukas");

            migrationBuilder.DropTable(
                name: "BudgetCode");

            migrationBuilder.DropIndex(
                name: "IX_MST_Budget_BudgetCodeId",
                table: "MST_Budget");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_BudgetId",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_CoaVendorCategoryId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "NoCoa",
                table: "VendorCategories");

            migrationBuilder.DropColumn(
                name: "ParentBudgetCodeId",
                table: "VendorCategories");

            migrationBuilder.DropColumn(
                name: "Activity",
                table: "MST_Budget");

            migrationBuilder.DropColumn(
                name: "BudgetCodeId",
                table: "MST_Budget");

            migrationBuilder.DropColumn(
                name: "NoCoa",
                table: "MST_Budget");

            migrationBuilder.DropColumn(
                name: "TypeBudget",
                table: "MST_Budget");

            migrationBuilder.DropColumn(
                name: "BudgetCodeId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "BudgetId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "CoaTextId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "CoaVendorCategoryId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "GrandTotal",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "InvoiceDescription",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "NoSAP",
                table: "Invoices");
        }
    }
}
