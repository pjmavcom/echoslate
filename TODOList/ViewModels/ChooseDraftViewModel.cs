using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Echoslate.Windows {
	public partial class ChooseDraftViewModel : ObservableObject {
		public ObservableCollection<HistoryItem> Drafts { get; }

		[ObservableProperty] private HistoryItem selectedHistoryItem;

		public ChooseDraftViewModel(IEnumerable<HistoryItem> drafts, HistoryItem defaultDraft = null) {
			var uncommitted = drafts.Where(d => !d.IsCommitted).ToList();
			Drafts = new ObservableCollection<HistoryItem>(uncommitted);

			SelectedHistoryItem = defaultDraft ?? uncommitted.FirstOrDefault();
		}
	}
}