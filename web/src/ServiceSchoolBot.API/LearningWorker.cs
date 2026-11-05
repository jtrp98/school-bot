using ServiceSchoolBot.Application.Services.Implementations.Assessment;

namespace ServiceSchoolBot.API;

public sealed class LearningWorker(IServiceScopeFactory scopes, ILogger<LearningWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = scopes.CreateScope();
                var expired = await scope.ServiceProvider.GetRequiredService<AttemptFinalizer>().SweepAsync(ct: stoppingToken);
                var graded = await scope.ServiceProvider.GetRequiredService<LearningSubmissionService>().SweepAsync(ct: stoppingToken);
                var exports = await scope.ServiceProvider.GetRequiredService<ScoreExportService>().SweepAsync(stoppingToken);
                if (expired.Completed + graded > 0) logger.LogInformation("Learning sweep completed: {ExpiredAttempts} expired attempts, {GradedSubmissions} graded submissions", expired.Completed, graded);
                if (expired.Deferred > 0) logger.LogWarning("Learning sweep deferred {Count} attempts because their stored evaluator is unavailable", expired.Deferred);
                if (exports.Completed > 0) logger.LogInformation("Reconciled {Count} grade export items", exports.Completed);
                if (exports.Deferred > 0) logger.LogWarning("Deferred {Count} grade exports pending current authority or verified writer", exports.Deferred);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { return; }
            catch (Exception ex) { logger.LogError(ex, "Learning sweep failed; durable work remains available for retry"); }
        }
    }
}
