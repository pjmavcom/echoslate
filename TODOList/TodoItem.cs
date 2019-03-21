/*	TodoItem.cs
 * 07-Feb-2019
 * 09:59:56
 *
 * 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace TODOList
{
	[Serializable]
	public class TodoItem : INotifyPropertyChanged
	{
		// FIELDS //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// FIELDS //
		private string _todo;
		private string _notes;
		private string _dateStarted;
		private string _timeStarted;
		private string _dateCompleted;
		private string _timeCompleted;
		private DateTime _timeTaken;
		private long _timeTakenInMinutes;
		private bool _isTimerOn;
		private bool _isComplete;
		private int _severity;
		private int _rank;
		private List<string> _tags;
		

		// PROPERTIES //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// PROPERTIES //
		public string Todo
		{
			get => _todo;
			set
			{
				_todo = value;
				ParseNewTags();
			}
		}
		public string TagsAndTodoToSave
		{
			get
			{
				string result = "";
				foreach (string t in _tags)
				{
					string[] pieces = t.Split('\r');
					foreach(string p in pieces)
					{
						if(p != "")
							result += p.Trim() + " ";
					}
				}
				result += _todo;
				return result;
			}
		}
		public string Notes
		{
			get => _notes;
			set
			{
				_notes = value;
				ParseNotes();
			}
		}
		public string NotesAndTags
		{
			get => "Notes: " + Notes + Environment.NewLine + "Tags:" + Environment.NewLine + TagsList;
		}
		public string StartDateTime => _dateStarted + "-" + _timeStarted;
		public string CompletedDateTime => _dateCompleted + "-" + _timeCompleted;
		public string DateStarted => _dateStarted;
		public string TimeStarted => _timeStarted;
		public int Severity
		{
			get => _severity;
			set => _severity = value;
		}
		public string DateCompleted
		{
			get => _dateCompleted;
			set => _dateCompleted = value;
		}
		public string TimeCompleted
		{
			get => _timeCompleted;
			set => _timeCompleted = value;
		}
		public DateTime TimeTaken
		{
			get => _timeTaken;
			set
			{
				_timeTaken = value;
				TimeTakenInMinutes = _timeTaken.Ticks / TimeSpan.TicksPerMinute;
				OnPropertyChanged();
			}
		}
		public long TimeTakenInMinutes
		{
			get => _timeTakenInMinutes; 
			set
			{
				_timeTakenInMinutes = value;
				OnPropertyChanged();
			}
		}
		public bool IsTimerOn
		{
			get => _isTimerOn;
			set => _isTimerOn = value;
		}
		public bool IsComplete
		{
			get => _isComplete;
			set
			{
				_isComplete = value;
				DateCompleted = IsComplete ? DateTime.Now.ToString(MainWindow.DATE) : "-";
				TimeCompleted = IsComplete ? DateTime.Now.ToString(MainWindow.TIME) : "-";
			}
		}
		public int Rank
		{
			get => _rank;
			set => _rank = value;
		}
		public List<string> Tags
		{
			get => _tags;
			set => _tags = value;
		}
		public string TagsList
		{
			get
			{
				string result = "";
				if (_tags.Count != 0)
					result = _tags[0];
				for (int i = 1; i < _tags.Count; i++)
				{
					result += Environment.NewLine + _tags[i];
				}
				return result;
			}
		}
		public string TagsSorted
		{
			get
			{
				string result = "";
				for (int i = 0; i < _tags.Count; i++)
				{
					result += _tags[i];
					if (i != _tags.Count)
						result += Environment.NewLine;
				}
				return result;
			}
		}

		// CONSTRUCTORS //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// CONSTRUCTORS //
		public TodoItem()
		{
			_tags = new List<string>();
			_todo = "";
			_notes = "";
			_dateStarted = DateTime.Now.ToString("yyyyMMdd");
			_timeStarted = DateTime.Now.ToString("HHmmss");
			_dateCompleted = "-";
			_timeCompleted = "-";
			_severity = 0;
			_rank = 0;
//			ParseTags();
		}
		public TodoItem(DateTime dateTime, string todo, string notes, int sev)
		{
			_tags = new List<string>();
//			_todo = todo;
//			_notes = notes;
			Todo = todo;
			Notes = notes;
			_dateStarted = dateTime.ToString("yyyyMMdd");
			_timeStarted = dateTime.ToString("HHmmss");
			_dateCompleted = "-";
			_timeCompleted = "-";
			_severity = sev;
			_rank = 0;
//			ParseTags();
		}
		public TodoItem(string newItem)
		{
			_tags = new List<string>();
			float version;
			string[] pieces = newItem.Split('|');
			if (pieces[0].Contains("VERSION"))
			{
				string[] versionPieces = pieces[0].Split(' ');
				version = Convert.ToSingle(versionPieces[1]);
			}
			else
				version = 2.0f;

			if (version <= 2.0f)
				LoadPre2_0(newItem);
			else if (version > 2.0f)
				Load2_0(newItem);
		}

		private void LoadPre2_0(string newItem)
		{
			string[] pieces = newItem.Split('|');
			_dateStarted = pieces[0].Trim();
			_timeStarted = pieces[1].Trim();
			_dateCompleted = pieces[2].Trim();
			_timeCompleted = pieces[3].Trim();

			TimeTaken = new DateTime(Convert.ToInt64(pieces[4].Trim()));

			_isComplete = Convert.ToBoolean(pieces[5]); 
			_rank = Convert.ToInt32(pieces[6]);
			_severity = Convert.ToInt32(pieces[7]);
			Todo = pieces[8].Trim();
//			_todo = pieces[8].Trim();
			
			if(pieces.Length > 9)
//				_notes = pieces[9].Trim();
				Notes = pieces[9].Trim();

//			ParseTags();
		}
		private void Load2_0(string newItem)
		{
			string[] pieces = newItem.Split('|');
			_dateStarted = pieces[1].Trim();
			_timeStarted = pieces[2].Trim();
			_dateCompleted = pieces[3].Trim();
			_timeCompleted = pieces[4].Trim();

			TimeTaken = new DateTime(Convert.ToInt64(pieces[5].Trim()));

			_isComplete = Convert.ToBoolean(pieces[6]); 
			_rank = Convert.ToInt32(pieces[7]);
			_severity = Convert.ToInt32(pieces[8]);
			Todo = pieces[9].Trim();
//			_todo = pieces[8].Trim();
			
			if(pieces.Length > 10)
//				_notes = pieces[9].Trim();
				Notes = pieces[10].Trim();

//			ParseTags();
		}

		// MONOGAME METHODS //////////////////////////////////////////////////////////////////////////////////////////////////////////////// MONOGAME METHODS //


		// METHODS ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// METHODS //
		public void ParseNewTags()
		{
			_todo = TagsAndTodoToSave;
			ParseTags();
		}
		public void ParseTags()
		{
			_tags = new List<string>();
			string temp = "";
			string[] tempPieces = _todo.Split('\r');
			foreach (string s in tempPieces)
				temp += s + " ";
			tempPieces = temp.Split('\n');
			temp = "";
			foreach (string s in tempPieces)
				temp += s + " ";
			
			string[] pieces = temp.Split(' ');
			bool isBeginningTag = false;

			List<string> list = new List<string>();
			for (int index = 0; index < pieces.Length; index++)
			{
				string s = pieces[index];
				if (s == "")
					continue;
				if (s.Contains('#'))
				{
					if (index == 0)
						isBeginningTag = true;
					
					string t = "";
					t = s.ToUpper();
					if (t.Equals("#FEATURES") || t.Equals("#F"))
						t = "#FEATURE";
					if (t.Equals("#BUGS") || t.Equals("#B"))
						t = "#BUG";
					if(!_tags.Contains(t))
						_tags.Add(t);
					
					s = s.Remove(0, 1);
					s = s.ToLower();
					if (s.Equals("f"))
						s = "feature";
					if (s.Equals("b"))
						s = "bug";
				}
				else
					isBeginningTag = false;
				
				if (isBeginningTag)
					continue;
				if (index == 0 ||
					index > 0 && pieces[index - 1].Contains(". ") ||
					index > 0 && pieces[index - 1].Contains("? ") ||
					list.Count == 0)
				{
					s = UpperFirstLetter(s);
				}
				list.Add(s);
			}

			string tempTodoTags = "";
			string tempTodo = "";
			foreach (string s in _tags)
			{
				if (s == "")
					continue;
				tempTodoTags += s + " ";
			}
			foreach (string s in list)
			{
				if (s == "")
					continue;
				tempTodo += s + " ";
			}
			_todo = tempTodo.Trim();
		}
		public void ParseNotes()
		{
			string[] pieces = _notes.Split(' ');
			List<string> list = new List<string>();
			for (int index = 0; index < pieces.Length; index++)
			{
				string s = pieces[index];
				if (index == 0 ||
					index > 0 && pieces[index - 1].Contains(". ") ||
					index > 0 && pieces[index - 1].Contains("? ") ||
					list.Count == 0)
				{
					s = UpperFirstLetter(s);
				}
				list.Add(s);
			}

			string tempNotes = "";
			foreach (string s in list)
			{
				if (s == "")
					continue;
				tempNotes += s + " ";
			}
			_notes = tempNotes.Trim();
		}
		private string UpperFirstLetter(string s)
		{
			string result = "";
			for (int i = 0; i < s.Length; i++)
			{
				if(i == 0)
					result += s[i].ToString().ToUpper();
				else
					result += s[i];
			}
			return result;
		}
		// METHOD  ///////////////////////////////////// ToString() //
		public override string ToString()
		{
			string result = "VERSION " + MainWindow.VERSION + "|" + _dateStarted + "|" + _timeStarted + "|" + _dateCompleted + "|" + _timeCompleted + "|" + _timeTaken.Ticks + "|" + _isComplete + "|" + _rank + "|" + _severity + "|" + TagsAndTodoToSave + "|" + _notes;
			return result;
		}
		public string ToClipboard()
		{
			string result = _dateCompleted + "-" + TimeTakenInMinutes + "m |" + BreakLines(_todo);
			if(_notes != "")
				result += Environment.NewLine + "\tNotes: " + BreakLines(_notes);
			return result;
		}
		private string BreakLines(string s)
		{
			int charLimit = 100;
			int currentCharCount = 0;
			List<string> lines = new List<string>();
			string result = "";
			string[] pieces = s.Split(' ');
			foreach (string word in pieces)
			{
				currentCharCount += word.Length + 1;

				if (currentCharCount <= charLimit)
				{
					result += word + " ";
				}
				else
				{
					currentCharCount = 0;
					result += Environment.NewLine + "\t\t" + word + " ";
				}
			}
			return result;
		}
		
		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}