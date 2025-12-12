/*	TagHolder.cs
 * 22-Mar-2019
 * 16:00:52
 *
 * 
 */

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Echoslate
{
	public class TagHolder
	{
		public string Text { get; set; }
		public TagHolder(string tag)
		{
			Text = tag;
		}
	}
}
