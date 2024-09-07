Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web

Public Class DTS_MODEL_DAYCLOSE_HEADER
    Public Property PostingDate As String
    Public Property CashAmount As Decimal
    Public Property OthersAmount As Decimal
    Public Property ExtraCashAmount As Decimal
    Public Property Remarks As String
    'Public Property EnteredBy As String
    Public Property Items As DTS_MODEL_DAYCLOSE_ITEMS()
End Class

