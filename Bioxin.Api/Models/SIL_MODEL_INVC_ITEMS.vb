Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web

Public Class SIL_MODEL_INVC_ITEMS
    Public Property VisOrder As Integer
    Public Property BaseType As String
    Public Property BaseEntry As Long
    Public Property BaseLine As Long
    Public Property ItemCode As String
    Public Property Quantity As Decimal
    'Public Property Price As Decimal
    Public Property DocDueDate As String
    Public Property UOM As String
    Public Property PriceBeforeDiscount As Decimal
    Public Property Discountamount As Decimal
    Public Property TaxCode As String
    Public Property DiscountPercentage As Decimal
    Public Property WhsCode As String
    Public Property VoucherNo As String
    Public Property ValidTill As String
    Public Property Batches As DTS_MODEL_INVC_BATCH()
    Public Property Serial As DTS_MODEL_INVC_SERIAL()
End Class
