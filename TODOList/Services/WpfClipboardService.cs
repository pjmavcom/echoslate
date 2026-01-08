using System.Windows;
using Echoslate.Core.Services;

namespace Echoslate.Services;

public class WpfClipboardService : IClipboardService
{
	public void SetText(string text) {
		Clipboard.SetText(text);
	}
}