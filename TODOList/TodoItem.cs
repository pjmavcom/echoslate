/*	TodoItem.cs
 * 07-Feb-2019
 * 09:59:56
 *
 * 
 */

using System;

namespace TODOList
{
	[Serializable]
	public class TodoItem
	{
		// FIELDS //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// FIELDS //


		// PROPERTIES //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// PROPERTIES //
		public string Todo { get; set; }
		public string Time { get; set; }
		public int Severity { get; set; }
		public string CompletedTime { get; set; }
		public bool Complete { get; set; }

		// CONSTRUCTORS //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// CONSTRUCTORS //
		public TodoItem()
		{
			Todo = "";
			Time = DateTime.Now.ToString();
			CompletedTime = " ----- ----- ";
			Severity = 0;
		}
		public TodoItem(string newItem)
		{
			string[] pieces = newItem.Split('|');
			Time = pieces[0].Trim();
			Severity = Convert.ToInt16(pieces[2]);
			Todo = pieces[3].Trim();
			Complete = Convert.ToBoolean(pieces[4]);
			CompletedTime = pieces[5].Trim();
		}

		// MONOGAME METHODS //////////////////////////////////////////////////////////////////////////////////////////////////////////////// MONOGAME METHODS //


		// METHODS ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// METHODS //
		// METHOD  ///////////////////////////////////// ToString() //
		public override string ToString()
		{
			return Time + "| Severity | " + Severity + " | " + Todo + " | " + Complete + " | " + CompletedTime;
		}
	}
}