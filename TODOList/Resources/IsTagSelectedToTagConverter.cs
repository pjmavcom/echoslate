// IsTagSelectedToTagConverter.cs

using System;
using System.Globalization;
using System.Windows.Data;

public class IsTagSelectedToTagConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type t, object p, CultureInfo c)
	{
		// values[0] = the tag string itself (from {Binding .})
		// values[1] = CurrentTagFilter from ViewModel
		if (values[0] is string tag && values[1] is string current && tag == current)
			return "Selected";   // matches the Trigger in TagLikeButton
		return null;
	}

	public object[] ConvertBack(object value, Type[] t, object p, CultureInfo c) 
		=> throw new NotImplementedException();
}