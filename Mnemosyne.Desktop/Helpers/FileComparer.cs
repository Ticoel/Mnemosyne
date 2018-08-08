using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mnemosyne.Desktop.Helpers
{
	public class FileComparer : IEqualityComparer<FileInfo>
	{
		public bool Equals(FileInfo x, FileInfo y)
		{
			return x.Name == y.Name;
		}

		public int GetHashCode(FileInfo obj)
		{
			return obj.Name.GetHashCode();
		}
	}
}
