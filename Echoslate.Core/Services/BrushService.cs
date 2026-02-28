using Echoslate.Core.Theming;

namespace Echoslate.Core.Services;

public class BrushService : IBrushService {
	static Func<ColorRgba, object>? _brushFactory { get; set; }
	
	public object TransparentBrush => CreateBrush(ColorRgba.Transparent);
	public object AppBackgroundBrush => CreateBrush(ColorRgba.AppBackground);
	public object AppDarkBackgroundBrush => CreateBrush(ColorRgba.AppDarkBackground);
	public object ControlBackgroundBrush => CreateBrush(ColorRgba.ControlBackground);
	public object EditorBackgroundBrush => CreateBrush(ColorRgba.EditorBackground);
	public object RaisedBackgroundBrush => CreateBrush(ColorRgba.RaisedBackground);
	
	public object ForegroundBrush => CreateBrush(ColorRgba.Foreground);
	public object SubtleForegroundBrush => CreateBrush(ColorRgba.SubtleForeground);
	
	public object LightAccentBrush => CreateBrush(ColorRgba.LightAccent);
	public object AccentBrush => CreateBrush(ColorRgba.Accent);
	public object AccentBrushHover => CreateBrush(ColorRgba.AccentHover);
	public object AccentBrushPressed => CreateBrush(ColorRgba.AccentPressed);
	public object DarkAccentBrush => CreateBrush(ColorRgba.DarkAccent);
	public object DarkFadedAccentBrush => CreateBrush(ColorRgba.DarkFadedAccent);
	
	public object BorderBrush => CreateBrush(ColorRgba.Border);
	public object BorderSilverBrush => CreateBrush(ColorRgba.BorderSilver);
	public object BorderMidBrush => CreateBrush(ColorRgba.BorderMid);
	public object BorderDarkBrush => CreateBrush(ColorRgba.BorderDark);

	public object ButtonDisabledBackgroundBrush => CreateBrush(ColorRgba.ButtonDisabledBackground);
	public object ButtonBackgroundBrush => CreateBrush(ColorRgba.ButtonBackground);
	public object ButtonHoverBackgroundBrush => CreateBrush(ColorRgba.ButtonHoverBackground);
	public object ButtonPressedBackgroundBrush => CreateBrush(ColorRgba.ButtonPressedBackground);
	
	public object TabActiveBackgroundBrush => CreateBrush(ColorRgba.TabActiveBackground);
	public object TabInactiveBackgroundBrush => CreateBrush(ColorRgba.TabInactiveBackground);
	
	public object SeverityNoneBrush => CreateBrush(ColorRgba.SeverityNone);
	public object SeverityHighBrush => CreateBrush(ColorRgba.SeverityHigh);
	public object SeverityLowBrush => CreateBrush(ColorRgba.SeverityLow);
	public object SeverityMedBrush => CreateBrush(ColorRgba.SeverityMed);
	
	public object AccentBlueBrush => CreateBrush(ColorRgba.AccentBlue);
	public object LightSuccessGreenBrush => CreateBrush(ColorRgba.LightSuccessGreen);
	public object SuccessGreenBrush => CreateBrush(ColorRgba.SuccessGreen);
	public object DarkSuccessGreenBrush => CreateBrush(ColorRgba.DarkSuccessGreen);
	public object DisabledSuccessGreenBrush => CreateBrush(ColorRgba.DisabledSuccessGreen);
	public object LightDangerRedBrush => CreateBrush(ColorRgba.LightDangerRed);
	public object DangerRedBrush => CreateBrush(ColorRgba.DangerRed);
	public object DarkDangerRedBrush => CreateBrush(ColorRgba.DarkDangerRed);
	public object DarkerDangerRedBrush => CreateBrush(ColorRgba.DarkerDangerRed);
	public object RefactorBlueBrush => CreateBrush(ColorRgba.RefactorBlue);
	public object ChoreGrayBrush => CreateBrush(ColorRgba.ChoreGray);
	public object DocsYellowBrush => CreateBrush(ColorRgba.DocsYellow);
	public object EditingOrangeBrush => CreateBrush(ColorRgba.EditingOrange);
	
	public object WarningBrush => CreateBrush(ColorRgba.DangerRed);
	public static object DefaultBrush => CreateBrush(ColorRgba.ChoreGray);

	private static readonly Dictionary<string, ColorRgba> CommitTypeColors = new(StringComparer.OrdinalIgnoreCase) {
		{ "feat", ColorRgba.SuccessGreen },
		{ "fix", ColorRgba.DangerRed },
		{ "refactor", ColorRgba.RefactorBlue },
		{ "chore", ColorRgba.ChoreGray },
		{ "docs", ColorRgba.DocsYellow },
	};

	public BrushService() {
		_brushFactory = rgba => null;
	}
	public void SetBrushFactory(Func<ColorRgba, object> factory) {
		_brushFactory = factory ?? throw new ArgumentNullException(nameof(factory));
	}
	public static object CreateBrush(ColorRgba color) {
		if (_brushFactory == null) {
			throw new InvalidOperationException("Brush factory not set. Call SetBrushFactory first.");
		}
		return _brushFactory(color);
	}
	public static object GetBrushForCommitType(string type) {
		if (CommitTypeColors.TryGetValue(type, out var color)) {
			return CreateBrush(color);
		}
		return DefaultBrush;
	}
	public object GetBrushForSeverity(int severity) {
		return severity switch {
			3 => CreateBrush(ColorRgba.SeverityHigh),
			2 => CreateBrush(ColorRgba.SeverityMed),
			1 => CreateBrush(ColorRgba.SeverityLow),
			0 => CreateBrush(ColorRgba.SeverityNone),
			_ => CreateBrush(ColorRgba.SeverityOff)
		};
	}
}