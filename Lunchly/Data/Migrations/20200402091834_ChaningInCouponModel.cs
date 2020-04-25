using Microsoft.EntityFrameworkCore.Migrations;

namespace Lunchly.Data.Migrations
{
    public partial class ChaningInCouponModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "isActive",
                table: "Coupons",
                newName: "IsActive");

            migrationBuilder.AddColumn<string>(
                name: "CouponType",
                table: "Coupons",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CouponType",
                table: "Coupons");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Coupons",
                newName: "isActive");
        }
    }
}
