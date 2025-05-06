using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OvertimeControlCodeFirst.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkActivityToOvertime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Workplaces",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "WorkActivityId",
                table: "Overtimes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Overtimes_WorkActivityId",
                table: "Overtimes",
                column: "WorkActivityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Overtimes_WorkActivities_WorkActivityId",
                table: "Overtimes",
                column: "WorkActivityId",
                principalTable: "WorkActivities",
                principalColumn: "WorkActivityId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Overtimes_WorkActivities_WorkActivityId",
                table: "Overtimes");

            migrationBuilder.DropIndex(
                name: "IX_Overtimes_WorkActivityId",
                table: "Overtimes");

            migrationBuilder.DropColumn(
                name: "WorkActivityId",
                table: "Overtimes");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Workplaces",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);
        }
    }
}
