using System.ComponentModel;
using System.Runtime.CompilerServices;
using Echoslate.Core.Models;
using Echoslate.Core.Services;

namespace Echoslate.Core.ViewModels;

public class WelcomeViewModel : INotifyPropertyChanged {
	public event Action<bool>? RequestClose;
	public string Version { get; set; }

	public IBrushService BrushService => AppServices.BrushService;

	private bool _showAgain;
	public bool ShowAgain {
		get => _showAgain;
		set {
			_showAgain = value;
			if (_showAgain) {
				ShowWindowString1 = "Show this window next time";
				ShowWindowString2 = "";
			} else {
				ShowWindowString1 = "Do not show this window again";
				ShowWindowString2 = "(Always load last file)";
			}

			OnPropertyChanged();
			OnPropertyChanged(nameof(ShowWindowString1));
			OnPropertyChanged(nameof(ShowWindowString2));
		}
	}
	private string _showWindowString1;
	public string ShowWindowString1 {
		get => _showWindowString1;
		set {
			if (_showWindowString1 == value) {
				return;
			}
			_showWindowString1 = value;
			OnPropertyChanged();
		}
	}
	private string _showWindowString2;
	public string ShowWindowString2 {
		get => _showWindowString2;
		set {
			if (_showWindowString2 == value) {
				return;
			}
			_showWindowString2 = value;
			OnPropertyChanged();
		}
	}

	public WelcomeViewModel() {
		ShowAgain = AppSettings.Instance.ShowWelcomeWindow;
		Version = AppServices.ApplicationService.GetVersion();
		OnPropertyChanged(nameof(Version));
	}
	public void SavePreferences() {
		AppSettings.Instance.ShowWelcomeWindow = ShowAgain;
		Log.Print($"ShowWelcomeWindow: {AppSettings.Instance.ShowWelcomeWindow}");
		AppSettings.Save();
	}
	public void Close(bool result) {
		RequestClose?.Invoke(result);
	}

	public event PropertyChangedEventHandler PropertyChanged;
	protected void OnPropertyChanged([CallerMemberName] string name = null) =>
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}