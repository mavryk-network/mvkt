using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Mvkt.Data;

#nullable disable

namespace Mvkt.Data.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(MvktContext))]
    [Migration("20250220100000_BakerCycles_BakerId_CycleDesc")]
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

