<%@ Page Language="C#" %>
<script runat="server">
public void Page_Load()
{
    Label1.Text = "Generated by C# codebehind in a self hosted server";
}
</script>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<html>
	<head>
		<title></title>
	</head>
	<body>
        <asp:Label ID="Label1" runat="server" Text="Label"></asp:Label>
	</body>
</html>