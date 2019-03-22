using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TODOList
{
	public partial class DlgTodoMultiItemEditor : Window
	{
		private int currentSeverity;
		private readonly TodoItem td;
		public TodoItem Result => td;
		public List<string> ResultTags;
		public bool isOk;
		public bool isComplete;
		private readonly int previousRank;

		private int _rank;

		private bool changeSev;
		private bool changeRank;
		private bool changeComplete;
		private bool changeTodo;
		public bool ChangeSev => changeSev;
		public bool ChangeRank => changeRank;
		public bool ChangeComplete => changeComplete;
		public bool ChangeTodo => changeTodo;
		public bool ChangeTag { get; set; }

		public List<TagHolder> Tags { get; set; }
		
		
		
		public DlgTodoMultiItemEditor(TodoItem td, List<string> tags)
		{
			InitializeComponent();

			Tags = new List<TagHolder>();
			foreach (string tag in tags)
				Tags.Add(new TagHolder(tag));
			
			this.td = new TodoItem(td.ToString())
			{
				IsTimerOn = td.IsTimerOn
			};
			currentSeverity = this.td.Severity;

			cbSev.SelectedIndex = currentSeverity;
			_rank = td.Rank;
			tbRank.Text = _rank.ToString();
			previousRank = td.Rank;
			
			CenterWindowOnMouse();
			btnComplete.Content = td.IsComplete ? "Reactivate" : "Complete";
			lbTags.ItemsSource = Tags;
			lbTags.Items.Refresh();
		}
		private void CenterWindowOnMouse()
		{
			Window win = Application.Current.MainWindow;

			if (win == null)
				return;
			double centerX = win.Width / 2 + win.Left;
			double centerY = win.Height / 2 + win.Top;
			Left = centerX - Width / 2;
			Top = centerY - Height / 2;
		}
		private void cbTSeverity_SelectionChanged(object sender, EventArgs e)
		{
			if (sender is ComboBox rb) currentSeverity = rb.SelectedIndex;
		}
		private void DeleteTag_OnClick(object sender, EventArgs e)
		{
			if (sender is Button b)
			{
				TagHolder th = b.DataContext as TagHolder;
				Tags.Remove(th);
				lbTags.Items.Refresh();
			}
		}
		private void AddTag_OnClick(object sender, EventArgs e)
		{
			string name = "#NewTag";
			int tagNumber = 0;
			bool nameExists = false;
			do
			{
				foreach (TagHolder t in Tags)
				{
					if (t.Text == name + tagNumber.ToString())
					{
						tagNumber++;
						nameExists = true;
						break;
					}
					else 
						nameExists = false;
				}
			} while (nameExists);
			TagHolder th = new TagHolder(name + tagNumber);
			Tags.Add(th);
			lbTags.Items.Refresh();
		}
		private void ckTags_Clicked(object sender, EventArgs e)
		{
			ChangeTag = (bool) ckTags.IsChecked;
		}
		private void ckRank_Clicked(object sender, EventArgs e)
		{
			changeRank = (bool) ckRank.IsChecked;
		}
		
		private void ckComplete_Clicked(object sender, EventArgs e)
		{
			changeComplete = (bool) ckComplete.IsChecked;
		}
		
		private void ckSev_Clicked(object sender, EventArgs e)
		{
			changeSev = (bool) ckSev.IsChecked;
		}
		
		private void ckTodo_Clicked(object sender, EventArgs e)
		{
			changeTodo = (bool) ckTodo.IsChecked;
		}
		
		private void btnRank_Click(object sender, EventArgs e)
		{
			Button b = sender as Button;

			if (b == null)
				return;
			string compar = (string) b.CommandParameter;

			
			if (compar == "up")
			{
				_rank--;
			}
			else if (compar == "down")
			{
				_rank++;
			}
			else if (compar == "top")
			{
				_rank = 0;
			}
			else if (compar == "bottom")
			{
				_rank = int.MaxValue;
			}

			_rank = _rank > 0 ? _rank : 0;
			tbRank.Text = _rank.ToString();
			td.Rank = _rank;
		}

		// METHOD  ///////////////////////////////////// btnOK() //
		private void btnOK_Click(object sender, EventArgs e)
		{
			string tempTodo = MainWindow.ExpandHashTagsInString(tbTodo.Text);
//			string tempTags = MainWindow.ExpandHashTagsInString(tbTags.Text);
			td.Todo = /*tempTags.Trim() + " " + */tempTodo.Trim();
//			td.Todo = tbTodo.Text;
			td.Severity = currentSeverity;
			isOk = true;
			
			if (previousRank > td.Rank)
				td.Rank--;

			ResultTags = new List<string>();
			foreach (TagHolder th in Tags)
				ResultTags.Add(th.Text);
			
			Close();
		}

		// METHOD  ///////////////////////////////////// btnComplete_Click() //
		private void btnComplete_Click(object sender, EventArgs e)
		{
			isOk = true;
			isComplete = true;
			td.IsComplete = !td.IsComplete;
			
			string tempTodo = MainWindow.ExpandHashTagsInString(tbTodo.Text);
//			string tempTags = MainWindow.ExpandHashTagsInString(tbTags.Text);
			td.Todo = /*tempTags.Trim() + " " + */tempTodo.Trim();
//			td.Todo = tbTodo.Text;
			td.ParseTags();
			td.Severity = currentSeverity;
			Close();
		}

		// METHOD  ///////////////////////////////////// btnCancel() //
		private void btnCancel_Click(object sender, EventArgs e)
		{
			Close();
		}
		
		private void tbRank_Changed(object sender, EventArgs e)
		{
			if (tbRank.Text == "")
				tbRank.Text = "0";
			td.Rank = Convert.ToInt32(tbRank.Text);
		}
		
		private void tbRank_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			var textBox = sender as TextBox;
			// Use SelectionStart property to find the caret position.
			// Insert the previewed text into the existing text in the textbox.
			var fullText = textBox.Text.Insert(textBox.SelectionStart, e.Text);
		
			double val;
			// If parsing is successful, set Handled to false
			e.Handled = !double.TryParse(fullText, out val);
		}
	}
}
