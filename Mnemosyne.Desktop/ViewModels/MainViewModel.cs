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
						CMDViewProfile.Notify();
						CMDAddProfile.Notify();
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

		public ObservableCollection<ProfileViewModel> Profils { get; }
		public ObservableCollection<string> ProfilsPaths { get; }

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
		private DateTime startTime;
		private TimeSpan elapsedTime;
		private DateTime endTime;
		private double speed;
		private TimeSpan remainingTime;
		private long copiedByte;
		private long byteToCopy;
		private ProfileViewModel currentProfil;

		public MainViewModel()
		{
			Profils = new ObservableCollection<ProfileViewModel>();
			ProfilsPaths = new ObservableCollection<string>();

			CMDSelectSource = new RelayAction((param) => SelectSource(), (param) => !IsRunning);
			CMDSelectTarget = new RelayAction((param) => SelectTarget(), (param) => !IsRunning && SourcePath != null);
			CMDViewProfile = new RelayAction((param) => { OpenProfilWindow(); }, (param) => !IsRunning && CurrentProfil != null && SourcePath != null && TargetPath != null && SourcePath != TargetPath);
			CMDAddProfile = new RelayAction((param) => { OpenAddingProfileWindow(); }, (param) => !IsRunning && SourcePath != null && TargetPath != null && SourcePath != TargetPath);
			CMDStart = new RelayAction(async (param) => { await Start(); }, (param) => !IsRunning && SourcePath != null && TargetPath != null && SourcePath != TargetPath);
			CMDCancel = new RelayAction((param) => { Cancel(); }, (param) => IsRunning);
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

		private bool A(FileInfo fileInfo)
		{
			var relative = fileInfo.FullName.Substring(SourcePath.Length + 1);
			return !CurrentProfil.FilesExcluded.Contains(relative);
		}

		private bool B(DirectoryInfo directoryInfo)
		{
			var relative = directoryInfo.FullName.Substring(SourcePath.Length + 1);
			return !CurrentProfil.DirectoriesExcluded.Contains(relative);
		}

		private async Task Count(DirectoryInfo sourceParentDirectory, DirectoryInfo targetParentDirectory)
		{
			var sourceChildFiles = sourceParentDirectory.EnumerateFiles();
			var sourceChildDirectories = sourceParentDirectory.EnumerateDirectories();

			var targetChildFiles = targetParentDirectory?.EnumerateFiles() ?? new List<FileInfo>();
			var targetChildDirectories = targetParentDirectory?.EnumerateDirectories() ?? new List<DirectoryInfo>();

			var filesToDelete = targetChildFiles.Except(sourceChildFiles, new FileComparer()).Where(A);
			var filesToUpdate = sourceChildFiles.Intersect(targetChildFiles, new FileComparer()).Where(A);
			var filesToCopy = sourceChildFiles.Except(targetChildFiles, new FileComparer()).Where(A);

			var directoriesToDelete = targetChildDirectories.Except(sourceChildDirectories, new DirectoryComparer()).Where(B);
			var directoriesToUpdate = sourceChildDirectories.Intersect(targetChildDirectories, new DirectoryComparer()).Where(B);
			var directoriesToCopy = sourceChildDirectories.Except(targetChildDirectories, new DirectoryComparer()).Where(B);

			FileCount += filesToDelete.Count();
			FolderCount += directoriesToDelete.Count();

			foreach (var fileToUpdate in filesToUpdate)
			{
				FileCount++;
				byteToCopy += fileToUpdate.Length;
			}

			foreach (var fileToCopy in filesToCopy)
			{
				FileCount++;
				byteToCopy += fileToCopy.Length;
			}

			foreach (var directoryToUpdate in directoriesToUpdate)
			{
				var relative = directoryToUpdate.FullName.Substring(SourcePath.Length + 1);
				var absolute = Path.Combine(TargetPath, relative);
				var targetDirectory = new DirectoryInfo(absolute);

				FolderCount++;

				await Count(directoryToUpdate, targetDirectory);
			}

			foreach (var directoryToCopy in directoriesToCopy)
			{
				FolderCount++;

				await Count(directoryToCopy, null);
			}
		}

		public void GetProfils()
		{
			var serializer = new XmlSerializer(typeof(ProfileViewModel));

			var directory = new DirectoryInfo(Path.Combine(Application.LocalUserAppDataPath, "Profiles"));

			if (!directory.Exists)
				directory.Create();

			var defaultProfil = from profil in Profils
								where profil.Name == "default"
								select profil;

			if (defaultProfil.Count() < 1)
				Profils.Add(ProfileViewModel.CreateDefault(false));

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
						var profil = (ProfileViewModel)serializer.Deserialize(stream);
						profil.FileInfo = file;
						Profils.Add(profil);
					}
				}
			}

			List<ProfileViewModel> toDelete = new List<ProfileViewModel>();

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
				CurrentProfil = defaultProfil.First();
		}

		private void OpenProfilWindow()
		{
			var win = new VisualizationView(SourcePath, CurrentProfil);
			win.Closed += (sender, e) => { GetProfils(); };
			win.ShowDialog();
		}

		private void OpenAddingProfileWindow()
		{
			var view = new AddingView(SourcePath);
			view.ShowDialog();
		}

		private void CopyData(FileStream sourceFileStream, FileStream targetFileStream, CancellationToken cancellationToken)
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

		private void UpdateData(FileStream sourceFileStream, FileStream targetFileStream, CancellationToken cancellationToken)
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

		private void UpdateMetadata(FileInfo sourceFile, FileInfo targetFile)
		{
			if (CurrentProfil.CreationTime)
				targetFile.CreationTime = sourceFile.CreationTime;

			if (CurrentProfil.LastAccessTime)
				targetFile.LastAccessTime = sourceFile.LastAccessTime;

			if (CurrentProfil.LastWriteTime)
				targetFile.LastWriteTime = sourceFile.LastWriteTime;

			if (CurrentProfil.Attributes)
				targetFile.Attributes = sourceFile.Attributes;

			if (CurrentProfil.AccessControl)
				targetFile.SetAccessControl(sourceFile.GetAccessControl());
		}

		private void UpdateMetadata(DirectoryInfo sourceDirectory, DirectoryInfo targetDirectory)
		{
			if (CurrentProfil.CreationTime)
				targetDirectory.CreationTime = sourceDirectory.CreationTime;

			if (CurrentProfil.LastAccessTime)
				targetDirectory.LastAccessTime = sourceDirectory.LastAccessTime;

			if (CurrentProfil.LastWriteTime)
				targetDirectory.LastWriteTime = sourceDirectory.LastWriteTime;

			if (CurrentProfil.Attributes)
				targetDirectory.Attributes = sourceDirectory.Attributes;

			if (CurrentProfil.AccessControl)
				targetDirectory.SetAccessControl(sourceDirectory.GetAccessControl());
		}

		private async Task Dispatch(DirectoryInfo sourceParentDirectory, DirectoryInfo targetParentDirectory, CancellationToken cancellationToken)
		{
			var sourceChildFiles = sourceParentDirectory.EnumerateFiles();
			var sourceChildDirectories= sourceParentDirectory.EnumerateDirectories();
			var targetChildFiles = targetParentDirectory.EnumerateFiles();
			var targetChildDirectories = targetParentDirectory.EnumerateDirectories();
			
			var filesToDelete = targetChildFiles.Except(sourceChildFiles, new FileComparer()).Where(A);
			var filesToUpdate = sourceChildFiles.Intersect(targetChildFiles, new FileComparer()).Where(A);
			var filesToCopy = sourceChildFiles.Except(targetChildFiles, new FileComparer()).Where(A);

			var directoriesToDelete = targetChildDirectories.Except(sourceChildDirectories, new DirectoryComparer()).Where(B);
			var directoriesToUpdate = sourceChildDirectories.Intersect(targetChildDirectories, new DirectoryComparer()).Where(B);
			var directoriesToCopy = sourceChildDirectories.Except(targetChildDirectories, new DirectoryComparer()).Where(B);

			foreach (var fileToDelete in filesToDelete)
			{
				fileToDelete.Delete();

				ItemPosition++;

				Output = "DELETE " + fileToDelete.Name;
			}

			foreach (var fileToUpdate in filesToUpdate)
			{
				var relative = fileToUpdate.FullName.Substring(SourcePath.Length + 1);
				var absolute = Path.Combine(TargetPath, relative);
				var targetChildFile = new FileInfo(absolute);

				try
				{
					var targetChildFileStream = targetChildFile.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);

					targetChildFileStream.SetLength(fileToUpdate.Length);
					targetChildFileStream.Flush();

					UpdateData(fileToUpdate.OpenRead(), targetChildFileStream, cancellationToken);
				}
				catch (OperationCanceledException)
				{
					targetChildFile.Delete();
					return;
				}

				UpdateMetadata(fileToUpdate, targetChildFile);

				ItemPosition++;

				Output = "UPDATE " + fileToUpdate.Name;
			}

			foreach (var fileToCopy in filesToCopy)
			{
				var relative = fileToCopy.FullName.Substring(SourcePath.Length + 1);
				var absolute = Path.Combine(TargetPath, relative);
				var targetChildFile = new FileInfo(absolute);

				try
				{
					CopyData(fileToCopy.OpenRead(), targetChildFile.Open(FileMode.Create, FileAccess.Write, FileShare.Write), cancellationToken);
				}
				catch (OperationCanceledException)
				{
					targetChildFile.Delete();
					return;
				}

				UpdateMetadata(fileToCopy, targetChildFile);

				ItemPosition++;

				Output = "COPY " + fileToCopy.Name;
			}

			foreach (var directoryToDelete in directoriesToDelete)
			{
				directoryToDelete.Delete(true);

				ItemPosition++;

				Output = "DELETE " + directoryToDelete.Name;
			}

			foreach (var directoryToUpdate in directoriesToUpdate)
			{
				var relative = directoryToUpdate.FullName.Substring(SourcePath.Length + 1);
				var absolute = Path.Combine(TargetPath, relative);
				var targetChildDirectory = new DirectoryInfo(absolute);

				UpdateMetadata(directoryToUpdate, targetChildDirectory);

				ItemPosition++;

				Output = "UPDATE " + directoryToUpdate.Name;

				await Dispatch(directoryToUpdate, targetChildDirectory, cancellationToken);
			}

			foreach (var directoryToCopy in directoriesToCopy)
			{
				var relative = directoryToCopy.FullName.Substring(SourcePath.Length + 1);
				var absolute = Path.Combine(TargetPath, relative);
				var targetChildDirectory = new DirectoryInfo(absolute);

				targetChildDirectory.Create();

				UpdateMetadata(directoryToCopy, targetChildDirectory);

				ItemPosition++;

				Output = "COPY " + directoryToCopy.Name;

				await Dispatch(directoryToCopy, targetChildDirectory, cancellationToken);
			}
		}

		private void ComputeTime(object state)
		{
			ElapsedTime = DateTime.Now - StartTime;

			if (ElapsedTime.TotalSeconds > 0)
				Speed = copiedByte / ElapsedTime.TotalSeconds;

			if (Speed > 0)
				RemainingTime = TimeSpan.FromSeconds((byteToCopy - copiedByte) / Speed);
		}

		private async Task Start()
		{
			async Task main()
			{
				FileCount = FolderCount = ItemPosition = 0;
				copiedByte = byteToCopy = 0;

				var sourceDirectory = new DirectoryInfo(SourcePath);
				var targetDirectory = new DirectoryInfo(TargetPath);

				cancellationTokenSource = new CancellationTokenSource();

				IsRunning = true;

				Output = "COUNT";

				await Count(sourceDirectory, targetDirectory);

				Debug.WriteLine(ItemCount);

				StartTime = DateTime.Now;

				var timer = new System.Threading.Timer(ComputeTime, null, 0, 100);

				await Dispatch(sourceDirectory, targetDirectory, cancellationTokenSource.Token);

				Debug.WriteLine(ItemPosition);

				timer.Dispose();

				EndTime = DateTime.Now;

				if (cancellationTokenSource.IsCancellationRequested)
					Output = "STOPPED";
				else
					Output = "FINISHED";

				cancellationTokenSource.Dispose();

				IsRunning = false;
			}

			await Task.Run(main);
		}

		private void Cancel()
		{
			cancellationTokenSource.Cancel();
		}
	}
}
