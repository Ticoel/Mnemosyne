using System;
using System.Collections.Generic;
using Windows.Storage;

namespace Mnemosyne
{
	public static class Settings
	{
		public static FontSize FontSize
		{
			get => (FontSize)(ApplicationData.Current.LocalSettings.Values["FontSize"] ?? FontSize.Normal);
			set => ApplicationData.Current.LocalSettings.Values["FontSize"] = (int)value;
		}

		private static event EventHandler Notify;

		private static List<Type> viewModels;

		public static void AddListener(Type viewModelType, EventHandler listener)
		{
			if (viewModels == null)
				viewModels = new List<Type>();

			if (viewModels.Contains(viewModelType))
				return;

			viewModels.Add(viewModelType);
			Notify += listener;
		}

		public static void RestoreDefaults()
		{
			ApplicationData.Current.LocalSettings.Values.Clear();
			Notify?.Invoke(null, EventArgs.Empty);
		}
	}
}
