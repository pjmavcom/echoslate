using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Echoslate.Core.Models;
using Echoslate.Core.Services;

namespace Echoslate.Core.ViewModels;

public class TodoMultiItemEditorViewModel : INotifyPropertyChanged {
	public List<string> ResultTags;
	public string ResultTodo;
	public int ResultRank;
	public int ResultSeverity;

	private string _todo;
	public string Todo {
		get => _todo;
		set {
			if (_todo != value) {
				_todo = value;
				IsTodoChangeable = true;
				OnPropertyChanged(nameof(IsTodoChangeable));
				OnPropertyChanged();
			}
		}
	}
	public bool IsSeverityChangeable { get; set; }
	public bool IsRankChangeable { get; set; }
	private bool _isCompleteChangeable;
	public bool IsCompleteChangeable {
		get => _isCompleteChangeable;
		set {
			_isCompleteChangeable = value;
			OnPropertyChanged();
		}
	}

	public bool IsTodoChangeable { get; set; }
	public bool IsTagChangeable { get; set; }

	public ObservableCollection<string> _tags;
	public ObservableCollection<string> Tags {
		get => _tags;
		set {
			_tags = value;
			IsTagChangeable = true;
			OnPropertyChanged(nameof(IsRankChangeable));
			OnPropertyChanged();
		}
	}
	private int _rank;
	public int Rank {
		get => _rank;
		set {
			_rank = value;
			IsRankChangeable = true;
			OnPropertyChanged(nameof(IsRankChangeable));
			OnPropertyChanged();
		}
	}
	public bool IsRankEnabled { get; set; }

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

	private readonly string _currentFilter;

	public string CompleteButtonContent { get; set; }

	private List<TodoItem> items;
	public readonly List<string> CommonTags;

	public TodoMultiItemEditorViewModel(List<TodoItem> items, string currentFilter) {
		if (items.Count != 0) {
			IsRankEnabled = items[0].CurrentView != View.History;
		}

		CommonTags = GetCommonTags(items);

		_currentFilter = currentFilter;
		_currentSeverity = items[0].Severity;
		_rank = items[0].CurrentView switch {
			View.Kanban => items[0].KanbanRank,
			View.TodoList => items[0].CurrentFilterRank,
			_ => _rank
		};

		CurrentSeverity = _currentSeverity;
		CompleteButtonContent = items[0].IsComplete ? "Reactivate" : "Complete";

		_tags = new ObservableCollection<string>();
		foreach (string tag in CommonTags) {
			_tags.Add(tag);
		}
	}
	private static List<string> GetCommonTags(List<TodoItem> items) {
		return new(items.Select(x => x.Tags ?? Enumerable.Empty<string>()).Aggregate((a, b) => a.Intersect(b).ToList()));
	}
	private void SetResult() {
		if (IsTagChangeable) {
			ResultTags = new List<string>();
			foreach (string th in _tags) {
				string tag = th.ToUpper();
				if (!ResultTags.Contains(tag))
					ResultTags.Add(tag);
			}
		}
		if (IsSeverityChangeable) {
			ResultSeverity = CurrentSeverity;
		}
		if (IsRankChangeable) {
			ResultRank = Rank;
		}
		if (IsTodoChangeable) {
			ResultTodo = Todo;
		}
	}
	private void Delete(string th) {
		_tags.Remove(th);
		IsTagChangeable = true;
		OnPropertyChanged(nameof(IsTagChangeable));
	}
	private void AddTag() {
		string name = "#NEWTAG";
		int tagNumber = 0;
		bool nameExists = false;
		do {
			foreach (string t in _tags) {
				if (t == name.ToUpper() + tagNumber ||
					t == "#" + name.ToUpper() + tagNumber) {
					tagNumber++;
					nameExists = true;
					break;
				}
				nameExists = false;
			}
		} while (nameExists);

		string th = name + tagNumber;
		_tags.Add(th);
		IsTagChangeable = true;
		OnPropertyChanged(nameof(IsTagChangeable));
	}
	public void OkCommand() {
		SetResult();
	}
	public void CompleteCommand() {
		SetResult();
	}
	public void CancelCommand() {
		IsTagChangeable = false;
		IsSeverityChangeable = false;
		IsRankChangeable = false;
		IsTodoChangeable = false;
		IsCompleteChangeable = false;
	}
	public void CycleSeverity() {
		CurrentSeverity++;
		IsSeverityChangeable = true;
		OnPropertyChanged(nameof(IsSeverityChangeable));
	}

	public ICommand CycleSeverityCommand => new RelayCommand(CycleSeverity);
	public ICommand RankToTopCommand => new RelayCommand(RankToTop);
	private void RankToTop() {
		Rank = 0;
		IsRankChangeable = true;
		OnPropertyChanged(nameof(IsRankChangeable));
	}
	public ICommand RankToBottomCommand => new RelayCommand(RankToBottom);
	private void RankToBottom() {
		Rank = int.MaxValue;
		IsRankChangeable = true;
		OnPropertyChanged(nameof(IsRankChangeable));
	}
	public ICommand AddTagCommand => new RelayCommand(AddTag);
	public ICommand DeleteTagCommand => new RelayCommand<string>(Delete);
	
	
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