using CommunityToolkit.Mvvm.ComponentModel;
using Echoslate.Core.Services;

namespace Echoslate.Core.ViewModels;

public class MessageWindowViewModel : ObservableObject {
	public string Message { get; }
	public string Title { get; }
	public DialogButton Buttons { get; }
	public DialogIcon Icon { get; }

	public DialogResult Result { get; set; } = DialogResult.None;

	public MessageWindowViewModel(string message, string title, DialogButton buttons, DialogIcon icon) {
		Message = message;
		Title = title;
		Buttons = buttons;
		Icon = icon;
	}

	// Helper to show/hide buttons
	public bool ShowOk => Buttons == DialogButton.Ok || Buttons == DialogButton.OkCancel;
	public bool ShowYes => Buttons == DialogButton.YesNo || Buttons == DialogButton.YesNoCancel;
	public bool ShowNo => Buttons == DialogButton.YesNo || Buttons == DialogButton.YesNoCancel;
	public bool ShowCancel => Buttons == DialogButton.OkCancel || Buttons == DialogButton.YesNoCancel;
}