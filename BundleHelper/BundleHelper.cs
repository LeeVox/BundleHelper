using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.WebPages;

namespace System.Web.Mvc
{
    /*
     *      Version     Date            Description
     *      1.0         Jun-25-2013     First draft
     *      1.1         Jun-26-2013     Updated for supporting Using Attributes
     *      1.1.5       Jun-26-2013     Fixed to ignore case for checking duplicated items
     */

    /*
     *  How to use:
     *      + Call those methods in master layout (~/View/Shared/_Layout.cshtml):
     *          <head>
     *              ....
     *              @Html.RenderStyles()
     *              @Html.RenderHeadScripts()
     *          </head>
     *          <body>
     *              ...
     *              @Html.RenderBodyScripts()
     *          </body>
     *      + In partial views, call those methods in case of adding js, css...
     *          @Html.AddStyle("~/Content/Site.css")
     *          @Html.AddStyle("~/Content/css")
     *          
     *          @Html.AddHeadScript("~/bundles/jquery")
     *          @Html.AddHeadScript(@<script>alert('this is script on head tag');</script>)
     *          
     *          @Html.AddBodyScript(@<script>alert('this is script at end of body tag');</script>)
     *          @Html.AddBodyScript("~/Scripts/HelloWorld.js")
     *          
     *          ...
     */

    /// <summary>
    /// Helper for removing duplicate css, js... from multi partial views.
    /// </summary>
    public static partial class BundleHelper
    {
        #region const

        // just provide any unique random string for keys
        private static readonly string PROJECT_NAME = HttpContext.Current.ApplicationInstance.GetType().Assembly.FullName;
        private static readonly string KEY_STYLES = PROJECT_NAME + "___styles";
        private static readonly string KEY_HEAD_SCRIPTS = PROJECT_NAME + "___head_scripts";
        private static readonly string KEY_HEAD_INLINE_SCRIPTS = PROJECT_NAME + "___head_inline_scripts";
        private static readonly string KEY_BODY_INLINE_SCRIPTS = PROJECT_NAME + "___body_inline_scripts";
        private static readonly string KEY_BODY_SCRIPTS = PROJECT_NAME + "___body_scripts";

        #endregion

        #region styles

        /// <summary>
        /// Add styles file to head tag
        /// </summary>
        /// <param name="filePath">
        /// path to css file
        /// </param>
        /// <example>
        /// <code>
        /// AddStyles("~/Content/Site.css");
        /// AddStyles("~/Content/css");
        /// </code>
        /// </example>
        public static object AddStyle(this HtmlHelper htmlHelper, string filePath, bool insertMode = false)
        {
            string style = System.Web.Optimization.Styles.Render(filePath).ToHtmlString();

            AddItemToList(GetHttpContextItem(htmlHelper, KEY_STYLES), style, WebPageContext.Current.Page.VirtualPath, insertMode);

            return null;
        }

        /// <summary>
        /// Render style tags to page. Should use in master layout.
        /// </summary>
        public static IHtmlString RenderStyles(this HtmlHelper htmlHelper)
        {
            return new HtmlString(string.Join("\r\n", GetHttpContextItem(htmlHelper, KEY_STYLES)));
        }

        #endregion

        #region head scripts

        /// <summary>
        /// Add script file to head tag
        /// </summary>
        /// <param name="filePath">
        /// path to javascript file
        /// </param>
        /// <example>
        /// <code>
        /// AddHeadScripts("~/Scripts/HelloWorld.js");
        /// AddHeadScripts("~/bundles/jquery");
        /// </code>
        /// </example>
        public static object AddHeadScript(this HtmlHelper htmlHelper, string filePath, bool insertMode = false)
        {
            string script = System.Web.Optimization.Scripts.Render(filePath).ToHtmlString();

            var bodyScripts = GetHttpContextItem(htmlHelper, KEY_BODY_SCRIPTS);
            BundleModel tmp = null;
            if ((tmp = Find(bodyScripts, script, true)) != null)
            {
                bodyScripts.Remove(tmp);
            }
            AddItemToList(GetHttpContextItem(htmlHelper, KEY_HEAD_SCRIPTS), script, WebPageContext.Current.Page.VirtualPath, insertMode);

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
        public static object AddHeadScript(this HtmlHelper htmlHelper, Func<object, HelperResult> inlineScript, bool insertMode = false)
        {
            string script = Regex.Replace(inlineScript.Invoke(null).ToHtmlString(), @"\s+", " ");

            var bodyScripts = GetHttpContextItem(htmlHelper, KEY_BODY_INLINE_SCRIPTS);
            BundleModel tmp = null;
            if ((tmp = Find(bodyScripts, script)) != null)
            {
                bodyScripts.Remove(tmp);
            }
            AddItemToList(GetHttpContextItem(htmlHelper, KEY_HEAD_INLINE_SCRIPTS), script, WebPageContext.Current.Page.VirtualPath, insertMode);

            return null; // just for razor syntax
        }

        /// <summary>
        /// Render script tags to head of page. Should use in master layout.
        /// </summary>
        public static IHtmlString RenderHeadScripts(this HtmlHelper htmlHelper)
        {
            // script files
            string scriptFiles = string.Join("\r\n", GetHttpContextItem(htmlHelper, KEY_HEAD_SCRIPTS));
            string inlineScripts = string.Join("\r\n", GetHttpContextItem(htmlHelper, KEY_HEAD_INLINE_SCRIPTS));

            return new HtmlString(scriptFiles + inlineScripts);
        }

        #endregion

        #region body scripts

        /// <summary>
        /// Add script file to end of body tag
        /// </summary>
        /// <param name="filePath">
        /// path to javascript file
        /// </param>
        /// <example>
        /// <code>
        /// AddBodyScripts("~/Scripts/HelloWorld.js");
        /// AddBodyScripts("~/bundles/jquery");
        /// </code>
        /// </example>
        public static object AddBodyScript(this HtmlHelper htmlHelper, string filePath, bool insertMode = false)
        {
            string script = System.Web.Optimization.Scripts.Render(filePath).ToHtmlString();

            var headScripts = GetHttpContextItem(htmlHelper, KEY_HEAD_SCRIPTS);
            if (Find(headScripts, script, true) == null)
            {
                AddItemToList(GetHttpContextItem(htmlHelper, KEY_BODY_SCRIPTS), script, WebPageContext.Current.Page.VirtualPath, insertMode);
            }

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
        public static object AddBodyScript(this HtmlHelper htmlHelper, Func<object, HelperResult> inlineScript, bool insertMode = false)
        {
            string script = Regex.Replace(inlineScript.Invoke(null).ToHtmlString(), @"\s+", " ");

            var headScripts = GetHttpContextItem(htmlHelper, KEY_HEAD_INLINE_SCRIPTS);
            if (Find(headScripts, script) == null)
            {
                AddItemToList(GetHttpContextItem(htmlHelper, KEY_BODY_INLINE_SCRIPTS), script, WebPageContext.Current.Page.VirtualPath, insertMode);
            }
            return null; // just for razor syntax
        }

        /// <summary>
        /// Render script tags to body of page. Should use in master layout.
        /// </summary>
        public static IHtmlString RenderBodyScripts(this HtmlHelper htmlHelper)
        {
            // script files
            string scriptFiles = string.Join("\r\n", GetHttpContextItem(htmlHelper, KEY_BODY_SCRIPTS));
            string inlineScripts = string.Join("\r\n", GetHttpContextItem(htmlHelper, KEY_BODY_INLINE_SCRIPTS));

            return new HtmlString(inlineScripts + scriptFiles);
        }

        #endregion

        #region ulti

        /// <summary>
        /// Get item in HttpContext for storing paths in each requests.
        /// </summary>
        private static IList<BundleModel> GetHttpContextItem(HtmlHelper htmlHelper, string key)
        {
            return GetHttpContextItem(htmlHelper.ViewContext.HttpContext, key);
        }

        /// <summary>
        /// Get item in HttpContext for storing paths in each requests.
        /// </summary>
        private static IList<BundleModel> GetHttpContextItem(HttpContextBase context, string key)
        {
            if (context.Items[key] == null)
                context.Items[key] = new List<BundleModel>();
            return context.Items[key] as IList<BundleModel>;
        }

        private static void AddItemToList(IList<BundleModel> list, string item, string source, bool insertMode)
        {
            var existingItem = list.FirstOrDefault(x => x.Content.Contains(item));

#if DEBUG
            if (existingItem != null && source != null)
            {
                existingItem.From += ";\r\n" + source;
            }
            else
            {
                var newModel = new BundleModel()
                {
                    From = source,
                    Content = item
                };

                if (insertMode)
                {
                    list.Insert(0, newModel);
                }
                else
                {
                    list.Add(newModel);
                }
            }
#else
            if (existingItem == null)
            {
                var newModel = new BundleModel()
                {
                    Content = item
                };

                if (insertMode)
                {
                    list.Insert(0, newModel);
                }
                else
                {
                    list.Add(newModel);
                }
            }
#endif
        }

        private static BundleModel Find(IList<BundleModel> list, string item, bool ignoreCase = false)
        {
            if (ignoreCase)
            {
                return list.FirstOrDefault(x => x.Content.ToLower().Contains(item.ToLower()));
            }
            else
            {
                return list.FirstOrDefault(x => x.Content.Contains(item));
            }
        }

        class BundleModel
        {
            public string From { get; set; }
            public string Content { get; set; }

            public override string ToString()
            {
#if DEBUG
                return string.Format("<!-- Added from: {0} -->\r\n{1}", From, Content);
#else
                return Content;
#endif
            }
        }

        #endregion
    }
}
