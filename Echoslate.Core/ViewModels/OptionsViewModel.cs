using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Echoslate.Core.Models;
using Echoslate.Core.Services;

namespace Echoslate.Core.ViewModels;

public class OptionsViewModel : INotifyPropertyChanged {
	public IBrushService BrushService => AppServices.BrushService;

	private object _gitStatusColor;
	public object? GitStatusColor {
		get => _gitStatusColor;
		set {
			if (_gitStatusColor == value) {
				return;
			}
			_gitStatusColor = value;
			OnPropertyChanged();
		}
	}

	private bool _autoSave;
	public bool AutoSave {
		get => _autoSave;
		set {
			_autoSave = value;
			OnPropertyChanged();
		}
	}
	private bool _globalHotkeys;
	public bool GlobalHotkeys {
		get => _globalHotkeys;
		set {
			_globalHotkeys = value;
			OnPropertyChanged();
		}
	}
	private bool _showWelcomeWindow;
	public bool ShowWelcomeWindow {
		get => _showWelcomeWindow;
		set {
			_showWelcomeWindow = value;
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
	private int _backupTime;
	public int BackupTime {
		get => _backupTime;
		set {
			_backupTime = value;
			OnPropertyChanged();
		}
	}
	private string _gitRepoPath;
	public string GitRepoPath {
		get => _gitRepoPath;
		set {
			_gitRepoPath = value;
			OnPropertyChanged();
		}
	}
	private string _gitStatusMessage;
	public string GitStatusMessage {
		get => _gitStatusMessage;
		set {
			_gitStatusMessage = value;
			OnPropertyChanged();
		}
	}
	public bool CanDetectBranch { get; set; }

	public bool Result { get; set; }


	public OptionsViewModel(AppSettings appSettings, AppData appData) {
		AutoSave = appData.FileSettings.AutoSave;
		GlobalHotkeys = appSettings.GlobalHotkeysEnabled;
		AutoBackup = appData.FileSettings.AutoBackup;
		BackupTime = appData.FileSettings.BackupTime;
		ShowWelcomeWindow = appSettings.ShowWelcomeWindow;
		GitRepoPath = appData.FileSettings.GitRepoPath;

		GitStatusColor = IsGitPathValid(GitRepoPath) ? BrushService.SuccessGreenBrush : BrushService.DangerRedBrush;
		UpdateGitFeaturesState();
	}
	public ICommand ChooseGitRepoPathCommand => new RelayCommand(ChooseGitRepoPath);
	private void ChooseGitRepoPath() {
		string folder = AppServices.DialogService.ChooseFolder(GitRepoPath, "Select the root folder of your Git repository (.git folder should be here)");
		if (folder != null) {
			string path = folder;
			path = Path.GetFullPath(path);

			var dir = new DirectoryInfo(path);
			while (dir != null) {
				if (Directory.Exists(Path.Combine(dir.FullName, ".git"))) {
					path = dir.FullName;
					break;
				}
				dir = dir.Parent;
			}
			if (IsGitPathValid(path)) {
				GitRepoPath = path;
				AppServices.DialogService.Show("Git repository path set successfully!", "Success", DialogButton.Ok, DialogIcon.Information);
			} else {
				AppServices.DialogService.Show("No .git folder found in selected directory.\nBranch detection will not work.", "Invalid Path", DialogButton.Ok, DialogIcon.Warning);
				GitRepoPath = null;
			}
		}

		UpdateGitFeaturesState();
	}
	private bool IsGitPathValid(string path) {
		if (string.IsNullOrWhiteSpace(path)) {
			Log.Warn($"Invalid git repo path: {path}");
			return false;
		}
		if (Directory.Exists(Path.Combine(path, ".git"))) {
			Log.Print($"Git repo found at: {path}");
			return true;
		}
		Log.Warn($"Invalid git repo path: {path}");
		return false;
	}

	private void UpdateGitFeaturesState() {
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