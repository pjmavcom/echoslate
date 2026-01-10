using System.Windows;
using System.Threading.Tasks;
using Echoslate.Core.Services;

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
	}
}