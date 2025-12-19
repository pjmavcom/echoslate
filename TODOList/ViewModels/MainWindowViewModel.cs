using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Application = System.Windows.Application;

namespace Echoslate.ViewModels {
	public class MainWindowViewModel : INotifyPropertyChanged {
		private AppData Data;
		public AppDataSettings AppDataSettings { get; set; }
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

		private List<TodoItem> _masterTodoItemsList;
		public List<TodoItem> MasterTodoItemsList {
			get => _masterTodoItemsList;
			set {
				_masterTodoItemsList = value;
				OnPropertyChanged();
			}
		}
		
		private List<HistoryItem> _masterHistoryItemsList;
		public List<HistoryItem> MasterHistoryItemsList {
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

		private List<string> _masterFilterTags;
		public List<string> MasterFilterTags {
			get => _masterFilterTags;
			set {
				_masterFilterTags = value;
				OnPropertyChanged();
			}
		}

		private bool _isChanged;

		public MainWindowViewModel(AppDataSettings appDataSettings) {
			AppDataSettings = appDataSettings;
			TodoListVM = new TodoListViewModel();
			KanbanVM = new KanbanViewModel();
			HistoryVM = new HistoryViewModel();
			
			_masterTodoItemsList = [];
			_masterHistoryItemsList = [];
			_masterFilterTags = [];

			if (appDataSettings.RecentFiles.Count != 0) {
				LoadRecentFile(appDataSettings);
			}
			SetWindowTitle();
			LoadCurrentData();
		}
		public void SetWindowTitle() {
			CurrentWindowTitle = AppDataSettings.WindowTitle + " - " + Data?.FileName;
		}
		public void RebuildAllViews() {
			TodoListVM.RebuildView();
			KanbanVM.RebuildView();
			HistoryVM.RebuildView();
		}
		public void LoadCurrentData() {
			if (Data != null) {
				MasterTodoItemsList = Data.TodoList;
				MasterHistoryItemsList = Data.HistoryList;
				MasterFilterTags = Data.FiltersList;
				
				if (MasterHistoryItemsList.Count != 0) {
					CurrentHistoryItem = MasterHistoryItemsList[0];
				}
			}
			TodoListVM.Initialize(this);
			KanbanVM.Initialize(this);
			HistoryVM.Initialize(this);
			RebuildAllViews();
			SetWindowTitle();
		}
		public void LoadRecentFile(AppDataSettings? settings) {
			while (true) {
				if (settings == null || settings.RecentFiles.Count == 0) {
					return;
				}

				if (File.Exists(settings.RecentFiles[0])) {
					Log.Print($"Loading recent file {settings.RecentFiles[0]}");
					new AppDataLoader(settings.RecentFiles[0], out Data);
					settings.SortRecentFiles(settings.RecentFiles[0]);
					return;
				}

				Log.Error($"{settings.RecentFiles[0]} does not exist.");
				settings.RecentFiles.RemoveAt(0);
			}
		}
		private void Save(string filePath) {
			AppDataSaver saver = new AppDataSaver();
			saver.Save(filePath, Data);
			AppDataSettings.AddRecentFile(Data.CurrentFilePath);
			SetWindowTitle();
		}
		private void Save() {
			Save(Data.CurrentFilePath);
		}
		private void Load(string? filePath) {
			var appDataLoader = new AppDataLoader(filePath, out Data);
			AppDataSettings.SortRecentFiles(filePath);
			LoadCurrentData();
			AppDataSettings.AddRecentFile(Data.CurrentFilePath);
			SetWindowTitle();
		}
		private void MenuNew() {
			Data = new AppData();
			LoadCurrentData();
			RebuildAllViews();
		}
		private void MenuLoad() {
			string basePath = Data == null ? "" : Data.BasePath;
			OpenFileDialog openFileDialog = new OpenFileDialog {
																   Title = @"Open file: ",
																   InitialDirectory = basePath,
																   Filter =
																	   @"txt files (*.txt)|*.txt|All files (*.*)|*.*"
															   };

			DialogResult dr = openFileDialog.ShowDialog();

			if (dr != System.Windows.Forms.DialogResult.OK) {
				return;
			}

			if (_isChanged) {
				DlgYesNo dlgYesNo = new DlgYesNo("Close", "Maybe save first?");
				dlgYesNo.ShowDialog();
				if (dlgYesNo.Result) {
					Save();
				}
			}
			Load(openFileDialog.FileName);
		}
		private void MenuSave() {
			Save();
		}
		private void MenuSaveAs() {
			Log.Print("Saving file as...");
			SaveFileDialog sfd = new SaveFileDialog {
														Title = @"Select folder to save file in.",
														FileName = Data.FileName,
														InitialDirectory = Data.BasePath,
														Filter = @"txt files (*.txt)|*.txt|All files (*.*)|*.*"
													};

			DialogResult dr = sfd.ShowDialog();
			if (dr != System.Windows.Forms.DialogResult.OK) {
				Log.Warn("File not saved. Continuing...");
				return;
			}
			
			Save(sfd.FileName);
		}
		private void MenuOptions() {
			bool autoSave = false;
			bool globalHotkeys = false;
			bool autoBackup = false;
			TimeSpan backupTime = new TimeSpan(0, 0, 0);
			DlgOptions options = new DlgOptions(autoSave, globalHotkeys, autoBackup, backupTime);
			options.ShowDialog();
			if (!options.Result)
				return;

#if DEBUG
#else
			_autoSave = options.AutoSave;
			_autoBackup = options.AutoBackup;
			_backupTime = options.BackupTime;
#endif
			AutoSave();
			
		}
		private void AutoSave() {
			// _isChanged = true;
			// _doBackup = true;
			// if (_currentOpenFile == "") {
				// SaveAs();
				// return;
			// }

			// if (_autoSave) {
				// if (AppDataSettings.RecentFiles.Count >= 1)
				// Save(AppDataSettings.RecentFiles[0]);
				// else
				// SaveAs();
			// }
		}
		private void MenuQuit() {
			Log.Print("Saving settings...");
			// SaveSettings();
			Log.Print("Settings saved.");

			if (_isChanged) {
				DlgYesNo dlg = new DlgYesNo("Close", "Maybe save first?");
				dlg.ShowDialog();
				if (dlg.Result) {
					Save(Data.CurrentFilePath);
				}

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
			AppDataSettings.RecentFiles.Remove(filePath);
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

		public event PropertyChangedEventHandler? PropertyChanged;
		private void OnPropertyChanged([CallerMemberName] string? name = null)
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}
}