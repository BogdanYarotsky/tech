CREATE DATABASE bogdanfirst;
GO

USE bogdanfirst;

-- =============================================
-- Schema Creation Script
-- Target: Developer Survey Data (Example)
-- =============================================
-- Drop tables in reverse order of creation if they exist (for easy re-running)
IF OBJECT_ID('dbo.ReportsTags', 'U') IS NOT NULL DROP TABLE dbo.ReportsTags;
IF OBJECT_ID('dbo.Reports', 'U') IS NOT NULL DROP TABLE dbo.Reports;
IF OBJECT_ID('dbo.Tags', 'U') IS NOT NULL DROP TABLE dbo.Tags;
IF OBJECT_ID('dbo.TagTypes', 'U') IS NOT NULL DROP TABLE dbo.TagTypes;
IF OBJECT_ID('dbo.Countries', 'U') IS NOT NULL DROP TABLE dbo.Countries;
GO

-- =============================================
-- Table: TagTypes
-- Purpose: Stores different categories of tags (e.g., Language, Framework, Database)
-- =============================================
CREATE TABLE dbo.TagTypes (
    TagTypeID INT IDENTITY(1,1) NOT NULL,  -- Auto-incrementing primary key
    TagTypeName NVARCHAR(100) NOT NULL,    -- Name of the tag type (e.g., 'Programming Language')

    -- Constraints
    CONSTRAINT PK_TagTypes PRIMARY KEY CLUSTERED (TagTypeID ASC),
    CONSTRAINT UQ_TagTypes_TagTypeName UNIQUE NONCLUSTERED (TagTypeName ASC) -- Ensure type names are unique
);
GO

PRINT 'Table [dbo].[TagTypes] created.';
GO

-- =============================================
-- Table: Tags
-- Purpose: Stores individual tags associated with a specific type.
-- =============================================
CREATE TABLE dbo.Tags (
    TagID INT IDENTITY(1,1) NOT NULL,    -- Auto-incrementing primary key
    TagName NVARCHAR(100) NOT NULL,      -- Name of the tag (e.g., 'C#', 'React', 'SQL Server')
    TagTypeID INT NOT NULL,              -- Foreign key linking to the TagType

    -- Constraints
    CONSTRAINT PK_Tags PRIMARY KEY CLUSTERED (TagID ASC),
    CONSTRAINT FK_Tags_TagTypes FOREIGN KEY (TagTypeID) REFERENCES dbo.TagTypes(TagTypeID)
        ON DELETE NO ACTION  -- Prevent deleting a TagType if Tags reference it
        ON UPDATE CASCADE,   -- If TagTypeID changes in TagTypes, update it here
    -- CONSTRAINT UQ_Tags_TagName UNIQUE NONCLUSTERED (TagName ASC) -- Assume tag names are globally unique
    CONSTRAINT UQ_Tags_TagName_TagTypeID UNIQUE NONCLUSTERED (TagTypeID, TagName ASC)
);
GO

-- Indexes
-- Index for finding tags by type quickly (supports the FK and common lookups)
CREATE NONCLUSTERED INDEX IX_Tags_TagTypeID ON dbo.Tags(TagTypeID);
GO

PRINT 'Table [dbo].[Tags] created.';
GO

-- =============================================
-- Table: Countries
-- Purpose: Stores a list of countries.
-- =============================================
CREATE TABLE dbo.Countries (
    CountryID INT IDENTITY(1,1) NOT NULL, -- Auto-incrementing primary key
    CountryName NVARCHAR(100) NOT NULL,   -- Name of the country (e.g., 'United States', 'Germany')

    -- Constraints
    CONSTRAINT PK_Countries PRIMARY KEY CLUSTERED (CountryID ASC),
    CONSTRAINT UQ_Countries_CountryName UNIQUE NONCLUSTERED (CountryName ASC) -- Ensure country names are unique
);
GO

PRINT 'Table [dbo].[Countries] created.';
GO

-- =============================================
-- Table: Reports
-- Purpose: Stores the main survey report data, linking to a country.
-- =============================================
CREATE TABLE dbo.Reports (
    ReportID INT IDENTITY(1,1) NOT NULL,  -- Auto-incrementing primary key
    CountryID INT NOT NULL,               -- Foreign key linking to the Country
    [Year] SMALLINT NOT NULL,             -- Survey year (SMALLINT is sufficient) - Use [] as Year is a keyword
    YearsCoding TINYINT NOT NULL,         -- Years of coding experience (TINYINT 0-255 is likely sufficient)
    YearlySalaryUSD INT NULL,             -- Yearly salary in USD (Allow NULLs if salary wasn't reported)

    -- Constraints
    CONSTRAINT PK_Reports PRIMARY KEY CLUSTERED (ReportID ASC),
    CONSTRAINT FK_Reports_Countries FOREIGN KEY (CountryID) REFERENCES dbo.Countries(CountryID)
        ON DELETE NO ACTION  -- Prevent deleting a Country if Reports reference it
        ON UPDATE CASCADE,   -- If CountryID changes in Countries, update it here
    CONSTRAINT CK_Reports_Year CHECK ([Year] BETWEEN 1980 AND YEAR(GETDATE()) + 1), -- Sensible year range check
    CONSTRAINT CK_Reports_YearsCoding CHECK (YearsCoding >= 0), -- Experience cannot be negative
    CONSTRAINT CK_Reports_YearlySalary CHECK (YearlySalaryUSD IS NULL OR YearlySalaryUSD >= 0) -- Salary cannot be negative
);
GO

-- Indexes
-- Index for finding reports by country (supports the FK and common lookups/joins)
CREATE NONCLUSTERED INDEX IX_Reports_CountryID ON dbo.Reports(CountryID);

-- Index for filtering/grouping by year
CREATE NONCLUSTERED INDEX IX_Reports_Year ON dbo.Reports([Year]);

-- Index for filtering/grouping by experience
CREATE NONCLUSTERED INDEX IX_Reports_YearsCoding ON dbo.Reports(YearsCoding);

-- Index for filtering by salary (especially useful if querying ranges)
CREATE NONCLUSTERED INDEX IX_Reports_YearlySalaryUSD ON dbo.Reports(YearlySalaryUSD) WHERE YearlySalaryUSD IS NOT NULL;

-- Example Composite Index: Useful for queries filtering by country AND year
CREATE NONCLUSTERED INDEX IX_Reports_CountryID_Year ON dbo.Reports(CountryID, [Year]);
GO

PRINT 'Table [dbo].[Reports] created.';
GO

-- =============================================
-- Table: ReportsTags (Many-to-Many Junction Table)
-- Purpose: Links Reports to the Tags associated with them.
-- =============================================
CREATE TABLE dbo.ReportsTags (
    ReportID INT NOT NULL, -- Foreign key to Reports
    TagID INT NOT NULL,    -- Foreign key to Tags

    -- Constraints
    -- Composite Primary Key ensures each Report/Tag combination is unique
    CONSTRAINT PK_ReportsTags PRIMARY KEY CLUSTERED (ReportID ASC, TagID ASC),

    -- Foreign Key to Reports table
    CONSTRAINT FK_ReportsTags_Reports FOREIGN KEY (ReportID) REFERENCES dbo.Reports(ReportID)
        ON DELETE CASCADE,  -- If a Report is deleted, remove its tag associations
        -- ON UPDATE CASCADE -- Not needed as ReportID is IDENTITY

    -- Foreign Key to Tags table
    CONSTRAINT FK_ReportsTags_Tags FOREIGN KEY (TagID) REFERENCES dbo.Tags(TagID)
        ON DELETE CASCADE,  -- If a Tag is deleted, remove its associations from reports
        -- ON UPDATE CASCADE -- Not needed as TagID is IDENTITY
);
GO

-- Indexes
-- The Clustered PK (ReportID, TagID) already efficiently supports finding tags for a given report.
-- Add an index optimized for finding reports for a given tag.
CREATE NONCLUSTERED INDEX IX_ReportsTags_TagID_ReportID ON dbo.ReportsTags(TagID ASC, ReportID ASC);
GO

PRINT 'Table [dbo].[ReportsTags] created.';
GO

PRINT 'Schema creation script finished successfully.';
GO