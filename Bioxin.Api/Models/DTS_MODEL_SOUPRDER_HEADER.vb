Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web

Public Class DTS_MODEL_SOUPRDER_HEADER
    Public Property CardCode As String
    Public Property BaseEntry As String
    Public Property UpdateDate As String
    Public Property ExcessAmount As Decimal
    Public Property Items As DTS_MODEL_SOUPRDER_ITEMS()
    Public Property PaymentDetails As DTS_MODEL_PMNT_DTLS()
End Class

