using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Windows.Forms;
using Echoslate.ViewModels;

namespace Echoslate;

public class AppDataFileSettings : INotifyPropertyChanged {
	private bool _autoSave;
	public bool AutoSave {
		get => _autoSave;
		set {
			_autoSave = value;
			OnPropertyChanged();
		}
	}
	private bool _autoBackup;
	public bool AutoBackup {
		get => _autoBackup;
		set {
			_autoBackup = value;
			OnPropertyChanged();
		}
	}
	private int _backupIncrement;
	public int BackupIncrement {
		get => _backupIncrement;
		set {
			_backupIncrement = value;
			OnPropertyChanged();
		}
	}
	private int _backupTime;
	public int BackupTime {
		get => _backupTime;
		set {
			_backupTime = value;
			OnPropertyChanged();
		}
	}

	private Version _currentProjectVersion;
	public Version CurrentProjectVersion {
		get => _currentProjectVersion;
		set {
			_currentProjectVersion = value;
			OnPropertyChanged();
		}
	}
	public string GitRepoPath { get; set; }

	private IncrementMode _incrementMode;
	public IncrementMode IncrementMode {
		get => _incrementMode;
		set {
			_incrementMode = value;
			OnPropertyChanged();
		}
	}
	[JsonIgnore] public string GitStatusMessage { get; set; }
	[JsonIgnore] public bool CanDetectBranch { get; set; }
	[JsonIgnore] public bool IsGitInstalled { get; set; }

	public AppDataFileSettings() {
		AutoSave = false;
		AutoBackup = false;
		BackupIncrement = 0;
		BackupTime = 5;
		CurrentProjectVersion = new Version(0, 0, 0, 0);
		IncrementMode = IncrementMode.None;
	}
	public static AppDataFileSettings Create(bool autoSave, bool autoBackup, int backupIncrement, int backupTime, Version version, IncrementMode incrementMode = IncrementMode.None) {
		return new AppDataFileSettings {
			AutoSave = autoSave,
			AutoBackup = autoBackup,
			BackupIncrement = backupIncrement,
			BackupTime = backupTime,
			CurrentProjectVersion = version,
			IncrementMode = incrementMode
		};
	}
	public string SuggestRepoPath(string currentFilePath) {
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
	public bool GitInstallCheck() {
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
	public void InitGitSettings(string currentFilePath) {
		string suggested = SuggestRepoPath(currentFilePath);
		bool pathValid = !string.IsNullOrEmpty(suggested) && Directory.Exists(Path.Combine(suggested, ".git"));
		IsGitInstalled = GitInstallCheck();

		if (pathValid && string.IsNullOrEmpty(GitRepoPath)) {
			var result = MessageBox.Show($"Git repository detected at:\n{suggested}\nUse this path for branch detection and scope suggestions?",
				"Git Repository Found",
				MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (result == DialogResult.Yes) {
				GitRepoPath = suggested;
				UpdateGitFeaturesState();
				if (IsGitInstalled) {
					Log.Debug($"Git ready ({GitStatusMessage})");
				}
			}
		}
	}
	public void UpdateGitFeaturesState() {
		if (Directory.Exists(Path.Combine(GitRepoPath, ".git")) && IsGitInstalled) {
			GitStatusMessage = $"✓ Repo: {GitRepoPath}";
			CanDetectBranch = true;
		} else if (string.IsNullOrEmpty(GitRepoPath) && !IsGitInstalled) {
			GitStatusMessage = "⚠ Git repository path not set\nGit is not installed - download from https://git-scm.com/downloads";
			CanDetectBranch = false;
		} else if (string.IsNullOrEmpty(GitRepoPath)) {
			GitStatusMessage = "⚠ Git repository path not set";
			CanDetectBranch = false;
		} else if (!IsGitInstalled) {
			GitStatusMessage = "⚠ Git not found — download from https://git-scm.com/downloads";
			CanDetectBranch = false;
		} else {
			GitStatusMessage = "⚠ Invalid repo path (no .git folder)";
			CanDetectBranch = false;
		}
	}


	public event PropertyChangedEventHandler? PropertyChanged;
	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
	protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
		if (EqualityComparer<T>.Default.Equals(field, value)) return false;
		field = value;
		OnPropertyChanged(propertyName);
		return true;
	}
}