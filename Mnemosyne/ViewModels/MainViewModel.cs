using Mnemosyne.Helpers;
using Mnemosyne.Views;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Mnemosyne.ViewModels
{
	public sealed class MainViewModel : BaseViewModel
	{
		public long FilePosition
		{
			get
			{
				return filePosition;
			}
			private set
			{
				if (value != filePosition)
				{
					filePosition = value;
					Notify("FilePosition");
					Notify("ItemPosition");
					Notify("ProgressPercent");
				}
			}
		}

		public long FolderPosition
		{
			get
			{
				return folderPosition;
			}
			private set
			{
				if (value != folderPosition)
				{
					folderPosition = value;
					Notify("FolderPosition");
					Notify("ItemPosition");
					Notify("ProgressPercent");
				}
			}
		}

		public long ItemPosition
			=> FilePosition + folderPosition;

		public long FileCount
		{
			get
			{
				return fileCount;
			}
			private set
			{
				if (value != fileCount)
				{
					fileCount = value;
					Notify("FileCount");
					Notify("ItemCount");
					Notify("ProgressPercent");
				}
			}
		}

		public long FolderCount
		{
			get
			{
				return folderCount;
			}
			private set
			{
				if (value != folderCount)
				{
					folderCount = value;
					Notify("FolderCount");
					Notify("ItemCount");
					Notify("ProgressPercent");
				}
			}
		}
		
		public long ItemCount
			=> FileCount + FolderCount;

		public float ProgressPercent
			=> (float)ItemPosition / ItemCount * 100f;

		public IStorageItem CurrentItem
		{
			get
			{
				return currentItem;
			}
			private set
			{
				if (value != currentItem)
				{
					currentItem = value;
					Notify("CurrentItem");
				}
			}
		}

		public StorageFolder Source
		{
			get
			{
				return source;
			}
			set
			{
				if (value != source)
				{
					source = value;
					Notify("Source");
					SelectDestinationAction.NotifyCanExecuteChanged();
					StartAction.NotifyCanExecuteChanged();
				}
			}
		}

		public StorageFolder Destination
		{
			get
			{
				return destination;
			}
			set
			{
				if (value != destination)
				{
					destination = value;
					Notify("Destination");
					StartAction.NotifyCanExecuteChanged();
				}
			}
		}

		public RelayAction SelectSourceAction { get; }
		public RelayAction SelectDestinationAction { get; }
		public RelayAction StartAction { get; }
		public RelayAction StopAction { get; }
		public RelayAction NavigateToSetting { get; }

		public bool IsRunning
		{
			get
			{
				return isRunning;
			}
			set
			{
				if (value != isRunning)
				{
					isRunning = value;
					Notify("IsRunning");
					StopAction.NotifyCanExecuteChanged();
				}
			}
		}

		private long filePosition;
		private long folderPosition;
		private long fileCount;
		private long folderCount;
		private StorageFolder source;
		private StorageFolder destination;
		private IStorageItem currentItem;
		private SynchronizationContext currentSynchronizationContext;
		private FolderPicker folderPicker;
		private bool isRunning;
		private IAsyncAction a;
		private IAsyncAction b;
		private IAsyncOperation<StorageFile> c;
		private IAsyncOperation<StorageFolder> d;

		public MainViewModel()
		{
			currentSynchronizationContext = SynchronizationContext.Current;

			folderPicker = new FolderPicker();
			folderPicker.FileTypeFilter.Add("*");

			SelectSourceAction = new RelayAction(async (parameter) => { await SelectSource(); }, (parameter) => { return true; });

			SelectDestinationAction = new RelayAction(async (parameter) => { await SelectDestination(); }, (parameter) => { return Source != null; });

			StartAction = new RelayAction(async (parameter) =>
			{
				try
				{
					IsRunning = true;
					await Start();
					isRunning = false;
				}
				catch (TaskCanceledException)
				{ }
			}, (parameter) =>
			{
				return Source != null && Destination != null;
			});

			StopAction = new RelayAction((parameter) => { Stop(); }, (parameter) => { return IsRunning; });

			NavigateToSetting = new RelayAction((parameter) =>
			{
				((Frame)Window.Current.Content).Navigate(typeof(SettingPage));
			}, (parameter) =>
			{
				return true;
			});
		}

		public MainViewModel(StorageFolder source, StorageFolder destination) : this()
		{
			Source = source ?? throw new ArgumentNullException();
			Destination = destination ?? throw new ArgumentNullException();
		}

		private IAsyncAction Count(StorageFolder parent)
		{
			return CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
			{
				var childs = await parent.GetItemsAsync();

				foreach (var child in childs)
				{
					if (child is StorageFile file)
					{
						FileCount++;
					}
					else if (child is StorageFolder folder)
					{
						FolderCount++;
						await Count(folder);
					}
				}
			});
		}

		private IAsyncAction Copy(StorageFolder source, StorageFolder destination)
		{
			return CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
			{
				var sourceChilds = await source.GetItemsAsync();

				foreach (var sourceChild in sourceChilds)
				{
					var relative = sourceChild.Path.Substring(source.Path.Length + 1);
					var absolute = Path.Combine(destination.Path, relative);
					var name = sourceChild.Name;
					var destinationItem = await destination.TryGetItemAsync(name);

					CurrentItem = sourceChild;

					if (sourceChild is StorageFile sourceFile)
					{
						if (destinationItem != null)
						{
							var destinationFile = (StorageFile)destinationItem;
							b = sourceFile.CopyAndReplaceAsync(destinationFile);
							await b;
						}
						else
						{
							c = sourceFile.CopyAsync(destination);
							await c;
						}

						FilePosition++;
					}
					else if (sourceChild is StorageFolder sourceFolder)
					{
						StorageFolder destinationFolder = null;

						if (destinationItem != null)
							destinationFolder = (StorageFolder)destinationItem;
						else
						{
							d = destination.CreateFolderAsync(name);
							destinationFolder = await d;
						}

						await Copy(sourceFolder, destinationFolder);

						FolderPosition++;
					}
				}
			});
		}

		public IAsyncAction Start()
		{
			a = CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
			{
				await Count(Source);
				await Copy(Source, Destination);
			});

			return a;
		}

		public void Stop()
		{
			a.Cancel();
			b?.Cancel();
			c?.Cancel();
			d?.Cancel();
			a = b = null;
			c = null;
			d = null;
		}

		public IAsyncAction SelectSource()
		{
			return CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
			{
				Source = await folderPicker.PickSingleFolderAsync();
			});
		}

		public IAsyncAction SelectDestination()
		{
			return CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
			{
				Destination = await folderPicker.PickSingleFolderAsync();
			});
		}
	}
}
