/*	HistoryItem.cs
 * 08-Feb-2019
 * 13:31:07
 *
 *
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Echoslate {
	public class HistoryItem : INotifyPropertyChanged {
		// FIELDS //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// FIELDS //
		private string _notes;
		private string _title;
		private string _dateAdded;
		private string _timeAdded;
		private List<TodoItem> _completedTodoItems;
		private bool _hasBeenCopied;

		public Version Version;
		public int VersionMajor {
			get => Version.Major;
			set {
				Version = new Version(value, Version.Minor, Version.Build, Version.Revision);
				OnPropertyChanged();
			}
		}
		public int VersionMinor {
			get => Version.Minor;
			set {
				Version = new Version(Version.Major, value, Version.Build, Version.Revision);
				OnPropertyChanged();
			}
		}
		public int VersionBuild {
			get => Version.Build;
			set {
				Version = new Version(Version.Major, Version.Minor, value, Version.Revision);
				OnPropertyChanged();
			}
		}
		public int VersionRevision {
			get => Version.Revision;
			set {
				Version = new Version(Version.Major, Version.Minor, Version.Build, value);
				OnPropertyChanged();
			}
		}
		public string VersionString => Version.ToString();

		private bool _isCommitted;
		public bool IsCommitted {
			get => _isCommitted;
			set { _isCommitted = value; }
		}
		public DateTime CommitDate { get; set; }

		public string FullCommitMessage { get; set; }

		// PROPERTIES //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// PROPERTIES //
		public string Notes {
			get => _notes;
			set => _notes = value;
		}
		public string Title {
			get => _title;
			set => _title = value;
		}
		public string DateAdded => _dateAdded;
		public string TimeAdded => _timeAdded;
		public string DateTimeAdded => _dateAdded + "-" + _timeAdded;
		public List<TodoItem> CompletedTodoItems {
			get => _completedTodoItems;
			set => _completedTodoItems = value;
		}
		public List<TodoItem> BugsCompleted { get; set; }
		public List<TodoItem> FeaturesCompleted { get; set; }
		public string TotalTime {
			get {
				long result = 0;
				foreach (TodoItem td in _completedTodoItems) {
					result += td.TimeTaken.Ticks;
				}
				return (result / TimeSpan.TicksPerMinute).ToString();
			}
		}
		public bool HasBeenCopied {
			get => _hasBeenCopied;
			private set {
				_hasBeenCopied = value;
				OnPropertyChanged();
			}
		}

		// CONSTRUCTORS //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// CONSTRUCTORS //
		public HistoryItem() : this(DateTime.Now.ToString(MainWindow.DATE_STRING_FORMAT), DateTime.Now.ToString(MainWindow.TIME_STRING_FORMAT)) {
			Version = new Version(7,7,7,7);
		}
		public HistoryItem(DateTime dateTime) : this(dateTime.ToString(MainWindow.DATE_STRING_FORMAT), dateTime.ToString(MainWindow.TIME_STRING_FORMAT)) {
		}
		public HistoryItem(string date, string time) {
			_completedTodoItems = new List<TodoItem>();
			BugsCompleted = new List<TodoItem>();
			FeaturesCompleted = new List<TodoItem>();
			_dateAdded = date;
			_timeAdded = time;
			_notes = "";
			Version = new Version();
			IsCommitted = false;
			FullCommitMessage = ToClipboard("0");
		}
		public HistoryItem(List<string> newItem) {
			_completedTodoItems = new List<TodoItem>();
			BugsCompleted = new List<TodoItem>();
			FeaturesCompleted = new List<TodoItem>();
			Load2_0(newItem);
			Version = new Version();
			IsCommitted = false;
			FullCommitMessage = ToClipboard("0");
		}
		private void Load2_0(List<string> newItem) {
			string[] pieces = newItem[0].Split('|');
			_hasBeenCopied = Convert.ToBoolean(pieces[0]);
			_dateAdded = pieces[1];
			_timeAdded = pieces[2];
			_title = pieces[3];
			_notes = AddNewLines(pieces[4]);

			int index = 0;
			newItem.RemoveAt(0);
			foreach (string s in newItem) {
				if (s == "VCSTodos") {
					break;
				}
				index++;
			}
			for (int i = 0; i < index; i++) {
				_notes += newItem[i] + Environment.NewLine;
			}

			for (int i = 0; i <= index; i++) {
				newItem.RemoveAt(0);
			}

			foreach (string s in newItem) {
				TodoItem td = new TodoItem(s);
				_completedTodoItems.Add(td);
			}
		}

		// METHODS ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// METHODS //
		public void AddCompletedTodo(TodoItem td) {
			_completedTodoItems.Add(td);
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
							"Estimated Time: " + TotalTime + Environment.NewLine +
							"Estimated Total Time: " + totalTimeSoFar;

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
			if (CompletedTodoItems.Count > 0) {
				result += Environment.NewLine + Environment.NewLine + "=Other Stuff======================================================================================================";
				foreach (TodoItem td in CompletedTodoItems)
					result += Environment.NewLine + "--" + td.ToClipboard();
			}
			return result;
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
		private string AddNewLines(string s) {
			return s.Replace("/n", Environment.NewLine);
		}
		private string RemoveNewLines(string s) {
			return s.Replace(Environment.NewLine, "/n");
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}