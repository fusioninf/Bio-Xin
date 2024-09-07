Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web

Public Class DTS_MODEL_PMNT_DTLS
    Public Property PaymentType As String ' C-Card,S-Cash,U-UPI
    Public Property Bank As String
    Public Property CardNo As String
    'Public Property CardHolderName As String
    Public Property Tranid As String
    Public Property Amount As Decimal
End Class
