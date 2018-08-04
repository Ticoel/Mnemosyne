using Mnemosyne.Desktop.Helpers;
using Mnemosyne.Desktop.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Serialization;

namespace Mnemosyne.Desktop.ViewModels
{
	public class ProfilViewModel : CommonViewModel
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
				}
			}
		}


		public RelayAction CMDSave { get; private set; }
		public RelayAction CMDRemove { get; private set; }

		Profile profil;

		public ProfilViewModel()
		{
			CMDSave = new RelayAction((param) => { if (!(bool)param) Save(); }, (param) => true);
		}

		private void Save()
		{
			var serializer = new XmlSerializer(typeof(Profile));

			serializer.Serialize(Profil.FileInfo.Open(System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.Write), Profil);

			Debug.WriteLine("Save");
		}

		public void Remove()
		{
			Profil.FileInfo.Delete();

			Debug.WriteLine("Remove");
		}
	}
}
