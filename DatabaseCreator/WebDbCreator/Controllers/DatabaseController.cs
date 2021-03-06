﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CoreManager;
using System.Text.RegularExpressions;

namespace WebDbCreator.Controllers
{
	public class DatabaseController : Controller
	{
		//
		// GET: /Database/

		public ActionResult Metadata(int databaseId)
		{
			return View("Metadata",databaseId);
		}

		public ActionResult Types(int databaseId)
		{
			using (Domain.Interfaces.IRepository repository = new EntityFrameworkDAL.EFRepository())
			{
				return View("Types", repository.Types.Where(x => x.Database.Id == databaseId).ToList());
			}
		}

		public ActionResult DeleteType(int typeId)
		{
			int databaseId;
			using (Domain.Interfaces.IRepository repository = new EntityFrameworkDAL.EFRepository())
			{
				databaseId = repository.Types.Single(x => x.Id == typeId).Database.Id;
			}
			DatabaseManager manager = new DatabaseManager();
			manager.DeleteType(typeId);
			return Metadata(databaseId);
		}

		[HttpGet]
		public ActionResult CreateType()
		{
			Domain.Type type = new Domain.Type();
			return View("Type",type);
		}

		[HttpPost]
		public ActionResult CreateType(Domain.Type type, int databaseId)
		{
			type.Properties = type.Properties.Select(x => { x.Type = type; return x; }).ToList();
			using(Domain.Interfaces.IRepository repository = new EntityFrameworkDAL.EFRepository())
			{
				Domain.Database database = repository.Databases.Single(x=>x.Id == databaseId);
				type.Database = database;
				DatabaseManager manager = new DatabaseManager();
				manager.CreateType(type);
			}

			return Metadata(databaseId);
		}

		public ActionResult Relationships(int databaseId)
		{
			using(Domain.Interfaces.IRepository repository = new EntityFrameworkDAL.EFRepository())
			{
				return View("Relationships", repository.Relationships
					.Where(x => x.Database.Id == databaseId)
					.ToList()
					.Select(x => new Domain.Relationship()
					{
						Id = x.Id,
						ParentType = x.ParentType,
						ChildType = x.ChildType,
						Name = x.Name,
						Database = x.Database,
						Created = x.Created,
						Modified = x.Modified
					}).ToList());
			}
		}

		[HttpGet]
		public ActionResult CreateRelationship()
		{
			Domain.Relationship relationship = new Domain.Relationship();
			return View("Relationship", relationship);
		}

		[HttpPost]
		public ActionResult CreateRelationship(Domain.Relationship relationship, int databaseId)
		{
			int parentId = int.Parse(Request["parent"]);
			int childId = int.Parse(Request["child"]);

			using(Domain.Interfaces.IRepository repository = new EntityFrameworkDAL.EFRepository())
			{
				relationship.ParentType = repository.Types.Single(x=>x.Id == parentId);
				relationship.ChildType = repository.Types.Single(x => x.Id == childId);
				relationship.Database = repository.Databases.Single(x => x.Id == databaseId);
				DatabaseManager manager = new DatabaseManager();
				manager.CreateRelationship(relationship);
			}

			return Metadata(databaseId);
		}

		public ActionResult DeleteRelationship(int relationshipId)
		{
			int databaseId;
			using (Domain.Interfaces.IRepository repository = new EntityFrameworkDAL.EFRepository())
			{
				databaseId = repository.Relationships.Single(x => x.Id == relationshipId).Database.Id;
			}
			DatabaseManager manager = new DatabaseManager();
			manager.DeleteRelationship(relationshipId);
			return Metadata(databaseId);
		}
	}
}
