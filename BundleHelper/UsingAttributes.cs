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
        protected int Position { get; set; }
        protected virtual Action<ActionExecutingContext, string, int> Function { get; set; }

        protected UsingBundle(Action<ActionExecutingContext, string, int> function, int position, params string[] files)
        {
            Function = function;
            Position = position;
            Files = files;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            foreach (var file in Files)
            {
                Function.Invoke(filterContext, file, Position);
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
            : base(BundleHelper.AddStyle, 0, files)
        {
        }

        /// <summary>
        /// Path to styles files
        /// </summary>
        /// <param name="position">
        /// Position of the stylesheet, order ascending, default is 0
        /// </param>
        public UsingStyles(int position, params string[] files)
            : base(BundleHelper.AddStyle, position, files)
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
            : base(BundleHelper.AddHeadScript, 0, files)
        {
        }

        /// <summary>
        /// Path to script files
        /// </summary>
        /// <param name="position">
        /// Position of the script, order ascending, default is 0
        /// </param>
        public UsingHeadScripts(int position, params string[] files)
            : base(BundleHelper.AddHeadScript, position, files)
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
            : base(BundleHelper.AddBodyScript, 0, files)
        {
        }

        /// <summary>
        /// Path to script files
        /// </summary>
        /// <param name="position">
        /// Position of the script, order ascending, default is 0
        /// </param>
        public UsingBodyScripts(int position, params string[] files)
            : base(BundleHelper.AddBodyScript, position, files)
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
            AddStyle(context, filePath, 0);
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
        /// <remarks>Used by UsingStyles Attribute</remarks>
        internal static void AddStyle(ActionExecutingContext context, string filePath, int position)
        {
            var item = new BundleModel
            {
                Position = position,
                Type = BundleType.Style,
                Value = Styles.Render(filePath).ToHtmlString()
            };

            if (LogSource)
                item.Source = GetSource(context);

            AddItem(GetContainer(context.HttpContext), item);

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
            AddHeadScript(context, filePath, 0);
        }

        /// <summary>
        /// Add script file to head tag
        /// </summary>
        /// <param name="filePath">
        /// path to script file
        /// </param>
        /// /// <param name="position">
        /// Position of the script, order ascending, default is 0
        /// </param>
        /// <remarks>Used by UsingHeadScripts Attribute</remarks>
        internal static void AddHeadScript(ActionExecutingContext context, string filePath, int position)
        {
            var item = new BundleModel
            {
                Position = position,
                Type = BundleType.HeadScript,
                Value = Scripts.Render(filePath).ToHtmlString()
            };

            if (LogSource)
                item.Source = GetSource(context);

            AddItem(GetContainer(context.HttpContext), item);
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
            AddBodyScript(context, filePath, 0);
        }

        /// <summary>
        /// Add script file to body tag
        /// </summary>
        /// <param name="filePath">
        /// path to script file
        /// </param>
        /// <param name="position">
        /// Position of the script, order ascending, default is 0
        /// </param>
        /// <remarks>Used by UsingBodyScripts Attribute</remarks>
        internal static void AddBodyScript(ActionExecutingContext context, string filePath, int position)
        {
            var item = new BundleModel
            {
                Position = position,
                Type = BundleType.BodyScript,
                Value = Scripts.Render(filePath).ToHtmlString()
            };

            if (LogSource)
                item.Source = GetSource(context);

            AddItem(GetContainer(context.HttpContext), item);
        }

        private static string GetSource(ActionExecutingContext context)
        {
            var source = context.Controller // controller name
                // action name
                + "." + context.ActionDescriptor.ActionName + "(" + string.Join(", ", context.ActionDescriptor.GetParameters().Select(x => x.ParameterType)) + ")";

            return source;
        }
    }
}