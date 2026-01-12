using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using Echoslate.Core.Models;
using Echoslate.Core.ViewModels;
using Echoslate.Wpf.Resources;

namespace Echoslate.Wpf.Views;

public partial class ItemNotesPanelView : UserControl, INotifyPropertyChanged {
	public static readonly DependencyProperty SelectedTodoItemProperty =
		DependencyProperty.Register(nameof(SelectedTodoItem), typeof(TodoItem), typeof(ItemNotesPanelView), new PropertyMetadata(null));

	public TodoItem SelectedTodoItem {
		get => (TodoItem)GetValue(SelectedTodoItemProperty);

		set => SetValue(SelectedTodoItemProperty, value);
	}


	public ItemNotesPanelView() {
		InitializeComponent();
	}
	private void RefreshAll() {
		if (VisualTreeHelper.GetParent(this) is DependencyObject parent) {
			var todoListView = this.TryFindParent<TodoDisplayView>();
			if (todoListView?.DataContext is TodoListViewModel vm) {
				vm.RefreshAll();
			}
		}
	}

	public ICommand NotesPanelCompleteCommand => new RelayCommand(() => RaiseEvent(new RoutedEventArgs(NotesPanelCompleteRequestedEvent)));

	public static readonly RoutedEvent NotesPanelCompleteRequestedEvent =
		EventManager.RegisterRoutedEvent("NotesPanelCompleteRequested", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ItemNotesPanelView));

	public event RoutedEventHandler NotesPanelCompleteRequested {
		add => AddHandler(NotesPanelCompleteRequestedEvent, value);
		remove => RemoveHandler(NotesPanelCompleteRequestedEvent, value);
	}

	public ICommand NotesPanelAddTagCommand => new RelayCommand(() => RaiseEvent(new RoutedEventArgs(NotesPanelEditTagsRequestedEvent)));

	public static readonly RoutedEvent NotesPanelEditTagsRequestedEvent =
		EventManager.RegisterRoutedEvent("NotesPanelEditTagsRequested", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ItemNotesPanelView));

	public event RoutedEventHandler NotesPanelEditTagsRequested {
		add => AddHandler(NotesPanelEditTagsRequestedEvent, value);
		remove => RemoveHandler(NotesPanelEditTagsRequestedEvent, value);
	}

	public ICommand CommitTitleEditCommand => new RelayCommand<TodoItem>(ih => {
		var binding = BindingOperations.GetBindingExpression(tbTodo, TextBox.TextProperty);
		binding?.UpdateSource();
	});
	public event PropertyChangedEventHandler? PropertyChanged;
	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
	protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
		if (EqualityComparer<T>.Default.Equals(field, value)) {
			return false;
		}
		field = value;
		OnPropertyChanged(propertyName);
		return true;
	}
}