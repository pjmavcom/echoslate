using System.Runtime.CompilerServices;

namespace Echoslate.Core.Models;

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
				string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
				string appFolder = Path.Combine(localAppData, "Echoslate");
				string logsFolder = Path.Combine(appFolder, "Logs");

				bool isDebug = false;
#if DEBUG
				isDebug = true;
#endif
				string fixedCurrent = Path.Combine(appFolder, isDebug ? "DebugLog.txt" : "CurrentLog.txt");
				string activeLogPath = fixedCurrent;

				AppPaths.EnsureFolder(appFolder);
				AppPaths.EnsureFolder(logsFolder);

				if (File.Exists(fixedCurrent)) {
					string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
					string archiveName = isDebug
						? $"Echoslate_DebugLog_{timestamp}.txt"
						: $"Echoslate_Log_{timestamp}.txt";
					string archivePath = Path.Combine(logsFolder, archiveName);

					const int MaxRetries = 3;
					bool archived = false;

					for (int i = 0; i < MaxRetries; i++) {
						try {
							File.Move(fixedCurrent, archivePath, overwrite: true);
							archived = true;
							break;
						} catch (IOException ex) when (IsFileLocked(ex)) {
							if (i == MaxRetries - 1) break;
							Thread.Sleep(200);
						}
					}

					if (!archived) {
						string fallbackName = isDebug
							? $"Echoslate_DebugLog_{timestamp}_fallback.txt"
							: $"Echoslate_Log_{timestamp}_fallback.txt";

						activeLogPath = Path.Combine(logsFolder, fallbackName);

						Print("WARNING: Previous log file is still locked by another process.");
						Print($"         Using fallback log for this session: {activeLogPath}");
					}
				}

				_streamWriter?.Dispose();

				var fs = new FileStream(activeLogPath, FileMode.Create, FileAccess.Write, FileShare.Read);
				_streamWriter = new StreamWriter(fs) {
					AutoFlush = true
				};

				Print("=== Echoslate started ===");
				Print($"Log file: {activeLogPath}");
				Print($"Version: {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}");
				Print($"Keeping max 10 archived log files in {logsFolder}");

				string archivePattern = isDebug ? "Echoslate_DebugLog_*.txt" : "Echoslate_Log_*.txt";
				var logFiles = Directory.GetFiles(logsFolder, archivePattern)
				   .Select(f => new FileInfo(f))
				   .OrderByDescending(f => f.CreationTimeUtc)
				   .ToList();

				foreach (var oldFile in logFiles.Skip(10)) {
					try {
						oldFile.Delete();
					} catch {
					}
				}
			} catch (Exception ex) {
				Console.WriteLine("Failed to initialize logging: " + ex);
			}
		}
	}

	private static bool IsFileLocked(IOException ex) {
		int errorCode = ex.HResult & 0xFFFF;
		return errorCode == 32 || errorCode == 33;
	}

	private static void Write(string msg) {
		lock (_lock) {
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
	public static void Test(object? msg = null,
							string tag = "",
							[CallerMemberName] string member = "",
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
			Print("=== Echoslate Log shutting down ===");
			_streamWriter?.Close();
			_streamWriter?.Dispose();
			_streamWriter = null;
		}
	}
}