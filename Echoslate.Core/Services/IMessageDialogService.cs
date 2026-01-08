namespace Echoslate.Core.Services;

public enum DialogResult {
	None,
	Ok,
	Cancel,
	Yes,
	No
}
public enum DialogButton {
	Ok,
	OkCancel,
	YesNo,
	YesNoCancel
}
public enum DialogIcon {
	None,
	Information,
	Question,
	Warning,
	Error
}

public interface IMessageDialogService {
	DialogResult Show(string message, string title, DialogButton dialogButton, DialogIcon dialogIcon);
	
}