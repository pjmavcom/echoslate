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
		private readonly string _dateStarted;
		private readonly string _timeStarted;
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
				string[] pieces = _todo.Split(' ');
				foreach (string s in pieces)
				{
					if (s.Contains("#"))
						_tags.Add(s.ToUpper());
				}
			}
		}
		public string Notes
		{
			get => _notes;
			set => _notes = value;
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
		public List<string> Tags => _tags;
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

		// CONSTRUCTORS //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// CONSTRUCTORS //
		public TodoItem()
		{
			_todo = "";
			_notes = "";
			_dateStarted = DateTime.Now.ToString("yyyyMMdd");
			_timeStarted = DateTime.Now.ToString("HHmmss");
			_dateCompleted = "-";
			_timeCompleted = "-";
			_severity = 0;
			_rank = 0;
			ParseTags();
		}
		public TodoItem(DateTime dateTime, string todo, string notes, int sev)
		{
			_todo = todo;
			_notes = notes;
			_dateStarted = dateTime.ToString("yyyyMMdd");
			_timeStarted = dateTime.ToString("HHmmss");
			_dateCompleted = "-";
			_timeCompleted = "-";
			_severity = sev;
			_rank = 0;
			ParseTags();
		}
		public TodoItem(string newItem)
		{
			string[] pieces = newItem.Split('|');
			_dateStarted = pieces[0].Trim();
			_timeStarted = pieces[1].Trim();
			_dateCompleted = pieces[2].Trim();
			_timeCompleted = pieces[3].Trim();

			_timeTaken = new DateTime(Convert.ToInt64(pieces[4].Trim()));

			_isComplete = Convert.ToBoolean(pieces[5]); 
			_rank = Convert.ToInt32(pieces[6]);
			_severity = Convert.ToInt32(pieces[7]);
			_todo = pieces[8].Trim();
			
			if(pieces.Length > 9)
				_notes = pieces[9].Trim();

			ParseTags();
		}

		// MONOGAME METHODS //////////////////////////////////////////////////////////////////////////////////////////////////////////////// MONOGAME METHODS //


		// METHODS ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// METHODS //
		// METHOD  ///////////////////////////////////// ParseTags() //
		public void ParseTags()
		{
			_tags = new List<string>();
			string[] pieces = _todo.Split(' ');
			foreach(string s in pieces)
			{
				if (s.Contains('#'))
					_tags.Add(s.ToUpper());
			}
		}
		// METHOD  ///////////////////////////////////// ToString() //
		public override string ToString()
		{
			string result = _dateStarted + "|" + _timeStarted + "|" + _dateCompleted + "|" + _timeCompleted + "|" + _timeTaken.Ticks + "|" + _isComplete + "|" + _rank + "|" + _severity + "|" + _todo + "|" + _notes;

			foreach (string s in _tags)
			{
				result += "|" + s;
			}
			return result;
		}
		public string ToClipboard()
		{
			string result = _dateCompleted + "-" + _timeCompleted + " | " + _todo;
			if(_notes != "")
				result += Environment.NewLine + "\t\tNotes: " + _notes;
			return result;
		}
		
		
		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}