﻿using Microsoft.EntityFrameworkCore.Migrations;

namespace CurrencyMonitor.Migrations
{
    public partial class initialcreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SubscriptionForExchangeRate",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMailAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodeCurrencyToSell = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodeCurrencyToBuy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionForExchangeRate", x => x.ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubscriptionForExchangeRate");
        }
    }
}
