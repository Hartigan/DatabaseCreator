using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CoreManager;
using System.Xml.Linq;
using System.Text.RegularExpressions;

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

		public ActionResult TypesMenu(int databaseId)
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
				Domain.Type type = repository.Types.Single(x=>x.Id == typeId);
				type.Properties = type.Properties.ToList();
				return View("TypeContent",
					new Tuple<Domain.Type,XDocument>(type, response));
			}
		}

		public ActionResult AddObject(int typeId)
		{
			if (Request.HttpMethod == "GET")
			{
				using(Domain.Interfaces.IRepository repository = new EntityFrameworkDAL.EFRepository())
				{
					Domain.Type type = repository.Types.Single(x => x.Id == typeId);
					type.Properties = type.Properties.ToList();
					return View(type);
				}
			}
			else if (Request.HttpMethod == "POST")
			{
				IEnumerable<string> objectKeys = Request.Form.AllKeys.Where(x => x.Contains("object_"));
				Regex propertyNameEx = new Regex(@"(?<=object_).*");

				XDocument request = new XDocument();
				XElement root = new XElement("objects");
				XElement obj = new XElement("object", new XAttribute("typeId", typeId));
				root.Add(obj);
				request.Add(root);
				foreach (string key in objectKeys)
				{
					string propertyName = propertyNameEx.Match(key).Value;
					obj.Add(new XElement(propertyName, Request[key]));
				}

				DataManager manager = new DataManager();
				XDocument response = manager.AddObjects(request);
				string newObjId = response.Element("objects").Elements("object").First().Attribute("id").Value;

				request = new XDocument();
				root = new XElement("relationships", new XAttribute("parentId", newObjId),
													new XAttribute("parentTypeId",typeId));
				request.Add(root);
				IEnumerable<string> relationshipsKeys = Request.Form.AllKeys.Where(x=>x.Contains("relationship_"));
				Regex getRelId = new Regex(@"(?<=relationship_)[0-9]+(?=_[0-9]+)");
				Regex getObjId = new Regex(@"(?<=relationship_[0-9]+_)[0-9]+");
				foreach(string key in relationshipsKeys)
				{
					string relId = getRelId.Match(key).Value;
					string objId = getObjId.Match(key).Value;
					IEnumerable<XElement> relationships = root.Elements("relationship").Where(x=> string.Compare(x.Attribute("relationshipId").Value,relId,true) == 0);
					XElement relationship;
					if (relationships.Count() == 0)
					{
						relationship = new XElement("relationship", new XAttribute("relationshipId",relId));
						root.Add(relationship);
					}
					else
					{
						relationship = relationships.First();
					}
					relationship.Add(new XElement("object", new XAttribute("id",objId)));
				}
				manager.AddRelationshipsForObject(request);
				using (Domain.Interfaces.IRepository repository = new EntityFrameworkDAL.EFRepository())
				{
					Domain.Type type = repository.Types.Single(x => x.Id == typeId);
					return Main(type.Database.Id);
				}
			}
			return RedirectToAction("Index", "Main");
		}

		public ActionResult ViewObject(int typeId, int objId)
		{
			XDocument request = new XDocument();
			XElement root = new XElement("objects");
			XElement obj = new XElement("object", new XAttribute("typeId", typeId),
													new XAttribute("id",objId));
			root.Add(obj);
			request.Add(root);

			DataManager manager = new DataManager();
			XDocument response = manager.GetObjectByTypeAndId(request);
			XDocument result = new XDocument();
			XElement rootResult = new XElement("content");
			result.Add(rootResult);
			rootResult.Add(response.Element("objects"));
			request = new XDocument();
			root = new XElement("relationships", new XAttribute("parentId", objId),
													new XAttribute("parentTypeId",typeId));
			request.Add(root);
			response = manager.GetRelationshipsForObject(request);
			rootResult.Add(response.Element("relationships"));


			Tuple<Domain.Type,IEnumerable<Domain.Relationship>,XDocument> model;
			using(Domain.Interfaces.IRepository repository = new EntityFrameworkDAL.EFRepository())
			{
				Domain.Type type = repository.Types.Single(x=>x.Id == typeId);
				type.Properties = type.Properties.ToList();
				IEnumerable<Domain.Relationship> relationships = repository.Relationships
															.Where(x => x.ParentType.Id == typeId)
															.ToList()
															.Select(x =>
															{
																return new Domain.Relationship()
																{
																	Id = x.Id,
																	ParentType = new Domain.Type()
																	{
																		Id = x.ParentType.Id,
																		Created = x.ParentType.Created,
																		Modified = x.ParentType.Modified,
																		Name = x.ParentType.Name,
																		Properties = x.ParentType.Properties.ToList()
																	},
																	ChildType = new Domain.Type()
																	{
																		Id = x.ChildType.Id,
																		Created = x.ChildType.Created,
																		Modified = x.ChildType.Modified,
																		Name = x.ChildType.Name,
																		Properties = x.ChildType.Properties.ToList()
																	},
																	Created = x.Created,
																	Modified = x.Modified,
																	Name = x.Name
																};
															}).ToList();
				model = new Tuple<Domain.Type, IEnumerable<Domain.Relationship>, XDocument>(type, relationships, result);
			}

			return View("ViewObject",model);
		}

		public ActionResult RelationshipsToBind(int typeId)
		{
			using(Domain.Interfaces.IRepository repository = new EntityFrameworkDAL.EFRepository())
			{
				List<Domain.Relationship> relationships = repository.Relationships
															.Where(x=>x.ParentType.Id == typeId)
															.ToList()
															.Select(x=> 
															{
																return new Domain.Relationship()
																{
																	Id = x.Id,
																	ParentType = new Domain.Type()
																	{
																		Id = x.ParentType.Id,
																		Created = x.ParentType.Created,
																		Modified = x.ParentType.Modified,
																		Name = x.ParentType.Name,
																		Properties = x.ParentType.Properties.ToList()
																	},
																	ChildType = new Domain.Type()
																	{
																		Id = x.ChildType.Id,
																		Created = x.ChildType.Created,
																		Modified = x.ChildType.Modified,
																		Name = x.ChildType.Name,
																		Properties = x.ChildType.Properties.ToList()
																	},
																	Created = x.Created,
																	Modified = x.Modified,
																	Name = x.Name
																};
															}).ToList();
				DataManager manager = new DataManager();
				List<Tuple<Domain.Relationship, XDocument>> model = new List<Tuple<Domain.Relationship, XDocument>>();
				foreach(Domain.Relationship relationship in relationships)
				{
					XDocument request = new XDocument();
					XElement root = new XElement("objects");
					XElement obj = new XElement("object", new XAttribute("typeId", relationship.ChildType.Id));
					root.Add(obj);
					request.Add(root);
					XDocument response = manager.GetObjectsByType(request);
					model.Add(new Tuple<Domain.Relationship, XDocument>(relationship, response));
				}
				return View("RelationshipsToBind", model);
			}
		}

		public ActionResult EditObject(int typeId, int objId)
		{
			if (Request.HttpMethod == "GET")
			{
				XDocument request = new XDocument();
				XElement root = new XElement("objects");
				XElement obj = new XElement("object", new XAttribute("typeId", typeId),
														new XAttribute("id", objId));
				root.Add(obj);
				request.Add(root);

				DataManager manager = new DataManager();
				XDocument response = manager.GetObjectByTypeAndId(request);
				XDocument result = new XDocument();
				XElement rootResult = new XElement("content");
				result.Add(rootResult);
				rootResult.Add(response.Element("objects"));
				request = new XDocument();
				root = new XElement("relationships", new XAttribute("parentId", objId),
														new XAttribute("parentTypeId", typeId));
				request.Add(root);
				response = manager.GetRelationshipsForObject(request);
				rootResult.Add(response.Element("relationships"));


				Tuple<Domain.Type, IEnumerable<Tuple<Domain.Relationship,XDocument>>, XDocument> model;
				using (Domain.Interfaces.IRepository repository = new EntityFrameworkDAL.EFRepository())
				{
					Domain.Type type = repository.Types.Single(x => x.Id == typeId);
					type.Properties = type.Properties.ToList();
					IEnumerable<Domain.Relationship> relationships = repository.Relationships
																.Where(x => x.ParentType.Id == typeId)
																.ToList()
																.Select(x =>
																{
																	return new Domain.Relationship()
																	{
																		Id = x.Id,
																		ParentType = new Domain.Type()
																		{
																			Id = x.ParentType.Id,
																			Created = x.ParentType.Created,
																			Modified = x.ParentType.Modified,
																			Name = x.ParentType.Name,
																			Properties = x.ParentType.Properties.ToList()
																		},
																		ChildType = new Domain.Type()
																		{
																			Id = x.ChildType.Id,
																			Created = x.ChildType.Created,
																			Modified = x.ChildType.Modified,
																			Name = x.ChildType.Name,
																			Properties = x.ChildType.Properties.ToList()
																		},
																		Created = x.Created,
																		Modified = x.Modified,
																		Name = x.Name
																	};
																}).ToList();

					List<Tuple<Domain.Relationship, XDocument>> relsWithObjects = new List<Tuple<Domain.Relationship, XDocument>>();
					foreach (Domain.Relationship relationship in relationships)
					{
						XDocument rqs = new XDocument();
						XElement rt = new XElement("objects");
						XElement o = new XElement("object", new XAttribute("typeId", relationship.ChildType.Id));
						rt.Add(o);
						rqs.Add(rt);
						XDocument rsp = manager.GetObjectsByType(rqs);
						relsWithObjects.Add(new Tuple<Domain.Relationship, XDocument>(relationship, rsp));
					}
					model = new Tuple<Domain.Type, IEnumerable<Tuple<Domain.Relationship, XDocument>>, XDocument>(type, relsWithObjects, result);
					return View("EditObject", model);
				}
			}
			else if (Request.HttpMethod == "POST")
			{
				IEnumerable<string> objectKeys = Request.Form.AllKeys.Where(x => x.Contains("object_"));
				Regex propertyNameEx = new Regex(@"(?<=object_).*");

				XDocument request = new XDocument();
				XElement root = new XElement("objects");
				XElement obj = new XElement("object", new XAttribute("typeId", typeId), new XAttribute("id",objId));
				root.Add(obj);
				request.Add(root);
				foreach (string key in objectKeys)
				{
					string propertyName = propertyNameEx.Match(key).Value;
					obj.Add(new XElement(propertyName, Request[key]));
				}
				using (Domain.Interfaces.IRepository repository = new EntityFrameworkDAL.EFRepository())
				{
					Domain.Type type = repository.Types.Single(x=>x.Id == typeId);
					IEnumerable<Domain.Property> boolProps = type.Properties.Where(x => x.SqlType.ToLower() == "bit");
					foreach(Domain.Property property in boolProps)
					{
						if (obj.Element(property.Name) == null)
						{
							obj.Add(new XElement(property.Name,"false"));
						}
					}
				}

				DataManager manager = new DataManager();
				XDocument response;
				manager.UpdateObjects(request);

				request = new XDocument();
				root = new XElement("relationships", new XAttribute("parentId", objId),
													new XAttribute("parentTypeId", typeId));
				request.Add(root);
				IEnumerable<string> relationshipsKeys = Request.Form.AllKeys.Where(x => x.Contains("relationship_"));
				Regex getRelId = new Regex(@"(?<=relationship_)[0-9]+(?=_[0-9]+)");
				Regex getObjId = new Regex(@"(?<=relationship_[0-9]+_)[0-9]+");

				XDocument rqst = new XDocument();
				XElement rqstRoot = new XElement("relationships", new XAttribute("parentId", objId),
										new XAttribute("parentTypeId", typeId));
				rqst.Add(rqstRoot);
				XDocument rsps = manager.GetRelationshipsForObject(request);

				IEnumerable<XElement> existsRels = rsps.Element("relationships").Elements("relationship");
				List<string> existsObjs = new List<string>();
				foreach(XElement rel in existsRels)
				{
					foreach(XElement xobj in rel.Elements("object"))
					{
						existsObjs.Add(rel.Attribute("relationshipId").Value + "_" + xobj.Attribute("id").Value);
					}
				}

				foreach (string key in relationshipsKeys)
				{
					string relId = getRelId.Match(key).Value;
					string relObjId = getObjId.Match(key).Value;
					string hash = relId + "_" + relObjId;
					if (existsObjs.Any(x=>x == hash))
					{
						existsObjs.Remove(hash);
					}
					else
					{
						IEnumerable<XElement> relationships = root.Elements("relationship").Where(x => string.Compare(x.Attribute("relationshipId").Value, relId, true) == 0);
						XElement relationship;
						if (relationships.Count() == 0)
						{
							relationship = new XElement("relationship", new XAttribute("relationshipId", relId));
							root.Add(relationship);
						}
						else
						{
							relationship = relationships.First();
						}
						relationship.Add(new XElement("object", new XAttribute("id", relObjId)));
					}
				}
				manager.AddRelationshipsForObject(request);

				request = new XDocument();
				root = new XElement("relationships", new XAttribute("parentId",objId),
													new XAttribute("parentTypeId", typeId));
				request.Add(root);

				foreach(string relToDelete in existsObjs)
				{
					string[] ids = relToDelete.Split('_');
					string relId = ids[0];
					string chId = ids[1];
					IEnumerable<XElement> relationships = root.Elements("relationship").Where(x => string.Compare(x.Attribute("relationshipId").Value, relId, true) == 0);
					XElement relationship;
					if (relationships.Count() == 0)
					{
						relationship = new XElement("relationship", new XAttribute("relationshipId", relId));
						root.Add(relationship);
					}
					else
					{
						relationship = relationships.First();
					}
					relationship.Add(new XElement("object", new XAttribute("id", chId)));
				}

				manager.DeleteRelationshipsForObject(request);

				using (Domain.Interfaces.IRepository repository = new EntityFrameworkDAL.EFRepository())
				{
					Domain.Type type = repository.Types.Single(x => x.Id == typeId);
					return Main(type.Database.Id);
				}
			}

			return RedirectToAction("Index", "Main");
		}

	}
}
