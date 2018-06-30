using System;
using System.Diagnostics;
using Windows.Storage;
using Windows.UI.Xaml.Data;

namespace Mnemosyne.Helpers
{
	class IStorageItemToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (value == null)
				return string.Empty;

			return ((IStorageItem)value).Name;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}
