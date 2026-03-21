using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Echoslate.Core.Models;

namespace Echoslate.Core.ViewModels;

public class EnumOption {
	public RecurringFrequency Value { get; set; }
	public string Display { get; set; } = string.Empty;
}

public class ReminderEditorViewModel : INotifyPropertyChanged {
	public bool IsEnabled {
		get => SelectedReminder != null;
	}
	public bool IsAddTodoEnabled {
		get => SelectedTodo != null;
	}
	public bool IsRemoveTodoEnabled {
		get => SelectedAttachmentTodo != null;
	}

	private ObservableCollection<TodoItem> _todos;
	public ObservableCollection<TodoItem> Todos {
		get => _todos;
		set {
			if (_todos == value) {
				return;
			}
			_todos = value;
			OnPropertyChanged();
		}
	}
	private TodoItem? _selectedTodo;
	public TodoItem? SelectedTodo {
		get => _selectedTodo;
		set {
			if (_selectedTodo == value) {
				return;
			}
			_selectedTodo = value;
			OnPropertyChanged();
			OnPropertyChanged(nameof(IsAddTodoEnabled));
		}
	}
	private ObservableCollection<ReminderInfo> _reminders;
	public ObservableCollection<ReminderInfo> Reminders {
		get => _reminders;
		set {
			if (_reminders == value) {
				return;
			}
			_reminders = value;
			OnPropertyChanged();
		}
	}
	private ReminderInfo? _selectedReminder;
	public ReminderInfo? SelectedReminder {
		get => _selectedReminder;
		set {
			if (_selectedReminder == value) {
				return;
			}
			_selectedReminder = value;
			OnPropertyChanged();
			OnPropertyChanged(nameof(IsEnabled));
			OnPropertyChanged(nameof(DueDate));
			OnPropertyChanged(nameof(DueHour));
			OnPropertyChanged(nameof(DueMinute));
		}
	}
	private TodoItem? _selectedAttachmentTodo;
	public TodoItem? SelectedAttachmentTodo {
		get => _selectedAttachmentTodo;
		set {
			if (_selectedAttachmentTodo == value) {
				return;
			}
			_selectedAttachmentTodo = value;
			OnPropertyChanged();
			OnPropertyChanged(nameof(IsRemoveTodoEnabled));
		}
	}
	public DateTime DueDate {
		get => SelectedReminder == null ? DateTime.MinValue : new DateTime(SelectedReminder.DueDate.Ticks);
		set {
			if (SelectedReminder != null) {
				SelectedReminder.DueDate = value;
			}
			OnPropertyChanged();
			SelectedReminder.UpdateValues();
		}
	}
	private int _dueHour;
	public int DueHour {
		get => SelectedReminder == null ? 0 : SelectedReminder.DueDate.Hour;
		set {
			_dueHour = value;
			if (_dueHour > 23) {
				_dueHour = 0;
				DueDate += TimeSpan.FromHours(24);
			}
			if (_dueHour < 0) {
				_dueHour = 23;
				DueDate -= TimeSpan.FromHours(24);
			}
			DueDate = new DateTime(DueDate.Year, DueDate.Month, DueDate.Day, _dueHour, DueDate.Minute, 0);
			if (DueDate >= SelectedReminder.SnoozeUntil) {
				SelectedReminder.SnoozeUntil = DateTime.MinValue;
			}
			OnPropertyChanged();
			SelectedReminder.UpdateValues();
		}
	}
	private int _dueMinute;
	public int DueMinute {
		get => SelectedReminder == null ? 0 : SelectedReminder.DueDate.Minute;
		set {
			_dueMinute = value;
			if (_dueMinute > 45) {
				_dueMinute = 0;
				DueHour++;
			}
			if (_dueMinute < 0) {
				_dueMinute = 45;
				DueHour--;
			}

			DueDate = new DateTime(DueDate.Year, DueDate.Month, DueDate.Day, DueDate.Hour, _dueMinute, 0);
			if (DueDate >= SelectedReminder.SnoozeUntil) {
				SelectedReminder.SnoozeUntil = DateTime.MinValue;
			}
			OnPropertyChanged();
			SelectedReminder.UpdateValues();
		}
	}

	private string _previewText;
	public string PreviewText {
		get => _previewText;
		set {
			if (_previewText == value) {
				return;
			}
			_previewText = value;
			OnPropertyChanged();
		}
	}
	public ObservableCollection<EnumOption> FrequencyOptions { get; set; }
	public ObservableCollection<EnumOption> AdvanceOptions { get; set; }


	public ReminderEditorViewModel(ObservableCollection<ReminderInfo> reminders, ObservableCollection<TodoItem> todos, TodoItem selectedItem) {
		Reminders = new ObservableCollection<ReminderInfo>(reminders);
		Todos = todos;
		if (Reminders.Count > 0) {
			SelectedReminder = Reminders[0];
		}

		FrequencyOptions = new ObservableCollection<EnumOption>();
		foreach (RecurringFrequency freq in Enum.GetValues<RecurringFrequency>()) {
			FrequencyOptions.Add(new EnumOption {
				Value = freq,
				Display = freq switch {
					RecurringFrequency.None => "None",
					RecurringFrequency.Hourly => "Every hour",
					RecurringFrequency.Daily => "Every day",
					RecurringFrequency.Weekly => "Every week",
					RecurringFrequency.Monthly => "Every month",
					RecurringFrequency.Yearly => "Every year",
					_ => freq.ToString()
				}
			});
		}
		AdvanceOptions = new ObservableCollection<EnumOption>();
		if (selectedItem != null) {
			SelectedReminder = Reminders.FirstOrDefault(reminder => reminder.Guid == selectedItem.ReminderGuids.FirstOrDefault());
		}
	}

	public ICommand DeleteTaskCommand => new RelayCommand(DeleteTask);
	public void DeleteTask() {
		foreach (TodoItem item in Todos) {
			item.ClearReminder(SelectedReminder.Guid);
		}
		Reminders.Remove(SelectedReminder);
	}
	public ICommand SaveChangesCommand => new RelayCommand(SaveChanges);
	public void SaveChanges() {
	}
	public ICommand AddNewTaskCommand => new RelayCommand(AddNewTask);
	public void AddNewTask() {
		ReminderInfo ri = new();
		SetDueDateNow(ri);
		Reminders.Add(ri);
		SelectedReminder = ri;
	}
	public ICommand SetDueDateNowCommand => new RelayCommand(() => SetDueDateNow(SelectedReminder));
	public void SetDueDateNow(ReminderInfo? reminder) {
		if (reminder == null) {
			return;
		}
		int dueHour = (DateTime.Now.TimeOfDay + new TimeSpan(0, 15, 0)).Hours;
		int dueMinute = ((DateTime.Now.TimeOfDay + new TimeSpan(0, 15, 0)).Minutes) / 15 * 15;
		TimeOnly time = new TimeOnly(dueHour, dueMinute);
		DateOnly date = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
		reminder.DueDate = new DateTime(date, time);
		reminder.UpdateValues();
		OnPropertyChanged(nameof(DueDate));
		OnPropertyChanged(nameof(DueMinute));
		OnPropertyChanged(nameof(DueHour));
	}
	public ICommand AddSelectedTodoCommand => new RelayCommand(AddSelectedTodo);
	public void AddSelectedTodo() {
		if (SelectedTodo != null && SelectedReminder != null) {
			SelectedReminder.TodoGuids.Add(SelectedTodo.Guid);
			SelectedReminder.Todos.Add(SelectedTodo);
			SelectedTodo.AddReminder(SelectedReminder);
			// SelectedTodo.ReminderGuid = SelectedReminder.Guid;
		}
	}
	public ICommand RemoveSelectedTodoCommand => new RelayCommand(RemoveSelectedTodo);
	public void RemoveSelectedTodo() {
		if (SelectedAttachmentTodo != null && SelectedReminder != null) {
			SelectedAttachmentTodo.ClearReminder(SelectedReminder.Guid);
			SelectedReminder.TodoGuids.Remove(SelectedAttachmentTodo.Guid);
			SelectedReminder.Todos.Remove(SelectedAttachmentTodo);
		}
	}

	public event PropertyChangedEventHandler? PropertyChanged;
	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public void Clear() {
		// Reminder.Clear();
	}
}