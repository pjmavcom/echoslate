using System.Collections.ObjectModel;

namespace Echoslate.Core.Resources;

public static class ObservableCollectionExtensions {
	public static void Sort<T>(this ObservableCollection<T> collection) where T : IComparable<T> {
		var sorted = collection.OrderBy(x => x).ToList();

		for (int i = 0; i < sorted.Count; i++) {
			collection.Move(collection.IndexOf(sorted[i]), i);
		}
	}

	public static void Sort(this ObservableCollection<string> collection, StringComparison comparison = StringComparison.OrdinalIgnoreCase) {
		var sorted = collection.OrderBy(x => x, StringComparer.Create(System.Globalization.CultureInfo.CurrentCulture, comparison == StringComparison.OrdinalIgnoreCase)).ToList();

		for (int i = 0; i < sorted.Count; i++) {
			collection.Move(collection.IndexOf(sorted[i]), i);
		}
	}
}