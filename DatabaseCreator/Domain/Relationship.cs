﻿using System;
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
		public virtual Type ParentType { get; set; }

		public virtual Type ChildType { get; set; }

		public virtual Database Database { get; set; }
	}
}
