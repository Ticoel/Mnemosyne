using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Mnemosyne.Desktop.Models
{
	[XmlRoot("profile")]
	public class Profile
	{
		[XmlIgnore()]
		public FileInfo FileInfo { get; set; }

		[XmlIgnore()]
		public bool IsModifiable { get; }

		[XmlAttribute("name")]
		public string Name { get; set; }

		[XmlElement("bufferlength")]
		public int BufferLength { get; set; }

		[XmlElement("creationtime")]
		public bool CreationTime { get; set; }

		[XmlElement("lastaccesstime")]
		public bool LastAccessTime { get; set; }

		[XmlElement("lastwritetime")]
		public bool LastWriteTime { get; set; }

		[XmlElement("attributes")]
		public bool Attributes { get; set; }

		[XmlElement("accesscontrol")]
		public bool AccessControl { get; set; }

		[XmlArray("directoriesexcluded")]
		[XmlArrayItem("directory")]
		public List<string> DirectoriesExcluded { get; set; }

		[XmlArray("filesexcluded")]
		[XmlArrayItem("file")]
		public List<string> FilesExcluded { get; set; }

		public Profile()
		{
			IsModifiable = true;
		}

		public Profile(bool isModifiable)
		{
			IsModifiable = isModifiable;
		}

		public static Profile CreateDefault(bool isModifiable)
		{
			return new Profile(isModifiable)
			{
				Name = "default",
				BufferLength = 1024,
				CreationTime = true,
				LastAccessTime = true,
				LastWriteTime = true,
				Attributes = true,
				AccessControl = true,
				DirectoriesExcluded = new List<string>()
				{
					"My Directory"
				},
				FilesExcluded = new List<string>()
				{
					"My File"
				}
			};
		}
	}
}
