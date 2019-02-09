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
		private readonly string _dateAdded;
		private readonly string _timeAdded;
		private readonly List<TodoItem> _completedTodos;

		// PROPERTIES //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// PROPERTIES //
		public string Notes
		{
			get => _notes;
			set => _notes = value;
		}
		public string DateAdded => _dateAdded;
		public string TimeAdded => _timeAdded;
		public string DateTimeAdded => _dateAdded + "-" + _timeAdded;
		public List<TodoItem> CompletedTodos => _completedTodos;

		// CONSTRUCTORS //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// CONSTRUCTORS //
		public HistoryItem(DateTime dateTime) : this(dateTime.ToString("yyyyMMdd"), dateTime.ToString("HHmmss"))
		{
			
		}
		public HistoryItem(string date, string time)
		{
			_completedTodos = new List<TodoItem>();
			_dateAdded = date;
			_timeAdded = time;
			_notes = "";
		}
		public HistoryItem(List<string> newItem)
		{
			_completedTodos = new List<TodoItem>();

			string[] pieces = newItem[0].Split('|');
			_dateAdded = pieces[0];
			_timeAdded = pieces[1];
			_notes = pieces[2];
			
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
			result += DateAdded + "|" + TimeAdded + "|" + Notes;

			result += Environment.NewLine + "VCSTodos";
			foreach (TodoItem td in CompletedTodos)
			{
				result += Environment.NewLine + td;
			}
			result += Environment.NewLine;
			result += "EndVCS" + Environment.NewLine;
			return result;
		}
		public string ToClipboard()
		{
			string result = DateAdded + Environment.NewLine + "-" + Notes;
			foreach (TodoItem td in CompletedTodos)
			{
				result += Environment.NewLine + "--" + td.ToClipboard();
			}
			return result;
		}
	}
}