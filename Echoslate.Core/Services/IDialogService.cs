using System.Collections.ObjectModel;
using Echoslate.Core.Models;
using Echoslate.Core.ViewModels;

namespace Echoslate.Core.Services;

public enum DialogResult {
	None,
	Ok,
	Cancel,
	Yes,
	No
}

public enum DialogButton {
	Ok,
	OkCancel,
	YesNo,
	YesNoCancel
}

public enum DialogIcon {
	None,
	Information,
	Question,
	Warning,
	Error
}

public interface IDialogService {
	Task<bool> ShowAboutAsync(string version);
	Task<bool> ShowHelpAsync();
	Task<OptionsViewModel?> ShowOptionsAsync(AppSettings appSettings, AppData appData);
	Task<bool> ShowWelcomeWindowAsync();
	Task<TagPickerViewModel?> ShowTagPickerAsync(List<TodoItem> todoItems, ObservableCollection<string> allTags, ObservableCollection<string> selectedTags);
	Task<TodoItemEditorViewModel?> ShowTodoItemEditorAsync(TodoItem td, string? currentListHash, ObservableCollection<string> allAvailableTags);
	Task<TodoMultiItemEditorViewModel?> ShowTodoMultiItemEditorAsync(List<TodoItem> items, string currentFilter, ObservableCollection<string> allAvailableTags);
	Task<EditTabsViewModel?> ShowEditTabsAsync(IEnumerable<string> filterNames);
	Task<ChooseDraftViewModel?> ShowChooseDraftAsync(IEnumerable<HistoryItem> drafts, HistoryItem defaultDraft = null);
	
	Task<bool> ShowDialogAsync(object view, string title);
	Task<T?> ShowDialogAsync<T>(object view, string title);

	Task<string?> OpenFile(string initialDirectory = "", string filter = "Echoslate files (*.echoslate)|*.echoslate");
	Task<string?> SaveFile(string defaultName = "New Project.echoslate", string initialDirectory = "", string filter = "Echoslate files (*.echoslate)|*.echoslate");
	string? ChooseFolder(string initialDirectory = "", string description = "Select Folder");
	
	DialogResult Show(string message, string title, DialogButton dialogButton, DialogIcon dialogIcon);
	
	
}