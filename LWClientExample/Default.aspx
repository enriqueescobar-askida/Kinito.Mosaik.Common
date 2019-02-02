<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="LWClientExample._Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Untitled Page</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <img id="NLOLogo" alt="" src="nlplogo-small.jpg" />&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
        <hr />
        
            <asp:Label ID="Label1" runat="server" BackColor="White" BorderColor="White" BorderStyle="Solid"
                Font-Bold="True" Font-Size="Large" ForeColor="#FF3300" Text="English to French  Translation"
                Font-Italic="True" Font-Names="Nina" 
            style="left:303px; top: 1px; position: relative"></asp:Label>
        
        <br />
        Enter your text
        <br />
        <textarea id="TextArea1" style="left: 1px; top: 2px; position: relative;" runat="server"
            cols="50" rows="20" enableviewstate="True">  </textarea>
        <textarea id="TextArea2" style="left: 1px; top: 2px; position: relative; height: 321px;"
            runat="server" cols="50" rows="20"></textarea>
        <br />
        <br />
        <input id="translate" type="submit" value="Translate" runat="server" onserverclick="Submit1_ServerClick"
            style="left: 735px; font-size: medium; position: absolute; color: #993300; top: 602px;
            background-color: #ffffcc; height: 31px; width: 120px; font-family: 'Arial Black';"
            size="15"  />
        <br />
        <br />
    </div>
    </form>

</body>
</html>
