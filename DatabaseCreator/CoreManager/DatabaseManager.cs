using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityFrameworkDAL;
using Exceptions;
using DatabaseGenerator;
using Domain.Interfaces;

namespace CoreManager
{
	public class DatabaseManager
	{
		public void CreateDatabase(Domain.Database database)
		{
			using(IRepository repository = new EFRepository())
			{
				if (repository.Databases.Any(x=> string.Compare(database.ServerName,x.ServerName,true) == 0 && string.Compare(database.DatabaseName,x.DatabaseName,true) == 0))
				{
					throw new DatabaseManagerException("This database is exists");
				}

				try
				{
					DatabaseGenerator.DatabaseGenerator databaseGenerator = new DatabaseGenerator.DatabaseGenerator(database.ServerConnectionString);
					databaseGenerator.OpenConnection();
					databaseGenerator.CreateDatabase(database);
					databaseGenerator.CloseConnection();
				}
				catch(Exception ex)
				{
					throw new DatabaseManagerException("Database wasn't created",ex);
				}
				database.Created = DateTime.UtcNow;
				database.Modified = DateTime.UtcNow;
				repository.AddDatabase(database);
			}
		}

		public void DeleteDatabase(int databaseId)
		{
			using(IRepository repository = new EFRepository())
			{
				Domain.Database databaseEntity;
				try
				{
					databaseEntity = repository.Databases.Single(x=>x.Id == databaseId);
				}
				catch(Exception ex)
				{
					throw new DatabaseManagerException("Database with id = {" + databaseId + "} is not exists", ex);
				}

				try
				{
					DatabaseGenerator.DatabaseGenerator databaseGenerator = new DatabaseGenerator.DatabaseGenerator(databaseEntity.ServerConnectionString);
					databaseGenerator.OpenConnection();
					databaseGenerator.DropDatabase(databaseEntity);
					databaseGenerator.CloseConnection();
				}
				catch (Exception ex)
				{
					throw new DatabaseManagerException("Database wasn't droped", ex);
				}

				IEnumerable<int> types = repository.Types.Where(x => x.Database.Id == databaseEntity.Id).Select(x=>x.Id).ToList();
				IEnumerable<int> relationships = repository.Relationships.Where(x => x.Database.Id == databaseEntity.Id).Select(x => x.Id).ToList();
				IEnumerable<int> properties = repository.Properties.Where(x => types.Any(y => y == x.Type.Id)).Select(x => x.Id).ToList();

				foreach(int relationshipId in relationships)
				{
					repository.RemoveRelationship(relationshipId);
				}

				foreach(int propertyId in properties)
				{
					repository.RemoveProperty(propertyId);
				}

				foreach(int typeId in types)
				{
					repository.RemoveType(typeId);
				}

				repository.RemoveDatabase(databaseId);
			}
		}

		public void CreateType(Domain.Type type)
		{
			using (IRepository repository = new EFRepository())
			{
				if (repository.Types.Any(x=>x.Name == type.Name && 
										string.Compare(type.Database.ServerName,x.Database.ServerName,true) == 0 &&
										string.Compare(type.Database.DatabaseName,x.Database.DatabaseName,true) == 0))
				{
					throw new DatabaseManagerException("This type is exists");
				}

				Domain.Database database;
				try
				{
					database = repository.Databases.Single(x => string.Compare(type.Database.ServerName, x.ServerName, true) == 0 &&
																string.Compare(type.Database.DatabaseName, x.DatabaseName, true) == 0);
				}
				catch (Exception ex)
				{
					throw new DatabaseManagerException("Database with ServerName = {" + type.Database.ServerName + "} DatabaseName = {" + type.Database.DatabaseName  + "} is not exists", ex);
				}

				try
				{
					DatabaseGenerator.DatabaseGenerator databaseGenerator = new DatabaseGenerator.DatabaseGenerator(database.ServerConnectionString);
					databaseGenerator.OpenConnection();
					databaseGenerator.CreateType(type);
					databaseGenerator.CloseConnection();
				}
				catch (Exception ex)
				{
					throw new DatabaseManagerException("Type wasn't created", ex);
				}
				type.Created = DateTime.UtcNow;
				type.Modified = DateTime.UtcNow;
				type.Database = database;

				foreach(Domain.Property property in type.Properies)
				{
					property.Modified = property.Created = DateTime.UtcNow;
				}

				repository.AddType(type);
			}
		}

		public void DeleteType(int typeId)
		{
			using (IRepository repository = new EFRepository())
			{
				Domain.Type typeEntity;
				try
				{
					typeEntity = repository.Types.Single(x => x.Id == typeId);
				}
				catch (Exception ex)
				{
					throw new DatabaseManagerException("Type with id = {" + typeId + "} is not exists", ex);
				}

				IQueryable<Domain.Relationship> relationships = repository.Relationships.Where(x => x.ParentType.Id == typeEntity.Id || x.ChildType.Id == typeEntity.Id);
				if (relationships.Count() != 0)
				{
					string ids = string.Join(",", relationships.Select(x => x.Id.ToString()));
					throw new DatabaseManagerException("This type has dependencies. Ids = {" + ids + "}");
				}

				try
				{
					DatabaseGenerator.DatabaseGenerator databaseGenerator = new DatabaseGenerator.DatabaseGenerator(typeEntity.Database.ServerConnectionString);
					databaseGenerator.OpenConnection();
					databaseGenerator.DropType(typeEntity);
					databaseGenerator.CloseConnection();
				}
				catch (Exception ex)
				{
					throw new DatabaseManagerException("Type wasn't droped", ex);
				}

				IEnumerable<int> properties = repository.Properties.Where(x => x.Type.Id == typeEntity.Id).Select(x=>x.Id).ToList();

				foreach (int propertyId in properties)
				{
					repository.RemoveProperty(propertyId);
				}

				repository.RemoveType(typeId);
			}
		}

		public void CreateRelationship(Domain.Relationship relationship)
		{
			using (IRepository repository = new EFRepository())
			{
				if (repository.Relationships.Any(x => x.Name == relationship.Name &&
										string.Compare(relationship.Database.ServerName, x.Database.ServerName, true) == 0 &&
										string.Compare(relationship.Database.DatabaseName, x.Database.DatabaseName, true) == 0))
				{
					throw new DatabaseManagerException("This relationship is exists");
				}

				Domain.Database database;
				try
				{
					database = repository.Databases.Single(x => string.Compare(relationship.Database.ServerName, x.ServerName, true) == 0 &&
																string.Compare(relationship.Database.DatabaseName, x.DatabaseName, true) == 0);
				}
				catch (Exception ex)
				{
					throw new DatabaseManagerException("Database with ServerName = {" + relationship.Database.ServerName + "} DatabaseName = {" + relationship.Database.DatabaseName + "} is not exists", ex);
				}

				Domain.Type parentType, childType;
				try
				{
					parentType = repository.Types.Single(x => x.Database.Id == database.Id && x.Name == relationship.ParentType.Name);
					childType = repository.Types.Single(x => x.Database.Id == database.Id && x.Name == relationship.ChildType.Name);
				}
				catch (Exception ex)
				{
					throw new DatabaseManagerException("Types id not exists", ex);
				}

				relationship.ParentType = parentType;
				relationship.ChildType = childType;

				try
				{
					DatabaseGenerator.DatabaseGenerator databaseGenerator = new DatabaseGenerator.DatabaseGenerator(database.ServerConnectionString);
					databaseGenerator.OpenConnection();
					databaseGenerator.CreateRelationship(relationship);
					databaseGenerator.CloseConnection();
				}
				catch (Exception ex)
				{
					throw new DatabaseManagerException("Relationship wasn't created", ex);
				}
				relationship.Created = DateTime.UtcNow;
				relationship.Modified = DateTime.UtcNow;
				relationship.Database = database;
				repository.AddRelationship(relationship);
			}
		}

		public void DeleteRelationship(int relationshipId)
		{
			using (IRepository repository = new EFRepository())
			{
				Domain.Relationship relationshipEntity;
				try
				{
					relationshipEntity = repository.Relationships.Single(x => x.Id == relationshipId);
				}
				catch (Exception ex)
				{
					throw new DatabaseManagerException("Relationship with id = {" + relationshipId + "} is not exists", ex);
				}

				try
				{
					DatabaseGenerator.DatabaseGenerator databaseGenerator = new DatabaseGenerator.DatabaseGenerator(relationshipEntity.Database.ServerConnectionString);
					databaseGenerator.OpenConnection();
					databaseGenerator.DropRelationship(relationshipEntity);
					databaseGenerator.CloseConnection();
				}
				catch (Exception ex)
				{
					throw new DatabaseManagerException("Relationship wasn't droped", ex);
				}

				repository.RemoveRelationship(relationshipId);
			}
		}
	}
}
