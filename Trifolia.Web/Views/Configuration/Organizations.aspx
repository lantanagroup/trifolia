<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Organizations.aspx.cs" Inherits="TemplateDatabase.Web.Account.OrganizationManagement.Organizations" %>
<%@ Register assembly="DevExpress.Web.ASPxGridView.v12.1, Version=12.1.7.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" namespace="DevExpress.Web.ASPxGridView" tagprefix="dx" %>
<%@ Register assembly="DevExpress.Web.ASPxEditors.v12.1, Version=12.1.7.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" namespace="DevExpress.Web.ASPxEditors" tagprefix="dx" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h1>
    Organizations</h1>
    <p>
        <dx:ASPxLabel ID="MessageLabel" runat="server" Font-Bold="True" ForeColor="Red" 
            Text="N/A" Visible="False">
        </dx:ASPxLabel>
    </p>
<p>
    <dx:ASPxGridView ID="ASPxGridView1" runat="server" AutoGenerateColumns="False" 
        ClientInstanceName="organizationsGrid" DataSourceID="OrganizationSource" 
        Width="100%" KeyFieldName="Id">
        <Columns>
            <dx:GridViewCommandColumn Caption=" " VisibleIndex="6" Width="100px">
                <EditButton Visible="True">
                </EditButton>
                <DeleteButton Visible="True">
                </DeleteButton>
                <HeaderTemplate>
                    <table cellpadding="0" cellspacing="0" class="style1">
                        <tr>
                            <td align="center">
                                <dx:ASPxButton ID="ASPxButton1" runat="server" AutoPostBack="False" Text="New" 
                                    Width="50px">
                                    <ClientSideEvents Click="function(s, e) {
	organizationsGrid.AddNewRow();
}" />
                                </dx:ASPxButton>
                            </td>
                        </tr>
                    </table>
                </HeaderTemplate>
            </dx:GridViewCommandColumn>
            <dx:GridViewDataCheckColumn FieldName="IsNew" Visible="False" VisibleIndex="0">
            </dx:GridViewDataCheckColumn>
            <dx:GridViewDataTextColumn FieldName="Id" Visible="False" VisibleIndex="1">
            </dx:GridViewDataTextColumn>
            <dx:GridViewDataTextColumn FieldName="Name" VisibleIndex="2" Width="300px">
                <PropertiesTextEdit>
                    <ValidationSettings>
                        <RequiredField ErrorText="Name is required" IsRequired="True" />
                    </ValidationSettings>
                </PropertiesTextEdit>
            </dx:GridViewDataTextColumn>
            <dx:GridViewDataTextColumn FieldName="ContactName" VisibleIndex="3">
            </dx:GridViewDataTextColumn>
            <dx:GridViewDataTextColumn FieldName="ContactPhone" Visible="False" 
                VisibleIndex="4">
                <PropertiesTextEdit>
                    <ValidationSettings>
                        <RegularExpression ErrorText="The value entered is not a phone number" 
                            ValidationExpression="^(\(?\d\d\d\)?)?( |-|\.)?\d\d\d( |-|\.)?\d{4,4}(( |-|\.)?[ext\.]+ ?\d+)?$" />
                    </ValidationSettings>
                </PropertiesTextEdit>
                <EditFormSettings Visible="True" />
            </dx:GridViewDataTextColumn>
            <dx:GridViewDataTextColumn FieldName="ContactEmail" Visible="False" 
                VisibleIndex="5">
                <PropertiesTextEdit>
                    <ValidationSettings>
                        <RegularExpression ErrorText="The email is not a valid email address" 
                            ValidationExpression="^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$" />
                    </ValidationSettings>
                </PropertiesTextEdit>
                <EditFormSettings Visible="True" />
            </dx:GridViewDataTextColumn>
        </Columns>
    </dx:ASPxGridView>
</p>
<table cellpadding="5" cellspacing="0" class="style1">
    <tr>
        <td align="right">
            <dx:ASPxButton ID="SaveButton" runat="server" onclick="SaveButton_Click" 
                Text="Save">
            </dx:ASPxButton>
        </td>
        <td width="100">
            <dx:ASPxButton ID="CancelButton" runat="server" onclick="CancelButton_Click" 
                Text="Cancel">
            </dx:ASPxButton>
        </td>
    </tr>
</table>
<p>
    <asp:ObjectDataSource ID="OrganizationSource" runat="server" 
        DataObjectTypeName="TemplateDatabase.Web.Account.OrganizationManagement.Organizations+OrganizationDisplay" 
        DeleteMethod="DeleteOrganization" InsertMethod="AddOrganization" 
        SelectMethod="GetOrganizations" 
        TypeName="TemplateDatabase.Web.Account.OrganizationManagement.Organizations+OrganizationDisplay" 
        UpdateMethod="UpdateOrganization"></asp:ObjectDataSource>
</p>
</asp:Content>
