using ServiceSchoolBot.Domain.Identity;

namespace ServiceSchoolBot.Application.Databases.Repository.Interfaces;

public interface IGradeRepository
{
    Task<bool> VerifyGradeWriterReadyAsync(int schoolId, CancellationToken ct = default);
}
