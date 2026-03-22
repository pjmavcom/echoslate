using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Echoslate.Core.Models;

namespace Echoslate.Avalonia.Windows;

public partial class QuickReminderWindow : UserControl, INotifyPropertyChanged {
	private ReminderInfo _reminder;
	public ReminderInfo Reminder {
		get => _reminder;
		set {
			if (_reminder == value) {
				return;
			}
			_reminder = value;
			OnPropertyChanged();
		}
	}
	private TodoItem? _item;
	public TodoItem? Item {
		get => _item;
		set {
			if (_item == value) {
				return;
			}
			_item = value;
			OnPropertyChanged();
		}
	}
	private DateTime _selectedDate;
	public DateTime SelectedDate {
		get => _selectedDate;
		set {
			if (_selectedDate == value) {
				return;
			}
			_selectedDate = value;
			OnPropertyChanged();
		}
	}
	private int _dueHour;
	public int DueHour {
		get => _dueHour;
		set {
			if (_dueHour == value) {
				return;
			}
			_dueHour = value;
			OnPropertyChanged();
		}
	}
	private int _dueMinute;
	public int DueMinute {
		get => _dueMinute;
		set {
			if (_dueMinute == value) {
				return;
			}
			_dueMinute = value;
			OnPropertyChanged();
		}
	}
	private DateTime _dueDate;
	public DateTime DueDate {
		get => _dueDate;
		set {
			if (_dueDate == value) {
				return;
			}
			_dueDate = value;
			OnPropertyChanged();
		}
	}
	private string _message;
	public string Message {
		get => _message;
		set {
			if (_message == value) {
				return;
			}
			_message = value;
			OnPropertyChanged();
		}
	}

	public QuickReminderWindow() {
		InitializeComponent();
		DataContext = this;
	}
	public QuickReminderWindow(TodoItem item = null) {
		InitializeComponent();
		DataContext = this;
		Item = item;
		SetDueDateNow();
	}
	private void SetDueDateNow_OnClick(object? sender, RoutedEventArgs e) {
		SetDueDateNow();
	}
	private void SetDueDateNow() {
		DueHour = (DateTime.Now.TimeOfDay + new TimeSpan(0, 15, 0)).Hours;
		DueMinute = ((DateTime.Now.TimeOfDay + new TimeSpan(0, 15, 0)).Minutes) / 15 * 15;
		TimeOnly time = new TimeOnly(DueHour, DueMinute);
		DateOnly date = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
		SelectedDate = new DateTime(date, time);
		OnPropertyChanged(nameof(SelectedDate));
		OnPropertyChanged(nameof(DueHour));
		OnPropertyChanged(nameof(DueMinute));
	}
	private void SaveChanges_OnClick(object? sender, RoutedEventArgs e) {
		DueDate = new DateTime(SelectedDate.Year, SelectedDate.Month, SelectedDate.Day, DueHour, DueMinute, 0);
		Reminder = new ReminderInfo {
			DueDate = DueDate,
			Message = Message
		};

		if (Item != null) {
			Item.ReminderGuids.Add(Reminder.Guid);
			Item.Reminders.Add(Reminder);
			Reminder.TodoGuids.Add(Item.Guid);
			Reminder.Todos.Add(Item);
		}

		if (Parent is Window window) {
			window.Close(Reminder);
		}
	}
	private void Cancel_OnClick(object? sender, RoutedEventArgs e) {
		if (Parent is Window window) {
			window.Close();
		}
	}


	public event PropertyChangedEventHandler? PropertyChanged;
	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}