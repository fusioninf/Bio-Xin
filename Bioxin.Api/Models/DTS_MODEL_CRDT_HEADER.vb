Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web

Public Class DTS_MODEL_CRDT_HEADER
    Public Property CardCode As String
    Public Property Branch As String
    Public Property PostingDate As String
    Public Property DocDueDate As String
    Public Property RefNo As String
    Public Property RefDate As String
    Public Property Remarks As String
    Public Property SalesEmployee As String
    Public Property ItemType As String
    Public Property Items As DTS_MODEL_CRDT_ITEMS()
    Public Property PaymentDetails As DTS_MODEL_PMNT_DTLS()
End Class

