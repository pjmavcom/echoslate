using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Echoslate.Core.Components;

public class FilterButton : INotifyPropertyChanged {
	public string Filter { get; }
	private int _count;
	public int Count {
		get => _count;
		set {
			if (_count == value) {
				return;
			}
			_count = value;
			OnPropertyChanged();
			OnPropertyChanged(nameof(DisplayTitle));
		}
	}
	private bool _isSelected;
	public bool IsSelected {
		get => _isSelected;
		set {
			if (_isSelected == value) {
				return;
			}
			_isSelected = value;
			OnPropertyChanged();
		}
	}
	
	public ICommand? SelectCommand { get; set; }

	public string DisplayTitle =>  Count > 0 ? $"{Filter} {Count}" : Filter;

	public FilterButton(string filter, int count, ICommand? selectCommand = null) {
		Filter = filter;
		Count = count;
		SelectCommand = selectCommand;
	}

	public event PropertyChangedEventHandler PropertyChanged;
	protected void OnPropertyChanged([CallerMemberName] string name = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}