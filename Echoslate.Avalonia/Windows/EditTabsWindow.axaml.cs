using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Echoslate.Avalonia.Windows;

public partial class EditTabsWindow : UserControl {
public EditTabsWindow() {
	InitializeComponent();
}
private void InitializeComponent() {
	AvaloniaXamlLoader.Load(this);
}
}