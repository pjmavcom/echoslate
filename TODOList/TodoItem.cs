/*	TodoItem.cs
 * 07-Feb-2019
 * 09:59:56
 *
 *
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using static System.Globalization.CultureInfo;

namespace TODOList
{
    [Serializable]
    public class TodoItem : INotifyPropertyChanged
    {
        // FIELDS //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// FIELDS //
        private string _todo = "";
        private string _notes = "";
        private string _problem = "";
        private string _solution = "";
        private string _dateStarted = "";
        private string _timeStarted = "";
        private string _dateCompleted = "";
        private string _timeCompleted = "";
        private DateTime _timeTaken;
        private long _timeTakenInMinutes;
        private bool _isTimerOn;
        private bool _isComplete;
        private int _severity;
        private int _kanban;
        private int _kanbanRank;
        private Dictionary<string, int> _rank;
        private List<string> _tags;

        // PROPERTIES //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// PROPERTIES //
        public string Todo
        {
            get => AddNewLines(_todo);
            set
            {
                _todo = ParseNotes(value);
                ParseNewTags();
            }
        }
        private string TagsAndTodoToSave
        {
            get
            {
                string result = "";
                foreach (string t in _tags)
                {
                    string[] pieces = t.Split('\r');
                    result = pieces.Where(p => p != "").Aggregate(result, (current, p) => current + (p.Trim() + " "));
                }

                result += _todo;
                return AddNewLines(result);
            }
        }
        public string Notes
        {
            get => AddNewLines(_notes);
            set => _notes = ParseNotes(value);
        }
        public string Problem
        {
            get => AddNewLines(_problem);
            set => _problem = ParseNotes(value);
        }
        public string Solution
        {
            get => AddNewLines(_solution);
            set => _solution = ParseNotes(value);
        }
        public string NotesAndTags => "Notes: " + Environment.NewLine + AddNewLines(Notes) + Environment.NewLine + "Problem: " + Environment.NewLine + AddNewLines(Problem) + Environment.NewLine + "Solution: " +
                                      Environment.NewLine + AddNewLines(Solution) + Environment.NewLine + "Tags:" + Environment.NewLine + TagsList;
        public string StartDateTime => _dateStarted + "" + "_" + _timeStarted;
        public string DateStarted => _dateStarted;
        public string TimeStarted => _timeStarted;
        public int Severity
        {
            get => _severity;
            set => _severity = value;
        }
        public int Kanban
        {
            get => _kanban;
            set => _kanban = value;
        }
        public int KanbanRank
        {
            get => _kanbanRank;
            set => _kanbanRank = value;
        }
        private string DateCompleted
        {
            set => _dateCompleted = value;
        }
        private string TimeCompleted
        {
            set => _timeCompleted = value;
        }
        public DateTime TimeTaken
        {
            get => _timeTaken;
            set
            {
                _timeTaken = value;
                TimeTakenInMinutes = _timeTaken.Ticks / TimeSpan.TicksPerMinute;
                OnPropertyChanged();
            }
        }
        public long TimeTakenInMinutes
        {
            get => _timeTakenInMinutes;
            private set
            {
                _timeTakenInMinutes = value;
                OnPropertyChanged();
            }
        }
        public bool IsTimerOn
        {
            get => _isTimerOn;
            set => _isTimerOn = value;
        }
        public bool IsComplete
        {
            get => _isComplete;
            set
            {
                _isComplete = value;
                DateCompleted = IsComplete ? DateTime.Now.ToString(MainWindow.DATE_STRING_FORMAT) : "-";
                TimeCompleted = IsComplete ? DateTime.Now.ToString(MainWindow.TIME_STRING_FORMAT) : "-";
            }
        }
        public Dictionary<string, int> Rank
        {
            get => _rank;
            set => _rank = value;
        }
        public string Ranks
        {
            get
            {
                string result = "";
                foreach (KeyValuePair<string, int> kvp in Rank)
                    result += kvp.Key + " # " + kvp.Value + ",";
                return result;
            }
        }
        public List<string> Tags
        {
            get => _tags;
            set => _tags = value;
        }
        private string TagsList
        {
            get
            {
                string result = "";
                if (_tags.Count != 0)
                    result = _tags[0];
                for (int i = 1; i < _tags.Count; i++)
                    result += Environment.NewLine + _tags[i];

                return result;
            }
        }
        public string TagsSorted
        {
            get
            {
                string result = "";
                for (int i = 0; i < _tags.Count; i++)
                {
                    result += _tags[i];
                    if (i != _tags.Count)
                        result += " "; // Environment.NewLine;
                }

                return result;
            }
        }

        // CONSTRUCTORS //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// CONSTRUCTORS //
        public TodoItem()
        {
            _tags = new List<string>();
            _todo = "";
            _notes = "";
            _problem = "";
            _solution = "";
            _dateStarted = DateTime.Now.ToString("yyyy/MM/dd");
            _timeStarted = DateTime.Now.ToString("HH:mm");
            _dateCompleted = "-";
            _timeCompleted = "-";
            _severity = 0;
            _rank = new Dictionary<string, int>();
        }
        public TodoItem(string newItem)
        {
            _tags = new List<string>();
            _rank = new Dictionary<string, int>();
            Load3_20(newItem);
        }
        private void FixDateTime()
        {
            if (!_dateStarted.Contains("-"))
            {
                _dateStarted = _dateStarted.Insert(4, "-");
                _dateStarted = _dateStarted.Insert(7, "-");
            }

            if (!_timeStarted.Contains(":"))
            {
                _timeStarted = _timeStarted.Insert(2, ":");
                _timeStarted = _timeStarted.Substring(0, 5);
            }
        }
        private void Load3_20(string newItem)
        {
            string[] pieces = newItem.Split('|');
            if (pieces.Length < 13)
            {
                new DlgErrorMessage("This save file is corrupted! A todo did not load!" + Environment.NewLine + newItem).ShowDialog();
                return;
            }
            _dateStarted = pieces[0].Trim();
            _timeStarted = pieces[1].Trim();
            _dateCompleted = pieces[2].Trim();
            _timeCompleted = pieces[3].Trim();

            TimeTaken = new DateTime(Convert.ToInt64(pieces[4].Trim()));

            _isComplete = Convert.ToBoolean(pieces[5]);

            string[] rankPieces = pieces[6].Split(',');
            foreach (string s in rankPieces)
            {
                if (s == "") continue;
                string[] rank = s.Split('#');
                _rank.Add(rank[0].Trim(), Convert.ToInt32(rank[1].Trim()));
            }

            _severity = Convert.ToInt32(pieces[7]);
            Todo = pieces[8].Trim();

            if (pieces.Length > 9) Notes = pieces[9].Trim();
            if (pieces.Length > 10) Kanban = Convert.ToInt32(pieces[10].Trim());
            if (pieces.Length > 11) KanbanRank = Convert.ToInt32(pieces[11].Trim());
            if (pieces.Length > 12) Problem = pieces[12];
            if (pieces.Length > 13) Solution = pieces[13];
        }
        private void ParseNewTags()
        {
            _todo = TagsAndTodoToSave;
            ParseTags();
        }
        private void ParseTags()
        {
            _tags = new List<string>();
            string[] tempPieces = _todo.Split('\r');
            string temp = tempPieces.Aggregate("", (current, s) => current + (s + " "));
            tempPieces = temp.Split('\n');
            temp = tempPieces.Aggregate("", (current, s) => current + (s + " "));

            string[] pieces = temp.Split(' ');
            bool isBeginningTag = false;

            List<string> list = new List<string>();
            for (int index = 0; index < pieces.Length; index++)
            {
                string s = pieces[index];
                if (s == "") continue;

                if (s.Contains('#'))
                {
                    if (index == 0) isBeginningTag = true;

                    var t = s.ToUpper();
                    if (t.Equals("#FEATURES") || t.Equals("#F")) t = "#FEATURE";
                    if (t.Equals("#BUGS") || t.Equals("#B")) t = "#BUG";

                    if (!_tags.Contains(t)) _tags.Add(t);

                    s = s.Remove(0, 1);
                    s = s.ToLower();
                    if (s.Equals("f")) s = "feature";
                    if (s.Equals("b")) s = "bug";
                }
                else
                {
                    isBeginningTag = false;
                }

                if (isBeginningTag) continue;
                
                if (index == 0 ||
                    index > 0 && pieces[index - 1].Contains(". ") ||
                    index > 0 && pieces[index - 1].Contains("? ") ||
                    list.Count == 0)
                {
                    s = UpperFirstLetter(s);
                }

                list.Add(s);
            }

            string tempTodo = "";
            foreach (string s in list)
            {
                if (s == "") continue;
                tempTodo += s + " ";
            }

            _todo = tempTodo.Trim();
        }
        private string ParseNotes(string notesToParse)
        {
            string[] pieces = notesToParse.Split(' ');
            List<string> list = new List<string>();
            for (int index = 0; index < pieces.Length; index++)
            {
                string s = pieces[index];
                if (index == 0 ||
                    index > 0 && pieces[index - 1].Contains(".") ||
                    index > 0 && pieces[index - 1].Contains("?") ||
                    list.Count == 0)
                {
                    s = UpperFirstLetter(s);
                }

                if (s.Contains("/n") || s.Contains(Environment.NewLine))
                    s = UpperFirstLetterOfNewLine(s);
                list.Add(s);
            }

            string tempNotes = "";
            foreach (string s in list)
            {
                if (s == "")
                    continue;
                tempNotes += s + " ";
            }

            return tempNotes.Trim();
        }
        private void ParseTodo()
        {
            string[] pieces = _todo.Split(' ');
            List<string> list = new List<string>();
            for (int index = 0; index < pieces.Length; index++)
            {
                string s = pieces[index];
                if (index == 0 ||
                    index > 0 && pieces[index - 1].Contains(".") ||
                    index > 0 && pieces[index - 1].Contains("?") ||
                    list.Count == 0)
                {
                    s = UpperFirstLetter(s);
                }

                list.Add(s);
            }

            string tempTodo = "";
            foreach (string s in list)
            {
                if (s == "") continue;
                tempTodo += s + " ";
            }

            _todo = tempTodo.Trim();
        }
        private string UpperFirstLetter(string s)
        {
            string result = "";
            for (int i = 0; i < s.Length; i++)
            {
                if (i == 0)
                {
                    result += s[i].ToString().ToUpper();
                }
                else
                {
                    result += s[i];
                }
            }

            return result;
        }
        private string UpperFirstLetterOfNewLine(string s)
        {
            s = RemoveNewLines(s);
            string[] parts = s.Split('/');
            string newString = string.Empty;
            int count = 0;
            foreach (string part in parts)
            {
                if (count == 0)
                {
                    count++;
                    newString += part;
                    continue;
                }
                if (part[0] == 'n')
                    newString += "/n" + UpperFirstLetter(part.Remove(0, 1));
                else
                    newString += "/" + part;
            }
            return newString;
        }
        public override string ToString()
        {
            string notes = _notes;
            string problem = _problem;
            string solution = _solution;
            notes = RemoveNewLines(notes);
            problem = RemoveNewLines(problem);
            solution = RemoveNewLines(solution);

            string result = _dateStarted + "|" +
                              _timeStarted + "|" +
                              _dateCompleted + "|" +
                              _timeCompleted + "|" +
                              _timeTaken.Ticks + "|" +
                              _isComplete + "|" +
                              Ranks + "|" +
                              _severity + "|" +
                              TagsAndTodoToSave + "|" +
                              notes + "|" +
                              _kanban + "|" +
                              _kanbanRank + "|" +
                              problem + "|" +
                              solution;
            return result;
        }
        public string ToClipboard()
        {
            string notes = AddNewLines(_notes);
            string problem = AddNewLines(_problem);
            string solution = AddNewLines(_solution);

            string result = _dateCompleted + "-" + TimeTakenInMinutes + "m |" + BreakLines(_todo);
            if (_notes != "")
                result += Environment.NewLine + "\tNotes: " + BreakLines(notes);
            if (_problem != "")
                result += Environment.NewLine + "\tProblem: " + BreakLines(problem);
            if (_solution != "")
                result += Environment.NewLine + "\tSolution: " + BreakLines(solution);

            return result;
        }
        private string BreakLines(string s)
        {
            int charLimit = 100;
            int currentCharCount = 0;
            string result = "";
            string[] pieces = s.Split(' ');
            foreach (string word in pieces)
            {
                currentCharCount += word.Length + 1;

                if (currentCharCount <= charLimit)
                    result += word + " ";
                else
                {
                    currentCharCount = 0;
                    result += Environment.NewLine + "\t\t" + word + " ";
                }
            }

            return result;
        }
        private string AddNewLines(string s)
        {
            return s.Replace("/n", Environment.NewLine);
        }
        private string RemoveNewLines(string s)
        {
            return s.Replace(Environment.NewLine, "/n");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}