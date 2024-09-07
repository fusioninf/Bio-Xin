Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web

Public Class SIL_MODEL_JOURNAL_HEADER
    Public Property PostingDate As String
    Public Property Remarks As String
    Public Property AccountCode As String
    Public Property TotalValue As Decimal
    Public Property Items As SIL_MODEL_JOURNAL_DETAILS()
End Class

