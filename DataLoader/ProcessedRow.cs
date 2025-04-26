public record ProcessedRow(
    string Country,
    int YearsCoding,
    int YearlySalaryUsd,
    List<Tag> Tags);