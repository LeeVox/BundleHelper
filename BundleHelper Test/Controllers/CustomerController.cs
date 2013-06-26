using System;
using System.Web.Mvc;
using BundleHelper_Test.Models;

namespace BundleHelper_Test.Controllers
{

    /*
     * All views in customer controller use those css and scripts
     */
    [UsingHeadScripts("~/Scripts/Customer/user.js")]
    [UsingStyles("~/Content/CurrentUser.css", "~/Content/Customer.css")]
    public class CustomerController : Controller
    {
        public static Customer CurrentUser
        {
            get { return GetSampleCustomer(); }
        }

        public ActionResult Index()
        {
            return View();
        }

        [UsingBodyScripts("~/Scripts/customer/action1.js")]
        public ActionResult Action1()
        {
            return View();
        }

        public ActionResult Action2()
        {
            return View();
        }

        [UsingBodyScripts("~/SCRIPTS/CuSTomer/uSEr.js", "~/Scripts/customer/action3.js")]
        [UsingStyles("~/Content/action3.css")]
        public ActionResult Action3()
        {
            /*
             * user.js is duplicated (because CustomerController already included it) and will be removed
             *      (BundleHelper ignore character case while checking for duplicated files)
             * action3.js will be added to body tag
             */
            return View();
        }

        private static Customer GetSampleCustomer()
        {
            return new Customer()
            {
                Name = "Dat Le",
                Birthday = new DateTime(1989, 7, 14),
                CurrentCast = 5000.0f,
                Email = "admin@lethanhdat.com",
                Password = "123456"
            };
        }

    }
}
