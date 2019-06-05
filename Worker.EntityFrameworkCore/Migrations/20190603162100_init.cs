using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Worker.EntityFrameworkCore.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
          
            migrationBuilder.Sql(
                @"
                    CREATE TRIGGER SetEmployeeTimestampOnUpdate
                    AFTER UPDATE ON Employee
                    BEGIN
                        UPDATE Employee
                        SET Timestamp = randomblob(8)
                        WHERE rowid = NEW.rowid;
                    END
                ");
             migrationBuilder.Sql(
                @"
                    CREATE TRIGGER SetEmployeeTimestampOnInsert
                    AFTER INSERT ON Employee
                    BEGIN
                        UPDATE Employee
                        SET Timestamp = randomblob(8)
                        WHERE rowid = NEW.rowid;
                    END
                ");

          

            migrationBuilder.Sql(
                @"
                    CREATE TRIGGER SetСontrollersTimestampOnUpdate
                    AFTER UPDATE ON Сontrollers
                    BEGIN
                        UPDATE Сontrollers
                        SET Timestamp = randomblob(8)
                        WHERE rowid = NEW.rowid;
                    END
                ");
            migrationBuilder.Sql(
               @"
                    CREATE TRIGGER SetСontrollersTimestampOnInsert
                    AFTER INSERT ON Сontrollers
                    BEGIN
                        UPDATE Сontrollers
                        SET Timestamp = randomblob(8)
                        WHERE rowid = NEW.rowid;
                    END
                ");


            migrationBuilder.Sql(
                @"
                    CREATE TRIGGER SetCardsTimestampOnUpdate
                    AFTER UPDATE ON Cards
                    BEGIN
                        UPDATE Cards
                        SET Timestamp = randomblob(8)
                        WHERE rowid = NEW.rowid;
                    END
                ");
            migrationBuilder.Sql(
                @"
                    CREATE TRIGGER SetCardsTimestampOnInsert
                    AFTER INSERT ON Cards
                    BEGIN
                        UPDATE Cards
                        SET Timestamp = randomblob(8)
                        WHERE rowid = NEW.rowid;
                    END
                ");

          

            migrationBuilder.Sql(
                @"
                    CREATE TRIGGER SetDevicesTimestampOnUpdate
                    AFTER UPDATE ON Devices
                    BEGIN
                        UPDATE Devices
                        SET Timestamp = randomblob(8)
                        WHERE rowid = NEW.rowid;
                    END
                ");
            migrationBuilder.Sql(
               @"
                    CREATE TRIGGER SetDevicesTimestampOnInsert
                    AFTER INSERT ON Devices
                    BEGIN
                        UPDATE Devices
                        SET Timestamp = randomblob(8)
                        WHERE rowid = NEW.rowid;
                    END
                ");

          

            migrationBuilder.Sql(
                @"
                    CREATE TRIGGER SetControllerConfigsTimestampOnUpdate
                    AFTER UPDATE ON ControllerConfigs
                    BEGIN
                        UPDATE ControllerConfigs
                        SET Timestamp = randomblob(8)
                        WHERE rowid = NEW.rowid;
                    END
                ");
            migrationBuilder.Sql(
                @"
                    CREATE TRIGGER SetControllerConfigsTimestampOnInsert
                    AFTER INSERT ON ControllerConfigs
                    BEGIN
                        UPDATE ControllerConfigs
                        SET Timestamp = randomblob(8)
                        WHERE rowid = NEW.rowid;
                    END
                ");


            migrationBuilder.Sql(
               @"
                    CREATE TRIGGER SetTerminalsTimestampOnUpdate
                    AFTER UPDATE ON Terminals
                    BEGIN
                        UPDATE Terminals
                        SET Timestamp = randomblob(8)
                        WHERE rowid = NEW.rowid;
                    END
                ");
            migrationBuilder.Sql(
                @"
                    CREATE TRIGGER SetTerminalsTimestampOnInsert
                    AFTER INSERT ON Terminals
                    BEGIN
                        UPDATE Terminals
                        SET Timestamp = randomblob(8)
                        WHERE rowid = NEW.rowid;
                    END
                ");

            

            migrationBuilder.Sql(
                @"
                    CREATE TRIGGER SetFingerprintsTimestampOnUpdate
                    AFTER UPDATE ON Fingerprints
                    BEGIN
                        UPDATE Fingerprints
                        SET Timestamp = randomblob(8)
                        WHERE rowid = NEW.rowid;
                    END
                ");
            migrationBuilder.Sql(
                @"
                    CREATE TRIGGER SetFingerprintsTimestampOnInsert
                    AFTER INSERT ON Fingerprints
                    BEGIN
                        UPDATE Fingerprints
                        SET Timestamp = randomblob(8)
                        WHERE rowid = NEW.rowid;
                    END
                ");


            migrationBuilder.Sql(
                @"
                    CREATE TRIGGER SetTerminalConfigsTimestampOnUpdate
                    AFTER UPDATE ON TerminalConfigs
                    BEGIN
                        UPDATE TerminalConfigs
                        SET Timestamp = randomblob(8)
                        WHERE rowid = NEW.rowid;
                    END
                ");
            migrationBuilder.Sql(
                @"
                    CREATE TRIGGER SetTerminalConfigsTimestampOnInsert
                    AFTER INSERT ON TerminalConfigs
                    BEGIN
                        UPDATE TerminalConfigs
                        SET Timestamp = randomblob(8)
                        WHERE rowid = NEW.rowid;
                    END
                ");

           
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cards");

            migrationBuilder.DropTable(
                name: "ControllerConfigs");

            migrationBuilder.DropTable(
                name: "Devices");

            migrationBuilder.DropTable(
                name: "Fingerprints");

            migrationBuilder.DropTable(
                name: "TerminalConfigs");

            migrationBuilder.DropTable(
                name: "Employee");

            migrationBuilder.DropTable(
                name: "Terminals");

            migrationBuilder.DropTable(
                name: "Сontrollers");
        }
    }
}
