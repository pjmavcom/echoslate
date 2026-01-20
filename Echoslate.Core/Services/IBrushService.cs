using Echoslate.Core.Theming;

namespace Echoslate.Core.Services;

public interface IBrushService {
	static abstract object GetBrushForCommitType(string type);
	object GetBrushForSeverity(int severity);

	void SetBrushFactory(Func<ColorRgba, object> factory);
	static abstract object CreateBrush(ColorRgba color);

	object AppBackgroundBrush { get; }
	object ForegroundBrush { get; }
	object AccentBrush { get; }
	object WarningBrush { get; }
	public static object DefaultBrush { get; }
	private static readonly Dictionary<string, ColorRgba> CommitTypeColors;
}