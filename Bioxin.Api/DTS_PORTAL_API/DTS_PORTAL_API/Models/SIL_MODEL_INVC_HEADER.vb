Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web

Public Class SIL_MODEL_INVC_HEADER
    Public Property CardCode As String
    Public Property Branch As String
    Public Property PostingDate As String
    Public Property DocDueDate As String
    Public Property RefNo As String
    Public Property RefDate As String
    Public Property Remarks As String
    Public Property SalesEmployee As String
    Public Property PaymentAccountCode As String
    Public Property ToBranch As String
    Public Property ItemType As String
    Public Property BaseEntry As String
    Public Property Items As SIL_MODEL_INVC_ITEMS()
    Public Property PaymentDetails As DTS_MODEL_PMNT_DTLS()
End Class

