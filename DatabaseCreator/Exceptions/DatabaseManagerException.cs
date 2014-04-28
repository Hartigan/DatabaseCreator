using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exceptions
{
	public class DatabaseManagerException : Exception
	{
		public DatabaseManagerException(string message) : base(message)
		{

		}

		public DatabaseManagerException(string message, Exception innerException) : base(message, innerException)
		{

		}
	}
}
