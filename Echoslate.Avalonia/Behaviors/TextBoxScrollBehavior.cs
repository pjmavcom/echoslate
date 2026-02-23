using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Reactive;
using Avalonia.VisualTree;

namespace Echoslate.Avalonia.Behaviors;

public static class TextBoxScrollBehavior {
	public static readonly AttachedProperty<bool> AlwaysScrollProperty =
		AvaloniaProperty.RegisterAttached<TextBox, bool>(
			"AlwaysScroll",
			typeof(TextBoxScrollBehavior),
			defaultValue: false);

	static TextBoxScrollBehavior() {
		AlwaysScrollProperty.Changed.Subscribe(
			new AnonymousObserver<AvaloniaPropertyChangedEventArgs<bool>>(OnAlwaysScrollChanged));
	}

	public static bool GetAlwaysScroll(AvaloniaObject obj) => obj.GetValue(AlwaysScrollProperty);
	public static void SetAlwaysScroll(AvaloniaObject obj, bool value) => obj.SetValue(AlwaysScrollProperty, value);

	private static void OnAlwaysScrollChanged(AvaloniaPropertyChangedEventArgs<bool> e) {
		if (e.Sender is not TextBox textBox) {
			return;
		}

		if (e.NewValue.GetValueOrDefault()) {
			textBox.PointerWheelChanged += TextBoxOnPointerWheelChanged;
		} else {
			textBox.PointerWheelChanged -= TextBoxOnPointerWheelChanged;
		}
	}

	private static void TextBoxOnPointerWheelChanged(object? sender, PointerWheelEventArgs e) {
		if (sender is not TextBox textBox) {
			return;
		}

		// Find the ScrollViewer inside the TextBox template.
		var scrollViewer = textBox
		   .GetVisualDescendants()
		   .OfType<ScrollViewer>()
		   .FirstOrDefault();

		if (scrollViewer is null) {
			return;
		}

		if (e.Delta.Y < 0) {
			scrollViewer.LineDown();
		} else if (e.Delta.Y > 0) {
			scrollViewer.LineUp();
		}

		e.Handled = true;
	}
}