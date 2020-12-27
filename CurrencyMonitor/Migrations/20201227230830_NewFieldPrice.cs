using Microsoft.EntityFrameworkCore.Migrations;

namespace CurrencyMonitor.Migrations
{
    public partial class NewFieldPrice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TargetPriceOfSellingCurrency",
                table: "SubscriptionForExchangeRate",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TargetPriceOfSellingCurrency",
                table: "SubscriptionForExchangeRate");
        }
    }
}
