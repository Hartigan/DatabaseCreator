using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFrameworkDAL
{
	class DomainContext : DbContext
	{
		public DomainContext() : base("CoreDatabaseConnectionString")
		{

		}

		public DbSet<Domain.Database> Databases { get; set; }

		public DbSet<Domain.Property> Properties { get; set; }

		public DbSet<Domain.Relationship> Relationsips { get; set; }

		public DbSet<Domain.Type> Types { get; set; }
	}
}
