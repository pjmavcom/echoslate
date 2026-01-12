using System.ComponentModel;
using System.Runtime.CompilerServices;

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

	public string DisplayTitle =>  Count > 0 ? $"{Filter} {Count}" : Filter;

	public FilterButton(string filter, int count) {
		Filter = filter;
		Count = count;
	}

	public event PropertyChangedEventHandler PropertyChanged;
	protected void OnPropertyChanged([CallerMemberName] string name = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}