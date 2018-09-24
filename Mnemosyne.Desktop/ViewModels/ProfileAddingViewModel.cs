using Mnemosyne.Desktop.Helpers;
using System.Xml.Serialization;
using System.IO;
using System.Windows.Forms;

namespace Mnemosyne.Desktop.ViewModels
{
	public sealed class ProfileAddingViewModel : CommonViewModel
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
					CMDCreateProfile.Notify();
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
		public RelayAction CMDCreateProfile { get; }

		private string sourcePath;
		private ProfileViewModel profile;
		private string selectedDirectory;
		private string selectedFile;
		private string path;

		public ProfileAddingViewModel()
		{
			CMDAddDirectory = new RelayAction((param) => AddDirectory(), (param) => true);
			CMDRemoveDirectory = new RelayAction((param) => Profile.DirectoriesExcluded.Remove(SelectedDirectory), (param) => SelectedDirectory != null);
			CMDAddFile = new RelayAction((param) => AddFile(), (param) => true);
			CMDRemoveFile = new RelayAction((param) => Profile.FilesExcluded.Remove(SelectedFile), (param) => SelectedFile != null);
			CMDCreateProfile = new RelayAction((param) => CreateProfile(), (param) => CheckProfileExistence());

			Profile = ProfileViewModel.CreateDefault(true);
			Profile.Name = string.Empty;
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

		private void CreateProfile()
		{
			var serializer = new XmlSerializer(typeof(ProfileViewModel));

			using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write))
			{
				serializer.Serialize(stream, Profile);
			}
		}

		private bool CheckProfileExistence()
		{
			if (string.IsNullOrWhiteSpace(Profile.Name))
				return false;

			var filename = Profile.Name + ".xml";

			foreach (var invalidChar in Path.GetInvalidFileNameChars())
				filename = filename.Replace(invalidChar, '_');

			path = Path.Combine(Application.LocalUserAppDataPath, "Profiles", filename);

			return !File.Exists(path);
		}
	}
}
