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
		public XDocument GetObjectsByType(Domain.Type type)
		{
			XDocument result = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
			XElement root = new XElement("objects");
			result.Add(root);
			SqlConnection connection = new SqlConnection(type.Database.ServerConnectionString);

			string columnNames = "Id," + string.Join(",", type.Properies.Select(x => x.Name));
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
					foreach(Domain.Property property in type.Properies)
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

			return result;
		}

		public void AddObjects(XDocument objects)
		{
			XElement root = objects.Element("objects");

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

					Domain.Property[] properties = type.Properies.Where(x => xmlObject.Elements().Any(y => string.Compare(y.Name.ToString(), x.Name, true) == 0)).ToArray();
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
					query.AppendLine("VALUES");
					query.AppendLine("(");
					query.AppendLine(allValues);
					query.AppendLine(")");
					try
					{
						connection.Open();
						SqlCommand command = new SqlCommand(query.ToString(), connection);
						command.ExecuteNonQuery();
						connection.Close();
					}
					catch(Exception ex)
					{
						throw new DataManagerException("Object wasn't added",ex);
					}
				}
			}
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

					Domain.Property[] properties = type.Properies.Where(x => xmlObject.Elements().Any(y => string.Compare(y.Name.ToString(), x.Name, true) == 0)).ToArray();
					string[] values = new string[properties.Length];
					for (int propertyIndex = 0; propertyIndex < properties.Length; propertyIndex++)
					{
						values[propertyIndex] = "SET " + properties[propertyIndex].Name + " = CONVERT(" + properties[propertyIndex].SqlType + ",'" + xmlObject.Element(properties[propertyIndex].Name).Value + "')";
					}
					string allValues = string.Join(",", values);
					StringBuilder query = new StringBuilder();
					query.AppendLine("USE [" + type.Database.DatabaseName + "]");
					query.AppendLine("UPDATE [" + type.Name + "]");
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
	}
}
