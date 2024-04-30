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
		private int _kanban;
		private int _kanbanRank;
		private Dictionary<string, int> _rank;
		private List<string> _tags;
	
		// PROPERTIES //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// PROPERTIES //
		public string Todo
		{
			get => _todo;
			set
			{
				_todo = value;
				ParseTodo();
				ParseNewTags();
			}
		}
		private string TagsAndTodoToSave
		{
			get
			{
				string result = "";
				foreach (string t in _tags)
				{
					string[] pieces = t.Split('\r');
					result = pieces.Where(p => p != "").Aggregate(result, (current, p) => current + (p.Trim() + " "));
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
		public string NotesAndTags => "Notes: " + Notes + Environment.NewLine + "Tags:" + Environment.NewLine + TagsList;
		public string StartDateTime => _dateStarted + "" + "_" + _timeStarted;
		public string DateStarted => _dateStarted;
		public string TimeStarted => _timeStarted;
		public int Severity
		{
			get => _severity;
			set => _severity = value;
		}

		public int Kanban
		{
			get => _kanban;
			set => _kanban = value;
		}

		public int KanbanRank
		{
			get => _kanbanRank;
			set => _kanbanRank = value;
		}
		private string DateCompleted
		{
			set => _dateCompleted = value;
		}
		private string TimeCompleted
		{
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
			private set
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
				DateCompleted = IsComplete ? DateTime.Now.ToString(MainWindow.DATE_STRING_FORMAT) : "-";
				TimeCompleted = IsComplete ? DateTime.Now.ToString(MainWindow.TIME_STRING_FORMAT) : "-";
			}
		}
		public Dictionary<string, int> Rank
		{
			get => _rank;
			set => _rank = value;
		}
		public string Ranks
		{
			get
			{
				string result = "";
				foreach (KeyValuePair<string, int> kvp in Rank)
					result += kvp.Key + " # " + kvp.Value + ",";
				return result;
			}
		}
		public List<string> Tags
		{
			get => _tags;
			set => _tags = value;
		}
		private string TagsList
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
						result += " ";// Environment.NewLine;
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
			_dateStarted = DateTime.Now.ToString("yyyy/MM/dd");
			_timeStarted = DateTime.Now.ToString("HH:mm");
			_dateCompleted = "-";
			_timeCompleted = "-";
			_severity = 0;
			_rank = new Dictionary<string, int>();
		}
		public TodoItem(string newItem)
		{
			_tags = new List<string>();
			_rank = new Dictionary<string, int>();
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
			else if (version < 3.06f)
				Load2_0(newItem);
			else if (version < 3.20f)
				Load3_06(newItem);
			else
				Load3_20(newItem);
		}

		// METHODS ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// METHODS //
		private void LoadPre2_0(string newItem)
		{
			string[] pieces = newItem.Split('|');
			_dateStarted = pieces[0].Trim();
			_timeStarted = pieces[1].Trim();
			_dateCompleted = pieces[2].Trim();
			_timeCompleted = pieces[3].Trim();

			TimeTaken = new DateTime(Convert.ToInt64(pieces[4].Trim()));

			_isComplete = Convert.ToBoolean(pieces[5]); 
			_rank.Add("All", Convert.ToInt32(pieces[6]));
			          
			_severity = Convert.ToInt32(pieces[7]);
			Todo = pieces[8].Trim();
			
			if(pieces.Length > 9)
				Notes = pieces[9].Trim();
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
			
			string[] rankPieces = pieces[7].Split(',');
			foreach (string s in rankPieces)
				if(s != "")
				{
					string[] rank = s.Split('#');
					_rank.Add(rank[0].Trim(), Convert.ToInt32(rank[1].Trim()));
				}
			
			_severity = Convert.ToInt32(pieces[8]);
			Todo = pieces[9].Trim();
			
			if(pieces.Length > 10)
				Notes = pieces[10].Trim();

			FixDateTime();
		}
		private void FixDateTime()
		{
			if (!_dateStarted.Contains("-"))
			{
				_dateStarted = _dateStarted.Insert(4, "-");
				_dateStarted = _dateStarted.Insert(7, "-");
			}
			if (!_timeStarted.Contains(":"))
			{
				_timeStarted = _timeStarted.Insert(2, ":");
				_timeStarted = _timeStarted.Substring(0, 5);
			}
		}
		private void Load3_06(string newItem)
		{
			string[] pieces = newItem.Split('|');
			_dateStarted = pieces[1].Trim();
			_timeStarted = pieces[2].Trim();
			_dateCompleted = pieces[3].Trim();
			_timeCompleted = pieces[4].Trim();

			TimeTaken = new DateTime(Convert.ToInt64(pieces[5].Trim()));

			_isComplete = Convert.ToBoolean(pieces[6]);
			
			string[] rankPieces = pieces[7].Split(',');
			foreach (string s in rankPieces)
				if(s != "")
				{
					string[] rank = s.Split('#');
					_rank.Add(rank[0].Trim(), Convert.ToInt32(rank[1].Trim()));
				}
			
			_severity = Convert.ToInt32(pieces[8]);
			Todo = pieces[9].Trim();
			
			if(pieces.Length > 10)
				Notes = pieces[10].Trim();
		}

		private void Load3_20(string newItem)
		{
			string[] pieces = newItem.Split('|');
			_dateStarted = pieces[1].Trim();
			_timeStarted = pieces[2].Trim();
			_dateCompleted = pieces[3].Trim();
			_timeCompleted = pieces[4].Trim();

			TimeTaken = new DateTime(Convert.ToInt64(pieces[5].Trim()));

			_isComplete = Convert.ToBoolean(pieces[6]);
			
			string[] rankPieces = pieces[7].Split(',');
			foreach (string s in rankPieces)
				if(s != "")
				{
					string[] rank = s.Split('#');
					_rank.Add(rank[0].Trim(), Convert.ToInt32(rank[1].Trim()));
				}
			
			_severity = Convert.ToInt32(pieces[8]);
			Todo = pieces[9].Trim();
			
			if(pieces.Length > 10)
				Notes = pieces[10].Trim();
			if (pieces.Length > 11)
				Kanban = Convert.ToInt32(pieces[11].Trim());
			if (pieces.Length > 12)
				KanbanRank = Convert.ToInt32(pieces[12].Trim());
		}
		private void ParseNewTags()
		{
			_todo = TagsAndTodoToSave;
			ParseTags();
		}
		private void ParseTags()
		{
			_tags = new List<string>();
			string[] tempPieces = _todo.Split('\r');
			string temp = tempPieces.Aggregate("", (current, s) => current + (s + " "));
			tempPieces = temp.Split('\n');
			temp = tempPieces.Aggregate("", (current, s) => current + (s + " "));

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

					var t = s.ToUpper();
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

			string tempTodo = "";
			foreach (string s in list)
			{
				if (s == "")
					continue;
				tempTodo += s + " ";
			}
			_todo = tempTodo.Trim();
		}
		private void ParseNotes()
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
		private void ParseTodo()
		{
			string[] pieces = _todo.Split(' ');
			List<string> list = new List<string>();
			for (int index = 0; index < pieces.Length; index++)
			{
				string s = pieces[index];
				if (index == 0 ||
					index > 0 && pieces[index - 1].Contains(".") ||
					index > 0 && pieces[index - 1].Contains("?") ||
					list.Count == 0)
				{
					s = UpperFirstLetter(s);
				}
				list.Add(s);
			}

			string tempTodo = "";
			foreach (string s in list)
			{
				if (s == "")
					continue;
				tempTodo += s + " ";
			}
			_todo = tempTodo.Trim();
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
		public override string ToString()
		{
			string notes = _notes;
			//if (_notes.Contains('\r'))
			//	notes = "tested1";
			//if (_notes.Contains('\n'))
			//	notes = "tested2";
			if (_notes.Contains(Environment.NewLine))
				notes = notes.Replace(Environment.NewLine, "/n");

			string result = "VERSION " + MainWindow.PROGRAM_VERSION + "|" +
			                _dateStarted + "|" +
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
			                _kanbanRank;
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
