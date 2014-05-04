using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CoreManager;
using System.Xml.Linq;

namespace WebDbCreator.Controllers
{
	public class DataController : Controller
	{
		//
		// GET: /Data/

		public ActionResult Main(int databaseId)
		{
			using(Domain.Interfaces.IRepository repository = new EntityFrameworkDAL.EFRepository())
			{
				return View("Main", repository.Databases.Single(x=>x.Id == databaseId));
			}
		}

		public ActionResult Types(int databaseId)
		{
			using(Domain.Interfaces.IRepository repository = new EntityFrameworkDAL.EFRepository())
			{
				return View("TypesMenu", repository.Types.Where(x=>x.Database.Id == databaseId).ToList());
			}
		}

		public ActionResult TypeContent(int typeId)
		{
			XDocument request = new XDocument();
			XElement root = new XElement("objects");
			XElement obj = new XElement("object", new XAttribute("typeId", typeId));
			root.Add(obj);
			request.Add(root);
			
			DataManager manager = new DataManager();
			XDocument response = manager.GetObjectsByType(request);

			using(Domain.Interfaces.IRepository repository = new EntityFrameworkDAL.EFRepository())
			{
				return View("TypeContent",
					new Tuple<IEnumerable<Domain.Property>,XDocument>(repository.Types.Single(x=>x.Id == typeId).Properties.ToList(),
																	response));
			}
		}

		//[HttpGet]
		//public ActionResult EditObject(int typeId, int objectId)
		//{
		//	XDocument request = new XDocument();
		//	XElement root = new XElement("objects");
		//	request.Add(root);
		//	XElement obj = new XElement("object", new XAttribute("typeId",typeId),
		//											new XAttribute("id",objectId));
		//	root.Add(obj);
		//	DataManager manager = new DataManager();
		//	XDocument response = manager.GetObjectByTypeAndId(request);

		//	return View(response);
		//}

		public ActionResult AddObject(int typeId)
		{
			using(Domain.Interfaces.IRepository repository = new EntityFrameworkDAL.EFRepository())
			{
				Domain.Type type = repository.Types.Single(x => x.Id == typeId);
				type.Properties = type.Properties.ToList();
				return View(type);
			}
		}

	}
}
