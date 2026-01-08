namespace Echoslate.Core.Services;

public static class AppServices {
	public static IBrushService BrushService { get; private set; }
	public static IMessageDialogService MessageDialogService { get; private set; }

	public static void Initialize(IBrushService brushService, IMessageDialogService messageDialogService) {
		BrushService = brushService;
		MessageDialogService = messageDialogService;
	}
	
}