using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseGenerator
{
	public class DatabaseGenerator
	{
		private SqlConnection _connection;
		public DatabaseGenerator(string connectionString)
		{
			_connection = new SqlConnection(connectionString);
		}

		public void OpenConnection()
		{
			_connection.Open();
		}

		public void CloseConnection()
		{
			_connection.Close();
		}

		public void CreateDatabase(Domain.Database database)
		{
			string query = "CREATE DATABASE [" + database.DatabaseName + "]";
			SqlCommand command = new SqlCommand(query, _connection);
			command.ExecuteNonQuery();
		}

		public void DropDatabase(Domain.Database database)
		{
			string query = "DROP DATABASE [" + database.DatabaseName + "]";
			SqlCommand command = new SqlCommand(query, _connection);
			command.ExecuteNonQuery();
		}

		public void CreateType(Domain.Type type)
		{
			StringBuilder query = new StringBuilder();
			query.AppendLine("USE [" + type.Database.DatabaseName + "]");
			query.AppendLine("CREATE TABLE [" + type.Name + "](");
			int propertiesCompletedCount = 0;
			foreach(Domain.Property property in type.Properies)
			{
				query.Append("[" + property.Name + "] ");
				query.Append(property.SqlType + " ");
				if (property.IsReqired)
				{
					query.Append("NOT NULL");
				}
				else
				{
					query.Append("NULL");
				}
				propertiesCompletedCount++;
				query.Append(",");
			}
			query.AppendLine("[id] [int] IDENTITY(1,1) NOT NULL,");
			query.AppendLine("CONSTRAINT [PK_" + type.Name + "] PRIMARY KEY CLUSTERED");
			query.AppendLine("( [id] ASC ) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]");
			query.AppendLine(") ON [PRIMARY]");

			SqlCommand command = new SqlCommand(query.ToString(), _connection);
			command.ExecuteNonQuery();

			foreach (Domain.Property property in type.Properies)
			{
				if (property.IsIndexed)
				{
					this.CreateIndex(property);
				}
			}
		}

		public void DropType(Domain.Type type)
		{
			StringBuilder query = new StringBuilder();
			query.AppendLine("USE [" + type.Database.DatabaseName + "]");
			query.AppendLine("DROP TABLE [" + type.Name + "]");
			SqlCommand command = new SqlCommand(query.ToString(), _connection);
			command.ExecuteNonQuery();
		}

		public void CreateIndex(Domain.Property property)
		{
			StringBuilder query = new StringBuilder();
			query.AppendLine("USE [" + property.Type.Database.DatabaseName + "]");
			query.AppendLine("CREATE NONCLUSTERED INDEX [Index_" + property.Type.Name + "_" + property.Name + "] ON [" + property.Type.Name + "]");
			query.AppendLine("(");
			query.AppendLine("[" + property.Name + "] ASC");
			query.AppendLine(")");
			SqlCommand command = new SqlCommand(query.ToString(), _connection);
			command.ExecuteNonQuery();
		}

		public void DropIndex(Domain.Property property)
		{
			StringBuilder query = new StringBuilder();
			query.AppendLine("USE [" + property.Type.Database.DatabaseName + "]");
			query.AppendLine("DROP INDEX INDEX [Index_" + property.Type.Name + "_" + property.Name + "] ON [" + property.Type.Name + "]");
			SqlCommand command = new SqlCommand(query.ToString(), _connection);
			command.ExecuteNonQuery();
		}

		public void CreateRelationship(Domain.Relationship relationship)
		{
			StringBuilder query = new StringBuilder();
			query.AppendLine("USE [" + relationship.Database.DatabaseName + "]");
			query.AppendLine("CREATE TABLE [" + relationship.Name + "](");
			query.AppendLine("[Parent] [int] NOT NULL,");
			query.AppendLine("[Child] [int] NOT NULL,");
			query.AppendLine(") ON [PRIMARY]");
			query.AppendLine("ALTER TABLE [" + relationship.Name + "]  WITH CHECK ADD  CONSTRAINT [FK_Parent_" + relationship.Name + "_" + relationship.ParentType.Name + "] FOREIGN KEY([Parent])");
			query.AppendLine("REFERENCES [" + relationship.ParentType.Name + "] ([id])");
			query.AppendLine("ALTER TABLE [" + relationship.Name + "] CHECK CONSTRAINT [FK_" + relationship.Name + "_" + relationship.ParentType.Name + "]");
			query.AppendLine("ALTER TABLE [" + relationship.Name + "]  WITH CHECK ADD  CONSTRAINT [FK_Child_" + relationship.Name + "_" + relationship.ChildType.Name + "] FOREIGN KEY([Child])");
			query.AppendLine("REFERENCES [" + relationship.ChildType.Name + "] ([id])");
			query.AppendLine("ALTER TABLE [" + relationship.Name + "] CHECK CONSTRAINT [FK_" + relationship.Name + "_" + relationship.ChildType.Name + "]");
			query.AppendLine("CREATE NONCLUSTERED INDEX [Index_" + relationship.Name + "_Parent] ON [" + relationship.Name + "]");
			query.AppendLine("( [Parent] ASC )");
			SqlCommand command = new SqlCommand(query.ToString(), _connection);
			command.ExecuteNonQuery();
		}

		public void DropRelationship(Domain.Relationship relationship)
		{
			StringBuilder query = new StringBuilder();
			query.AppendLine("USE [" + relationship.Database.DatabaseName + "]");
			query.AppendLine("DROP TABLE [" + relationship.Name + "]");
			SqlCommand command = new SqlCommand(query.ToString(), _connection);
			command.ExecuteNonQuery();
		}
	}
}
