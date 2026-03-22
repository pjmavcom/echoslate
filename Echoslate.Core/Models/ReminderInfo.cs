using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Avalonia.Controls;
using Echoslate.Core.Services;

namespace Echoslate.Core.Models;

public enum RecurringFrequency {
	None = 0,
	Hourly = 1,
	Daily = 24,
	Weekly = 24 * 7,
	Monthly = 24 * 30,
	Yearly = 24 * 365
}

public class ReminderInfo : INotifyPropertyChanged {
	private Guid _guid;
	public Guid Guid {
		get => _guid;
		set {
			if (_guid == value) {
				return;
			}
			_guid = value;
			OnPropertyChanged();
		}
	}
	private HashSet<Guid> _todoGuids;
	public HashSet<Guid> TodoGuids {
		get => _todoGuids;
		set {
			if (_todoGuids == value) {
				return;
			}
			_todoGuids = value;
			OnPropertyChanged();
		}
	}
	private ObservableCollection<TodoItem> _todos;
	[JsonIgnore]
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
	private DateTime _snoozeUntil;
	public DateTime SnoozeUntil {
		get => _snoozeUntil;
		set {
			if (_snoozeUntil == value) {
				return;
			}
			_snoozeUntil = value;
			OnPropertyChanged();
		}
	}
	private bool _isRecurring;
	public bool IsRecurring {
		get => _isRecurring;
		set {
			if (_isRecurring == value) {
				return;
			}
			_isRecurring = value;
			if (_isRecurring && RecurringFrequency == RecurringFrequency.None) {
				RecurringFrequency = RecurringFrequency.Hourly;
			}
			if (!_isRecurring) {
				RecurringFrequency = RecurringFrequency.None;
			}
			OnPropertyChanged();
		}
	}
	private RecurringFrequency _recurringFrequency;
	public RecurringFrequency RecurringFrequency {
		get => _recurringFrequency;
		set {
			if (_recurringFrequency == value) {
				return;
			}
			_recurringFrequency = value;
			if (_recurringFrequency == RecurringFrequency.None) {
				IsRecurring = false;
			} else {
				IsRecurring = true;
			}
			OnPropertyChanged();
		}
	}
	private int _leadTimeMinutes;
	public int LeadTimeMinutes {
		get => _leadTimeMinutes;
		set {
			if (_leadTimeMinutes == value) {
				return;
			}
			_leadTimeMinutes = value;
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
	private DateTime _lastNotified;
	public DateTime LastNotified {
		get => _lastNotified;
		set {
			if (_lastNotified == value) {
				return;
			}
			_lastNotified = value;
			OnPropertyChanged();
		}
	}

	[JsonIgnore]
	public bool IsSnoozeActive {
		get => SnoozeUntil != DateTime.MinValue;
	}
	[JsonIgnore]
	public bool IsDueNow {
		get {
			if (!HasDueDate) {
				return false;
			}
			if (DueDate - new TimeSpan(0, LeadTimeMinutes, 0) > DateTime.Now) {
				return false;
			}
			return !IsSnoozeActive || SnoozeUntil < DateTime.Now;
		}
	}
	[JsonIgnore]
	public string DueDateString {
		get {
			if (IsSnoozeActive) {
				return $"{SnoozeUntil:yyyy-MM-dd - HH:mm}";
			}
			if (HasDueDate) {
				return $"{DueDate:yyy-MM-dd - HH:mm}";
			}
			return "";
		}
	}


	[JsonIgnore] public bool IsActive => HasDueDate || IsSnoozeActive;
	[JsonIgnore]
	public bool HasDueDate {
		get => DueDate != DateTime.MinValue;
	}

	private string _todo;
	[JsonIgnore]
	public string Todo {
		get => _todo;
		set {
			if (_todo == value) {
				return;
			}
			_todo = value;
			OnPropertyChanged();
		}
	}

	public ReminderInfo() {
		DueDate = DateTime.MinValue;
		SnoozeUntil = DateTime.MinValue;
		Guid = Guid.NewGuid();
		Todos = [];
		TodoGuids = [];
	}
	public ReminderInfo Copy() {
		ReminderInfo info = new() {
			Guid = Guid,
			Todos = Todos,
			TodoGuids = TodoGuids,
			DueDate = DueDate,
			SnoozeUntil = SnoozeUntil,
			RecurringFrequency = RecurringFrequency,
			LeadTimeMinutes = LeadTimeMinutes,
			Message = Message,
			LastNotified = LastNotified,
			IsRecurring = IsRecurring,
		};
		return info;
	}
	public ReminderInfo? Search(Guid guid) {
		if (Guid == guid) {
			return this;
		}
		return null;
	}
	public void Clear() {
		if (IsRecurring) {
			int freq = (int)RecurringFrequency;
			SnoozeUntil = DateTime.MinValue;
			int count = -1;
			while (DueDate < DateTime.Now) {
				DueDate += new TimeSpan(freq, 0, 0);
				count++;
			}
			if (count > 0) {
				AppServices.DialogService.Show($"The alarm has passed through {count} recurring overdue dates.", "Recurring dates overdue", DialogButton.Ok, DialogIcon.Warning);
			}
		} else {
			DueDate = DateTime.MinValue;
			SnoozeUntil = DateTime.MinValue;
		}
	}
	public void ClearTodo(TodoItem item) {
		TodoGuids.Remove(item.Guid);
		TodoItem? todo = Todos.FirstOrDefault(t => t.Guid == item.Guid);
		if (todo != null) {
			Todos.Remove(todo);
		}
	}
	public void UpdateValues() {
		OnPropertyChanged(nameof(DueDateString));
		OnPropertyChanged(nameof(IsSnoozeActive));
	}
	public void SetSnooze(TimeSpan snoozeTime) {
		SnoozeUntil = DateTime.Now + snoozeTime;
		OnPropertyChanged(nameof(IsSnoozeActive));
	}

	public event PropertyChangedEventHandler? PropertyChanged;
	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
	protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
		if (EqualityComparer<T>.Default.Equals(field, value))
			return false;
		field = value;
		OnPropertyChanged(propertyName);
		return true;
	}
}