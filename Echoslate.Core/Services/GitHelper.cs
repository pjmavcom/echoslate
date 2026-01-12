using System.Diagnostics;
using Echoslate.Core.Models;

namespace Echoslate.Core.Services;

public static class GitHelper {
	public static void InitGitSettings(AppData data) {
		string suggested = SuggestRepoPath(data.CurrentFilePath);
		bool pathValid = !string.IsNullOrEmpty(suggested) && Directory.Exists(Path.Combine(suggested, ".git"));
		data.FileSettings.IsGitInstalled = GitInstallCheck();

		if (pathValid && string.IsNullOrEmpty(data.FileSettings.GitRepoPath)) {
			Core.Services.DialogResult result = AppServices.DialogService.Show($"Git repository detected at:\n{suggested}\nUse this path for branch detection and scope suggestions?", "Git Repository Found", DialogButton.YesNo, DialogIcon.Question);
			if (result == (Core.Services.DialogResult)DialogResult.Yes) {
				data.FileSettings.GitRepoPath = suggested;
				UpdateGitFeaturesState(data);
				if (data.FileSettings.IsGitInstalled) {
					Log.Debug($"Git ready ({data.FileSettings.GitStatusMessage})");
				}
			}
		}
	}
	public static string SuggestRepoPath(string currentFilePath) {
		string currentDir = Path.GetDirectoryName(currentFilePath);

		var dir = new DirectoryInfo(currentDir);
		while (dir != null) {
			if (Directory.Exists(Path.Combine(dir.FullName, ".git"))) {
				return dir.FullName;
			}
			dir = dir.Parent;
		}
		return null;
	}
	public static bool GitInstallCheck() {
		try {
			var process = new Process {
				StartInfo = new ProcessStartInfo {
					FileName = "git",
					Arguments = "--version",
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					CreateNoWindow = true
				}
			};
			process.Start();
			process.WaitForExit(5000);
			return process.ExitCode == 0;
		} catch {
			return false;
		}
	}
	public static void UpdateGitFeaturesState(AppData data) {
		if (Directory.Exists(Path.Combine(data.FileSettings.GitRepoPath, ".git")) && data.FileSettings.IsGitInstalled) {
			data.FileSettings.GitStatusMessage = $"✓ Repo: {data.FileSettings.GitRepoPath}";
			data.FileSettings.CanDetectBranch = true;
		} else if (string.IsNullOrEmpty(data.FileSettings.GitRepoPath) && !data.FileSettings.IsGitInstalled) {
			data.FileSettings.GitStatusMessage = "⚠ Git repository path not set\nGit is not installed - download from https://git-scm.com/downloads";
			data.FileSettings.CanDetectBranch = false;
		} else if (string.IsNullOrEmpty(data.FileSettings.GitRepoPath)) {
			data.FileSettings.GitStatusMessage = "⚠ Git repository path not set";
			data.FileSettings.CanDetectBranch = false;
		} else if (!data.FileSettings.IsGitInstalled) {
			data.FileSettings.GitStatusMessage = "⚠ Git not found — download from https://git-scm.com/downloads";
			data.FileSettings.CanDetectBranch = false;
		} else {
			data.FileSettings.GitStatusMessage = "⚠ Invalid repo path (no .git folder)";
			data.FileSettings.CanDetectBranch = false;
		}
	}
}