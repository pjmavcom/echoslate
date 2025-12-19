using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace EchoSlate.Behaviors {
	public static class ScrollBehavior {
		public static readonly DependencyProperty AlwaysScrollProperty =
			DependencyProperty.RegisterAttached(
												"AlwaysScroll",
												typeof(bool),
												typeof(ScrollBehavior),
												new PropertyMetadata(false, OnAlwaysScrollChanged));

		public static bool GetAlwaysScroll(DependencyObject obj) => (bool)obj.GetValue(AlwaysScrollProperty);
		public static void SetAlwaysScroll(DependencyObject obj, bool value) => obj.SetValue(AlwaysScrollProperty, value);

		private static void OnAlwaysScrollChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is UIElement element) {
				if ((bool)e.NewValue) {
					element.PreviewMouseWheel += HandlePreviewMouseWheel;
				} else {
					element.PreviewMouseWheel -= HandlePreviewMouseWheel;
				}
			}
		}

		private static void HandlePreviewMouseWheel(object sender, MouseWheelEventArgs e) {
			if (sender is not Control control) {
				return;
			}
			var scrollViewer = FindChild<ScrollViewer>(control);

			if (scrollViewer != null) {
				if (e.Delta < 0) {
					scrollViewer.LineDown();
				} else {
					scrollViewer.LineUp();
				}
				e.Handled = true;
			}
		}
		private static T FindChild<T>(DependencyObject parent) where T : DependencyObject {
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++) {
				var child = VisualTreeHelper.GetChild(parent, i);
				if (child is T t) {
					return t;
				}
				var found = FindChild<T>(child);
				if (found != null) {
					return found;
				}
			}
			return null;
		}
	}
}