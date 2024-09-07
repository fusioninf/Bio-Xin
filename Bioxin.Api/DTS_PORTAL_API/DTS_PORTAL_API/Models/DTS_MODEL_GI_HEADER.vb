Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web

Public Class DTS_MODEL_GI_HEADER
    Public Property Branch As String
    Public Property PostingDate As String
    Public Property RefNo As String
    Public Property RefDate As String
    Public Property Remarks As String
    'Public Property EnteredBy As String
    Public Property Items As DTS_MODEL_GI_ITEMS()
End Class

