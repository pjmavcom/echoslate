using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Echoslate.Core.Models;

public enum IncrementMode {
	None,
	Major,
	Minor,
	Build,
	Revision
}
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

	private bool _autoIncrement;
	public bool AutoIncrement {
		get => _autoIncrement;
		set {
			if (_autoIncrement == value) {
				return;
			}
			_autoIncrement = value;
			IncrementMode = _autoIncrement ? IncrementMode.Revision : IncrementMode.None;
			OnPropertyChanged();
		}
	}
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
		AutoSave = true;
		AutoBackup = true;
		BackupIncrement = 0;
		BackupTime = 5;
		CurrentProjectVersion = new Version(0, 0, 0, 0);
		IncrementMode = IncrementMode.None;
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