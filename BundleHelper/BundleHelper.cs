using System.Collections.Generic;
using System.Linq;
using System.Web.Optimization;
using System.Web.WebPages;

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
            public int Position { get; set; }
            public string Source { get; set; }
            public string Value { get; set; }
            public BundleType Type { get; set; }

            public override string ToString()
            {
                if (LogSource)
                    return string.Format("<!-- Position [{2}], added from: {0} -->\r\n{1}", Source, Value, Position);
                else
                    return Value;
            }
        }

        enum BundleType // Order by HTML struct, do not change their int value
        {
            Style = 0,
            InlineStyle = 1,
            HeadScript = 2,
            HeadInlineScript = 4,
            BodyInlineScript = 8,
            BodyScript = 16
        }

        #endregion

        #region const

        // just provide any unique random string for key
        private static readonly string Key = "BundlerHelper__" + DateTime.Now.ToString();

        private static readonly bool LogSource = HttpContext.Current.IsDebuggingEnabled;

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
            return AddStyle(htmlHelper, filePath, 0);
        }

        /// <summary>
        /// Add styles file to head tag
        /// </summary>
        /// <param name="filePath">
        /// path to css file
        /// </param>
        /// <param name="position">
        /// Position of the stylesheet, order ascending, default is 0
        /// </param>
        /// <example>
        /// <code>
        /// AddStyles("~/Content/Site.css", 0);
        /// AddStyles("~/Content/css", int.MaxValue);
        /// </code>
        /// </example>
        public static object AddStyle(this HtmlHelper htmlHelper, string filePath, int position)
        {
            var item = new BundleModel
            {
                Position = position,
                Type = BundleType.Style,
                Value = Styles.Render(filePath).ToHtmlString()
            };

            if (LogSource)
                item.Source = WebPageContext.Current.Page.VirtualPath;

            AddItem(GetContainer(htmlHelper), item);

            return null; // just for razor syntax
        }

        /// <summary>
        /// Render style tags to page. Should use in master layout.
        /// </summary>
        public static IHtmlString RenderStyles(this HtmlHelper htmlHelper)
        {
            var allStyles = GetContainer(htmlHelper).Where(x => x.Type == BundleType.Style || x.Type == BundleType.InlineStyle).OrderBy(x => x.Position).ThenBy(x => x.Type);
            return new HtmlString(string.Join("\r\n", allStyles));
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
            return AddHeadScript(htmlHelper, filePath, 0);
        }

        /// <summary>
        /// Add script file to head tag
        /// </summary>
        /// <param name="filePath">
        /// path to javascript file
        /// </param>
        /// <param name="position">
        /// Position of the script, order ascending, default is 0
        /// </param>
        /// <example>
        /// <code>
        /// AddHeadScripts("~/Scripts/HelloWorld.js", 0);
        /// AddHeadScripts("~/bundles/jquery", int.MaxValue);
        /// </code>
        /// </example>
        public static object AddHeadScript(this HtmlHelper htmlHelper, string filePath, int position)
        {
            var item = new BundleModel
            {
                Position = position,
                Type = BundleType.HeadScript,
                Value = Scripts.Render(filePath).ToHtmlString()
            };

            if (LogSource)
                item.Source = WebPageContext.Current.Page.VirtualPath;

            AddItem(GetContainer(htmlHelper), item);

            return null; // just for razor syntax
        }

        /// <summary>
        /// Render script tags to head of page. Should use in master layout.
        /// </summary>
        public static IHtmlString RenderHeadScripts(this HtmlHelper htmlHelper)
        {
            var allHeadScripts = GetContainer(htmlHelper).Where(x => x.Type == BundleType.HeadScript || x.Type == BundleType.HeadInlineScript).OrderBy(x => x.Position).ThenBy(x => x.Type);
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
            return AddBodyScript(htmlHelper, filePath, 0);
        }

        /// <summary>
        /// Add script file to end of body tag
        /// </summary>
        /// <param name="filePath">
        /// path to javascript file
        /// </param>
        /// <param name="position">
        /// Position of the script, order ascending, default is 0
        /// </param>
        /// <example>
        /// <code>
        /// AddBodyScripts("~/Scripts/HelloWorld.js", 0);
        /// AddBodyScripts("~/bundles/jquery", int.MaxValue);
        /// </code>
        /// </example>
        public static object AddBodyScript(this HtmlHelper htmlHelper, string filePath, int position)
        {
            var item = new BundleModel
            {
                Position = position,
                Type = BundleType.BodyScript,
                Value = Scripts.Render(filePath).ToHtmlString()
            };

            if (LogSource)
                item.Source = WebPageContext.Current.Page.VirtualPath;

            AddItem(GetContainer(htmlHelper), item);

            return null; // just for razor syntax
        }

        /// <summary>
        /// Render script tags to body of page. Should use in master layout.
        /// </summary>
        public static IHtmlString RenderBodyScripts(this HtmlHelper htmlHelper)
        {
            var allBodyScripts = GetContainer(htmlHelper).Where(x => x.Type == BundleType.BodyInlineScript || x.Type == BundleType.BodyScript).OrderBy(x => x.Position).ThenBy(x => x.Type);
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
            if (context.Items[Key] == null)
                context.Items[Key] = new List<BundleModel>();
            return context.Items[Key] as IList<BundleModel>;
        }

        private static void AddItem(IList<BundleModel> list, BundleModel item, bool ignoreCase = true)
        {
            var existingItem = list.FirstOrDefault(x => x.Value.IndexOf(item.Value, ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture) >= 0);
            if (existingItem == null)
            {
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
