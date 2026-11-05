using ServiceSchoolBot.Application.Databases.Repository.Interfaces;
using System.Collections.Concurrent;
using ServiceSchoolBot.Domain.Identity;

namespace ServiceSchoolBot.Infrastructure.Repositories.Storage.Memory;

public sealed class GradeRepository : IGradeRepository
{
    public Task<bool> VerifyGradeWriterReadyAsync(int schoolId, CancellationToken ct = default)
    {
        return Task.FromResult(true);
    }
}
