<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DisplayImgs.aspx.cs" Inherits="WcfRestService3.DisplayImgs" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Images of Transactors</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    <h1>Transactors ...
        <asp:LinkButton ID="LinkButton1" runat="server" onclick="LinkButton1_Click">Kill em ALL</asp:LinkButton>
&nbsp;<asp:LinkButton ID="LinkButton2" runat="server" onclick="LinkButton2_Click">Refresh</asp:LinkButton>
        </h1>
    <p></p>
    </div>
    <asp:DataList ID="DataList1" runat="server">
    <ItemTemplate>
        <asp:Image ID="Image1" runat="server" AlternateText='<%# Eval("Name","~/pics/{0}") %>' 
            ImageUrl='<%# Eval("Name","~/pics/{0}") %>'  />
            <asp:Label ID="Label1" runat="server" Text='<%# Eval("Name","{0}") %>' ></asp:Label>
    </ItemTemplate>
    </asp:DataList>
    </form>
</body>
</html>
