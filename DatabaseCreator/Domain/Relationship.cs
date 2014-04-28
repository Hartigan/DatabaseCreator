using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
	public class Relationship : BaseClass
	{
		[MaxLength(128)]
		public string Name { get; set; }
		public Type ParentType { get; set; }

		public Type ChildType { get; set; }

		public Database Database { get; set; }
	}
}
