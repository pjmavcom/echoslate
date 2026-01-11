using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Echoslate.Core.Models;
using Echoslate.Core.Services;

namespace Echoslate.Core.ViewModels;

public class TodoItemEditorViewModel : INotifyPropertyChanged {
	public ObservableCollection<string> SeverityOptions { get; } = new() { "None", "Low", "Med", "High" };
	private Guid _guid;
	public Guid Guid {
		get => _guid;
		set {
			_guid = value;
			OnPropertyChanged();
		}
	}

	private int _currentSeverity;
	public int CurrentSeverity {
		get => _currentSeverity;
		set {
			if (value > 3) {
				value = 0;
			}
			_currentSeverity = value;
			OnPropertyChanged();
			OnPropertyChanged(nameof(SeverityButtonBackground));
		}
	}
	public object SeverityButtonBackground => AppServices.BrushService.GetBrushForSeverity(CurrentSeverity);

	private int _rank;
	public int Rank {
		get => _rank;
		set {
			_rank = value;
			OnPropertyChanged();
		}
	}
	public bool IsRankEnabled { get; set; }

	private int _timeInMinutes;
	public int TimeInMinutes {
		get => _timeInMinutes;
		set {
			_timeInMinutes = value;
			OnPropertyChanged();
		}
	}

	private int _kanbanId;
	public int KanbanId {
		get => _kanbanId;
		set {
			_kanbanId = value;
			OnPropertyChanged();
		}
	}

	private string _todoText;
	public string TodoText {
		get => _todoText;
		set {
			_todoText = value;
			OnPropertyChanged();
		}
	}

	private string _notes;
	public string Notes {
		get => _notes;
		set {
			_notes = value;
			OnPropertyChanged();
		}
	}

	private string _problem;
	public string Problem {
		get => _problem;
		set {
			_problem = value;
			OnPropertyChanged();
		}
	}

	private string _solution;
	public string Solution {
		get => _solution;
		set {
			_solution = value;
			OnPropertyChanged();
		}
	}

	private readonly TodoItem _item;
	public TodoItem ResultTodoItem => _item;
	public bool Result;

	private ObservableCollection<string> AllAvailableTags;
	private ObservableCollection<string> _tags;
	public ObservableCollection<string> Tags {
		get => _tags;
		set {
			_tags = value;
			OnPropertyChanged();
		}
	}

	private List<string> ResultTags { get; set; }
	public string SelectedTag { get; set; }
	private readonly string _currentListHash;

	private int _previousRank;


	public TodoItemEditorViewModel(TodoItem td, string? currentListHash, ObservableCollection<string> allAvailableTags) {
		_item = TodoItem.Copy(td);
		_currentListHash = currentListHash ?? "All";

		Guid = _item.Id;
		AllAvailableTags = allAvailableTags;
		IsRankEnabled = _item.CurrentView != View.History;

		switch (_item.CurrentView) {
			case View.TodoList:
				_previousRank = _item.Rank[_currentListHash];
				Rank = _item.Rank[_currentListHash];
				break;
			case View.Kanban:
				_previousRank = _item.KanbanRank;
				Rank = _item.KanbanRank;
				break;
		}

		CurrentSeverity = _item.Severity;
		KanbanId = _item.Kanban;
		TodoText = _item.Todo;
		Notes = _item.Notes;
		Problem = _item.Problem;
		Solution = _item.Solution;
		Tags = new ObservableCollection<string>(_item.Tags);
		Notes = _item.Notes;
		if (Notes.Contains("/n")) {
			Notes = Notes.Replace("/n", Environment.NewLine);
		}
	}
	private void SetTodo() {
		ResultTodoItem.Severity = CurrentSeverity;
		switch (_item.CurrentView) {
			case View.TodoList:
				ResultTodoItem.Rank[_currentListHash] = Rank;
				break;
			case View.Kanban:
				ResultTodoItem.KanbanRank = Rank;
				break;
		}
		ResultTodoItem.Kanban = KanbanId;
		ResultTodoItem.TimeTaken = new TimeSpan(0, TimeInMinutes, 0);
		ResultTodoItem.Notes = Notes;

		string tempTodo = ExpandHashTagsInString(TodoText);
		string tempTags = "";
		ResultTags = new List<string>();
		foreach (string th in Tags)
			if (!ResultTags.Contains(th))
				ResultTags.Add(th);
		foreach (string tag in ResultTags)
			tempTags += tag + " ";
		tempTags = ExpandHashTagsInString(tempTags);

		ResultTodoItem.Tags = new ObservableCollection<string>();
		ResultTodoItem.Todo = tempTags.Trim() + " " + tempTodo.Trim();
		ResultTodoItem.Problem = Problem;
		ResultTodoItem.Solution = Solution;
	}
	public static string ExpandHashTagsInString(string todo) {
		string[] pieces = todo.Split(' ');

		List<string> list = [];
		foreach (string piece in pieces) {
			string s = piece;
			if (s.Contains('#')) {
				s = s.ToUpper();
				if (s.Equals("#FEATURES"))
					s = "#FEATURE";

				if (s.Equals("#BUGS"))
					s = "#BUG";

				s = s.ToLower();
			}

			list.Add(s);
		}

		return list.Where(s => s != "").Aggregate("", (current, s) => current + (s + " "));
	}

	public ICommand RankToTopCommand => new RelayCommand(RankToTop);
	private void RankToTop() {
		Rank = 0;
	}
	public ICommand RankToBottomCommand => new RelayCommand(RankToBottom);
	private void RankToBottom() {
		Rank = int.MaxValue;
	}
	public ICommand CycleSeverityCommand => new RelayCommand(CycleSeverity);
	public void CycleSeverity() {
		CurrentSeverity++;
	}
	public void OkCommand() {
		Result = true;
		SetTodo();
	}
	public void CompleteCommand() {
		Result = true;
		_item.IsComplete = true;
		SetTodo();
	}
	public ICommand AddTagCommand => new RelayCommand(AddTag);
	public async void AddTag() {
		List<string> selectedTags = new(Tags);

		Task<TagPickerViewModel?> vmTask = AppServices.DialogService.ShowTagPickerAsync([_item], AllAvailableTags, new List<string>(selectedTags));
		TagPickerViewModel tpvm = await vmTask;

		if (tpvm == null) {
			return;
		}
		if (tpvm.Result) {
			foreach (string tag in selectedTags) {
				Tags.Remove(tag);
			}
			foreach (string tag in tpvm.SelectedTags) {
				Tags.Add(tag);
			}
		}
	}
	public ICommand DeleteTagCommand => new RelayCommand<string>(DeleteTag);
	public void DeleteTag(string holder) {
		if (Tags.Contains(holder)) {
			Tags.Remove(holder);
		}
		OnPropertyChanged(nameof(Tags));
	}
	public ICommand CycleKanbanCommand => new RelayCommand(CycleKanban);
	public void CycleKanban() {
		KanbanId++;
		KanbanId %= 4;
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