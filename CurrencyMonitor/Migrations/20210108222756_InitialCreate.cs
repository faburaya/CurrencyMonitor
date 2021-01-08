using Microsoft.EntityFrameworkCore.Migrations;

namespace CurrencyMonitor.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RecognizedCurrency",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecognizedCurrency", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionForExchangeRate",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EMailAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CodeCurrencyToSell = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CodeCurrencyToBuy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TargetPriceOfSellingCurrency = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionForExchangeRate", x => x.ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecognizedCurrency");

            migrationBuilder.DropTable(
                name: "SubscriptionForExchangeRate");
        }
    }
}
