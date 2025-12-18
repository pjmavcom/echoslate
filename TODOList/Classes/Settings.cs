using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;

namespace Echoslate {
	public class Settings {
		public string BasePath { get; set; }
		public string SettingsFileName { get; set; }
		public ObservableCollection<string> RecentFiles { get; set; }

		public Rectangle Window { get; set; }
		public int PomoWorkTimerLength;
		public int PomoBreakTimerLength;
		public bool GlobalHotkeysEnabled;
		public int PreviousSessionLastActiveTab;
		

		public Settings(string basePath, string settingsFileName) {
			BasePath = basePath;
			SettingsFileName = settingsFileName;
			LoadSettings();
		}

		private void LoadSettings() {
			RecentFiles = new ObservableCollection<string>();

			string filePath = BasePath + SettingsFileName;
			if (!File.Exists(filePath)) {
				Log.Print("Settings file does not exist. Creating new settings file.");
				SaveSettings();
			}
			StreamReader stream = new StreamReader(File.Open(filePath, FileMode.Open));

			Log.Print($"Loading settings file: {filePath}");
			if (LoadV2_1Settings(stream)) {
				Log.Print("v2.1 settings loaded.");
			} else {
				stream.Close();
				FixCorruptedSettingsFile();
			}
			stream.Close();
		}
		private void FixCorruptedSettingsFile() {
			Log.Print("Previous settings file corrupted. Create a new settings?");
			DlgYesNo dlgYesNo = new DlgYesNo("Corrupted or missing file",
											 "Error with the settings file, create a new one?");
			dlgYesNo.ShowDialog();
			if (dlgYesNo.Result) {
				SaveSettings();
				Log.Print("YES- Created new settings file.");
				DlgYesNo dlg;
				dlg = new DlgYesNo("New settings file created");
				dlg.ShowDialog();
			} else {
				Log.Print("NO- Not creating new settings file.");
			}
		}
		private bool LoadV2_1Settings(StreamReader stream) {
			string line = stream.ReadLine();
			if (line != "RECENTFILES") {
				return false;
			}

			Log.Print("Reading RECENTFILES...");
			while (line != null) {
				line = stream.ReadLine();
				if (line == "RECENTFILES" || line == "") {
					continue;
				}
				if (line == "WINDOWPOSITION") {
					break;
				}

				if (File.Exists(line)) {
					RecentFiles.Add(line);
					Log.Print($"Added {line} to RecentFiles");
				} else {
					Log.Warn($"File does not exist: {line}");
				}
			}

			if (line == "WINDOWPOSITION") {
				Log.Print("Reading WINDOWPOSITION...");
				Window = Window with { Y = Convert.ToInt16(stream.ReadLine()) };
				Window = Window with { X = Convert.ToInt16(stream.ReadLine()) };
				Window = Window with { Height = Convert.ToInt16(stream.ReadLine()) };
				Window = Window with { Width = Convert.ToInt16(stream.ReadLine()) };
				Log.Print($"Set window position: ({Window.Y}, {Window.X}) and size: ({Window.Height}, {Window.Width})");
			} else {
				Log.Error("WINDOWPOSITION could not be found.");
				return false;
			}

			line = stream.ReadLine();
			if (line == "POMOTIMERSETTINGS") {
				Log.Print("Reading POMOTIMERSETTINGS...");
				PomoWorkTimerLength = Convert.ToInt16(stream.ReadLine());
				PomoBreakTimerLength = Convert.ToInt16(stream.ReadLine());
				Log.Print($"Pomodoro timer set to {PomoWorkTimerLength} / {PomoBreakTimerLength}");
			} else {
				Log.Error("POMOTIMERSETTINGS could not be found.");
				return false;
			}

			line = stream.ReadLine();
			if (line == "GLOBALHOTKEYS") {
				Log.Print("Reading GLOBALHOTKEYS...");
				GlobalHotkeysEnabled = Convert.ToBoolean(stream.ReadLine());
				Log.Print($"Global hotkeys set to: {GlobalHotkeysEnabled}");
			} else {
				Log.Error("GLOBALHOTKEYS could not be found.");
				return false;
			}

			line = stream.ReadLine();
			if (line == "PREVIOUSSESSIONLASTACTIVETAB") {
				Log.Print("Reading PREVIOUSSESSIONLASTACTIVETAB...");
				stream.ReadLine();
				// PreviousSessionLastActiveTab = Convert.ToInt16(stream.ReadLine());
				// Log.Print($"Previous tab set to {PreviousSessionLastActiveTab}");
			} else {
				Log.Error("PREVIOUSSESSIONLASTACTIVETAB could not be found.");
				return false;
			}

			return true;
		}
		private void SaveSettings() {
			string filePath = BasePath + SettingsFileName;
			StreamWriter stream = new StreamWriter(File.Open(filePath, FileMode.Create));

			stream.WriteLine("RECENTFILES");
			foreach (string s in RecentFiles) {
				if (s == "") {
					continue;
				}
				stream.WriteLine(s);
			}

			stream.WriteLine("WINDOWPOSITION");
			stream.WriteLine(Window.Y);
			stream.WriteLine(Window.X);
			stream.WriteLine(Window.Height);
			stream.WriteLine(Window.Width);
			stream.WriteLine("POMOTIMERSETTINGS");
			stream.WriteLine(PomoWorkTimerLength);
			stream.WriteLine(PomoBreakTimerLength);
			stream.WriteLine("GLOBALHOTKEYS");
			stream.WriteLine(GlobalHotkeysEnabled);
			stream.WriteLine("PREVIOUSSESSIONLASTACTIVETAB");
			stream.WriteLine(0);

			stream.Close();
		}
		public void SortRecentFiles(string recent) {
			Log.Print($"Sorting {recent} to top of list.");
			if (RecentFiles.Contains(recent)) {
				RecentFiles.Remove(recent);
			}
			RecentFiles.Insert(0, recent);

			while (RecentFiles.Count >= 10) {
				Log.Print($"Removing excess file: {RecentFiles[RecentFiles.Count - 1]}");
				RecentFiles.RemoveAt(RecentFiles.Count - 1);
			}
		}
	}
}