using Mnemosyne.Desktop.Helpers;
using Mnemosyne.Desktop.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using System.Xml.Serialization;

namespace Mnemosyne.Desktop.ViewModels
{
	public sealed class MainViewModel : CommonViewModel
	{
		public string SourcePath
		{
			get => sourcePath;
			set
			{
				if (value != sourcePath)
				{
					sourcePath = value;
					Notify(nameof(SourcePath));
					CMDStart.Notify();
					CMDSelectTarget.Notify();
				}
			}
		}

		public string TargetPath
		{
			get => targetPath;
			set
			{
				if (value != targetPath)
				{
					targetPath = value;
					Notify(nameof(TargetPath));
					CMDStart.Notify();
				}
			}
		}

		public long FileCount
		{
			get => fileCount;
			set
			{
				if (value != fileCount)
				{
					fileCount = value;
					Notify(nameof(FileCount));
					Notify(nameof(ItemCount));
				}
			}
		}

		public long FolderCount
		{
			get => folderCount;
			set
			{
				if (value != folderCount)
				{
					folderCount = value;
					Notify(nameof(FolderCount));
					Notify(nameof(ItemCount));
				}
			}
		}

		public long ItemCount
		{
			get => FileCount + FolderCount;
		}

		public long ItemPosition
		{
			get => itemPosition;
			set
			{
				if (value != itemPosition)
				{
					itemPosition = value;
					Notify(nameof(ItemPosition));
				}
			}
		}

		public string Output
		{
			get => output;
			set
			{
				if (value != output)
				{
					output = value;
					Notify(nameof(Output));
				}
			}
		}

		public bool IsRunning
		{
			get => isRunning;
			set
			{
				if (value != isRunning)
				{
					isRunning = value;
					Notify(nameof(IsRunning));
					System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
					{
						CMDSelectSource.Notify();
						CMDSelectTarget.Notify();
						CMDViewProfil.Notify();
						CMDAdd.Notify();
						CMDStart.Notify();
						CMDCancel.Notify();
					}));
				}
			}
		}

		public DateTime StartTime
		{
			get => startTime;
			set
			{
				if (value != startTime)
				{
					startTime = value;
					Notify(nameof(StartTime));
				}
			}
		}

		public TimeSpan ElapsedTime
		{
			get => elapsedTime;
			set
			{
				if (value != elapsedTime)
				{
					elapsedTime = value;
					Notify(nameof(ElapsedTime));
				}
			}
		}

		public DateTime EndTime
		{
			get => endTime;
			set
			{
				if (value != endTime)
				{
					endTime = value;
					Notify(nameof(EndTime));
				}
			}
		}

		public DateTime EstimateEndTime
		{
			get => estimateEndTime;
			set
			{
				if (value != estimateEndTime)
				{
					estimateEndTime = value;
					Notify(nameof(EstimateEndTime));
				}
			}
		}

		public double Speed
		{
			get => speed;
			set
			{
				if (value != speed)
				{
					speed = value;
					Notify(nameof(Speed));
				}
			}
		}

		public TimeSpan RemainingTime
		{
			get => remainingTime;
			set
			{
				if (value != remainingTime)
				{
					remainingTime = value;
					Notify(nameof(RemainingTime));
				}
			}
		}

		public ObservableCollection<Profile> Profils { get; }
		public ObservableCollection<string> ProfilsPaths { get; }

		public Profile CurrentProfil
		{
			get
			{
				return currentProfil;
			}
			set
			{
				if (value != currentProfil)
				{
					currentProfil = value;
					Notify(nameof(CurrentProfil));
					System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
					{
						CMDViewProfil.Notify();
					}));
				}
			}
		}

		public RelayAction CMDSelectSource { get; }
		public RelayAction CMDSelectTarget { get; }
		public RelayAction CMDViewProfil { get; }
		public RelayAction CMDAdd { get; }
		public RelayAction CMDStart { get; }
		public RelayAction CMDCancel { get; }

		private static readonly object readLock = new object();
		private static readonly object writeLock = new object();
		private string sourcePath;
		private string targetPath;
		private long fileCount;
		private long folderCount;
		private long itemPosition;
		private string output;
		private bool isRunning;
		private CancellationTokenSource cancellationTokenSource;
		private System.Timers.Timer timer;
		private DateTime startTime;
		private TimeSpan elapsedTime;
		private DateTime endTime;
		private double speed;
		private TimeSpan remainingTime;
		private long copiedByte;
		private long byteToCopy;
		private DateTime estimateEndTime;
		private Profile currentProfil;

		public MainViewModel()
		{
			Profils = new ObservableCollection<Profile>();
			ProfilsPaths = new ObservableCollection<string>();

			CMDSelectSource = new RelayAction((param) => SelectSource(), (param) => !IsRunning);
			CMDSelectTarget = new RelayAction((param) => SelectTarget(), (param) => !IsRunning && SourcePath != null);
			CMDViewProfil = new RelayAction((param) => { OpenProfilWindow(); }, (param) => !IsRunning && CurrentProfil != null);
			CMDAdd = new RelayAction((param) => { OpenAddingProfileWindow(); }, (param) => !IsRunning);
			CMDStart = new RelayAction(async (param) => { await Start(); }, (param) => !IsRunning && SourcePath != null && TargetPath != null);
			CMDCancel = new RelayAction((param) => { Cancel(); }, (param) => IsRunning);

			timer = new System.Timers.Timer(100);		
			timer.Elapsed += (sender, e) => { ElapsedTime = e.SignalTime - StartTime; Speed = copiedByte / ElapsedTime.TotalSeconds; RemainingTime = TimeSpan.FromSeconds((byteToCopy - copiedByte) / Speed); EstimateEndTime = e.SignalTime + ElapsedTime; };
		}

		private void SelectSource()
		{
			using (var window = new FolderBrowserDialog())
			{
				var input = window.ShowDialog();

				if (input.HasFlag(DialogResult.OK))
					SourcePath = window.SelectedPath;
			}
		}

		private void SelectTarget()
		{
			using (var window = new FolderBrowserDialog())
			{
				var input = window.ShowDialog();

				if (input.HasFlag(DialogResult.OK))
					TargetPath = window.SelectedPath;
			}
		}

		private async Task Count(DirectoryInfo parentDirectory)
		{
			var childFiles = parentDirectory.EnumerateFiles();
			var childFolders = parentDirectory.EnumerateDirectories();

			FileCount += childFiles.Count();
			FolderCount += childFolders.Count();

			foreach (var childFile in childFiles)
				byteToCopy += childFile.Length;

			foreach (var childFolder in childFolders)
				await Count(childFolder);
		}

		public void GetProfils()
		{
			var serializer = new XmlSerializer(typeof(Profile));

			var directory = new DirectoryInfo(Path.Combine(Application.LocalUserAppDataPath, "Profiles"));

			if (!directory.Exists)
				directory.Create();

			var defaultProfil = from profil in Profils
								where profil.Name == "default"
								select profil;

			if (defaultProfil.Count() < 1)
			{
				Profils.Add(Profile.CreateDefault(false));
			}

			var files = directory.EnumerateFiles();

			foreach (var file in files)
			{
				var profils = from p in Profils
							  where p.FileInfo != null
							  where p.FileInfo.FullName == file.FullName
							  select p;

				if (profils.Count() < 1)
				{
					using (var stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
					{
						var profil = (Profile)serializer.Deserialize(stream);
						profil.FileInfo = file;
						Profils.Add(profil);
					}
				}
			}

			List<Profile> toDelete = new List<Profile>();

			foreach (var profil in Profils)
			{
				if (profil.FileInfo != null &&
					!File.Exists(profil.FileInfo.FullName))
				{
					toDelete.Add(profil);
				}
			}

			foreach (var profil in toDelete)
			{
				if (profil == CurrentProfil)
					CurrentProfil = null;
				Profils.Remove(profil);
			}

			if (CurrentProfil == null)
			{
				CurrentProfil = defaultProfil.First();
			}
		}

		private void OpenProfilWindow()
		{
			var win = new VisualizationView(CurrentProfil);
			win.Closed += (sender, e) => { GetProfils(); };
			win.ShowDialog();
		}

		private void OpenAddingProfileWindow()
		{
			var view = new AddingView();
			view.ShowDialog();
		}

		private void CopyFile(FileStream sourceFileStream, FileStream targetFileStream, CancellationToken cancellationToken)
		{
			try
			{
				for (int i = 0; i < sourceFileStream.Length; i += CurrentProfil.BufferLength)
				{
					cancellationToken.ThrowIfCancellationRequested();

					var buffer = new byte[sourceFileStream.Length];
					var count = (int)(sourceFileStream.Length - i > CurrentProfil.BufferLength ? CurrentProfil.BufferLength : sourceFileStream.Length - i);

					sourceFileStream.Read(buffer, 0, count);
					targetFileStream.Write(buffer, 0, count);

					copiedByte += count;
				}
			}
			catch (OperationCanceledException)
			{
				throw  new OperationCanceledException(cancellationToken);
			}
			finally
			{
				targetFileStream.Flush();

				sourceFileStream.Dispose();
				targetFileStream.Dispose();
			}
		}

		private void UpdateFile(FileStream sourceFileStream, FileStream targetFileStream, CancellationToken cancellationToken)
		{
			try
			{
				for (int i = 0; i < sourceFileStream.Length; i += CurrentProfil.BufferLength)
				{
					cancellationToken.ThrowIfCancellationRequested();

					var sourceBuffer = new byte[CurrentProfil.BufferLength];
					var targetBuffer = new byte[CurrentProfil.BufferLength];
					var count = (int)(sourceFileStream.Length - i > CurrentProfil.BufferLength ? CurrentProfil.BufferLength : sourceFileStream.Length - i);

					sourceFileStream.Seek(i, SeekOrigin.Begin);
					targetFileStream.Seek(i, SeekOrigin.Begin);

					sourceFileStream.Read(sourceBuffer, 0, count);
					targetFileStream.Read(targetBuffer, 0, count);

					if (!sourceBuffer.SequenceEqual(targetBuffer))
					{
						targetFileStream.Seek(i, SeekOrigin.Begin);
						targetFileStream.Write(sourceBuffer, 0, count);
					}

					copiedByte += count;
				}
			}
			catch (OperationCanceledException)
			{
				throw new OperationCanceledException(cancellationToken);
			}
			finally
			{
				targetFileStream.Flush();

				sourceFileStream.Dispose();
				targetFileStream.Dispose();
			}
		}

		private async Task Dispatch(DirectoryInfo parentDirectory, CancellationToken cancellationToken)
		{
			var childItems = parentDirectory.EnumerateFileSystemInfos();

			foreach (var sourceChildItem in childItems)
			{
				var relative = sourceChildItem.FullName.Substring(SourcePath.Length + 1);
				var absolute = Path.Combine(TargetPath, relative);

				ItemPosition++;

				if (sourceChildItem is FileInfo sourceChildFile)
				{
					var targetChildFile = new FileInfo(absolute);

					if (targetChildFile.Exists)
					{
						if (sourceChildFile.Length == targetChildFile.Length)
						{
							Output = "UPDATE " + sourceChildItem.Name;

							try
							{
								UpdateFile(sourceChildFile.OpenRead(), targetChildFile.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite), cancellationToken);
							}
							catch (OperationCanceledException)
							{
								targetChildFile.Delete();
								return;
							}
						}
						else
						{
							Output = "CHANGE " + sourceChildItem.Name;

							try
							{
								var targetChildFileStream = targetChildFile.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);

								targetChildFileStream.SetLength(sourceChildFile.Length);
								targetChildFileStream.Flush();

								UpdateFile(sourceChildFile.OpenRead(), targetChildFileStream, cancellationToken);
							}
							catch (OperationCanceledException)
							{
								targetChildFile.Delete();
								return;
							}
						}
					}
					else
					{
						Output = "CREATE " + sourceChildItem.Name;

						try
						{
							CopyFile(sourceChildFile.OpenRead(), targetChildFile.Open(FileMode.Create, FileAccess.Write, FileShare.Write), cancellationToken);
						}
						catch (OperationCanceledException)
						{
							targetChildFile.Delete();
							return;
						}
					}

					if (CurrentProfil.CreationTime)
						targetChildFile.CreationTime = sourceChildFile.CreationTime;

					if (CurrentProfil.LastAccessTime)
						targetChildFile.LastAccessTime = sourceChildFile.LastAccessTime;

					if (CurrentProfil.LastWriteTime)
						targetChildFile.LastWriteTime = sourceChildFile.LastWriteTime;

					if(CurrentProfil.Attributes)
						targetChildFile.Attributes = sourceChildFile.Attributes;

					if (CurrentProfil.AccessControl)
					{
						var fileSecurity = sourceChildFile.GetAccessControl();
						targetChildFile.SetAccessControl(fileSecurity);
					}
				}
				else if (sourceChildItem is DirectoryInfo sourceChildDirectory)
				{
					DirectoryInfo targetChildDirectory = null;

					if (!Directory.Exists(absolute))
						targetChildDirectory = Directory.CreateDirectory(absolute);
					else
						targetChildDirectory = new DirectoryInfo(absolute);

					if (CurrentProfil.CreationTime)
						targetChildDirectory.CreationTime = sourceChildDirectory.CreationTime;

					if (CurrentProfil.LastAccessTime)
						targetChildDirectory.LastAccessTime = sourceChildDirectory.LastAccessTime;

					if (CurrentProfil.LastWriteTime)
						targetChildDirectory.LastWriteTime = sourceChildDirectory.LastWriteTime;

					if (CurrentProfil.Attributes)
						targetChildDirectory.Attributes = sourceChildDirectory.Attributes;

					if (CurrentProfil.AccessControl)
					{
						var fileSecurity = sourceChildDirectory.GetAccessControl();
						targetChildDirectory.SetAccessControl(fileSecurity);
					}

					await Dispatch(sourceChildDirectory, cancellationToken);
				}
			}
		}

		private async Task Start()
		{
			async void main()
			{
				Debug.WriteLine("START");

				FileCount = FolderCount = ItemPosition = 0;
				copiedByte = byteToCopy = 0;

				var sourceDirectory = new DirectoryInfo(SourcePath);

				cancellationTokenSource = new CancellationTokenSource();

				IsRunning = true;

				Output = "Count";

				await Count(sourceDirectory);

				StartTime = DateTime.Now;

				timer.Start();

				await Dispatch(sourceDirectory, cancellationTokenSource.Token);

				timer.Stop();

				EndTime = DateTime.Now;

				if (cancellationTokenSource.IsCancellationRequested)
					Output = "Stoped";

				cancellationTokenSource.Dispose();

				IsRunning = false;

				Output = "End";
			}

			await Task.Run(main);
		}

		private void Cancel()
		{
			cancellationTokenSource.Cancel();
			Debug.WriteLine("Stop");
		}
	}
}
