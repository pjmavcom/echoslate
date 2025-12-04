/*	TodoListHolder.cs
 * 20-Mar-2019
 * 11:46:32
 *
 * 
 */

using System;
using System.Collections.ObjectModel;
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
		public ObservableCollection<string> Tags => _td.Tags;
		public string FirstTag => Tags.Count > 0 ? Tags[0] : "";
		
		public string SeverityName
		{
			get {
				switch (_td.Severity) {
					case 0:
						return "None";
					case 1:
						return "Low";
					case 2:
						return "Medium";
					case 3:
						return "High";
					default:
						return "Unknown";
				}
			}
		}
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
		public int KanbanRank
		{
			get => _td.KanbanRank;
			set => _td.KanbanRank = value;
		}
		public bool IsTimerOn => _td.IsTimerOn;
		public long TimeTakenInMinutes => _td.TimeTakenInMinutes;
		public string TimeTakenDisplay => $"{TimeTakenInMinutes:00.##} : {TimeTaken.Second:00}";
		public DateTime TimeTaken
		{
			get => _td.TimeTaken;
			set
			{
				_td.TimeTaken = value;
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
		public bool HasTag(string tag) {
			return Tags.Contains(tag);
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
