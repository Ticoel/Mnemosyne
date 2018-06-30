using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Mnemosyne.Helpers
{
    public class EnumConverter : IValueConverter
    {
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			switch ((FontSize)value)
			{
				case FontSize.Small:
					return 16;
				case FontSize.Normal:
				default:
					return 24;
				case FontSize.Tall:
					return 32;
			};
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}
