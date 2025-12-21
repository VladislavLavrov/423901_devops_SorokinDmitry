using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SortRollWebApp.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Factories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Factories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Steels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    SteelCode = table.Column<double>(type: "REAL", nullable: false),
                    K1 = table.Column<double>(type: "REAL", nullable: false),
                    K2 = table.Column<double>(type: "REAL", nullable: false),
                    K3 = table.Column<double>(type: "REAL", nullable: false),
                    K4 = table.Column<double>(type: "REAL", nullable: false),
                    K5 = table.Column<double>(type: "REAL", nullable: false),
                    K6 = table.Column<double>(type: "REAL", nullable: false),
                    K7 = table.Column<double>(type: "REAL", nullable: false),
                    K8 = table.Column<double>(type: "REAL", nullable: false),
                    K9 = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Steels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RollingMills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    IdFactory = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RollingMills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RollingMills_Factories_IdFactory",
                        column: x => x.IdFactory,
                        principalTable: "Factories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RollingStands",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Number = table.Column<int>(type: "INTEGER", nullable: false),
                    DB = table.Column<double>(type: "REAL", nullable: false),
                    DSH = table.Column<double>(type: "REAL", nullable: false),
                    MU = table.Column<double>(type: "REAL", nullable: false),
                    FPOD = table.Column<double>(type: "REAL", nullable: false),
                    LKL = table.Column<double>(type: "REAL", nullable: false),
                    AKL = table.Column<double>(type: "REAL", nullable: false),
                    IR = table.Column<double>(type: "REAL", nullable: false),
                    ETA = table.Column<double>(type: "REAL", nullable: false),
                    NZ = table.Column<double>(type: "REAL", nullable: false),
                    NNOM = table.Column<double>(type: "REAL", nullable: false),
                    NDVN = table.Column<double>(type: "REAL", nullable: false),
                    NDVMIN = table.Column<double>(type: "REAL", nullable: false),
                    NDVMAX = table.Column<double>(type: "REAL", nullable: false),
                    PP = table.Column<double>(type: "REAL", nullable: false),
                    PDOP = table.Column<double>(type: "REAL", nullable: false),
                    MDOP = table.Column<double>(type: "REAL", nullable: false),
                    C = table.Column<double>(type: "REAL", nullable: false),
                    IdRollingMill = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RollingStands", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RollingStands_RollingMills_IdRollingMill",
                        column: x => x.IdRollingMill,
                        principalTable: "RollingMills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SteelSections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    IdRollingMill = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SteelSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SteelSections_RollingMills_IdRollingMill",
                        column: x => x.IdRollingMill,
                        principalTable: "RollingMills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InitialParameters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    W0 = table.Column<double>(type: "REAL", nullable: false),
                    P0 = table.Column<double>(type: "REAL", nullable: false),
                    L0 = table.Column<double>(type: "REAL", nullable: false),
                    T0 = table.Column<double>(type: "REAL", nullable: false),
                    TAU = table.Column<double>(type: "REAL", nullable: false),
                    TAU0 = table.Column<double>(type: "REAL", nullable: false),
                    LR = table.Column<double>(type: "REAL", nullable: false),
                    VK = table.Column<double>(type: "REAL", nullable: false),
                    T0min = table.Column<double>(type: "REAL", nullable: false),
                    T0max = table.Column<double>(type: "REAL", nullable: false),
                    IdSteelSection = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InitialParameters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InitialParameters_SteelSections_IdSteelSection",
                        column: x => x.IdSteelSection,
                        principalTable: "SteelSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Passes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    N = table.Column<int>(type: "INTEGER", nullable: false),
                    SchemaInput = table.Column<int>(type: "INTEGER", nullable: false),
                    SchemaCaliber = table.Column<int>(type: "INTEGER", nullable: false),
                    H0 = table.Column<double>(type: "REAL", nullable: false),
                    B0 = table.Column<double>(type: "REAL", nullable: false),
                    H1 = table.Column<double>(type: "REAL", nullable: false),
                    B1 = table.Column<double>(type: "REAL", nullable: false),
                    W = table.Column<double>(type: "REAL", nullable: false),
                    S = table.Column<double>(type: "REAL", nullable: false),
                    BVR = table.Column<double>(type: "REAL", nullable: false),
                    BD = table.Column<double>(type: "REAL", nullable: false),
                    R = table.Column<double>(type: "REAL", nullable: false),
                    ROV = table.Column<double>(type: "REAL", nullable: false),
                    R8 = table.Column<double>(type: "REAL", nullable: false),
                    SUMX = table.Column<double>(type: "REAL", nullable: false),
                    PSI = table.Column<double>(type: "REAL", nullable: false),
                    Z = table.Column<double>(type: "REAL", nullable: false),
                    SP = table.Column<double>(type: "REAL", nullable: false),
                    TOP = table.Column<double>(type: "REAL", nullable: false),
                    IdSteelSection = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Passes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Passes_SteelSections_IdSteelSection",
                        column: x => x.IdSteelSection,
                        principalTable: "SteelSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InitialParameters_IdSteelSection",
                table: "InitialParameters",
                column: "IdSteelSection");

            migrationBuilder.CreateIndex(
                name: "IX_Passes_IdSteelSection",
                table: "Passes",
                column: "IdSteelSection");

            migrationBuilder.CreateIndex(
                name: "IX_RollingMills_IdFactory",
                table: "RollingMills",
                column: "IdFactory");

            migrationBuilder.CreateIndex(
                name: "IX_RollingStands_IdRollingMill",
                table: "RollingStands",
                column: "IdRollingMill");

            migrationBuilder.CreateIndex(
                name: "IX_SteelSections_IdRollingMill",
                table: "SteelSections",
                column: "IdRollingMill");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InitialParameters");

            migrationBuilder.DropTable(
                name: "Passes");

            migrationBuilder.DropTable(
                name: "RollingStands");

            migrationBuilder.DropTable(
                name: "Steels");

            migrationBuilder.DropTable(
                name: "SteelSections");

            migrationBuilder.DropTable(
                name: "RollingMills");

            migrationBuilder.DropTable(
                name: "Factories");
        }
    }
}
