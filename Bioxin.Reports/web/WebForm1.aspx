<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WebForm1.aspx.cs" Inherits="CrystalReportCallingFromWeb.web.WebForm1" %>

<%@ Register Assembly="CrystalDecisions.Web, Version=13.0.4000.0, Culture=neutral, PublicKeyToken=692fbea5521e1304" Namespace="CrystalDecisions.Web" TagPrefix="CR" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body style="overflow: auto;">
   <%-- 22222222222
    <div style="height: 400px; width: 600px" id="divReportPDF">
            <iframe id="iframePDF" runat="server" style="height:400px; width:600px;" >
            </iframe>
        </div>
    111111111--%>
    <form id="form1" runat="server">
        <div style="height: 400px; width: 600px" id="dvReport">
            <CR:CrystalReportViewer ID="CrystalReportViewer1" runat="server" AutoDataBind="true" Height="400px" Width="600px"
                EnableDatabaseLogonPrompt="False" EnableParameterPrompt="False" BorderStyle="None"
                DisplayStatusbar="false" HasCrystalLogo="False" BackColor="White" PrintMode="Pdf"
                ToolPanelView="None"></CR:CrystalReportViewer>
        </div>
        
        <%--<div>
            <div class="row">
                <div class="col-sm-4">
                    <asp:Button ID="btnprint" runat="server" Text="print"  onclick="Print()" />
                </div>
                <div class="col-sm-8">12345</div>
            </div>
        </div>--%>
    </form>
    <script type="text/javascript">  
    function Print() {  
        var dvReport = document.getElementById("dvReport");
        alert("1")
        var frame1 = dvReport.getElementsByTagName("iframeReport")[0];  
        alert("2")
        if (navigator.appName.indexOf("Internet Explorer") != -1 || navigator.appVersion.indexOf("Trident") != -1) {  
            frame1.name = frame1.id;  
            window.frames[frame1.id].focus();  
            window.frames[frame1.id].print();  
        } else {  
            var frameDoc = frame1.contentWindow ? frame1.contentWindow : frame1.contentDocument.document ? frame1.contentDocument.document : frame1.contentDocument;  
            frameDoc.print();  
        }  
    }  
    </script>
</body>
</html>
