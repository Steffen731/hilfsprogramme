Option Strict On
Option Explicit On

Imports System
Imports System.Windows.Forms
Imports AtomeAkt.Desktop.UI

Friend Module Program
    <STAThread>
    Public Sub Main()
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)
        Application.Run(New MainForm())
    End Sub
End Module
