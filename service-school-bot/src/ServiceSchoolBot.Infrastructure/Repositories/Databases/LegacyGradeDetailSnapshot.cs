namespace ServiceSchoolBot.Infrastructure.Repositories.Databases;

public sealed class LegacyGradeDetailSnapshot
{
    public int Id { get; set; }
    public int GradeId { get; set; }
    public int LearnerId { get; set; }
    public int SchoolId { get; set; }
    public string Xmin { get; set; } = "";
    public string Document { get; set; } = "";
}
