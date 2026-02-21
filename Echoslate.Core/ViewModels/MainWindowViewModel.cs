using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Echoslate.Core.Models;
using Echoslate.Core.Services;

namespace Echoslate.Core.ViewModels;

public enum PomoActiveState {
	Idle,
	Work,
	Break
}

public class MainWindowViewModel : INotifyPropertyChanged {
	private PeriodicTimer? _timer;

	public AppData Data;
	public AppSettings AppSettings { get; set; }

	private bool _isDebugMenuVisible;
	public bool IsDebugMenuVisible {
		get => _isDebugMenuVisible;
		set {
			if (_isDebugMenuVisible == value) {
				return;
			}
			_isDebugMenuVisible = value;
			OnPropertyChanged();
		}
	}

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
			OnPropertyChanged(nameof(PomoWorkTimeMinutes));
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
			OnPropertyChanged(nameof(PomoBreakTimeMinutes));
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

	public int PomoProgressBarValue { get; set; }
	public bool PomoIsWorkMode => PomoState == PomoActiveState.Work;


	public MainWindowViewModel(AppSettings appSettings) {
#if DEBUG
		IsDebugMenuVisible = true;
#endif

		AppSettings = appSettings;
		TodoListVM = new TodoListViewModel();
		KanbanVM = new KanbanViewModel();
		HistoryVM = new HistoryViewModel();
	}
	private void SetupApplicationState() {
		foreach (TodoItem item in MasterTodoItemsList) {
			item.UpdateTags(Data.AllTags);
		}

		SetWindowTitle();

		SetPomoTimers();
		UpdatePomoTimerUI();
		_backupTimerMax = new TimeSpan(0, Data.FileSettings.BackupTime, 0);
		_backupTimer = _backupTimerMax;

		StartTimer();
	}
	private void SetPomoTimers() {
		PomoWorkTime = AppSettings.PomoWorkTimerLength;
		PomoBreakTime = AppSettings.PomoBreakTimerLength;
	}
	private async void StartTimer() {
		if (_timer != null) {
			return;
		}
		_timer = new PeriodicTimer(TimeSpan.FromSeconds(1));

		while (await _timer.WaitForNextTickAsync()) {
			await AppServices.DispatcherService.InvokeAsync(() => {
				UpdateBackupTimer();
				UpdateTodoTimers();
				UpdatePomoTimer();
				UpdateAutoSaveTimer();
			});
		}
	}
	public void StopTimer() {
		_timer?.Dispose();
		_timer = null;
	}
	public void UpdateAutoSaveTimer() {
		if (!Data.FileSettings.AutoSave) {
			return;
		}
		AttemptAutoSave();
	}
	private void AttemptAutoSave() {
		if (!Data.FileSettings.AutoSave) {
			Log.Warn("AutoSave is not enabled");
			return;
		}
		if (IsPendingSave && (DateTime.Now - LastBackupAttempt).TotalSeconds >= DebounceSeconds) {
			_ = AutoSaveAsync();
			Log.Print("Autosaving");
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
	private void UpdatePomoTimerUI() {
		OnPropertyChanged(nameof(PomoLabelContent));
		OnPropertyChanged(nameof(PomoProgressBarValue));
		OnPropertyChanged(nameof(PomoIsWorkMode));
	}
	public void SetWindowTitle() {
		CurrentWindowTitle = "Echoslate v" + GetAppFileVersion() + " - " + Data?.FileName;
	}
	public static string GetAppFileVersion() {
		var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
		var fileVersionAttribute = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
		if (fileVersionAttribute != null && !string.IsNullOrWhiteSpace(fileVersionAttribute.Version)) {
			Log.Print($"FileVersion: {fileVersionAttribute.Version}");
			return fileVersionAttribute.Version;
		}

		// Fallback to AssemblyVersion if FileVersion attribute missing
		Log.Print($"FileVersion: {assembly.GetName().Version}");
		return assembly.GetName().Version?.ToString() ?? "Unknown";
	}
	public void LoadCurrentData() {
		Log.Print("Disabling AutoSave and AutoBackup");
		bool autoSave = Data.FileSettings.AutoSave;
		bool autoBackup = Data.FileSettings.AutoBackup;
		Data.FileSettings.AutoSave = false;
		Data.FileSettings.AutoBackup = false;
		
		Log.Print("Assigning primary lists");
		MasterTodoItemsList = Data.TodoList;
		MasterFilterTags = Data.FiltersList;
		MasterHistoryItemsList = Data.HistoryList;
		if (MasterHistoryItemsList.Count == 0) {
			MasterHistoryItemsList.Add(new HistoryItem());
		}
		Log.Print("Getting all TodoItemTags");
		foreach (TodoItem item in MasterTodoItemsList) {
			foreach (string tag in item.Tags) {
				if (!Data.AllTags.Contains(tag)) {
					Data.AllTags.Add(tag);
				}
			}
		}
		CurrentHistoryItem = MasterHistoryItemsList[0];

		Log.Print("Assigning CollectionChanged events...");
		MasterTodoItemsList.CollectionChanged += OnCollectionChanged;
		MasterHistoryItemsList.CollectionChanged += OnCollectionChanged;
		MasterFilterTags.CollectionChanged += OnCollectionChanged;

		Log.Print("Subscribing to PropertyChanged events for all items");
		SubscribeToExistingItems();

		Log.Print("Initializing ViewModels...");
		TodoListVM.Initialize(this);
		KanbanVM.Initialize(this);
		HistoryVM.Initialize(this);

		Log.Print("Setting window title...");
		SetWindowTitle();

		Log.Print("Setting up application state...");
		SetupApplicationState();

		Log.Print("Restoring AutoSave and AutoBackup settings");
		Data.FileSettings.AutoSave = autoSave;
		Data.FileSettings.AutoBackup = autoBackup;

		Log.Success("LoadCurrentData complete.");
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
	}
	private void SubscribeToExistingItems() {
		Log.Print("Subscribing to MasterTodoItems...");
		foreach (var item in MasterTodoItemsList) {
			SubscribeToItem(item);
		}
		Log.Print("Subscribing to HistoryList CompletedTodoItems..");
		foreach (var item in MasterHistoryItemsList) {
			SubscribeToItem(item);
			foreach (TodoItem todo in item.CompletedTodoItems) {
				SubscribeToItem(todo);
			}
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
		Log.Debug("In DEBUG MODE. Saving to temp directory...");
		filePath = $"C:\\MyBinaries\\TestData\\{Data.FileName}{Data.FileExtension}";
#endif
		AppDataSaver saver = new AppDataSaver();
		saver.Save(filePath, Data);
		AppSettings.AddRecentFile(Data.CurrentFilePath);
		SetWindowTitle();
		ClearChangedFlag();
	}
	private void Save() {
		if (Data == null || !File.Exists(Data.CurrentFilePath)) {
			return;
		}
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
	public async void Load(string? filePath) {
		Log.Print("Loading AppData...");
		Data = AppDataLoader.Load(filePath, Data);

		Log.Print("Disabling AutoSave while loading.");
		bool autoSave = Data.FileSettings.AutoSave;
		bool autoBackup = Data.FileSettings.AutoBackup;
		Data.FileSettings.AutoSave = false;
		Data.FileSettings.AutoBackup = false;

		Log.Print("Initializing Git settings...");
		GitHelper.InitGitSettings(Data);

		Log.Print("Loading project data...");
		LoadCurrentData();
		await Task.Yield();
		Log.Success("Project data loaded.");

		Log.Print($"Adding {filePath} to RecentFiles.");
		AppSettings.AddRecentFile(Data.CurrentFilePath);

		Log.Print("Setting window title...");
		SetWindowTitle();

		Log.Print("Resetting file changed flag...");
		ClearChangedFlag();

		Log.Print("Restoring AutoSave and AutoBackup settings.");
		Data.FileSettings.AutoSave = autoSave;
		Data.FileSettings.AutoBackup = autoBackup;

		Log.Print("Normalizing project data...");
		CleanAndNormalizeData();
	}
	public void CleanAndNormalizeData() {
		foreach (TodoItem item in Data.TodoList) {
			item.NormalizeData();
		}

		foreach (HistoryItem hItem in Data.HistoryList) {
			foreach (TodoItem item in hItem.CompletedTodoItems) {
				item.NormalizeData();
			}
		}
	}
	public async void CreateNewFile() {
		Log.Print("Creating new Data");
		Data = new AppData();

		Log.Print("Loading current data...");
		LoadCurrentData();

		Log.Print("Clearing item changed flags...");
		ClearChangedFlag();

		Log.Print("Choosing file to save as...");
		string saveFile = await AppServices.DialogService.SaveFile(Data.FileName, Data.BasePath);

		if (saveFile == null) {
			Log.Warn("File not saved. Shutting down...");
			AppServices.ApplicationService.Shutdown();
			return;
		}
		
		Log.Print("Setting current file path");
		Data.CurrentFilePath = saveFile;

		Log.Print($"Saving file: {saveFile}");
		Save(saveFile);
		SetupApplicationState();
	}
	public async Task<bool> OpenFile() {
		string basePath = Data == null ? "" : Data.BasePath;

		string loadFile = await AppServices.DialogService.OpenFile(basePath);
		if (loadFile == null) {
			return false;
		}
		if (_isChanged) {
			Save();
		}
		Load(loadFile);
		return true;
	}
	public async Task<bool> OpenFileAsync() {
		try {
			string? path = await AppServices.DialogService.OpenFile();
			if (string.IsNullOrEmpty(path)) {
				Log.Warn("No file selected.");
				return false;
			}
			Log.Print($"Selected path: {path}");
			if (_isChanged) {
				Log.Print("Saving existing file...");
				Save();
			}
			Log.Print("Loading file...");
			Load(path);
			return true;
		} catch (Exception ex) {
			Log.Error($"OpenFileAsync failed: {ex}");
			return false;
		}
	}


	public ICommand MenuNewCommand => new RelayCommand(CreateNewFile);
	public ICommand MenuLoadCommand => new RelayCommand(() => OpenFile());
	public ICommand SaveCommand => new RelayCommand(Save);
	public ICommand MenuSaveCommand => new RelayCommand(Save);
	public ICommand MenuSaveAsCommand => new RelayCommand(SaveAs);
	public async void SaveAs() {
		Log.Print("Saving file as...");

		string saveFile = await AppServices.DialogService.SaveFile(Data.FileName, Data.BasePath);
		if (saveFile == null) {
			Log.Warn("File not saved. Continuing...");
			return;
		}
		Data.CurrentFilePath = saveFile;
		Save(saveFile);
	}
	public ICommand MenuOptionsCommand => new RelayCommand(MenuOptions);
	private async void MenuOptions() {
		Task<OptionsViewModel?> vmTask = AppServices.DialogService.ShowOptionsAsync(AppSettings, Data);
		OptionsViewModel? vm = await vmTask;
		if (vm == null) {
			return;
		}
		if (vm.Result) {
#if DEBUG
			Data.FileSettings.AutoSave = false;
			Data.FileSettings.AutoBackup = false;
#else
			Data.FileSettings.AutoSave = vm.AutoSave;
			Data.FileSettings.AutoBackup = vm.AutoBackup;
#endif
			// AppDataSettings.GlobalHotkeysEnabled = options.GlobalHotkeys;
			Data.FileSettings.AutoIncrement = vm.AutoIncrement;
			AppSettings.ShowWelcomeWindow = vm.ShowWelcomeWindow;
			AppSettings.BackupTime = new TimeSpan(0, vm.BackupTime, 0);
			Data.FileSettings.CanDetectBranch = vm.CanDetectBranch;
			Data.FileSettings.GitRepoPath = vm.GitRepoPath;
			
			Save();
		}
	}
	public ICommand MenuQuitCommand => new RelayCommand(MenuQuit);
	private void MenuQuit() {
		Log.Print("Saving settings...");
		Log.Print("Settings saved.");

		if (_isChanged) {
			Save(Data.CurrentFilePath);

			Log.Print("Shutting down...");
			Log.Shutdown();
		} else {
			Log.Print("Shutting down...");
			Log.Shutdown();
		}
		AppServices.ApplicationService.Shutdown();
	}
	public ICommand MenuHelpCommand => new RelayCommand(MenuHelp);
	private void MenuHelp() {
		AppServices.DialogService.ShowHelpAsync();
	}
	public ICommand MenuRecentFilesLoadCommand => new RelayCommand<string>(path => Load(path));
	public ICommand MenuRecentFilesRemoveCommand => new RelayCommand<string>(path => RemoveRecentFile(path));
	public void RemoveRecentFile(string path) {
		AppSettings.RecentFiles.Remove(path);
	}
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
	public ICommand ShowAboutWindowCommand => new RelayCommand(() => { AppServices.DialogService.ShowAboutAsync(); });
	public ICommand ShowHotkeysWindowCommand => new RelayCommand(() => { AppServices.DialogService.ShowHelpAsync(); });
	public ICommand QuickLoadPreviousCommand => new RelayCommand(QuickLoad);
	public void QuickLoad() {
		if (AppSettings.RecentFiles.Count > 1) {
			Load(AppSettings.RecentFiles[1]);
		}
	}

	public ICommand DebugStepIntoCommand => new RelayCommand(DebugStepInto);
	public void DebugStepInto() {
		var data = Data;
	}

	public void OnClosing(object? sender, CancelEventArgs e) {
		if (Data != null && Data.HistoryList != null) {
			foreach (HistoryItem item in Data.HistoryList) {
				if (item == Data.CurrentHistoryItem) {
					continue;
				}
				item.IsCommitted = true;
			}
		}
		StopTimer();
		Log.Print($"All history items committed.");
		Save();
	}


	public event PropertyChangedEventHandler? PropertyChanged;
	private void OnPropertyChanged([CallerMemberName] string? name = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}