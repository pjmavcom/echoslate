using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

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
	private string _todo;
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
	public bool HasDueDate {
		get => DueDate != DateTime.MinValue;
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
	public bool IsSnoozeActive {
		get => SnoozeUntil != DateTime.MinValue;
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
	public bool IsActive => HasDueDate || IsSnoozeActive;
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

	public ReminderInfo() {
		DueDate = DateTime.MinValue;
		SnoozeUntil = DateTime.MinValue;
		Guid = Guid.NewGuid();
	}
	public ReminderInfo Copy() {
		ReminderInfo info = new() {
			Guid = Guid,
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
	public void Clear() {
		if (IsRecurring) {
			var freq = (int)RecurringFrequency;
			DueDate += new TimeSpan(freq, 0, 0);
		} else {
			DueDate = DateTime.MinValue;
			SnoozeUntil = DateTime.MinValue;
		}
	}
	public void UpdateValues() {
		OnPropertyChanged(nameof(DueDateString));
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