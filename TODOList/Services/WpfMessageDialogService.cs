using System.Windows;
using Echoslate.Core.Services;

namespace Echoslate.Services;

public class WpfMessageDialogService : IMessageDialogService {
	public DialogResult Show(string message, string title, DialogButton buttons, DialogIcon icon) {
		MessageBoxButton wpfButtons = buttons switch {
			DialogButton.Ok => MessageBoxButton.OK,
			DialogButton.OkCancel => MessageBoxButton.OKCancel,
			DialogButton.YesNo => MessageBoxButton.YesNo,
			DialogButton.YesNoCancel => MessageBoxButton.YesNoCancel,
			_ => MessageBoxButton.OK
		};

		MessageBoxImage wpfIcon = icon switch {
			DialogIcon.Information => MessageBoxImage.Information,
			DialogIcon.Question => MessageBoxImage.Question,
			DialogIcon.Warning => MessageBoxImage.Warning,
			DialogIcon.Error => MessageBoxImage.Error,
			_ => MessageBoxImage.None
		};

		var wpfResult = MessageBox.Show(message, title, wpfButtons, wpfIcon);

		return wpfResult switch {
			MessageBoxResult.OK => DialogResult.Ok,
			MessageBoxResult.Cancel => DialogResult.Cancel,
			MessageBoxResult.Yes => DialogResult.Yes,
			MessageBoxResult.No => DialogResult.No,
			_ => DialogResult.None
		};
	}
}