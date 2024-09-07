Imports System.Data.OleDb
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic
Imports System.Data
Imports Sap.Data.Hana
Imports System.Data.Odbc


Public Class DBAccess_HANAServer

    Public Sub convertFetchString(ByRef str As String)
        str = Regex.Replace(str, "\=(\s{0,})\'", "=N'")
        str = Regex.Replace(str, "\<>(\s{0,})\'", "<>N'")
        str = Regex.Replace(str, "\((\s{0,})\'", "(N'")
        str = Regex.Replace(str, "LIKE(\s{0,})\'", "LIKE N'")
    End Sub

    Public Sub convertUpdateString(ByRef str As String)
        str = Regex.Replace(str, "\=(\s{0,})\'", "=N'")
        str = Regex.Replace(str, "\<>(\s{0,})\'", "<>N'")
        str = Regex.Replace(str, "like(\s{0,})\'", "like N'")
    End Sub

    Public Sub convertDeleteString(ByRef str As String)
        str = Regex.Replace(str, "\=(\s{0,})\'", "=N'")
        str = Regex.Replace(str, "\<>(\s{0,})\'", "<>N'")
        str = Regex.Replace(str, "like(\s{0,})\'", "like N'")

    End Sub

    Public Sub convertInsertString(ByRef str As String)
        str = Regex.Replace(str, "values(\s{0,})\((\s{0,})\'", "values(N'")
        str = Regex.Replace(str, "\,(\s{0,})\'", ",N'")
        str = Regex.Replace(str, "VALUES(\s{0,})\((\s{0,})\'", "VALUES(N'")
        str = Regex.Replace(str, "like(\s{0,})\'", "like N'")
    End Sub

    Public Sub convertCheckString(ByRef str As String)
        str = Regex.Replace(str, "\=(\s{0,})\'", "=N'")
        str = Regex.Replace(str, "\<>(\s{0,})\'", "<>N'")
        str = Regex.Replace(str, "like(\s{0,})\'", "like N'")
    End Sub

    Public Sub initConnection(ByRef sqlCommand As HanaCommand, ByVal DBName As String)
        Dim con As New HanaConnection(Connection.getConnectionString(DBName))
        If con.State = Data.ConnectionState.Closed Then
            con.Open()
        End If
        sqlCommand.Connection = con
        sqlCommand.CommandTimeout = 0
    End Sub

    Public Sub initConnection1(ByRef HanaCommand As HanaCommand, ByVal DBName As String)
        Dim str As String
        Try
            str = Connection.getConnectionString(DBName)
            'Dim HANAconn As OdbcConnection = New OdbcConnection()

            ' "DRIVER={HDBODBC};UID=SYSTEM;PWD=*****;SERVERNODE=hanaaisi2:30015;CS=RESOLV_FIORI";

            'Dim HANAconn As OdbcConnection = New OdbcConnection("DRIVER={HDBODBC};UID=B1ADMIN;PWD=SapB1Hana;SERVERNODE=E03@192.168.50.60:30015;CS=ASTERISK_UAT")
            ''Dim con_HANA As New HanaConnection(str)
            'If HANAconn.State = ConnectionState.Open Then
            '    HANAconn.Close()
            'End If
            'HANAconn.Open()
        Catch ex As Exception
            ex.Message.ToString()
        End Try

        Dim con As New HanaConnection(Connection.getConnectionString(DBName))
        If con.State = Data.ConnectionState.Closed Then
            con.Open()
        End If
        HanaCommand.Connection = con
        HanaCommand.CommandTimeout = 0
    End Sub

    Public Sub initConnection(ByRef HanaCommand As HanaCommand, ByRef sqlTrans As HanaTransaction, ByVal DBName As String)
        Dim con As New HanaConnection(Connection.getConnectionString(DBName))
        If con.State = Data.ConnectionState.Closed Then
            con.Open()
        End If
        sqlTrans = con.BeginTransaction(Data.IsolationLevel.Serializable)
        HanaCommand.Connection = con
        HanaCommand.CommandTimeout = 0
        HanaCommand.Transaction = sqlTrans
    End Sub

    Public Sub disposeConnection(ByRef HanaCommand As HanaCommand, ByVal DBName As String)
        Dim con As HanaConnection = HanaCommand.Connection
        HanaCommand.Dispose()
        con.Close()
        con.Dispose()
    End Sub

    Public Sub disposeConnection(ByRef HanaCommand As HanaCommand, ByRef sqlTrans As HanaTransaction, ByRef qStr As String, ByVal DBName As String)
        Dim con As HanaConnection = HanaCommand.Connection
        sqlTrans.Dispose()
        HanaCommand.Dispose()
        con.Close()
        con.Dispose()
        qStr = Nothing
    End Sub

    Public Sub executeCommand(ByVal HanaCommand As HanaCommand, ByVal DBName As String)
        initConnection(HanaCommand, DBName)
        Dim Ex As Exception = Nothing
        Try
            HanaCommand.ExecuteNonQuery()
        Catch Ex
        Finally
            disposeConnection(HanaCommand, DBName)
            If Not Ex Is Nothing Then Throw Ex
        End Try
    End Sub

    Public Sub executeQuery(ByVal qString As String, ByVal DBName As String)
        Dim sqlTrans As HanaTransaction = Nothing
        Dim HanaCommand As New HanaCommand
        initConnection(HanaCommand, sqlTrans, DBName)

        Dim Ex As Exception = Nothing
        Try
            HanaCommand.CommandText = qString
            HanaCommand.ExecuteNonQuery()
            sqlTrans.Commit()
        Catch Ex
            sqlTrans.Rollback()
        Finally
            disposeConnection(HanaCommand, sqlTrans, qString, DBName)
            If Not Ex Is Nothing Then Throw Ex
        End Try
    End Sub

    'Public Function getValueFromSP(ByVal HanaCommand As HanaCommand, ByVal DBName As String) As Data.DataTable
    '    Dim sqlAdapter As New HanaDataAdapter
    '    Dim dt_SP As New Data.DataTable

    '    Dim Ex As Exception = Nothing
    '    Try
    '        initConnection(HanaCommand, DBName)
    '        sqlAdapter = New HanaDataAdapter(HanaCommand)
    '        sqlAdapter.Fill(dt_SP)
    '    Catch Ex
    '    Finally
    '        sqlAdapter.Dispose()
    '        disposeConnection(HanaCommand, DBName)
    '        If Not Ex Is Nothing Then Throw Ex
    '    End Try
    '    Return dt_SP
    'End Function

    Public Function getValueFromSP(ByVal HanaCommand As HanaCommand, ByVal DBName As String) As Data.DataTable
        Dim sqlAdapter As New HanaDataAdapter
        Dim dt_SP As New Data.DataTable

        Dim Ex As Exception = Nothing
        Try
            initConnection(HanaCommand, DBName)
            sqlAdapter = New HanaDataAdapter(HanaCommand)
            sqlAdapter.Fill(dt_SP)
        Catch Ex
        Finally
            sqlAdapter.Dispose()
            disposeConnection(HanaCommand, DBName)
            If Not Ex Is Nothing Then Throw Ex
        End Try
        Return dt_SP
    End Function


    Public Function getValueFromSPDS(ByVal HanaCommand As HanaCommand, ByVal DBName As String) As Data.DataSet
        Dim sqlAdapter As New HanaDataAdapter
        Dim dt_SP As New Data.DataSet

        Dim Ex As Exception = Nothing
        Try
            initConnection(HanaCommand, DBName)
            sqlAdapter = New HanaDataAdapter(HanaCommand)
            sqlAdapter.Fill(dt_SP)
        Catch Ex
        Finally
            sqlAdapter.Dispose()
            disposeConnection(HanaCommand, DBName)
            If Not Ex Is Nothing Then Throw Ex
        End Try
        Return dt_SP
    End Function

    Public Function getValueFromSP_DataSet(ByVal HanaCommand As HanaCommand, ByVal DBName As String) As Data.DataSet
        Dim sqlAdapter As New HanaDataAdapter
        Dim ds_SP As New Data.DataSet

        Dim Ex As Exception = Nothing
        Try
            initConnection(HanaCommand, DBName)
            sqlAdapter = New HanaDataAdapter(HanaCommand)
            sqlAdapter.Fill(ds_SP)
        Catch Ex
        Finally
            sqlAdapter.Dispose()
            disposeConnection(HanaCommand, DBName)
            If Not Ex Is Nothing Then Throw Ex
        End Try
        Return ds_SP
    End Function
    Public Function getQueryDataRow(ByRef QueryString As String, ByVal DBName As String) As Data.DataRow
        Dim con As New HanaConnection(Connection.getConnectionString(DBName))

        convertFetchString(QueryString)
        Dim sqlAdapter As HanaDataAdapter = New HanaDataAdapter(QueryString, con)
        Dim sqlDataTable As New Data.DataTable

        Dim Ex As Exception = Nothing
        Try
            sqlAdapter.Fill(sqlDataTable)
        Catch Ex
        Finally
            sqlAdapter.Dispose()
            con.Close()
            con.Dispose()
            If Not Ex Is Nothing Then Throw Ex
        End Try
        QueryString = Nothing
        If sqlDataTable.Rows.Count > 0 Then
            Return sqlDataTable.Rows(0)
        Else
            Return Nothing
        End If
    End Function
    Public Function getQueryDataTable(ByRef QueryString As String, ByVal DBName As String) As Data.DataTable
        Dim con As New HanaConnection(Connection.getConnectionString(DBName))
        convertFetchString(QueryString)
        Dim sqlAdapter As HanaDataAdapter = New HanaDataAdapter(QueryString, con)
        Dim sqlDataTable As New Data.DataTable

        Dim Ex As Exception = Nothing
        Try
            sqlAdapter.Fill(sqlDataTable)
        Catch Ex
        Finally
            sqlAdapter.Dispose()
            con.Close()
            con.Dispose()
            If Not Ex Is Nothing Then Throw Ex
        End Try
        QueryString = Nothing
        Return sqlDataTable
    End Function

    Public Function getQueryDataSet(ByRef QueryString As String, ByVal DBName As String) As Data.DataSet
        Dim con As New HanaConnection(Connection.getConnectionString(DBName))
        convertFetchString(QueryString)
        Dim sqlAdapter As HanaDataAdapter = New HanaDataAdapter(QueryString, con)
        Dim sqlDataSet As New Data.DataSet

        Dim Ex As Exception = Nothing
        Try
            sqlAdapter.Fill(sqlDataSet)
        Catch Ex
        Finally
            sqlAdapter.Dispose()
            con.Close()
            con.Dispose()
            If Not Ex Is Nothing Then Throw Ex
        End Try
        QueryString = Nothing
        Return sqlDataSet
    End Function


End Class

