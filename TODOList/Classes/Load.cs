using System;
using System.Collections.Generic;
using System.IO;

namespace Echoslate;

public class Load {
	public string FilePath;
	public List<TodoItem> MasterList;
	public List<HistoryItem> HistoryItems;
	public List<string> FilterTags;
	

	public Load(string filePath) {
		FilePath = filePath;

		ClearLists();
		Load2_1SaveFile();
	}
	private void ClearLists() {
		MasterList = new List<TodoItem>();
		HistoryItems = new List<HistoryItem>();
		FilterTags = new List<string>();
	}
	
	private void Load2_1SaveFile() {
		StreamReader stream = new StreamReader(File.Open(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));

		string? line;
		while (true) {
			line = stream.ReadLine();
			if (line != null && line.Contains("=====TABS"))
				continue;
			if (line != null && line.Contains("=====FILESETTINGS"))
				break;

			FilterTags.Add(line);
		}

		stream.ReadLine();
		stream.ReadLine();
		// _backupIncrement = Convert.ToInt16(stream.ReadLine());
		stream.ReadLine();
		stream.ReadLine();
		// int backupMinutes = Convert.ToInt16(stream.ReadLine());
		// _backupTime = new TimeSpan(0, backupMinutes, 0);
		stream.ReadLine();
		stream.ReadLine();
		// _autoBackup = Convert.ToBoolean(stream.ReadLine());
		stream.ReadLine();
		stream.ReadLine();
		// _autoSave = Convert.ToBoolean(stream.ReadLine());
		stream.ReadLine();
		stream.ReadLine();
		// ConvertProjectVersion(stream.ReadLine());

		while (true) {
			line = stream.ReadLine();
			if (line != null && line.Contains("=====VCS"))
				break;

			if (line != null && line.Contains("=====TODO"))
				continue;

			TodoItem td = new TodoItem(line);
			MasterList.Add(td);
		}

		List<string> history = [];
		while (line  != null) {
			line = stream.ReadLine();
			switch (line) {
				case "NewVCS":
					history = [];
					continue;
				case "EndVCS":
					HistoryItems.Add(new HistoryItem(history));
					continue;
				default:
					if (line != null) {
						history.Add(line);
					}
					break;
			}
		}
		stream.Close();
	}
}