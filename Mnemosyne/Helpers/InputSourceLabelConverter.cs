using System;
using Windows.Storage;
using Windows.UI.Xaml.Data;

namespace Mnemosyne.Helpers
{
	class InputSourceLabelConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (value == null)
				return "Source";

			return ((StorageFolder)value).DisplayName;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}
