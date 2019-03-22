/*	TabItemHolder.cs
 * 22-Mar-2019
 * 16:01:46
 *
 * 
 */

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace TODOList
{
	public class TabItemHolder : INotifyPropertyChanged
	{
		private TabItem _ti;
		public bool _canMoveUp;
		public bool _canMoveDown;
		private int _currentIndex;
		private int _maxIndex;
		private Visibility _isVisible;

		public string Header
		{
			get => _ti.Header as string;
			set
			{
				_ti.Header = value;
				OnPropertyChanged();
			}
		}
		public string Name
		{
			get => _ti.Name;
			set 
			{
				_ti.Name = value;
				OnPropertyChanged();
			}
		}
		public bool CanMoveUp
		{
			get => _canMoveUp;
			set
			{
				_canMoveUp = value;
				OnPropertyChanged();
			}
		}
		public bool CanMoveDown
		{
			get => _canMoveDown;
			set
			{
				_canMoveDown = value;
				OnPropertyChanged();
			}
		}
		public int CurrentIndex
		{
			get => _currentIndex;
			set
			{
				CanMoveUp = true;
				CanMoveDown = true;
				Enabled = true;
				IsVisible = Visibility.Visible;
				
				if (value <= 0)
				{
					_currentIndex = 0;
					CanMoveUp = false;
					CanMoveDown = false;
					Enabled = false;
					IsVisible = Visibility.Hidden;
				}
				else if (value == 1)
				{
					_currentIndex = 1;
					CanMoveUp = false;
				}
				else if (value >= _maxIndex - 1)
				{
					_currentIndex = _maxIndex - 1;
					CanMoveDown = false;
				}
				else
				{
					_currentIndex = value;
				}
				OnPropertyChanged();
			}
		}
		public int MaxIndex
		{
			get => _maxIndex;
			set => _maxIndex = value;
		}
		public bool Enabled { get; set; }
		public Visibility IsVisible
		{
			get => _isVisible;
			set
			{
				_isVisible = value;
				OnPropertyChanged();
			}
		}
		public TabItemHolder(TabItem ti)
		{
			_ti = ti;
		}
		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}