using DataLoader;

public record struct Report(
    string Country,
    int Year,
    int YearsCoding,
    int YearlySalaryUsd,
    List<Tag> Tags);