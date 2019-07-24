<%@ Page Language="C#" AutoEventWireup="true" CodeFile="BasicReportForm.aspx.cs" Inherits="ReportForm" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
    <head runat="server">
        <title>Windward Engine Demonstration</title>
        <style type="text/css">
            td {
                width:200px;
            }
            .floatRight {
                float:right;
                text-align:right;
            }
        </style>
    </head>
    <body>
        <p class="floatRight"> .NET Engine Demo </p>
        <img src="images/Windward.png" width="260" height="43" alt="Run a sample report" />
        <div align="center">
            <h3>Windward Personnel Leave of Absence</h3>
            <form id="form1" method="post" runat="server">
                <table border="1">
                    <tr align="left">
                        <td>Leave Request Id:</td>
                        <td>#1</td>
                    </tr>
                    <tr align="left">
                        <td>Employee Name:</td>
                        <td>Maria Anders</td>
                    </tr>
                    <tr align="left">
                        <td>Employee ID:</td>
                        <td>12209</td>
                    </tr>
                    <tr align="left">
                        <td>Manager Name:</td>
                        <td>Hanna Moos</td>
                    </tr>
                    <tr align="left">
                        <td>Manager Email:</td>
                        <td>hannam@ng.com</td>
                    </tr>
                    <tr align="left">
                        <td>Leave Start Date:</td>
                        <td>5/12/2009</td>
                    </tr>
                    <tr align="left">
                        <td>Leave End Date:</td>
                        <td>5/27/2009</td>
                    </tr>
                </table>
                <p>
                    <asp:Button ID="btnSubmit" runat="server" Text="Create Letter" OnClick="btnSubmit_Click" />
                </p>
            </form>
        </div>
    </body>
</html>