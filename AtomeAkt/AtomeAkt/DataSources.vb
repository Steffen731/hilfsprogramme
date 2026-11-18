Imports System.Globalization

Public Class DataSources

    Public Sub New()
        Dim myData1 As String = My.Resources.isotope.ToString
        Dim myRows1() As String = myData1.Split(ControlChars.CrLf.ToCharArray, StringSplitOptions.RemoveEmptyEntries)
        sIsotope = myRows1.Select(Function(x) x.Split(",")).ToArray()

        lIsotope.Clear()
        Dim iOZ, iNZ As Integer
        Dim dblH As Double

        For i As Integer = 1 To UBound(sIsotope)
            iOZ = Integer.Parse(sIsotope(i)(0))
            iNZ = Integer.Parse(sIsotope(i)(1))
            dblH = Double.Parse(sIsotope(i)(3), CultureInfo.InvariantCulture)
            If dblH > 0 Then lIsotope.Add({iOZ, iNZ, dblH})
        Next

        Dim myData As String = My.Resources.pse.ToString
        Dim myRows() As String = myData.Split(ControlChars.CrLf.ToCharArray, StringSplitOptions.RemoveEmptyEntries)
        sPSEData = myRows.Select(Function(x) x.Split(",")).ToArray()

    End Sub

    Public ReadOnly ElektronenPerPeriode As Array = {2, 8, 8, 18, 18, 32, 32}
    Public ReadOnly ElektronenBisPeriode As Array = {2, 10, 18, 36, 54, 86, 118}

    Public ReadOnly ElektronenPerSchale As Array = {2, 8, 18, 32, 50}
    Public ReadOnly ElektronenBisSchale As Array = {2, 10, 28, 60, 110}

    Private lIsotope As New List(Of Array)
    Public Property IsotopenListe() As List(Of Array)
        Get
            Return lIsotope
        End Get
        Set(ByVal value As List(Of Array))
            lIsotope = value
        End Set
    End Property

    Private sIsotope As Array
    Public Property IsotopenArray() As Array
        Get
            Return sIsotope
        End Get
        Set(ByVal value As Array)
            sIsotope = value
        End Set
    End Property

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
