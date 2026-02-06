using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
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

	public async Task<bool> ShowAboutAsync() {
		return await ShowDialogAsync(new AboutWindow(), "About Echoslate");
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
	public Task<TodoItemEditorViewModel?> ShowTodoItemEditorAsync(TodoItem td, string? currentListHash, ObservableCollection<string> allAvailableTags) {
		throw new System.NotImplementedException();
	}
	public Task<TodoMultiItemEditorViewModel?> ShowTodoMultiItemEditorAsync(List<TodoItem> items, string currentFilter) {
		throw new System.NotImplementedException();
	}
	public async Task<EditTabsViewModel?> ShowEditTabsAsync(IEnumerable<string> filterNames) {
		EditTabsViewModel vm = new EditTabsViewModel(filterNames);
		EditTabsWindow view = new EditTabsWindow(vm);
		return await ShowDialogAsync<EditTabsViewModel>(view, "Edit Tabs");
	}
	public Task<ChooseDraftViewModel?> ShowChooseDraftAsync(IEnumerable<HistoryItem> drafts, HistoryItem defaultDraft = null) {
		throw new System.NotImplementedException();
	}
	public Task<bool> ShowDialogAsync(object view, string title) {
		return ShowDialogAsync(view, title, _owner);
	}
	public async Task<bool> ShowDialogAsync(object view, string title, Window owner) {
		var window = new Window {
			Content = view,
			Title = title,
			WindowStartupLocation = WindowStartupLocation.CenterOwner,
			SizeToContent = SizeToContent.WidthAndHeight,
			ShowInTaskbar = false,
			CanResize = false
		};

		bool? dialogResult = await window.ShowDialog<bool?>(owner);
		return dialogResult == true;
	}
	public async Task<T?> ShowDialogAsync<T>(object view, string title) {
		var window = new Window {
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
	public string OpenFile(string initialDirectory, string filter) {
		IStorageFolder? suggestedFolder = GetSuggestedStartLocation(initialDirectory).Result;

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
		IReadOnlyList<IStorageFile> files = _topLevel.StorageProvider.OpenFilePickerAsync(options).Result;

		if (files.FirstOrDefault() is { } file) {
			return file.Path?.LocalPath;
		}
		return null;
	}

	public async Task<IStorageFolder> GetSuggestedStartLocation(string initialDirectory = "") {
		if (!string.IsNullOrEmpty(initialDirectory)) {
			return await _topLevel.StorageProvider.TryGetFolderFromPathAsync(initialDirectory);
		} else {
			return await _topLevel.StorageProvider.TryGetFolderFromPathAsync(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
		}
	}
	public string? SaveFile(string defaultName = "New Project.echoslate", string initialDirectory = "", string filter = "Echoslate files (*.echoslate)|*.echoslate") {
		IStorageFolder? suggestedFolder = GetSuggestedStartLocation(initialDirectory).Result;

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
		IStorageFile? file = _topLevel.StorageProvider.SaveFilePickerAsync(options).Result;
		return file.Path?.LocalPath;
	}

	public string? ChooseFolder(string initialDirectory = "", string description = "Select Folder") {
		var options = new FolderPickerOpenOptions {
			Title = "Select Folder"
		};

		if (!string.IsNullOrEmpty(initialDirectory)) {
			options.SuggestedStartLocation = _topLevel.StorageProvider.TryGetFolderFromPathAsync(initialDirectory).GetAwaiter().GetResult();
		}

		IReadOnlyList<IStorageFolder>? folder = _topLevel.StorageProvider.OpenFolderPickerAsync(options).GetAwaiter().GetResult();
		return folder?.FirstOrDefault().Path?.LocalPath;
	}
	public DialogResult Show(string message, string title, DialogButton buttons, DialogIcon icon) {
		var vm = new MessageWindowViewModel(message, title, buttons, icon);
		var view = new MessageWindow(vm);
		var window = new Window {
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
}