using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Echoslate.Core.Models;

namespace Echoslate.Core.ViewModels;

public class EnumOption {
	public RecurringFrequency Value { get; set; }
	public string Display { get; set; } = string.Empty;
}

public class ReminderEditorViewModel : INotifyPropertyChanged {
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
	private TodoItem _item;
	public TodoItem Item {
		get => _item;
		set {
			if (_item == value) {
				return;
			}
			_item = value;
			OnPropertyChanged();
		}
	}
	public bool HasReminder {
		get => Reminder.IsActive; //_hasReminder;
		set {
			if (Reminder.IsActive == value) {
				return;
			}
			Reminder.IsActive = value;
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
	private int _dueHour;
	public int DueHour {
		get => _dueHour;
		set {
			if (_dueHour == value) {
				return;
			}
			_dueHour = value;
			if (_dueHour > 23) {
				_dueHour = 0;
				DueDate += TimeSpan.FromHours(24);
			}
			if (_dueHour < 0) {
				_dueHour = 23;
				DueDate -= TimeSpan.FromHours(24);
			}
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
			if (_dueMinute > 45) {
				_dueMinute = 0;
				DueHour++;
			}
			if (_dueMinute < 0) {
				_dueMinute = 45;
				DueHour--;
			}

			OnPropertyChanged();
		}
	}
	public bool IsRecurring {
		get => Reminder.IsRecurring;
		set {
			if (Reminder.IsRecurring == value) {
				return;
			}
			Reminder.IsRecurring = value;
			OnPropertyChanged();
		}
	}
	public RecurringFrequency Frequency {
		get => Reminder.Frequency;
		set {
			if (Reminder.Frequency == value) {
				return;
			}
			Reminder.Frequency = value;
			OnPropertyChanged();
		}
	}
	public int AdvanceMinutes {
		get => Reminder.AdvanceMinutes;
		set {
			if (Reminder.AdvanceMinutes == value) {
				return;
			}
			Reminder.AdvanceMinutes = value;
			OnPropertyChanged();
		}
	}
	public string Message {
		get => Reminder.Message;
		set {
			if (Reminder.Message == value) {
				return;
			}
			Reminder.Message = value;
			OnPropertyChanged();
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


	public ReminderEditorViewModel(List<TodoItem> items) {
		Item = items.First();
		Reminder = Item.Reminder.Copy();
		if (Reminder.DueDate == DateTimeOffset.MinValue) {
			Reminder.DueDate = DateTimeOffset.Now;
			DueHour = DueDate.Hour;
			DueMinute = DueDate.Minute;
		}
		DueDate = Reminder.DueDate.DateTime;
		DueHour = (DueDate.TimeOfDay + new TimeSpan(0, 15, 0)).Hours;
		DueMinute = ((DueDate.TimeOfDay + new TimeSpan(0, 15, 0)).Minutes) / 15 * 15;

		FrequencyOptions = new ObservableCollection<EnumOption>();
		foreach (RecurringFrequency freq in Enum.GetValues<RecurringFrequency>()) {
			if (freq == RecurringFrequency.None) {
				continue;
			}
			FrequencyOptions.Add(new EnumOption {
				Value = freq,
				Display = freq switch {
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
	}
	public void OnOk() {
		DateOnly date = new DateOnly(DueDate.Year, DueDate.Month, DueDate.Day);
		TimeOnly time = new TimeOnly(DueHour, DueMinute);
		Reminder.DueDate = new DateTimeOffset(date, time, TimeZoneInfo.Local.GetUtcOffset(DateTime.Now));
	}

	public event PropertyChangedEventHandler? PropertyChanged;
	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

}