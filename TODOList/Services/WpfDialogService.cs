using System.Windows;
using System.Threading.Tasks;
using System.Windows.Forms;
using Echoslate.Core.Services;
using Application = System.Windows.Application;
using DialogResult = Echoslate.Core.Services.DialogResult;
using MessageBox = System.Windows.MessageBox;


namespace Echoslate.WPF.Services {
	public class WpfDialogService : IDialogService {
		private Window _owner;

		public WpfDialogService(Window owner) {
			_owner = owner;
		}
		public Task<bool> ShowDialogAsync(object view, string title) {
			if (_owner == null) {
				GetOwner();
			}
			var window = new Window {
				Content = view,
				Title = title,
				Owner = _owner,
				WindowStartupLocation = WindowStartupLocation.CenterOwner,
				SizeToContent = SizeToContent.WidthAndHeight,
				ResizeMode = ResizeMode.NoResize,
				ShowInTaskbar = false
			};

			bool? dialogResult = window.ShowDialog();
			return Task.FromResult(dialogResult == true);
		}
		public Task<T?> ShowDialogAsync<T>(object view, string title) {
			if (_owner == null) {
				GetOwner();
			}
			return Task.Run(() => {
				var window = new Window {
					Content = view,
					Title = title,
					Owner = _owner,
					WindowStartupLocation = WindowStartupLocation.CenterOwner,
					SizeToContent = SizeToContent.WidthAndHeight,
					ResizeMode = ResizeMode.NoResize,
					ShowInTaskbar = false
				};

				bool? dialogResult = window.ShowDialog();

				if (dialogResult == true && view is FrameworkElement fe && fe.DataContext != null) {
					var prop = fe.DataContext.GetType().GetProperty("Result");
					if (prop != null && prop.PropertyType == typeof(T)) {
						return (T?)prop.GetValue(fe.DataContext);
					}
				}

				return default(T);
			});
		}
		private void GetOwner() {
			_owner = Application.Current.MainWindow;
		}
		public string? OpenFile(string initialDirectory, string filter) {
			if (_owner == null) {
				GetOwner();
			}
			var dialog = new OpenFileDialog {
				Filter = filter,
				InitialDirectory = initialDirectory
			};

			return dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK ? dialog.FileName : null;
		}

		public string? SaveFile(string defaultName, string initialDirectory, string filter) {
			if (_owner == null) {
				GetOwner();
			}
			var dialog = new SaveFileDialog {
				Filter = filter,
				FileName = defaultName,
				InitialDirectory = initialDirectory
			};

			return dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK ? dialog.FileName : null;
		}

		public string? ChooseFolder(string initialDirectory, string description) {
			if (_owner == null) {
				GetOwner();
			}
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
}