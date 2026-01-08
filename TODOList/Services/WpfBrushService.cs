using System;
using System.Collections.Generic;
using System.Windows.Media;
using Echoslate.Core.Services;
using Echoslate.Core.Theming;

namespace Echoslate.Services;

public class WpfBrushService : IBrushService {
	private static object ToBrush(ColorRgba color) => new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));

	public object AppBackgroundBrush => ToBrush(ColorRgba.AppBackground);
	public object ForegroundBrush => ToBrush(ColorRgba.Foreground);
	public object AccentBrush => ToBrush(ColorRgba.Accent);
	public object WarningBrush => ToBrush(ColorRgba.DangerRed);
	public static object DefaultBrush => ToBrush(ColorRgba.ChoreGray);

	private static readonly Dictionary<string, ColorRgba> CommitTypeColors = new(StringComparer.OrdinalIgnoreCase) {
		{ "feat", ColorRgba.SuccessGreen },
		{ "fix", ColorRgba.DangerRed },
		{ "refactor", ColorRgba.RefactorBlue },
		{ "chore", ColorRgba.ChoreGray },
		{ "docs", ColorRgba.DocsYellow },
	};

	public static object GetBrushForCommitType(string type) {
		if (CommitTypeColors.TryGetValue(type, out var color)) {
			return ToBrush(color);
		}
		return DefaultBrush;
	}
	public object GetBrushForSeverity(int severity) {
			return severity switch {
				3 => ToBrush(ColorRgba.SeverityHigh),
				2 => ToBrush(ColorRgba.SeverityMed),
				1 => ToBrush(ColorRgba.SeverityLow),
				0 => ToBrush(ColorRgba.SeverityNone),
				_ => ToBrush(ColorRgba.SeverityOff)
			};
	}
	
}