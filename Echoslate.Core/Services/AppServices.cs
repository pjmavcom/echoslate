namespace Echoslate.Core.Services;

public static class AppServices {
	public static IApplicationService ApplicationService { get; private set; }
	public static IBrushService BrushService { get; private set; }
	public static IMessageDialogService MessageDialogService { get; private set; }
	public static IFileDialogService FileDialogService { get; private set; }
	public static IDispatcherService DispatcherService { get; private set; }
	public static IClipboardService ClipboardService { get; private set; }
	public static IDialogService DialogService { get; private set; }

	public static void Initialize(IApplicationService applicationService, IBrushService brushService, IMessageDialogService messageDialogService, IFileDialogService fileDialogService, IDispatcherService dispatcherService, IClipboardService clipboardService, IDialogService dialogService) {
		ApplicationService = applicationService;
		BrushService = brushService;
		MessageDialogService = messageDialogService;
		FileDialogService = fileDialogService;
		DispatcherService = dispatcherService;
		ClipboardService = clipboardService;
		DialogService = dialogService;
	}
}