using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CoreManager;

namespace WebDbCreator.Controllers
{
	public class MetadataController : Controller
	{
		//
		// GET: /Metadata/

		public ActionResult Databases()
		{
			using(Domain.Interfaces.IRepository repository = new EntityFrameworkDAL.EFRepository())
			{
				return View("Databases",repository.Databases.ToList());
			}
		}

		public ActionResult DeleteDatabase(int databaseId)
		{
			DatabaseManager manager = new DatabaseManager();
			manager.DeleteDatabase(databaseId);
			return RedirectToAction("Index", "Main");
		}

		[HttpGet]
		public ActionResult CreateDatabase()
		{
			return View("Database",new Domain.Database());
		}

		[HttpPost]
		public ActionResult CreateDatabase(Domain.Database database)
		{
			DatabaseManager manager = new DatabaseManager();
			manager.CreateDatabase(database);
			return RedirectToAction("Index", "Main");
		}

	}
}
