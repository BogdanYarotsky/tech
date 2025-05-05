using System.Data;
using FluentMigrator;

namespace DataLoader.Database.Migrations
{
    [Migration(1, "InitialCreate")]
    public class InitialCreate : Migration
    {
        public override void Up()
        {
            Create.Table("TagTypes")
                .WithColumn("TagTypeID").AsInt32().NotNullable().PrimaryKey("PK_TagTypes").Identity()
                .WithColumn("TagTypeName").AsString(100).NotNullable().Unique("UQ_TagTypes_TagTypeName");

            Create.Table("Tags")
                .WithColumn("TagID").AsInt32().NotNullable().PrimaryKey("PK_Tags").Identity()
                .WithColumn("TagName").AsString(100).NotNullable()
                .WithColumn("TagTypeID").AsInt32().NotNullable()
                    .ForeignKey("FK_Tags_TagTypes", "TagTypes", "TagTypeID")
                    .OnDelete(Rule.None) // Corresponds to ON DELETE NO ACTION
                    .OnUpdate(Rule.Cascade); // Corresponds to ON UPDATE CASCADE

            // Unique constraint for TagTypeID and TagName combination
            Create.UniqueConstraint("UQ_Tags_TagName_TagTypeID")
                  .OnTable("Tags").Columns("TagTypeID", "TagName");

            // Index for finding tags by type quickly
            Create.Index("IX_Tags_TagTypeID")
                  .OnTable("Tags").OnColumn("TagTypeID");

            Create.Table("Countries")
                .WithColumn("CountryID").AsInt32().NotNullable().PrimaryKey("PK_Countries").Identity()
                .WithColumn("CountryName").AsString(100).NotNullable().Unique("UQ_Countries_CountryName");

            Create.Table("Reports")
                .WithColumn("ReportID").AsInt32().NotNullable().PrimaryKey("PK_Reports").Identity()
                .WithColumn("CountryID").AsInt32().NotNullable()
                    .ForeignKey("FK_Reports_Countries", "Countries", "CountryID")
                    .OnDelete(Rule.None)    // Corresponds to ON DELETE NO ACTION
                    .OnUpdate(Rule.Cascade) // Corresponds to ON UPDATE CASCADE
                .WithColumn("Year").AsInt16().NotNullable() // SMALLINT -> AsInt16
                .WithColumn("YearsCoding").AsByte().NotNullable() // TINYINT -> AsByte
                .WithColumn("YearlySalaryUSD").AsInt32().Nullable();

            // Check constraints require raw SQL as FluentMigrator doesn't have specific methods for them
            // Note: GETDATE() is SQL Server specific. Use appropriate function if targeting other DBs.
            Execute.Sql(@"
                ALTER TABLE dbo.Reports
                ADD CONSTRAINT CK_Reports_Year CHECK ([Year] BETWEEN 1980 AND YEAR(GETDATE()) + 1);");
            Execute.Sql(@"
                ALTER TABLE dbo.Reports
                ADD CONSTRAINT CK_Reports_YearsCoding CHECK (YearsCoding >= 0);");
            Execute.Sql(@"
                ALTER TABLE dbo.Reports
                ADD CONSTRAINT CK_Reports_YearlySalary CHECK (YearlySalaryUSD >= 0);");

            Create.Index("IX_Reports_CountryID").OnTable("Reports").OnColumn("CountryID");
            Create.Index("IX_Reports_Year").OnTable("Reports").OnColumn("Year");
            Create.Index("IX_Reports_YearsCoding").OnTable("Reports").OnColumn("YearsCoding");

            // Filtered Index (Requires SQL Server 2008+ or compatible DB)
            // Use raw SQL for filtered index as direct support might vary or be less intuitive
            Execute.Sql(@"
                CREATE NONCLUSTERED INDEX IX_Reports_YearlySalaryUSD
                ON dbo.Reports(YearlySalaryUSD)
                WHERE YearlySalaryUSD IS NOT NULL;");
            // // Alternative FM syntax (might work depending on provider):
            // Create.Index("IX_Reports_YearlySalaryUSD").OnTable("Reports")
            //       .OnColumn("YearlySalaryUSD")
            //       .WithOptions().Filter("YearlySalaryUSD IS NOT NULL");


            // Composite Index
            Create.Index("IX_Reports_CountryID_Year").OnTable("Reports")
                  .OnColumn("CountryID").Ascending()
                  .OnColumn("Year").Ascending();

            // =============================================
            // Table: ReportsTags (Many-to-Many Junction Table)
            // =============================================
            Create.Table("ReportsTags")
                .WithColumn("ReportID").AsInt32().NotNullable()
                    .ForeignKey("FK_ReportsTags_Reports", "Reports", "ReportID")
                    .OnDelete(Rule.Cascade) // If a Report is deleted, remove its tag associations
                .WithColumn("TagID").AsInt32().NotNullable()
                    .ForeignKey("FK_ReportsTags_Tags", "Tags", "TagID")
                    .OnDelete(Rule.Cascade); // If a Tag is deleted, remove its associations from reports

            // Composite Primary Key
            // Needs to be defined separately after columns if composite
            Create.PrimaryKey("PK_ReportsTags")
                  .OnTable("ReportsTags").Columns("ReportID", "TagID");

            // Index optimized for finding reports for a given tag
            Create.Index("IX_ReportsTags_TagID_ReportID").OnTable("ReportsTags")
                  .OnColumn("TagID").Ascending()
                  .OnColumn("ReportID").Ascending();
        }

        public override void Down()
        {
            Delete.Index("IX_ReportsTags_TagID_ReportID").OnTable("ReportsTags");
            Delete.Table("ReportsTags");
            Delete.Index("IX_Reports_CountryID_Year").OnTable("Reports");
            Execute.Sql(@"DROP INDEX IF EXISTS IX_Reports_YearlySalaryUSD ON dbo.Reports;");
            Delete.Index("IX_Reports_YearsCoding").OnTable("Reports");
            Delete.Index("IX_Reports_Year").OnTable("Reports");
            Delete.Index("IX_Reports_CountryID").OnTable("Reports");
            Execute.Sql(@"ALTER TABLE dbo.Reports DROP CONSTRAINT IF EXISTS CK_Reports_YearlySalary;");
            Execute.Sql(@"ALTER TABLE dbo.Reports DROP CONSTRAINT IF EXISTS CK_Reports_YearsCoding;");
            Execute.Sql(@"ALTER TABLE dbo.Reports DROP CONSTRAINT IF EXISTS CK_Reports_Year;");
            Delete.Table("Reports");
            Delete.Table("Countries");
            Delete.UniqueConstraint("UQ_Tags_TagName_TagTypeID").FromTable("Tags"); // Drop separately created constraint
            Delete.Table("Tags");
            Delete.Table("TagTypes");
        }
    }
}