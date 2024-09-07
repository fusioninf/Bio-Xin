Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web

Public Class DTS_MODEL_SQ_HEADER
    Public Property CardCode As String
    Public Property Branch As String
    Public Property PostingDate As String
    Public Property DueDate As String
    Public Property RefNo As String
    Public Property RefDate As String
    Public Property PhoneNo As String
    Public Property ExternalDoctorsRef As String
    Public Property ShiptoCode As String
    Public Property BilltoCode As String
    Public Property PatientAge As Integer
    Public Property PatientConcern As String
    Public Property DoctorsCode As String
    Public Property DoctorsComment As String
    Public Property DoctorSuggestion As String
    Public Property DoctorObservation As String
    Public Property FollowupDate As String
    Public Property Remarks As String
    Public Property InvoiceNo As String
    Public Property Items As DTS_MODEL_SQ_ITEMS()
    Public Property Testing As DTS_MODEL_SQ_HSTDTLS()
End Class

