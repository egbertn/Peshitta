<%@ page title="" language="C#" masterpagefile="~/Book.master" autoeventwireup="true" inherits="peshitta.nl.Book"  CodeBehind="~/Book.aspx.cs" enableviewstate="false" %>

<asp:Content ID="Content1" ContentPlaceHolderID="NewBibleContent" Runat="Server">
<ctrl:ctBook runat="server" ID="addBook" HyperLinkCss="amen" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="NewAltContent" Runat="Server">
</asp:Content>

