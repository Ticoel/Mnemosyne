using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mnemosyne.Desktop.ViewModels
{
	public class ActivityViewModel : CommonViewModel
	{
		public long FileCount
		{
			get => fileCount;
			set
			{
				if (value != fileCount)
				{
					fileCount = value;
					Notify(nameof(FileCount));
					Notify(nameof(ItemCount));
				}
			}
		}

		public long FolderCount
		{
			get => folderCount;
			set
			{
				if (value != folderCount)
				{
					folderCount = value;
					Notify(nameof(FolderCount));
					Notify(nameof(ItemCount));
				}
			}
		}

		public long ItemCount
		{
			get => FileCount + FolderCount;
		}

		public long ByteCount
		{
			get => byteCount;
			set
			{
				if (value != byteCount)
				{
					byteCount = value;
					Notify(nameof(ByteCount));
				}
			}
		}

		public long CopiedByte
		{
			get => copiedByte;
			set
			{
				if (value != copiedByte)
				{
					copiedByte = value;
					Notify(nameof(CopiedByte));
				}
			}
		}

		private long fileCount;
		private long folderCount;
		private long byteCount;
		private long copiedByte;
	}
}
