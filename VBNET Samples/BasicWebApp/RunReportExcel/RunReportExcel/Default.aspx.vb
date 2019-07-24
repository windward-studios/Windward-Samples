Public Class _Default
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub

    Protected Sub btnRunReport_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnRunReport.Click
        'DisplayReport.aspx will generate the report.
        Response.Redirect("DisplayReport.aspx")
    End Sub
End Class