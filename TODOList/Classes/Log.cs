using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

public static class Log {
	private static StreamWriter _streamWriter;
	private static readonly object _lock = new object();

	public static string InfoString = " [INFO] ";
	public static string WarningString = " [WARNING] ";
	public static string ErrorString = " [ERROR] ";
	public static string DebugString = " [DEBUG] ";
	public static string SuccessString = " [SUCCESS] ";
	public static string TestingString = " [TESTING] ";


	public static void Initialize() {
		lock (_lock) {
			try {
				string exeDir = AppDomain.CurrentDomain.BaseDirectory;
				string baseName = "Echoslate_Log";
				string extension = ".txt";

				string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
				string newLogPath = Path.Combine(exeDir, $"{baseName}_{timestamp}{extension}");

				_streamWriter = new StreamWriter(newLogPath, append: false) { AutoFlush = true };

				var logFiles = Directory.GetFiles(exeDir, $"{baseName}_*{extension}")
				   .Select(f => new FileInfo(f))
				   .OrderByDescending(f => f.CreationTime)
				   .ToList();

				foreach (var oldFile in logFiles.Skip(10)) {
					try {
						oldFile.Delete();
					} catch {
					}
				}

				Print("=== Todo App started ===");
				Print($"Log file: {newLogPath}");
				Print($"Version: {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}");
				Print($"Keeping max 10 log files — old ones auto-deleted");
			} catch (Exception ex) {
				Console.WriteLine("Failed to initialize logging: " + ex);
			}
		}
	}

	private static void Write(string msg) {
		lock (_lock) {
			// Console.WriteLine(msg);
			if (Console.OpenStandardOutput() != Stream.Null) {
				Console.WriteLine(msg);
			}
			_streamWriter?.WriteLine(msg);
		}
	}
	private static string Prefix(string level,
								 string tag = "",
								 [CallerMemberName] string member = "",
								 [CallerFilePath] string file = "",
								 [CallerLineNumber] int line = 0) {
		string time = DateTime.Now.ToString("HH:mm:ss.fff");
		string shortFile = System.IO.Path.GetFileName(file);
		return $"[{time}] {level} {tag} [{shortFile}:{line} {member}]";
	}
	public static void Print(object msg,
							 string tag = "",
							 [CallerMemberName] string member = "",
							 [CallerFilePath] string file = "",
							 [CallerLineNumber] int line = 0) {
		Write($"{Prefix(InfoString, tag, member, file, line)} {msg}");
	}
	public static void Warn(object msg,
							string tag = "",
							[CallerMemberName] string member = "",
							[CallerFilePath] string file = "",
							[CallerLineNumber] int line = 0) {
		Write($"{Prefix(WarningString, tag, member, file, line)} {msg}");
	}
	public static void Debug(object msg,
							 string tag = "",
							 [CallerMemberName] string member = "",
							 [CallerFilePath] string file = "",
							 [CallerLineNumber] int line = 0) {
		Write($"{Prefix(DebugString, tag, member, file, line)} {msg}");
	}
	public static void Success(object msg,
							   string tag = "",
							   [CallerMemberName] string member = "",
							   [CallerFilePath] string file = "",
							   [CallerLineNumber] int line = 0) {
		Write($"{Prefix(SuccessString, tag, member, file, line)} {msg}");
	}
	public static void Test(object? msg = null, string tag = "", [CallerMemberName] string member = "",
							[CallerFilePath] string file = "",
							[CallerLineNumber] int line = 0) {
		if (msg == null) {
			Write($"{Prefix(TestingString, tag, member, file, line)} Test");
		} else {
			Write($"{Prefix(TestingString, tag, member, file, line)} {msg}");
		}
	}
	public static void Error(object msg,
							 string tag = "",
							 [CallerMemberName] string member = "",
							 [CallerFilePath] string file = "",
							 [CallerLineNumber] int line = 0) {
		if (msg == null) {
			Write($"{Prefix(ErrorString, tag, member, file, line)} Object not found!");
			return;
		}
		Write($"{Prefix(ErrorString, tag, member, file, line)} {msg}");
	}
	public static void Shutdown() {
		lock (_lock) {
			Print("=== Echoslate shutting down ===");
			_streamWriter?.Close();
			_streamWriter?.Dispose();
			_streamWriter = null;
		}
	}
}