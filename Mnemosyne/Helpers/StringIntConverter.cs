using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Mnemosyne.Helpers
{
    class StringIntConverter : IValueConverter
    {
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			return ((int)value).ToString();
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			if (int.TryParse((string)value, out int result))
				return result;
			else
				return 0;
		}
	}
}
