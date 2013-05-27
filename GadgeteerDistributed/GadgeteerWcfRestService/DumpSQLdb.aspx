<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DumpSQLdb.aspx.cs" Inherits="GadgeteerWcfRestService.DumpSQLdb" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>DUMP SQL Database Content including pics ...</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    
        <asp:LinkButton ID="LinkButton1" runat="server" onclick="LinkButton1_Click">Kill em all ...</asp:LinkButton>
    
    </div>
    <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False" 
        DataKeyNames="ID" DataSourceID="EntityDataSource1" AllowSorting="True">
        <Columns>
            <asp:BoundField DataField="ID" HeaderText="ID" ReadOnly="True" 
                SortExpression="ID" />
            <asp:BoundField DataField="lightSENS" HeaderText="lightSENS" 
                SortExpression="lightSENS" />
            <asp:BoundField DataField="humSENS" HeaderText="humSENS" 
                SortExpression="humSENS" />
            <asp:BoundField DataField="tempSENS" HeaderText="tempSENS" 
                SortExpression="tempSENS" />
            <asp:BoundField DataField="Date" HeaderText="Date" SortExpression="Date" />
                        <asp:TemplateField HeaderText="Transactor pic" >
            <ItemTemplate>
                <img src="TPHandler.ashx?Id=<%# Eval("ID") %>" alt="<%#Eval("ID") %>"/>
            </ItemTemplate>
            </asp:TemplateField>

        </Columns>
    </asp:GridView>
    <asp:EntityDataSource ID="EntityDataSource1" runat="server" 
        ConnectionString="name=TDataEntities" DefaultContainerName="TDataEntities" 
        EnableFlattening="False" EntitySetName="TDataItems">
    </asp:EntityDataSource>
    </form>
</body>
</html>
