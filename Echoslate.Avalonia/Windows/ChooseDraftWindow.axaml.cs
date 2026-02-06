using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Echoslate.Avalonia.Windows;

public partial class ChooseDraftWindow : UserControl {
public ChooseDraftWindow() {
	InitializeComponent();
}
private void InitializeComponent() {
	AvaloniaXamlLoader.Load(this);
}
}