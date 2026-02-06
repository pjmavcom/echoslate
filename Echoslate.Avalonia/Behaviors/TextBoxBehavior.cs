using System;
using System.Linq;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Echoslate.Avalonia.Behaviors;

public static class TextBoxBehavior {
	public static readonly AttachedProperty<bool> RemoveSpacesProperty =
		AvaloniaProperty.RegisterAttached<TextBox, bool>(
			"RemoveSpaces",
			typeof(TextBoxBehavior),
			defaultValue: false);

	// Used to prevent re-entrancy when we set tb.Text inside TextChanged.
	private static readonly AttachedProperty<bool> IsSanitizingProperty =
		AvaloniaProperty.RegisterAttached<TextBox, bool>(
			"IsSanitizing",
			typeof(TextBoxBehavior),
			defaultValue: false);

	static TextBoxBehavior() {
		RemoveSpacesProperty.Changed.AddClassHandler<TextBox>(OnRemoveSpacesChanged);
	}

	public static void SetRemoveSpaces(AvaloniaObject element, bool value) =>
		element.SetValue(RemoveSpacesProperty, value);

	public static bool GetRemoveSpaces(AvaloniaObject element) =>
		element.GetValue(RemoveSpacesProperty);

	private static void OnRemoveSpacesChanged(TextBox tb, AvaloniaPropertyChangedEventArgs e) {
		if (e.NewValue is not bool enabled)
			return;

		if (enabled) {
			tb.AddHandler(InputElement.TextInputEvent, Tb_OnTextInput, RoutingStrategies.Tunnel);
			tb.TextChanged += Tb_OnTextChanged;
		} else {
			tb.RemoveHandler(InputElement.TextInputEvent, Tb_OnTextInput);
			tb.TextChanged -= Tb_OnTextChanged;
		}
	}

	private static void Tb_OnTextInput(object? sender, TextInputEventArgs e) {
		// Prevent whitespace from ever being inserted by normal typing.
		if (!string.IsNullOrEmpty(e.Text) && e.Text.Any(char.IsWhiteSpace))
			e.Handled = true;
	}

	private static void Tb_OnTextChanged(object? sender, TextChangedEventArgs e) {
		if (sender is not TextBox tb)
			return;

		if (tb.GetValue(IsSanitizingProperty))
			return;

		var text = tb.Text ?? string.Empty;

		// Remove *all* whitespace characters (spaces, tabs, newlines, etc.)
		var cleaned = Regex.Replace(text, @"\s+", "");

		if (text == cleaned)
			return;

		try {
			tb.SetValue(IsSanitizingProperty, true);

			// Try to keep caret in a reasonable place.
			var oldCaret = tb.CaretIndex;
			var removedBeforeCaret = Regex.Matches(text[..Math.Clamp(oldCaret, 0, text.Length)], @"\s").Count;

			tb.Text = cleaned;

			var newCaret = Math.Clamp(oldCaret - removedBeforeCaret, 0, cleaned.Length);
			tb.CaretIndex = newCaret;
		}
		finally {
			tb.SetValue(IsSanitizingProperty, false);
		}
	}
}