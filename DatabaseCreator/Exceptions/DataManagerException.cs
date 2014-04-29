using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exceptions
{
	public class DataManagerException : Exception
	{
		public DataManagerException(string message) : base(message)
		{

		}

		public DataManagerException(string message, Exception innerException)
			: base(message, innerException)
		{

		}
	}
}
