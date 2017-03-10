<%@ Page Language="VB" AutoEventWireup="false" CodeFile="TimelogAverager.aspx.vb" Inherits="TimelogAverager" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    
    </div>
        <asp:FileUpload ID="fileSelector" AllowMultiple="true" runat="server" />
        <br />
        <br />
        <asp:Button ID="Button1" runat="server" Text="Average timelogs" />
        <br />
        <div>

            <asp:Chart ID="Chart1" runat="server" BorderlineWidth="1" BorderlineColor="Black" EnableViewState="true" ChartType="Point">
                <ChartAreas>
                    <asp:ChartArea Name="MainChartArea">
                    </asp:ChartArea>
                </ChartAreas>
            </asp:Chart>

        </div>

        <br />
        <asp:Button ID="Button_Download" runat="server" Text="Download averaged reference log" Enabled="false"/>
        <br />
        <br />
        <span id="Span1" runat="server"></span>

    </form>
</body>
</html>
