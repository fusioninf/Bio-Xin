Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web

Public Class DTS_MODEL_TRNS_HEADER
    Public Property CardCode As String
    Public Property PostingDate As String
    Public Property DueDate As String
    Public Property RefDate As String
    Public Property ShiptoCode As String
    Public Property ContactPerson As String
    Public Property SalesEmployee As String
    Public Property FromWarehouse As String
    Public Property ToWareHouse As String
    Public Property Remarks As String
    Public Property EnteredBy As String
    Public Property Items As DTS_MODEL_TRNS_ITEMS()
End Class

