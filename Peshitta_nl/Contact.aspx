<%@ page language="C#" autoeventwireup="true" title="Contacteer redactie" inherits="peshitta.nl.Contact"  CodeBehind="~/Contact.aspx.cs" masterpagefile="~/MasterPage.master" enableviewstate="false" %>
<asp:Content ID="Content1" ContentPlaceHolderID="BibleContent"  runat="server">

<div class="contents">
<h2>Contacteer redactie</h2>

    <table style="border:none; margin-top:100px" summary="Uw contact gegevens">
           <tr style="display:none" id="msgLine" runat="server">
            <td colspan="2" style="height: 52px; text-align:left" >
                <asp:ValidationSummary ID="ValidationSummary1" runat="server" Height="12px" ValidationGroup="one"
                    Width="347px"/>
                    <asp:RequiredFieldValidator ID="fullNameRequired" runat="server" ControlToValidate="FullName"
                    ErrorMessage="Geef een naam" Display="None" ValidationGroup="one"></asp:RequiredFieldValidator>
                    <asp:RegularExpressionValidator ID="validEmail" runat="server" ControlToValidate="Email"
                    ErrorMessage="Geef een geldig e-mail." ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" Display="None" ValidationGroup="one"/>
            </td>
        </tr>        
        <tr>
            <td style="width: 75px">
                <asp:Label ID="lblFullName" runat="server" AccessKey="F" Text="Naam"/></td>
            <td style="width: 200px;" >
                <asp:TextBox ID="FullName" AutoCompleteType="DisplayName" runat="server" TabIndex="1" MaxLength="60"/></td>
        </tr>
        <tr >
            <td style="width: 75px" >
                <asp:Label ID="lblPhone" runat="server" AccessKey="P" AssociatedControlID="Phone"
                    Text="Telefoon"/></td>
            <td style="width: 200px" >
                <asp:TextBox ID="Phone" AutoCompleteType="homephone" runat="server" TabIndex="2" MaxLength="14"/>
                <asp:CustomValidator ID="ValidPhone" runat="server" ControlToValidate="Phone" Display="Dynamic"
                    ErrorMessage="Vul een geldig telefoon in" 
                    SetFocusOnError="True" ValidateEmptyText="True"/></td>
        </tr>
        <tr >
            <td style="width: 75px" >
                <asp:Label ID="lblEmail" runat="server" AccessKey="E" AssociatedControlID="Email"
                    Text="E-mail"/></td>
            <td style="width: 200px" >
                <asp:TextBox ID="Email" AutoCompleteType="email" runat="server" TabIndex="3" Width="149px" MaxLength="255"/>                
                </td>
        </tr>
        <tr>
            <td  style="width: 75px">
                <asp:Label ID="lblSubject" runat="server" AccessKey="U" AssociatedControlID="Subject"
                    Text="Onderwerp"/></td>
            <td style="width: 200px" >
                <asp:TextBox ID="Subject" runat="server" TabIndex="4" MaxLength="100"/>
            </td>
        </tr>
        <tr >
            <td  style="width: 75px" class="leftb">
            &nbsp;
            </td>
            <td style="width: 200px" class="rightb">
                <asp:Button ID="btnContact" runat="server" Text="Neem contact met mij op!" /></td>
        </tr>
		<tr >
            <td  style="width: 200px">&nbsp;</td>
        </tr>
		</table>

        <table>
				<tr >
            <td  style="width: 345px">
                <asp:Label ID="lblMessage" runat="server" Text="<b>Dank u. We zullen spoedig contact met u opnemen.</b>"
                    Visible="False"/></td>
        </tr>
        
    </table>
</div>

</asp:Content>