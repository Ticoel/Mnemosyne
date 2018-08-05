using Mnemosyne.Desktop.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Serialization;

namespace Mnemosyne.Desktop.ViewModels
{
	public class VisualizationViewModel : CommonViewModel
	{
		public ProfileViewModel Profil
		{
			get => profil;
			set
			{
				if (value != profil)
				{
					profil = value;
					Notify(nameof(Profil));
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
		public RelayAction CMDSave { get; private set; }

		ProfileViewModel profil;
		private string newDirectory;
		private string selectedDirectory;
		private string newFile;
		private string selectedFile;

		public VisualizationViewModel()
		{
			CMDAddDirectory = new RelayAction((param) => Profil.DirectoriesExcluded.Add(NewDirectory), (param) => !string.IsNullOrWhiteSpace(NewDirectory));
			CMDRemoveDirectory = new RelayAction((param) => Profil.DirectoriesExcluded.Remove(SelectedDirectory), (param) => SelectedDirectory != null);
			CMDAddFile = new RelayAction((param) => Profil.FilesExcluded.Add(NewFile), (param) => !string.IsNullOrWhiteSpace(NewFile));
			CMDRemoveFile = new RelayAction((param) => Profil.FilesExcluded.Remove(SelectedFile), (param) => SelectedFile != null);

			CMDSave = new RelayAction((param) => { if (!(bool)param) Save(); }, (param) => true);
		}

		private void Save()
		{
			var serializer = new XmlSerializer(typeof(ProfileViewModel));

			serializer.Serialize(Profil.FileInfo.Open(System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.Write), Profil);
			
		}

		public void Remove()
		{
			Profil.FileInfo.Delete();
		}
	}
}
