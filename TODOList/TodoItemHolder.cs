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
		private int _rank;
		private DateTime _timeTaken;
		private long _timeTakenInMinutes;
		

		// PROPERTIES //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// PROPERTIES //
		public TodoItem TD
		{
			get => _td;
			set => _td = value;
		}
		public int Rank
		{
			get => _rank;
			set => _rank = value;
		}
		public string Todo => _td.Todo;
		public string NotesAndTags => _td.NotesAndTags;
		public string TagsSorted => _td.TagsSorted;
		public string StartDateTime => _td.StartDateTime;
		public int Severity
		{
			get => _td.Severity;
			set => _td.Severity = value;
		}
//		public long TimeTakenInMinutes => _td.TimeTakenInMinutes;
		public bool IsTimerOn => _td.IsTimerOn;
//		public DateTime TimeTaken => _td.TimeTaken;
		public long TimeTakenInMinutes
		{
			get => _timeTakenInMinutes; 
			set
			{
				_timeTakenInMinutes = value;
				OnPropertyChanged();
			}
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
		public string TimeStarted => TD.TimeStarted;
		public string DateStarted => TD.DateStarted;
		
		
		// CONSTRUCTORS //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// CONSTRUCTORS //
		public TodoItemHolder(TodoItem td)
		{
			_td = td;
			_rank = int.MaxValue;
		}

		// MONOGAME METHODS //////////////////////////////////////////////////////////////////////////////////////////////////////////////// MONOGAME METHODS //


		// METHODS ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// METHODS //
		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

	}
}