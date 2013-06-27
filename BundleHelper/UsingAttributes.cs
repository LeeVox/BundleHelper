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
        protected virtual Action<ActionExecutingContext, string> Function { get; set; }

        public UsingBundle(params string[] files)
        {
            this.Files = files;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            foreach (var file in Files)
            {
                Function.Invoke(filterContext, file);
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
            : base(files)
        {
            Function = (context, file) => BundleHelper.AddStyle(context, file);
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
            : base(files)
        {
            Function = (context, file) => BundleHelper.AddHeadScript(context, file);
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
            : base(files)
        {
            Function = (context, file) => BundleHelper.AddBodyScript(context, file);
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
        public static object AddStyle(ActionExecutingContext context, string filePath, bool insertMode = false)
        {
            var item = new BundleModel()
            {
                Type = BundleType.Style,
                Value = Styles.Render(filePath).ToHtmlString()
            };

#if DEBUG
            item.Source = GetSource(context);
#endif

            AddItem(GetContainer(context.HttpContext), item, insertMode);

            return null; // just for razor syntax

        }

        /// <summary>
        /// Add script file to head tag
        /// </summary>
        /// <param name="filePath">
        /// path to script file
        /// </param>
        /// <remarks>Used by UsingHeadScripts Attribute</remarks>
        public static object AddHeadScript(ActionExecutingContext context, string filePath, bool insertMode = false)
        {
            var item = new BundleModel()
            {
                Type = BundleType.HeadScript,
                Value = Scripts.Render(filePath).ToHtmlString()
            };

#if DEBUG
            item.Source = GetSource(context);
#endif

            AddItem(GetContainer(context.HttpContext), item, insertMode);

            return null; // just for razor syntax
        }

        /// <summary>
        /// Add script file to body tag
        /// </summary>
        /// <param name="filePath">
        /// path to script file
        /// </param>
        /// <remarks>Used by UsingBodyScripts Attribute</remarks>
        public static object AddBodyScript(ActionExecutingContext context, string filePath, bool insertMode = false)
        {
            var item = new BundleModel()
            {
                Type = BundleType.BodyScript,
                Value = Scripts.Render(filePath).ToHtmlString()
            };

#if DEBUG
            item.Source = GetSource(context);
#endif

            AddItem(GetContainer(context.HttpContext), item, insertMode);

            return null; // just for razor syntax
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