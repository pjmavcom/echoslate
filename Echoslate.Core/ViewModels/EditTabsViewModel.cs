using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Echoslate.Core.Resources;

namespace Echoslate.Core.ViewModels;

public class EditTabsViewModel : INotifyPropertyChanged {
	private readonly List<string> _newTabItemList;
	private ObservableCollection<string> _filterNames;
	public ObservableCollection<string> FilterNames {
		get => _filterNames;
		set {
			_filterNames = value;
			OnPropertyChanged();
		}
	}
	private ObservableCollection<string> _selectedItems;
	public ObservableCollection<string> SelectedItems {
		get => _selectedItems;
		set {
			_selectedItems = value;
			OnPropertyChanged();
		}
	}

	private string _newTabName;
	public string NewTabName {
		get => _newTabName;
		set {
			_newTabName = value;
			OnPropertyChanged();
		}
	}
	public bool Result;
	private List<string> _resultList;
	public ObservableCollection<string> ResultList;


	public EditTabsViewModel(IEnumerable<string> filterNames) {
		SelectedItems = new ObservableCollection<string>();
		SelectedItems.CollectionChanged += TestChanged;
		FilterNames = new ObservableCollection<string>(filterNames);
		FilterNames.Remove("All");
	}
	private void TestChanged(object? sender, NotifyCollectionChangedEventArgs e) {
	}
	public ICommand NewTabCommand => new RelayCommand(() => {
		FilterNames.Add(NewTabName.CapitalizeFirstLetter());
		NewTabName = string.Empty;
	});
	public ICommand DeleteCommand => new RelayCommand(() => {
		var list = SelectedItems.ToList();
		foreach (string s in list) {
			if (FilterNames.Contains(s)) {
				FilterNames.Remove(s);
			}
		}
	});
	public List<string> MoveSelectedItemsUp() {
		HashSet<string> selectedSet = SelectedItems.ToHashSet();
		List<string> listToRemove = FilterNames.Where(selectedSet.Contains).ToList();

		int bufferIndex = 0;
		foreach (string s in listToRemove) {
			int index = FilterNames.IndexOf(s);
			if (index <= bufferIndex) {
				bufferIndex++;
				continue;
			}
			(FilterNames[index], FilterNames[index - 1]) = (FilterNames[index - 1], FilterNames[index]);
		}
		return listToRemove;
	}
	public List<string> MoveSelectedItemsDown() {
		HashSet<string> selectedSet = SelectedItems.Cast<string>().ToHashSet();
		List<string> listToRemove = FilterNames.Where(selectedSet.Contains).ToList();
		listToRemove.Reverse();

		int bufferIndex = FilterNames.Count - 1;
		foreach (string s in listToRemove) {
			int index = FilterNames.IndexOf(s);
			if (index >= bufferIndex) {
				bufferIndex--;
				continue;
			}
			(FilterNames[index], FilterNames[index + 1]) = (FilterNames[index + 1], FilterNames[index]);
		}
		listToRemove.Reverse();
		return listToRemove;
	}
	public void OnOk() {
		_resultList = [];
		_resultList.AddRange(FilterNames);
		ResultList = new ObservableCollection<string>(_resultList);
		Result = true;
	}


	public event PropertyChangedEventHandler? PropertyChanged;
	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
	protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
		if (EqualityComparer<T>.Default.Equals(field, value)) return false;
		field = value;
		OnPropertyChanged(propertyName);
		return true;
	}
}