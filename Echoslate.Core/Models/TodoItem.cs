using System.Collections.ObjectModel;using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Echoslate.Core.Resources;

namespace Echoslate.Core.Models;

public enum View {
	TodoList,
	Kanban,
	History
}

[Serializable]
public class TodoItem : INotifyPropertyChanged {
	private HashSet<Guid> _reminderGuids;
	public HashSet<Guid> ReminderGuids {
		get => _reminderGuids;
		set {
			if (_reminderGuids == value) {
				return;
			}
			_reminderGuids = value;
			OnPropertyChanged();
		}
	}
	private ObservableCollection<ReminderInfo> _reminders;
	[JsonIgnore]
	public ObservableCollection<ReminderInfo> Reminders {
		get => _reminders;
		set {
			if (_reminders == value) {
				return;
			}
			_reminders = value;
			OnPropertyChanged();
			OnPropertyChanged(nameof(HasActiveReminder));
			OnPropertyChanged(nameof(ReminderDueDateString));
			// OnPropertyChanged(nameof(IsReminderDueNow));
			// OnPropertyChanged(nameof(IsReminderSnoozing));
		}
	}
	[JsonIgnore]
	public bool HasActiveReminder {
		get => Reminders.Count > 0;
	}
	public ReminderInfo? GetNearestReminder() {
		ReminderInfo nearestReminder = new();
		nearestReminder.DueDate = DateTime.MaxValue;
		foreach (ReminderInfo ri in Reminders) {
			if (ri.DueDate < nearestReminder.DueDate) {
				nearestReminder = ri;
			}
		}
		if (nearestReminder.DueDate == DateTime.MaxValue) {
			return null;
		}
		return nearestReminder;
	}
	[JsonIgnore]
	public string ReminderDueDateString {
		get {
			if (Reminders == null) {
				return "";
			}
			ReminderInfo? nearestReminder = GetNearestReminder();
			if (nearestReminder == null) {
				return "";
			}
			if (nearestReminder.IsSnoozeActive) {
				return $"{nearestReminder.SnoozeUntil:yyyy-MM-dd - HH:mm}";
			}
			if (nearestReminder.IsActive) {
				return $"{nearestReminder.DueDate:yyy-MM-dd - HH:mm}";
			}
			return "";
		}
	}
	// [JsonIgnore]
	// public bool IsReminderDueNow {
	// get => Reminders is { IsDueNow: true };
	// }
	// [JsonIgnore]
	// public bool IsReminderSnoozing {
	// get => Reminders is { IsSnoozeActive: true };
	// }
	// [JsonIgnore]
	// public bool IsReminderActive {
	// get => Reminders is { IsActive: true };
	// }
	// [JsonIgnore] public DateTime ReminderDueDate => Reminders.DueDate;
	// [JsonIgnore] public string ReminderMessage => Reminders.Message;

	private Guid _guid;
	public Guid Guid {
		get => _guid;
		set {
			_guid = value;
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
	private int _priority;
	public int Priority {
		get => _priority;
		set {
			if (_priority == value) {
				return;
			}
			_priority = value;
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
			string currentFilter = _currentFilter.TrimStart('#').ToLower().CapitalizeFirstLetter();
			return CurrentView switch {
				View.Kanban => KanbanRank,
				View.TodoList when Rank.ContainsKey(currentFilter) => Rank[currentFilter],
				_ => -1
			};
		}
		set {
			string currentFilter = _currentFilter.TrimStart('#').ToLower().CapitalizeFirstLetter();
			switch (CurrentView) {
				case View.Kanban:
					KanbanRank = value;
					break;
				case View.TodoList:
					Rank[currentFilter] = value;
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
		NotesProblemsSolutions + Environment.NewLine + "Tags:" + Environment.NewLine + TagsList;
	[JsonIgnore]
	public string NotesProblemsSolutions =>
		"Notes: " + Environment.NewLine + AddNewLines(Notes) + Environment.NewLine + "Problem: " + Environment.NewLine + AddNewLines(Problem) + Environment.NewLine + "Solution: " +
		Environment.NewLine + AddNewLines(Solution);

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
		Guid = Guid.NewGuid();
		_reminders = [];
		_reminderGuids = [];
		_todo = string.Empty;
		_notes = string.Empty;
		_problem = string.Empty;
		_solution = string.Empty;
		_timeTaken = TimeSpan.Zero;
		_dateTimeStarted = DateTime.Now;
		_dateTimeCompleted = DateTime.MaxValue;
		_isTimerOn = false;
		_isComplete = false;
		_severity = 0;
		_priority = 0;
		_kanban = 0;
		_kanbanRank = int.MaxValue;
		_tags = [];
		_rank = [];
		_currentView = View.TodoList;
		NormalizeData();
	}
	public static TodoItem Copy(TodoItem item, bool createNewGuid = false) {
		TodoItem newItem = new TodoItem() {
			Guid = createNewGuid ? Guid.NewGuid() : item.Guid,
			ReminderGuids = item.ReminderGuids,
			Reminders = item.Reminders,
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
			Priority = item.Priority,
			Kanban = item.Kanban,
			KanbanRank = item.KanbanRank,
			Rank = item.Rank,
			CurrentView = item.CurrentView,
		};
		newItem.NormalizeData();
		return newItem;
	}

	// public void SetSnooze(TimeSpan snoozeTime) {
		// Reminders.SnoozeUntil = DateTime.Now + snoozeTime;
		// OnPropertyChanged(nameof(IsReminderSnoozing));
	// }
	// public void ClearReminder() {
		// Reminders.Clear();
	// }
	public void UpdateReminder() {
		OnPropertyChanged(nameof(Reminders));
		OnPropertyChanged(nameof(ReminderDueDateString));
		OnPropertyChanged(nameof(HasActiveReminder));
	}
	public void ClearReminders() {
		ReminderGuids.Clear();
		Reminders.Clear();
		UpdateReminder();
	}
	public void ClearReminder(Guid guid) {
        ReminderGuids.Remove(guid);
        ReminderInfo? toRemove = Reminders.FirstOrDefault(ri => ri.Guid == guid);
		if (toRemove != null) {
			Reminders.Remove(toRemove);
		}
		UpdateReminder();
	}
	public void NormalizeData() {
		NormalizeRankKeys();
		NormalizeTags();
	}
	private void NormalizeRankKeys() {
		Dictionary<string, int> newRank = new Dictionary<string, int>();
		foreach (KeyValuePair<string, int> kvp in Rank) {
			string key = kvp.Key.TrimStart('#').ToLower().CapitalizeFirstLetter();
			if (kvp.Key != key) {
				Log.Warn($"Changing {kvp.Key} on TodoItem: {Guid}");
			}
			int value = kvp.Value;
			if (!newRank.ContainsKey(key)) {
				newRank.Add(key, value);
			}
		}
		Rank = newRank;
	}
	private void NormalizeTags() {
		ObservableCollection<string> newTags = new();
		foreach (string tag in Tags) {
			string newTag = "#" + tag.TrimStart('#').ToUpper();
			if (tag != newTag) {
				Log.Warn($"Changing {tag} on TodoItem: {Guid}");
			}
			newTags.Add(newTag);
		}
		Tags = newTags;
	}
	public bool SearchByGuid(Guid id) {
		return Guid == id;
	}
	public bool HasId(Guid id) {
		return Guid == id;
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
	public string GetHistoryItemNotes() {
		string result = "";
		if (_notes != "") {
			result += BreakLinesAddTabs(_notes) + Environment.NewLine;
		}
		if (_problem != "") {
			result += "Problem: " + BreakLinesAddTabs(_problem) + Environment.NewLine;
		}
		if (_solution != "") {
			result += "Solution: " + BreakLinesAddTabs(_solution) + Environment.NewLine;
		}
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
	public void AddReminder(ReminderInfo ri) {
		Reminders.Add(ri);
		ReminderGuids.Add(ri.Guid);
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