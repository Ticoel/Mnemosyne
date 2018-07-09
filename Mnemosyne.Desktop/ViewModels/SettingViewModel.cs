using Mnemosyne.Desktop.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Mnemosyne.ViewModels
{
	public class SettingViewModel : CommonViewModel
	{
		public List<FontSize> FontSizes
		{
			get => Enum.GetValues(typeof(FontSize)).Cast<FontSize>().ToList();
		}
	}
}
