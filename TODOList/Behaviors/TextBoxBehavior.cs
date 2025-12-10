using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Echoslate.Behaviors;

public static class TextBoxBehavior {
	public static readonly DependencyProperty RemoveSpacesProperty =
		DependencyProperty.RegisterAttached(
											"RemoveSpaces", typeof(bool), typeof(TextBoxBehavior),
											new PropertyMetadata(false, OnRemoveSpacesChanged));
	public static void SetRemoveSpaces(DependencyObject element, bool value) =>
		element.SetValue(RemoveSpacesProperty, value);
	public static bool GetRemoveSpaces(DependencyObject element) =>
		(bool)element.GetValue(RemoveSpacesProperty);
	private static void OnRemoveSpacesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (d is TextBox tb) {
			if ((bool)e.NewValue) {
				tb.PreviewTextInput += Tb_PreviewTextInput;
				DataObject.AddPastingHandler(tb, Tb_OnPaste);
				tb.TextChanged += Tb_TextChanged;
			} else {
				tb.PreviewTextInput -= Tb_PreviewTextInput;
				DataObject.RemovePastingHandler(tb, Tb_OnPaste);
				tb.TextChanged -= Tb_TextChanged;
			}
		}
	}
	private static void Tb_PreviewTextInput(object sender, TextCompositionEventArgs e) {
		System.Diagnostics.Debug.WriteLine($"Input: '{e.Text}'");
		e.Handled = e.Text.Any(char.IsWhiteSpace);
	}
	private static void Tb_OnPaste(object sender, DataObjectPastingEventArgs e) {
		if (e.DataObject.GetDataPresent(DataFormats.Text)) {
			string text = (string)e.DataObject.GetData(DataFormats.Text);
			string cleaned = text.Replace(" ", "").Replace("\t", "").Replace("\r", "").Replace("\n", "");
			if (text != cleaned) {
				e.CancelCommand();
				((TextBox)sender).Text += cleaned;
			}
		}
	}
	private static void Tb_TextChanged(object sender, TextChangedEventArgs e) {
		var tb = (TextBox)sender;
		if (tb.Text.Any(char.IsWhiteSpace)) {
			int caret = tb.CaretIndex;
			tb.Text = tb.Text.Replace(" ", "").Replace("\t", "");
			tb.CaretIndex = caret > tb.Text.Length ? tb.Text.Length : caret;
		}
	}
}