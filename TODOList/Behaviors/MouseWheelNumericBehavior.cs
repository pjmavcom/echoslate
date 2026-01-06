using System;

namespace Echoslate.Behaviors;

using System.Windows;
using System.Windows.Input;

public static class MouseWheelNumericBehavior {
	public static readonly DependencyProperty EnabledProperty =
		DependencyProperty.RegisterAttached("Enabled", typeof(bool), typeof(MouseWheelNumericBehavior),
			new PropertyMetadata(false, OnEnabledChanged));
	public static bool GetEnabled(DependencyObject obj) => (bool)obj.GetValue(EnabledProperty);
	public static void SetEnabled(DependencyObject obj, bool value) => obj.SetValue(EnabledProperty, value);

	public static readonly DependencyProperty TargetPropertyProperty =
		DependencyProperty.RegisterAttached("TargetProperty", typeof(string), typeof(MouseWheelNumericBehavior),
			new PropertyMetadata(null));
	public static string GetTargetProperty(DependencyObject obj) => (string)obj.GetValue(TargetPropertyProperty);
	public static void SetTargetProperty(DependencyObject obj, string value) => obj.SetValue(TargetPropertyProperty, value);

	public static readonly DependencyProperty MinimumProperty =
		DependencyProperty.RegisterAttached("Minimum", typeof(int), typeof(MouseWheelNumericBehavior),
			new PropertyMetadata(0));
	public static int GetMinimum(DependencyObject obj) => (int)obj.GetValue(MinimumProperty);
	public static void SetMinimum(DependencyObject obj, int value) => obj.SetValue(MinimumProperty, value);
	
	public static readonly DependencyProperty MaximumProperty =
		DependencyProperty.RegisterAttached("Maximum", typeof(int), typeof(MouseWheelNumericBehavior),
			new PropertyMetadata(int.MaxValue));
	public static int GetMaximum(DependencyObject obj) => (int)obj.GetValue(MaximumProperty);
	public static void SetMaximum(DependencyObject obj, int value) => obj.SetValue(MaximumProperty, value);
	
	private static void OnEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (d is UIElement element) {
			if ((bool)e.NewValue) {
				element.MouseWheel += Element_MouseWheel;
				element.PreviewMouseWheel += Element_PreviewMouseWheel;
			} else {
				element.MouseWheel -= Element_MouseWheel;
				element.PreviewMouseWheel -= Element_PreviewMouseWheel;
			}
		}
	}

	private static void Element_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
		if (sender is FrameworkElement fe && GetEnabled(fe)) {
			HandleMouseWheel(fe, e);
			e.Handled = true;
		}
	}

	private static void Element_MouseWheel(object sender, MouseWheelEventArgs e) {
		if (sender is FrameworkElement fe && GetEnabled(fe)) {
			HandleMouseWheel(fe, e);
		}
	}

	private static void HandleMouseWheel(FrameworkElement fe, MouseWheelEventArgs e) {
		if (fe.DataContext == null) {
			return;
		}

		string propertyName = GetTargetProperty(fe);
		if (string.IsNullOrEmpty(propertyName)) {
			return;
		}

		var property = fe.DataContext.GetType().GetProperty(propertyName);
		if (property == null || property.PropertyType != typeof(int) || !property.CanWrite) {
			return;
		}
		int min = GetMinimum(fe);
		int max = GetMaximum(fe);
		int current = (int)property.GetValue(fe.DataContext);
		int step = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) ? 5 : 1;
		int delta = e.Delta > 0 ? step : -step;

		int newValue = Math.Max(0, current + delta);
		newValue = Math.Clamp(newValue, min, max);
		property.SetValue(fe.DataContext, newValue);
	}
}