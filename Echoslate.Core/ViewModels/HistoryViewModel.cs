using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Echoslate.Core.Models;
using Echoslate.Core.Resources;
using Echoslate.Core.Services;

namespace Echoslate.Core.ViewModels;

public class HistoryViewModel : INotifyPropertyChanged {
	private AppData Data { get; set; }
	public string GitRepoPath {
		get => Data.FileSettings.GitRepoPath;
		set {
			Data.FileSettings.GitRepoPath = value;
			OnPropertyChanged();
		}
	}
	public string GitStatusMessage {
		get => Data.FileSettings.GitStatusMessage;
		set {
			Data.FileSettings.GitStatusMessage = value;
			OnPropertyChanged();
		}
	}
	private string _currentGitBranch;
	public string CurrentGitBranch {
		get => _currentGitBranch;
		set {
			_currentGitBranch = value;
			OnPropertyChanged();
		}
	}

	private ObservableCollection<TodoItem> _todoList;
	private ObservableCollection<HistoryItem> _allHistoryItems;

	private HistoryItem _currentHistoryItem;
	public HistoryItem CurrentHistoryItem {
		get => _currentHistoryItem;
		private set {
			SetProperty(ref _currentHistoryItem, value);
			UpdateCategorizedLists();
		}
	}

	public int VersionMajor {
		get => CurrentHistoryItem.VersionMajor;
		set => CurrentHistoryItem.VersionMajor = value;
	}
	public int VersionMinor {
		get => CurrentHistoryItem.VersionMinor;
		set => CurrentHistoryItem.VersionMinor = value;
	}
	public int VersionRevision {
		get => CurrentHistoryItem.VersionRevision;
		set => CurrentHistoryItem.VersionRevision = value;
	}
	public int VersionBuild {
		get => CurrentHistoryItem.VersionBuild;
		set => CurrentHistoryItem.VersionBuild = value;
	}

	public bool IsCurrentSelected => SelectedHistoryItem == CurrentHistoryItem;
	public bool CanCommit => IsCurrentSelected && CurrentHistoryItem.CompletedTodoItems.Any() && CurrentHistoryItem.Title != "";

	private HistoryItem _selectedHistoryItem;
	public HistoryItem SelectedHistoryItem {
		get => _selectedHistoryItem;
		set {
			if (value == null) {
				return;
			}
			if (_selectedHistoryItem != value) {
				_selectedHistoryItem = value;
				UpdateCategorizedLists();
				if (SelectedHistoryItem == null) {
					return;
				}
				OnPropertyChanged(nameof(Title));
				OnPropertyChanged(nameof(Notes));
				OnPropertyChanged(nameof(IsCommitted));
				OnPropertyChanged(nameof(CanBeCommitted));
				OnPropertyChanged(nameof(CommitDate));
				_selectedHistoryItem.GenerateCommitMessage();
				OnPropertyChanged(nameof(CommitMessage));
			}
		}
	}
	private ObservableCollection<HistoryItem> _selectedHistoryItems;
	public ObservableCollection<HistoryItem> SelectedHistoryItems {
		get => _selectedHistoryItems;
		set {
			_selectedHistoryItems = value;
			OnPropertyChanged();
		}
	}
	public string Title {
		get => SelectedHistoryItem.Title;
		set {
			if (SelectedHistoryItem.Title != value) {
				SelectedHistoryItem.Title = value;
				OnPropertyChanged();
				SelectedHistoryItem.GenerateCommitMessage();
				OnPropertyChanged(nameof(CommitMessage));
			}
		}
	}
	public string Notes {
		get => SelectedHistoryItem.Notes;
		set {
			if (SelectedHistoryItem.Notes != value) {
				SelectedHistoryItem.Notes = value;
				OnPropertyChanged();
				SelectedHistoryItem.GenerateCommitMessage();
				OnPropertyChanged(nameof(CommitMessage));
			}
		}
	}
	public bool CanBeCommitted => (SelectedHistoryItem.CompletedTodoItems.Count > 0 && !SelectedHistoryItem.IsCommitted);
	public bool IsCommitted => SelectedHistoryItem.IsCommitted;
	public DateTime CommitDate => SelectedHistoryItem.CommitDate;
	public string CommitMessage => SelectedHistoryItem.FullCommitMessage;

	private string _commitType;
	public string CommitType {
		get => _commitType;
		set {
			_commitType = value;
			OnPropertyChanged();
			CurrentHistoryItem.Type = _commitType;
			CurrentHistoryItem.GenerateCommitMessage();
			OnPropertyChanged(nameof(CommitMessage));
		}
	}
	private string _commitScope;
	public string CommitScope {
		get => _commitScope;
		set {
			_commitScope = value;
			OnPropertyChanged();
			CurrentHistoryItem.Scope = _commitScope;
			CurrentHistoryItem.GenerateCommitMessage();
			OnPropertyChanged(nameof(CommitMessage));
		}
	}
	private string _customScope;
	public string CustomScope {
		get => _customScope;
		set {
			_customScope = value;
			OnPropertyChanged();
			CurrentHistoryItem.Scope = _customScope;
			CurrentHistoryItem.GenerateCommitMessage();
			OnPropertyChanged(nameof(CommitMessage));
			_commitScope = "";
			OnPropertyChanged(nameof(CommitScope));
		}
	}


	public List<string> CommitTypes { get; } = new() {
		"feat",
		"fix",
		"refactor",
		"chore",
		"docs"
	};
	public ObservableCollection<string> CommitScopes { get; set; }

	public IEnumerable<HistoryItem> CommittedHistoryItems {
		get { return _allHistoryItems; }
	}

	public ObservableCollection<TodoItem> BugsCompleted { get; } = new();
	public ObservableCollection<TodoItem> FeaturesCompleted { get; } = new();
	public ObservableCollection<TodoItem> OtherCompleted { get; } = new();

	public int BugsCount => BugsCompleted.Count;
	public int FeaturesCount => FeaturesCompleted.Count;
	public int OtherCount => OtherCompleted.Count;

	private IncrementMode _selectedIncrementMode = IncrementMode.None;
	public IncrementMode SelectedIncrementMode {
		get => _selectedIncrementMode;
		set {
			_selectedIncrementMode = value;
			OnPropertyChanged();
		}
	}
	private bool _isTypeScope2Layer;
	public bool IsTypeScope2Layer {
		get => _isTypeScope2Layer;
		set {
			if (_isTypeScope2Layer == value) {
				return;
			}
			_isTypeScope2Layer = value;
			OnPropertyChanged();
		}
	}


	public HistoryViewModel() {
		CurrentHistoryItem = new HistoryItem {
			Title = "Work in progress",
			Version = new Version(0, 0, 0, 1)
		};

		_todoList = [];
		_allHistoryItems = [];
		SelectedHistoryItem = CurrentHistoryItem;
		SelectedHistoryItems = [];
		BugsCompleted = [];
		FeaturesCompleted = [];
		OtherCompleted = [];
	}
	public void Initialize(MainWindowViewModel mainWindowVM) {
		Data = mainWindowVM.Data;
		SelectedIncrementMode = mainWindowVM.Data.FileSettings.IncrementMode;
		_todoList = mainWindowVM.MasterTodoItemsList;
		_allHistoryItems = mainWindowVM.MasterHistoryItemsList;
		LoadData();
		OnPropertyChanged(nameof(CommittedHistoryItems));

		CommitScopes = Data.CommitScopes;
		CurrentHistoryItem = mainWindowVM.CurrentHistoryItem;
		CurrentHistoryItem.CompletedTodoItems.CollectionChanged += (s, e) => UpdateCategorizedLists();

		foreach (var h in _allHistoryItems) {
			foreach (var todoItem in h.CompletedTodoItems) {
				todoItem.CurrentView = View.History;
			}
		}
		if (!string.IsNullOrEmpty(CurrentHistoryItem.Type)) {
			CommitType = CurrentHistoryItem.Type;
		}
		if (!string.IsNullOrEmpty(CurrentHistoryItem.Scope)) {
			if (CommitScopes.Contains(CurrentHistoryItem.Scope)) {
				CommitScope = CurrentHistoryItem.Scope;
			} else {
				CustomScope = CurrentHistoryItem.Scope;
			}
		}
	}
	public void LoadData() {
		foreach (HistoryItem historyItem in _allHistoryItems) {
			historyItem.SortCompletedTodoItems();
		}
		CurrentHistoryItem = _allHistoryItems.FirstOrDefault(h => !h.IsCommitted) ??
							 new HistoryItem {
								 Title = "Work in progress",
								 Version = new Version(0, 0, 0, 1)
							 };
		if (!ReferenceEquals(CurrentHistoryItem, _allHistoryItems.FirstOrDefault())) {
			_allHistoryItems.Insert(0, CurrentHistoryItem);
		}
		SelectedHistoryItem = CurrentHistoryItem;
	}
	public void UpdateCategorizedLists() {
		if (SelectedHistoryItem == null) {
			BugsCompleted.Clear();
			FeaturesCompleted.Clear();
			OtherCompleted.Clear();
			return;
		}

		var completed = SelectedHistoryItem.CompletedTodoItems;

		BugsCompleted.Clear();
		FeaturesCompleted.Clear();
		OtherCompleted.Clear();

		foreach (var todo in completed) {
			if (todo.Tags?.Contains("#BUG", StringComparer.OrdinalIgnoreCase) == true) {
				BugsCompleted.Add(TodoItem.Copy(todo));
			} else if (todo.Tags?.Contains("#FEATURE", StringComparer.OrdinalIgnoreCase) == true) {
				FeaturesCompleted.Add(TodoItem.Copy(todo));
			} else {
				OtherCompleted.Add(TodoItem.Copy(todo));
			}
		}
		OnPropertyChanged(nameof(IsCurrentSelected));
		OnPropertyChanged(nameof(CanCommit));
		OnPropertyChanged(nameof(BugsCount));
		OnPropertyChanged(nameof(BugsCompleted));
		OnPropertyChanged(nameof(FeaturesCount));
		OnPropertyChanged(nameof(FeaturesCompleted));
		OnPropertyChanged(nameof(OtherCount));
		OnPropertyChanged(nameof(OtherCompleted));
		SelectedHistoryItem.SortCompletedTodoItems();
		SelectedHistoryItem.GenerateCommitMessage();
		OnPropertyChanged(nameof(CommitMessage));
		OnPropertyChanged(nameof(CommittedHistoryItems));
		OnPropertyChanged(nameof(Title));
		OnPropertyChanged(nameof(Notes));
	}
	private bool IsGitRepoValid() {
		if (string.IsNullOrEmpty(GitRepoPath)) {
			return false;
		}
		string gitDir = Path.Combine(GitRepoPath, ".git");
		if (!Directory.Exists(gitDir)) {
			GitStatusMessage = "⚠ Repository path no longer valid (missing .git)";
			return false;
		}
		return true;
	}
	private bool IsGitInstalled() {
		return GitHelper.GitInstallCheck();
	}
	private void SuggestTypeAndScopeFromBranch(string branchName) {
		if (string.IsNullOrWhiteSpace(branchName)) {
			return;
		}

		int slashIndex = branchName.IndexOf('/');
		if (slashIndex > 0 && slashIndex < branchName.Length - 1) {
			string detectedType = branchName.Substring(0, slashIndex);
			string detectedScope = branchName.Substring(slashIndex + 1).Replace("/", "-");

			CommitType = detectedType;
			if (CommitScopes.Contains(detectedScope)) {
				CustomScope = string.Empty;
				CommitScope = detectedScope;
			} else {
				CustomScope = detectedScope;
			}
		} else {
			if (CommitScopes.Contains(branchName)) {
				CustomScope = string.Empty;
				CommitScope = branchName;
			} else {
				CustomScope = branchName;
			}
		}

		UpdateCommitMessagePreview();
	}
	public void UpdateCommitMessagePreview() {
		SelectedHistoryItem.GenerateCommitMessage();
		OnPropertyChanged(nameof(CommitScope));
		OnPropertyChanged(nameof(CustomScope));
		OnPropertyChanged(nameof(CommitMessage));
		OnPropertyChanged(nameof(CommitType));
	}
	public ICommand DetectBranchCommand => new RelayCommand(DetectBranch);
	private void DetectBranch() {
		CurrentGitBranch = "(detecting...)";

		if (!IsGitRepoValid()) {
			CurrentGitBranch = "(invalid repo path)";
			return;
		}

		if (!IsGitInstalled()) {
			CurrentGitBranch = "(git not in PATH)";
			return;
		}

		try {
			var process = new Process {
				StartInfo = new ProcessStartInfo {
					FileName = "git",
					Arguments = "rev-parse --abbrev-ref HEAD",
					WorkingDirectory = GitRepoPath,
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					CreateNoWindow = true
				}
			};

			process.Start();
			string output = process.StandardOutput.ReadToEnd().Trim();
			string error = process.StandardError.ReadToEnd().Trim();
			process.WaitForExit();

			if (process.ExitCode == 0) {
				if (string.IsNullOrEmpty(output) || output == "HEAD") {
					CurrentGitBranch = "(detached HEAD)";
				} else {
					CurrentGitBranch = output;

					if (SelectedHistoryItem != null) {
						SuggestTypeAndScopeFromBranch(output);
					}
				}
			} else {
				CurrentGitBranch = $"(git error: {error})";
			}
		} catch (Exception ex) {
			CurrentGitBranch = "(failed to run git)";
		}
	}
	public Version IncrementVersion(Version currentVersion, IncrementMode mode) {
		return mode switch {
			IncrementMode.Major => new Version(currentVersion.Major + 1, currentVersion.Minor, currentVersion.Build, currentVersion.Revision),
			IncrementMode.Minor => new Version(currentVersion.Major, currentVersion.Minor + 1, currentVersion.Build, currentVersion.Revision),
			IncrementMode.Build => new Version(currentVersion.Major, currentVersion.Minor, currentVersion.Build + 1, currentVersion.Revision),
			IncrementMode.Revision => new Version(currentVersion.Major, currentVersion.Minor, currentVersion.Build, currentVersion.Revision + 1),
			_ => currentVersion
		};
	}


	public ICommand CommitCommand => new RelayCommand(CommitCurrent);
	public void CommitCurrent() {
		if (SelectedHistoryItem != _allHistoryItems[0] && !SelectedHistoryItem.IsCommitted) {
			SelectedHistoryItem.IsCommitted = true;
			SelectedHistoryItem.CommitDate = DateTime.Now;
			SelectedHistoryItem.CompletedTodoItems.CollectionChanged -= (s, e) => UpdateCategorizedLists();
			SelectedHistoryItem.GenerateCommitMessage();
			OnPropertyChanged(nameof(CanBeCommitted));
			CopyCommitMessage();
			return;
		}
			
		if (!string.IsNullOrWhiteSpace(CustomScope)) {
			var newScope = CustomScope.Replace(" ", "-");
			if (!CommitScopes.Contains(newScope)) {
				CommitScopes.Add(newScope);
				CommitScopes.Sort();
			}
			CustomScope = string.Empty;
			CommitScope = newScope;
		}
		CurrentHistoryItem = _allHistoryItems[0];
		CurrentHistoryItem.Type = CommitType;
		CurrentHistoryItem.IsCommitted = true;
		CurrentHistoryItem.CommitDate = DateTime.Now;
		CurrentHistoryItem.CompletedTodoItems.CollectionChanged -= (s, e) => UpdateCategorizedLists();

		CurrentHistoryItem.GenerateCommitMessage();
		CopyCommitMessage();

		CurrentHistoryItem = new HistoryItem {
			Title = "Work in progress",
			Version = IncrementVersion(CurrentHistoryItem.Version, SelectedIncrementMode)
		};
		CurrentHistoryItem.CompletedTodoItems.CollectionChanged += (s, e) => UpdateCategorizedLists();
		CurrentHistoryItem.Type = CommitType;
		CurrentHistoryItem.Scope = CommitScope;

		_allHistoryItems.Insert(0, CurrentHistoryItem);
		SelectedHistoryItem = CurrentHistoryItem;
	}
	public ICommand CopyCommitMessageCommand => new RelayCommand(CopyCommitMessage);
	public void CopyCommitMessage() {
		if (SelectedHistoryItem?.FullCommitMessage != null) {
			AppServices.ClipboardService.SetText(SelectedHistoryItem.FullCommitMessage);
		}
	}
	public ICommand ReactivateTodoCommand => new RelayCommand<TodoItem>(ReactivateTodo);
	public void ReactivateTodo(TodoItem ih) {
		if (ih == null) {
			Log.Error("TodoItem is null!");
			return;
		}
		TodoItem item = ih;
		if (!SelectedHistoryItem.IsCommitted && SelectedHistoryItem.RemoveCompletedTodo(item.Id)) {
			item.IsComplete = false;
			SelectedHistoryItem.SortCompletedTodoItems();
			if (SelectedHistoryItem.CompletedTodoItems.Count == 0) {
				Title = "Work in progress";
				Notes = "";
			}
			UpdateCategorizedLists();
		}
		_todoList.Add(item);
	}
	public ICommand EditTodoCommand => new RelayCommand<Object>(EditTodo);
	public async void EditTodo(Object ih) {
		if (ih == null) {
			Log.Error("TodoItem is null!");
			return;
		}
		TodoItem item = (TodoItem)ih;
		Task<TodoItemEditorViewModel?> vmTask = AppServices.DialogService.ShowTodoItemEditorAsync(item, null, new ObservableCollection<string>(Data.AllTags));
		TodoItemEditorViewModel? vm = await vmTask;
		if (vm != null) {
			TodoItem newItem = vm.ResultTodoItem;
			CurrentHistoryItem.RemoveCompletedTodo(newItem.Id);
			CurrentHistoryItem.CompletedTodoItems.Add(newItem);
		}
	}
	public ICommand SelectIncrementCommand => new RelayCommand(() => {
		var values = Enum.GetValues(typeof(IncrementMode)).Cast<IncrementMode>();
		int currentIndex = Array.IndexOf(values.ToArray(), SelectedIncrementMode);
		int nextIndex = (currentIndex + 1) % values.Count();

		SelectedIncrementMode = values.ElementAt(nextIndex);
	});
	public ICommand UndoCommitCommand => new RelayCommand<HistoryItem>(UndoCommit);
	public void UndoCommit(HistoryItem item) {
		if (SelectedHistoryItems == null || SelectedHistoryItems.Count <= 0) {
			return;
		}

		foreach (HistoryItem hItem in SelectedHistoryItems) {
			if (hItem == CurrentHistoryItem) {
				continue;
			}
			hItem.IsCommitted = false;
			hItem.CommitDate = DateTime.MinValue;
			hItem.CompletedTodoItems.CollectionChanged += (s, e) => UpdateCategorizedLists();
			
			Log.Print($"Undoing commit for: {item.Title}");
			OnPropertyChanged(nameof(IsCommitted));
			OnPropertyChanged(nameof(CanBeCommitted));
		}
	}
	public ICommand RecommitCommand => new RelayCommand<HistoryItem?>(Recommit);
	public void Recommit(HistoryItem? item) {
		if (SelectedHistoryItems == null || SelectedHistoryItems.Count <= 0) {
			return;
		}

		foreach (HistoryItem hItem in SelectedHistoryItems) {
			if (hItem == CurrentHistoryItem) {
				continue;
			}
			hItem.IsCommitted = true;
			Log.Print($"Recommitted: {hItem.Title}");
			OnPropertyChanged(nameof(SelectedHistoryItem.IsCommitted));
			OnPropertyChanged(nameof(IsCommitted));
		}
	}
	public ICommand DeleteCommand => new RelayCommand<HistoryItem>(Delete);
	public void Delete(HistoryItem item) {
		if (item == null) {
			Log.Error("Item is null!");
			return;
		}
		if (item.CompletedTodoItems.Count > 0) {
			Log.Error("HistoryItem had completed Todos. Remove before deleting.");
			return;
		}
		if (_allHistoryItems.Contains(item)) {
			_allHistoryItems.Remove(item);
			Log.Print($"Deleted: {item.Title}");
			UpdateCategorizedLists();
		}
	}

	public ICommand BumpMajorCommand => new RelayCommand(() => {
		CurrentHistoryItem.VersionMajor++;
		CurrentHistoryItem.VersionMinor = 0;
		CurrentHistoryItem.VersionBuild = 0;
		CurrentHistoryItem.VersionRevision = 0;
	});
	public ICommand BumpMinorCommand => new RelayCommand(() => {
		CurrentHistoryItem.VersionMinor++;
		CurrentHistoryItem.VersionBuild = 0;
		CurrentHistoryItem.VersionRevision = 0;
	});
	public ICommand BumpBuildCommand => new RelayCommand(() => {
		CurrentHistoryItem.VersionBuild++;
		CurrentHistoryItem.VersionRevision = 0;
	});
	public ICommand BumpRevisionCommand => new RelayCommand(() => { CurrentHistoryItem.VersionRevision++; });


	public event PropertyChangedEventHandler PropertyChanged;
	protected void OnPropertyChanged([CallerMemberName] string name = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
		if (Equals(field, value)) {
			return false;
		}
		field = value;
		OnPropertyChanged(propertyName);
		return true;
	}
}