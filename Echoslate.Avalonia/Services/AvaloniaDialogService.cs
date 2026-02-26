using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Platform.Storage;
using Echoslate.Avalonia.Windows;
using Echoslate.Core.Models;
using Echoslate.Core.Services;
using Echoslate.Core.ViewModels;

namespace Echoslate.Avalonia.Services;

public class AvaloniaDialogService : IDialogService {
	private Window _owner;
	private TopLevel _topLevel;
	public AvaloniaDialogService(Window owner) {
		_owner = owner;
		_topLevel = owner;
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
	public Task<bool> ShowWelcomeWindowAsync() {
		throw new System.NotSupportedException();
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
	public async Task<bool> ShowDialogAsync(object view, string title, Window owner) {
		Window window = new Window {
			Content = view,
			Title = title,
			WindowStartupLocation = WindowStartupLocation.CenterOwner,
			SizeToContent = SizeToContent.WidthAndHeight,
			ShowInTaskbar = false,
			CanResize = false
		};
		window.Opened += async (s, e) => {
			await Task.Delay(1); // tiny yield to let layout settle
			InputElement? firstFocusable = window.GetLogicalDescendants()
			   .OfType<InputElement>()
			   .FirstOrDefault(el => el.Focusable && el.IsVisible && el.IsEnabled);

			firstFocusable?.Focus();
		};

		bool? dialogResult = await window.ShowDialog<bool?>(owner);
		return dialogResult == true;
	}
	public async Task<T?> ShowDialogAsync<T>(object view, string title) {
		Window window = new Window {
			Content = view,
			Title = title,
			WindowStartupLocation = WindowStartupLocation.CenterOwner,
			SizeToContent = SizeToContent.WidthAndHeight,
			ShowInTaskbar = false,
			CanResize = false
		};

		T? dialogResult = await window.ShowDialog<T?>(_owner);
		return dialogResult;
	}
	public async Task<string?> OpenFile(string initialDirectory, string filter) {
		Log.Print($"Opening directory: {(string.IsNullOrEmpty(initialDirectory) ? "<null or empty>" : initialDirectory)}");
		IStorageFolder? suggestedFolder = await GetSuggestedStartLocation(initialDirectory);

		if (suggestedFolder == null) {
			Log.Error("Could not get a valid folder...");
			return null;
		}

		Log.Print($"Found directory at: {suggestedFolder.Path}");
		FilePickerOpenOptions options = new FilePickerOpenOptions {
			Title = "Open Echoslate Project",
			SuggestedStartLocation = suggestedFolder,
			AllowMultiple = false,
			FileTypeFilter = new List<FilePickerFileType> {
				new FilePickerFileType("Echoslate Project") {
					Patterns = new[] {
						"*.echoslate"
					}
				},
				new FilePickerFileType("JSON File") {
					Patterns = new[] {
						"*.json"
					}
				},
				new FilePickerFileType("All Files") {
					Patterns = new[] {
						"*.*"
					}
				}
			}
		};
		Log.Print("Opening file picker...");
		Task<IReadOnlyList<IStorageFile>> filesTask = _topLevel.StorageProvider.OpenFilePickerAsync(options);
		IReadOnlyList<IStorageFile> files = await filesTask;
		// IReadOnlyList<IStorageFile> files = _topLevel.StorageProvider.OpenFilePickerAsync(options).Result;

		if (files.FirstOrDefault() is { } file) {
			Log.Print($"Found file at: {file.Path?.LocalPath}");
			return file.Path?.LocalPath;
		}
		Log.Print("No file found.");
		return null;
	}

	public async Task<IStorageFolder> GetSuggestedStartLocation(string initialDirectory = "") {
		if (!string.IsNullOrEmpty(initialDirectory)) {
			Log.Print($"Getting path at: {initialDirectory}");
			return await _topLevel.StorageProvider.TryGetFolderFromPathAsync(initialDirectory);
		} else {
			string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			Log.Warn($"initialDirectory is empty. Getting default directory at: {folder.ToString()}");
			if (_topLevel == null) {
				Log.Error("_topLevel is null!");
				return null;
			}
			if (_topLevel.StorageProvider == null) {
				Log.Error("StorageProvider is null!");
				return null;
			}
			Task<IStorageFolder?> resultTask = _topLevel.StorageProvider.TryGetFolderFromPathAsync(folder);
			IStorageFolder? folderResult = await resultTask;
			if (folderResult == null) {
				Log.Error("TryGetFolderFromPathAsync returned null!");
			} else {
				Log.Print($"Suggested folder path: {folderResult.Path?.LocalPath ?? "null"}");
			}
			return folderResult;
		}
	}
	public async Task<string?> SaveFile(string defaultName = "New Project.echoslate", string initialDirectory = "", string filter = "Echoslate files (*.echoslate)|*.echoslate") {
		Log.Print($"Opening directory: {(string.IsNullOrEmpty(initialDirectory) ? "<null or empty>" : initialDirectory)}");
		IStorageFolder? suggestedFolder = await GetSuggestedStartLocation(initialDirectory);

		if (suggestedFolder == null) {
			Log.Error("Could not get a valid folder...");
			return null;
		}

		Log.Print($"Found directory at: {suggestedFolder.Path}");
		FilePickerSaveOptions options = new FilePickerSaveOptions {
			Title = "Save Echoslate Project",
			SuggestedStartLocation = suggestedFolder,
			SuggestedFileName = defaultName,
			DefaultExtension = "echoslate",
			FileTypeChoices = new List<FilePickerFileType> {
				new FilePickerFileType("Echoslate Project") {
					Patterns = new[] {
						"*.echoslate"
					}
				}
			}
		};
		Log.Print("Opening file picker...");
		IStorageFile? file = await _topLevel.StorageProvider.SaveFilePickerAsync(options);

		if (file != null) {
			Log.Print($"Save file name: {file.Path?.LocalPath}");
			return file.Path?.LocalPath;
		}
		Log.Print("No file found.");
		return null;
	}

	public string? ChooseFolder(string initialDirectory = "", string description = "Select Folder") {
		FolderPickerOpenOptions options = new FolderPickerOpenOptions {
			Title = "Select Folder"
		};

		if (!string.IsNullOrEmpty(initialDirectory)) {
			options.SuggestedStartLocation = _topLevel.StorageProvider.TryGetFolderFromPathAsync(initialDirectory).GetAwaiter().GetResult();
		}

		IReadOnlyList<IStorageFolder>? folder = _topLevel.StorageProvider.OpenFolderPickerAsync(options).GetAwaiter().GetResult();
		return folder?.FirstOrDefault().Path?.LocalPath;
	}
	public DialogResult Show(string message, string title, DialogButton buttons, DialogIcon icon) {
		MessageWindowViewModel vm = new MessageWindowViewModel(message, title, buttons, icon);
		MessageWindow view = new MessageWindow(vm);
		Window window = new Window {
			Content = view,
			Title = title,
			WindowStartupLocation = WindowStartupLocation.CenterOwner,
			SizeToContent = SizeToContent.WidthAndHeight,
			ShowInTaskbar = false,
			CanResize = false,
			Focusable = true,
			IsEnabled = true
		};

		window.Show();
		return vm.Result;
	}
	public async Task<DialogResult?> ShowAsync(string message, string title, DialogButton buttons, DialogIcon icon, object? owner = null) {
		Window? windowOwner = owner as Window;
		MessageWindowViewModel vm = new MessageWindowViewModel(message, title, buttons, icon);
		MessageWindow view = new MessageWindow(vm);
		Window window = new Window {
			Content = view,
			Title = title,
			WindowStartupLocation = WindowStartupLocation.CenterOwner,
			SizeToContent = SizeToContent.WidthAndHeight,
			ShowInTaskbar = false,
			CanResize = false,
			Focusable = true,
			IsEnabled = true
		};

		if (windowOwner != null) {
			Task task = window.ShowDialog(windowOwner);
			await task;
		} else {
			window.Show();
		}

		return vm.Result;
	}
}