using System.Collections.Generic;

namespace Echoslate.Core.Services {
	public interface IBrushService {
		static abstract object GetBrushForCommitType(string type);
		object GetBrushForSeverity(int severity);

		object AppBackgroundBrush { get; }
		object ForegroundBrush { get; }
		object AccentBrush { get; }
		object WarningBrush { get; }

	}
}