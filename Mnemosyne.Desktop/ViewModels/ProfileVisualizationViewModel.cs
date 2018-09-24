using Mnemosyne.Desktop.Helpers;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Mnemosyne.Desktop.ViewModels
{
	public class ProfileVisualizationViewModel : CommonViewModel
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
				}
			}
		}

		public ProfileViewModel Profile
		{
			get => profile;
			set
			{
				if (value != profile)
				{
					profile = value;
					Notify(nameof(Profile));
				}
			}
		}

		public string SelectedDirectory
		{
			get => selectedDirectory;
			set
			{
				if (value != selectedDirectory)
				{
					selectedDirectory = value;
					Notify(nameof(SelectedDirectory));
					CMDRemoveDirectory.Notify();
				}
			}
		}

		public string SelectedFile
		{
			get => selectedFile;
			set
			{
				if (value != selectedDirectory)
				{
					selectedFile = value;
					Notify(nameof(SelectedFile));
					CMDRemoveFile.Notify();
				}
			}
		}

		public RelayAction CMDAddDirectory { get; }
		public RelayAction CMDRemoveDirectory { get; }
		public RelayAction CMDAddFile { get; }
		public RelayAction CMDRemoveFile { get; }
		public RelayAction CMDRemoveProfile { get; }
		public RelayAction CMDSaveProfile { get; }

		private string sourcePath;
		ProfileViewModel profile;
		private string selectedDirectory;
		private string selectedFile;

		public ProfileVisualizationViewModel()
		{
			CMDAddDirectory = new RelayAction((param) => AddDirectory(), (param) => true);
			CMDRemoveDirectory = new RelayAction((param) => Profile.DirectoriesExcluded.Remove(SelectedDirectory), (param) => SelectedDirectory != null);
			CMDAddFile = new RelayAction((param) => AddFile(), (param) => true);
			CMDRemoveFile = new RelayAction((param) => Profile.FilesExcluded.Remove(SelectedFile), (param) => SelectedFile != null);
			CMDRemoveProfile = new RelayAction((param) => { Remove(); }, (param) => true);
			CMDSaveProfile = new RelayAction((param) => { if (!(bool)param) Save(); }, (param) => true);
		}

		private void AddDirectory()
		{
			FolderBrowserDialog window = new FolderBrowserDialog();

			if (window.ShowDialog() == DialogResult.OK)
			{
				var path = window.SelectedPath;

				if (path.StartsWith(SourcePath))
				{
					var relative = window.SelectedPath.Substring(SourcePath.Length + 1);
					Profile.DirectoriesExcluded.Add(relative);
				}
			}
		}

		private void AddFile()
		{
			OpenFileDialog window = new OpenFileDialog
			{
				InitialDirectory = SourcePath
			};

			if (window.ShowDialog() == DialogResult.OK)
			{
				var path = window.FileName;

				if (path.StartsWith(SourcePath))
				{
					var relative = path.Substring(SourcePath.Length + 1);
					Profile.FilesExcluded.Add(relative);
				}
			}
		}

		private void Save()
		{
			var serializer = new XmlSerializer(typeof(ProfileViewModel));

			serializer.Serialize(Profile.FileInfo.Open(FileMode.Create, FileAccess.Write, FileShare.Write), Profile);
			
		}

		private void Remove()
		{
			Profile.FileInfo.Delete();
		}
	}
}
