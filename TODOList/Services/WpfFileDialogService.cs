using System.Windows;
using System.Windows.Forms;
using Echoslate.Core.Services;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace Echoslate.WPF.Services {
	public class WpfFileDialogService : IFileDialogService {
		private readonly Window _owner;

		public WpfFileDialogService(Window owner) {
			_owner = owner;
		}

		public string? OpenFile(string initialDirectory, string filter) {
			var dialog = new OpenFileDialog {
				Filter = filter,
				InitialDirectory = initialDirectory
			};

			return dialog.ShowDialog(_owner) == true ? dialog.FileName : null;
		}

		public string? SaveFile(string defaultName, string initialDirectory, string filter) {
			var dialog = new SaveFileDialog {
				Filter = filter,
				FileName = defaultName,
				InitialDirectory = initialDirectory
			};

			return dialog.ShowDialog(_owner) == true ? dialog.FileName : null;
		}

		public string? ChooseFolder(string initialDirectory, string description) {
			var dialog = new FolderBrowserDialog {
				Description = description,
				UseDescriptionForTitle = true,
				SelectedPath = initialDirectory ?? string.Empty,
				ShowNewFolderButton = true
			};

			if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
				return dialog.SelectedPath;
			}

			return null;
		}
	}
}