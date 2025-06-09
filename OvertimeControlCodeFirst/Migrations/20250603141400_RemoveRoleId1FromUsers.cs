using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OvertimeControlCodeFirst.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRoleId1FromUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Overtimes_Employees_EmployeeId1",
                table: "Overtimes");

            migrationBuilder.DropForeignKey(
                name: "FK_Overtimes_WorkActivities_WorkActivityId",
                table: "Overtimes");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Areas_AreaId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Overtimes_EmployeeId1",
                table: "Overtimes");

            migrationBuilder.DropColumn(
                name: "EmployeeId1",
                table: "Overtimes");

            migrationBuilder.RenameColumn(
                name: "dateStart",
                table: "Overtimes",
                newName: "DateStart");

            migrationBuilder.RenameColumn(
                name: "dateEnd",
                table: "Overtimes",
                newName: "DateEnd");

            migrationBuilder.AlterColumn<int>(
                name: "SecretariatId",
                table: "Users",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "AreaId",
                table: "Users",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "IpAddress",
                table: "LoginAudits",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddForeignKey(
                name: "FK_Overtimes_WorkActivities_WorkActivityId",
                table: "Overtimes",
                column: "WorkActivityId",
                principalTable: "WorkActivities",
                principalColumn: "WorkActivityId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Areas_AreaId",
                table: "Users",
                column: "AreaId",
                principalTable: "Areas",
                principalColumn: "AreaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Overtimes_WorkActivities_WorkActivityId",
                table: "Overtimes");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Areas_AreaId",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "DateStart",
                table: "Overtimes",
                newName: "dateStart");

            migrationBuilder.RenameColumn(
                name: "DateEnd",
                table: "Overtimes",
                newName: "dateEnd");

            migrationBuilder.AlterColumn<int>(
                name: "SecretariatId",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "AreaId",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmployeeId1",
                table: "Overtimes",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IpAddress",
                table: "LoginAudits",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Overtimes_EmployeeId1",
                table: "Overtimes",
                column: "EmployeeId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Overtimes_Employees_EmployeeId1",
                table: "Overtimes",
                column: "EmployeeId1",
                principalTable: "Employees",
                principalColumn: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Overtimes_WorkActivities_WorkActivityId",
                table: "Overtimes",
                column: "WorkActivityId",
                principalTable: "WorkActivities",
                principalColumn: "WorkActivityId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Areas_AreaId",
                table: "Users",
                column: "AreaId",
                principalTable: "Areas",
                principalColumn: "AreaId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
