using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
	public class Relationship : BaseClass
	{
		public Type ParentType { get; set; }

		public Type ChildType { get; set; }
	}
}
