using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.Input;
using Echoslate.Windows;
using Application = System.Windows.Application;

namespace Echoslate.ViewModels {
	public enum PomoActiveState {
		Idle,
		Work,
		Break
	}

	public class MainWindowViewModel : INotifyPropertyChanged {
		public AppData Data;
		public AppSettings AppSettings { get; set; }

		private string _currentWindowTitle;
		public string CurrentWindowTitle {
			get => _currentWindowTitle;
			set {
				_currentWindowTitle = value;
				OnPropertyChanged();
			}
		}

		public TodoListViewModel TodoListVM { get; }
		public KanbanViewModel KanbanVM { get; }
		public HistoryViewModel HistoryVM { get; }

		private ObservableCollection<TodoItem> _masterTodoItemsList;
		public ObservableCollection<TodoItem> MasterTodoItemsList {
			get => _masterTodoItemsList;
			set {
				_masterTodoItemsList = value;
				OnPropertyChanged();
			}
		}

		private ObservableCollection<HistoryItem> _masterHistoryItemsList;
		public ObservableCollection<HistoryItem> MasterHistoryItemsList {
			get => _masterHistoryItemsList;
			set {
				_masterHistoryItemsList = value;
				OnPropertyChanged();
			}
		}
		private HistoryItem _currentHistoryItem;
		public HistoryItem CurrentHistoryItem {
			get => _currentHistoryItem;
			set {
				_currentHistoryItem = value;
				OnPropertyChanged();
			}
		}

		private ObservableCollection<string> _masterFilterTags;
		public ObservableCollection<string> MasterFilterTags {
			get => _masterFilterTags;
			set {
				_masterFilterTags = value;
				OnPropertyChanged();
			}
		}

		private bool _isChanged;
		public bool IsChanged {
			get => _isChanged;
			set {
				if (_isChanged != value) {
					_isChanged = value;
					if (value) {
						IsPendingSave = true;
						AttemptAutoSave();
						LastBackupAttempt = DateTime.Now;
					}
					OnPropertyChanged();
				}
			}
		}
		public bool IsPendingSave { get; set; }
		public DateTime LastBackupAttempt { get; set; }
		private const double DebounceSeconds = 1.5;

		private TimeSpan _backupTimer;
		private TimeSpan _backupTimerMax;

		private TimeSpan _pomoTimer;
		public TimeSpan PomoTimer {
			get => _pomoTimer;
			set {
				_pomoTimer = value;
				OnPropertyChanged();
				UpdatePomoTimerUI();
			}
		}
		private bool _isPomoTimerOn;
		private TimeSpan _pomoWorkTime;
		public TimeSpan PomoWorkTime {
			get => _pomoWorkTime;
			set {
				_pomoWorkTime = value;
				AppSettings.PomoWorkTimerLength = value;
				OnPropertyChanged();
			}
		}
		public int PomoWorkTimeMinutes {
			get => _pomoWorkTime.Minutes;
			set {
				PomoWorkTime = new TimeSpan(0, value, 0);
				OnPropertyChanged();
			}
		}
		private TimeSpan _pomoTimeLeft;
		public TimeSpan PomoTimeLeft {
			get => _pomoTimeLeft;
			set {
				_pomoTimeLeft = value;
				OnPropertyChanged();
				UpdatePomoTimerUI();
			}
		}
		private TimeSpan _pomoBreakTime;
		public TimeSpan PomoBreakTime {
			get => _pomoBreakTime;
			set {
				_pomoBreakTime = value;
				AppSettings.PomoBreakTimerLength = value;
				OnPropertyChanged();
			}
		}
		public int PomoBreakTimeMinutes {
			get => _pomoBreakTime.Minutes;
			set {
				PomoBreakTime = new TimeSpan(0, value, 0);
				OnPropertyChanged();
			}
		}
		public string PomoLabelContent => $"{PomoTimeLeft.Minutes:00}:{PomoTimeLeft.Seconds:00}";

		private PomoActiveState _pomoState;
		public PomoActiveState PomoState {
			get => _pomoState;
			set {
				_pomoState = value;
				OnPropertyChanged();
				UpdatePomoTimerUI();
			}
		}
		private PomoActiveState _pomoLastActiveState;

		public Brush PomoBackground {
			get => _pomoState switch {
				PomoActiveState.Idle => Brushes.Transparent,
				PomoActiveState.Work => Brushes.Maroon,
				PomoActiveState.Break => Brushes.LimeGreen,
				_ => Brushes.Transparent
			};
		}
		public int PomoProgressBarValue { get; set; }
		public bool PomoIsWorkMode => PomoState == PomoActiveState.Work;

		private void UpdatePomoTimerUI() {
			OnPropertyChanged(nameof(PomoLabelContent));
			OnPropertyChanged(nameof(PomoBackground));
			OnPropertyChanged(nameof(PomoProgressBarValue));
			OnPropertyChanged(nameof(PomoIsWorkMode));
		}


		public MainWindowViewModel(AppSettings appSettings) {
			AppSettings = appSettings;
			TodoListVM = new TodoListViewModel();
			KanbanVM = new KanbanViewModel();
			HistoryVM = new HistoryViewModel();

			// if (appDataSettings.RecentFiles.Count != 0) {
			// 	LoadRecentFile(appDataSettings);
			// 	LoadCurrentData();
			// } else {
			// 	CreateNewFile();
			// }
		}
		private void SetupApplicationState() {
			foreach (TodoItem item in MasterTodoItemsList) {
				item.UpdateTags(Data.AllTags);
			}

			SetWindowTitle();
			SetPomoTimers();

			var timer = new DispatcherTimer();
			timer.Tick += Timer_Tick;
			timer.Interval = new TimeSpan(TimeSpan.TicksPerSecond);
			timer.Start();

			_backupTimerMax = new TimeSpan(0, Data.FileSettings.BackupTime, 0);
			// _backupTimerMax = new TimeSpan(0, 0, 10);
			_backupTimer = _backupTimerMax;
			Log.Print($"{_backupTimer}");
		}

		private void SetPomoTimers() {
			PomoWorkTime = AppSettings.PomoWorkTimerLength;
			PomoBreakTime = AppSettings.PomoBreakTimerLength;
		}
		public void Timer_Tick(object? sender, EventArgs e) {
			UpdateBackupTimer();
			UpdateTodoTimers();
			UpdatePomoTimer();
			UpdateAutoSaveTimer();
		}
		public void UpdateAutoSaveTimer() {
			if (!Data.FileSettings.AutoSave) {
				return;
			}
			AttemptAutoSave();
		}
		private void AttemptAutoSave() {
			if (IsPendingSave && (DateTime.Now - LastBackupAttempt).TotalSeconds >= DebounceSeconds) {
				_ = AutoSaveAsync();
				Log.Test("Autosaving");
				IsPendingSave = false;
			}
		}
		public void UpdateBackupTimer() {
			_backupTimer = _backupTimer.Subtract(new TimeSpan(0, 0, 1));
			if (_backupTimer <= TimeSpan.Zero) {
				_backupTimer = _backupTimerMax;
				BackupSave();
			}
		}
		public void UpdateTodoTimers() {
			foreach (TodoItem item in MasterTodoItemsList) {
				if (item.IsTimerOn) {
					item.TimeTaken = item.TimeTaken.Add(TimeSpan.FromSeconds(1));
				}
			}
		}
		public void UpdatePomoTimer() {
			if (_isPomoTimerOn) {
				PomoTimer = PomoTimer.Add(new TimeSpan(0, 0, 1));
				if (PomoState == PomoActiveState.Work) {
					double timerTicks = PomoTimer.Ticks;
					double workTicks = PomoWorkTime.Ticks;
					PomoProgressBarValue = 100 - (int)(100 * (timerTicks / workTicks));
					PomoTimeLeft = PomoWorkTime - PomoTimer;
					if (PomoTimer >= PomoWorkTime) {
						PomoState = PomoActiveState.Break;
						PomoTimer = new TimeSpan(0, 0, 0);
					}
				} else if (PomoState == PomoActiveState.Break) {
					double timerTicks = PomoTimer.Ticks;
					double breakTicks = PomoBreakTime.Ticks;
					PomoProgressBarValue = 100 - (int)(100 * (timerTicks / breakTicks));
					PomoTimeLeft = PomoBreakTime - PomoTimer;
					if (PomoTimer >= PomoBreakTime) {
						PomoState = PomoActiveState.Work;
						PomoTimer = new TimeSpan(0, 0, 0);
					}
				}
				_pomoLastActiveState = _pomoState;
			}
		}
		public void SetWindowTitle() {
			CurrentWindowTitle = AppSettings.WindowTitle + " - " + Data?.FileName;
		}
		public void RebuildAllViews() {
			TodoListVM.RebuildView();
			KanbanVM.RebuildView();
			HistoryVM.RebuildView();
		}
		public void LoadCurrentData() {
			MasterTodoItemsList = Data.TodoList;
			MasterFilterTags = Data.FiltersList;
			MasterHistoryItemsList = Data.HistoryList;
			if (MasterHistoryItemsList.Count == 0) {
				MasterHistoryItemsList.Add(new HistoryItem());
			}
			foreach (TodoItem item in MasterTodoItemsList) {
				foreach (string tag in item.Tags) {
					if (!Data.AllTags.Contains(tag)) {
						Data.AllTags.Add(tag);
					}
				}
			}
			CurrentHistoryItem = MasterHistoryItemsList[0];

			MasterTodoItemsList.CollectionChanged += OnCollectionChanged;
			MasterHistoryItemsList.CollectionChanged += OnCollectionChanged;
			MasterFilterTags.CollectionChanged += OnCollectionChanged;
			SubscribeToExistingItems();

			TodoListVM.Initialize(this);
			KanbanVM.Initialize(this);
			HistoryVM.Initialize(this);
			RebuildAllViews();
			SetWindowTitle();

			// TODO Remove this if all is well
			// foreach (TodoItem item in MasterTodoItemsList) {
			// item.UpdateDates();
			// }
			SetupApplicationState();
		}
		private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
			if (e.NewItems != null) {
				foreach (var item in e.NewItems) {
					SubscribeToItem(item);
				}
			}

			if (e.OldItems != null) {
				foreach (var item in e.OldItems) {
					UnsubscribeFromItem(item);
				}
			}
			MarkAsChanged();
			RebuildAllViews();
		}
		private void SubscribeToExistingItems() {
			foreach (var item in MasterTodoItemsList) {
				SubscribeToItem(item);
			}
			foreach (var item in MasterHistoryItemsList) {
				SubscribeToItem(item);
			}
		}
		private void SubscribeToItem(object item) {
			if (item is INotifyPropertyChanged notifyItem) {
				notifyItem.PropertyChanged += OnItemPropertyChanged;
			}
		}
		private void UnsubscribeFromItem(object item) {
			if (item is INotifyPropertyChanged notifyItem) {
				notifyItem.PropertyChanged -= OnItemPropertyChanged;
			}
		}
		private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e) {
			MarkAsChanged();
		}
		private void MarkAsChanged() {
			if (!IsChanged) {
				IsChanged = true;
			}
		}
		private void ClearChangedFlag() {
			IsChanged = false;
		}
		public void LoadRecentFile(AppSettings? settings) {
			while (true) {
				if (settings == null || AppSettings.RecentFiles.Count == 0) {
					return;
				}

				if (File.Exists(AppSettings.RecentFiles[0])) {
					Log.Print($"Loading recent file {AppSettings.RecentFiles[0]}");
					Data = AppDataLoader.Load(AppSettings.RecentFiles[0]);
					AppSettings.SortRecentFiles(AppSettings.RecentFiles[0]);
					return;
				}

				Log.Error($"{AppSettings.RecentFiles[0]} does not exist.");
				AppSettings.RecentFiles.RemoveAt(0);
			}
		}
		private async Task AutoSaveAsync() {
			try {
				await SaveToMainFileAsync();
				ClearChangedFlag();
			} catch (Exception ex) {
				Log.Error($"Auto save failed: {ex.Message}");
			}
		}
		private async Task SaveToMainFileAsync() {
#if DEBUG
			string filePath = $"C:\\MyBinaries\\TestData\\{Data.FileName}{Data.FileExtension}";
#else
			string filePath = AppSettings.RecentFiles[0];
#endif
			AppDataSaver saver = new AppDataSaver();
			saver.Save(filePath, Data);
		}
		private void Save(string filePath) {
#if DEBUG
			filePath = $"C:\\MyBinaries\\TestData\\{Data.FileName}{Data.FileExtension}";
#endif
			AppDataSaver saver = new AppDataSaver();
			saver.Save(filePath, Data);
			AppSettings.AddRecentFile(Data.CurrentFilePath);
			SetWindowTitle();
			ClearChangedFlag();
		}
		private void Save() {
			Save(Data.CurrentFilePath);
		}
		private void BackupSave() {
			if (!Data.FileSettings.AutoBackup) {
				return;
			}

#if DEBUG
			string path = "C:\\MyBinaries\\TestData\\" + Data.FileName + ".bak" + Data.FileSettings.BackupIncrement;
#else
			string path = AppSettings.RecentFiles[0] + ".bak" + Data.FileSettings.BackupIncrement;
#endif
			Log.Print($"Backing up to: {path}");
			AppDataSaver saver = new AppDataSaver();
			saver.Save(path, Data);

			Data.FileSettings.BackupIncrement++;
			Data.FileSettings.BackupIncrement %= 10;
		}
		public void Load(string? filePath) {
			Data = AppDataLoader.Load(filePath);
			AppSettings.SortRecentFiles(filePath);
			LoadCurrentData();
			AppSettings.AddRecentFile(Data.CurrentFilePath);
			SetWindowTitle();
			ClearChangedFlag();
		}
		public void CreateNewFile() {
			Data = new AppData();

			LoadCurrentData();
			RebuildAllViews();
			ClearChangedFlag();


			SaveFileDialog sfd = new SaveFileDialog {
				Title = @"Select folder to save file in.",
				FileName = Data.FileName,
				InitialDirectory = Data.BasePath,
				Filter = @"Echoslate files (*.echoslate)|*.echoslate|All files (*.*)|*.*"
			};
			DialogResult dr = sfd.ShowDialog();
			if (dr != System.Windows.Forms.DialogResult.OK) {
				Log.Warn("File not saved. Continuing...");
				Application.Current.Shutdown();
				return;
			}
			Application.Current.MainWindow?.Activate();

			Data.CurrentFilePath = sfd.FileName;
			Save(sfd.FileName);

			Window mainWindow = Application.Current.MainWindow;
			if (mainWindow != null) {
				mainWindow.Activate();
				if (mainWindow.WindowState == WindowState.Minimized)
					mainWindow.WindowState = WindowState.Normal;

				mainWindow.Topmost = true;
				mainWindow.Topmost = false;
				mainWindow.Focus();
			}
			SetupApplicationState();
		}
		private void MenuNew() {
			CreateNewFile();
		}
		public bool OpenFile() {
			string basePath = Data == null ? "" : Data.BasePath;
			OpenFileDialog openFileDialog = new OpenFileDialog {
				Title = @"Open file: ",
				InitialDirectory = basePath,
				Filter = @"Echoslate files (*.echoslate)|*.echoslate|All files (*.*)|*.*"
			};

			DialogResult dr = openFileDialog.ShowDialog();

			if (dr != System.Windows.Forms.DialogResult.OK) {
				return false;
			}

			if (_isChanged) {
				Save();
			}
			Load(openFileDialog.FileName);
			return true;
		}
		private void MenuLoad() {
			OpenFile();
		}
		private void MenuSave() {
			Save();
		}
		public void SaveAs() {
			Log.Print("Saving file as...");
			SaveFileDialog sfd = new SaveFileDialog {
				Title = @"Select folder to save file in.",
				FileName = Data.FileName,
				InitialDirectory = Data.BasePath,
				Filter = @"Echoslate files (*.echoslate)|*.echoslate|All files (*.*)|*.*"
			};

			DialogResult dr = sfd.ShowDialog();
			if (dr != System.Windows.Forms.DialogResult.OK) {
				Log.Warn("File not saved. Continuing...");
				return;
			}

			Application.Current.MainWindow?.Activate();

			Data.CurrentFilePath = sfd.FileName;
			Save(sfd.FileName);
			Window mainWindow = Application.Current.MainWindow;
			if (mainWindow != null) {
				mainWindow.Activate();
				if (mainWindow.WindowState == WindowState.Minimized)
					mainWindow.WindowState = WindowState.Normal;

				mainWindow.Topmost = true;
				mainWindow.Focus();
				mainWindow.Topmost = false;
			}
		}
		private void MenuSaveAs() {
			SaveAs();
		}
		private void MenuOptions() {
			bool autoSave = Data.FileSettings.AutoSave;
			bool globalHotkeys = AppSettings.GlobalHotkeysEnabled;
			bool autoBackup = Data.FileSettings.AutoBackup;
			int backupTime = AppSettings.BackupTime.Minutes;
			bool welcomeWindow = AppSettings.SkipWelcome;
			DlgOptions options = new DlgOptions(autoSave, globalHotkeys, autoBackup, backupTime, welcomeWindow);
			options.ShowDialog();
			if (options.Result) {
#if DEBUG
				Data.FileSettings.AutoSave = false;
				Data.FileSettings.AutoBackup = false;
#else
				Data.FileSettings.AutoSave = options.AutoSave;
				Data.FileSettings.AutoBackup = options.AutoBackup;
#endif
				// AppDataSettings.GlobalHotkeysEnabled = options.GlobalHotkeys;
				AppSettings.SkipWelcome = !options.WelcomeWindow;
				AppSettings.BackupTime = new TimeSpan(0, options.BackupTime, 0);
			}
		}
		private void MenuQuit() {
			Log.Print("Saving settings...");
			// SaveSettings();
			Log.Print("Settings saved.");

			if (_isChanged) {
				Save(Data.CurrentFilePath);

				Log.Print("Shutting down...");
				Log.Shutdown();
			} else {
				Log.Print("Shutting down...");
				Log.Shutdown();
			}
			Application.Current.Shutdown();
		}
		private void MenuHelp() {
			DlgHelp dlgH = new DlgHelp();
			dlgH.ShowDialog();
		}

		private void MenuRecentFilesLoad(string? filePath) {
			Load(filePath);
		}
		private void MenuRecentFilesRemove(string? filePath) {
			AppSettings.RecentFiles.Remove(filePath);
		}

		public ICommand MenuNewCommand => new RelayCommand(MenuNew);
		public ICommand MenuLoadCommand => new RelayCommand(MenuLoad);
		public ICommand MenuSaveCommand => new RelayCommand(MenuSave);
		public ICommand MenuSaveAsCommand => new RelayCommand(MenuSaveAs);
		public ICommand MenuOptionsCommand => new RelayCommand(MenuOptions);
		public ICommand MenuQuitCommand => new RelayCommand(MenuQuit);
		public ICommand MenuHelpCommand => new RelayCommand(MenuHelp);

		public ICommand MenuRecentFilesLoadCommand => new RelayCommand<string>(MenuRecentFilesLoad);
		public ICommand MenuRecentFilesRemoveCommand => new RelayCommand<string>(MenuRecentFilesRemove);

		public ICommand PomoTimerToggleCommand => new RelayCommand(PomoTimerToggle);
		public void PomoTimerToggle() {
			_isPomoTimerOn = !_isPomoTimerOn;
			if (_isPomoTimerOn) {
				if (_pomoLastActiveState == PomoActiveState.Idle) {
					PomoProgressBarValue = 0;
					PomoState = PomoActiveState.Work;
				} else {
					PomoState = _pomoLastActiveState;
				}
			} else {
				PomoState = PomoActiveState.Idle;
			}
		}
		public ICommand PomoTimerResetCommand => new RelayCommand(() => {
			_isPomoTimerOn = false;
			_pomoLastActiveState = PomoActiveState.Idle;
			PomoProgressBarValue = 0;
			PomoState = PomoActiveState.Idle;
			PomoTimer = TimeSpan.Zero;
			PomoTimeLeft = TimeSpan.Zero;
		});
		public ICommand ShowAboutWindowCommand => new RelayCommand(() => {
			var window = Application.Current.MainWindow;
			var aboutWindow = new AboutWindow() {
				Owner = window
			};
			aboutWindow.ShowDialog();
		});
		public ICommand ShowHotkeysWindowCommand => new RelayCommand(() => {
			var window = Application.Current.MainWindow;
			var hotkeys = new DlgHelp() {
				Owner = window
			};
			hotkeys.ShowDialog();
		});

		public ICommand QuickSaveCommand => new RelayCommand(MenuSave);
		public ICommand QuickLoadPreviousCommand => new RelayCommand(QuickLoad);
		public void QuickLoad() {
			if (AppSettings.RecentFiles.Count > 1) {
				MenuRecentFilesLoad(AppSettings.RecentFiles[1]);
			}
		}
		public ICommand StartStopTimerCommand => new RelayCommand(PomoTimerToggle);


		public event PropertyChangedEventHandler? PropertyChanged;
		private void OnPropertyChanged([CallerMemberName] string? name = null)
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}
}