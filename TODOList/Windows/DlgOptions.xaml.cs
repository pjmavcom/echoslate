using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;


namespace Echoslate {
	public partial class DlgOptions : INotifyPropertyChanged {
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
		private bool _welcomeWindow;
		public bool WelcomeWindow {
			get => _welcomeWindow;
			set {
				_welcomeWindow = value;
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

		public bool Result;

		public DlgOptions(AppSettings appSettings, AppData appData) {
			InitializeComponent();
			DataContext = this;
			AutoSave = appData.FileSettings.AutoSave;
			GlobalHotkeys = appSettings.GlobalHotkeysEnabled;
			AutoBackup = appData.FileSettings.AutoBackup;
			BackupTime = appData.FileSettings.BackupTime;
			WelcomeWindow = !appSettings.SkipWelcome;
			GitRepoPath = appData.FileSettings.GitRepoPath;

			CenterWindowOnMouse();
		}
		private void CenterWindowOnMouse() {
			Window win = Application.Current.MainWindow;

			if (win == null)
				return;
			double centerX = win.Width / 2 + win.Left;
			double centerY = win.Height / 2 + win.Top;
			Left = centerX - Width / 2;
			Top = centerY - Height / 2;
		}
		public ICommand OkCommand => new RelayCommand(Ok);
		private void Ok() {
			Result = true;
			Close();
		}
		public ICommand CancelCommand => new RelayCommand(Cancel);
		private void Cancel() {
			Result = false;
			Close();
		}
		public ICommand ChooseGitRepoPathCommand => new RelayCommand(ChooseGitRepoPath);
		private void ChooseGitRepoPath() {
			var dialog = new FolderBrowserDialog(); // or CommonOpenFileDialog for modern look
			dialog.Description = "Select the root folder of your Git repository (.git folder should be here)";

			if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
				string path = dialog.SelectedPath;
				path = Path.GetFullPath(path);

				var dir = new DirectoryInfo(path);
				while (dir != null) {
					if (Directory.Exists(Path.Combine(dir.FullName, ".git"))) {
						path = dir.FullName;
						break;
					}
					dir = dir.Parent;
				}
				if (Directory.Exists(Path.Combine(path, ".git"))) {
					GitRepoPath = path;
					MessageBox.Show("Git repository path set successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
				} else {
					MessageBox.Show("No .git folder found in selected directory.\nBranch detection will not work.", "Invalid Path", MessageBoxButton.OK, MessageBoxImage.Warning);
					GitRepoPath = null;
				}
			}

			UpdateGitFeaturesState();
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
}