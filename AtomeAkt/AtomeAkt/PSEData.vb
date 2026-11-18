Public Class PSEData

    Public Sub New()
        Dim myData As String = My.Resources.pse.ToString
        Dim myRows() As String = myData.Split(vbLf)
        sPSEData = myRows.Select(Function(x) x.Split(",")).ToArray
    End Sub

    Private sPSEData As Array
    Public Property PSEData() As Array
        Get
            Return sPSEData
        End Get
        Set(ByVal value As Array)
            sPSEData = value
        End Set
    End Property

End Class
