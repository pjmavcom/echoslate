using Echoslate.Core.Theming;

namespace Echoslate.Core.Services;

public interface IBrushService {
	static abstract object GetBrushForCommitType(string type);
	object GetBrushForSeverity(int severity);

	void SetBrushFactory(Func<ColorRgba, object> factory);
	static abstract object CreateBrush(ColorRgba color);

	object TransparentBrush { get; }
	
	object AppBackgroundBrush { get; }
	object AppDarkBackgroundBrush { get; }
	object ControlBackgroundBrush { get; }
	object EditorBackgroundBrush { get; }
	object RaisedBackgroundBrush { get; }
	
	object ForegroundBrush { get; }
	object SubtleForegroundBrush { get; }
	
	object LightAccentBrush { get; }
	object AccentBrush { get; }
	object AccentBrushHover { get; }
	object AccentBrushPressed { get; }
	object DarkAccentBrush { get; }
	object DarkFadedAccentBrush { get; }
	
	object BorderBrush { get; }
	object BorderSilverBrush { get; }
	object BorderMidBrush { get; }
	object BorderDarkBrush { get; }
	
	object ButtonDisabledBackgroundBrush { get; }
	object ButtonBackgroundBrush { get; }
	object ButtonHoverBackgroundBrush { get; }
	object ButtonPressedBackgroundBrush { get; }
	
	object TabActiveBackgroundBrush { get; }
	object TabInactiveBackgroundBrush { get; }
	
	object SeverityNoneBrush { get; }
	object SeverityHighBrush { get; }
	object SeverityLowBrush { get; }
	object SeverityMedBrush { get; }
	
	object AccentBlueBrush { get; }
	object LightSuccessGreenBrush { get; }
	object SuccessGreenBrush { get; }
	object DarkSuccessGreenBrush { get; }
	object DisabledSuccessGreenBrush { get; }
	object LightDangerRedBrush { get; }
	object DangerRedBrush { get; }
	object DarkDangerRedBrush { get; }
	object DarkerDangerRedBrush { get; }
	object RefactorBlueBrush { get; }
	object ChoreGrayBrush { get; }
	object DocsYellowBrush { get; }
	object EditingOrangeBrush { get; }
	
	object WarningBrush { get; }
	
	public static object DefaultBrush { get; }
	private static readonly Dictionary<string, ColorRgba> CommitTypeColors;
}