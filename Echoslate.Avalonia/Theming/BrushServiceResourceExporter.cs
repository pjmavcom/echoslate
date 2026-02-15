using Avalonia.Controls;
using Avalonia.Media;
using Echoslate.Core.Services;

namespace Echoslate.Avalonia.Theming;

public static class BrushServiceResourceExporter {
	public static void ExportTo(IResourceDictionary resources, IBrushService brushService) {
		resources["TransparentBrush"] = (IBrush)brushService.TransparentBrush;

		resources["AppBackgroundBrush"] = (IBrush)brushService.AppBackgroundBrush;
		resources["AppDarkBackgroundBrush"] = (IBrush)brushService.AppDarkBackgroundBrush;
		resources["ControlBackgroundBrush"] = (IBrush)brushService.ControlBackgroundBrush;
		resources["EditorBackgroundBrush"] = (IBrush)brushService.EditorBackgroundBrush;
		resources["RaisedBackgroundBrush"] = (IBrush)brushService.RaisedBackgroundBrush;

		resources["ForegroundBrush"] = (IBrush)brushService.ForegroundBrush;
		resources["SubtleForegroundBrush"] = (IBrush)brushService.SubtleForegroundBrush;

		resources["LightAccentBrush"] = (IBrush)brushService.LightAccentBrush;
		resources["AccentBrush"] = (IBrush)brushService.AccentBrush;
		resources["DarkAccentBrush"] = (IBrush)brushService.DarkAccentBrush;

		resources["BorderBrush"] = (IBrush)brushService.BorderBrush;
		resources["BorderSilverBrush"] = (IBrush)brushService.BorderSilverBrush;
		resources["BorderMidBrush"] = (IBrush)brushService.BorderMidBrush;
		resources["BorderDarkBrush"] = (IBrush)brushService.BorderDarkBrush;

		resources["ButtonDisabledBackgroundBrush"] = (IBrush)brushService.ButtonDisabledBackgroundBrush;
		resources["ButtonBackgroundBrush"] = (IBrush)brushService.ButtonBackgroundBrush;
		resources["ButtonHoverBackgroundBrush"] = (IBrush)brushService.ButtonHoverBackgroundBrush;
		resources["ButtonPressedBackgroundBrush"] = (IBrush)brushService.ButtonPressedBackgroundBrush;

		resources["SeverityNoneBrush"] = (IBrush)brushService.SeverityNoneBrush;
		resources["SeverityLowBrush"] = (IBrush)brushService.SeverityLowBrush;
		resources["SeverityMedBrush"] = (IBrush)brushService.SeverityMedBrush;
		resources["SeverityHighBrush"] = (IBrush)brushService.SeverityHighBrush;

		resources["WarningBrush"] = (IBrush)brushService.WarningBrush;
		resources["EditingBrush"] = (IBrush)brushService.EditingBrush;
	}
}