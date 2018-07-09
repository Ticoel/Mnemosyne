using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;

namespace Mnemosyne.Desktop.Helpers
{
	public class FontSizeToIntConverter : IValueConverter
    {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
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

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
