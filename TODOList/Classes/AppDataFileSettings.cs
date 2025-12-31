using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
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
	[JsonIgnore]
	public string GitStatusMessage { get; set; }
	[JsonIgnore]
	public bool CanDetectBranch { get; set; }

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
	public void UpdateGitFeaturesState() {
		if (string.IsNullOrEmpty(GitRepoPath)) {
			GitStatusMessage = "⚠ Git repository path not set";
			CanDetectBranch = false;
		} else if (Directory.Exists(Path.Combine(GitRepoPath, ".git"))) {
			GitStatusMessage = $"✓ Repo: {Path.GetFileName(GitRepoPath)}";
			CanDetectBranch = true;
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