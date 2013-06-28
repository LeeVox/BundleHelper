using System.Collections.Generic;
using System.Linq;
using System.Web.Optimization;
using System.Web.WebPages;
using BundleHelper;

namespace System.Web.Mvc
{
    /*
     *  Author: Dat Le (http://www.lethanhdat.com)
     * 
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
        #region inner classes
        
        class BundleModel
        {
            public string Source { get; set; }
            public string Value { get; set; }
            public BundleType Type { get; set; }

            public override string ToString()
            {
#if DEBUG
                return string.Format("<!-- Added from: {0} -->\r\n{1}", Source, Value);
#else
                return Value;
#endif
            }
        }

        enum BundleType // Order by HTML struct, do not change their int value
        {
            Style = 0,
            HeadScript = 1,
            HeadInlineScript = 2,
            BodyInlineScript = 4,
            BodyScript = 8
        }

        #endregion

        #region const

        // just provide any unique random string for key
        private static readonly string KEY = "BundlerHelper__" + DateTime.Now.ToString();

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
        public static object AddStyle(this HtmlHelper htmlHelper, string filePath)
        {
            return AddStyle(htmlHelper, filePath, false);
        }

        /// <summary>
        /// Add styles file to head tag
        /// </summary>
        /// <param name="filePath">
        /// path to css file
        /// </param>
        /// <param name="addToTop">
        /// Add to top of stylesheet zone in head tag
        /// </param>
        /// <example>
        /// <code>
        /// AddStyles("~/Content/Site.css", true);
        /// AddStyles("~/Content/css", false);
        /// </code>
        /// </example>
        public static object AddStyle(this HtmlHelper htmlHelper, string filePath, bool addToTop)
        {
            var item = new BundleModel()
            {
                Type = BundleType.Style,
                Source = WebPageContext.Current.Page.VirtualPath,
                Value = Styles.Render(filePath).ToHtmlString()
            };
            AddItem(GetContainer(htmlHelper), item, addToTop);

            return null; // just for razor syntax
        }

        /// <summary>
        /// Render style tags to page. Should use in master layout.
        /// </summary>
        public static IHtmlString RenderStyles(this HtmlHelper htmlHelper)
        {
            return new HtmlString(string.Join("\r\n", GetContainer(htmlHelper).Where(x => x.Type == BundleType.Style)));
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
        public static object AddHeadScript(this HtmlHelper htmlHelper, string filePath)
        {
            return AddHeadScript(htmlHelper, filePath, false);
        }

        /// <summary>
        /// Add script file to head tag
        /// </summary>
        /// <param name="filePath">
        /// path to javascript file
        /// </param>
        /// <param name="addToTop">
        /// Add to top of script zone in head tag
        /// </param>
        /// <example>
        /// <code>
        /// AddHeadScripts("~/Scripts/HelloWorld.js", false);
        /// AddHeadScripts("~/bundles/jquery", true);
        /// </code>
        /// </example>
        public static object AddHeadScript(this HtmlHelper htmlHelper, string filePath, bool addToTop)
        {
            var item = new BundleModel()
            {
                Type = BundleType.HeadScript,
                Source = WebPageContext.Current.Page.VirtualPath,
                Value = Scripts.Render(filePath).ToHtmlString()
            };

            AddItem(GetContainer(htmlHelper), item, addToTop);

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
                Source = WebPageContext.Current.Page.VirtualPath,
                Value = InlineScriptsController.CompactScript(htmlHelper, inlineScript.Invoke(null).ToHtmlString())
            };

            AddItem(GetContainer(htmlHelper), item, false);

            return null; // just for razor syntax
        }

        /// <summary>
        /// Render script tags to head of page. Should use in master layout.
        /// </summary>
        public static IHtmlString RenderHeadScripts(this HtmlHelper htmlHelper)
        {
            var allHeadScripts = GetContainer(htmlHelper).Where(x => x.Type == BundleType.HeadScript || x.Type == BundleType.HeadInlineScript).OrderBy(x => x.Type);
            return new HtmlString(string.Join("\r\n", allHeadScripts));
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
        public static object AddBodyScript(this HtmlHelper htmlHelper, string filePath)
        {
            return AddBodyScript(htmlHelper, filePath, false);
        }

        /// <summary>
        /// Add script file to end of body tag
        /// </summary>
        /// <param name="filePath">
        /// path to javascript file
        /// </param>
        /// <param name="addToTop">
        /// Add to top of script zone in body tag
        /// </param>
        /// <example>
        /// <code>
        /// AddBodyScripts("~/Scripts/HelloWorld.js", false);
        /// AddBodyScripts("~/bundles/jquery", true);
        /// </code>
        /// </example>
        public static object AddBodyScript(this HtmlHelper htmlHelper, string filePath, bool addToTop)
        {
            var item = new BundleModel()
            {
                Type = BundleType.BodyScript,
                Source = WebPageContext.Current.Page.VirtualPath,
                Value = Scripts.Render(filePath).ToHtmlString()
            };

            AddItem(GetContainer(htmlHelper), item, addToTop);

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
                Source = WebPageContext.Current.Page.VirtualPath,
                Value = InlineScriptsController.CompactScript(htmlHelper, inlineScript.Invoke(null).ToHtmlString())
            };

            AddItem(GetContainer(htmlHelper), item, false);

            return null; // just for razor syntax
        }

        /// <summary>
        /// Render script tags to body of page. Should use in master layout.
        /// </summary>
        public static IHtmlString RenderBodyScripts(this HtmlHelper htmlHelper)
        {
            var allBodyScripts = GetContainer(htmlHelper).Where(x => x.Type == BundleType.BodyInlineScript || x.Type == BundleType.BodyScript).OrderBy(x => x.Type);
            return new HtmlString(string.Join("\r\n", allBodyScripts));
        }

        #endregion

        #region ulti

        private static IList<BundleModel> GetContainer(HtmlHelper htmlHelper)
        {
            return GetContainer(htmlHelper.ViewContext.HttpContext);
        }

        private static IList<BundleModel> GetContainer(HttpContextBase context)
        {
            if (context.Items[KEY] == null)
                context.Items[KEY] = new List<BundleModel>();
            return context.Items[KEY] as IList<BundleModel>;
        }

        private static void AddItem(IList<BundleModel> list, BundleModel item, bool addToTop = false, bool ignoreCase = true)
        {
            var existingItem = list.FirstOrDefault(x => x.Value.IndexOf(item.Value, ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture) >= 0);
            if (existingItem == null)
            {
                if (addToTop)
                    list.Insert(0, item);
                else
                    list.Add(item);
            }
            else
            {
                existingItem.Source += ";\r\n" + item.Source;
                if ((int)item.Type < (int)existingItem.Type)
                    existingItem.Type = item.Type;
            }
        }

        #endregion
    }
}
