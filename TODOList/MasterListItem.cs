/*	MasterListItem.cs
 * 22-Mar-2019
 * 16:06:14
 *
 * 
 */

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TODOList
{
	public class MasterListItem : INotifyPropertyChanged
	{
		// FIELDS //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// FIELDS //
		private TodoItem _td;
		private List<int> _listRanks;

		// PROPERTIES //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// PROPERTIES //
		public TodoItem TD
		{
			get => _td;
			set
			{
				_td = value;
				OnPropertyChanged();
			}
		}
		public List<int> ListRanks
		{
			get => _listRanks;
			set
			{
				_listRanks = value;
				OnPropertyChanged();
			}
		}

		// CONSTRUCTORS //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// CONSTRUCTORS //
		public MasterListItem(TodoItem td)
		{
			_td = td;
			_listRanks = new List<int>();
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