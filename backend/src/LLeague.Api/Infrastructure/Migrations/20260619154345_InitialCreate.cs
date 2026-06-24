using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LLeague.Api.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        _ = migrationBuilder.CreateTable(
            name: "AdminUsers",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Username = table.Column<string>(type: "text", nullable: false),
                PasswordHash = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                _ = table.PrimaryKey("PK_AdminUsers", x => x.Id);
            });

        _ = migrationBuilder.CreateTable(
            name: "Seasons",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                Year = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                _ = table.PrimaryKey("PK_Seasons", x => x.Id);
            });

        _ = migrationBuilder.CreateTable(
            name: "Teams",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Number = table.Column<int>(type: "integer", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                Affiliation = table.Column<string>(type: "text", nullable: false),
                City = table.Column<string>(type: "text", nullable: false),
                Region = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false)
            },
            constraints: table =>
            {
                _ = table.PrimaryKey("PK_Teams", x => x.Id);
            });

        _ = migrationBuilder.CreateTable(
            name: "Events",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                SeasonId = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                Slug = table.Column<string>(type: "text", nullable: false),
                StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                Location = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                _ = table.PrimaryKey("PK_Events", x => x.Id);
                _ = table.ForeignKey(
                    name: "FK_Events_Seasons_SeasonId",
                    column: x => x.SeasonId,
                    principalTable: "Seasons",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        _ = migrationBuilder.CreateTable(
            name: "Divisions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                EventId = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                Color = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                _ = table.PrimaryKey("PK_Divisions", x => x.Id);
                _ = table.ForeignKey(
                    name: "FK_Divisions_Events_EventId",
                    column: x => x.EventId,
                    principalTable: "Events",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        _ = migrationBuilder.CreateTable(
            name: "Matches",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                DivisionId = table.Column<Guid>(type: "uuid", nullable: false),
                Round = table.Column<int>(type: "integer", nullable: false),
                Number = table.Column<int>(type: "integer", nullable: false),
                Stage = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                ScheduledTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                StartTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                _ = table.PrimaryKey("PK_Matches", x => x.Id);
                _ = table.ForeignKey(
                    name: "FK_Matches_Divisions_DivisionId",
                    column: x => x.DivisionId,
                    principalTable: "Divisions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        _ = migrationBuilder.CreateTable(
            name: "Tables",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                DivisionId = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                _ = table.PrimaryKey("PK_Tables", x => x.Id);
                _ = table.ForeignKey(
                    name: "FK_Tables_Divisions_DivisionId",
                    column: x => x.DivisionId,
                    principalTable: "Divisions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        _ = migrationBuilder.CreateTable(
            name: "TeamDivisions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                DivisionId = table.Column<Guid>(type: "uuid", nullable: false),
                Arrived = table.Column<bool>(type: "boolean", nullable: false),
                ArrivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                _ = table.PrimaryKey("PK_TeamDivisions", x => x.Id);
                _ = table.ForeignKey(
                    name: "FK_TeamDivisions_Divisions_DivisionId",
                    column: x => x.DivisionId,
                    principalTable: "Divisions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                _ = table.ForeignKey(
                    name: "FK_TeamDivisions_Teams_TeamId",
                    column: x => x.TeamId,
                    principalTable: "Teams",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        _ = migrationBuilder.CreateTable(
            name: "MatchParticipants",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                TableId = table.Column<Guid>(type: "uuid", nullable: false),
                TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                Ready = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                _ = table.PrimaryKey("PK_MatchParticipants", x => x.Id);
                _ = table.ForeignKey(
                    name: "FK_MatchParticipants_Matches_MatchId",
                    column: x => x.MatchId,
                    principalTable: "Matches",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                _ = table.ForeignKey(
                    name: "FK_MatchParticipants_Tables_TableId",
                    column: x => x.TableId,
                    principalTable: "Tables",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                _ = table.ForeignKey(
                    name: "FK_MatchParticipants_Teams_TeamId",
                    column: x => x.TeamId,
                    principalTable: "Teams",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        _ = migrationBuilder.CreateTable(
            name: "Scoresheets",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                MatchParticipantId = table.Column<Guid>(type: "uuid", nullable: false),
                Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                Score = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                _ = table.PrimaryKey("PK_Scoresheets", x => x.Id);
                _ = table.ForeignKey(
                    name: "FK_Scoresheets_MatchParticipants_MatchParticipantId",
                    column: x => x.MatchParticipantId,
                    principalTable: "MatchParticipants",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        _ = migrationBuilder.CreateTable(
            name: "MissionValues",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ScoresheetId = table.Column<Guid>(type: "uuid", nullable: false),
                MissionId = table.Column<string>(type: "text", nullable: false),
                ClauseIndex = table.Column<int>(type: "integer", nullable: false),
                ValueType = table.Column<string>(type: "text", nullable: false),
                ValueRaw = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                _ = table.PrimaryKey("PK_MissionValues", x => x.Id);
                _ = table.ForeignKey(
                    name: "FK_MissionValues_Scoresheets_ScoresheetId",
                    column: x => x.ScoresheetId,
                    principalTable: "Scoresheets",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        _ = migrationBuilder.CreateIndex(
            name: "IX_AdminUsers_Username",
            table: "AdminUsers",
            column: "Username",
            unique: true);

        _ = migrationBuilder.CreateIndex(
            name: "IX_Divisions_EventId",
            table: "Divisions",
            column: "EventId");

        _ = migrationBuilder.CreateIndex(
            name: "IX_Events_SeasonId",
            table: "Events",
            column: "SeasonId");

        _ = migrationBuilder.CreateIndex(
            name: "IX_Matches_DivisionId",
            table: "Matches",
            column: "DivisionId");

        _ = migrationBuilder.CreateIndex(
            name: "IX_MatchParticipants_MatchId",
            table: "MatchParticipants",
            column: "MatchId");

        _ = migrationBuilder.CreateIndex(
            name: "IX_MatchParticipants_TableId",
            table: "MatchParticipants",
            column: "TableId");

        _ = migrationBuilder.CreateIndex(
            name: "IX_MatchParticipants_TeamId",
            table: "MatchParticipants",
            column: "TeamId");

        _ = migrationBuilder.CreateIndex(
            name: "IX_MissionValues_ScoresheetId",
            table: "MissionValues",
            column: "ScoresheetId");

        _ = migrationBuilder.CreateIndex(
            name: "IX_Scoresheets_MatchParticipantId",
            table: "Scoresheets",
            column: "MatchParticipantId",
            unique: true);

        _ = migrationBuilder.CreateIndex(
            name: "IX_Tables_DivisionId",
            table: "Tables",
            column: "DivisionId");

        _ = migrationBuilder.CreateIndex(
            name: "IX_TeamDivisions_DivisionId",
            table: "TeamDivisions",
            column: "DivisionId");

        _ = migrationBuilder.CreateIndex(
            name: "IX_TeamDivisions_TeamId_DivisionId",
            table: "TeamDivisions",
            columns: ["TeamId", "DivisionId"],
            unique: true);

        _ = migrationBuilder.CreateIndex(
            name: "IX_Teams_Number_Region",
            table: "Teams",
            columns: ["Number", "Region"],
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        _ = migrationBuilder.DropTable(
            name: "AdminUsers");

        _ = migrationBuilder.DropTable(
            name: "MissionValues");

        _ = migrationBuilder.DropTable(
            name: "TeamDivisions");

        _ = migrationBuilder.DropTable(
            name: "Scoresheets");

        _ = migrationBuilder.DropTable(
            name: "MatchParticipants");

        _ = migrationBuilder.DropTable(
            name: "Matches");

        _ = migrationBuilder.DropTable(
            name: "Tables");

        _ = migrationBuilder.DropTable(
            name: "Teams");

        _ = migrationBuilder.DropTable(
            name: "Divisions");

        _ = migrationBuilder.DropTable(
            name: "Events");

        _ = migrationBuilder.DropTable(
            name: "Seasons");
    }
}
