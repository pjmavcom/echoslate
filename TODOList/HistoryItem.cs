/*	HistoryItem.cs
 * 08-Feb-2019
 * 13:31:07
 *
 * 
 */

using System;
using System.Collections.Generic;

namespace TODOList
{
	public class HistoryItem
	{
		// FIELDS //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// FIELDS //
		private string _notes;
		private string _title;
		private readonly string _dateAdded;
		private readonly string _timeAdded;
		private List<TodoItem> _completedTodos;
		private List<TodoItem> _completedTodosBugs;
		private List<TodoItem> _completedTodosFeatures;

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

			string[] pieces = newItem[0].Split('|');
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

		// METHOD  ///////////////////////////////////// ToString() //
		public override string ToString()
		{
			string result = "NewVCS" + Environment.NewLine; 
			result += DateAdded + "|" + TimeAdded + "|" + Title + "|" + Notes;

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
							"Estimated Total Time: " + totalTimeSoFar + Environment.NewLine +
							"Notes: " + Notes +
							"=Bugs Squashed====================================================================================================";
			foreach (TodoItem td in CompletedTodosBugs)
				result += Environment.NewLine + "--" + td.ToClipboard();
			
			result += Environment.NewLine + "--" + "=Features Added============================================================================================================================";
			foreach (TodoItem td in CompletedTodosFeatures)
				result += Environment.NewLine + "--" + td.ToClipboard();
			
			result += Environment.NewLine + "--" + "=Other Stuff===============================================================================================================================";
			foreach (TodoItem td in CompletedTodos)
				result += Environment.NewLine + "--" + td.ToClipboard();
			return result;
		}
	}
}