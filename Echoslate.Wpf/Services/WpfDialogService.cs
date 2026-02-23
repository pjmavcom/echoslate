using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Forms;
using Echoslate.Core.Models;
using Echoslate.Core.Services;
using Echoslate.Core.ViewModels;
using Echoslate.Wpf.Windows;
using DialogResult = Echoslate.Core.Services.DialogResult;
using MessageBox = System.Windows.MessageBox;


namespace Echoslate.Wpf.Services;

public class WpfDialogService : IDialogService {
	private Window _owner;

	public WpfDialogService(Window owner) {
		_owner = owner;
	}
	public async Task<bool> ShowAboutAsync(string version) {
		return await ShowDialogAsync(new AboutWindow(version), "About Echoslate");
	}
	public async Task<bool> ShowHelpAsync() {
		return await ShowDialogAsync(new HelpWindow(), "Hotkey Help");
	}
	public async Task<OptionsViewModel?> ShowOptionsAsync(AppSettings appSettings, AppData appData) {
		OptionsViewModel vm = new OptionsViewModel(appSettings, appData);
		OptionsWindow view = new OptionsWindow(vm);
		return await ShowDialogAsync<OptionsViewModel>(view, "Options");
	}
	public async Task<bool> ShowWelcomeWindowAsync() {
		WelcomeViewModel vm = new WelcomeViewModel();
		WelcomeWindow view = new WelcomeWindow(vm);
		return await ShowDialogAsync(view, "Welcome to Echoslate", null);
	}
	public async Task<TagPickerViewModel?> ShowTagPickerAsync(List<TodoItem> todoItems, ObservableCollection<string> allTags, ObservableCollection<string> selectedTags) {
		TagPickerViewModel vm = new TagPickerViewModel(todoItems, allTags, selectedTags);
		TagPickerWindow view = new TagPickerWindow(vm);
		return await ShowDialogAsync<TagPickerViewModel>(view, "Tag Picker");
	}
	public async Task<TodoItemEditorViewModel?> ShowTodoItemEditorAsync(TodoItem td, string? currentListHash, ObservableCollection<string> allAvailableTags) {
		TodoItemEditorViewModel vm = new TodoItemEditorViewModel(td, currentListHash, allAvailableTags);
		TodoItemEditorWindow view = new TodoItemEditorWindow(vm);
		return await ShowDialogAsync<TodoItemEditorViewModel>(view, "Edit Todo Item");
	}
	public async Task<TodoMultiItemEditorViewModel?> ShowTodoMultiItemEditorAsync(List<TodoItem> items, string currentFilter, ObservableCollection<string> allAvailableTags) {
		TodoMultiItemEditorViewModel vm = new TodoMultiItemEditorViewModel(items, currentFilter, allAvailableTags);
		TodoMultiItemEditorWindow view = new TodoMultiItemEditorWindow(vm);
		return await ShowDialogAsync<TodoMultiItemEditorViewModel>(view, "Edit Multiple Todo Items");
	}
	public async Task<EditTabsViewModel?> ShowEditTabsAsync(IEnumerable<string> filterNames) {
		EditTabsViewModel vm = new EditTabsViewModel(filterNames);
		EditTabsWindow view = new EditTabsWindow(vm);
		return await ShowDialogAsync<EditTabsViewModel>(view, "Edit Tabs");
	}
	public async Task<ChooseDraftViewModel?> ShowChooseDraftAsync(IEnumerable<HistoryItem> drafts, HistoryItem defaultDraft = null) {
		ChooseDraftViewModel vm = new ChooseDraftViewModel(drafts, defaultDraft);
		ChooseDraftWindow view = new ChooseDraftWindow(vm);
		return await ShowDialogAsync<ChooseDraftViewModel>(view, "Append to which draft?");
	}
	public Task<bool> ShowDialogAsync(object view, string title) {
		return ShowDialogAsync(view, title, _owner);
	}
	public Task<bool> ShowDialogAsync(object view, string title, Window owner) {
		var window = new Window {
			Content = view,
			Title = title,
			Owner = owner,
			WindowStartupLocation = WindowStartupLocation.CenterOwner,
			SizeToContent = SizeToContent.WidthAndHeight,
			ResizeMode = ResizeMode.NoResize,
			ShowInTaskbar = false
		};

		bool? dialogResult = window.ShowDialog();
		return Task.FromResult(dialogResult == true);
	}
	public Task<T?> ShowDialogAsync<T>(object view, string title = "Dialog") {
		var window = new Window {
			Content = view,
			Title = title,
			Owner = _owner,
			WindowStartupLocation = WindowStartupLocation.CenterOwner,
			SizeToContent = SizeToContent.WidthAndHeight,
			ResizeMode = ResizeMode.NoResize,
			ShowInTaskbar = false
		};

		bool? dialogResult = window.ShowDialog();

		if (dialogResult == true && view is FrameworkElement fe && fe.DataContext != null) {
			if (fe.DataContext is T vm) {
				return Task.FromResult(vm);
			} else {
				var prop = fe.DataContext.GetType().GetProperty("Result");
				if (prop != null && prop.PropertyType is T result) {
					return Task.FromResult(result);
				}
			}
		}

		return Task.FromResult(default(T));
	}
	public async Task<string?> OpenFile(string initialDirectory, string filter) {
		Log.Print($"Opening directory: {(string.IsNullOrEmpty(initialDirectory) ? "<null or empty>" : initialDirectory)}");
		string suggestedFolder = GetSuggestedStartLocation(initialDirectory);

		if (string.IsNullOrEmpty(suggestedFolder)) {
			Log.Error("Could not get a valid folder...");
			return null;
		}

		Log.Print($"Found directory at: {suggestedFolder}");
		
		var dialog = new OpenFileDialog {
			Filter = filter,
			InitialDirectory = suggestedFolder
		};

		Log.Print("Opening file picker...");
		return dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK ? dialog.FileName : null;
	}
	public string GetSuggestedStartLocation(string initialDirectory = "") {
		if (!string.IsNullOrEmpty(initialDirectory) && Directory.Exists(initialDirectory)) {
			Log.Print($"Getting path at: {initialDirectory}");
			return initialDirectory;
		}
		var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
		Log.Warn($"initialDirectory is empty. Getting default directory at: {folder.ToString()}");

		if (string.IsNullOrEmpty(folder)) {
			Log.Error("Could not find default directory!");
		} else {
			Log.Print($"Suggested folder path: {folder}");
		}
		return folder;
	}

	public async Task<string?> SaveFile(string defaultName, string initialDirectory, string filter) {
		var dialog = new SaveFileDialog {
			Filter = filter,
			FileName = defaultName,
			InitialDirectory = initialDirectory
		};

		return dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK ? dialog.FileName : null;
	}

	public string? ChooseFolder(string initialDirectory, string description) {
		var dialog = new FolderBrowserDialog {
			Description = description,
			UseDescriptionForTitle = true,
			SelectedPath = initialDirectory ?? string.Empty,
			ShowNewFolderButton = true
		};

		if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
			return dialog.SelectedPath;
		}

		return null;
	}

	public DialogResult Show(string message, string title, DialogButton buttons, DialogIcon icon) {
		MessageBoxButton wpfButtons = buttons switch {
			DialogButton.Ok => MessageBoxButton.OK,
			DialogButton.OkCancel => MessageBoxButton.OKCancel,
			DialogButton.YesNo => MessageBoxButton.YesNo,
			DialogButton.YesNoCancel => MessageBoxButton.YesNoCancel,
			_ => MessageBoxButton.OK
		};

		MessageBoxImage wpfIcon = icon switch {
			DialogIcon.Information => MessageBoxImage.Information,
			DialogIcon.Question => MessageBoxImage.Question,
			DialogIcon.Warning => MessageBoxImage.Warning,
			DialogIcon.Error => MessageBoxImage.Error,
			_ => MessageBoxImage.None
		};

		var wpfResult = MessageBox.Show(message, title, wpfButtons, wpfIcon);

		return wpfResult switch {
			MessageBoxResult.OK => DialogResult.Ok,
			MessageBoxResult.Cancel => DialogResult.Cancel,
			MessageBoxResult.Yes => DialogResult.Yes,
			MessageBoxResult.No => DialogResult.No,
			_ => DialogResult.None
		};
	}

}