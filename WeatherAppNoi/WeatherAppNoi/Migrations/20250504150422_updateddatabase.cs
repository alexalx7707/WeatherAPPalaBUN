using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeatherAppNoi.Migrations
{
    /// <inheritdoc />
    public partial class updateddatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UvIndex",
                table: "WeatherData");

            migrationBuilder.DropColumn(
                name: "Visibility",
                table: "WeatherData");

            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "WeatherData",
                newName: "TimeStamp");

            migrationBuilder.RenameColumn(
                name: "Condition",
                table: "WeatherData",
                newName: "WeatherIcon");

            migrationBuilder.AlterColumn<double>(
                name: "WindSpeed",
                table: "WeatherData",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<DateTime>(
                name: "TimeStamp",
                table: "WeatherData",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<double>(
                name: "Temperature",
                table: "WeatherData",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<double>(
                name: "FeelsLike",
                table: "WeatherData",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<string>(
                name: "CityName",
                table: "WeatherData",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "WeatherData",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WeatherDescription",
                table: "WeatherData",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CityName",
                table: "WeatherData");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "WeatherData");

            migrationBuilder.DropColumn(
                name: "WeatherDescription",
                table: "WeatherData");

            migrationBuilder.RenameColumn(
                name: "TimeStamp",
                table: "WeatherData",
                newName: "Timestamp");

            migrationBuilder.RenameColumn(
                name: "WeatherIcon",
                table: "WeatherData",
                newName: "Condition");

            migrationBuilder.AlterColumn<int>(
                name: "WindSpeed",
                table: "WeatherData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "REAL");

            migrationBuilder.AlterColumn<int>(
                name: "Timestamp",
                table: "WeatherData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<int>(
                name: "Temperature",
                table: "WeatherData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "REAL");

            migrationBuilder.AlterColumn<int>(
                name: "FeelsLike",
                table: "WeatherData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "REAL");

            migrationBuilder.AddColumn<int>(
                name: "UvIndex",
                table: "WeatherData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Visibility",
                table: "WeatherData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
