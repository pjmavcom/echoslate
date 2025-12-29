using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Echoslate.Windows {
	public partial class ChooseDraftWindow : Window {
		public HistoryItem Result { get; private set; }

		public ChooseDraftWindow(IEnumerable<HistoryItem> drafts, HistoryItem defaultDraft = null) {
			InitializeComponent();

			var vm = new ChooseDraftViewModel(drafts, defaultDraft);
			DataContext = vm;
			CenterWindow();
		}
		private void CenterWindow() {
			Window win = Application.Current.MainWindow;

			if (win == null) {
				return;
			}
			double centerX = win.Width / 2 + win.Left;
			double centerY = win.Height / 2 + win.Top;
			Left = centerX - Width / 2;
			Top = centerY - Height / 2;
		}

		private void Ok_Click(object sender, RoutedEventArgs e) {
			var vm = (ChooseDraftViewModel)DataContext;
			Result = vm.SelectedHistoryItem;

			if (Result != null) {
				DialogResult = true;
			}

			Close();
		}

		private void Cancel_Click(object sender, RoutedEventArgs e) {
			DialogResult = false;
			Close();
		}
	}
}