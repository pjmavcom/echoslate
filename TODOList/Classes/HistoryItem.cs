/*	HistoryItem.cs
 * 08-Feb-2019
 * 13:31:07
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

namespace Echoslate {
	public class HistoryItem : INotifyPropertyChanged {
		private Version _version;
		public Version Version {
			get => _version;
			set {
				_version = value;
				OnPropertyChanged();
			}
		}

		private string _title = "";
		public string Title {
			get => _title;
			set {
				_title = value;
				OnPropertyChanged();
			}
		}

		private string _dateAdded = "";
		public string DateAdded {
			get => _dateAdded;
			set {
				_dateAdded = value;
				OnPropertyChanged();
			}
		}

		private string _timeAdded = "";
		public string TimeAdded {
			get => _timeAdded;
			set {
				_timeAdded = value;
				OnPropertyChanged();
			}
		}

		private string _notes = "";
		public string Notes {
			get => _notes;
			set {
				_notes = value;
				OnPropertyChanged();
			}
		}

		private ObservableCollection<TodoItem> _completedTodoItems;
		public ObservableCollection<TodoItem> CompletedTodoItems {
			get => _completedTodoItems;
			set {
				_completedTodoItems = value;
				OnPropertyChanged();
			}
		}

		private bool _hasBeenCopied;
		public bool HasBeenCopied {
			get => _hasBeenCopied;
			private set {
				_hasBeenCopied = value;
				OnPropertyChanged();
			}
		}

		[JsonIgnore]
		public int VersionMajor {
			get => Version.Major;
			set {
				Version = new Version(value, Version.Minor, Version.Build, Version.Revision);
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public int VersionMinor {
			get => Version.Minor;
			set {
				Version = new Version(Version.Major, value, Version.Build, Version.Revision);
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public int VersionBuild {
			get => Version.Build;
			set {
				Version = new Version(Version.Major, Version.Minor, value, Version.Revision);
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public int VersionRevision {
			get => Version.Revision;
			set {
				Version = new Version(Version.Major, Version.Minor, Version.Build, value);
				OnPropertyChanged();
			}
		}
		[JsonIgnore] public string VersionString => Version.ToString();

		private bool _isCommitted;
		public bool IsCommitted {
			get => _isCommitted;
			set {
				_isCommitted = value;
				OnPropertyChanged();
			}
		}

		private DateTime _commitDate;
		public DateTime CommitDate {
			get => _commitDate;
			set {
				_commitDate = value;
				OnPropertyChanged();
			}
		}


		private string _fullCommitMessage;
		[JsonIgnore]
		public string FullCommitMessage {
			get => _fullCommitMessage;
			set {
				_fullCommitMessage = value;
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public string TotalTime {
			get {
				long result = 0;
				foreach (TodoItem td in _completedTodoItems) {
					result += td.TimeTaken.Ticks;
				}
				return (result / TimeSpan.TicksPerMinute).ToString();
			}
		}

		[JsonIgnore] public string DateTimeAdded => _dateAdded + "-" + _timeAdded;

		public List<TodoItem> BugsCompleted = [];
		public List<TodoItem> FeaturesCompleted = [];
		public List<TodoItem> OtherCompleted = [];


		// CONSTRUCTORS //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// CONSTRUCTORS //
		public HistoryItem() {
			Version = new Version();
			_title = string.Empty;
			_dateAdded = string.Empty;
			_timeAdded = string.Empty;
			_notes = string.Empty;
			CompletedTodoItems = [];
			_hasBeenCopied = false;
			_isCommitted = false;
			_commitDate = new DateTime();

			SortCompletedTodoItems();
		}
		public static HistoryItem Create(string date, string time) {
			return new HistoryItem {
									   DateAdded = date,
									   TimeAdded = time
								   };
		}
		public void AddCompletedTodo(TodoItem td) {
			CompletedTodoItems.Add(td);
			SortCompletedTodoItems();
		}
		public bool HasCompletedTodo(Guid id) {
			foreach (TodoItem item in CompletedTodoItems) {
				if (item.HasId(id)) {
					return true;
				}
			}
			return false;
		}
		public bool RemoveCompletedTodo(Guid id) {
			foreach (TodoItem item in CompletedTodoItems) {
				if (item.HasId(id)) {
					CompletedTodoItems.Remove(item);
					return true;
				}
			}
			return false;
		}
		public void SortCompletedTodoItems() {
			BugsCompleted.Clear();
			FeaturesCompleted.Clear();
			OtherCompleted.Clear();
			foreach (TodoItem item in _completedTodoItems) {
				if (item.Tags.Contains("#BUG")) {
					BugsCompleted.Add(item);
				} else if (item.Tags.Contains("#FEATURE")) {
					FeaturesCompleted.Add(item);
				} else {
					OtherCompleted.Add(item);
				}
			}
			FullCommitMessage = ToClipboard("0");
		}
		public void SetCopied() {
			HasBeenCopied = true;
		}
		public void ResetCopied() {
			HasBeenCopied = false;
		}
		public override string ToString() {
			string result = "NewVCS" + Environment.NewLine;
			result += HasBeenCopied + "|" + DateAdded + "|" + TimeAdded + "|" + Title + "|" + RemoveNewLines(Notes) + Environment.NewLine;
			result += "VCSTodos";
			foreach (TodoItem td in CompletedTodoItems) {
				result += Environment.NewLine + td;
			}

			foreach (TodoItem td in BugsCompleted) {
				result += Environment.NewLine + td;
			}

			foreach (TodoItem td in FeaturesCompleted) {
				result += Environment.NewLine + td;
			}
			result += Environment.NewLine;
			result += "EndVCS" + Environment.NewLine;
			return result;
		}
		public void GenerateCommitMessage() {
			FullCommitMessage = ToClipboard("0");
		}
		public string ToClipboard(string totalTimeSoFar) {
			string result = DateAdded + "- " + Title + Environment.NewLine +
							"Estimated Time: " + TotalTime + Environment.NewLine;

			if (!Notes.Equals(""))
				result += Environment.NewLine + "Notes: " + BreakLines(Notes) + Environment.NewLine;

			if (BugsCompleted.Count > 0) {
				result += Environment.NewLine + Environment.NewLine + "=Bugs Squashed====================================================================================================";
				foreach (TodoItem td in BugsCompleted)
					result += Environment.NewLine + "--" + td.ToClipboard();
			}
			if (FeaturesCompleted.Count > 0) {
				result += Environment.NewLine + Environment.NewLine + "=Features Added===================================================================================================";
				foreach (TodoItem td in FeaturesCompleted)
					result += Environment.NewLine + "--" + td.ToClipboard();
			}
			if (OtherCompleted.Count > 0) {
				result += Environment.NewLine + Environment.NewLine + "=Other Stuff======================================================================================================";
				foreach (TodoItem td in OtherCompleted)
					result += Environment.NewLine + "--" + td.ToClipboard();
			}
			return result;
		}
		public void UpdateDates() {
			if (DateTime.TryParseExact(DateAdded, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate)) {
			} else {
				parsedDate = DateTime.Today;
			}
			if (TimeSpan.TryParseExact(TimeAdded, @"hhmmss", CultureInfo.InvariantCulture, out TimeSpan parsedTime)) {
			}
			CommitDate = parsedDate.Date + parsedTime;

			MigrateCommitMessages();
		}
		private void MigrateCommitMessages() {
			string message = Title?.Trim();
			message = message.Remove(0, 1);

			int spaceIndex = message.IndexOf(' ');
			if (spaceIndex > 0) {
				string versionPart = message.Substring(0, spaceIndex).Trim();
				string titlePart = message.Substring(spaceIndex).TrimStart();

				// Basic check: is it four numbers with dots?
				if (versionPart.Count(c => c == '.') == 3 && versionPart.All(c => char.IsDigit(c) || c == '.')) {
					Version = Version.Parse(versionPart); // Store as string or Version.Parse(versionPart)
					Title = titlePart;
				}
			}
		}
		private string BreakLines(string s) {
			int charLimit = 100;
			int currentCharCount = 0;
			string result = "";
			string[] pieces = s.Split(' ');
			foreach (string word in pieces) {
				currentCharCount += word.Length + 1;

				if (currentCharCount <= charLimit) {
					result += word + " ";
				} else {
					currentCharCount = 0;
					result += Environment.NewLine + "\t" + word + " ";
				}
			}
			return result;
		}
		private static string AddNewLines(string s) {
			return s.Replace("/n", Environment.NewLine);
		}
		private static string RemoveNewLines(string s) {
			return s.Replace(Environment.NewLine, "/n");
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}