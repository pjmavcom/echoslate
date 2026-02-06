using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Echoslate.Avalonia.Windows;

public partial class TodoMultiItemEditorWindow : UserControl {
public TodoMultiItemEditorWindow() {
	InitializeComponent();
}
private void InitializeComponent() {
	AvaloniaXamlLoader.Load(this);
}
}