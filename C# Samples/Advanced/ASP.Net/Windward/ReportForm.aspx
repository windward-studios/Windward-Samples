<%@ Page Language="C#" AutoEventWireup="true" CodeFile="ReportForm.aspx.cs" Inherits="ReportForm" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
    <head runat="server">
        <title>Windward Reports Demonstration</title>
        <style type="text/css">
            div {
                text-align:center;
            }
            tr {
                text-align:left;
            }
            table {
                margin-left:auto;
                margin-right:auto;
            }
            .floatRight {
                float:right;
            }
        </style>
    </head>
    <body>
        <p class="floatRight"><em>.NET Engine Demo</em></p>
        <img src="images/Windward.png" width="260" height="43" alt="Run a sample report" />
        <div>
            <h3>Windward Personnel Leave of Absence</h3>
            <form id="form1" method="post" runat="server">
                <p>Leave Request</p>
                <p>
                    <asp:DropDownList ID="listVar" runat="server" />
                </p>
                <table>
                    <tr>
                        <td>
                            <asp:RadioButton ID="rbPdf" runat="server" GroupName="format" Checked="True" />
                            Adobe PDF
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:RadioButton ID="rbDocx" runat="server" GroupName="format" />
                            Microsoft Word
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:RadioButton ID="rbCss" runat="server" GroupName="format" />
                            HTML
                        </td>
                    </tr>
                </table>
                <p>
                    <asp:Button ID="btnSubmit" runat="server" Text="Create Letter" OnClick="btnSubmit_Click" />
                </p>
            </form>
        </div>
    </body>
</html>
