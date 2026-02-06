using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Input;
using Echoslate.Core.Models;
using Echoslate.Core.ViewModels;

namespace Echoslate.Avalonia.Views;

public partial class ItemNotesPanelView : UserControl, INotifyPropertyChanged {
	public static readonly StyledProperty<TodoItem?> SelectedItemProperty =
		AvaloniaProperty.Register<ItemNotesPanelView, TodoItem?>(nameof(SelectedItem));

	public TodoItem? SelectedItem {
		get => GetValue(SelectedItemProperty);
		set => SetValue(SelectedItemProperty, value);
	}

	public ItemNotesPanelView() {
		InitializeComponent();
	}
	static ItemNotesPanelView() {
		SelectedItemProperty.Changed.AddClassHandler<ItemNotesPanelView>((control, change) => {
			var oldItem = change.OldValue;
			var newItem = change.NewValue;
		});
	}
	private void InitializeComponent() {
		AvaloniaXamlLoader.Load(this);
	}
	// private void RefreshAll() {
		// if (VisualTreeHelper.GetParent(this) is DependencyObject parent) {
		// var todoListView = this.TryFindParent<TodoDisplayView>();
		// if (todoListView?.DataContext is TodoListViewModel vm) {
		// vm.RefreshAll();
		// }
		// }
	// }

	public ICommand NotesPanelCompleteCommand => new RelayCommand(() => RaiseEvent(new RoutedEventArgs(NotesPanelCompleteRequestedEvent)));

	public static readonly RoutedEvent NotesPanelCompleteRequestedEvent =
		RoutedEvent.Register<ItemNotesPanelView, RoutedEventArgs>(nameof(NotesPanelCompleteRequested), RoutingStrategies.Bubble);

	public event EventHandler<RoutedEventArgs>? NotesPanelCompleteRequested {
		add => AddHandler(NotesPanelCompleteRequestedEvent, value);
		remove => RemoveHandler(NotesPanelCompleteRequestedEvent, value);
	}

	public ICommand NotesPanelAddTagCommand => new RelayCommand(() => RaiseEvent(new RoutedEventArgs(NotesPanelEditTagsRequestedEvent)));

	public static readonly RoutedEvent NotesPanelEditTagsRequestedEvent =
		RoutedEvent.Register<ItemNotesPanelView, RoutedEventArgs>(nameof(NotesPanelEditTagsRequested), RoutingStrategies.Bubble);

	public event EventHandler<RoutedEventArgs>? NotesPanelEditTagsRequested {
		add => AddHandler(NotesPanelEditTagsRequestedEvent, value);
		remove => RemoveHandler(NotesPanelEditTagsRequestedEvent, value);
	}

	// public ICommand CommitTitleEditCommand => new RelayCommand<TodoItem>(ih => {
	// var binding = BindingOperations.GetBindingExpression(tbTodo, TextBox.TextProperty);
	// binding?.UpdateSource();
	// });
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