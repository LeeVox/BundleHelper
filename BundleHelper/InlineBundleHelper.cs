using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.WebPages;
using BundleHelper;

namespace BundleHelper
{
    public class BundleHelperAreaRegistration : AreaRegistration
    {
        internal static readonly string VERSION = "v" + Assembly.GetExecutingAssembly().GetName().Version.ToString();
        internal static readonly string URL_PATTERN = "BundleHelper/" + VERSION + "/{controller}/{action}/{id}";

        public override string AreaName
        {
            get
            {
                return "BundleHelperArea";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            var x = context.Routes;
            context.MapRoute(
                "BundleHelper_route",
                URL_PATTERN
            );
        }
    }

    public class InlineBundleController : Controller
    {
        internal static HttpStatusCodeResult HTTP_304_NOT_MODIFIED_RESULT = new HttpStatusCodeResult(304);

        internal static readonly string SCRIPT_LINK = "~/" + BundleHelperAreaRegistration.URL_PATTERN.Replace("{controller}", "InlineBundle").Replace("{action}", "Script").Replace("{id}", string.Empty);
        internal static readonly string STYLESHEET_LINK = "~/" + BundleHelperAreaRegistration.URL_PATTERN.Replace("{controller}", "InlineBundle").Replace("{action}", "Stylesheet").Replace("{id}", string.Empty);

        public ActionResult Script(int? id)
        {
            var ret = System.Web.Mvc.BundleHelper.GetInlineScript(HttpContext, id);
            if (ret == null)
                return HTTP_304_NOT_MODIFIED_RESULT;
            else
                return JavaScript(ret);
        }

        public ActionResult Stylesheet(int? id)
        {
            var ret = System.Web.Mvc.BundleHelper.GetInlineStylesheet(HttpContext, id);
            if (ret == null)
                return HTTP_304_NOT_MODIFIED_RESULT;
            else
                return new ContentResult() { Content = ret, ContentType = "text/css" };
        }
    }
}


namespace System.Web.Mvc
{
    public static partial class BundleHelper
    {
        private static readonly bool IsDebugMode = HttpContext.Current.IsDebuggingEnabled;

        #region const

        // just provide any unique random string for key
        private static readonly string INLINE_SCRIPT_RAWKEY_COMPACTKEY = "BundlerHelper_InlineScripts_RAWKEY_COMPACTKEY__" + DateTime.Now.ToString();
        private static readonly string INLINE_SCRIPT_COMPACTKEY_CONTENT = "BundlerHelper_InlineScripts_COMPACTKEY_CONTENT__" + DateTime.Now.ToString();

        private static readonly string INLINE_STYLESHEET_RAWKEY_COMPACTKEY = "BundlerHelper_InlineStyleSheets_RAWKEY_COMPACTKEY__" + DateTime.Now.ToString();
        private static readonly string INLINE_STYLESHEET_COMPACTKEY_CONTENT = "BundlerHelper_InlineStyleSheets_COMPACTKEY_CONTENT__" + DateTime.Now.ToString();

        private static readonly Regex rgxRemoveSpaces = new Regex(@"\s+");
        private static readonly Regex rgxRemoveComments1 = new Regex(@"//.*?$", RegexOptions.Multiline);
        private static readonly Regex rgxRemoveComments2 = new Regex(@"/\*.*?\*/", RegexOptions.Singleline);
        private static readonly Regex rgxGetScriptContents = new Regex(@"\s*<\s*script.*?>\s*(?<content>.+?)\s*</\s*script\s*>\s*", RegexOptions.Singleline);
        private static readonly Regex rgxGetStyleSheetContents = new Regex(@"\s*<\s*style.*?>\s*(?<content>.+?)\s*</\s*style\s*>\s*", RegexOptions.Singleline);

        #endregion

        #region wrapper

        /// <summary>
        /// Add inline stylesheet to head tag
        /// </summary>
        /// <param name="inlineStyleSheet">
        /// content of stylesheet, must begin with char @
        /// </param>
        /// <example>
        /// <code>
        /// AddStyle(<b>@</b><style>h2 { color:red; }</style>);
        /// </code>
        /// </example>
        public static object AddStyle(this HtmlHelper htmlHelper, Func<object, HelperResult> inlineStyleSheet)
        {
            var item = new BundleModel()
            {
                Type = BundleType.InlineStyle,
                Value = StoreStyleSheet(htmlHelper, inlineStyleSheet.Invoke(null).ToHtmlString())
            };

            if (LOG_SOURCE)
                item.Source = WebPageContext.Current.Page.VirtualPath;

            AddItem(GetContainer(htmlHelper), item, false);

            return null; // just for razor syntax
        }

        /// <summary>
        /// Add inline scripts to head tag
        /// </summary>
        /// <param name="inlineScript">
        /// content of scripts, must begin with char @
        /// </param>
        /// <example>
        /// <code>
        /// AddHeadScripts(<b>@</b><script>alert('hello world');</script>);
        /// </code>
        /// </example>
        public static object AddHeadScript(this HtmlHelper htmlHelper, Func<object, HelperResult> inlineScript)
        {
            var item = new BundleModel()
            {
                Type = BundleType.HeadInlineScript,
                Value = StoreScript(htmlHelper, inlineScript.Invoke(null).ToHtmlString())
            };

            if (LOG_SOURCE)
                item.Source = WebPageContext.Current.Page.VirtualPath;

            AddItem(GetContainer(htmlHelper), item, false);

            return null; // just for razor syntax
        }

        /// <summary>
        /// Add inline scripts to end of body tag
        /// </summary>
        /// <param name="inlineScript">
        /// content of scripts, must begin with char @
        /// </param>
        /// <example>
        /// <code>
        /// AddBodyScripts(<b>@</b><script>alert('hello world');</script>);
        /// </code>
        /// </example>
        public static object AddBodyScript(this HtmlHelper htmlHelper, Func<object, HelperResult> inlineScript)
        {
            var item = new BundleModel()
            {
                Type = BundleType.BodyInlineScript,
                Value = StoreScript(htmlHelper, inlineScript.Invoke(null).ToHtmlString())
            };

            if (LOG_SOURCE)
                item.Source = WebPageContext.Current.Page.VirtualPath;

            AddItem(GetContainer(htmlHelper), item, false);

            return null; // just for razor syntax
        }

        public static string GetInlineScript(HttpContextBase context, int? id)
        {
            if (id != null)
            {
                var compactKey_Content = GetDictionary<int, string>(context, INLINE_SCRIPT_COMPACTKEY_CONTENT);
                if (compactKey_Content.ContainsKey(id.Value) && ("Script:" + id.Value.ToString()) != context.Request.Headers.Get("If-None-Match"))
                {
                    context.Response.Headers.Add("Etag", "Script:" + id.Value.ToString());
                    return compactKey_Content[id.Value];
                }
            }

            return null;
        }

        public static string GetInlineStylesheet(HttpContextBase context, int? id)
        {
            if (id != null)
            {
                var compactKey_Content = GetDictionary<int, string>(context, INLINE_STYLESHEET_COMPACTKEY_CONTENT);
                if (compactKey_Content.ContainsKey(id.Value) && ("Stylesheet:" + id.Value.ToString()) != context.Request.Headers.Get("If-None-Match"))
                {
                    context.Response.Headers.Add("Etag", "Stylesheet:" + id.Value.ToString());
                    return compactKey_Content[id.Value];
                }
            }

            return null;
        }

        #endregion

        #region util

        private static string Normalize(string rawContent)
        {
            // remove comments
            var normalizedContent = rgxRemoveComments1.Replace(rawContent, string.Empty);
            normalizedContent = rgxRemoveComments2.Replace(normalizedContent, string.Empty);

            // remove spaces
            normalizedContent = rgxRemoveSpaces.Replace(normalizedContent, " ");

            return normalizedContent;
        }

        private static string StoreScript(HtmlHelper helper, string rawScript)
        {
            int id;
            var rawKey_CompactKey = GetDictionary<int, int>(helper.ViewContext.HttpContext, INLINE_SCRIPT_RAWKEY_COMPACTKEY);

            var rawKey = rawScript.GetHashCode();
            if (rawKey_CompactKey.ContainsKey(rawKey))
            {
                id = rawKey_CompactKey[rawKey];
            }
            else
            {
                var compactKey_Content = GetDictionary<int, string>(helper.ViewContext.HttpContext, INLINE_SCRIPT_COMPACTKEY_CONTENT);

                var normalizedContent = IsDebugMode ? rawScript : Normalize(rawScript);

                string compactScript = string.Empty;
                foreach (var match in rgxGetScriptContents.Matches(normalizedContent))
                {
                    compactScript += (match as Match).Groups["content"].Value;
                }

                int compactKey = compactScript.GetHashCode();
                if (!compactKey_Content.ContainsKey(compactKey))
                {
                    compactKey_Content.Add(compactKey, compactScript);
                }

                rawKey_CompactKey.Add(rawKey, compactKey);
                id = compactKey;
            }

            return Scripts.Render(InlineBundleController.SCRIPT_LINK + id).ToHtmlString();
        }

        private static string StoreStyleSheet(HtmlHelper helper, string rawStylesheet)
        {
            int id;
            var rawKey_CompactKey = GetDictionary<int, int>(helper.ViewContext.HttpContext, INLINE_STYLESHEET_RAWKEY_COMPACTKEY);

            var rawKey = rawStylesheet.GetHashCode();
            if (rawKey_CompactKey.ContainsKey(rawKey))
            {
                id = rawKey_CompactKey[rawKey];
            }
            else
            {
                var compactKey_Content = GetDictionary<int, string>(helper.ViewContext.HttpContext, INLINE_STYLESHEET_COMPACTKEY_CONTENT);

                var normalizedContent = IsDebugMode ? rawStylesheet : Normalize(rawStylesheet);

                string compactStylesheet = string.Empty;
                foreach (var match in rgxGetStyleSheetContents.Matches(normalizedContent))
                {
                    compactStylesheet += (match as Match).Groups["content"].Value;
                }

                int compactKey = compactStylesheet.GetHashCode();
                if (!compactKey_Content.ContainsKey(compactKey))
                {
                    compactKey_Content.Add(compactKey, compactStylesheet);
                }

                rawKey_CompactKey.Add(rawKey, compactKey);
                id = compactKey;
            }

            return Styles.Render(InlineBundleController.STYLESHEET_LINK + id).ToHtmlString();
        }

        private static Dictionary<TKey, TValue> GetDictionary<TKey, TValue>(HttpContextBase context, string key)
        {
            if (context.Application[key] == null)
            {
                context.Application[key] = new Dictionary<TKey, TValue>();
            }
            return context.Application[key] as Dictionary<TKey, TValue>;
        }

        #endregion
    }
}