/*	TodoItem.cs
 * 07-Feb-2019
 * 09:59:56
 *
 *
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using static System.Globalization.CultureInfo;

namespace Echoslate {
	[Serializable]
	public class TodoItem : INotifyPropertyChanged {
		// FIELDS //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// FIELDS //
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

		private string _dateStarted;
		public string DateStarted {
			get => _dateStarted;
			set {
				_dateStarted = value;
				OnPropertyChanged();
			}
		}

		private string _timeStarted;
		public string TimeStarted {
			get => _timeStarted;
			set {
				_timeStarted = value;
				OnPropertyChanged();
			}
		}

		private string _dateCompleted;
		private string DateCompleted {
			set {
				_dateCompleted = value;
				OnPropertyChanged();
			}
		}

		private string _timeCompleted;
		private string TimeCompleted {
			set {
				_timeCompleted = value;
				OnPropertyChanged();
			}
		}

		private DateTime _timeTaken;
		public DateTime TimeTaken {
			get => _timeTaken;
			set {
				_timeTaken = value;
				_timeTakenInMinutes = _timeTaken.Ticks / TimeSpan.TicksPerMinute;
				OnPropertyChanged();
			}
		}

		private long _timeTakenInMinutes;
		[JsonIgnore]
		public long TimeTakenInMinutes {
			get => _timeTakenInMinutes;
			set {
				_timeTakenInMinutes = value;
				_timeTaken = new DateTime(_timeTakenInMinutes * TimeSpan.TicksPerMinute);
				OnPropertyChanged();
			}
		}

		private bool _isTimerOn;
		public bool IsTimerOn {
			get => _isTimerOn;
			set {
				_isTimerOn = value;
				OnPropertyChanged();
			}
		}

		private bool _isComplete;
		public bool IsComplete {
			get => _isComplete;
			set {
				_isComplete = value;
				DateCompleted = IsComplete ? DateTime.Now.ToString(MainWindow.DATE_STRING_FORMAT) : "-";
				TimeCompleted = IsComplete ? DateTime.Now.ToString(MainWindow.TIME_STRING_FORMAT) : "-";
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


		// PROPERTIES //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// PROPERTIES //
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
		public string StartDateTime =>
			_dateStarted + "" + "_" + _timeStarted;

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
						result += " "; // Environment.NewLine;
				}

				return result;
			}
		}

		// CONSTRUCTORS //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// CONSTRUCTORS //
		public TodoItem() {
			_todo = string.Empty;
			_notes = string.Empty;
			_problem = string.Empty;
			_solution = string.Empty;
			_dateStarted = string.Empty;
			_timeStarted = string.Empty;
			_dateCompleted = string.Empty;
			_timeCompleted = string.Empty;
			_timeTaken = new DateTime();
			_isTimerOn = false;
			_isComplete = false;
			_severity = 0;
			_kanban = 0;
			_kanbanRank = 0;
			_tags = [];
			_rank = [];
			Tags.CollectionChanged += (s, e) => _hashedTags = null;
		}
		public void UpdateDates() {
			if (DateTime.TryParseExact(_dateStarted, "yyyy/MM/dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate)) {
			} else {
				parsedDate = DateTime.Today;
			}
			if (TimeSpan.TryParseExact(_timeStarted, @"hh\:mm", CultureInfo.InvariantCulture, out TimeSpan parsedTime)) {
			}
			DateTimeStarted = parsedDate.Date + parsedTime;
		}
		public void UpdateTags(HashSet<string> tags) {
			if (string.IsNullOrWhiteSpace(Todo)) {
				return;
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
				if (Regex.IsMatch(Todo, pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)) {
					Tags.Add(t);
				}
			}
		}
		public static TodoItem Create(string newItem) {
			string[] pieces = newItem.Split('|');
			if (pieces.Length < 13) {
				new DlgErrorMessage("This save file is corrupted! A todo did not load!" + Environment.NewLine + newItem).ShowDialog();
				return new TodoItem();
			}
			string dateStarted = pieces[0].Trim();
			string timeStarted = pieces[1].Trim();
			string dateCompleted = pieces[2].Trim();
			string timeCompleted = pieces[3].Trim();
		
			DateTime timeTaken = new DateTime(Convert.ToInt64(pieces[4].Trim()));
		
			bool isComplete = Convert.ToBoolean(pieces[5]);
		
			Dictionary<string, int> ranks = [];
			string[] rankPieces = pieces[6].Split(',');
			foreach (string s in rankPieces) {
				if (s == "") continue;
				string[] rank = s.Split('#');
				ranks.Add(rank[0].Trim(), Convert.ToInt32(rank[1].Trim()));
			}
		
			int severity = Convert.ToInt32(pieces[7]);
			string todo = pieces[8].Trim();
		
			string notes = "";
			int kanban = 0;
			int kanbanRank = 0;
			string problem = "";
			string solution = "";
			if (pieces.Length > 9) notes = pieces[9].Trim();
			if (pieces.Length > 10) kanban = Convert.ToInt32(pieces[10].Trim());
			if (pieces.Length > 11) kanbanRank = Convert.ToInt32(pieces[11].Trim());
			if (pieces.Length > 12) problem = pieces[12];
			if (pieces.Length > 13) solution = pieces[13];
			return new TodoItem {
									DateStarted = dateStarted,
									TimeStarted = timeStarted,
									DateCompleted = dateCompleted,
									TimeCompleted = timeCompleted,
									TimeTaken = timeTaken,
									IsComplete = isComplete,
									Rank = ranks,
									Severity = severity,
									Todo = todo,
									Notes = notes,
									Kanban = kanban,
									KanbanRank = kanbanRank,
									Problem = problem,
									Solution = solution
								};
		}
		// public TodoItem() {
		// _tags = new ObservableCollection<string>();
		// _todo = "";
		// _notes = "";
		// _problem = "";
		// _solution = "";
		// _dateStarted = DateTime.Now.ToString("yyyy/MM/dd");
		// _timeStarted = DateTime.Now.ToString("HH:mm");
		// _dateCompleted = "-";
		// _timeCompleted = "-";
		// _severity = 0;
		// _rank = new Dictionary<string, int>();
		// }
		// public TodoItem(string newItem) {
		// _tags = new ObservableCollection<string>();
		// _rank = new Dictionary<string, int>();
		// Load3_20(newItem);
		// }
		public void CleanNotes() {
			Todo = ParseNotes(Todo);
			ParseNewTags();
			Notes = ParseNotes(Notes);
			Problem = ParseNotes(Problem);
			Solution = ParseNotes(Solution);
		}
		public void ResetTimer() {
			IsTimerOn = false;
			TimeTaken = new DateTime(0);
		}
		private void FixDateTime() {
			if (!_dateStarted.Contains("-")) {
				_dateStarted = _dateStarted.Insert(4, "-");
				_dateStarted = _dateStarted.Insert(7, "-");
			}

			if (!_timeStarted.Contains(":")) {
				_timeStarted = _timeStarted.Insert(2, ":");
				_timeStarted = _timeStarted.Substring(0, 5);
			}
		}
		// private void Load3_20(string newItem) {
		// 	string[] pieces = newItem.Split('|');
		// 	if (pieces.Length < 13) {
		// 		new DlgErrorMessage("This save file is corrupted! A todo did not load!" + Environment.NewLine + newItem).ShowDialog();
		// 		return;
		// 	}
		// 	_dateStarted = pieces[0].Trim();
		// 	_timeStarted = pieces[1].Trim();
		// 	_dateCompleted = pieces[2].Trim();
		// 	_timeCompleted = pieces[3].Trim();
		//
		// 	TimeTaken = new DateTime(Convert.ToInt64(pieces[4].Trim()));
		//
		// 	_isComplete = Convert.ToBoolean(pieces[5]);
		//
		// 	string[] rankPieces = pieces[6].Split(',');
		// 	foreach (string s in rankPieces) {
		// 		if (s == "") continue;
		// 		string[] rank = s.Split('#');
		// 		_rank.Add(rank[0].Trim(), Convert.ToInt32(rank[1].Trim()));
		// 	}
		//
		// 	_severity = Convert.ToInt32(pieces[7]);
		// 	Todo = pieces[8].Trim();
		//
		// 	if (pieces.Length > 9) Notes = pieces[9].Trim();
		// 	if (pieces.Length > 10) Kanban = Convert.ToInt32(pieces[10].Trim());
		// 	if (pieces.Length > 11) KanbanRank = Convert.ToInt32(pieces[11].Trim());
		// 	if (pieces.Length > 12) Problem = pieces[12];
		// 	if (pieces.Length > 13) Solution = pieces[13];
		// }
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
				if (s == "") continue;

				if (s.Contains('#')) {
					if (index == 0) isBeginningTag = true;

					var t = s.ToUpper();
					if (t.Equals("#FEATURES") || t.Equals("#F")) t = "#FEATURE";
					if (t.Equals("#BUGS") || t.Equals("#B")) t = "#BUG";

					if (!_tags.Contains(t)) _tags.Add(t);

					s = s.Remove(0, 1);
					s = s.ToLower();
					if (s.Equals("f")) s = "feature";
					if (s.Equals("b")) s = "bug";
				} else {
					isBeginningTag = false;
				}

				if (isBeginningTag) continue;

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
				if (s == "") continue;
				tempTodo += s + " ";
			}

			// TODO Figure out how to sort ObservableCollections 
			// _tags.Sort();
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

				if (s.Contains("/n") || s.Contains(Environment.NewLine))
					s = UpperFirstLetterOfNewLine(s);
				list.Add(s);
			}

			string tempNotes = "";
			foreach (string s in list) {
				if (s == "")
					continue;
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
				if (s == "") continue;
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
				if (part[0] == 'n')
					newString += "/n" + UpperFirstLetter(part.Remove(0, 1));
				else
					newString += "/" + part;
			}
			return newString;
		}
		public override string ToString() {
			string notes = _notes;
			string problem = _problem;
			string solution = _solution;
			notes = RemoveNewLines(notes);
			problem = RemoveNewLines(problem);
			solution = RemoveNewLines(solution);

			string result = _dateStarted + "|" +
							_timeStarted + "|" +
							_dateCompleted + "|" +
							_timeCompleted + "|" +
							_timeTaken.Ticks + "|" +
							_isComplete + "|" +
							Ranks + "|" +
							_severity + "|" +
							TagsAndTodoToSave + "|" +
							notes + "|" +
							_kanban + "|" +
							_kanbanRank + "|" +
							problem + "|" +
							solution;
			return result;
		}
		public string ToClipboard() {
			string notes = AddNewLines(_notes);
			string problem = AddNewLines(_problem);
			string solution = AddNewLines(_solution);

			string result = _dateCompleted + "-" + TimeTakenInMinutes + "m |" + BreakLines(_todo);
			if (_notes != "")
				result += Environment.NewLine + "\tNotes: " + BreakLines(notes);
			if (_problem != "")
				result += Environment.NewLine + "\tProblem: " + BreakLines(problem);
			if (_solution != "")
				result += Environment.NewLine + "\tSolution: " + BreakLines(solution);

			return result;
		}
		private string BreakLines(string s) {
			int charLimit = 100;
			int currentCharCount = 0;
			string result = "";
			string[] pieces = s.Split(' ');
			foreach (string word in pieces) {
				currentCharCount += word.Length + 1;

				if (currentCharCount <= charLimit)
					result += word + " ";
				else {
					currentCharCount = 0;
					result += Environment.NewLine + "\t\t" + word + " ";
				}
			}

			return result;
		}
		private string AddNewLines(string s) {
			return s.Replace("/n", Environment.NewLine);
		}
		private string RemoveNewLines(string s) {
			return s.Replace(Environment.NewLine, "/n");
		}
		public void AddTag(string tag) {
			if (!_tags.Contains(tag) && tag != string.Empty)
				_tags.Add(tag);
			ParseNewTags();
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}