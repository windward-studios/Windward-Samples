﻿<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FormDB2
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
        Me.GenerateButton = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'GenerateButton
        '
        Me.GenerateButton.Location = New System.Drawing.Point(62, 37)
        Me.GenerateButton.Name = "GenerateButton"
        Me.GenerateButton.Size = New System.Drawing.Size(122, 38)
        Me.GenerateButton.TabIndex = 0
        Me.GenerateButton.Text = "Generate DB2 report"
        Me.GenerateButton.UseVisualStyleBackColor = True
        '
        'FormDB2
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(265, 123)
        Me.Controls.Add(Me.GenerateButton)
        Me.Name = "FormDB2"
        Me.Text = "Windward VB DB2 Example"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents GenerateButton As System.Windows.Forms.Button

End Class
