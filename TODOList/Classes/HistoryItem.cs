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
		private string _type;
		public string Type {
			get => _type;
			set {
				_type = value;
				OnPropertyChanged();
			}
		}

		private string _scope;
		public string Scope {
			get => _scope;
			set {
				_scope = value;
				OnPropertyChanged();
			}
		}
		private string _branch;
		public string Branch {
			get => _branch;
			set {
				_branch = value;
				OnPropertyChanged();
			}
		}

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
				OnPropertyChanged(nameof(Version));
				OnPropertyChanged(nameof(VersionString));
			}
		}
		[JsonIgnore]
		public int VersionMinor {
			get => Version.Minor;
			set {
				Version = new Version(Version.Major, value, Version.Build, Version.Revision);
				OnPropertyChanged();
				OnPropertyChanged(nameof(Version));
				OnPropertyChanged(nameof(VersionString));
			}
		}
		[JsonIgnore]
		public int VersionBuild {
			get => Version.Build;
			set {
				Version = new Version(Version.Major, Version.Minor, value, Version.Revision);
				OnPropertyChanged();
				OnPropertyChanged(nameof(Version));
				OnPropertyChanged(nameof(VersionString));
			}
		}
		[JsonIgnore]
		public int VersionRevision {
			get => Version.Revision;
			set {
				Version = new Version(Version.Major, Version.Minor, Version.Build, value);
				OnPropertyChanged();
				OnPropertyChanged(nameof(Version));
				OnPropertyChanged(nameof(VersionString));
			}
		}
		[JsonIgnore] public string VersionString => Version.ToString();

		private bool _isCommitted;
		public bool IsCommitted {
			get => _isCommitted;
			set {
				_isCommitted = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(IsEditing));
			}
		}
		public bool IsEditing => !IsCommitted;

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

		private string _fullTitle;
		public string FullTitle => Type + "(" + Scope + "): " + Title;


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
			_scope = string.Empty;
			_type = string.Empty;

			SortCompletedTodoItems();
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
			FullCommitMessage = ToClipboard();
		}
		public void GenerateCommitMessage() {
			FullCommitMessage = ToClipboard();
		}
		public string ToClipboard() {
			// if (Title.Contains("(")) {
			// 	int scopeIndex = Title.IndexOf("(");
			// 	int scopeEndIndex = Title.IndexOf(")");
			// 	string type = Title.Substring(0, scopeIndex);
			// 	string scope = Title.Substring(scopeIndex + 1, scopeEndIndex - scopeIndex - 1);
			// 	if (string.IsNullOrEmpty(Type)) {
			// 		Type = type;
			// 	}
			// 	if (string.IsNullOrEmpty(Scope)) {
			// 		Scope = scope;
			// 		Title = Title.Substring(scopeEndIndex + 2);
			// 	}
			// }
			Scope = Scope.Replace(" ", "-");
			string result = FullTitle + Environment.NewLine;

			if (BugsCompleted.Count > 0) {
				foreach (TodoItem td in BugsCompleted) {
					result += Environment.NewLine + "- " + td.ToClipboard();
				}
				result += Environment.NewLine;
			}
			if (FeaturesCompleted.Count > 0) {
				foreach (TodoItem td in FeaturesCompleted) {
					result += Environment.NewLine + "- " + td.ToClipboard();
				}
				result += Environment.NewLine;
			}
			if (OtherCompleted.Count > 0) {
				foreach (TodoItem td in OtherCompleted) {
					result += Environment.NewLine + "- " + td.ToClipboard();
				}
				result += Environment.NewLine;
			}
			if (!Notes.Equals("")) {
				result += BreakLines(Notes);
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
		private static string RemoveNewLines(string s) {
			return s.Replace(Environment.NewLine, "/n");
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}