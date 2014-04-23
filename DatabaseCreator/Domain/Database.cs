﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Domain
{
	public class Database : BaseClass
	{
		[MaxLength(128)]
		public string ServerName { get; set; }

		[MaxLength(128)]
		public string DatabaseName { get; set; }
	}
}