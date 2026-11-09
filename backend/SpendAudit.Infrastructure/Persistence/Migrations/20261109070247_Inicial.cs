using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpendAudit.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Empresas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NombreRazonSocial = table.Column<string>(type: "text", nullable: false),
                    RTN = table.Column<string>(type: "text", nullable: false),
                    EstadoSuscripcion = table.Column<int>(type: "integer", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empresas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PoliticasGasto",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    DiasVigenciaMaxima = table.Column<int>(type: "integer", nullable: false),
                    Activa = table.Column<bool>(type: "boolean", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoliticasGasto", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Puestos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Puestos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TerminosRestringidos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    PoliticaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Termino = table.Column<string>(type: "text", nullable: false),
                    PoliticaGastoId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TerminosRestringidos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TerminosRestringidos_PoliticasGasto_PoliticaGastoId",
                        column: x => x.PoliticaGastoId,
                        principalTable: "PoliticasGasto",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TopesPuesto",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    PoliticaId = table.Column<Guid>(type: "uuid", nullable: false),
                    PuestoId = table.Column<Guid>(type: "uuid", nullable: false),
                    TopeDiarioHNL = table.Column<decimal>(type: "numeric", nullable: false),
                    PoliticaGastoId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TopesPuesto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TopesPuesto_PoliticasGasto_PoliticaGastoId",
                        column: x => x.PoliticaGastoId,
                        principalTable: "PoliticasGasto",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TopesPuesto_Puestos_PuestoId",
                        column: x => x.PuestoId,
                        principalTable: "Puestos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    PuestoId = table.Column<Guid>(type: "uuid", nullable: true),
                    NombreCompleto = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Rol = table.Column<int>(type: "integer", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Usuarios_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Usuarios_Puestos_PuestoId",
                        column: x => x.PuestoId,
                        principalTable: "Puestos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LotesLiquidacion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false),
                    FechaLiquidacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LotesLiquidacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LotesLiquidacion_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Comprobantes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoteId = table.Column<Guid>(type: "uuid", nullable: true),
                    TipoArchivo = table.Column<int>(type: "integer", nullable: false),
                    NombreArchivoOriginal = table.Column<string>(type: "text", nullable: false),
                    UrlArchivo = table.Column<string>(type: "text", nullable: false),
                    NumeroFactura = table.Column<string>(type: "text", nullable: true),
                    CAI = table.Column<string>(type: "text", nullable: true),
                    ProveedorRTN = table.Column<string>(type: "text", nullable: true),
                    FechaEmision = table.Column<DateOnly>(type: "date", nullable: true),
                    MontoTotal = table.Column<decimal>(type: "numeric", nullable: true),
                    ISV = table.Column<decimal>(type: "numeric", nullable: true),
                    TextoExtraido = table.Column<string>(type: "text", nullable: true),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    ResultadoAuditoria = table.Column<int>(type: "integer", nullable: true),
                    FechaCarga = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comprobantes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comprobantes_LotesLiquidacion_LoteId",
                        column: x => x.LoteId,
                        principalTable: "LotesLiquidacion",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Comprobantes_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DetalleItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    ComprobanteId = table.Column<Guid>(type: "uuid", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false),
                    Subtotal = table.Column<decimal>(type: "numeric", nullable: false),
                    EsRestringido = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetalleItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetalleItems_Comprobantes_ComprobanteId",
                        column: x => x.ComprobanteId,
                        principalTable: "Comprobantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HallazgosAuditoria",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    ComprobanteId = table.Column<Guid>(type: "uuid", nullable: false),
                    Regla = table.Column<int>(type: "integer", nullable: false),
                    Severidad = table.Column<int>(type: "integer", nullable: false),
                    Detalle = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HallazgosAuditoria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HallazgosAuditoria_Comprobantes_ComprobanteId",
                        column: x => x.ComprobanteId,
                        principalTable: "Comprobantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comprobantes_EmpresaId",
                table: "Comprobantes",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Comprobantes_LoteId",
                table: "Comprobantes",
                column: "LoteId");

            migrationBuilder.CreateIndex(
                name: "IX_Comprobantes_UsuarioId",
                table: "Comprobantes",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleItems_ComprobanteId",
                table: "DetalleItems",
                column: "ComprobanteId");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleItems_EmpresaId",
                table: "DetalleItems",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_HallazgosAuditoria_ComprobanteId",
                table: "HallazgosAuditoria",
                column: "ComprobanteId");

            migrationBuilder.CreateIndex(
                name: "IX_HallazgosAuditoria_EmpresaId",
                table: "HallazgosAuditoria",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_LotesLiquidacion_EmpresaId",
                table: "LotesLiquidacion",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_LotesLiquidacion_UsuarioId",
                table: "LotesLiquidacion",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticasGasto_EmpresaId",
                table: "PoliticasGasto",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Puestos_EmpresaId",
                table: "Puestos",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_TerminosRestringidos_EmpresaId",
                table: "TerminosRestringidos",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_TerminosRestringidos_PoliticaGastoId",
                table: "TerminosRestringidos",
                column: "PoliticaGastoId");

            migrationBuilder.CreateIndex(
                name: "IX_TopesPuesto_EmpresaId",
                table: "TopesPuesto",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_TopesPuesto_PoliticaGastoId",
                table: "TopesPuesto",
                column: "PoliticaGastoId");

            migrationBuilder.CreateIndex(
                name: "IX_TopesPuesto_PuestoId",
                table: "TopesPuesto",
                column: "PuestoId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_EmpresaId",
                table: "Usuarios",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_PuestoId",
                table: "Usuarios",
                column: "PuestoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetalleItems");

            migrationBuilder.DropTable(
                name: "HallazgosAuditoria");

            migrationBuilder.DropTable(
                name: "TerminosRestringidos");

            migrationBuilder.DropTable(
                name: "TopesPuesto");

            migrationBuilder.DropTable(
                name: "Comprobantes");

            migrationBuilder.DropTable(
                name: "PoliticasGasto");

            migrationBuilder.DropTable(
                name: "LotesLiquidacion");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Empresas");

            migrationBuilder.DropTable(
                name: "Puestos");
        }
    }
}
