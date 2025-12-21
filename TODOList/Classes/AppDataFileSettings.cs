using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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
	private IncrementMode _incrementMode;
	public IncrementMode IncrementMode {
		get => _incrementMode;
		set {
			_incrementMode = value;
			OnPropertyChanged();
		}
	}

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