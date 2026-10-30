using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModulosComerciales.Migrations
{
    /// <inheritdoc />
    public partial class AgregarFechaFinalizacionSolicitud : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaFinalizacion",
                table: "SolicitudesAutorizacion",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaFinalizacion",
                table: "SolicitudesAutorizacion");
        }
    }
}
