﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
	public class Type : BaseClass
	{
		[MaxLength(128)]
		public string Name { get; set; }

		public virtual Database Database { get; set; }

		public virtual ICollection<Property> Properties { get; set; }
	}
}
