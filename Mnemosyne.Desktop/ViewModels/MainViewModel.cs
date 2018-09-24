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
					CMDAddProfile.Notify();
					CMDViewProfile.Notify();
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
					CMDAddProfile.Notify();
					CMDViewProfile.Notify();
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
						CMDViewProfile.Notify();
						CMDAddProfile.Notify();
						CMDStart.Notify();
						CMDCancel.Notify();
						CMDViewLog.Notify();
					}));
				}
			}
		}

		public ActivityViewModel Activity
		{
			get => activity;
			set
			{
				if (value != activity)
				{
					activity = value;
					Notify(nameof(Activity));
				}
			}
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

		public ObservableCollection<ProfileViewModel> Profiles { get; }

		public ProfileViewModel CurrentProfil
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
						CMDViewProfile.Notify();
					}));
				}
			}
		}

		public RelayAction CMDSelectSource { get; }

		public RelayAction CMDSelectTarget { get; }

		public RelayAction CMDViewProfile { get; }

		public RelayAction CMDAddProfile { get; }

		public RelayAction CMDStart { get; }

		public RelayAction CMDCancel { get; }

		public RelayAction CMDViewLog { get; }

		private string sourcePath;
		private string targetPath;
		private bool isRunning;
		private long itemPosition;
		private CancellationTokenSource cancellationTokenSource;
		private DateTime startTime;
		private DateTime endTime;
		private TimeSpan elapsedTime;
		private double speed;
		private TimeSpan remainingTime;
		private ProfileViewModel currentProfil;
		private ActivityViewModel activity;

		public MainViewModel()
		{
			Profiles = new ObservableCollection<ProfileViewModel>();

			CMDSelectSource = new RelayAction((param) => SelectSource(), (param) => !IsRunning);
			CMDSelectTarget = new RelayAction((param) => SelectTarget(), (param) => !IsRunning && SourcePath != null);
			CMDViewProfile = new RelayAction((param) => { OpenProfilView(); }, (param) => !IsRunning && CurrentProfil != null && SourcePath != null && TargetPath != null && SourcePath != TargetPath);
			CMDAddProfile = new RelayAction((param) => { OpenAddingProfileView(); }, (param) => !IsRunning && SourcePath != null && TargetPath != null && SourcePath != TargetPath);
			CMDStart = new RelayAction(async (param) => await Start(), (param) => !IsRunning && SourcePath != null && TargetPath != null && SourcePath != TargetPath);
			CMDCancel = new RelayAction((param) => { Cancel(); }, (param) => IsRunning);
			CMDViewLog = new RelayAction((param) => { OpenLogView(); }, (param) => { return !IsRunning; });
		}

		private void SelectSource()
		{
			using (var window = new FolderBrowserDialog())
			{
				var input = window.ShowDialog();

				if (input.HasFlag(DialogResult.OK))
				{
					SourcePath = window.SelectedPath;

					var message = string.Format("New source selected: \"{0}\".", SourcePath);
					Trace.TraceInformation(message);
				}
			}
		}

		private void SelectTarget()
		{
			using (var window = new FolderBrowserDialog())
			{
				var input = window.ShowDialog();

				if (input.HasFlag(DialogResult.OK))
				{
					TargetPath = window.SelectedPath;

					var message = string.Format("New target selected: \"{0}\".", TargetPath);
					Trace.TraceInformation(message);
				}
			}
		}

		public void GetProfils()
		{
			var serializer = new XmlSerializer(typeof(ProfileViewModel));

			var profilesFodler = new DirectoryInfo(Path.Combine(Application.LocalUserAppDataPath, "Profiles"));

			if (!profilesFodler.Exists)
				profilesFodler.Create();

			var defaultProfile = from profile in Profiles
								 where profile.Name == "default"
								 select profile;

			if (defaultProfile.Count() < 1)
				Profiles.Add(ProfileViewModel.CreateDefault(false));

			var profilefiles = profilesFodler.EnumerateFiles();

			foreach (var profileFile in profilefiles)
			{
				var profiles = from profile in Profiles
							   where profile.FileInfo != null
							   where profile.FileInfo.FullName == profileFile.FullName
							   select profile;

				if (profiles.Count() < 1)
				{
					using (var stream = profileFile.Open(FileMode.Open, FileAccess.Read, FileShare.None))
					{
						var profil = (ProfileViewModel)serializer.Deserialize(stream);
						profil.FileInfo = profileFile;
						Profiles.Add(profil);
					}
				}
			}

			List<ProfileViewModel> toDelete = new List<ProfileViewModel>();

			foreach (var profile in Profiles)
			{
				if (profile.FileInfo != null &&
					!File.Exists(profile.FileInfo.FullName))
				{
					toDelete.Add(profile);
				}
			}

			foreach (var profile in toDelete)
			{
				if (profile == CurrentProfil)
					CurrentProfil = null;
				Profiles.Remove(profile);
			}

			if (CurrentProfil == null)
				CurrentProfil = defaultProfile.First();
		}

		private void OpenProfilView()
		{
			var view = new ProfileVisualizationView(SourcePath, CurrentProfil);
			view.Closed += (sender, e) => { GetProfils(); };
			view.ShowDialog();
		}

		private void OpenAddingProfileView()
		{
			var view = new AddingView(SourcePath);
			view.ShowDialog();
		}

		private void OpenLogView()
		{
			var view = new LogView();
			view.ShowDialog();
		}

		private bool FileIsCopyable(FileInfo file)
		{
			var relative = file.FullName.Substring(SourcePath.Length + 1);
			return !CurrentProfil.FilesExcluded.Contains(relative);
		}

		private bool FolderIsCopyable(DirectoryInfo folder)
		{
			var relative = folder.FullName.Substring(SourcePath.Length + 1);
			return !CurrentProfil.DirectoriesExcluded.Contains(relative);
		}

		private void Count(DirectoryInfo sourceParentFolder, DirectoryInfo targetParentFolder)
		{
			if (sourceParentFolder.Attributes.HasFlag(FileAttributes.ReparsePoint))
				return;

			var sourceChildFiles = sourceParentFolder.EnumerateFiles();
			var sourceChildDirectories = sourceParentFolder.EnumerateDirectories();

			var targetChildFiles = targetParentFolder?.EnumerateFiles() ?? new List<FileInfo>();
			var targetChildDirectories = targetParentFolder?.EnumerateDirectories() ?? new List<DirectoryInfo>();

			var filesToDelete = targetChildFiles.Except(sourceChildFiles, new FileComparer()).Where(FileIsCopyable);
			var filesToUpdate = sourceChildFiles.Intersect(targetChildFiles, new FileComparer()).Where(FileIsCopyable);
			var filesToCopy = sourceChildFiles.Except(targetChildFiles, new FileComparer()).Where(FileIsCopyable);

			var directoriesToDelete = targetChildDirectories.Except(sourceChildDirectories, new DirectoryComparer()).Where(FolderIsCopyable);
			var directoriesToUpdate = sourceChildDirectories.Intersect(targetChildDirectories, new DirectoryComparer()).Where(FolderIsCopyable);
			var directoriesToCopy = sourceChildDirectories.Except(targetChildDirectories, new DirectoryComparer()).Where(FolderIsCopyable);

			Activity.FileCount += filesToDelete.Count();
			Activity.FolderCount += directoriesToDelete.Count();

			foreach (var fileToUpdate in filesToUpdate)
			{
				Activity.FileCount++;
				Activity.ByteCount += fileToUpdate.Length;
			}

			foreach (var fileToCopy in filesToCopy)
			{
				Activity.FileCount++;
				Activity.ByteCount += fileToCopy.Length;
			}

			foreach (var directoryToUpdate in directoriesToUpdate)
			{
				var relative = directoryToUpdate.FullName.Substring(SourcePath.Length + 1);
				var absolute = Path.Combine(TargetPath, relative);
				var targetDirectory = new DirectoryInfo(absolute);

				Activity.FolderCount++;

				Count(directoryToUpdate, targetDirectory);
			}

			foreach (var directoryToCopy in directoriesToCopy)
			{
				Activity.FolderCount++;

				Count(directoryToCopy, null);
			}
		}

		private void DeleteFile(FileInfo file)
		{
			try
			{
				WriteMessage(string.Format("File is deleting to \"{0}\".", file.FullName), MessageType.Information);

				if (file.IsReadOnly)
					file.Attributes &= ~FileAttributes.ReadOnly;

				file.Delete();

				WriteMessage(string.Format("File is deleted with successful to \"{0}\".", file.FullName), MessageType.Information);
			}
			catch (Exception e)
			{
				WriteMessage(string.Format("File delete is failed to \"{0}\", because \"{1}\".", file.FullName, e.Message), MessageType.Error);
			}
		}

		private void DeleteDirectory(DirectoryInfo directory, CancellationToken cancellationToken)
		{
			try
			{
				if (!directory.Exists)
					return;

				WriteMessage(string.Format("Folder is deleting to \"{0}\".", directory.FullName), MessageType.Information);

				var files = directory.EnumerateFiles();
				var directories = directory.EnumerateDirectories();

				foreach (var file in files)
				{
					cancellationToken.ThrowIfCancellationRequested();

					DeleteFile(file);
				}

				foreach (var dir in directories)
				{
					cancellationToken.ThrowIfCancellationRequested();

					DeleteDirectory(dir, cancellationToken);
				}

				if (directory.Attributes.HasFlag(FileAttributes.ReadOnly))
					directory.Attributes &= ~FileAttributes.ReadOnly;

				directory.Delete();

				WriteMessage(string.Format("Folder is deleted with successful to \"{0}\".", directory.FullName), MessageType.Information);
			}
			catch (OperationCanceledException)
			{
				throw new OperationCanceledException(cancellationToken);
			}
			catch (Exception e)
			{
				WriteMessage(string.Format("Folder delete is failed to \"{0}\", because \"{1}\".", directory.FullName, e.Message), MessageType.Error);
			}
		}

		private void CopyData(FileInfo sourceFile, FileInfo targetFile, CancellationToken cancellationToken)
		{
			try
			{
				WriteMessage(string.Format("Data are copying to \"{0}\".", targetFile.FullName), MessageType.Information);

				using (FileStream sourceStream = sourceFile.Open(FileMode.Open, FileAccess.Read, FileShare.None),
					targetStream = targetFile.Open(FileMode.CreateNew, FileAccess.Write, FileShare.None))
				{
					var buffer = new byte[CurrentProfil.BufferLength];

					for (int i = 0; i < sourceStream.Length; i += CurrentProfil.BufferLength)
					{
						cancellationToken.ThrowIfCancellationRequested();

						var remainingBytesCount = sourceStream.Length - i;
						var count = (int)(remainingBytesCount < CurrentProfil.BufferLength ? remainingBytesCount : CurrentProfil.BufferLength);

						sourceStream.Read(buffer, 0, count);
						targetStream.Write(buffer, 0, count);

						Activity.CopiedByte += count;
					}
				}

				WriteMessage(string.Format("Data are copied with successful to \"{0}\".", targetFile.FullName), MessageType.Information);
			}
			catch (OperationCanceledException)
			{
				targetFile.Delete();

				WriteMessage(string.Format("Data copy is canceled. The current file is deleted to \"{0}\".", targetFile.FullName), MessageType.Warning);

				throw new OperationCanceledException(cancellationToken);
			}
			catch (Exception e)
			{
				WriteMessage(string.Format("Data copy is failed to \"{0}\", because \"{1}\".", targetFile.FullName, e.Message), MessageType.Error);
			}
		}

		private void UpdateData(FileInfo sourceFile, FileInfo targetFile, CancellationToken cancellationToken)
		{
			try
			{
				WriteMessage(string.Format("Data are updating to \"{0}\".", targetFile.FullName), MessageType.Information);

				using (FileStream sourceStream = sourceFile.Open(FileMode.Open, FileAccess.Read, FileShare.None),
					targetStream = targetFile.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None))
				{
					var sourceBuffer = new byte[CurrentProfil.BufferLength];
					var targetBuffer = new byte[CurrentProfil.BufferLength];

					targetStream.SetLength(sourceStream.Length);

					for (int i = 0; i < sourceStream.Length; i += CurrentProfil.BufferLength)
					{
						cancellationToken.ThrowIfCancellationRequested();

						var remainingBytesCount = sourceStream.Length - i;
						var count = (int)(remainingBytesCount < CurrentProfil.BufferLength ? remainingBytesCount : CurrentProfil.BufferLength);

						sourceStream.Read(sourceBuffer, 0, count);
						targetStream.Read(targetBuffer, 0, count);

						if (!sourceBuffer.SequenceEqual(targetBuffer))
						{
							targetStream.Position = i;
							targetStream.Write(sourceBuffer, 0, count);
						}

						Activity.CopiedByte += count;
					}
				}
				
				WriteMessage(string.Format("Data are updated with successful to \"{0}\".", targetFile.FullName), MessageType.Information);
			}
			catch (OperationCanceledException)
			{
				DeleteFile(targetFile);

				WriteMessage(string.Format("Data update is canceled. The current file is deleted to \"{0}\".", targetFile.FullName), MessageType.Warning);

				throw new OperationCanceledException(cancellationToken);
			}
			catch (Exception e)
			{
				WriteMessage(string.Format("Data copy is failed to \"{0}\", because \"{1}\".", targetFile.FullName, e.Message), MessageType.Error);
			}
		}

		private async Task CopyMetadata(FileInfo sourceFile, FileInfo targetFile, int delay, int lap, CancellationToken cancellationToken)
		{
			var currentLap = 0;

			while (true)
			{
				try
				{
					if (targetFile.Exists)
					{
						WriteMessage(string.Format("Metadata are updating to \"{0}\".", targetFile.FullName), MessageType.Information);

						if (CurrentProfil.CreationTime)
							targetFile.CreationTimeUtc = sourceFile.CreationTimeUtc;

						if (CurrentProfil.LastAccessTime)
							targetFile.LastAccessTimeUtc = sourceFile.LastAccessTimeUtc;

						if (CurrentProfil.LastWriteTime)
							targetFile.LastWriteTime = sourceFile.LastWriteTimeUtc;

						if (CurrentProfil.Attributes)
							targetFile.Attributes = sourceFile.Attributes;

						if (CurrentProfil.AccessControl)
							targetFile.SetAccessControl(sourceFile.GetAccessControl());

						WriteMessage(string.Format("Metadata are updated with successful to \"{0}\".", targetFile.FullName), MessageType.Information);
					}

					break;
				}
				catch (IOException e)
				{
					WriteMessage(string.Format("Metadata update is failed to \"{0}\", yet {1} trying, because \"{2}\".", targetFile.FullName, currentLap - lap, e.Message), MessageType.Warning);

					if (++currentLap > lap)
						break;
					else
						WriteMessage(string.Format("Data copy is failed to \"{0}\", because \"{1}\".", targetFile.FullName, e.Message), MessageType.Error);

					if (delay > 0)
						await Task.Delay(delay, cancellationToken);
				}
				catch (Exception e)
				{
					WriteMessage(string.Format("Data copy is failed to \"{0}\", because \"{1}\".", targetFile.FullName, e.Message), MessageType.Error);
				}
			}
		}

		private async Task Dispatch(DirectoryInfo sourceParentDirectory, DirectoryInfo targetParentDirectory, CancellationToken cancellationToken)
		{
			if (sourceParentDirectory.Attributes.HasFlag(FileAttributes.ReparsePoint))
				return;

			var sourceChildFiles = sourceParentDirectory.EnumerateFiles();
			var sourceChildDirectories= sourceParentDirectory.EnumerateDirectories();
			var targetChildFiles = targetParentDirectory.EnumerateFiles();
			var targetChildDirectories = targetParentDirectory.EnumerateDirectories();
			
			var filesToDelete = targetChildFiles.Except(sourceChildFiles, new FileComparer()).Where(FileIsCopyable);
			var filesToUpdate = sourceChildFiles.Intersect(targetChildFiles, new FileComparer()).Where(FileIsCopyable);
			var filesToCopy = sourceChildFiles.Except(targetChildFiles, new FileComparer()).Where(FileIsCopyable);

			var directoriesToDelete = targetChildDirectories.Except(sourceChildDirectories, new DirectoryComparer()).Where(FolderIsCopyable);
			var directoriesToUpdate = sourceChildDirectories.Intersect(targetChildDirectories, new DirectoryComparer()).Where(FolderIsCopyable);
			var directoriesToCopy = sourceChildDirectories.Except(targetChildDirectories, new DirectoryComparer()).Where(FolderIsCopyable);

			foreach (var fileToDelete in filesToDelete)
			{
				DeleteFile(fileToDelete);

				ItemPosition++;
			}

			foreach (var fileToUpdate in filesToUpdate)
			{
				var relative = fileToUpdate.FullName.Substring(SourcePath.Length + 1);
				var absolute = Path.Combine(TargetPath, relative);
				var targetChildFile = new FileInfo(absolute);

				try
				{
					UpdateData(fileToUpdate, targetChildFile, cancellationToken);
					await CopyMetadata(fileToUpdate, targetChildFile, 500, 10, cancellationToken);
				}
				catch (OperationCanceledException)
				{
					return;
				}

				ItemPosition++;
			}

			foreach (var fileToCopy in filesToCopy)
			{
				var relative = fileToCopy.FullName.Substring(SourcePath.Length + 1);
				var absolute = Path.Combine(TargetPath, relative);
				var targetChildFile = new FileInfo(absolute);

				try
				{
					CopyData(fileToCopy, targetChildFile, cancellationToken);
					await CopyMetadata(fileToCopy, targetChildFile, 500, 10, cancellationToken);
				}
				catch (OperationCanceledException)
				{
					return;
				}

				ItemPosition++;
			}

			foreach (var directoryToDelete in directoriesToDelete)
			{
				DeleteDirectory(directoryToDelete, cancellationToken);

				ItemPosition++;
			}

			foreach (var directoryToUpdate in directoriesToUpdate)
			{
				var relative = directoryToUpdate.FullName.Substring(SourcePath.Length + 1);
				var absolute = Path.Combine(TargetPath, relative);
				var targetChildDirectory = new DirectoryInfo(absolute);

				ItemPosition++;

				await Dispatch(directoryToUpdate, targetChildDirectory, cancellationToken);
			}

			foreach (var directoryToCopy in directoriesToCopy)
			{
				var relative = directoryToCopy.FullName.Substring(SourcePath.Length + 1);
				var absolute = Path.Combine(TargetPath, relative);
				var targetChildDirectory = new DirectoryInfo(absolute);

				targetChildDirectory.Create();

				ItemPosition++;

				await Dispatch(directoryToCopy, targetChildDirectory, cancellationToken);
			}
		}

		private void ComputeTime(object state)
		{
			ElapsedTime = DateTime.Now - StartTime;

			if (ElapsedTime.TotalSeconds > 0)
				Speed = Activity.CopiedByte / ElapsedTime.TotalSeconds;

			if (Speed > 0)
				RemainingTime = TimeSpan.FromSeconds((Activity.ByteCount - Activity.CopiedByte) / Speed);
		}

		private async Task Start()

		{
			async Task main()
			{
				Activity = new ActivityViewModel();
				Activity.FileCount = Activity.FolderCount = ItemPosition = 0;
				Activity.CopiedByte = Activity.ByteCount = 0;

				var sourceDirectory = new DirectoryInfo(SourcePath);
				var targetDirectory = new DirectoryInfo(TargetPath);

				var targetDrive = new DriveInfo(Path.GetPathRoot(TargetPath));

				cancellationTokenSource = new CancellationTokenSource();

				IsRunning = true;

				WriteMessage(string.Format("Syncrhoniszation started from \"{0}\" to \"{1}\".", SourcePath, TargetPath), MessageType.Information);
				
				WriteMessage(string.Format("Counting the files and folders."), MessageType.Information);

				try
				{
					Count(sourceDirectory, targetDirectory);
				}
				catch(Exception e)
				{
					WriteMessage(string.Format("Syncrhoniszation failed from \"{0}\" to \"{1}\" because \"{2}\".", SourcePath, TargetPath, e.Message), MessageType.Error);

					return;
				}

				WriteMessage(string.Format("{0} file(s) and {1} folder(s) counted.", Activity.FileCount, Activity.FolderCount), MessageType.Information);

				if (Activity.ByteCount > targetDrive.AvailableFreeSpace)
				{
					WriteMessage(string.Format("Syncrhoniszation failed from \"{0}\" to \"{1}\" because the available space to the target is less than the source size.", SourcePath, TargetPath), MessageType.Warning);

					return;
				}

				WriteMessage(string.Format("Analysing the files and folders."), MessageType.Information);

				StartTime = DateTime.Now;

				var timer = new System.Threading.Timer(ComputeTime, null, 0, 100);

				await Dispatch(sourceDirectory, targetDirectory, cancellationTokenSource.Token);

				timer.Dispose();

				EndTime = DateTime.Now;
				
				WriteMessage(string.Format("Files and folders analysed."), MessageType.Information);

				var endMessage = string.Empty;

				if (cancellationTokenSource.IsCancellationRequested)
					endMessage = string.Format("Syncrhoniszation cancelled from \"{0}\" to \"{1}\".", SourcePath, TargetPath);
				else
					endMessage = string.Format("Syncrhoniszation finished from \"{0}\" to \"{1}\".", SourcePath, TargetPath); ;

				cancellationTokenSource.Dispose();

				IsRunning = false;

				WriteMessage(endMessage, MessageType.Information);
			}

			await Task.Run(main);
		}

		private void Cancel()
		{
			cancellationTokenSource.Cancel();
		}
	}
}
