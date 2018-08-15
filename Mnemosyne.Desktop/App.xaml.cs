using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Mnemosyne.Desktop
{
	public partial class App : Application
	{
		public App()
		{
			var logFolderPath = Path.Combine(System.Windows.Forms.Application.LocalUserAppDataPath, "Logs");

			if (!Directory.Exists(logFolderPath))
				Directory.CreateDirectory(logFolderPath);

			var date = DateTime.Now;
			var filename = string.Format("{0}-Mnemosyne.log", date.ToString("yyyy-MM-dd"));
			var logFilePath = Path.Combine(logFolderPath, filename);

			var stream = new FileStream(logFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
			stream.Position = stream.Length;

			Trace.Listeners.Add(new TextWriterTraceListener(stream));

			Startup += App_Startup;
			Exit += App_Exit;
		}

		private void App_Startup(object sender, StartupEventArgs e)
		{
			var message = "Starting.";
			Trace.TraceInformation(message);
			Trace.Flush();
		}

		private void App_Exit(object sender, ExitEventArgs e)
		{
			var message = string.Format("Closing with return code {0}.", e.ApplicationExitCode);
			Trace.TraceInformation(message);
			Trace.Flush();
		}
	}
}
