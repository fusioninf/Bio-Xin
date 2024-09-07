Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web

Public Class DTS_MODEL_GR_ITEMS
    Public Property VisOrder As Integer
    Public Property ItemCode As String
    Public Property UOM As String
    Public Property Quantity As Double
    Public Property Price As Double
    'Public Property CostCenter1 As String
    Public Property EmployeeCostCenter As String
    Public Property DepartmentCostCenter As String
    Public Property MachineCostCenter As String
    Public Property Remarks As String
    'Public Property CostCenter5 As String
    Public Property Batches As DTS_MODEL_GR_BATCH()
    Public Property Serial As DTS_MODEL_GR_SERIAL()
End Class
