<%@ Control Language="C#" %>
<link href="/Styles/bootstrap.min.css" rel="stylesheet" type="text/css" />
<link href="/Styles/bootstrap-select.min.css" rel="stylesheet" type="text/css" />
<link href="/Styles/font-awesome.min.css" rel="stylesheet" type="text/css" />
<link href="/Styles/bootstrap-spinedit.css" rel="stylesheet" type="text/css" />
<link href="/Styles/datepicker.css" rel="stylesheet" type="text/css" />
<link rel="stylesheet" type="text/css" href="/Styles/Site.css" />

<!-- API Scripts -->
<script type="text/javascript" src="/Scripts/lib/jquery/jquery-1.10.2.min.js"></script>
<script type="text/javascript" src="/Scripts/lib/jquery/jquery.blockUI.js"></script>
<script type="text/javascript" src="/Scripts/lib/jquery/jquery.ajax-progress.js"></script>
<script type="text/javascript" src="/Scripts/lib/jquery/jquery.hint.js"></script>
<script type="text/javascript" src="/Scripts/lib/jquery/jquery.fileDownload.js"></script>
<script type="text/javascript" src="/Scripts/lib/jquery/jquery.highlight.js"></script>
<script type="text/javascript" src="/Scripts/lib/jquery/jquery.cookie.js"></script>
<script type="text/javascript" src="/Scripts/lib/jquery/jquery.scrollTo.min.js"></script>

<script type="text/javascript" src="/Scripts/lib/bootstrap/bootstrap.js"></script>
<script type="text/javascript" src="/Scripts/lib/bootstrap/bootstrap-select.min.js"></script>
<script type="text/javascript" src="/Scripts/lib/bootstrap/bootstrap-spinedit.js"></script>
<script type="text/javascript" src="/Scripts/lib/bootstrap/bootstrap-datepicker.js"></script>

<script type="text/javascript" src="/Scripts/lib/moment-with-locales.min.js"></script>
<script type="text/javascript" src="/Scripts/lib/lodash.min.js"></script>
<script type="text/javascript" src="/Scripts/Mvc/MicrosoftAjax.js"></script>
<script type="text/javascript" src="/Scripts/Mvc/MicrosoftMvcAjax.js"></script>
<script type="text/javascript" src="/Scripts/Mvc/MicrosoftMvcValidation.js"></script>
<script type="text/javascript" src="/Scripts/lib/knockout/knockout-2.2.1.js"></script>
<script type="text/javascript" src="/Scripts/lib/knockout/knockout.mapping.2.4.1.js"></script>
<script type="text/javascript" src="/Scripts/lib/knockout/knockout.programmatic.js"></script>
<script type="text/javascript" src="/Scripts/lib/knockout/knockout.validation.js"></script>
<script type="text/javascript" src="/Scripts/knockout.templateSelect.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
<script type="text/javascript" src="/Scripts/knockout.valuesetSelect.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
<script type="text/javascript" src="/Scripts/knockout.validation.rules.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
<script type="text/javascript" src="/Scripts/knockout.extensions.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>

<script type="text/javascript" src="/Scripts/lib/Sha1.js"></script>
<script type="text/javascript" src="/Scripts/lib/q/q.js"></script>

<script type="text/javascript" src="/Scripts/lib/angular/angular.min.js"></script>
<script type="text/javascript" src="/Scripts/lib/angular/angular-cookies.js"></script>
<script type="text/javascript" src="/Scripts/lib/angular/ui-bootstrap-tpls-2.5.0.min.js"></script>

<!-- Application Scripts -->
<script type="text/javascript" src="/Scripts/angular/startup.js"></script>
<script type="text/javascript" src="/Scripts/angular/services.js"></script>
<script type="text/javascript" src="/Scripts/utils.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
<script type="text/javascript" src="/Scripts/PermissionManagement.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
<script type="text/javascript" src="/Scripts/SupportRequestHandler.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
<script type="text/javascript" src="/Scripts/helpModel.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>

<!-- Localization -->
<script type="text/javascript" src="/api/Localization?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>

<!-- Config -->
<script type="text/javascript" src="/api/Admin/Config?isRef=True&<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
