using BundleHelper;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.WebPages;

namespace BundleHelper
{
    public class BundleHelperAreaRegistration : AreaRegistration
    {
        internal static readonly string Version = "v" + Assembly.GetExecutingAssembly().GetName().Version;
        internal static readonly string UrlPattern = "BundleHelper/" + Version + "/{controller}/{action}/{id}";

        public override string AreaName
        {
            get
            {
                return "BundleHelperArea";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "BundleHelper_route",
                UrlPattern
            );
        }
    }

    public class InlineBundleController : Controller
    {
        internal static HttpStatusCodeResult Http304NotModifiedResult = new HttpStatusCodeResult(304);

        internal static readonly string ScriptLink = "~/" + BundleHelperAreaRegistration.UrlPattern.Replace("{controller}", "InlineBundle").Replace("{action}", "Script").Replace("{id}", string.Empty);
        internal static readonly string StylesheetLink = "~/" + BundleHelperAreaRegistration.UrlPattern.Replace("{controller}", "InlineBundle").Replace("{action}", "Stylesheet").Replace("{id}", string.Empty);

        public ActionResult Script(int? id)
        {
            var ret = System.Web.Mvc.BundleHelper.GetInlineScript(HttpContext, id);
            if (ret == null)
                return Http304NotModifiedResult;
            else
                return JavaScript(ret);
        }

        public ActionResult Stylesheet(int? id)
        {
            var ret = System.Web.Mvc.BundleHelper.GetInlineStylesheet(HttpContext, id);
            if (ret == null)
                return Http304NotModifiedResult;
            else
                return new ContentResult { Content = ret, ContentType = "text/css" };
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
        private static readonly string InlineScriptRawkeyCompactkey = "BundlerHelper_InlineScripts_RAWKEY_COMPACTKEY__" + DateTime.Now;
        private static readonly string InlineScriptCompactkeyContent = "BundlerHelper_InlineScripts_COMPACTKEY_CONTENT__" + DateTime.Now;

        private static readonly string InlineStylesheetRawkeyCompactkey = "BundlerHelper_InlineStyleSheets_RAWKEY_COMPACTKEY__" + DateTime.Now;
        private static readonly string InlineStylesheetCompactkeyContent = "BundlerHelper_InlineStyleSheets_COMPACTKEY_CONTENT__" + DateTime.Now;

        private static readonly Regex RgxRemoveSpaces = new Regex(@"\s+");
        private static readonly Regex RgxRemoveComments1 = new Regex(@"//.*?$", RegexOptions.Multiline);
        private static readonly Regex RgxRemoveComments2 = new Regex(@"/\*.*?\*/", RegexOptions.Singleline);
        private static readonly Regex RgxGetScriptContents = new Regex(@"\s*<\s*script.*?>\s*(?<content>.+?)\s*</\s*script\s*>\s*", RegexOptions.Singleline);
        private static readonly Regex RgxGetStyleSheetContents = new Regex(@"\s*<\s*style.*?>\s*(?<content>.+?)\s*</\s*style\s*>\s*", RegexOptions.Singleline);

        #endregion

        #region wrapper

        #region Add Style

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
            return AddStyle(htmlHelper, inlineStyleSheet, 0);
        }

        /// <summary>
        /// Add inline stylesheet to head tag
        /// </summary>
        /// <param name="inlineStyleSheet">
        /// content of stylesheet, must begin with char @
        /// </param>
        /// <param name="position">
        /// Position of the stylesheet, order ascending, default is 0
        /// </param>
        /// <example>
        /// <code>
        /// AddStyle(<b>@</b><style>h2 { color:red; }</style>, 0);
        /// </code>
        /// </example>
        public static object AddStyle(this HtmlHelper htmlHelper, Func<object, HelperResult> inlineStyleSheet, int position)
        {
            var item = new BundleModel
            {
                Position = position,
                Type = BundleType.InlineStyle,
                Value = StoreStyleSheet(htmlHelper, inlineStyleSheet.Invoke(null).ToHtmlString())
            };

            if (LogSource)
                item.Source = WebPageContext.Current.Page.VirtualPath;

            AddItem(GetContainer(htmlHelper), item, false);

            return null; // just for razor syntax
        }

        #endregion

        #region Add Head Script

        /// <summary>
        /// Add inline scripts to head tag
        /// </summary>
        /// <param name="inlineScript">
        /// content of scripts, must begin with char @
        /// </param>
        /// <example>
        /// <code>
        /// AddHeadScripts(<b>@</b><script>alert('hello world');</script>, 100);
        /// </code>
        /// </example>
        public static object AddHeadScript(this HtmlHelper htmlHelper, Func<object, HelperResult> inlineScript)
        {
            return AddHeadScript(htmlHelper, inlineScript, 0);
        }

        /// <summary>
        /// Add inline scripts to head tag
        /// </summary>
        /// <param name="inlineScript">
        /// content of scripts, must begin with char @
        /// </param>
        /// <param name="position">
        /// Position of the script, order ascending, default is 0
        /// </param>
        /// <example>
        /// <code>
        /// AddHeadScripts(<b>@</b><script>alert('hello world');</script>, 100);
        /// </code>
        /// </example>
        public static object AddHeadScript(this HtmlHelper htmlHelper, Func<object, HelperResult> inlineScript, int position)
        {
            var item = new BundleModel
            {
                Position = position,
                Type = BundleType.HeadInlineScript,
                Value = StoreScript(htmlHelper, inlineScript.Invoke(null).ToHtmlString())
            };

            if (LogSource)
                item.Source = WebPageContext.Current.Page.VirtualPath;

            AddItem(GetContainer(htmlHelper), item, false);

            return null; // just for razor syntax
        }

        #endregion

        #region Add Body Script

        /// <summary>
        /// Add inline scripts to end of body tag
        /// </summary>
        /// <param name="inlineScript">
        /// content of scripts, must begin with char @
        /// </param>
        /// <example>
        /// <code>
        /// AddBodyScripts(<b>@</b><script>alert('hello world');</script>, 100);
        /// </code>
        /// </example>
        public static object AddBodyScript(this HtmlHelper htmlHelper, Func<object, HelperResult> inlineScript)
        {
            return AddBodyScript(htmlHelper, inlineScript, 0);
        }

        /// <summary>
        /// Add inline scripts to end of body tag
        /// </summary>
        /// <param name="inlineScript">
        /// content of scripts, must begin with char @
        /// </param>
        /// <param name="position">
        /// Position of the script, order ascending, default is 0
        /// </param>
        /// <example>
        /// <code>
        /// AddBodyScripts(<b>@</b><script>alert('hello world');</script>, 100);
        /// </code>
        /// </example>
        public static object AddBodyScript(this HtmlHelper htmlHelper, Func<object, HelperResult> inlineScript, int position)
        {
            var item = new BundleModel
            {
                Position = position,
                Type = BundleType.BodyInlineScript,
                Value = StoreScript(htmlHelper, inlineScript.Invoke(null).ToHtmlString())
            };

            if (LogSource)
                item.Source = WebPageContext.Current.Page.VirtualPath;

            AddItem(GetContainer(htmlHelper), item, false);

            return null; // just for razor syntax
        }

        #endregion

        #region Get Style/Script

        public static string GetInlineScript(HttpContextBase context, int? id)
        {
            if (id != null)
            {
                var compactKey_Content = GetDictionary<int, string>(context, InlineScriptCompactkeyContent);
                if (compactKey_Content.ContainsKey(id.Value) && ("Script:" + id.Value) != context.Request.Headers.Get("If-None-Match"))
                {
                    context.Response.Headers.Add("Etag", "Script:" + id.Value);
                    return compactKey_Content[id.Value];
                }
            }

            return null;
        }

        public static string GetInlineStylesheet(HttpContextBase context, int? id)
        {
            if (id != null)
            {
                var compactKey_Content = GetDictionary<int, string>(context, InlineStylesheetCompactkeyContent);
                if (compactKey_Content.ContainsKey(id.Value) && ("Stylesheet:" + id.Value) != context.Request.Headers.Get("If-None-Match"))
                {
                    context.Response.Headers.Add("Etag", "Stylesheet:" + id.Value);
                    return compactKey_Content[id.Value];
                }
            }

            return null;
        }

        #endregion

        #endregion

        #region util

        private static string Normalize(string rawContent)
        {
            // remove comments
            var normalizedContent = RgxRemoveComments1.Replace(rawContent, string.Empty);
            normalizedContent = RgxRemoveComments2.Replace(normalizedContent, string.Empty);

            // remove spaces
            normalizedContent = RgxRemoveSpaces.Replace(normalizedContent, " ");

            return normalizedContent;
        }

        private static string StoreScript(HtmlHelper helper, string rawScript)
        {
            int id;
            var rawKey_CompactKey = GetDictionary<int, int>(helper.ViewContext.HttpContext, InlineScriptRawkeyCompactkey);

            var rawKey = rawScript.GetHashCode();
            if (rawKey_CompactKey.ContainsKey(rawKey))
            {
                id = rawKey_CompactKey[rawKey];
            }
            else
            {
                var compactKey_Content = GetDictionary<int, string>(helper.ViewContext.HttpContext, InlineScriptCompactkeyContent);

                var normalizedContent = IsDebugMode ? rawScript : Normalize(rawScript);

                string compactScript = string.Empty;
                foreach (Match match in RgxGetScriptContents.Matches(normalizedContent))
                {
                    compactScript += match.Groups["content"].Value;
                }

                int compactKey = compactScript.GetHashCode();
                if (!compactKey_Content.ContainsKey(compactKey))
                {
                    compactKey_Content.Add(compactKey, compactScript);
                }

                rawKey_CompactKey.Add(rawKey, compactKey);
                id = compactKey;
            }

            return Scripts.Render(InlineBundleController.ScriptLink + id).ToHtmlString();
        }

        private static string StoreStyleSheet(HtmlHelper helper, string rawStylesheet)
        {
            int id;
            var rawKey_CompactKey = GetDictionary<int, int>(helper.ViewContext.HttpContext, InlineStylesheetRawkeyCompactkey);

            var rawKey = rawStylesheet.GetHashCode();
            if (rawKey_CompactKey.ContainsKey(rawKey))
            {
                id = rawKey_CompactKey[rawKey];
            }
            else
            {
                var compactKey_Content = GetDictionary<int, string>(helper.ViewContext.HttpContext, InlineStylesheetCompactkeyContent);

                var normalizedContent = IsDebugMode ? rawStylesheet : Normalize(rawStylesheet);

                string compactStylesheet = string.Empty;
                foreach (Match match in RgxGetStyleSheetContents.Matches(normalizedContent))
                {
                    compactStylesheet += match.Groups["content"].Value;
                }

                int compactKey = compactStylesheet.GetHashCode();
                if (!compactKey_Content.ContainsKey(compactKey))
                {
                    compactKey_Content.Add(compactKey, compactStylesheet);
                }

                rawKey_CompactKey.Add(rawKey, compactKey);
                id = compactKey;
            }

            return Styles.Render(InlineBundleController.StylesheetLink + id).ToHtmlString();
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