using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Echoslate.Core.Models;

namespace Echoslate.Core.ViewModels;

public class AlarmPopupViewModel : INotifyPropertyChanged {
	public event EventHandler? RequestClose; // simple event

	public bool Result = false;
	public bool HasSelection => SelectedReminder != null;
	public bool HasSnoozableSelection => SelectedReminder != null && SelectedReminder.IsDueNow && !SelectedReminder.IsSnoozeActive;

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
	private ReminderInfo _selectedReminder;
	public ReminderInfo SelectedReminder {
		get => _selectedReminder;
		set {
			if (_selectedReminder == value) {
				return;
			}
			_selectedReminder = value;
			OnPropertyChanged();
		}
	}
	// private ObservableCollection<ReminderInfo> _reminders;
	// public ObservableCollection<ReminderInfo> Reminders {
		// get => _reminders;
		// set {
			// if (_reminders == value) {
				// return;
			// }
			// _reminders = value;
			// OnPropertyChanged();
		// }
	// }
	// private ReminderInfo _selectedReminder;
	// public ReminderInfo SelectedReminder {
		// get => _selectedReminder;
		// set {
			// if (_selectedReminder == value) {
				// return;
			// }
			// _selectedReminder = value;
			// OnPropertyChanged();
		// }
	// }
	private int _snoozeMinutes;
	public int SnoozeMinutes {
		get => _snoozeMinutes;
		set {
			if (_snoozeMinutes == value) {
				return;
			}
			_snoozeMinutes = value;
			OnPropertyChanged();
		}
	}


	public AlarmPopupViewModel(ObservableCollection<ReminderInfo> reminders) {
		_reminders = reminders;
		SnoozeMinutes = 15;
	}
	public void UpdateAlarmsList() {
		bool isComplete = true;

		foreach (ReminderInfo reminder in Reminders) {
			if (reminder.IsDueNow) {
				isComplete = false;
				break;
			}
		}
		if (isComplete) {
			RaiseRequestClose();
			Log.Test();
		}
	}

	public ICommand SetSnoozeCommand10 => new RelayCommand<string>(s => SetSnooze(int.Parse(s)));
	public ICommand SetSnoozeCommand => new RelayCommand<int>(m => SetSnooze(m));
	public void SetSnooze(int minutes) {
		SelectedReminder.SetSnooze(new TimeSpan(0, minutes, 0));
		OnPropertyChanged(nameof(HasSnoozableSelection));
		SelectedReminder.UpdateValues();
		UpdateAlarmsList();
	}
	public ICommand SnoozeAllCommand => new RelayCommand(SnoozeAll);
	public void SnoozeAll() {
		foreach (ReminderInfo reminder in Reminders) {
			reminder.SetSnooze(new TimeSpan(0, SnoozeMinutes, 0));
			reminder.UpdateValues();
		}
		UpdateAlarmsList();
	}
	public ICommand DismissCommand => new RelayCommand(Dismiss);
	public void Dismiss() {
		SelectedReminder.Clear();
		SelectedReminder.UpdateValues();
		UpdateAlarmsList();
		Log.Test();
	}
	public ICommand DismissAllCommand => new RelayCommand(DismissAll);
	public void DismissAll() {
		foreach (ReminderInfo reminder in Reminders) {
			reminder.Clear();
			reminder.UpdateValues();
		}
		UpdateAlarmsList();
	}


	protected void RaiseRequestClose() {
		RequestClose?.Invoke(this, EventArgs.Empty);
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
	public void SelectionChanged() {
		Log.Test();
		OnPropertyChanged(nameof(HasSelection));
		OnPropertyChanged(nameof(HasSnoozableSelection));
	}
}