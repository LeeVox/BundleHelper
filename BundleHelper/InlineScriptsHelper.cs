using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace BundleHelper
{
    public class BundleHelperAreaRegistration : AreaRegistration
    {
        private BundleHelperController[] controllers = new BundleHelperController[]{
            new InlineScriptsController()
        };

        public override string AreaName
        {
            get
            {
                return "BundleHelper";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            foreach (var controller in controllers)
            {
                context.MapRoute( controller.RouteName, controller.URL, controller.Defaults );
            }
        }
    }

    public abstract class BundleHelperController : Controller
    {
        // just provide any unique random string for key
        private static readonly string RAWKEY_COMPACTKEY = "BundlerHelper_InlineScriptsHelper_RAWKEY_COMPACTKEY__" + DateTime.Now.ToString();
        private static readonly string COMPACTKEY_CONTENT = "BundlerHelper_InlineScriptsHelper_COMPACTKEY_CONTENT__" + DateTime.Now.ToString();

        private static readonly Regex rgxRemoveSpaces = new Regex(@"\s+");
        private static readonly Regex rgxRemoveTag = new Regex(@"\s*<\s*script.*?>\s*(?<content>.+)\s*</\s*script\s*>\s*");

        internal string RouteName { get; set; }
        internal string URL { get; set; }
        internal object Defaults { get; set; }

        internal BundleHelperController(string routeName, string controllerName)
        {
            this.RouteName = routeName;
            this.URL = "BundleHelper/" + controllerName + "/{id}";
            this.Defaults = new { controller = controllerName, action = "Get", id = UrlParameter.Optional };
        }

        private static Dictionary<TKey, TValue> GetDictionary<TKey, TValue>(HttpContextBase context, string key)
        {
            if (context.Application[key] == null)
            {
                context.Application[key] = new Dictionary<TKey, TValue>();
            }
            return context.Application[key] as Dictionary<TKey, TValue>;
        }

        protected static int _CompactScrip(HtmlHelper helper, string rawScript)
        {
            var rawKey_CompactKey = GetDictionary<int, int>(helper.ViewContext.HttpContext, RAWKEY_COMPACTKEY);
            var compactKey_Content = GetDictionary<int, string>(helper.ViewContext.HttpContext, COMPACTKEY_CONTENT);

            int id;

            var rawKey = rawScript.GetHashCode();
            if (rawKey_CompactKey.ContainsKey(rawKey))
            {
                id = rawKey_CompactKey[rawKey];
            }
            else
            {
                var removedSpaces = rgxRemoveSpaces.Replace(rawScript, " ");
                var compactScript = rgxRemoveTag.Match(removedSpaces).Groups[1].Value;

                int compactKey = compactScript.GetHashCode();
                if (!compactKey_Content.ContainsKey(compactKey))
                {
                    compactKey_Content.Add(compactKey, compactScript);
                    rawKey_CompactKey.Add(rawKey, compactKey);
                }

                id = compactKey;
            }

            return id;
        }

        public JavaScriptResult Get(int? id)
        {
            var compactKey_Content = GetDictionary<int, string>(HttpContext, COMPACTKEY_CONTENT);

            var content = string.Empty;
            if (id != null && compactKey_Content.ContainsKey(id.Value))
            {
                content = compactKey_Content[id.Value];
            }
            return JavaScript(content);
        }
    }

    public class InlineScriptsController : BundleHelperController
    {
        public InlineScriptsController()
            : base("BundleHelper_default", "InlineScripts")
        {
        }

        public static string CompactScript(HtmlHelper helper, string script)
        {
            var id = _CompactScrip(helper, script);
            return "<script src='BundleHelper/InlineScripts/" + id + "'></script>";
        }
    }
}
