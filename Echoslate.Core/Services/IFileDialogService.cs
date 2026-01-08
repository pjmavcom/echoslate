namespace Echoslate.Core.Services;

public interface IFileDialogService {
	string? OpenFile(string initialDirectory = "", string filter = "Echoslate files (*.echoslate)|*.echoslate");
	string? SaveFile(string defaultName = "New Project.echoslate", string initialDirectory = "", string filter = "Echoslate files (*.echoslate)|*.echoslate");
	string? ChooseFolder(string initialDirectory = "", string description = "Select Folder");
}