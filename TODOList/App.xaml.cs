using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using TODOList.Resources;

namespace TODOList {
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App {
		protected override void OnStartup(StartupEventArgs e) {
			// Force JIT of all converters
			var converters = new IValueConverter[] {
													   new SeverityToBrushConverter(),
													   new BoolToLimeBrushConverter(),
													   new BoolToTimerTextConverter(),
												   };
			foreach (var c in converters) c.Convert(0, typeof(Brush), null, CultureInfo.CurrentCulture);
			base.OnStartup(e);
		}
	}
}