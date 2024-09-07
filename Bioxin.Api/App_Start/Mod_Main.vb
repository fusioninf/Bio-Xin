Imports System.Net.Http

Imports Sap.Data.Hana

Public Class Mod_Main
    Public Shared qStr As String = ""
    Public Shared DBSQL As DBAccess_HANAServer = New DBAccess_HANAServer

    Friend Shared Function VerifiedAccessToken(ByRef Req As HttpRequestMessage) As Boolean
        Dim ExternalAccessKey As String = "395853453A544D40444738264E3D753B506E5761613C61743B4A6E355F613944533C5940403356524E45454844"
        Dim AccessKey As String = String.Empty
        Dim headerValues As IEnumerable(Of String) = Nothing

        If Req.Headers.TryGetValues("AccessKey", headerValues) Then
            AccessKey = headerValues.FirstOrDefault()
        Else
            Return False
        End If

        Return (AccessKey = ExternalAccessKey)
    End Function

    Friend Shared Function Authenticate(ByRef Req As HttpRequestMessage, ByRef UserId As String, ByRef Branch As String, ByRef dtError As DataTable) As Boolean

        Dim ExternalAccessKey As String = "395853453A544D40444738264E3D753B506E5761613C61743B4A6E355F613944533C5940403356524E45454844"
        Dim DevToken As String = "3146315B32325C4E30633F32453E40545F6E324F69574A6D403A64424541"
        Dim Counter As Boolean = False
        Dim Authticate As Boolean = False
        Dim AuthToken As String

        For I As Integer = 0 To Req.Headers.Count - 1
            Dim AccessKey As String = Req.Headers(I).Key().ToString
            If AccessKey = "AccessKey" Then
                Counter = True
                Dim AccessVal As String = Req.Headers.GetValues("AccessKey").First
                Branch = Req.Headers.GetValues("Branch").First
                UserId = Req.Headers.GetValues("UserId").First
                AuthToken = Req.Headers.GetValues("AuthToken").First
                If AccessVal <> ExternalAccessKey Then
                    Authticate = False
                    Exit Function
                Else
                    Authticate = True
                End If
                If AuthToken <> DevToken Then
                    Authticate = False
                    Authticate = GetAccessToken(UserId, AuthToken)
                End If
                Exit For
            End If
        Next
        If Authticate = True Then
            qStr = "UPDATE ""$DTS_TRACK_USER"" SET ""LASTUPDT""=NOW() " &
                   "WHERE ""APPCODE""='BXN' AND USERCODE='" + UserId + "'"
            DBSQL.executeQuery(qStr, "")
        Else
            Dim NewRow As DataRow
            dtError.Columns.Add("ReturnCode")
            dtError.Columns.Add("ReturnMsg")
            NewRow = dtError.NewRow
            NewRow.Item("ReturnCode") = "-99999"
            NewRow.Item("ReturnMsg") = "AccessDenied !! "
            dtError.Rows.Add(NewRow)
        End If

        Return Authticate
        'If Counter = True Then
        '    Return True
        'Else
        '    Return False
        'End If
    End Function
    Friend Shared Function AuthenticateLogIn(ByRef Req As HttpRequestMessage) As Boolean

        Dim ExternalAccessKey As String = "395853453A544D40444738264E3D753B506E5761613C61743B4A6E355F613944533C5940403356524E45454844"

        Dim Counter As Boolean = False
        For I As Integer = 0 To Req.Headers.Count - 1
            Dim AccessKey As String = Req.Headers(I).Key().ToString
            'Dim AccessVal As String = Request.Headers(I).GetValues(Request.Headers(I).Key().ToString,)
            If AccessKey = "AccessKey" Then
                Counter = True
                Dim AccessVal As String = Req.Headers.GetValues("AccessKey").First
                If AccessVal <> ExternalAccessKey Then
                    Return False
                Else
                    Return True
                End If
            End If
        Next
        If Counter = False Then
            Return False
        End If
    End Function

    Public Shared Function isUserAllreadyConnected(ByVal UserCode As String, ByRef ErrorMessage As String) As Boolean
        'Return True if the user is logged in else False

        Dim ss As String = Connection.getSessionTime.ToString
        qStr = "SELECT ""USERCODE"" " &
                   "FROM ""$DTS_TRACK_USER"" T  " &
                   "WHERE T.""APPCODE""='BXN' " &
                   "    AND T.""USERCODE""= '" + UserCode + "' " &
                    "    AND SECONDS_BETWEEN(T.""LASTUPDT"", NOW()) <=" + Connection.getSessionTime.ToString + ""

        If DBSQL.getQueryDataTable(qStr, "").Rows.Count > 0 Then
            ErrorMessage = "The user is already logged in or not logged out properly "
            Return True
        Else
            Return False
        End If
    End Function


    Public Shared Function GetAccessToken(ByVal UserCode As String, ByVal AuthToken As String) As Boolean
        'Return True if the user is logged in else False
        qStr = "SELECT ""AUTHTOKEN"" " &
                   "FROM ""$DTS_TRACK_USER"" T  " &
                   "WHERE T.""APPCODE""='BXN' " &
                   "    AND T.""USERCODE""='" + UserCode + "' " &
                   "    AND T.""AUTHTOKEN""='" + AuthToken + "' "

        If DBSQL.getQueryDataTable(qStr, "").Rows.Count > 0 Then
            Return True
        Else
            'ErrorMessage = "The user is already logged in. "
            Return False
        End If
    End Function

    'Public Sub UserTrackingRow_Delete(ByVal UserCode As String, ByVal UserType As String)
    '    Dim SqlTrans As HanaTransaction = Nothing
    '    Dim SqlComm As New HanaCommand
    '    DBSQL.initConnection(SqlComm, SqlTrans)

    '    Dim Ex As Exception = Nothing
    '    Try
    '        qStr = "DELETE FROM [@ESPL_TRACK_USER] " &
    '               "WHERE APPCODE='" + AppCode + "' AND USERCODE='" + UserCode + "' AND USERTYPE='" + UserType + "'"
    '        DBSQL.convertDeleteString(qStr)
    '        SqlComm.CommandText = qStr
    '        SqlComm.ExecuteNonQuery()
    '        SqlTrans.Commit()
    '    Catch Ex
    '        SqlTrans.Rollback()
    '    Finally
    '        DBSQL.disposeConnection(SqlComm, SqlTrans, qStr)
    '        If Not Ex Is Nothing Then Throw Ex
    '    End Try
    'End Sub


    Public Shared Function Process_API(ByVal HanaDbCommand_SP As HanaCommand) As Data.DataTable
        Try
            Return DBSQL.getValueFromSP(HanaDbCommand_SP, "")
        Catch ex As Exception
            Dim dt_Error As New DataTable
            Dim NewRow As DataRow

            dt_Error.Columns.Add("ReturnCode")
            dt_Error.Columns.Add("ReturnMsg")
            NewRow = dt_Error.NewRow
            NewRow.Item("ReturnCode") = "2222"
            NewRow.Item("ReturnMsg") = ex.Message
            dt_Error.Rows.Add(NewRow)
            Return dt_Error
        End Try
    End Function

    Public Shared Function Process_APIDS(ByVal HanaDbCommand_SP As HanaCommand) As Data.DataSet
        Try
            Return DBSQL.getValueFromSPDS(HanaDbCommand_SP, "")
        Catch ex As Exception
            Dim dt_Error As New DataTable
            Dim ds As New DataSet
            Dim NewRow As DataRow

            dt_Error.Columns.Add("ReturnCode")
            dt_Error.Columns.Add("ReturnMsg")
            NewRow = dt_Error.NewRow
            NewRow.Item("ReturnCode") = "2222"
            NewRow.Item("ReturnMsg") = ex.Message
            dt_Error.Rows.Add(NewRow)
            ds.Tables.Add(dt_Error)
            Return ds
        End Try
    End Function


End Class
