using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Echoslate.Core.Models;

namespace Echoslate.Core.ViewModels;

public class TagPickerViewModel : INotifyPropertyChanged {
	private ObservableCollection<string> _allAvailableTags;
	public ObservableCollection<string> AllAvailableTags {
		get => _allAvailableTags;
		set {
			_allAvailableTags = value;
			OnPropertyChanged();
		}
	}
	public IEnumerable AllAvailableTagsView {
		get {
			var items = AllAvailableTags.OrderBy(t => t, StringComparer.OrdinalIgnoreCase);
			return items;
		}
	}
	private ObservableCollection<string> _selectedTags;
	public ObservableCollection<string> SelectedTags {
		get => _selectedTags;
		set {
			_selectedTags = value;
			OnPropertyChanged();
		}
	}
	private string _newTagName;
	public string NewTagName {
		get => _newTagName;
		set {
			_newTagName = value;
			OnPropertyChanged();
		}
	}
	private List<TodoItem> _selectedTodoItems;
	public List<TodoItem> SelectedTodoItems {
		get => _selectedTodoItems;
		set {
			_selectedTodoItems = value;
			OnPropertyChanged();
		}
	}

	public bool Result;


	public TagPickerViewModel(List<TodoItem> todoItems, ObservableCollection<string> allTags, ObservableCollection<string> selectedTags) {
		SelectedTodoItems = todoItems;
		AllAvailableTags = allTags;
		SelectedTags = selectedTags;
	}
	public void NewTag() {
		if (string.IsNullOrWhiteSpace(NewTagName)) {
			return;
		}

		string newTag = NewTagName.ToUpper();
		if (!newTag.StartsWith("#")) {
			newTag = "#" + newTag;
		}

		AllAvailableTags.Add(newTag);
		OnPropertyChanged(nameof(AllAvailableTagsView));
		SelectedTags.Add(newTag);
		OnPropertyChanged(nameof(SelectedTags));
		NewTagName = "";
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