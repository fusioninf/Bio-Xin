Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web

Public Class DTS_MODEL_ITRNR_HEADER
    Public Property CardCode As String
    Public Property PostingDate As String
    Public Property DueDate As String
    Public Property RefDate As String
    Public Property ShiptoCode As String
    Public Property FromWarehouse As String
    Public Property ContactPerson As String
    Public Property Remarks As String
    Public Property BaseEntry As String
    Public Property SalesEmployee As String
    Public Property ApprovedBy As String
    Public Property ApprovedDate As String
    Public Property SOEntry As String
    Public Property Items As DTS_MODEL_ITRND_ITEMS()
End Class

