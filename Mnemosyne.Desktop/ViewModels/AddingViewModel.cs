using Mnemosyne.Desktop.Models;
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

		public string ProfilName
		{
			get => Profil.Name;
			set
			{
				if (value != Profil.Name)
				{
					Profil.Name = value;
					Notify(nameof(ProfilName));
					CMDCreate.Notify();
				}
			}
		}

		public RelayAction CMDCreate { get; }

		private Profile profil;

		public AddingViewModel()
		{
			CMDCreate = new RelayAction((param) => Create(), (param) => CheckExistence());

			Profil = Profile.CreateDefault(true);
			Profil.Name = "";
		}

		private void Create()
		{
			var serializer = new XmlSerializer(typeof(Profile));

			using (var stream = new FileStream(Path.Combine(Application.LocalUserAppDataPath, "Profiles", ProfilName + ".xml"), FileMode.Create, FileAccess.Write, FileShare.Write))
			{
				serializer.Serialize(stream, Profil);
			}
		}

		private bool CheckExistence()
		{
			return !File.Exists(Path.Combine(Application.LocalUserAppDataPath, "Profiles", ProfilName + ".xml"));
		}
	}
}
