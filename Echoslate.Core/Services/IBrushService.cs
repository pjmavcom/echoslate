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
	object DarkAccentBrush { get; }
	
	object BorderBrush { get; }
	object BorderSilverBrush { get; }
	object BorderMidBrush { get; }
	object BorderDarkBrush { get; }
	
	object ButtonDisabledBackgroundBrush { get; }
	object ButtonBackgroundBrush { get; }
	object ButtonHoverBackgroundBrush { get; }
	object ButtonPressedBackgroundBrush { get; }
	
	public object SeverityNoneBrush { get; }
	public object SeverityHighBrush { get; }
	public object SeverityLowBrush { get; }
	public object SeverityMedBrush { get; }
	
	object WarningBrush { get; }
	public static object DefaultBrush { get; }
	private static readonly Dictionary<string, ColorRgba> CommitTypeColors;
}