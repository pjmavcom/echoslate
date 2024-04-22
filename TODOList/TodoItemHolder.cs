/*	TodoListHolder.cs
 * 20-Mar-2019
 * 11:46:32
 *
 * 
 */

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TODOList
{
	public class TodoItemHolder : INotifyPropertyChanged
	{
		// FIELDS //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// FIELDS //
		private TodoItem _td;
		private DateTime _timeTaken;
		private long _timeTakenInMinutes;
		
		// PROPERTIES //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// PROPERTIES //
		public TodoItem TD
		{
			get => _td;
			set => _td = value;
		}
		public int Rank { get; set; }
		public string Todo => _td.Todo;
		public string NotesAndTags => _td.NotesAndTags;
		public string TagsSorted => _td.TagsSorted;
		public string StartDateTime => _td.StartDateTime;
		public int Severity
		{
			get => _td.Severity;
			set => _td.Severity = value;
		}

		public int Kanban
		{
			get => _td.Kanban;
			set => _td.Kanban = value;
		}
		public bool IsTimerOn => _td.IsTimerOn;
		public long TimeTakenInMinutes
		{
			get => _td.TimeTakenInMinutes; 
//			set
//			{
//				imeTakenInMinutes = value;
//				OnPropertyChanged();
//			}
		}
		public DateTime TimeTaken
		{
			get => _td.TimeTaken;//_timeTaken;
			set
			{
				_td.TimeTaken = value;
//				TimeTakenInMinutes = _timeTaken.Ticks / TimeSpan.TicksPerMinute;
				OnPropertyChanged();
			}
		}
		public string TimeStarted => TD.TimeStarted;
		public string DateStarted => TD.DateStarted;
		
		// CONSTRUCTORS //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// CONSTRUCTORS //
		public TodoItemHolder(TodoItem td)
		{
			_td = td;
			Rank = int.MaxValue;
		}

		// METHODS ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// METHODS //
		public override string ToString()
		{
			string result = "";
			result += Rank + " | " + TD.Ranks;
			return result;
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
