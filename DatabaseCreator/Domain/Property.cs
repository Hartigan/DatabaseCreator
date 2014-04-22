﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
	public class Property : BaseClass
	{
		[MaxLength(128)]
		public string Name { get; set; }

		public bool IsIndexed { get; set; }

		[MaxLength(32)]
		public string SqlType { get; set; }

		public Type Type { get; set; }
	}
}
