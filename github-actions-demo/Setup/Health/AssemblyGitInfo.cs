namespace github_actions_demo.Setup.Health;

internal sealed class AssemblyGitInfo
{
    public string? BaseTag { get; set; }
    public string? Branch { get; set; }
    public string? Commit { get; set; }
    public string? Sha { get; set; }
    public string? MajorVersion { get; set; }
    public string? MinorVersion { get; set; }
    public string? PatchVersion { get; set; }
}
