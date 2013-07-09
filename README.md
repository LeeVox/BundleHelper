BundleHelper
============
Helper for auto removing duplicate css, js...

 * [Why BundleHelper?](#why-bundlehelper)
 * [How to use](#how-to-use)
 * [How to install](#how-to-install)
 * [Change Logs](#change-logs)

Why BundleHelper?
------------
If you need to use a lot of partial views, child actions... and each requires some javascripts, stylesheets... for working.
 * __Without BundleHelper__, you can solve that issue by those solutions:
  1. _Include javascripts/stylesheets... inside partial views._ But your page will render a lot of duplicated scripts/css and can lead to a conflict.
  2. _Include javascripts/stylesheets... only in the parent views._ But you need remember to include them when create new views.

 * __With BundleHelper__, you just include all required scripts/stylesheets in your partial views or action views... This tool will help you remove duplicated entries, place them into correct place of page (end of head or body tag), so developers can maintain code easier and client can browse page faster.

How to use
------------
* __[REQUIRED]__ You must add those code to master layout file (default: ~/View/Shared/_Layout.cshtml)

```html
<html>
 <head>
  <!-- content of head -->
  
  @Html.RenderStyles() <!-- render stylesheets -->
  @Html.RenderHeadScripts() <!-- should render javascript frameworks at the end of head tag -->
 </head>
 <body>
  <!-- content of body -->
  
  @Html.RenderBodyScripts() <!-- should render javascripts at the end of body -->
 </body>
</html>
```

* In your views or partial views... refer to external javascripts/stylesheets by those functions:

```csharp
@Html.AddStyle("~/Content/Site.css")
@Html.AddStyle("~/Content/css")

@Html.AddHeadScript("~/bundles/jquery")
@Html.AddHeadScript(@<script>alert('this is script on head tag');</script>)

@Html.AddBodyScript(@<script>alert('this is script at end of body tag');</script>)
@Html.AddBodyScript("~/Scripts/HelloWorld.js")
```

* You can also add directly javascripts/css into your views (not recommended):

```html
@Html.AddStyle(@<style>body { background-color:red; }</style>)

@Html.AddHeadScript(@<script>alert('this is BundleHelper');</script>)

@Html.AddBodyScript(
 @<div>
  <script>
   alert('we can add multiple scripts at one time...');
  </script>
  <script>
   alert('...like this.');
  </script>
 </div>)
```
 
* If you need to include some javascripts/stylesheets for all views of a controller but not for other controllers. Add those attributes for that controller:

```csharp
[UsingStyles("~/Content/Site.css", "~/Content/css")]
[UsingHeadScripts("~/bundles/jquery", "~/Scripts/Global.js")]
[UsingBodyScripts("~/bundles/jquery", "~/Scripts/HelloWorld.js")]
public class BookController : Controller
{
  // code here
}
```

How to install
------------
+ You can install it easily via Nuget: https://www.nuget.org/packages/BundleHelper/
+ Or just copy those files into your project (put them to any folder you want)
 - BundleHelper.cs
 - InlineBundleHelper.cs
 - UsingAttributes.cs

Change Logs
------------
 *      Version			Date            Description
 
 *      2.0 Build 2		Jul-08-2013     Just updated for new format of version's build numer.
 *      2.0 Build 1		Jul-03-2013		Updated some minor changes related to dependencies...
 *      2.0				Jul-01-2013     Added feature compress inline javascripts, inline css...
 *      1.2 Build 1		Jun-28-2013     Updated for some minor enhancements
 *      1.2				Jun-27-2013     Refactored code
 *      1.1 Build 5		Jun-26-2013     Fixed to ignore case for checking duplicated items
 *      1.1				Jun-26-2013     Updated for supporting Using Attributes
 *      1.0				Jun-25-2013     First draft
