<%@ Page Language="VB" AutoEventWireup="false" CodeFile="ProcessMachineData.aspx.vb" Inherits="ProcessMachineData" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">

    <table>
        <tr><td>Comparison sequence:</td><td><asp:FileUpload ID="FileUpload1" runat="server" /></td></tr>
        <tr><td>Reference sequence:</td><td><asp:FileUpload ID="FileUpload2" runat="server" /></td></tr> 
    </table>

	<br />
	<br />
	<asp:Button ID="Process_Button" runat="server" Text="Go" Width="223px" />
	<br />
    Adjust cutoff: <asp:TextBox ID="TextBox1" runat="server" ></asp:TextBox><asp:Button ID="UpdateButton" runat="server" Text="Update" ValidateRequestMode="Enabled" />
    <asp:RegularExpressionValidator ID="RegularExpressionValidator1" ControlToValidate="TextBox1" runat="server" ErrorMessage="Must be a numerical value or a range" ValidationExpression="[\s\d]+\-?[\s\d]*"></asp:RegularExpressionValidator>
        <br />
        <div>

            <asp:Chart ID="Chart1" runat="server" BorderlineWidth="1" BorderlineColor="Black" EnableViewState="true" ChartType="Point">
                <ChartAreas>
                    <asp:ChartArea Name="MainChartArea">
                        <AxisX >
                            <StripLines>
                                <asp:StripLine TextAlignment="Near" BorderDashStyle="Solid" BorderColor="red" BorderWidth="4" BackColor="red" IntervalOffset="-1" />
                            </StripLines>
                        </AxisX>
                    </asp:ChartArea>
                </ChartAreas>
            </asp:Chart>

        </div>

        <span ID="myspan" runat="server"></span>

    </form>
</body>
</html>