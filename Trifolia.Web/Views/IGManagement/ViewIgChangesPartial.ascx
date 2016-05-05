<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<Trifolia.Web.Models.TemplateManagement.DifferenceModel>>" %>

<%
    foreach(Trifolia.Web.Models.TemplateManagement.DifferenceModel lResult in Model)
    {
        var hasChangedFields = lResult.Difference != null && lResult.Difference.ChangedFields.Count > 0;
        var hasChangedConstraints = lResult.Difference != null && lResult.Difference.ChangedConstraints.Count > 0;
%>
    <div>
        <h4><%:lResult.TemplateName %> [<a href="/TemplateManagement/View?templateId=<%= lResult.Id %>">View</a>]</h4>

        <% if (hasChangedFields || hasChangedConstraints) { %>
        <div style="padding-left: 20px;">
            <span class="linkText" onclick="ToggleChanges(this, 'table')">Show Enumerated Changes</span>
            <table border="0" width="100%" cellpadding="5" cellspacing="0" class="dxgvTable_Youthful" style="padding-top: 5px; padding-right: 20px; display: none;">
                <thead>
                    <tr>
                        <td class="dxgvHeader_Youthful">Change Type</td>
                        <td class="dxgvHeader_Youthful">Description</td>
                        <td class="dxgvHeader_Youthful">Old</td>
                        <td class="dxgvHeader_Youthful">New</td>
                    </tr>
                </thead>
                <tbody>
                    <% 
                        if (hasChangedFields) 
                        { 
                            foreach (Trifolia.Generation.Versioning.ComparisonFieldResult lChange in lResult.Difference.ChangedFields)
                            {
                    %>
                    <tr class="dxgvDataRow_Youthful">
                        <td class="dxgv">Meta-Data</td>
                        <td class="dxgv"><%= lChange.Name %></td>
                        <td class="dxgv"><%= lChange.Old %></td>
                        <td class="dxgv"><%= lChange.New %></td>
                    </tr>
                    <%   
                            }
                        }
                    %>
                    <% 
                        if (hasChangedConstraints) 
                        { 
                            foreach (Trifolia.Generation.Versioning.ComparisonConstraintResult lChange in 
                                lResult.Difference.ChangedConstraints.Where(y => y.Type != Trifolia.Generation.Versioning.CompareStatuses.Unchanged))
                            {
                    %>
                    <tr class="dxgvDataRow_Youthful">
                        <td class="dxgv"><%= lChange.Type.ToString() %></td>
                        <td class="dxgv">CONF #: <%= lChange.Number %></td>
                        <td class="dxgv"><%= lChange.OldNarrative %></td>
                        <td class="dxgv"><%= lChange.NewNarrative %></td>
                    </tr>
                    <%   
                            }
                        }
                    %>
                </tbody>
            </table>

            <br />
            <span class="linkText" onclick="ToggleChanges(this, 'div')">Show Inline Changes</span>
            <div style="display: none;">
                <%= lResult.GetInlineConstraintsHtml() %>
            </div>
        </div>
        <% } %>
    </div>
<%} %>