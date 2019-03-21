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

namespace TODOList
{
	public class HistoryItem : INotifyPropertyChanged
	{
		// FIELDS //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// FIELDS //
		private string _notes;
		private string _title;
		private string _dateAdded;
		private string _timeAdded;
		private List<TodoItem> _completedTodos;
		private List<TodoItem> _completedTodosBugs;
		private List<TodoItem> _completedTodosFeatures;
		private bool _hasBeenCopied;

		// PROPERTIES //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// PROPERTIES //
		public string Notes
		{
			get => _notes;
			set => _notes = value;
		}
		public string Title
		{
			get => _title;
			set => _title = value;
		}
		public string DateAdded => _dateAdded;
		public string TimeAdded => _timeAdded;
		public string DateTimeAdded => _dateAdded + "-" + _timeAdded;
		public List<TodoItem> CompletedTodos
		{
			get => _completedTodos;
			set => _completedTodos = value;
		}
		public List<TodoItem> CompletedTodosBugs
		{
			get => _completedTodosBugs;
			set => _completedTodosBugs = value;
		}
		public List<TodoItem> CompletedTodosFeatures
		{
			get => _completedTodosFeatures;
			set => _completedTodosFeatures = value;
		}
		public string TotalTime
		{
			get
			{
				long result = 0;
				foreach (TodoItem td in _completedTodos)
				{
					result += td.TimeTaken.Ticks;
				}
				return (result / TimeSpan.TicksPerMinute).ToString();
			}
		}
		public bool HasBeenCopied
		{
			get => _hasBeenCopied;
			set
			{
				_hasBeenCopied = value;
				OnPropertyChanged();
			}
		}
		// CONSTRUCTORS //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// CONSTRUCTORS //
		public HistoryItem(DateTime dateTime) : this(dateTime.ToString("yyyyMMdd"), dateTime.ToString("HHmmss"))
		{
			
		}
		public HistoryItem(string date, string time)
		{
			_completedTodos = new List<TodoItem>();
			_completedTodosBugs = new List<TodoItem>();
			_completedTodosFeatures = new List<TodoItem>();
			_dateAdded = date;
			_timeAdded = time;
			_notes = "";
		}
		public HistoryItem(List<string> newItem)
		{
			_completedTodos = new List<TodoItem>();
			_completedTodosBugs = new List<TodoItem>();
			_completedTodosFeatures = new List<TodoItem>();

			float version;
			string[] pieces = newItem[0].Split('|');
			if (pieces[0].Contains("VERSION"))
			{
				string[] versionPieces = pieces[0].Split(' ');
				version = Convert.ToSingle(versionPieces[1]);
			}
			else
				version = 2.0f;

			
			// Heres where versions are loaded
			if(version <= 2.0f)
				LoadPre2_0(newItem);
			if (version > 2.0f)
				Load2_0(newItem);
			
			
		}
		public void Load2_0(List<string> newItem)
		{
			string[] pieces = newItem[0].Split('|');
			_hasBeenCopied = Convert.ToBoolean(pieces[1]);
			_dateAdded = pieces[2];
			_timeAdded = pieces[3];
			_title = pieces[4];
			_notes = pieces[5];
			
			int index = 0;
			newItem.RemoveAt(0);
			foreach (string s in newItem)
			{
				if (s == "VCSTodos")
					break;
				else
					index++;
			}
			for (int i = 0; i < index; i++)
			{
				_notes += newItem[i] + Environment.NewLine;
			}

			for(int i = 0; i <= index; i++)
				newItem.RemoveAt(0);
			
			foreach (string s in newItem)
			{
				TodoItem td = new TodoItem(s);
				_completedTodos.Add(td);
			}
		}
		public void LoadPre2_0(List<string> newItem)
		{
			string[] pieces = newItem[0].Split('|');
			_hasBeenCopied = true;
			_dateAdded = pieces[0];
			_timeAdded = pieces[1];
			_title = pieces[2];
			_notes = pieces[3];
			
			int index = 0;
			newItem.RemoveAt(0);
			foreach (string s in newItem)
			{
				if (s == "VCSTodos")
					break;
				else
					index++;
			}
			for (int i = 0; i < index; i++)
			{
				_notes += newItem[i] + Environment.NewLine;
			}

			for(int i = 0; i <= index; i++)
				newItem.RemoveAt(0);
			
			foreach (string s in newItem)
			{
				TodoItem td = new TodoItem(s);
				_completedTodos.Add(td);
			}
		}

		// MONOGAME METHODS //////////////////////////////////////////////////////////////////////////////////////////////////////////////// MONOGAME METHODS //


		// METHODS ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// METHODS //
		// METHOD  ///////////////////////////////////// AddCompletedTodo() //
		public void AddCompletedTodo(TodoItem td)
		{
			_completedTodos.Add(td);
		}

		public void SetCopied()
		{
			HasBeenCopied = true;
		}
		public void ResetCopied()
		{
			HasBeenCopied = false;
		}
		// METHOD  ///////////////////////////////////// ToString() //
		public override string ToString()
		{
			string result = "NewVCS" + Environment.NewLine; 
			result += "VERSION " + MainWindow.VERSION + "|" + HasBeenCopied + "|" + DateAdded + "|" + TimeAdded + "|" + Title + "|" + Notes;

			result += Environment.NewLine + "VCSTodos";
			foreach (TodoItem td in CompletedTodos)
				result += Environment.NewLine + td;
			foreach (TodoItem td in CompletedTodosBugs)
				result += Environment.NewLine + td;
			foreach (TodoItem td in CompletedTodosFeatures)
				result += Environment.NewLine + td;
			result += Environment.NewLine;
			result += "EndVCS" + Environment.NewLine;
			return result;
		}
		public string ToClipboard(string totalTimeSoFar)
		{
			string result = DateAdded + "- " + Title + Environment.NewLine +
							"Estimated Time: " + TotalTime + Environment.NewLine +
							"Estimated Total Time: " + totalTimeSoFar;

			if (!Notes.Equals(""))
				result += Environment.NewLine + "Notes: " + BreakLines(Notes) + Environment.NewLine;

			if (CompletedTodosBugs.Count > 0)
			{
				result += Environment.NewLine + Environment.NewLine + "=Bugs Squashed====================================================================================================";
				foreach (TodoItem td in CompletedTodosBugs)
					result += Environment.NewLine + "--" + td.ToClipboard();
			}
			if (CompletedTodosFeatures.Count > 0)
			{
				result += Environment.NewLine + Environment.NewLine + "=Features Added===================================================================================================";
				foreach (TodoItem td in CompletedTodosFeatures)
					result += Environment.NewLine + "--" + td.ToClipboard();
			}
			if (CompletedTodos.Count > 0)
			{
				result += Environment.NewLine + Environment.NewLine + "=Other Stuff======================================================================================================";
				foreach (TodoItem td in CompletedTodos)
					result += Environment.NewLine + "--" + td.ToClipboard();
			}
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
					result += Environment.NewLine + "\t" + word + " ";
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