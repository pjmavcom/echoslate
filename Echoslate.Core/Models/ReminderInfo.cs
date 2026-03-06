using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Echoslate.Core.Models;

public enum RecurringFrequency {
	None = 0,
	Hourly = 1,
	Daily = 24,
	Weekly = 24*7,
	Monthly = 24*30,
	Yearly = 24*365
}

public class ReminderInfo : INotifyPropertyChanged {
	private bool _isActive;
	public bool IsActive {
		get => _isActive;
		set {
			if (_isActive == value) {
				return;
			}
			_isActive = value;
			OnPropertyChanged();
		}
	}
	private DateTimeOffset _dueDate;
	public DateTimeOffset DueDate {
		get => _dueDate;
		set {
			if (_dueDate == value) {
				return;
			}
			_dueDate = value;
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
			OnPropertyChanged();
		}
	}
	private RecurringFrequency _frequency;
	public RecurringFrequency Frequency {
		get => _frequency;
		set {
			if (_frequency == value) {
				return;
			}
			_frequency = value;
			OnPropertyChanged();
		}
	}
	private int _advanceMinutes;
	public int AdvanceMinutes {
		get => _advanceMinutes;
		set {
			if (_advanceMinutes == value) {
				return;
			}
			_advanceMinutes = value;
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
	private DateTimeOffset _lastNotified;
	public DateTimeOffset LastNotified {
		get => _lastNotified;
		set {
			if (_lastNotified == value) {
				return;
			}
			_lastNotified = value;
			OnPropertyChanged();
		}
	}
	private bool _isSnoozeActive;
	public bool IsSnoozeActive {
		get => _isSnoozeActive;
		set {
			if (_isSnoozeActive == value) {
				return;
			}
			_isSnoozeActive = value;
			OnPropertyChanged();
		}
	}
	private DateTimeOffset _snoozeUntil;
	public DateTimeOffset SnoozeUntil {
		get => _snoozeUntil;
		set {
			if (_snoozeUntil == value) {
				return;
			}
			_snoozeUntil = value;
			OnPropertyChanged();
		}
	}


	public void Clear() {
		IsActive = false;
		IsSnoozeActive = false;
	}
	public ReminderInfo Copy() {
		ReminderInfo info = new() {
			IsActive = IsActive,
			IsSnoozeActive = IsSnoozeActive,
			DueDate = DueDate,
			Frequency = Frequency,
			AdvanceMinutes = AdvanceMinutes,
			Message = Message,
			LastNotified = LastNotified,
			SnoozeUntil = SnoozeUntil,
			IsRecurring = IsRecurring,

		};
		return info;
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