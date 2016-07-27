<%@ Control Language="C#" %>
<link href="/Styles/bootstrap.min.css" rel="stylesheet" type="text/css" />
<link href="/Styles/bootstrap-select.min.css" rel="stylesheet" type="text/css" />
<link href="/Styles/font-awesome.min.css" rel="stylesheet" type="text/css" />
<link href="/Styles/bootstrap-spinedit.css" rel="stylesheet" type="text/css" />
<link href="/Styles/datepicker.css" rel="stylesheet" type="text/css" />
<link rel="stylesheet" type="text/css" href="/Styles/Site.css" />

<!-- API Scripts -->
<script type="text/javascript" src="/Scripts/jquery/jquery-1.10.2.min.js"></script>
<script type="text/javascript" src="/Scripts/jquery/jquery.blockUI.js"></script>
<script type="text/javascript" src="/Scripts/jquery/jquery.ajax-progress.js"></script>
<script type="text/javascript" src="/Scripts/jquery/jquery.hint.js"></script>
<script type="text/javascript" src="/Scripts/jquery/jquery.fileDownload.js"></script>
<script type="text/javascript" src="/Scripts/jquery/jquery.highlight.js"></script>
<script type="text/javascript" src="/Scripts/jquery/jquery.cookie.js"></script>
<script type="text/javascript" src="/Scripts/jquery/jquery.scrollTo.min.js"></script>

<script type="text/javascript" src="/Scripts/bootstrap/bootstrap.js"></script>
<script type="text/javascript" src="/Scripts/bootstrap/bootstrap-select.min.js"></script>
<script type="text/javascript" src="/Scripts/bootstrap/bootstrap-spinedit.js"></script>
<script type="text/javascript" src="/Scripts/bootstrap/bootstrap-datepicker.js"></script>

<script type="text/javascript" src="/Scripts/Mvc/MicrosoftAjax.js"></script>
<script type="text/javascript" src="/Scripts/Mvc/MicrosoftMvcAjax.js"></script>
<script type="text/javascript" src="/Scripts/Mvc/MicrosoftMvcValidation.js"></script>
<script type="text/javascript" src="/Scripts/knockout/knockout-2.2.1.js"></script>
<script type="text/javascript" src="/Scripts/knockout/knockout.mapping.2.4.1.js"></script>
<script type="text/javascript" src="/Scripts/knockout/knockout.programmatic.js"></script>
<script type="text/javascript" src="/Scripts/knockout/knockout.validation.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
<script type="text/javascript" src="/Scripts/knockout/knockout.hint.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
<script type="text/javascript" src="/Scripts/knockout/knockout.helpContext.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
<script type="text/javascript" src="/Scripts/knockout/knockout.tooltip.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
<script type="text/javascript" src="/Scripts/knockout/knockout.localization.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
<script type="text/javascript" src="/Scripts/knockout/knockout.highlight.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
<script type="text/javascript" src="/Scripts/knockout/knockout.templateSelect.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
<script type="text/javascript" src="/Scripts/knockout/knockout.valuesetSelect.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
<script type="text/javascript" src="/Scripts/knockout/knockout.spinedit.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
<script type="text/javascript" src="/Scripts/knockout/knockout.file.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
<script type="text/javascript" src="/Scripts/knockout/knockout.date.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>

<script type="text/javascript" src="/Scripts/Sha1.js"></script>
<script type="text/javascript" src="/Scripts/q/q.js"></script>

<!-- Custom Trifolia knockout bindings -->
<script type="text/javascript" src="/Scripts/knockout/knockout.datagrid.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
<script type="text/javascript" src="/Scripts/knockout/knockout.file.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
<script type="text/javascript" src="/Scripts/knockout/knockout.validation.rules.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>

<!-- Application Scripts -->
<script type="text/javascript" src="/Scripts/utils.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
<script type="text/javascript" src="/Scripts/PermissionManagement.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
<script type="text/javascript" src="/Scripts/SupportRequestHandler.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
<script type="text/javascript" src="/Scripts/helpModel.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>

<!-- Localization -->
<script type="text/javascript" src="/api/Localization?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>

<!-- Config -->
<script type="text/javascript" src="/api/Admin/Config?isRef=True&<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
