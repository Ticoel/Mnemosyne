using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace Mnemosyne.Desktop.Helpers
{
	public class PathToFileNameConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is string path)
				return Path.GetFileName(path);
			else if (parameter is string label)
				return label;
			else
				return string.Empty;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
