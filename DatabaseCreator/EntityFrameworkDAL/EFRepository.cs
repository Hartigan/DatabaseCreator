using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain;
using Domain.Interfaces;

namespace EntityFrameworkDAL
{
	class EFRepository : IRepository
	{
		private DomainContext _context;
		public EFRepository()
		{
			_context = new DomainContext();
		}

		public void AddDatabase(Database database)
		{
			_context.Databases.Add(database);
			_context.SaveChanges();
		}

		public void UpdateDatabase(Database database)
		{
			Database obj = _context.Databases.Single(x => x.Id == database.Id);
			obj.Created = database.Created;
			obj.DatabaseName = database.DatabaseName;
			obj.Modified = database.Modified;
			obj.ServerName = database.ServerName;
			_context.SaveChanges();
		}

		public void RemoveDatabase(int databaseId)
		{
			Database obj = _context.Databases.Single(x => x.Id == databaseId);
			_context.Databases.Remove(obj);
			_context.SaveChanges();
		}

		public IQueryable<Database> Databases
		{
			get { return _context.Databases; }
		}

		public void AddProperty(Property property)
		{
			_context.Properties.Add(property);
			_context.SaveChanges();
		}

		public void UpdateProperty(Property property)
		{
			Property obj = _context.Properties.Single(x => x.Id == property.Id);
			obj.Created = property.Created;
			obj.IsIndexed = property.IsIndexed;
			obj.Modified = property.Modified;
			obj.Name = property.Name;
			obj.SqlType = property.SqlType;
			obj.Type = property.Type;
			_context.SaveChanges();
		}

		public void RemoveProperty(int propertyId)
		{
			Property obj = _context.Properties.Single(x => x.Id == propertyId);
			_context.SaveChanges();
		}

		public IQueryable<Property> Properties
		{
			get { return _context.Properties; }
		}

		public void AddRelationship(Relationship relationship)
		{
			_context.Relationsips.Add(relationship);
			_context.SaveChanges();
		}

		public void UpdateRelationship(Relationship relationship)
		{
			Relationship obj = _context.Relationsips.Single(x => x.Id == relationship.Id);
			obj.ChildType = relationship.ChildType;
			obj.Created = relationship.Created;
			obj.Modified = relationship.Modified;
			obj.Name = relationship.Name;
			obj.ParentType = relationship.ParentType;
			_context.SaveChanges();
		}

		public void RemoveRelationship(int relationshipId)
		{
			Relationship obj = _context.Relationsips.Single(x => x.Id == relationshipId);
			_context.Relationsips.Remove(obj);
			_context.SaveChanges();
		}

		public IQueryable<Relationship> Relationships
		{
			get { return _context.Relationsips; }
		}

		public void AddType(Domain.Type type)
		{
			_context.Types.Add(type);
			_context.SaveChanges();
		}

		public void UpdateType(Domain.Type type)
		{
			Domain.Type obj = _context.Types.Single(x => x.Id == type.Id);
			obj.Created = type.Created;
			obj.Database = type.Database;
			obj.Modified = type.Modified;
			obj.Name = type.Name;
			_context.SaveChanges();
		}

		public void RemoveType(int typeId)
		{
			Domain.Type obj = _context.Types.Single(x => x.Id == typeId);
			_context.Types.Remove(obj);
			_context.SaveChanges();
		}

		public IQueryable<Domain.Type> Types
		{
			get { return _context.Types; }
		}

		public void Dispose()
		{
			_context.Dispose();
		}
	}
}
