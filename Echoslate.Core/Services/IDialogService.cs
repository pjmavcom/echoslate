namespace Echoslate.Core.Services;

public interface IDialogService {
	Task<bool> ShowDialogAsync(object view, string title);
	Task<T?> ShowDialogAsync<T>(object view, string title);
}