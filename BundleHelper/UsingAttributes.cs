using System.Linq;
using System.Web.Optimization;

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
     *      + In controller or action, provide those attributes:
     *          [UsingStyles("~/Content/Site.css", "~/Content/css")]
     *          
     *          [UsingHeadScripts("~/bundles/jquery", "~/Scripts/Global.js")]
     *          
     *          [UsingBodyScripts("~/bundles/jquery", "~/Scripts/HelloWorld.js")]
     *          
     *          ...
     */

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public abstract class UsingBundle : ActionFilterAttribute
    {
        protected string[] Files { get; set; }
        protected bool AddToTop { get; set; }
        protected virtual Action<ActionExecutingContext, string, bool> Function { get; set; }

        public UsingBundle(Action<ActionExecutingContext, string, bool> function, bool addToTop, params string[] files)
        {
            this.Function = function;
            this.AddToTop = addToTop;
            this.Files = files;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            foreach (var file in Files)
            {
                Function.Invoke(filterContext, file, AddToTop);
            }
        }
    }

    /// <summary>
    /// Add styles files to head tag
    /// </summary>
    public class UsingStyles : UsingBundle
    {
        /// <summary>
        /// Path to styles files
        /// </summary>
        public UsingStyles(params string[] files)
            : base(BundleHelper.AddStyle, false, files)
        {
        }

        /// <summary>
        /// Path to styles files
        /// </summary>
        /// <param name="addToTop">
        /// Add to top of stylesheet zone in head tag
        /// </param>
        public UsingStyles(bool addToTop, params string[] files)
            : base(BundleHelper.AddStyle, addToTop, files)
        {
        }
    }

    /// <summary>
    /// Add script files to head tag
    /// </summary>
    public class UsingHeadScripts : UsingBundle
    {
        /// <summary>
        /// Path to script files
        /// </summary>
        public UsingHeadScripts(params string[] files)
            : base(BundleHelper.AddHeadScript, false, files)
        {
        }

        /// <summary>
        /// Path to script files
        /// </summary>
        /// <param name="addToTop">
        /// Add to top of script zone in head tag
        /// </param>
        public UsingHeadScripts(bool addToTop, params string[] files)
            : base(BundleHelper.AddHeadScript, addToTop, files)
        {
        }
    }

    /// <summary>
    /// Add script files to body tag
    /// </summary>
    public class UsingBodyScripts : UsingBundle
    {
        /// <summary>
        /// Path to script files
        /// </summary>
        public UsingBodyScripts(params string[] files)
            : base(BundleHelper.AddBodyScript, false, files)
        {
        }

        /// <summary>
        /// Path to script files
        /// </summary>
        /// <param name="addToTop">
        /// Add to top of script zone in body tag
        /// </param>
        public UsingBodyScripts(bool addToTop, params string[] files)
            : base(BundleHelper.AddBodyScript, addToTop, files)
        {
        }
    }

    // Extend the BundleHelper for supporting Using Attributes
    public static partial class BundleHelper
    {
        /// <summary>
        /// Add styles file to head tag
        /// </summary>
        /// <param name="filePath">
        /// path to css file
        /// </param>
        /// <remarks>Used by UsingStyles Attribute</remarks>
        internal static void AddStyle(ActionExecutingContext context, string filePath)
        {
            AddStyle(context, filePath, false);
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
        /// <remarks>Used by UsingStyles Attribute</remarks>
        internal static void AddStyle(ActionExecutingContext context, string filePath, bool addToTop)
        {
            var item = new BundleModel()
            {
                Type = BundleType.Style,
                Value = Styles.Render(filePath).ToHtmlString()
            };

            if (LOG_SOURCE)
                item.Source = GetSource(context);

            AddItem(GetContainer(context.HttpContext), item, addToTop);

        }

        /// <summary>
        /// Add script file to head tag
        /// </summary>
        /// <param name="filePath">
        /// path to script file
        /// </param>
        /// <remarks>Used by UsingHeadScripts Attribute</remarks>
        internal static void AddHeadScript(ActionExecutingContext context, string filePath)
        {
            AddHeadScript(context, filePath);
        }

        /// <summary>
        /// Add script file to head tag
        /// </summary>
        /// <param name="filePath">
        /// path to script file
        /// </param>
        /// /// <param name="addToTop">
        /// Add to top of script zone in head tag
        /// </param>
        /// <remarks>Used by UsingHeadScripts Attribute</remarks>
        internal static void AddHeadScript(ActionExecutingContext context, string filePath, bool addToTop)
        {
            var item = new BundleModel()
            {
                Type = BundleType.HeadScript,
                Value = Scripts.Render(filePath).ToHtmlString()
            };

            if (LOG_SOURCE)
                item.Source = GetSource(context);

            AddItem(GetContainer(context.HttpContext), item, addToTop);
        }

        /// <summary>
        /// Add script file to body tag
        /// </summary>
        /// <param name="filePath">
        /// path to script file
        /// </param>
        /// <remarks>Used by UsingBodyScripts Attribute</remarks>
        internal static void AddBodyScript(ActionExecutingContext context, string filePath)
        {
            AddBodyScript(context, filePath);
        }

        /// <summary>
        /// Add script file to body tag
        /// </summary>
        /// <param name="filePath">
        /// path to script file
        /// </param>
        /// <param name="addToTop">
        /// Add to top of script zone in body tag
        /// </param>
        /// <remarks>Used by UsingBodyScripts Attribute</remarks>
        internal static void AddBodyScript(ActionExecutingContext context, string filePath, bool addToTop)
        {
            var item = new BundleModel()
            {
                Type = BundleType.BodyScript,
                Value = Scripts.Render(filePath).ToHtmlString()
            };

            if (LOG_SOURCE)
                item.Source = GetSource(context);

            AddItem(GetContainer(context.HttpContext), item, addToTop);
        }

        private static string GetSource(ActionExecutingContext context)
        {
            var source = context.Controller.ToString() // controller name
                // action name
                + "." + context.ActionDescriptor.ActionName + "(" + string.Join(", ", context.ActionDescriptor.GetParameters().Select(x => x.ParameterType)) + ")";

            return source;
        }
    }
}