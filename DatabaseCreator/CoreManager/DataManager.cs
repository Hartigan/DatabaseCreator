using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exceptions;
using Domain.Interfaces;
using System.Xml.Linq;
using System.Data.SqlClient;

namespace CoreManager
{
	public class DataManager
	{
		public XDocument GetObjectsByType(XDocument types)
		{
			XDocument result = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
			XElement root = new XElement("objects");
			result.Add(root);

			XElement objectTypes = types.Element("objects");
			foreach(XElement xmlType in objectTypes.Elements("object"))
			{
				string strTypeId = xmlType.Attribute("typeId").Value;
				int typeId;
				if (!int.TryParse(strTypeId, out typeId))
				{
					throw new DataManagerException("Invalid typeId");
				}
				using(Domain.Interfaces.IRepository repository = new EntityFrameworkDAL.EFRepository())
				{
					Domain.Type type = repository.Types.Single(x=>x.Id == typeId);

					SqlConnection connection = new SqlConnection(type.Database.ServerConnectionString);

					string columnNames = "Id," + string.Join(",", type.Properties.Select(x => x.Name));
					StringBuilder query = new StringBuilder();
					query.AppendLine("USE [" + type.Database.DatabaseName + "]");
					query.Append("SELECT ");
					query.AppendLine(columnNames);
					query.AppendLine("FROM [" + type.Name + "]");

					try
					{
						connection.Open();
						SqlCommand command = new SqlCommand(query.ToString(), connection);
						SqlDataReader dataReader = command.ExecuteReader();
						while(dataReader.Read())
						{
							XElement xmlObject = new XElement("object");
							xmlObject.Add(new XAttribute("type",type.Name));
							xmlObject.Add(new XAttribute("id",dataReader["Id"].ToString()));
							xmlObject.Add(new XAttribute("typeId",type.Id));
							foreach(Domain.Property property in type.Properties)
							{
								XElement xmlProperty = new XElement(property.Name, dataReader[property.Name]);
								xmlObject.Add(xmlProperty);
							}
							root.Add(xmlObject);
						}
						connection.Close();
					}
					catch (Exception ex)
					{
						throw new DataManagerException("Invalid operation", ex);
					}
				}
			}

			return result;
		}

		public XDocument GetObjectByTypeAndId(XDocument doc)
		{
			XDocument result = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
			XElement root = new XElement("objects");
			result.Add(root);

			XElement objectTypes = doc.Element("objects");
			foreach (XElement xmlType in objectTypes.Elements("object"))
			{
				string strTypeId = xmlType.Attribute("typeId").Value;
				int typeId;
				if (!int.TryParse(strTypeId, out typeId))
				{
					throw new DataManagerException("Invalid typeId");
				}

				string strId = xmlType.Attribute("id").Value;
				int id;
				if (!int.TryParse(strId, out id))
				{
					throw new DataManagerException("Invalid id");
				}

				using (Domain.Interfaces.IRepository repository = new EntityFrameworkDAL.EFRepository())
				{
					Domain.Type type = repository.Types.Single(x => x.Id == typeId);

					SqlConnection connection = new SqlConnection(type.Database.ServerConnectionString);

					string columnNames = "Id," + string.Join(",", type.Properties.Select(x => x.Name));
					StringBuilder query = new StringBuilder();
					query.AppendLine("USE [" + type.Database.DatabaseName + "]");
					query.Append("SELECT ");
					query.AppendLine(columnNames);
					query.AppendLine("FROM [" + type.Name + "]");
					query.AppendLine("WHERE Id = " + id);

					try
					{
						connection.Open();
						SqlCommand command = new SqlCommand(query.ToString(), connection);
						SqlDataReader dataReader = command.ExecuteReader();
						while (dataReader.Read())
						{
							XElement xmlObject = new XElement("object");
							xmlObject.Add(new XAttribute("type", type.Name));
							xmlObject.Add(new XAttribute("id", dataReader["Id"].ToString()));
							xmlObject.Add(new XAttribute("typeId", type.Id));
							foreach (Domain.Property property in type.Properties)
							{
								XElement xmlProperty = new XElement(property.Name, dataReader[property.Name]);
								xmlObject.Add(xmlProperty);
							}
							root.Add(xmlObject);
						}
						connection.Close();
					}
					catch (Exception ex)
					{
						throw new DataManagerException("Invalid operation", ex);
					}
				}
			}

			return result;
		}

		public XDocument AddObjects(XDocument objects)
		{
			XElement root = objects.Element("objects");
			XDocument result = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
			XElement rootResult = new XElement("objects");
			result.Add(rootResult);

			foreach(XElement xmlObject in root.Elements("object"))
			{
				string strTypeId = xmlObject.Attribute("typeId").Value;
				int typeId;
				if (!int.TryParse(strTypeId, out typeId))
				{
					throw new DataManagerException("Invalid typeId");
				}

				using(Domain.Interfaces.IRepository repository = new EntityFrameworkDAL.EFRepository())
				{
					Domain.Type type;
					try
					{
						type = repository.Types.Single(x => x.Id == typeId);
					}
					catch(Exception ex)
					{
						throw new DataManagerException("Type with id = {" + typeId + "} is not exists");
					}

					SqlConnection connection = new SqlConnection(type.Database.ServerConnectionString);

					Domain.Property[] properties = type.Properties.Where(x => xmlObject.Elements().Any(y => string.Compare(y.Name.ToString(), x.Name, true) == 0)).ToArray();
					string[] values = new string[properties.Length];
					for(int propertyIndex=0;propertyIndex<properties.Length;propertyIndex++)
					{
						values[propertyIndex] = "CONVERT(" + properties[propertyIndex].SqlType + ",'" + xmlObject.Element(properties[propertyIndex].Name).Value + "')";
					}
					string allValues = string.Join(",",values);
					string columnNames = string.Join(",", properties.Select(x => x.Name));
					StringBuilder query = new StringBuilder();
					query.AppendLine("USE [" + type.Database.DatabaseName + "]");
					query.AppendLine("INSERT INTO [" + type.Name + "]");
					query.AppendLine("(");
					query.AppendLine(columnNames);
					query.AppendLine(")");
					query.AppendLine("OUTPUT INSERTED.Id");
					query.AppendLine("VALUES");
					query.AppendLine("(");
					query.AppendLine(allValues);
					query.AppendLine(")");

					try
					{
						connection.Open();
						SqlCommand command = new SqlCommand(query.ToString(), connection);
						int objectId = (int)command.ExecuteScalar();
						rootResult.Add(new XElement("object", new XAttribute("id",objectId),
															new XAttribute("typeId",typeId)));
						connection.Close();
					}
					catch(Exception ex)
					{
						throw new DataManagerException("Object wasn't added",ex);
					}
				}
			}

			return result;
		}

		public void DeleteObjects(XDocument objects)
		{
			XElement root = objects.Element("objects");

			foreach (XElement xmlObject in root.Elements("object"))
			{
				string strTypeId = xmlObject.Attribute("typeId").Value;
				int typeId;
				if (!int.TryParse(strTypeId, out typeId))
				{
					throw new DataManagerException("Invalid typeId");
				}

				string strId = xmlObject.Attribute("Id").Value;
				int id;
				if (!int.TryParse(strId, out id))
				{
					throw new DataManagerException("Invalid id");
				}

				using (Domain.Interfaces.IRepository repository = new EntityFrameworkDAL.EFRepository())
				{
					Domain.Type type;
					try
					{
						type = repository.Types.Single(x => x.Id == typeId);
					}
					catch (Exception ex)
					{
						throw new DataManagerException("Type with id = {" + typeId + "} is not exists");
					}

					SqlConnection connection = new SqlConnection(type.Database.ServerConnectionString);

					StringBuilder query = new StringBuilder();
					query.AppendLine("USE [" + type.Database.DatabaseName + "]");
					query.AppendLine("DELETE FROM [" + type.Name + "]");
					query.AppendLine("WHERE");
					query.AppendLine("Id = " + id);
					try
					{
						connection.Open();
						SqlCommand command = new SqlCommand(query.ToString(), connection);
						command.ExecuteNonQuery();
						connection.Close();
					}
					catch (Exception ex)
					{
						throw new DataManagerException("Object wasn't deleted", ex);
					}
				}
			}
		}

		public void UpdateObjects(XDocument objects)
		{
			XElement root = objects.Element("objects");

			foreach (XElement xmlObject in root.Elements("object"))
			{
				string strTypeId = xmlObject.Attribute("typeId").Value;
				int typeId;
				if (!int.TryParse(strTypeId, out typeId))
				{
					throw new DataManagerException("Invalid typeId");
				}

				string strId = xmlObject.Attribute("id").Value;
				int id;
				if (!int.TryParse(strId, out id))
				{
					throw new DataManagerException("Invalid id");
				}

				using (Domain.Interfaces.IRepository repository = new EntityFrameworkDAL.EFRepository())
				{
					Domain.Type type;
					try
					{
						type = repository.Types.Single(x => x.Id == typeId);
					}
					catch (Exception ex)
					{
						throw new DataManagerException("Type with id = {" + typeId + "} is not exists");
					}

					SqlConnection connection = new SqlConnection(type.Database.ServerConnectionString);

					Domain.Property[] properties = type.Properties.Where(x => xmlObject.Elements().Any(y => string.Compare(y.Name.ToString(), x.Name, true) == 0)).ToArray();
					string[] values = new string[properties.Length];
					for (int propertyIndex = 0; propertyIndex < properties.Length; propertyIndex++)
					{
						values[propertyIndex] = properties[propertyIndex].Name + " = CONVERT(" + properties[propertyIndex].SqlType + ",'" + xmlObject.Element(properties[propertyIndex].Name).Value + "')";
					}
					string allValues = string.Join(",", values);
					StringBuilder query = new StringBuilder();
					query.AppendLine("USE [" + type.Database.DatabaseName + "]");
					query.AppendLine("UPDATE [" + type.Name + "] SET ");
					query.AppendLine(allValues);
					query.AppendLine("WHERE Id = " + id);
					try
					{
						connection.Open();
						SqlCommand command = new SqlCommand(query.ToString(), connection);
						command.ExecuteNonQuery();
						connection.Close();
					}
					catch (Exception ex)
					{
						throw new DataManagerException("Object wasn't updated", ex);
					}
				}
			}
		}

		public XDocument GetRelationshipsForObject(XDocument relationshipsDoc)
		{
			XDocument result;
			XElement root;
			SqlConnection connection;
			StringBuilder query;
			using(Domain.Interfaces.IRepository repository = new EntityFrameworkDAL.EFRepository())
			{
				try
				{
					XElement relationshipsRoot = relationshipsDoc.Element("relationships");
					int parentId = int.Parse(relationshipsRoot.Attribute("parentId").Value);
					int typeId = int.Parse(relationshipsRoot.Attribute("parentTypeId").Value);

					Domain.Type type = repository.Types.Single(x=>x.Id == typeId);

					result = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
					root = new XElement("relationships", new XAttribute("parentId", parentId),
																new XAttribute("parentTypeId", type.Id),
																new XAttribute("parentTypeName",type.Name));
					result.Add(root);
					connection = new SqlConnection(type.Database.ServerConnectionString);
					connection.Open();

					List<Domain.Relationship> relationships = repository.Relationships.Where(x => x.ParentType.Id == type.Id).ToList();
					foreach (Domain.Relationship relationship in relationships)
					{
						XElement xmlRelationship = new XElement("relationship", new XAttribute("name", relationship.Name),
																				new XAttribute("relationshipId", relationship.Id));
						root.Add(xmlRelationship);

						string columnNames = "ch.Id," + string.Join(",", relationship.ChildType.Properties.Select(x => "ch." + x.Name));
						query = new StringBuilder();
						query.AppendLine("USE [" + type.Database.DatabaseName + "]");
						query.Append("SELECT ");
						query.AppendLine(columnNames);
						query.AppendLine("FROM [" + relationship.Name + "] rel");
						query.AppendLine("INNER JOIN [" + relationship.ChildType.Name + "] ch");
						query.AppendLine("ON rel.Child = ch.Id");
						query.AppendLine("WHERE rel.Parent = " + parentId);

						SqlCommand command = new SqlCommand(query.ToString(), connection);
						SqlDataReader dataReader = command.ExecuteReader();
						while (dataReader.Read())
						{
							XElement xmlObject = new XElement("object");
							xmlObject.Add(new XAttribute("type", type.Name));
							xmlObject.Add(new XAttribute("id", dataReader["Id"].ToString()));
							xmlObject.Add(new XAttribute("typeId", relationship.ChildType.Id));
							foreach (Domain.Property property in relationship.ChildType.Properties)
							{
								XElement xmlProperty = new XElement(property.Name, dataReader[property.Name]);
								xmlObject.Add(xmlProperty);
							}
							xmlRelationship.Add(xmlObject);
						}

					}

					connection.Close();
				}
				catch (Exception ex)
				{
					throw new DataManagerException("Invalid operation", ex);
				}
				
			}


			return result;
		}

		public void AddRelationshipsForObject(XDocument relationshipsDoc)
		{
			using(Domain.Interfaces.IRepository repository = new EntityFrameworkDAL.EFRepository())
			{
				try
				{
					XElement root = relationshipsDoc.Element("relationships");
					int parentTypeId = int.Parse(root.Attribute("parentTypeId").Value);
					int parentId = int.Parse(root.Attribute("parentId").Value);

					Domain.Type parentType = repository.Types.Single(x => x.Id == parentTypeId);

					StringBuilder query = new StringBuilder();
					query.AppendLine("USE [" + parentType.Database.DatabaseName + "]");

					foreach (XElement xmlRelationship in root.Elements("relationship"))
					{
						int relationshipId = int.Parse(xmlRelationship.Attribute("relationshipId").Value);
						Domain.Relationship relationship = repository.Relationships.Single(x => x.Id == relationshipId);

						foreach(XElement xmlObject in xmlRelationship.Elements("object"))
						{
							int childId = int.Parse(xmlObject.Attribute("id").Value);
							query.AppendLine("INSERT INTO [" + relationship.Name + "]");
							query.AppendLine("(Parent, Child)");
							query.AppendLine("VALUES");
							query.AppendLine("("+ parentId +"," + childId + ")");
						}
					}
					SqlConnection connection = new SqlConnection(parentType.Database.ServerConnectionString);
					connection.Open();
					SqlCommand command = new SqlCommand(query.ToString(), connection);
					command.ExecuteNonQuery();
					connection.Close();
				}
				catch(Exception ex)
				{
					throw new DataManagerException("Relationship wasn't added",ex);
				}
			}
		}

		public void DeleteRelationshipsForObject(XDocument relationshipsDoc)
		{
			using (Domain.Interfaces.IRepository repository = new EntityFrameworkDAL.EFRepository())
			{
				try
				{
					XElement root = relationshipsDoc.Element("relationships");
					int parentTypeId = int.Parse(root.Attribute("parentTypeId").Value);
					int parentId = int.Parse(root.Attribute("parentId").Value);

					Domain.Type parentType = repository.Types.Single(x => x.Id == parentTypeId);

					StringBuilder query = new StringBuilder();
					query.AppendLine("USE [" + parentType.Database.DatabaseName + "]");

					foreach (XElement xmlRelationship in root.Elements("relationship"))
					{
						int relationshipId = int.Parse(xmlRelationship.Attribute("relationshipId").Value);
						Domain.Relationship relationship = repository.Relationships.Single(x => x.Id == relationshipId);

						foreach (XElement xmlObject in xmlRelationship.Elements("object"))
						{
							int childId = int.Parse(xmlObject.Attribute("id").Value);
							query.AppendLine("DELETE FROM [" + relationship.Name + "]");
							query.AppendLine("WHERE");
							query.AppendLine("Parent = " + parentId + " AND Child = " + childId);
						}
					}
					SqlConnection connection = new SqlConnection(parentType.Database.ServerConnectionString);
					connection.Open();
					SqlCommand command = new SqlCommand(query.ToString(), connection);
					command.ExecuteNonQuery();
					connection.Close();
				}
				catch (Exception ex)
				{
					throw new DataManagerException("Relationship wasn't deleted", ex);
				}
			}
		}
	}
}
