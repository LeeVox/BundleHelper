using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BundleHelper_Test.Models;

namespace BundleHelper_Test.Controllers
{

    public class BookController : Controller
    {
        public ActionResult Index()
        {
            var model = GetSampleModel();
            return View(model);
        }

        public ActionResult Detail(int id)
        {
            var model = GetSampleModel().FirstOrDefault(x => x.ID == id);
            return View(model);
        }

        [HttpPost]
        public JsonResult CheckStock(int id)
        {
            var ret = id*10;
            return Json(ret);
        }

        private IEnumerable<Book> GetSampleModel()
        {
            List<Book> model = new List<Book>();

            model.Add(new Book() { ID = 1, Title = "Book 1", Author = "Author A", PublishDate = new DateTime(2013, 1, 1) });
            model.Add(new Book() { ID = 2, Title = "Book 2", Author = "Author B", PublishDate = new DateTime(2013, 2, 2) });
            model.Add(new Book() { ID = 3, Title = "Book 3", Author = "Author C", PublishDate = new DateTime(2013, 3, 3) });
            model.Add(new Book() { ID = 4, Title = "Book 4", Author = "Author D", PublishDate = new DateTime(2013, 4, 4) });
            model.Add(new Book() { ID = 5, Title = "Book 5", Author = "Author E", PublishDate = new DateTime(2013, 5, 5) });

            return model;
        }
    }
}
