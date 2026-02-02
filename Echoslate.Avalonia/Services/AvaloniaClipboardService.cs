using System;
using Avalonia.Input;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Echoslate.Core.Services;

namespace Echoslate.Avalonia.Services {
	public class AvaloniaClipboardService : IClipboardService {
		private readonly TopLevel _topLevel;
		
		public AvaloniaClipboardService(TopLevel topLevel) {
			_topLevel = topLevel ?? throw new ArgumentNullException(nameof(topLevel));
		}
		
		public async Task SetTextAsync(string text) {
			IClipboard? clipboard = _topLevel.Clipboard;
			if (clipboard == null) {
				throw new InvalidOperationException("Clipboard not available.");
			}

			await clipboard.SetTextAsync(text);
		}

		public void SetText(string text) {
			SetTextAsync(text).GetAwaiter().GetResult();
		}
	}
}