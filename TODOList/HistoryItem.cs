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
		private string _dateAdded;
		private string _timeAdded;
		private List<TodoItem> _completedTodos;

		// PROPERTIES //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// PROPERTIES //
		public string Notes => _notes;
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
		}
		public HistoryItem(string notes, List<TodoItem> completed)
		{
			_notes = notes;
			_completedTodos = completed;
		}

		// MONOGAME METHODS //////////////////////////////////////////////////////////////////////////////////////////////////////////////// MONOGAME METHODS //


		// METHODS ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// METHODS //
		// METHOD  ///////////////////////////////////// AddCompletedTodo() //
		public void AddCompletedTodo(TodoItem td)
		{
			_completedTodos.Add(td);
		}

	}
}