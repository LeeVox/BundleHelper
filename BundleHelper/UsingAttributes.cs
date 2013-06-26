using System.Linq;

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
        protected virtual string[] Files { get; set; }
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
        public static object AddStyle(ActionExecutingContext context, string path, bool insertMode = false)
        {
            string style = System.Web.Optimization.Styles.Render(path).ToHtmlString();

            string source = null;
#if DEBUG
            // add controller name
            source = context.Controller.ToString();

            // add action name if any
            source += "." + context.ActionDescriptor.ActionName + "(" + string.Join(", ", context.ActionDescriptor.GetParameters().Select(x => x.ParameterType)) + ")";
#endif

            AddItemToList(GetHttpContextItem(context.HttpContext, KEY_STYLES), style, source, insertMode);
            return null;

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
            string script = System.Web.Optimization.Scripts.Render(filePath).ToHtmlString();

            string source = null;
#if DEBUG
            // add controller name
            source = context.Controller.ToString();

            // add action name if any
            source += "." + context.ActionDescriptor.ActionName + "(" + string.Join(", ", context.ActionDescriptor.GetParameters().Select(x => x.ParameterType)) + ")";
#endif
            var bodyScripts = GetHttpContextItem(context.HttpContext, KEY_BODY_SCRIPTS);
            BundleModel tmp = null;
            if ((tmp = Find(bodyScripts, script, true)) != null)
            {
                bodyScripts.Remove(tmp);
            }
            AddItemToList(GetHttpContextItem(context.HttpContext, KEY_HEAD_SCRIPTS), script, source, insertMode);

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
            string script = System.Web.Optimization.Scripts.Render(filePath).ToHtmlString();

            string source = null;
#if DEBUG
            // add controller name
            source = context.Controller.ToString();

            // add action name if any
            source += "." + context.ActionDescriptor.ActionName + "(" + string.Join(", ", context.ActionDescriptor.GetParameters().Select(x => x.ParameterType)) + ")";
#endif
            var headScripts = GetHttpContextItem(context.HttpContext, KEY_HEAD_SCRIPTS);
            if (Find(headScripts, script, true) == null)
            {
                AddItemToList(GetHttpContextItem(context.HttpContext, KEY_BODY_SCRIPTS), script, source, insertMode);
            }

            return null; // just for razor syntax
        }
    }
}