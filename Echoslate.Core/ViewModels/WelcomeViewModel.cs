using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Echoslate.Core.Models;

namespace Echoslate.Core.ViewModels;

public class WelcomeViewModel : INotifyPropertyChanged {
	public ICommand CreateNewCommand { get; }
	public ICommand OpenExistingCommand { get; }

	private bool _dontShowAgain;
	public bool DontShowAgain {
		get => _dontShowAgain;
		set {
			_dontShowAgain = value;
			OnPropertyChanged();
		}
	}

	public WelcomeViewModel(Action createNew, Action openExisting) {
		CreateNewCommand = new RelayCommand(createNew);
		OpenExistingCommand = new RelayCommand(openExisting);

		DontShowAgain = AppSettings.Instance.SkipWelcome;
	}

	public void SavePreference() {
		AppSettings.Instance.SkipWelcome = DontShowAgain;
		AppSettings.Save();
	}

	public event PropertyChangedEventHandler PropertyChanged;
	protected void OnPropertyChanged([CallerMemberName] string name = null) =>
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}