using Mnemosyne.Helpers;
using Mnemosyne.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Mnemosyne.ViewModels
{
	public class SettingViewModel : BaseViewModel
	{
		public List<FontSize> FontSizes
			=> Enum.GetValues(typeof(FontSize)).Cast<FontSize>().ToList();

		public RelayAction RestoreDefaultsAction
		{
			get
			{
				return restoreDefaultsAction;
			}
			set
			{
				if (value != restoreDefaultsAction)
				{
					restoreDefaultsAction = value;
					Notify("SaveAction");
				}
			}
		}

		private RelayAction restoreDefaultsAction;

		public SettingViewModel()
		{
			RestoreDefaultsAction = new RelayAction((parameter) =>
			{
				Settings.RestoreDefaults();;
			}, (parameter) =>
			{
				return true;
			});
		}
	}
}
