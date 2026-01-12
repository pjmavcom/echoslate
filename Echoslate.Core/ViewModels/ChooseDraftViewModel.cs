using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Echoslate.Core.Models;

namespace Echoslate.Core.ViewModels;

public partial class ChooseDraftViewModel : ObservableObject {
	public ObservableCollection<HistoryItem> Drafts { get; }
	public HistoryItem ResultHistoryItem { get; private set; }
	public bool Result { get; set; }

	[ObservableProperty] private HistoryItem _selectedHistoryItem;

	public ChooseDraftViewModel(IEnumerable<HistoryItem> drafts, HistoryItem defaultDraft = null) {
		var uncommitted = drafts.Where(d => !d.IsCommitted).ToList();
		Drafts = new ObservableCollection<HistoryItem>(uncommitted);

		SelectedHistoryItem = defaultDraft ?? uncommitted.FirstOrDefault();
	}
	public void SetResult() {
		ResultHistoryItem = SelectedHistoryItem;
		Result = true;
	}
}