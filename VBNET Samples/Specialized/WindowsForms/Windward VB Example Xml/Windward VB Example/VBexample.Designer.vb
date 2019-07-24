<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class VBexample
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.GenerateXml = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'GenerateXml
        '
        Me.GenerateXml.Location = New System.Drawing.Point(85, 25)
        Me.GenerateXml.Name = "GenerateXml"
        Me.GenerateXml.Size = New System.Drawing.Size(126, 37)
        Me.GenerateXml.TabIndex = 1
        Me.GenerateXml.Text = "Generate XML report"
        Me.GenerateXml.UseVisualStyleBackColor = True
        '
        'VBexample
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(297, 85)
        Me.Controls.Add(Me.GenerateXml)
        Me.Name = "VBexample"
        Me.Text = "Windward VB example"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents GenerateXml As System.Windows.Forms.Button

End Class
