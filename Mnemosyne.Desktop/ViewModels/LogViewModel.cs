using Mnemosyne.Desktop.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mnemosyne.Desktop.ViewModels
{
	public class LogViewModel : CommonViewModel
	{
		public ObservableCollection<FileInfo> LogFiles { get; }

		public FileInfo CurrentLogFile
		{
			get => currentLogFile;
			set
			{
				if (value != currentLogFile)
				{
					currentLogFile = value;
					Notify(nameof(CurrentLogFile));
				}
			}
		}

		public string LogData
		{
			get => logData;
			set
			{
				if (value != logData)
				{
					logData = value;
					Notify(nameof(LogData));
				}
			}
		}

		public RelayAction CMDGetLogFiles { get; }

		public RelayAction CMDOpenLogFile { get; }

		private readonly DirectoryInfo logFolder = new DirectoryInfo(Path.Combine(Application.LocalUserAppDataPath, "Logs"));

		private FileInfo currentLogFile;

		private string logData;

		public LogViewModel()
		{
			LogFiles = new ObservableCollection<FileInfo>();

			CMDGetLogFiles = new RelayAction((param) => GetLogFiles(), (param) => true);

			CMDOpenLogFile = new RelayAction((param) => OpenLogFile(), (param) => true);
		}

		private void GetLogFiles()
		{
			var files = logFolder.EnumerateFiles();

			var filesToAdd = files.Except(LogFiles);

			filesToAdd.ToList().ForEach((item) => LogFiles.Add(item));
		}

		private void OpenLogFile()
		{
		}
	}
}
