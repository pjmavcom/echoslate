using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using Avalonia.Reactive;
using Echoslate.Core.Models;

namespace Echoslate.Avalonia.Behaviors;

public static class MouseWheelNumericBehavior {
	public static readonly AttachedProperty<bool> EnabledProperty =
		AvaloniaProperty.RegisterAttached<Interactive, bool>("Enabled", typeof(MouseWheelNumericBehavior));
	
	public static readonly AttachedProperty<bool> OnlyOnControlProperty =
		AvaloniaProperty.RegisterAttached<Interactive, bool>("OnlyOnControl", typeof(MouseWheelNumericBehavior), false);

	public static readonly AttachedProperty<int> MinimumProperty =
		AvaloniaProperty.RegisterAttached<Interactive, int>("Minimum", typeof(MouseWheelNumericBehavior), 0);

	public static readonly AttachedProperty<int> MaximumProperty =
		AvaloniaProperty.RegisterAttached<Interactive, int>("Maximum", typeof(MouseWheelNumericBehavior), int.MaxValue);

	public static readonly AttachedProperty<string> TargetPropertyProperty =
		AvaloniaProperty.RegisterAttached<Interactive, string>("TargetProperty", typeof(MouseWheelNumericBehavior));

	static MouseWheelNumericBehavior() {
		EnabledProperty.Changed.Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs<bool>>(
			onNext: e => OnEnabledChanged(e)
		));
	}

	public static bool GetEnabled(Control control) => control.GetValue(EnabledProperty);
	public static void SetEnabled(Control control, bool value) => control.SetValue(EnabledProperty, value);
	
	public static bool GetOnlyOnControl(Control control) => control.GetValue(OnlyOnControlProperty);
	public static void SetOnlyOnControl(Control control, bool value) => control.SetValue(OnlyOnControlProperty, value);

	public static int GetMinimum(Control control) => control.GetValue(MinimumProperty);
	public static void SetMinimum(Control control, int value) => control.SetValue(MinimumProperty, value);

	public static int GetMaximum(Control control) => control.GetValue(MaximumProperty);
	public static void SetMaximum(Control control, int value) => control.SetValue(MaximumProperty, value);

	public static string GetTargetProperty(Control control) => control.GetValue(TargetPropertyProperty);
	public static void SetTargetProperty(Control control, string value) => control.SetValue(TargetPropertyProperty, value);

	private static void OnEnabledChanged(AvaloniaPropertyChangedEventArgs<bool> e) {
		if (e.Sender is not Control control) {
			return;
		}

		if (e.NewValue.GetValueOrDefault()) {
			control.PointerWheelChanged += Control_PointerWheelChanged;
		} else {
			control.PointerWheelChanged -= Control_PointerWheelChanged;
		}
	}

	private static void Control_PointerWheelChanged(object? sender, PointerWheelEventArgs e) {
		if (sender is not Control control) {
			return;
		}
		if (GetOnlyOnControl(control) && !e.KeyModifiers.HasFlag(KeyModifiers.Control)) {
			return;
		}

		var vm = control.DataContext;
		if (vm == null) {
			return;
		}

		string targetPropName = GetTargetProperty(control);
		if (string.IsNullOrEmpty(targetPropName)) {
			return;
		}

		var prop = vm.GetType().GetProperty(targetPropName);
		if (prop == null || prop.PropertyType != typeof(int)) {
			return;
		}

		int current = (int)prop.GetValue(vm)!;

		int delta = e.Delta.Y > 0 ? 1 : -1;
		if (e.KeyModifiers.HasFlag(KeyModifiers.Shift) && !GetOnlyOnControl(control)) {
			delta *= 5;
		}

		int newValue = current + delta;

		int min = GetMinimum(control);
		int max = GetMaximum(control);
		newValue = Math.Max(min, Math.Min(max, newValue));

		prop.SetValue(vm, newValue);

		if (control is TextBlock textBlock) {
			textBlock.Text = newValue.ToString();
		}

		e.Handled = true;
	}
}