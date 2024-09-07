Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Public Class DTS_DWNPYMT_HEADER
    Public Property CardCode As String
    Public Property PostingDate As String
    Public Property FullAmount As Decimal
    Public Property PaymentDetails As DTS_MODEL_PMNT_DTLS()
End Class
