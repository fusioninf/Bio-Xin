Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web

Public Class DTS_MODEL_SOUPRDER_ITEMS
    Public Property Type As String '--C- CreditMemo,I - Invoice
    Public Property VisOrder As String
    Public Property BaseType As String
    Public Property BaseEntry As Integer
    Public Property BaseLine As Integer
    Public Property DocDueDate As String
    Public Property ItemCode As String
    Public Property Quantity As Decimal
    Public Property WareHouse As String
    Public Property WhsCode As String


    Public Property SequenceNo As Integer
    Public Property PriceBeforeDiscount As Decimal
    Public Property DiscountPercentage As Decimal
    Public Property TaxCode As String
    Public Property UOM As String
    Public Property Discountamount As Decimal
End Class
