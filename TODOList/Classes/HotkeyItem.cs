using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Echoslate;

public class HotkeyItem : INotifyPropertyChanged{
	private string _hotkey;
	public string Hotkey {
		get => _hotkey;
		set {
			_hotkey = value;
			OnPropertyChanged();
		}
	}
	private string _description;
	public string Description {
		get => _description;
		set {
			_description = value;
			OnPropertyChanged();
		}
	}



	public event PropertyChangedEventHandler? PropertyChanged;
	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
	protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
		if (EqualityComparer<T>.Default.Equals(field, value)) return false;
		field = value;
		OnPropertyChanged(propertyName);
		return true;
	}
}