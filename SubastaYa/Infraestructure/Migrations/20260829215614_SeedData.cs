using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Categorias",
                columns: new[] { "Id", "Nombre", "UrlIcono" },
                values: new object[,]
                {
                    { 1, "Tecnología", "tech.png" },
                    { 2, "Coleccionables", "col.png" },
                    { 3, "Indumentaria", "ropa.png" },
                    { 4, "Vehículos", "auto.png" }
                });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "Email", "FechaRegistro", "Nombre", "PasswordHash" },
                values: new object[,]
                {
                    { 1, "vendedor@test.com", new DateTime(2026, 8, 19, 12, 0, 0, 0, DateTimeKind.Unspecified), "Vendedor", "hash123" },
                    { 2, "comprador1@test.com", new DateTime(2026, 8, 24, 12, 0, 0, 0, DateTimeKind.Unspecified), "Comprador 1", "hash123" },
                    { 3, "comprador2@test.com", new DateTime(2026, 8, 27, 12, 0, 0, 0, DateTimeKind.Unspecified), "Comprador 2", "hash123" },
                    { 4, "sinfondos@test.com", new DateTime(2026, 8, 28, 12, 0, 0, 0, DateTimeKind.Unspecified), "Sin Fondos", "hash123" }
                });

            migrationBuilder.InsertData(
                table: "Billeteras",
                columns: new[] { "Id", "SaldoDisponible", "SaldoRetenido", "SaldoTotal", "UsuarioId", "Version" },
                values: new object[,]
                {
                    { 1, 0m, 0m, 0m, 1, 1 },
                    { 2, 105000m, 45000m, 150000m, 2, 1 },
                    { 3, 200000m, 0m, 200000m, 3, 1 },
                    { 4, 500m, 0m, 500m, 4, 1 }
                });

            migrationBuilder.InsertData(
                table: "Subastas",
                columns: new[] { "Id", "CategoriaId", "Descripcion", "Estado", "FechaFin", "FechaInicio", "IncrementoMinimo", "PrecioBase", "Titulo", "UrlImagen", "VendedorId", "Version" },
                values: new object[,]
                {
                    { 1, 1, "Activa estándar", "ACTIVA", new DateTime(2026, 8, 29, 12, 30, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 8, 29, 11, 0, 0, 0, DateTimeKind.Unspecified), 1000m, 30000m, "Notebook Pro", "img1.png", 1, 1 },
                    { 2, 2, "Activa crítica", "ACTIVA", new DateTime(2026, 8, 29, 12, 1, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 8, 29, 10, 0, 0, 0, DateTimeKind.Unspecified), 500m, 10000m, "Reloj Antiguo", "img2.png", 1, 1 },
                    { 3, 4, "Inicia mañana", "PROGRAMADA", new DateTime(2026, 8, 31, 12, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 8, 30, 12, 0, 0, 0, DateTimeKind.Unspecified), 50000m, 1500000m, "Auto Usado", "img3.png", 1, 1 },
                    { 4, 1, "Para liquidar", "ACTIVA", new DateTime(2026, 8, 28, 12, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 8, 26, 12, 0, 0, 0, DateTimeKind.Unspecified), 1000m, 20000m, "Monitor 24", "img4.png", 1, 1 },
                    { 5, 3, "Nadie ofertó", "ACTIVA", new DateTime(2026, 8, 27, 12, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 8, 24, 12, 0, 0, 0, DateTimeKind.Unspecified), 2000m, 50000m, "Campera Cuero", "img5.png", 1, 1 }
                });

            migrationBuilder.InsertData(
                table: "Pujas",
                columns: new[] { "Id", "CompradorId", "FechaPuja", "Monto", "SubastaId" },
                values: new object[,]
                {
                    { 1, 2, new DateTime(2026, 8, 29, 11, 20, 0, 0, DateTimeKind.Unspecified), 35000m, 1 },
                    { 2, 2, new DateTime(2026, 8, 29, 11, 40, 0, 0, DateTimeKind.Unspecified), 45000m, 1 },
                    { 3, 3, new DateTime(2026, 8, 27, 12, 0, 0, 0, DateTimeKind.Unspecified), 25000m, 4 }
                });

            migrationBuilder.InsertData(
                table: "TransaccionesLedger",
                columns: new[] { "Id", "BilleteraId", "Fecha", "Monto", "SubastaId", "Tipo" },
                values: new object[,]
                {
                    { 1, 2, new DateTime(2026, 8, 25, 12, 0, 0, 0, DateTimeKind.Unspecified), 150000m, null, "DEPOSITO" },
                    { 2, 2, new DateTime(2026, 8, 29, 11, 40, 0, 0, DateTimeKind.Unspecified), 45000m, 1, "RETENCION" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Billeteras",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Billeteras",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Billeteras",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Pujas",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Pujas",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Pujas",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Subastas",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Subastas",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Subastas",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "TransaccionesLedger",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "TransaccionesLedger",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Billeteras",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Subastas",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Subastas",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
