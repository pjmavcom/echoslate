/*	TagHolder.cs
 * 22-Mar-2019
 * 16:00:52
 *
 * 
 */

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TODOList
{
	public class TagHolder : INotifyPropertyChanged
	{
		public string Text { get; set; }
		public TagHolder(string tag)
		{
			Text = tag;
		}
		
		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}