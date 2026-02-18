using System.ComponentModel;
using System.Runtime.CompilerServices;
using Echoslate.Core.Models;
using Echoslate.Core.Services;

namespace Echoslate.Core.ViewModels;

public class WelcomeViewModel : INotifyPropertyChanged {
	public IBrushService BrushService => AppServices.BrushService;
	
	private bool _dontShowAgain;
	public bool DontShowAgain {
		get => _dontShowAgain;
		set {
			_dontShowAgain = value;
			OnPropertyChanged();
		}
	}

	public WelcomeViewModel() {
		DontShowAgain = !AppSettings.Instance.ShowWelcomeWindow;
	}

	public void SavePreferences() {
		AppSettings.Instance.ShowWelcomeWindow = !DontShowAgain;
		Log.Print($"ShowWelcomeWindow: {AppSettings.Instance.ShowWelcomeWindow}");
		AppSettings.Save();
	}

	public event PropertyChangedEventHandler PropertyChanged;
	protected void OnPropertyChanged([CallerMemberName] string name = null) =>
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}