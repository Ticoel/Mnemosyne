using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mnemosyne.Desktop.Helpers
{
	public class DirectoryComparer : IEqualityComparer<DirectoryInfo>
	{
		public bool Equals(DirectoryInfo x, DirectoryInfo y)
		{
			return x.Name == y.Name;
		}

		public int GetHashCode(DirectoryInfo obj)
		{
			return obj.Name.GetHashCode();
		}
	}
}
