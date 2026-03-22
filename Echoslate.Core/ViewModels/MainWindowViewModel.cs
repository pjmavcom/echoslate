using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Echoslate.Core.Models;
using Echoslate.Core.Services;
using WindowState = Echoslate.Core.Services.WindowState;

namespace Echoslate.Core.ViewModels;

public enum PomoActiveState {
	Idle,
	Work,
	Break
}

public class MainWindowViewModel : INotifyPropertyChanged {
	private string _statusBarText;
	public string StatusBarText {
		get => _statusBarText;
		set {
			if (_statusBarText == value) {
				return;
			}
			_statusBarText = value;
			OnPropertyChanged();
		}
	}

	public string ReminderStatusBarTextCount {
		get {
			if (MasterReminders == null) {
				return string.Empty;
			}
			int numReminders = MasterReminders.Count;
			return $"{numReminders} Reminder" + (numReminders > 1 ? "s" : "") + " active";
		}
	}
	public DateTime NextDue;
	public string ReminderStatusBarTextDueDate {
		get {
			if (MasterReminders == null) {
				return string.Empty;
			}
			NextDue = DateTime.MaxValue;
			foreach (ReminderInfo ri in MasterReminders) {
				if (ri.DueDate < NextDue) {
					NextDue = ri.DueDate;
				}
			}
			return $"Next Due: {NextDue}";
		}
	}
	public ReminderDueIn ReminderStatusBarTextColorFlag {
		get {
			TimeSpan minutes = NextDue - DateTime.Now;
			if (minutes < new TimeSpan(0, 15, 0)) {
				return ReminderDueIn.FifteenMinutes;
			} else if (minutes < new TimeSpan(0, 30, 0)) {
				return ReminderDueIn.ThirtyMinutes;
			} else if (minutes < new TimeSpan(1, 0, 0)) {
				return ReminderDueIn.OneHour;
			} else if (minutes < new TimeSpan(4, 0, 0)) {
				return ReminderDueIn.FourHours;
			}
			return ReminderDueIn.MoreThanFourHours;
		}
	}
	public object ReminderStatusBarTextColor => AppServices.BrushService.GetBrushForDueDates(ReminderStatusBarTextColorFlag);

	public ObservableCollection<ReminderInfo> MasterReminders;
	private PeriodicTimer? _timer;
	private int _reminderTimerTicks;
	private int _reminderTimerTicksMax = 5;
	private bool _alarmWindowOpen = false;
	private bool _showAlarmsWindow = false;

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
					IsPendingBackup = true;
				}
				OnPropertyChanged();
			}
		}
	}
	private bool _isPendingBackup;
	public bool IsPendingBackup {
		get => _isPendingBackup;
		set {
			if (!Data.FileSettings.AutoBackup) {
				return;
			}
			if (_isPendingBackup == value) {
				return;
			}
			_isPendingBackup = value;
			OnPropertyChanged();
		}
	}
	private bool _isPendingSave;
	public bool IsPendingSave {
		get => _isPendingSave;
		set {
			if (!Data.FileSettings.AutoSave) {
				return;
			}
			if (_isPendingSave == value) {
				return;
			}
			_isPendingSave = value;
			OnPropertyChanged();
		}
	}
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
	private int _pomoWorkTimeMinutes;
	public int PomoWorkTimeMinutes {
		get {
			if (_pomoWorkTimeMinutes == 0) {
				return (PomoWorkTime.Hours * 60 + PomoWorkTime.Minutes);
			} else {
				return _pomoWorkTimeMinutes;
			}
		}
		set {
			_pomoWorkTimeMinutes = value;
			int hours = _pomoWorkTimeMinutes / 60;
			int minutes = _pomoWorkTimeMinutes % 60;
			PomoWorkTime = new TimeSpan(hours, minutes, 0);
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
	private int _pomoBreakTimeMinutes;
	public int PomoBreakTimeMinutes {
		get {
			if (_pomoBreakTimeMinutes == 0) {
				return (PomoBreakTime.Hours * 60 + PomoBreakTime.Minutes);
			} else {
				return _pomoBreakTimeMinutes;
			}
		}
		set {
			_pomoBreakTimeMinutes = value;
			int hours = _pomoBreakTimeMinutes / 60;
			int minutes = _pomoBreakTimeMinutes % 60;
			PomoBreakTime = new TimeSpan(hours, minutes, 0);
			OnPropertyChanged();
		}
	}
	public string PomoLabelContent => $"{PomoTimeLeft.Hours:00}:{PomoTimeLeft.Minutes:00}:{PomoTimeLeft.Seconds:00}";

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
	private bool _arePomoControlsVisible;
	public bool ArePomoControlsVisible {
		get => _arePomoControlsVisible;
		set {
			if (_arePomoControlsVisible == value) {
				return;
			}
			_arePomoControlsVisible = value;
			OnPropertyChanged();
		}
	}

	public MainWindowViewModel(AppSettings appSettings) {
#if DEBUG
		IsDebugMenuVisible = true;
#endif

		AppSettings = appSettings;
		TodoListVM = new TodoListViewModel();
		KanbanVM = new KanbanViewModel();
		HistoryVM = new HistoryViewModel();
		StatusBarText = "Welcome to Echoslate!";
	}
	private void SetupApplicationState() {
		foreach (TodoItem item in MasterTodoItemsList) {
			item.UpdateTags(Data.AllTags);
		}
		LinkRemindersToTodos();
		OnPropertyChanged(nameof(ReminderStatusBarTextCount));
		OnPropertyChanged(nameof(ReminderStatusBarTextDueDate));

		SetWindowTitle();

		SetPomoTimers();
		UpdatePomoTimerUI();
		_backupTimerMax = new TimeSpan(0, Data.FileSettings.BackupTime, 0);
		_backupTimer = _backupTimerMax;

		StartTimer();
	}
	public void LinkRemindersToTodos() {
		foreach (ReminderInfo ri in MasterReminders) {
			if (ri.TodoGuids.Count == 0) {
				continue;
			}
			foreach (Guid guid in ri.TodoGuids) {
				TodoItem? item = FindTodoItemByGuid(guid);
				if (item == null) {
					continue;
				}
				ri.Todos.Add(item);
				item.AddReminder(ri);
			}
		}
	}

	public TodoItem? FindTodoItemByGuid(Guid guid) {
		foreach (TodoItem item in MasterTodoItemsList) {
			if (item.SearchByGuid(guid)) {
				return item;
			}
		}
		return null;
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
				UpdateReminderTimer();
			});
		}
	}

	public async void UpdateReminderTimer() {
		if (!_alarmWindowOpen) {
			_reminderTimerTicks++;
			if (_reminderTimerTicks >= _reminderTimerTicksMax) {
				ObservableCollection<ReminderInfo> dueItems = [];
				_showAlarmsWindow = false;
				foreach (ReminderInfo reminder in MasterReminders) {
					if (reminder.IsDueNow) {
						dueItems.Add(reminder.Copy());
						_showAlarmsWindow = true;
					}
				}
				bool isactive = AppServices.ApplicationService.IsActive();
				if (isactive && _showAlarmsWindow) {
					_alarmWindowOpen = true;
					Task<AlarmPopupViewModel?> vmTask = AppServices.DialogService.ShowAlarmPopupAsync(dueItems);
					AlarmPopupViewModel vm = await vmTask;
					if (vm != null) {
						UpdateRemindersFromDialog(dueItems, vm.Reminders);
					}
					_alarmWindowOpen = false;
				}
				_reminderTimerTicks = 0;
				OnPropertyChanged(nameof(ReminderStatusBarTextCount));
				OnPropertyChanged(nameof(ReminderStatusBarTextDueDate));
				OnPropertyChanged(nameof(ReminderStatusBarTextColor));
			}
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
		if (!IsPendingSave) {
			return;
		}
		AttemptAutoSave();
	}

	private void AttemptAutoSave() {
		if ((DateTime.Now - LastBackupAttempt).TotalSeconds >= DebounceSeconds) {
			_ = AutoSaveAsync();
			Log.Print("Autosaving");
			IsPendingSave = false;
		}
	}

	public void UpdateBackupTimer() {
		_backupTimer = _backupTimer.Subtract(new TimeSpan(0, 0, 1));
		if (_backupTimer <= TimeSpan.Zero) {
			_backupTimer = _backupTimerMax;
			if (!IsPendingBackup) {
				Log.Print("No data has changed. Backup canceled.");
				return;
			}
			BackupSave();
			IsPendingBackup = false;
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

	private void UpdateRemindersFromDialog(ObservableCollection<ReminderInfo>? dueItems, ObservableCollection<ReminderInfo> reminders) {
		if (dueItems != null) {
			HashSet<Guid> guids = dueItems.Select(item => item.Guid).Distinct().ToHashSet();
			foreach (Guid guid in guids) {
				if (MasterReminders.Any(item => item.Guid == guid)) {
					MasterReminders.Remove(MasterReminders.First(item => item.Guid == guid));
				}
			}
		} else {
			MasterReminders.Clear();
		}
		foreach (ReminderInfo reminder in reminders) {
			if (reminder.IsActive) {
				MasterReminders.Add(reminder);
				OnPropertyChanged(nameof(ReminderStatusBarTextCount));
				OnPropertyChanged(nameof(ReminderStatusBarTextDueDate));
			}
		}
		foreach (ReminderInfo ri in MasterReminders) {
			foreach (TodoItem item in ri.Todos) {
				item.UpdateReminder();
			}
		}
	}

	public void SetWindowTitle() {
		CurrentWindowTitle = "Echoslate v" + AppServices.ApplicationService.GetVersion() + " - " + Data?.FileName;
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
		MasterReminders = Data.Reminders;
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
		MasterReminders.CollectionChanged += OnCollectionChanged;

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
			LastBackupAttempt = DateTime.Now;
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

	private void InitialBackup() {
		string path = AppSettings.RecentFiles[0] + ".initialbak";
		Log.Print($"Backing up to: {path}");
		AppDataSaver saver = new AppDataSaver();
		saver.BackupSave(path, Data);
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
		saver.BackupSave(path, Data);

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

		Log.Print("Normalizing project data...");
		CleanAndNormalizeData();

		Log.Print("Resetting file changed flag...");
		ClearChangedFlag();

		Log.Print("Restoring AutoSave and AutoBackup settings.");
		Data.FileSettings.AutoSave = autoSave;
		Data.FileSettings.AutoBackup = autoBackup;

		Log.Print("Performing initial backup...");
		InitialBackup();
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
		bool isDataAlreadyLoaded = false;
		if (Data != null) {
			isDataAlreadyLoaded = true;
		} else {
			Data = new AppData();
			LoadCurrentData();
			ClearChangedFlag();
		}
		Log.Print("Choosing file to save as...");
		string saveFile = await AppServices.DialogService.SaveFile(Data.FileName, Data.BasePath);

		if (saveFile == null) {
			if (isDataAlreadyLoaded == false) {
				Log.Warn("File not saved. Shutting down...");
				Window? owner = AppServices.ApplicationService.GetWindow() as Window;
				await AppServices.DialogService.ShowAsync("No file created. Shutting down.", "Shutting down...", DialogButton.Ok, DialogIcon.Error, owner);
				AppServices.ApplicationService.Shutdown();
			} else {
				Log.Warn("File save canceled. Resuming previous file...");
			}
		} else {
			Log.Print("Creating new Data");
			Data = new AppData();

			Log.Print("Loading current data...");
			LoadCurrentData();

			Log.Print("Clearing item changed flags...");
			ClearChangedFlag();

			Log.Print("Setting current file path");
			Data.CurrentFilePath = saveFile;

			Log.Print($"Saving file: {saveFile}");
			Save(saveFile);
			SetupApplicationState();
		}
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

	public void SetWindowPosition(double width = 0, double height = 0) {
		double windowWidth = width;
		double windowHeight = height;
		PixelPoint pos = new(50, 50);
		Avalonia.Controls.WindowState windowState = Avalonia.Controls.WindowState.Normal;
		if (width == 0 || height == 0) {
			windowWidth = AppSettings.Instance.WindowWidth;
			windowHeight = AppSettings.Instance.WindowHeight;
			pos = new PixelPoint((int)AppSettings.Instance.WindowLeft, (int)AppSettings.Instance.WindowTop);
			windowState = AppSettings.Instance.WindowState switch {
				WindowState.Maximized => Avalonia.Controls.WindowState.Maximized,
				WindowState.Minimized => Avalonia.Controls.WindowState.Minimized,
				_ => Avalonia.Controls.WindowState.Normal
			};
		}
		Window mainWindow = AppServices.ApplicationService.GetWindow() as Window;

		mainWindow.Position = pos;
		mainWindow.Width = windowWidth;
		mainWindow.Height = windowHeight;
		mainWindow.WindowState = windowState;
		Log.Print($"Window position: {mainWindow.Position}");
		Log.Print($"Window size: {mainWindow.Width}x{mainWindow.Height}");
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
		}
		Log.Print("Shutting down...");
		Log.Shutdown();
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
	public ICommand ShowAboutWindowCommand => new RelayCommand(() => { AppServices.DialogService.ShowAboutAsync("v" + AppServices.ApplicationService.GetVersion()); });
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
	public ICommand DebugSetResolutionCommand => new RelayCommand<string>(width => DebugSetResolution(width));

	public void DebugSetResolution(string width) {
		double windowWidth = 0;
		double windowHeight = 0;
		switch (width) {
			case "1920":
				windowWidth = 1920;
				windowHeight = 1080;
				break;
			case "1366":
				windowWidth = 1366;
				windowHeight = 768;
				break;
			case "2560":
				windowWidth = 2560;
				windowHeight = 1440;
				break;
			case "3840":
				windowWidth = 3840;
				windowHeight = 2160;
				break;
			case "1024":
				windowWidth = 1024;
				windowHeight = 768;
				break;
			case "1600":
				windowWidth = 1600;
				windowHeight = 900;
				break;
			case "1280":
				windowWidth = 1280;
				windowHeight = 720;
				break;
			case "1536":
				windowWidth = 1536;
				windowHeight = 864;
				break;
			case "1440":
				windowWidth = 1440;
				windowHeight = 900;
				break;

			default:
				windowWidth = 0;
				windowHeight = 0;
				break;
		}
		SetWindowPosition(windowWidth, windowHeight);
	}
	public ICommand ShowReminderWindowCommand => new RelayCommand(() => ShowReminderWindow());
	public async void ShowReminderWindow(TodoItem item = null) {
		Task<ReminderEditorViewModel?> vmTask = AppServices.DialogService.ShowReminderEditorAsync(MasterReminders, MasterTodoItemsList, item);
		ReminderEditorViewModel vm = await vmTask;
		if (vm == null) {
			return;
		}
		UpdateRemindersFromDialog(null, vm.Reminders);
	}
	public ICommand ShowQuickReminderWindowCommand => new RelayCommand(() => ShowQuickReminderWindow());
	public async void ShowQuickReminderWindow(TodoItem item = null) {
		Task<ReminderInfo?> task = AppServices.DialogService.ShowQuickReminderAsync(item);
		ReminderInfo? ri = await task;
		if (ri == null) {
			return;
		}
		MasterReminders.Add(ri);
		OnPropertyChanged(nameof(ReminderStatusBarTextCount));
		OnPropertyChanged(nameof(ReminderStatusBarTextDueDate));
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