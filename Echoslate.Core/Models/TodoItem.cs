using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Echoslate.Core.Models;

public enum View {
	TodoList,
	Kanban,
	History
}

[Serializable]
public class TodoItem : INotifyPropertyChanged {
	private Guid _id;
	[JsonIgnore]
	public Guid Id {
		get => _id;
		set {
			_id = value;
			OnPropertyChanged();
		}
	}

	private string _todo;
	public string Todo {
		get => AddNewLines(_todo);
		set {
			_todo = value;
			ParseNewTags();
			OnPropertyChanged();
		}
	}

	private string _notes;
	public string Notes {
		get => AddNewLines(_notes);
		set {
			_notes = value;
			OnPropertyChanged();
		}
	}

	private string _problem;
	public string Problem {
		get => AddNewLines(_problem);
		set {
			_problem = value;
			OnPropertyChanged();
		}
	}

	private string _solution;
	public string Solution {
		get => AddNewLines(_solution);
		set {
			_solution = value;
			OnPropertyChanged();
		}
	}

	private DateTime _dateTimeStarted;
	public DateTime DateTimeStarted {
		get => _dateTimeStarted;
		set {
			_dateTimeStarted = value;
			OnPropertyChanged();
		}
	}
	private DateTime _dateTimeCompleted;
	public DateTime DateTimeCompleted {
		get => _dateTimeCompleted;
		set {
			_dateTimeCompleted = value;
			OnPropertyChanged();
		}
	}
	private TimeSpan _timeTaken;
	[JsonIgnore]
	public TimeSpan TimeTaken {
		get => _timeTaken;
		set {
			_timeTaken = value;
			OnPropertyChanged();
			OnPropertyChanged(nameof(IsTimerOn));
		}
	}
	private bool _isTimerOn;
	public bool IsTimerOn {
		get => _isTimerOn;
		set {
			_isTimerOn = value;
			OnPropertyChanged();
			OnPropertyChanged(nameof(TimeTaken));
		}
	}

	private bool _isComplete;
	public bool IsComplete {
		get => _isComplete;
		set {
			_isComplete = value;
			DateTimeCompleted = IsComplete ? DateTime.Now : DateTime.MinValue;
			OnPropertyChanged();
		}
	}

	private int _severity;
	public int Severity {
		get => _severity;
		set {
			_severity = value;
			OnPropertyChanged();
		}
	}

	private int _kanban;
	public int Kanban {
		get => _kanban;
		set {
			_kanban = value;
			KanbanRank = int.MaxValue;
			OnPropertyChanged();
		}
	}
	private int _kanbanRank;
	public int KanbanRank {
		get => _kanbanRank;
		set {
			_kanbanRank = value;
			OnPropertyChanged();
		}
	}

	private Dictionary<string, int> _rank;
	public Dictionary<string, int> Rank {
		get => _rank;
		set {
			_rank = value;
			OnPropertyChanged();
		}
	}
	private int _currentFilterRank;
	[JsonIgnore]
	public int CurrentFilterRank {
		get {
			return CurrentView switch {
				View.Kanban => KanbanRank,
				View.TodoList when Rank.ContainsKey(_currentFilter) => Rank[_currentFilter],
				_ => -1
			};
		}
		set {
			switch (CurrentView) {
				case View.Kanban:
					KanbanRank = value;
					break;
				case View.TodoList:
					Rank[_currentFilter] = value;
					break;
			}
			OnPropertyChanged();
		}
	}
	[JsonIgnore] public int CurrentKanbanFilter { get; set; }
	private string _currentFilter = "All";
	public string CurrentFilter {
		get => _currentFilter;
		set {
			_currentFilter = value;
			OnPropertyChanged();
		}
	}

	private View _currentView;
	[JsonIgnore]
	public View CurrentView {
		get => _currentView;
		set {
			_currentView = value;
			OnPropertyChanged();
		}
	}

	private ObservableCollection<string> _tags;
	public ObservableCollection<string> Tags {
		get => _tags;
		set {
			_tags = value;
			ParseNewTags();
			OnPropertyChanged();
		}
	}
	private HashSet<string> _hashedTags;
	[JsonIgnore]
	public HashSet<string> HashedTags {
		get {
			if (_hashedTags == null) {
				_hashedTags = new HashSet<string>(Tags);
			}
			return _hashedTags;
		}
	}
	private bool _isPrioritySorted;
	[JsonIgnore]
	public bool IsPrioritySorted {
		get => _isPrioritySorted;
		set {
			_isPrioritySorted = value;
			OnPropertyChanged();
		}
	}

	[JsonIgnore]
	private string TagsAndTodoToSave {
		get {
			string result = "";
			foreach (string t in _tags) {
				string[] pieces = t.Split('\r');
				result = pieces.Where(p => p != "").Aggregate(result, (current, p) => current + (p.Trim() + " "));
			}

			result += _todo;
			return AddNewLines(result);
		}
	}
	[JsonIgnore]
	public string NotesAndTags =>
		"Notes: " + Environment.NewLine + AddNewLines(Notes) + Environment.NewLine + "Problem: " + Environment.NewLine + AddNewLines(Problem) + Environment.NewLine + "Solution: " +
		Environment.NewLine + AddNewLines(Solution) + Environment.NewLine + "Tags:" + Environment.NewLine + TagsList;

	[JsonIgnore]
	public string Ranks {
		get {
			string result = "";
			foreach (KeyValuePair<string, int> kvp in Rank) {
				result += kvp.Key + " # " + kvp.Value + ",";
			}
			return result;
		}
	}

	[JsonIgnore]
	private string TagsList {
		get {
			string result = "";
			if (_tags.Count != 0)
				result = _tags[0];
			for (int i = 1; i < _tags.Count; i++)
				result += Environment.NewLine + _tags[i];

			return result;
		}
	}

	[JsonIgnore]
	public string TagsSorted {
		get {
			string result = "";
			for (int i = 0; i < _tags.Count; i++) {
				result += _tags[i];
				if (i != _tags.Count)
					result += " ";
			}

			return result;
		}
	}
	[JsonIgnore] public bool HasTags => Tags.Count > 0;
	[JsonIgnore] public string FirstTag => Tags.Count > 0 ? Tags[0] : "";


	public TodoItem() {
		Id = Guid.NewGuid();
		_todo = string.Empty;
		_notes = string.Empty;
		_problem = string.Empty;
		_solution = string.Empty;
		_timeTaken = new TimeSpan();
		_dateTimeStarted = DateTime.Now;
		_dateTimeCompleted = DateTime.MaxValue;
		_isTimerOn = false;
		_isComplete = false;
		_severity = 0;
		_kanban = 0;
		_kanbanRank = 0;
		_tags = [];
		_rank = [];
		_currentView = View.TodoList;
	}
	public static TodoItem Copy(TodoItem item, bool createNewGuid = false) {
		return new TodoItem() {
			Id = createNewGuid ? Guid.NewGuid() : item.Id,
			Todo = item.Todo,
			Notes = item.Notes,
			Problem = item.Problem,
			Solution = item.Solution,
			Tags = item.Tags,
			DateTimeStarted = item.DateTimeStarted,
			DateTimeCompleted = item.DateTimeCompleted,
			TimeTaken = item.TimeTaken,
			IsTimerOn = item.IsTimerOn,
			IsComplete = item.IsComplete,
			Severity = item.Severity,
			Kanban = item.Kanban,
			KanbanRank = item.KanbanRank,
			Rank = item.Rank,
			CurrentView = item.CurrentView,
		};
	}
	public TodoItem? SearchById(Guid id) {
		if (Id == id) {
			return this;
		}
		return null;
	}
	public bool HasId(Guid id) {
		return Id == id;
	}
	public bool HasTag(string tag) {
		return Tags.Contains(tag);
	}
	public bool UpdateTags(HashSet<string> tags) {
		bool allTagsChanged = false;
		if (string.IsNullOrWhiteSpace(Todo)) {
			return false;
		}
		foreach (string tag in Tags) {
			if (tags.Contains(tag)) {
				continue;
			}
			tags.Add(tag);
			allTagsChanged = true;
		}
		foreach (string t in tags) {
			if (HashedTags.Contains(t) || string.IsNullOrWhiteSpace(t)) {
				continue;
			}
			string cleanTag = t.TrimStart('#').Trim();
			if (string.IsNullOrEmpty(cleanTag)) {
				continue;
			}
			string escapedTag = Regex.Escape(cleanTag);
			string pattern = $@"\b{escapedTag}\b";
			if (!Regex.IsMatch(Todo, pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)) {
				continue;
			}
			if (!Tags.Contains(t)) {
				Tags.Add(t);
			}
		}
		return allTagsChanged;
	}
	public void CleanNotes() {
		Todo = ParseNotes(Todo);
		ParseNewTags();
		Notes = ParseNotes(Notes);
		Problem = ParseNotes(Problem);
		Solution = ParseNotes(Solution);
	}
	public void ResetTimer() {
		IsTimerOn = false;
		TimeTaken = new TimeSpan(0);
	}
	private void ParseNewTags() {
		_todo = TagsAndTodoToSave;
		ParseTags();
	}
	private void ParseTags() {
		_tags = new ObservableCollection<string>();
		string[] tempPieces = _todo.Split('\r');
		string temp = tempPieces.Aggregate("", (current, s) => current + (s + " "));
		tempPieces = temp.Split('\n');
		temp = tempPieces.Aggregate("", (current, s) => current + (s + " "));

		string[] pieces = temp.Split(' ');
		bool isBeginningTag = false;

		List<string> list = new List<string>();
		for (int index = 0; index < pieces.Length; index++) {
			string s = pieces[index];
			if (s == "") {
				continue;
			}

			if (s.Contains('#')) {
				if (index == 0) {
					isBeginningTag = true;
				}

				var t = s.ToUpper();
				if (t.Equals("#FEATURES") || t.Equals("#F")) {
					t = "#FEATURE";
				}
				if (t.Equals("#BUGS") || t.Equals("#B")) {
					t = "#BUG";
				}

				if (!_tags.Contains(t)) {
					_tags.Add(t);
				}

				s = s.Remove(0, 1);
				s = s.ToLower();
				if (s.Equals("f")) {
					s = "feature";
				}
				if (s.Equals("b")) {
					s = "bug";
				}
			} else {
				isBeginningTag = false;
			}

			if (isBeginningTag) {
				continue;
			}

			if (index == 0 ||
				index > 0 && pieces[index - 1].Contains(". ") ||
				index > 0 && pieces[index - 1].Contains("? ") ||
				list.Count == 0) {
				s = UpperFirstLetter(s);
			}

			list.Add(s);
		}

		string tempTodo = "";
		foreach (string s in list) {
			if (s == "") {
				continue;
			}
			tempTodo += s + " ";
		}

		var sorted = _tags.OrderBy(x => x).ToList();
		_tags.Clear();
		foreach (string s in sorted) {
			_tags.Add(s);
		}
		_todo = tempTodo.Trim();
	}
	private string ParseNotes(string notesToParse) {
		string[] pieces = notesToParse.Split(' ');
		List<string> list = new List<string>();
		for (int index = 0; index < pieces.Length; index++) {
			string s = pieces[index];
			if (index == 0 ||
				index > 0 && pieces[index - 1].Contains(".") ||
				index > 0 && pieces[index - 1].Contains("?") ||
				list.Count == 0) {
				s = UpperFirstLetter(s);
			}

			if (s.Contains("/n") || s.Contains(Environment.NewLine)) {
				s = UpperFirstLetterOfNewLine(s);
			}
			list.Add(s);
		}

		string tempNotes = "";
		foreach (string s in list) {
			if (s == "") {
				continue;
			}
			tempNotes += s + " ";
		}

		return tempNotes.Trim();
	}
	private void ParseTodo() {
		string[] pieces = _todo.Split(' ');
		List<string> list = new List<string>();
		for (int index = 0; index < pieces.Length; index++) {
			string s = pieces[index];
			if (index == 0 ||
				index > 0 && pieces[index - 1].Contains(".") ||
				index > 0 && pieces[index - 1].Contains("?") ||
				list.Count == 0) {
				s = UpperFirstLetter(s);
			}

			list.Add(s);
		}

		string tempTodo = "";
		foreach (string s in list) {
			if (s == "") {
				continue;
			}
			tempTodo += s + " ";
		}

		_todo = tempTodo.Trim();
	}
	private string UpperFirstLetter(string s) {
		string result = "";
		for (int i = 0; i < s.Length; i++) {
			if (i == 0) {
				result += s[i].ToString().ToUpper();
			} else {
				result += s[i];
			}
		}

		return result;
	}
	private string UpperFirstLetterOfNewLine(string s) {
		s = RemoveNewLines(s);
		string[] parts = s.Split('/');
		string newString = string.Empty;
		int count = 0;
		foreach (string part in parts) {
			if (count == 0) {
				count++;
				newString += part;
				continue;
			}
			if (part[0] == 'n') {
				newString += "/n" + UpperFirstLetter(part.Remove(0, 1));
			} else {
				newString += "/" + part;
			}
		}
		return newString;
	}
	public string ToClipboard() {
		string notes = AddNewLines(_notes);
		string problem = AddNewLines(_problem);
		string solution = AddNewLines(_solution);

		string result = BreakLines(_todo);
		result += GetNotesProblemSolution();

		return result;
	}
	public string GetNotesProblemSolution() {
		string result = "";
		if (_notes != "") {
			result += "\tNotes: " + BreakLinesAddTabs(_notes) + Environment.NewLine;
		}
		if (_problem != "") {
			result += "\tProblem: " + BreakLinesAddTabs(_problem) + Environment.NewLine;
		}
		if (_solution != "") {
			result += "\tSolution: " + BreakLinesAddTabs(_solution) + Environment.NewLine;
		}
		return result;
	}
	public string GetNotesProblemSolutionWithoutTabs() {
		string result = "";
		if (_notes != "") {
			result += BreakLines(_notes);
		}
		if (_problem != "") {
			result += "Problem: " + BreakLines(_problem);
		}
		if (_solution != "") {
			result += "Solution: " + BreakLines(_solution);
		}
		return result;
	}
	private string BreakLines(string s) {
		int charLimit = 140;
		int currentCharCount = 0;
		string result = "";
		string[] sentences = s.Split("\n");
		foreach (string sentence in sentences) {
			var trimmed = sentence.Replace("\r", "");
			if (trimmed.Length <= charLimit) {
				result += trimmed + Environment.NewLine;
				continue;
			}
			string[] pieces = trimmed.Split(' ');
			foreach (string word in pieces) {
				currentCharCount += word.Length + 1;

				if (currentCharCount <= charLimit) {
					result += word + " ";
				} else {
					currentCharCount = word.Length;
					result = result.Trim();
					result += Environment.NewLine + "\t" + word;
				}
			}
			result += Environment.NewLine;
		}
		return result;
	}
	private string BreakLinesAddTabs(string s) {
		int charLimit = 140;
		int currentCharCount = 0;
		string result = "";
		string[] sentences = s.Split("\n");
		foreach (string sentence in sentences) {
			var trimmed = sentence.Replace("\r", "");
			if (trimmed.Length <= charLimit) {
				result += trimmed + Environment.NewLine + "\t";
				continue;
			}
			string[] pieces = trimmed.Split(' ');
			foreach (string word in pieces) {
				currentCharCount += word.Length + 1;

				if (currentCharCount <= charLimit) {
					result += word + " ";
				} else {
					currentCharCount = word.Length + 1;
					result = result.Trim();
					result += Environment.NewLine + "\t  " + word + " ";
				}
			}
			result += Environment.NewLine + "\t";
		}
		return result.TrimEnd(['\r', '\n', '\t']);
	}

	private string AddNewLines(string s) {
		return s.Replace("/n", Environment.NewLine);
	}
	private string RemoveNewLines(string s) {
		return s.Replace(Environment.NewLine, "/n");
	}
	public void AddTag(string tag) {
		if (!_tags.Contains(tag) && tag != string.Empty) {
			_tags.Add(tag);
		}
		ParseNewTags();
	}

	public event PropertyChangedEventHandler? PropertyChanged;
	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
	protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
		if (EqualityComparer<T>.Default.Equals(field, value)) {
			return false;
		}
		field = value;
		OnPropertyChanged(propertyName);
		return true;
	}
}