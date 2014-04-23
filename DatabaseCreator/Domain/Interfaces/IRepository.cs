using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
	public interface IRepository : IDisposable
	{
		void AddDatabase(Database database);
		void UpdateDatabase(Database database);
		void RemoveDatabase(int databaseId);
		IQueryable<Database> Databases { get; }

		void AddProperty(Property property);
		void UpdateProperty(Property property);
		void RemoveProperty(int propertyId);
		IQueryable<Property> Properties { get; }

		void AddRelationship(Relationship relationship);
		void UpdateRelationship(Relationship relationship);
		void RemoveRelationship(int relationshipId);
		IQueryable<Relationship> Relationships { get; }

		void AddType(Type type);
		void UpdateType(Type type);
		void RemoveType(int typeId);
		IQueryable<Type> Types { get; }

	}
}
