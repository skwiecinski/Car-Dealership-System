using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalonSamochodowy.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppRoles",
                columns: table => new
                {
                    RoleID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PermissionLevel = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppRoles", x => x.RoleID);
                });

            migrationBuilder.CreateTable(
                name: "Dealerships",
                columns: table => new
                {
                    DealershipID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Owner = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dealerships", x => x.DealershipID);
                });

            migrationBuilder.CreateTable(
                name: "Engines",
                columns: table => new
                {
                    EngineID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EngineName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    EngineSize = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Power = table.Column<int>(type: "INTEGER", nullable: false),
                    Price = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Engines", x => x.EngineID);
                });

            migrationBuilder.CreateTable(
                name: "Features",
                columns: table => new
                {
                    FeatureID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FeatureName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Features", x => x.FeatureID);
                });

            migrationBuilder.CreateTable(
                name: "VehicleModels",
                columns: table => new
                {
                    ModelID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Brand = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ModelName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleModels", x => x.ModelID);
                });

            migrationBuilder.CreateTable(
                name: "AppUsers",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleID = table.Column<int>(type: "INTEGER", nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    BirthDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUsers", x => x.UserID);
                    table.ForeignKey(
                        name: "FK_AppUsers_AppRoles_RoleID",
                        column: x => x.RoleID,
                        principalTable: "AppRoles",
                        principalColumn: "RoleID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrimLevels",
                columns: table => new
                {
                    TrimID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ModelID = table.Column<int>(type: "INTEGER", nullable: false),
                    TrimName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    BasePrice = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrimLevels", x => x.TrimID);
                    table.ForeignKey(
                        name: "FK_TrimLevels_VehicleModels_ModelID",
                        column: x => x.ModelID,
                        principalTable: "VehicleModels",
                        principalColumn: "ModelID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    ClientID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserID = table.Column<int>(type: "INTEGER", nullable: false),
                    NIP = table.Column<string>(type: "TEXT", maxLength: 15, nullable: true),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.ClientID);
                    table.ForeignKey(
                        name: "FK_Clients_AppUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AppUsers",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Workers",
                columns: table => new
                {
                    WorkerID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserID = table.Column<int>(type: "INTEGER", nullable: false),
                    Payroll = table.Column<decimal>(type: "TEXT", nullable: false),
                    EndOfContractDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DealershipID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workers", x => x.WorkerID);
                    table.ForeignKey(
                        name: "FK_Workers_AppUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AppUsers",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Workers_Dealerships_DealershipID",
                        column: x => x.DealershipID,
                        principalTable: "Dealerships",
                        principalColumn: "DealershipID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrimFeatures",
                columns: table => new
                {
                    TrimID = table.Column<int>(type: "INTEGER", nullable: false),
                    FeatureID = table.Column<int>(type: "INTEGER", nullable: false),
                    IsStandard = table.Column<bool>(type: "INTEGER", nullable: false),
                    AdditionalPrice = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrimFeatures", x => new { x.TrimID, x.FeatureID });
                    table.ForeignKey(
                        name: "FK_TrimFeatures_Features_FeatureID",
                        column: x => x.FeatureID,
                        principalTable: "Features",
                        principalColumn: "FeatureID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrimFeatures_TrimLevels_TrimID",
                        column: x => x.TrimID,
                        principalTable: "TrimLevels",
                        principalColumn: "TrimID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    VehicleID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VIN = table.Column<string>(type: "TEXT", maxLength: 17, nullable: false),
                    TrimID = table.Column<int>(type: "INTEGER", nullable: false),
                    EngineID = table.Column<int>(type: "INTEGER", nullable: false),
                    Mileage = table.Column<int>(type: "INTEGER", nullable: false),
                    IsUsed = table.Column<bool>(type: "INTEGER", nullable: false),
                    DealershipID = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.VehicleID);
                    table.ForeignKey(
                        name: "FK_Vehicles_Dealerships_DealershipID",
                        column: x => x.DealershipID,
                        principalTable: "Dealerships",
                        principalColumn: "DealershipID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Vehicles_Engines_EngineID",
                        column: x => x.EngineID,
                        principalTable: "Engines",
                        principalColumn: "EngineID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Vehicles_TrimLevels_TrimID",
                        column: x => x.TrimID,
                        principalTable: "TrimLevels",
                        principalColumn: "TrimID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    JobID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VehicleID = table.Column<int>(type: "INTEGER", nullable: false),
                    WorkerID = table.Column<int>(type: "INTEGER", nullable: false),
                    FeatureID = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.JobID);
                    table.ForeignKey(
                        name: "FK_Jobs_Features_FeatureID",
                        column: x => x.FeatureID,
                        principalTable: "Features",
                        principalColumn: "FeatureID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Jobs_Vehicles_VehicleID",
                        column: x => x.VehicleID,
                        principalTable: "Vehicles",
                        principalColumn: "VehicleID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Jobs_Workers_WorkerID",
                        column: x => x.WorkerID,
                        principalTable: "Workers",
                        principalColumn: "WorkerID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SalesOrders",
                columns: table => new
                {
                    OrderID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VehicleID = table.Column<int>(type: "INTEGER", nullable: false),
                    ClientID = table.Column<int>(type: "INTEGER", nullable: false),
                    WorkerID = table.Column<int>(type: "INTEGER", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FinalPrice = table.Column<decimal>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    DealershipID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesOrders", x => x.OrderID);
                    table.ForeignKey(
                        name: "FK_SalesOrders_Clients_ClientID",
                        column: x => x.ClientID,
                        principalTable: "Clients",
                        principalColumn: "ClientID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalesOrders_Dealerships_DealershipID",
                        column: x => x.DealershipID,
                        principalTable: "Dealerships",
                        principalColumn: "DealershipID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalesOrders_Vehicles_VehicleID",
                        column: x => x.VehicleID,
                        principalTable: "Vehicles",
                        principalColumn: "VehicleID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalesOrders_Workers_WorkerID",
                        column: x => x.WorkerID,
                        principalTable: "Workers",
                        principalColumn: "WorkerID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VehicleFeatures",
                columns: table => new
                {
                    VehicleID = table.Column<int>(type: "INTEGER", nullable: false),
                    FeatureID = table.Column<int>(type: "INTEGER", nullable: false),
                    PurchasePrice = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleFeatures", x => new { x.VehicleID, x.FeatureID });
                    table.ForeignKey(
                        name: "FK_VehicleFeatures_Features_FeatureID",
                        column: x => x.FeatureID,
                        principalTable: "Features",
                        principalColumn: "FeatureID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VehicleFeatures_Vehicles_VehicleID",
                        column: x => x.VehicleID,
                        principalTable: "Vehicles",
                        principalColumn: "VehicleID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_RoleID",
                table: "AppUsers",
                column: "RoleID");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_UserID",
                table: "Clients",
                column: "UserID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_FeatureID",
                table: "Jobs",
                column: "FeatureID");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_VehicleID",
                table: "Jobs",
                column: "VehicleID");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_WorkerID",
                table: "Jobs",
                column: "WorkerID");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_ClientID",
                table: "SalesOrders",
                column: "ClientID");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_DealershipID",
                table: "SalesOrders",
                column: "DealershipID");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_VehicleID",
                table: "SalesOrders",
                column: "VehicleID");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_WorkerID",
                table: "SalesOrders",
                column: "WorkerID");

            migrationBuilder.CreateIndex(
                name: "IX_TrimFeatures_FeatureID",
                table: "TrimFeatures",
                column: "FeatureID");

            migrationBuilder.CreateIndex(
                name: "IX_TrimLevels_ModelID",
                table: "TrimLevels",
                column: "ModelID");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleFeatures_FeatureID",
                table: "VehicleFeatures",
                column: "FeatureID");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_DealershipID",
                table: "Vehicles",
                column: "DealershipID");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_EngineID",
                table: "Vehicles",
                column: "EngineID");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_TrimID",
                table: "Vehicles",
                column: "TrimID");

            migrationBuilder.CreateIndex(
                name: "IX_Workers_DealershipID",
                table: "Workers",
                column: "DealershipID");

            migrationBuilder.CreateIndex(
                name: "IX_Workers_UserID",
                table: "Workers",
                column: "UserID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Jobs");

            migrationBuilder.DropTable(
                name: "SalesOrders");

            migrationBuilder.DropTable(
                name: "TrimFeatures");

            migrationBuilder.DropTable(
                name: "VehicleFeatures");

            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropTable(
                name: "Workers");

            migrationBuilder.DropTable(
                name: "Features");

            migrationBuilder.DropTable(
                name: "Vehicles");

            migrationBuilder.DropTable(
                name: "AppUsers");

            migrationBuilder.DropTable(
                name: "Dealerships");

            migrationBuilder.DropTable(
                name: "Engines");

            migrationBuilder.DropTable(
                name: "TrimLevels");

            migrationBuilder.DropTable(
                name: "AppRoles");

            migrationBuilder.DropTable(
                name: "VehicleModels");
        }
    }
}
