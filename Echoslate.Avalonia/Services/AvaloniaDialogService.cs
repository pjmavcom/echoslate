using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using Echoslate.Avalonia.Windows;
using Echoslate.Core.Models;
using Echoslate.Core.Services;
using Echoslate.Core.ViewModels;

namespace Echoslate.Avalonia.Services;

public class AvaloniaDialogService : IDialogService {
	private Window _owner;
	public AvaloniaDialogService(Window owner) {
		_owner = owner;
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
		throw new System.NotImplementedException();
	}
	public Task<TagPickerViewModel?> ShowTagPickerAsync(List<TodoItem> todoItems, ObservableCollection<string> allTags, List<string> selectedTags) {
		throw new System.NotImplementedException();
	}
	public Task<TodoItemEditorViewModel?> ShowTodoItemEditorAsync(TodoItem td, string? currentListHash, ObservableCollection<string> allAvailableTags) {
		throw new System.NotImplementedException();
	}
	public Task<TodoMultiItemEditorViewModel?> ShowTodoMultiItemEditorAsync(List<TodoItem> items, string currentFilter) {
		throw new System.NotImplementedException();
	}
	public Task<EditTabsViewModel?> ShowEditTabsAsync(IEnumerable<string> filterNames) {
		throw new System.NotImplementedException();
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
	public string? OpenFile(string initialDirectory = "", string filter = "Echoslate files (*.echoslate)|*.echoslate") {
		throw new System.NotImplementedException();
	}
	public string? SaveFile(string defaultName = "New Project.echoslate", string initialDirectory = "", string filter = "Echoslate files (*.echoslate)|*.echoslate") {
		throw new System.NotImplementedException();
	}
	public string? ChooseFolder(string initialDirectory = "", string description = "Select Folder") {
		throw new System.NotImplementedException();
	}
	public DialogResult Show(string message, string title, DialogButton dialogButton, DialogIcon dialogIcon) {
		throw new System.NotImplementedException();
	}
}