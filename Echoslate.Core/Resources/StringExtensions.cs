namespace Echoslate.Core.Resources;

public static class StringExtensions {
	public static string CapitalizeFirstLetter(this string s) {
		if (string.IsNullOrWhiteSpace(s)) {
			return s ?? "";
		}
		if (s.Length == 1) {
			return char.ToUpperInvariant(s[0]).ToString();
		}

		return char.ToUpperInvariant(s[0]) + s.Substring(1);
	}
}
