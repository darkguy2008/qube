<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Forms.aspx.cs" Inherits="Forms" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <script type="text/javascript" src="https://code.jquery.com/jquery-1.11.3.min.js"></script>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    
        <asp:ValidationSummary runat="server" ID="vSummary" />

        <q:QubeStandardForm runat="server" ID="frm">
            <q:QubeStandardPanel runat="server">

                <q:QubeFormBaseField runat="server" ID="field1" Type="Alphanumeric" Required="true" PlaceHolder="test" DisplayName="Omg!" />

            </q:QubeStandardPanel>
        </q:QubeStandardForm>

        <input type="button" onclick="javascript:test()" />
        <script type="text/javascript">
            function test() {
                $("#frmfield1").val("OMG");
            }
        </script>

    </div>
    </form>
</body>
</html>
