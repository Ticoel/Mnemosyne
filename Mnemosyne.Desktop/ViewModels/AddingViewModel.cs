using Mnemosyne.Desktop.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;

namespace Mnemosyne.Desktop.ViewModels
{
	public sealed class AddingViewModel : CommonViewModel
	{
		public Profile Profil
		{
			get => profil;
			set
			{
				if (value != profil)
				{
					profil = value;
					Notify(nameof(Profil));
					CMDCreate.Notify();
				}
			}
		}

		public string NewDirectory
		{
			get => newDirectory;
			set
			{
				if (value != newDirectory)
				{
					newDirectory = value;
					Notify(nameof(NewDirectory));
					CMDAddDirectory.Notify();
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

		public string NewFile
		{
			get => newFile;
			set
			{
				if (value != newFile)
				{
					newFile = value;
					Notify(nameof(NewFile));
					CMDAddFile.Notify();
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
		public RelayAction CMDCreate { get; }

		private Profile profil;
		private string newDirectory;
		private string selectedDirectory;
		private string newFile;
		private string selectedFile;

		public AddingViewModel()
		{
			CMDAddDirectory = new RelayAction((param) => Profil.DirectoriesExcluded.Add(NewDirectory), (param) => !string.IsNullOrWhiteSpace(NewDirectory));
			CMDRemoveDirectory = new RelayAction((param) => Profil.DirectoriesExcluded.Remove(SelectedDirectory), (param) => SelectedDirectory != null);
			CMDAddFile = new RelayAction((param) => Profil.FilesExcluded.Add(NewFile), (param) => !string.IsNullOrWhiteSpace(NewFile));
			CMDRemoveFile = new RelayAction((param) => Profil.FilesExcluded.Remove(SelectedFile), (param) => SelectedFile != null);
			CMDCreate = new RelayAction((param) => Create(), (param) => CheckExistence());

			Profil = Profile.CreateDefault(true);
			Profil.Name = string.Empty;
		}

		private void Create()
		{
			var serializer = new XmlSerializer(typeof(Profile));

			using (var stream = new FileStream(Path.Combine(Application.LocalUserAppDataPath, "Profiles", Profil.Name + ".xml"), FileMode.Create, FileAccess.Write, FileShare.Write))
			{
				serializer.Serialize(stream, Profil);
			}
		}

		private bool CheckExistence()
		{
			return !File.Exists(Path.Combine(Application.LocalUserAppDataPath, "Profiles", Profil.Name + ".xml"));
		}
	}
}
