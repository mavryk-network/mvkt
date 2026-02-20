using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mvkt.Data.Migrations
{
    /// <inheritdoc />
    public partial class BakerCycles_BakerId_CycleDesc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ""IX_BakerCycles_BakerId_Cycle_Desc""
                ON ""BakerCycles"" (""BakerId"", ""Cycle"" DESC);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BakerCycles_BakerId_Cycle_Desc",
                table: "BakerCycles");
        }
    }
}
