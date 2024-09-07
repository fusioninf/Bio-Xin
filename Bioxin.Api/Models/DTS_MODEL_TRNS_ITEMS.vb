Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web

Public Class DTS_MODEL_TRNS_ITEMS
    Public Property VisOrder As String
    Public Property BaseType As String
    Public Property BaseEntry As Integer
    Public Property BaseLine As Integer
    Public Property ItemCode As String
    Public Property FromWareHouse As String
    Public Property ToWareHouse As String
    Public Property Quantity As Double
    Public Property Price As Decimal
    Public Property Remarks As String
    Public Property Batches As DTS_MODEL_TRNS_BATCH()
    Public Property Serial As DTS_MODEL_TRNS_SERIAL()
End Class
