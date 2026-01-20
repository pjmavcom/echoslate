using Echoslate.Core.Theming;

namespace Echoslate.Core.Services;

public class BrushService : IBrushService {
	static Func<ColorRgba, object>? _brushFactory { get; set; }
	
	public object AppBackgroundBrush => CreateBrush(ColorRgba.AppBackground);
	public object ForegroundBrush => CreateBrush(ColorRgba.Foreground);
	public object AccentBrush => CreateBrush(ColorRgba.Accent);
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