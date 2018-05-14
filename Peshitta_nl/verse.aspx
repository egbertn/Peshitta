<%@ page language="C#" enableeventvalidation="false" enableviewstate="false" autoeventwireup="true" masterpagefile="~/popup.master" inherits="peshitta.nl.verse" CodeBehind="~/verse.aspx.cs" %>
<asp:Content runat="server" ContentPlaceHolderID="ContentPlaceHolder1">
    
<h6 class="imglabel"><asp:Literal runat="server" ID="_versindication"/></h6>
<p><asp:Literal runat="server" ID="_txt" /></p>
<br />
<asp:Button  runat="server" ID="_btn" Text="Sluiten" />
</asp:Content>