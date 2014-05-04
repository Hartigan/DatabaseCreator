using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebDbCreator.Controllers
{
	public class TypeController : Controller
	{
		//
		// GET: /Type/

		public ActionResult Metadata(int typeId)
		{
			using(Domain.Interfaces.IRepository repository = new EntityFrameworkDAL.EFRepository())
			{
				Domain.Type type = repository.Types.Single(x=>x.Id == typeId);
				type.Database = type.Database;
				type.Properties = type.Properties.ToList();
				return View("Metadata",type);
			}
		}

	}
}
