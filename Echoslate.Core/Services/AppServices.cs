using Echoslate.Core.ViewModels;

namespace Echoslate.Core.Services;

public static class AppServices {
	public static MainWindowViewModel MainWindowVM { get; private set; }
	public static IApplicationService ApplicationService { get; private set; }
	public static IBrushService BrushService { get; private set; }
	public static IDispatcherService DispatcherService { get; private set; }
	public static IClipboardService ClipboardService { get; private set; }
	public static IDialogService DialogService { get; private set; }

	public static void Initialize(MainWindowViewModel mainVM, IApplicationService applicationService, IDispatcherService dispatcherService, IClipboardService clipboardService, IDialogService dialogService) {
		MainWindowVM = mainVM;
		ApplicationService = applicationService;
		BrushService = new BrushService();
		DispatcherService = dispatcherService;
		ClipboardService = clipboardService;
		DialogService = dialogService;
	}
}