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

public interface IDialogService {
	Task<bool> ShowDialogAsync(object view, string title);
	Task<T?> ShowDialogAsync<T>(object view, string title);

	string? OpenFile(string initialDirectory = "", string filter = "Echoslate files (*.echoslate)|*.echoslate");
	string? SaveFile(string defaultName = "New Project.echoslate", string initialDirectory = "", string filter = "Echoslate files (*.echoslate)|*.echoslate");
	string? ChooseFolder(string initialDirectory = "", string description = "Select Folder");
	
	DialogResult Show(string message, string title, DialogButton dialogButton, DialogIcon dialogIcon);
}