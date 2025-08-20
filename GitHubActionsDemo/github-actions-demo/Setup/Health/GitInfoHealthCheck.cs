using System.Reflection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace github_actions_demo.Setup.Health;
public class GitInfoHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
    {
        try
        {
            return await Task.FromResult(HealthCheckResult.Healthy(
                "GitInfo",
                    GitInfoDictionary()));
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(
                status: context.Registration.FailureStatus,
                exception: ex);
        }
    }

    internal IReadOnlyDictionary<string, object> GitInfoDictionary()
    {
        var entryAssembly = Assembly.GetEntryAssembly();
        Type? gitAssemblyGit = entryAssembly?.GetType("ThisAssembly+Git");
        Type? gitAssemblyGitBase = entryAssembly?.GetType("ThisAssembly+Git+BaseVersion");

        string branch = (string)(gitAssemblyGit?.GetField("Branch")?.GetValue(null));
        string commit = (string)(gitAssemblyGit?.GetField("Commit")?.GetValue(null));
        string gitSha = (string)(gitAssemblyGit?.GetField("Sha")?.GetValue(null));
        string gitMajorVersion = (string)(gitAssemblyGitBase?.GetField("Major")?.GetValue(null));
        string gitMinorVersion = (string)(gitAssemblyGitBase?.GetField("Minor")?.GetValue(null));
        string gitPatchVersion = (string)(gitAssemblyGitBase?.GetField("Patch")?.GetValue(null));
        string gitBaseTag = (string)(gitAssemblyGit?.GetField("BaseTag")?.GetValue(null));

        var assemblyGitInfo = new AssemblyGitInfo
        {
            Branch = branch ?? string.Empty,
            Commit = commit ?? string.Empty,
            Sha = gitSha ?? string.Empty,
            MajorVersion = gitMajorVersion ?? string.Empty,
            MinorVersion = gitMinorVersion ?? string.Empty,
            BaseTag = gitBaseTag ?? string.Empty,
            PatchVersion = gitPatchVersion ?? string.Empty
        };

        return new Dictionary<string, object>()
                {
                    { "gitBranch", assemblyGitInfo.Branch }, //ThisAssembly.Git.Branch },
                    { "gitCommit", assemblyGitInfo.Commit },
                    { "gitSha", assemblyGitInfo.Sha},
                    { "gitBaseVersion", $"{assemblyGitInfo.MajorVersion}.{assemblyGitInfo.MinorVersion}.{assemblyGitInfo.PatchVersion}" },
                    { "gitBaseTag", assemblyGitInfo.BaseTag }
                };
    }
}

