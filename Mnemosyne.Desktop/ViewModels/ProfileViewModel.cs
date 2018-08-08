using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Mnemosyne.Desktop.ViewModels
{
	[XmlRoot("profile")]
	public class ProfileViewModel : CommonViewModel
	{
		[XmlIgnore()]
		public FileInfo FileInfo { get; set; }

		[XmlIgnore()]
		public bool IsModifiable { get; }

		[XmlIgnore()]
		public string Name
		{
			get => name;
			set
			{
				if (value != name)
				{
					name = value;
					Notify(nameof(Name));
				}
			}
		}

		[XmlIgnore()]
		public int BufferLength
		{
			get => bufferLength;
			set
			{
				if (value != bufferLength)
				{
					bufferLength = value;
					Notify(nameof(BufferLength));
				}
			}
		}

		[XmlIgnore()]
		public bool CreationTime
		{
			get => creationTime;
			set
			{
				if (value != creationTime)
				{
					creationTime = value;
					Notify(nameof(CreationTime));
				}
			}
		}

		[XmlIgnore()]
		public bool LastAccessTime
		{
			get => lastAccessTime;
			set
			{
				if (value != lastAccessTime)
				{
					lastAccessTime = value;
					Notify(nameof(LastAccessTime));
				}
			}
		}

		[XmlIgnore()]
		public bool LastWriteTime
		{
			get => lastWriteTime;
			set
			{
				if (value != lastWriteTime)
				{
					lastWriteTime = value;
					Notify(nameof(LastWriteTime));
				}
			}
		}

		[XmlIgnore()]
		public bool Attributes
		{
			get => attributes;
			set
			{
				if (value != attributes)
				{
					attributes = value;
					Notify(nameof(Attributes));
				}
			}
		}

		[XmlIgnore()]
		public bool AccessControl
		{
			get => accessControl;
			set
			{
				if (value != accessControl)
				{
					accessControl = value;
					Notify(nameof(AccessControl));
				}
			}
		}

		[XmlIgnore()]
		public ObservableCollection<string> DirectoriesExcluded
		{
			get => directoriesExcluded;
			set
			{
				if (value != directoriesExcluded)
				{
					directoriesExcluded = value;
					Notify(nameof(DirectoriesExcluded));
				}
			}
		}

		[XmlIgnore()]
		public ObservableCollection<string> FilesExcluded
		{
			get => filesExcluded;
			set
			{
				if (value != filesExcluded)
				{
					filesExcluded = value;
					Notify(nameof(FilesExcluded));
				}
			}
		}

		[XmlElement("name")]
		public string name;

		[XmlElement("bufferlength")]
		public int bufferLength;

		[XmlElement("creationtime")]
		public bool creationTime;

		[XmlElement("lastaccesstime")]
		public bool lastAccessTime;

		[XmlElement("lastwritetime")]
		public bool lastWriteTime;

		[XmlElement("attributes")]
		public bool attributes;

		[XmlElement("accesscontrol")]
		public bool accessControl;

		[XmlArray("directoriesexcluded")]
		[XmlArrayItem("directory")]
		public ObservableCollection<string> directoriesExcluded;

		[XmlArray("filesexcluded")]
		[XmlArrayItem("file")]
		public ObservableCollection<string> filesExcluded;

		public ProfileViewModel()
		{
			IsModifiable = true;
		}

		public ProfileViewModel(bool isModifiable)
		{
			IsModifiable = isModifiable;
		}

		public static ProfileViewModel CreateDefault(bool isModifiable)
		{
			return new ProfileViewModel(isModifiable)
			{
				Name = "default",
				BufferLength = 4096,
				CreationTime = true,
				LastAccessTime = true,
				LastWriteTime = true,
				Attributes = true,
				AccessControl = true,
				DirectoriesExcluded = new ObservableCollection<string>(),
				FilesExcluded = new ObservableCollection<string>()
			};
		}
	}
}
