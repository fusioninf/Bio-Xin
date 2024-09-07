Imports System.Net.Http
Imports System.Web.Http
Imports Sap.Data.Hana

Imports System.Data

Imports System.Data.OleDb
Imports System.Data.Odbc
Imports System.Data.SqlClient.SqlDataAdapter

Imports System.Data.SqlClient
Imports System.Linq
Imports System.Net

Namespace Controllers
    Public Class DTS_PostController
        Inherits ApiController

        Dim G_DI_Company As SAPbobsCOM.Company = Nothing
        Dim rSet As SAPbobsCOM.Recordset
        Dim dtTable As New DataTable
        Dim dtError As New DataTable
        Dim NewCol As DataColumn
        Dim NewRow As DataRow
        Dim ColCount As Integer
        Dim dt_Error As New DataTable
        Public Shared DBSQL As DBAccess_HANAServer = New DBAccess_HANAServer

        <Route("Api/PostStockTransferRequest")>
        <HttpPost>
        Public Function PostStockTransferRequest(ByVal TransferDetails As DTS_MODEL_ITRNR_HEADER) As HttpResponseMessage

            Dim G_DI_Company As SAPbobsCOM.Company = Nothing
            Dim strRegnNo As String = ""
            dtTable.Columns.Add("ReturnCode")
            dtTable.Columns.Add("ReturnDocEntry")
            dtTable.Columns.Add("ReturnObjType")
            dtTable.Columns.Add("ReturnSeries")
            dtTable.Columns.Add("ReturnDocNum")
            dtTable.Columns.Add("ReturnMsg")
            Try
                Dim Branch As String = ""
                Dim UserID As String = ""


                If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                    Return Request.CreateResponse(HttpStatusCode.OK, dtError)
                End If

                Dim qstr As String = ""
                qstr = "SELECT ""U_SAPUNAME"" ,""U_SAPPWD"" " & vbNewLine &
                       " FROM ""OUSR"" A " & vbNewLine &
                       " WHERE A.""USER_CODE"" ='" + UserID + "'"
                Dim dRow As Data.DataRow
                dRow = DBSQL.getQueryDataRow(qstr, "")
                Dim SAPUserId As String = ""
                Dim SAPPassword As String = ""
                Try
                    SAPUserId = dRow.Item("U_SAPUNAME")
                    SAPPassword = dRow.Item("U_SAPPWD")
                Catch ex As Exception
                End Try
                Connection.ConnectSAP(G_DI_Company, SAPUserId, SAPPassword)

                rSet = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                qstr = "Select Top 1 N.""Series"",B.""BPLId"",B.""DfltResWhs"" ""WhsCode"" ,TO_CHAR(NOW(),'YYYYYMMDD') ""DocDate"" " & vbNewLine &
                                "  From ""NNM1"" N  " & vbNewLine &
                                "       Inner Join ""OFPR"" O On O.""Indicator""=N.""Indicator"" " & vbNewLine &
                                "       Inner Join ""OBPL"" B On B.""BPLId""=N.""BPLId"" " & vbNewLine &
                                " Where N.""ObjectCode""='1250000001' " & vbNewLine &
                                "   And O.""PeriodStat"" In ('N','C') And N.""Locked""='N' AND N.""Remark""='" + Branch + "'  " & vbNewLine &
                                "   And TO_CHAR('" + TransferDetails.PostingDate + "','YYYYYMMDD') Between TO_CHAR(O.""F_RefDate"",'YYYYYMMDD') And TO_CHAR(O.""T_RefDate"",'YYYYYMMDD')"
                'qstr = "Select Top 1 N.""Series"",B.""BPLId"",B.""DfltResWhs"" ""WhsCode"" ,TO_CHAR(GETDATE(),'YYYYYMMDD') ""DocDate"" " & vbNewLine &
                '                "  From ""NNM1"" N  " & vbNewLine &
                '                "       Inner Join ""OFPR"" O On O.""Indicator""=N.""Indicator"" " & vbNewLine &
                '                "       Inner Join ""OWHS"" W On W.""WhsCode""='" + TransferDetails.FromWarehouse.ToString + "' " & vbNewLine &
                '                "       Inner Join ""OBPL"" B On B.""BPLId""=W.""BPLId"" And B.""BPLId""=N.""BPLId"" " & vbNewLine &
                '                " Where N.""ObjectCode""='1250000001' " & vbNewLine &
                '                "   And O.""PeriodStat"" In ('N','C') And N.""Locked""='N' AND N.""Remark""='" + Branch + "'  " & vbNewLine &
                '                "   And TO_CHAR('" + TransferDetails.PostingDate + "','YYYYYMMDD') Between TO_CHAR(O.F_RefDate,'YYYYYMMDD') And TO_CHAR(O.T_RefDate,'YYYYYMMDD')"
                rSet.DoQuery(qstr)

                If rSet.RecordCount > 0 Then
                    Dim CrtdDate As String = rSet.Fields.Item("DocDate").Value
                    Dim DocDate As String = TransferDetails.PostingDate
                    Dim StockTransfer As SAPbobsCOM.StockTransfer = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInventoryTransferRequest)
                    StockTransfer.DocObjectCode = SAPbobsCOM.BoObjectTypes.oInventoryTransferRequest

                    StockTransfer.CardCode = TransferDetails.CardCode
                    'StockTransfer.BPL_IDAssignedToInvoice = TransferDetails.Branch
                    StockTransfer.Series = rSet.Fields.Item("Series").Value
                    StockTransfer.DocDate = New Date(Mid(DocDate, 1, 4), Mid(DocDate, 5, 2), Mid(DocDate, 7, 2))
                    StockTransfer.DueDate = New Date(Mid(TransferDetails.DueDate, 1, 4), Mid(TransferDetails.DueDate, 5, 2), Mid(TransferDetails.DueDate, 7, 2))
                    StockTransfer.TaxDate = New Date(Mid(TransferDetails.RefDate, 1, 4), Mid(TransferDetails.RefDate, 5, 2), Mid(TransferDetails.RefDate, 7, 2))
                    If TransferDetails.ContactPerson <> "" Then
                        StockTransfer.ContactPerson = TransferDetails.ContactPerson
                    End If


                    qstr = "SELECT ""WhsCode"" FROM ""OWHS"" WHERE ""U_BUSUNIT""='" + Branch.ToString + "' AND ""U_WHSTYPE"" ='I'"
                    Dim toWhsrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                    toWhsrSet.DoQuery(qstr)
                    Dim toWhsCode As String = toWhsrSet.Fields.Item("WhsCode").Value


                    StockTransfer.ShipToCode = TransferDetails.ShiptoCode
                    StockTransfer.Comments = TransferDetails.Remarks
                    StockTransfer.FromWarehouse = TransferDetails.FromWarehouse
                    StockTransfer.ToWarehouse = toWhsCode

                    StockTransfer.UserFields.Fields.Item("U_BRANCH").Value = Branch
                    If TransferDetails.SalesEmployee.ToString <> "" Then
                        StockTransfer.SalesPersonCode = TransferDetails.SalesEmployee
                    End If
                    'StockTransfer.SalesPersonCode = TransferDetails.SalesEmployee
                    'Delivery.DiscountPercent = DeliveryDetails.Discount
                    StockTransfer.UserFields.Fields.Item("U_CRTDBY").Value = UserID
                    Dim approvedDate As String = TransferDetails.ApprovedDate
                    If TransferDetails.ApprovedBy <> Nothing Then
                        StockTransfer.UserFields.Fields.Item("U_APPRVBY").Value = TransferDetails.ApprovedBy
                    End If
                    If TransferDetails.ApprovedDate <> Nothing Then
                        StockTransfer.UserFields.Fields.Item("U_APPRVDT").Value = New Date(Mid(approvedDate, 1, 4), Mid(approvedDate, 5, 2), Mid(approvedDate, 7, 2))
                    End If
                    Try
                        StockTransfer.UserFields.Fields.Item("U_BASENTRY").Value = TransferDetails.BaseEntry.ToString
                    Catch ex As Exception
                    End Try
                    'StockTransfer.UserFields.Fields.Item("U_CRTDDT").Value = New Date(Mid(CrtdDate, 1, 4), Mid(CrtdDate, 5, 2), Mid(CrtdDate, 7, 2))
                    For Each Item As DTS_MODEL_ITRND_ITEMS In TransferDetails.Items
                        StockTransfer.Lines.ItemCode = Item.ItemCode
                        StockTransfer.Lines.FromWarehouseCode = TransferDetails.FromWarehouse
                        StockTransfer.Lines.WarehouseCode = toWhsCode
                        'Delivery.Lines.UserFields.Fields.Item("U_PACKQTY").Value = Item.PackingQuantity
                        'Delivery.Lines.UserFields.Fields.Item("U_PACKSIZE").Value = Item.PackingSize
                        StockTransfer.Lines.Quantity = Item.Quantity
                        StockTransfer.Lines.Price = Item.Price
                        StockTransfer.Lines.DistributionRule = Branch
                        Try
                            StockTransfer.Lines.UserFields.Fields.Item("U_BASENTR").Value = Item.BaseEntry.ToString
                        Catch ex As Exception
                        End Try
                        Try
                            StockTransfer.Lines.UserFields.Fields.Item("U_BASELINE").Value = Item.BaseLine.ToString
                        Catch ex As Exception
                        End Try
                        Try
                            StockTransfer.Lines.UserFields.Fields.Item("U_BASETYPE").Value = Item.BaseType.ToString
                        Catch ex As Exception
                        End Try
                        StockTransfer.Lines.UserFields.Fields.Item("U_REMARKS").Value = IIf(Item.Remarks Is Nothing, "", Item.Remarks)
                        StockTransfer.Lines.Add()
                    Next

                    Dim lRetCode As Integer
                    'Dim s = Delivery.GetAsXML
                    lRetCode = StockTransfer.Add
                    Dim ErrorMessage As String = ""
                    If lRetCode <> 0 Then
                        Dim sErrMsg As String = G_DI_Company.GetLastErrorDescription()
                        G_DI_Company.GetLastError(lRetCode, sErrMsg)
                        ErrorMessage = sErrMsg
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(StockTransfer)
                        G_DI_Company.Disconnect()
                        NewRow = dtTable.NewRow
                        NewRow.Item("ReturnCode") = "-2222"
                        NewRow.Item("ReturnDocEntry") = "-1"
                        NewRow.Item("ReturnObjType") = "-1"
                        NewRow.Item("ReturnSeries") = "-1"
                        NewRow.Item("ReturnDocNum") = "-1"
                        NewRow.Item("ReturnMsg") = ErrorMessage
                        dtTable.Rows.Add(NewRow)
                        Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                    Else
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(StockTransfer)
                        Dim ObjType As String = G_DI_Company.GetNewObjectType.ToString.TrimEnd
                        Dim DLEntry As String = G_DI_Company.GetNewObjectKey.Trim.ToString
                        Dim ErrMsg As String = ""

                        If ObjType = "112" Then
                            qstr = "SELECT B.""SeriesName"",CAST(A.""DocNum"" AS VARCHAR) ""DocNum"",B.""SeriesName""||'/'||CAST(A.""DocNum"" AS VARCHAR) ""StrDocNum"" " & vbNewLine &
                               " From ""ODRF"" A " & vbNewLine &
                               " INNER JOIN ""NNM1"" B ON B.""ObjectCode""=A.""ObjType"" AND B.""Series""=A.""Series"" " & vbNewLine &
                               " WHERE A.""DocEntry""='" + DLEntry + "' "
                        ElseIf ObjType = "1250000001" Then
                            qstr = "SELECT B.""SeriesName"",CAST(A.""DocNum"" AS VARCHAR) ""DocNum"",B.""SeriesName""||'/'||CAST(A.""DocNum"" AS VARCHAR) ""StrDocNum"" " & vbNewLine &
                               " From ""OWTQ"" A " & vbNewLine &
                               " INNER JOIN ""NNM1"" B ON B.""ObjectCode""=A.""ObjType"" AND B.""Series""=A.""Series"" " & vbNewLine &
                               " WHERE A.""DocEntry""='" + DLEntry + "' "
                        End If
                        Dim PostrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                        PostrSet.DoQuery(qstr)
                        Dim ReturnDocNo = PostrSet.Fields.Item("StrDocNum").Value
                        G_DI_Company.Disconnect()
                        NewRow = dtTable.NewRow
                        NewRow.Item("ReturnCode") = "0000"
                        NewRow.Item("ReturnDocEntry") = DLEntry
                        NewRow.Item("ReturnObjType") = ObjType
                        NewRow.Item("ReturnSeries") = PostrSet.Fields.Item("SeriesName").Value
                        NewRow.Item("ReturnDocNum") = PostrSet.Fields.Item("DocNum").Value
                        If ObjType = "112" Then
                            NewRow.Item("ReturnMsg") = "Your Request No for Approval. " + ReturnDocNo + " successfully submitted"
                        Else
                            NewRow.Item("ReturnMsg") = "Your Request No. " + ReturnDocNo + " successfully submitted"
                        End If

                        dtTable.Rows.Add(NewRow)
                        Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                    End If
                Else
                    G_DI_Company.Disconnect()
                    NewRow = dtTable.NewRow
                    NewRow.Item("ReturnCode") = "-2222"
                    NewRow.Item("ReturnDocEntry") = "-1"
                    NewRow.Item("ReturnObjType") = "-1"
                    NewRow.Item("ReturnSeries") = "-1"
                    NewRow.Item("ReturnDocNum") = "-1"
                    NewRow.Item("ReturnMsg") = "Series not found"
                    dtTable.Rows.Add(NewRow)
                    Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                End If

            Catch __unusedException1__ As Exception
                Try
                    G_DI_Company.Disconnect()
                Catch ex1 As Exception
                End Try
                NewRow = dtTable.NewRow
                NewRow.Item("ReturnCode") = "-2222"
                NewRow.Item("ReturnMsg") = __unusedException1__.Message.ToString
                NewRow.Item("ReturnDocEntry") = "-1"
                NewRow.Item("ReturnObjType") = "-1"
                NewRow.Item("ReturnSeries") = "-1"
                NewRow.Item("ReturnDocNum") = "-1"
                dtTable.Rows.Add(NewRow)
                Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
            End Try

        End Function
        <Route("Api/PostMultiStockTransferRequest")>
        <HttpPost>
        Public Function PostMultiStockTransferRequest(ByVal TransferDetails As DTS_MODEL_ITRNR_HEADER) As HttpResponseMessage

            Dim G_DI_Company As SAPbobsCOM.Company = Nothing
            Dim strRegnNo As String = ""
            dtTable.Columns.Add("ReturnCode")
            dtTable.Columns.Add("ReturnDocEntry")
            dtTable.Columns.Add("ReturnObjType")
            dtTable.Columns.Add("ReturnSeries")
            dtTable.Columns.Add("ReturnDocNum")
            dtTable.Columns.Add("ReturnMsg")
            Try
                Dim Branch As String = ""
                Dim UserID As String = ""


                If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                    Return Request.CreateResponse(HttpStatusCode.OK, dtError)
                End If

                Dim qstr As String = ""
                qstr = "SELECT ""U_SAPUNAME"" ,""U_SAPPWD"" " & vbNewLine &
                       " FROM ""OUSR"" A " & vbNewLine &
                       " WHERE A.""USER_CODE"" ='" + UserID + "'"
                Dim dRow As Data.DataRow
                dRow = DBSQL.getQueryDataRow(qstr, "")
                Dim SAPUserId As String = ""
                Dim SAPPassword As String = ""
                Try
                    SAPUserId = dRow.Item("U_SAPUNAME")
                    SAPPassword = dRow.Item("U_SAPPWD")
                Catch ex As Exception
                End Try
                Connection.ConnectSAP(G_DI_Company, SAPUserId, SAPPassword)

                rSet = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                qstr = "Select Top 1 N.""Series"",B.""BPLId"",B.""DfltResWhs"" ""WhsCode"" ,TO_CHAR(NOW(),'YYYYYMMDD') ""DocDate"" " & vbNewLine &
                                "  From ""NNM1"" N  " & vbNewLine &
                                "       Inner Join ""OFPR"" O On O.""Indicator""=N.""Indicator"" " & vbNewLine &
                                "       Inner Join ""OBPL"" B On B.""BPLId""=N.""BPLId"" " & vbNewLine &
                                " Where N.""ObjectCode""='1250000001' " & vbNewLine &
                                "   And O.""PeriodStat"" In ('N','C') And N.""Locked""='N' AND N.""Remark""='" + Branch + "'  " & vbNewLine &
                                "   And TO_CHAR('" + TransferDetails.PostingDate + "','YYYYYMMDD') Between TO_CHAR(O.""F_RefDate"",'YYYYYMMDD') And TO_CHAR(O.""T_RefDate"",'YYYYYMMDD')"
                'qstr = "Select Top 1 N.""Series"",B.""BPLId"",B.""DfltResWhs"" ""WhsCode"" ,TO_CHAR(GETDATE(),'YYYYYMMDD') ""DocDate"" " & vbNewLine &
                '                "  From ""NNM1"" N  " & vbNewLine &
                '                "       Inner Join ""OFPR"" O On O.""Indicator""=N.""Indicator"" " & vbNewLine &
                '                "       Inner Join ""OWHS"" W On W.""WhsCode""='" + TransferDetails.FromWarehouse.ToString + "' " & vbNewLine &
                '                "       Inner Join ""OBPL"" B On B.""BPLId""=W.""BPLId"" And B.""BPLId""=N.""BPLId"" " & vbNewLine &
                '                " Where N.""ObjectCode""='1250000001' " & vbNewLine &
                '                "   And O.""PeriodStat"" In ('N','C') And N.""Locked""='N' AND N.""Remark""='" + Branch + "'  " & vbNewLine &
                '                "   And TO_CHAR('" + TransferDetails.PostingDate + "','YYYYYMMDD') Between TO_CHAR(O.F_RefDate,'YYYYYMMDD') And TO_CHAR(O.T_RefDate,'YYYYYMMDD')"
                rSet.DoQuery(qstr)

                If rSet.RecordCount > 0 Then
                    G_DI_Company.StartTransaction()
                    Dim CrtdDate As String = rSet.Fields.Item("DocDate").Value
                    Dim DocDate As String = TransferDetails.PostingDate
                    Dim StockTransfer As SAPbobsCOM.StockTransfer = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInventoryTransferRequest)
                    StockTransfer.DocObjectCode = SAPbobsCOM.BoObjectTypes.oInventoryTransferRequest

                    Try
                        StockTransfer.CardCode = TransferDetails.CardCode
                        If TransferDetails.ContactPerson <> "" Then
                            StockTransfer.ContactPerson = TransferDetails.ContactPerson
                        End If
                        StockTransfer.ShipToCode = TransferDetails.ShiptoCode
                        If TransferDetails.SalesEmployee.ToString <> "" Then
                            StockTransfer.SalesPersonCode = TransferDetails.SalesEmployee
                        End If
                    Catch ex As Exception

                    End Try

                    'StockTransfer.BPL_IDAssignedToInvoice = TransferDetails.Branch
                    StockTransfer.Series = rSet.Fields.Item("Series").Value
                    StockTransfer.DocDate = New Date(Mid(DocDate, 1, 4), Mid(DocDate, 5, 2), Mid(DocDate, 7, 2))
                    StockTransfer.DueDate = New Date(Mid(TransferDetails.DueDate, 1, 4), Mid(TransferDetails.DueDate, 5, 2), Mid(TransferDetails.DueDate, 7, 2))
                    StockTransfer.TaxDate = New Date(Mid(TransferDetails.RefDate, 1, 4), Mid(TransferDetails.RefDate, 5, 2), Mid(TransferDetails.RefDate, 7, 2))



                    qstr = "SELECT ""WhsCode"" FROM ""OWHS"" WHERE ""U_BUSUNIT""='" + Branch.ToString + "' AND ""U_WHSTYPE"" ='I'"
                    Dim toWhsrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                    toWhsrSet.DoQuery(qstr)
                    Dim toWhsCode As String = toWhsrSet.Fields.Item("WhsCode").Value



                    StockTransfer.Comments = TransferDetails.Remarks
                    StockTransfer.FromWarehouse = TransferDetails.FromWarehouse
                    StockTransfer.ToWarehouse = toWhsCode

                    StockTransfer.UserFields.Fields.Item("U_BRANCH").Value = Branch
                    'StockTransfer.SalesPersonCode = TransferDetails.SalesEmployee
                    'Delivery.DiscountPercent = DeliveryDetails.Discount
                    StockTransfer.UserFields.Fields.Item("U_CRTDBY").Value = UserID
                    StockTransfer.UserFields.Fields.Item("U_ECOMMUL").Value = "Y"

                    Dim approvedDate As String = TransferDetails.ApprovedDate
                    If TransferDetails.ApprovedBy <> Nothing Then
                        StockTransfer.UserFields.Fields.Item("U_APPRVBY").Value = TransferDetails.ApprovedBy
                    End If
                    If TransferDetails.ApprovedDate <> Nothing Then
                        StockTransfer.UserFields.Fields.Item("U_APPRVDT").Value = New Date(Mid(approvedDate, 1, 4), Mid(approvedDate, 5, 2), Mid(approvedDate, 7, 2))
                    End If
                    Try
                        StockTransfer.UserFields.Fields.Item("U_BASENTRY").Value = TransferDetails.BaseEntry.ToString
                    Catch ex As Exception
                    End Try
                    'StockTransfer.UserFields.Fields.Item("U_CRTDDT").Value = New Date(Mid(CrtdDate, 1, 4), Mid(CrtdDate, 5, 2), Mid(CrtdDate, 7, 2))
                    For Each Item As DTS_MODEL_ITRND_ITEMS In TransferDetails.Items
                        StockTransfer.Lines.ItemCode = Item.ItemCode
                        StockTransfer.Lines.FromWarehouseCode = TransferDetails.FromWarehouse
                        StockTransfer.Lines.WarehouseCode = toWhsCode
                        'Delivery.Lines.UserFields.Fields.Item("U_PACKQTY").Value = Item.PackingQuantity
                        'Delivery.Lines.UserFields.Fields.Item("U_PACKSIZE").Value = Item.PackingSize
                        StockTransfer.Lines.Quantity = Item.Quantity
                        StockTransfer.Lines.Price = Item.Price
                        StockTransfer.Lines.DistributionRule = Branch
                        Try
                            StockTransfer.Lines.UserFields.Fields.Item("U_BASENTR").Value = Item.BaseEntry.ToString
                        Catch ex As Exception
                        End Try
                        Try
                            StockTransfer.Lines.UserFields.Fields.Item("U_BASELINE").Value = Item.BaseLine.ToString
                        Catch ex As Exception
                        End Try
                        Try
                            StockTransfer.Lines.UserFields.Fields.Item("U_BASETYPE").Value = Item.BaseType.ToString
                        Catch ex As Exception
                        End Try
                        StockTransfer.Lines.UserFields.Fields.Item("U_REMARKS").Value = IIf(Item.Remarks Is Nothing, "", Item.Remarks)
                        StockTransfer.Lines.Add()
                    Next

                    Dim lRetCode As Integer
                    'Dim s = Delivery.GetAsXML
                    lRetCode = StockTransfer.Add
                    Dim ErrorMessage As String = ""
                    If lRetCode <> 0 Then
                        Dim sErrMsg As String = G_DI_Company.GetLastErrorDescription()
                        G_DI_Company.GetLastError(lRetCode, sErrMsg)
                        ErrorMessage = sErrMsg
                        If G_DI_Company.InTransaction Then
                            G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                        End If
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(StockTransfer)
                        G_DI_Company.Disconnect()
                        NewRow = dtTable.NewRow
                        NewRow.Item("ReturnCode") = "-2222"
                        NewRow.Item("ReturnDocEntry") = "-1"
                        NewRow.Item("ReturnObjType") = "-1"
                        NewRow.Item("ReturnSeries") = "-1"
                        NewRow.Item("ReturnDocNum") = "-1"
                        NewRow.Item("ReturnMsg") = ErrorMessage
                        dtTable.Rows.Add(NewRow)
                        Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                    Else
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(StockTransfer)
                        Dim ObjType As String = G_DI_Company.GetNewObjectType.ToString.TrimEnd
                        Dim DLEntry As String = G_DI_Company.GetNewObjectKey.Trim.ToString
                        Dim ErrMsg As String = ""
                        Dim Lineid As Integer = 1
                        qstr = "do " & vbNewLine &
                                " begin"
                        For Each Dtls As String In TransferDetails.SOEntry.Split(New String() {","}, StringSplitOptions.None)
                            qstr = qstr + vbNewLine + "INSERT INTO ""@DTS_DR_SOTRNS""(""DocEntry"",""LineId"",""VisOrder"",""U_SOENTRY"") " &
                                        "VALUES('" + DLEntry.ToString + "','" + Lineid.ToString + "','" + Lineid.ToString + "','" + Dtls.ToString + "');"

                            Lineid = Lineid + 1

                        Next
                        qstr = qstr + vbNewLine + "end;"

                        Dim Sorset As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                        Sorset.DoQuery(qstr)

                        If ObjType = "112" Then
                            qstr = "SELECT B.""SeriesName"",CAST(A.""DocNum"" AS VARCHAR) ""DocNum"",B.""SeriesName""||'/'||CAST(A.""DocNum"" AS VARCHAR) ""StrDocNum"" " & vbNewLine &
                               " From ""ODRF"" A " & vbNewLine &
                               " INNER JOIN ""NNM1"" B ON B.""ObjectCode""=A.""ObjType"" AND B.""Series""=A.""Series"" " & vbNewLine &
                               " WHERE A.""DocEntry""='" + DLEntry + "' "
                        ElseIf ObjType = "1250000001" Then
                            qstr = "SELECT B.""SeriesName"",CAST(A.""DocNum"" AS VARCHAR) ""DocNum"",B.""SeriesName""||'/'||CAST(A.""DocNum"" AS VARCHAR) ""StrDocNum"" " & vbNewLine &
                               " From ""OWTQ"" A " & vbNewLine &
                               " INNER JOIN ""NNM1"" B ON B.""ObjectCode""=A.""ObjType"" AND B.""Series""=A.""Series"" " & vbNewLine &
                               " WHERE A.""DocEntry""='" + DLEntry + "' "
                        End If

                        Dim PostrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                        PostrSet.DoQuery(qstr)
                        Dim ReturnDocNo = PostrSet.Fields.Item("StrDocNum").Value
                        G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit)
                        G_DI_Company.Disconnect()
                        NewRow = dtTable.NewRow
                        NewRow.Item("ReturnCode") = "0000"
                        NewRow.Item("ReturnDocEntry") = DLEntry
                        NewRow.Item("ReturnObjType") = ObjType
                        NewRow.Item("ReturnSeries") = PostrSet.Fields.Item("SeriesName").Value
                        NewRow.Item("ReturnDocNum") = PostrSet.Fields.Item("DocNum").Value
                        If ObjType = "112" Then
                            NewRow.Item("ReturnMsg") = "Your Request No for Approval. " + ReturnDocNo + " successfully submitted"
                        Else
                            NewRow.Item("ReturnMsg") = "Your Request No. " + ReturnDocNo + " successfully submitted"
                        End If

                        dtTable.Rows.Add(NewRow)
                        Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                    End If
                Else
                    If G_DI_Company.InTransaction Then
                        G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                    End If
                    G_DI_Company.Disconnect()
                    NewRow = dtTable.NewRow
                    NewRow.Item("ReturnCode") = "-2222"
                    NewRow.Item("ReturnDocEntry") = "-1"
                    NewRow.Item("ReturnObjType") = "-1"
                    NewRow.Item("ReturnSeries") = "-1"
                    NewRow.Item("ReturnDocNum") = "-1"
                    NewRow.Item("ReturnMsg") = "Series not found"
                    dtTable.Rows.Add(NewRow)
                    Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                End If

            Catch __unusedException1__ As Exception
                Try
                    If G_DI_Company.InTransaction Then
                        G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                    End If
                Catch ex As Exception

                End Try
                Try
                    G_DI_Company.Disconnect()
                Catch ex1 As Exception
                End Try
                NewRow = dtTable.NewRow
                NewRow.Item("ReturnCode") = "-2222"
                NewRow.Item("ReturnMsg") = __unusedException1__.Message.ToString
                NewRow.Item("ReturnDocEntry") = "-1"
                NewRow.Item("ReturnObjType") = "-1"
                NewRow.Item("ReturnSeries") = "-1"
                NewRow.Item("ReturnDocNum") = "-1"
                dtTable.Rows.Add(NewRow)
                Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
            End Try

        End Function
        <Route("Api/PostStockTransferRequestApproval")>
        <HttpPost>
        Public Function PostStockTransferRequestApproval(ByVal ApprovalDetails As DTS_MODEL_ITRNR_APPROVAL) As HttpResponseMessage

            Dim G_DI_Company As SAPbobsCOM.Company = Nothing
            Dim strRegnNo As String = ""
            dtTable.Columns.Add("ReturnCode")
            dtTable.Columns.Add("ReturnDocEntry")
            dtTable.Columns.Add("ReturnObjType")
            dtTable.Columns.Add("ReturnSeries")
            dtTable.Columns.Add("ReturnDocNum")
            dtTable.Columns.Add("ReturnMsg")
            Try
                Dim Branch As String = ""
                Dim UserID As String = ""


                If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                    Return Request.CreateResponse(HttpStatusCode.OK, dtError)
                End If

                Dim qstr As String = ""
                qstr = "SELECT ""U_SAPUNAME"" ,""U_SAPPWD"" " & vbNewLine &
                       " FROM ""OUSR"" A " & vbNewLine &
                       " WHERE A.""USER_CODE"" ='" + UserID + "'"
                Dim dRow As Data.DataRow
                dRow = DBSQL.getQueryDataRow(qstr, "")
                Dim SAPUserId As String = ""
                Dim SAPPassword As String = ""
                Try
                    SAPUserId = dRow.Item("U_SAPUNAME")
                    SAPPassword = dRow.Item("U_SAPPWD")
                Catch ex As Exception
                End Try
                Connection.ConnectSAP(G_DI_Company, SAPUserId, SAPPassword)

                Dim oApprovalRequestsService As SAPbobsCOM.ApprovalRequestsService = G_DI_Company.GetCompanyService().GetBusinessService(SAPbobsCOM.ServiceTypes.ApprovalRequestsService)
                Dim oApprovalRequestParams As SAPbobsCOM.ApprovalRequestParams = oApprovalRequestsService.GetDataInterface(SAPbobsCOM.ApprovalRequestsServiceDataInterfaces.arsApprovalRequestParams)
                Dim oApprovalRequest As SAPbobsCOM.ApprovalRequest
                Dim oApprovalRequestDecision As SAPbobsCOM.ApprovalRequestDecision

                oApprovalRequestParams.Code = ApprovalDetails.ApprovalID
                oApprovalRequest = oApprovalRequestsService.GetApprovalRequest(oApprovalRequestParams)
                oApprovalRequestDecision = oApprovalRequest.ApprovalRequestDecisions.Add()
                oApprovalRequestDecision.Status = IIf(ApprovalDetails.Approved = "Y", SAPbobsCOM.BoApprovalRequestDecisionEnum.ardApproved, SAPbobsCOM.BoApprovalRequestDecisionEnum.ardNotApproved)
                oApprovalRequestDecision.Remarks = ApprovalDetails.ApprovalRemark
                oApprovalRequestDecision.ApproverUserName = SAPUserId
                oApprovalRequestDecision.ApproverPassword = SAPPassword
                Try
                    oApprovalRequestsService.UpdateRequest(oApprovalRequest)
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oApprovalRequest)
                    'Dim ObjType As String = G_DI_Company.GetNewObjectType.ToString.TrimEnd
                    'Dim DLEntry As String = G_DI_Company.GetNewObjectKey.Trim.ToString
                    'Dim ErrMsg As String = ""
                    Dim ReturnDocNo = ApprovalDetails.ApprovalID
                    G_DI_Company.Disconnect()
                    NewRow = dtTable.NewRow
                    NewRow.Item("ReturnCode") = "0000"
                    NewRow.Item("ReturnDocEntry") = ReturnDocNo
                    NewRow.Item("ReturnObjType") = ReturnDocNo
                    NewRow.Item("ReturnSeries") = ReturnDocNo
                    NewRow.Item("ReturnDocNum") = ReturnDocNo
                    NewRow.Item("ReturnMsg") = "Your Request No for Approval . " + ReturnDocNo + " successfully submitted"

                    dtTable.Rows.Add(NewRow)
                    Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                Catch ex As Exception
                    Dim sErrMsg As String = G_DI_Company.GetLastErrorDescription()
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oApprovalRequest)
                    G_DI_Company.Disconnect()
                    NewRow = dtTable.NewRow
                    NewRow.Item("ReturnCode") = "-2222"
                    NewRow.Item("ReturnDocEntry") = "-1"
                    NewRow.Item("ReturnObjType") = "-1"
                    NewRow.Item("ReturnSeries") = "-1"
                    NewRow.Item("ReturnDocNum") = "-1"
                    NewRow.Item("ReturnMsg") = ex.Message
                    dtTable.Rows.Add(NewRow)
                    Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                End Try

            Catch __unusedException1__ As Exception
                Try
                    G_DI_Company.Disconnect()
                Catch ex1 As Exception
                End Try
                NewRow = dtTable.NewRow
                NewRow.Item("ReturnCode") = "-2222"
                NewRow.Item("ReturnMsg") = __unusedException1__.Message.ToString
                NewRow.Item("ReturnDocEntry") = "-1"
                NewRow.Item("ReturnObjType") = "-1"
                NewRow.Item("ReturnSeries") = "-1"
                NewRow.Item("ReturnDocNum") = "-1"
                dtTable.Rows.Add(NewRow)
                Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
            End Try

        End Function
        <Route("Api/PostStockTransferRequestApprovedAdd")>
        <HttpPost>
        Public Function PostStockTransferRequestApprovedAdd(ByVal DraftDetails As DTS_MODEL_ITRNR_APPVADD) As HttpResponseMessage

            Dim G_DI_Company As SAPbobsCOM.Company = Nothing
            Dim strRegnNo As String = ""
            dtTable.Columns.Add("ReturnCode")
            dtTable.Columns.Add("ReturnDocEntry")
            dtTable.Columns.Add("ReturnObjType")
            dtTable.Columns.Add("ReturnSeries")
            dtTable.Columns.Add("ReturnDocNum")
            dtTable.Columns.Add("ReturnMsg")
            Try
                Dim Branch As String = ""
                Dim UserID As String = ""


                If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                    Return Request.CreateResponse(HttpStatusCode.OK, dtError)
                End If

                Dim qstr As String = ""
                qstr = "SELECT ""U_SAPUNAME"" ,""U_SAPPWD"" " & vbNewLine &
                       " FROM ""OUSR"" A " & vbNewLine &
                       " WHERE A.""USER_CODE"" ='" + UserID + "'"
                Dim dRow As Data.DataRow
                dRow = DBSQL.getQueryDataRow(qstr, "")
                Dim SAPUserId As String = ""
                Dim SAPPassword As String = ""
                Try
                    SAPUserId = dRow.Item("U_SAPUNAME")
                    SAPPassword = dRow.Item("U_SAPPWD")
                Catch ex As Exception
                End Try
                Connection.ConnectSAP(G_DI_Company, SAPUserId, SAPPassword)
                Dim ReturnDocNo = ""
                For Each Item As DTS_MODEL_ITRNR_DRFTADD In DraftDetails.DraftId
                    Dim oDraft As SAPbobsCOM.Documents = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDrafts)
                    If oDraft.GetByKey(Item.DraftEntry) Then
                        'oDraft.Confirmed = SAPbobsCOM.BoYesNoEnum.tYES
                        Dim lRetCode As Integer = oDraft.SaveDraftToDocument()
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(oDraft)
                        Dim ErrorMessage As String = ""
                        If lRetCode <> 0 Then
                            Dim sErrMsg As String = G_DI_Company.GetLastErrorDescription()
                            G_DI_Company.GetLastError(lRetCode, sErrMsg)
                            ErrorMessage = sErrMsg
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(oDraft)
                            G_DI_Company.Disconnect()
                            NewRow = dtTable.NewRow
                            NewRow.Item("ReturnCode") = "-2222"
                            NewRow.Item("ReturnDocEntry") = "-1"
                            NewRow.Item("ReturnObjType") = "-1"
                            NewRow.Item("ReturnSeries") = "-1"
                            NewRow.Item("ReturnDocNum") = "-1"
                            NewRow.Item("ReturnMsg") = ErrorMessage
                            dtTable.Rows.Add(NewRow)
                            Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                        Else
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(oDraft)
                            Dim ObjType As String = G_DI_Company.GetNewObjectType.ToString.TrimEnd
                            Dim DLEntry As String = G_DI_Company.GetNewObjectKey.Trim.ToString
                            Dim ErrMsg As String = ""

                            If ObjType = "112" Then
                                qstr = "SELECT B.""SeriesName"",CAST(A.""DocNum"" AS VARCHAR) ""DocNum"",B.""SeriesName""||'/'||CAST(A.""DocNum"" AS VARCHAR) ""StrDocNum"" " & vbNewLine &
                               " From ""ODRF"" A " & vbNewLine &
                               " INNER JOIN ""NNM1"" B ON B.""ObjectCode""=A.""ObjType"" AND B.""Series""=A.""Series"" " & vbNewLine &
                               " WHERE A.""DocEntry""='" + DLEntry + "' "
                            ElseIf ObjType = "1250000001" Then
                                qstr = "SELECT B.""SeriesName"",CAST(A.""DocNum"" AS VARCHAR) ""DocNum"",B.""SeriesName""||'/'||CAST(A.""DocNum"" AS VARCHAR) ""StrDocNum"" " & vbNewLine &
                               " From ""OWTQ"" A " & vbNewLine &
                               " INNER JOIN ""NNM1"" B ON B.""ObjectCode""=A.""ObjType"" AND B.""Series""=A.""Series"" " & vbNewLine &
                               " WHERE A.""DocEntry""='" + DLEntry + "' "
                            End If
                            Dim PostrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                            PostrSet.DoQuery(qstr)
                            ReturnDocNo = ReturnDocNo + " - " + PostrSet.Fields.Item("StrDocNum").Value

                        End If
                    End If
                Next
                G_DI_Company.Disconnect()
                NewRow = dtTable.NewRow
                NewRow.Item("ReturnCode") = "0000"
                NewRow.Item("ReturnDocEntry") = ReturnDocNo
                NewRow.Item("ReturnObjType") = ReturnDocNo
                NewRow.Item("ReturnSeries") = ReturnDocNo
                NewRow.Item("ReturnDocNum") = ReturnDocNo
                NewRow.Item("ReturnMsg") = "Your Request No. " + ReturnDocNo + " successfully submitted"

                dtTable.Rows.Add(NewRow)
                Return Request.CreateResponse(HttpStatusCode.OK, dtTable)



            Catch __unusedException1__ As Exception
                Try
                    G_DI_Company.Disconnect()
                Catch ex1 As Exception
                End Try
                NewRow = dtTable.NewRow
                NewRow.Item("ReturnCode") = "-2222"
                NewRow.Item("ReturnMsg") = __unusedException1__.Message.ToString
                NewRow.Item("ReturnDocEntry") = "-1"
                NewRow.Item("ReturnObjType") = "-1"
                NewRow.Item("ReturnSeries") = "-1"
                NewRow.Item("ReturnDocNum") = "-1"
                dtTable.Rows.Add(NewRow)
                Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
            End Try

        End Function
        <Route("Api/PostStockTransferReceiptFromIntransit")>
        <HttpPost>
        Public Function PostStockTransferReceiptFromIntransit(ByVal TransferDetails As DTS_MODEL_ITRN_RCPTINT) As HttpResponseMessage

            Dim G_DI_Company As SAPbobsCOM.Company = Nothing
            Dim strRegnNo As String = ""
            dtTable.Columns.Add("ReturnCode")
            dtTable.Columns.Add("ReturnDocEntry")
            dtTable.Columns.Add("ReturnObjType")
            dtTable.Columns.Add("ReturnSeries")
            dtTable.Columns.Add("ReturnDocNum")
            dtTable.Columns.Add("ReturnMsg")
            Try
                Dim Branch As String = ""
                Dim UserID As String = ""


                If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                    Return Request.CreateResponse(HttpStatusCode.OK, dtError)
                End If

                Dim qstr As String = ""
                qstr = "SELECT ""U_SAPUNAME"" ,""U_SAPPWD"" " & vbNewLine &
                       " FROM ""OUSR"" A " & vbNewLine &
                       " WHERE A.""USER_CODE"" ='" + UserID + "'"
                Dim dRow As Data.DataRow
                dRow = DBSQL.getQueryDataRow(qstr, "")
                Dim SAPUserId As String = ""
                Dim SAPPassword As String = ""
                Try
                    SAPUserId = dRow.Item("U_SAPUNAME")
                    SAPPassword = dRow.Item("U_SAPPWD")
                Catch ex As Exception
                End Try
                Connection.ConnectSAP(G_DI_Company, SAPUserId, SAPPassword)

                rSet = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                qstr = "Select Top 1 N.""Series"",B.""BPLId"",B.""DfltResWhs"" ""WhsCode"" ,TO_CHAR(NOW(),'YYYYYMMDD') ""DocDate"" " & vbNewLine &
                                "  From ""NNM1"" N  " & vbNewLine &
                                "       Inner Join ""OFPR"" O On O.""Indicator""=N.""Indicator"" " & vbNewLine &
                                "       Inner Join ""OBPL"" B On B.""BPLId""=N.""BPLId"" " & vbNewLine &
                                " Where N.""ObjectCode""='67' " & vbNewLine &
                                "   And O.""PeriodStat"" In ('N','C') And N.""Locked""='N' AND N.""Remark""='" + Branch + "'  " & vbNewLine &
                                "   And TO_CHAR('" + TransferDetails.PostingDate + "','YYYYYMMDD') Between TO_CHAR(O.""F_RefDate"",'YYYYYMMDD') And TO_CHAR(O.""T_RefDate"",'YYYYYMMDD')"
                'qstr = "Select Top 1 N.""Series"",B.""BPLId"",B.""DfltResWhs"" ""WhsCode"" ,TO_CHAR(GETDATE(),'YYYYYMMDD') ""DocDate"" " & vbNewLine &
                '                "  From ""NNM1"" N  " & vbNewLine &
                '                "       Inner Join ""OFPR"" O On O.""Indicator""=N.""Indicator"" " & vbNewLine &
                '                "       Inner Join ""OWHS"" W On W.""WhsCode""='" + TransferDetails.FromWarehouse.ToString + "' " & vbNewLine &
                '                "       Inner Join ""OBPL"" B On B.""BPLId""=W.""BPLId"" And B.""BPLId""=N.""BPLId"" " & vbNewLine &
                '                " Where N.""ObjectCode""='1250000001' " & vbNewLine &
                '                "   And O.""PeriodStat"" In ('N','C') And N.""Locked""='N' AND N.""Remark""='" + Branch + "'  " & vbNewLine &
                '                "   And TO_CHAR('" + TransferDetails.PostingDate + "','YYYYYMMDD') Between TO_CHAR(O.F_RefDate,'YYYYYMMDD') And TO_CHAR(O.T_RefDate,'YYYYYMMDD')"
                rSet.DoQuery(qstr)

                If rSet.RecordCount > 0 Then
                    'Dim CrtdDate As String = rSet.Fields.Item("DocDate").Value
                    Dim DocDate As String = TransferDetails.PostingDate
                    Dim StockTransfer As SAPbobsCOM.StockTransfer = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oStockTransfer)
                    StockTransfer.DocObjectCode = SAPbobsCOM.BoObjectTypes.oStockTransfer

                    'StockTransfer.CardCode = TransferDetails.CardCode
                    'StockTransfer.BPL_IDAssignedToInvoice = TransferDetails.Branch
                    StockTransfer.Series = rSet.Fields.Item("Series").Value
                    StockTransfer.DocDate = New Date(Mid(DocDate, 1, 4), Mid(DocDate, 5, 2), Mid(DocDate, 7, 2))
                    StockTransfer.DueDate = New Date(Mid(DocDate, 1, 4), Mid(DocDate, 5, 2), Mid(DocDate, 7, 2))
                    StockTransfer.TaxDate = New Date(Mid(DocDate, 1, 4), Mid(DocDate, 5, 2), Mid(DocDate, 7, 2))

                    StockTransfer.UserFields.Fields.Item("U_CRTDBY").Value = UserID
                    qstr = "SELECT * FROM ""WTR1"" WHERE ""DocEntry""='" + TransferDetails.DocEntry.ToString + "'"
                    Dim DetailsrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                    DetailsrSet.DoQuery(qstr)
                    While Not DetailsrSet.EoF
                        StockTransfer.FromWarehouse = DetailsrSet.Fields.Item("WhsCode").Value
                        StockTransfer.ToWarehouse = TransferDetails.ReceiptWarehouse
                        StockTransfer.Lines.BaseType = SAPbobsCOM.InvBaseDocTypeEnum.WarehouseTransfers
                        StockTransfer.Lines.BaseEntry = DetailsrSet.Fields.Item("DocEntry").Value
                        StockTransfer.Lines.BaseLine = DetailsrSet.Fields.Item("LineNum").Value
                        StockTransfer.Lines.ItemCode = DetailsrSet.Fields.Item("ItemCode").Value
                        StockTransfer.Lines.FromWarehouseCode = DetailsrSet.Fields.Item("WhsCode").Value
                        StockTransfer.Lines.WarehouseCode = TransferDetails.ReceiptWarehouse
                        StockTransfer.Lines.DistributionRule = Branch
                        Dim oItems As SAPbobsCOM.Items = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oItems)
                        oItems.GetByKey(DetailsrSet.Fields.Item("ItemCode").Value)
                        If oItems.ManageBatchNumbers = SAPbobsCOM.BoYesNoEnum.tYES Then
                            qstr = " SELECT * FROM ""IBT1"" WHERE ""BaseType""=67 and ""BaseEntry""='" + DetailsrSet.Fields.Item("DocEntry").Value.ToString + "' and ""BaseLinNum""='" + DetailsrSet.Fields.Item("LineNum").Value.ToString + "' AND ""WhsCode""='" + DetailsrSet.Fields.Item("WhsCode").Value + "'"
                            Dim DetailBatchsrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                            DetailBatchsrSet.DoQuery(qstr)
                            Dim i As Integer = 0
                            While Not DetailBatchsrSet.EoF
                                StockTransfer.Lines.BatchNumbers.SetCurrentLine(i)
                                StockTransfer.Lines.BatchNumbers.BatchNumber = DetailBatchsrSet.Fields.Item("BatchNum").Value
                                StockTransfer.Lines.BatchNumbers.Quantity = DetailBatchsrSet.Fields.Item("Quantity").Value
                                StockTransfer.Lines.BatchNumbers.Add()
                                i = i + 1
                                DetailBatchsrSet.MoveNext()
                            End While
                        End If
                        If oItems.ManageSerialNumbers = SAPbobsCOM.BoYesNoEnum.tYES Then
                            qstr = "SELECT * FROM ""SRI1"" WHERE ""BaseType"" ='67' and ""BaseEntry"" ='" + DetailsrSet.Fields.Item("DocEntry").Value.ToString + "' and ""BaseLinNum""='" + DetailsrSet.Fields.Item("LineNum").Value.ToString + "' and ""WhsCode"" ='" + DetailsrSet.Fields.Item("WhsCode").Value + "'"
                            'qstr = " SELECT * FROM ""IBT1"" WHERE ""BaseType""=67 and ""BaseEntry""='" + DetailsrSet.Fields.Item("DocEntry").Value + "' and ""BaseLinNum""='" + DetailsrSet.Fields.Item("LineNum").Value + "' AND ""WhsCode""='" + DetailsrSet.Fields.Item("WhsCode").Value + "'"
                            Dim DetailSerialsrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                            DetailSerialsrSet.DoQuery(qstr)
                            Dim i As Integer = 0
                            While Not DetailSerialsrSet.EoF
                                StockTransfer.Lines.SerialNumbers.SetCurrentLine(i)
                                'StockTransfer.Lines.SerialNumbers.InternalSerialNumber = Serial.InternalSerialNumber
                                StockTransfer.Lines.SerialNumbers.SystemSerialNumber = DetailSerialsrSet.Fields.Item("SysSerial").Value
                                StockTransfer.Lines.SerialNumbers.Add()
                                i = i + 1
                                DetailSerialsrSet.MoveNext()
                            End While
                        End If
                        StockTransfer.Lines.Add()
                        DetailsrSet.MoveNext()
                    End While


                    Dim lRetCode As Integer
                    'Dim s = Delivery.GetAsXML
                    lRetCode = StockTransfer.Add
                    Dim ErrorMessage As String = ""
                    If lRetCode <> 0 Then
                        Dim sErrMsg As String = G_DI_Company.GetLastErrorDescription()
                        G_DI_Company.GetLastError(lRetCode, sErrMsg)
                        ErrorMessage = sErrMsg
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(StockTransfer)
                        G_DI_Company.Disconnect()
                        NewRow = dtTable.NewRow
                        NewRow.Item("ReturnCode") = "-2222"
                        NewRow.Item("ReturnDocEntry") = "-1"
                        NewRow.Item("ReturnObjType") = "-1"
                        NewRow.Item("ReturnSeries") = "-1"
                        NewRow.Item("ReturnDocNum") = "-1"
                        NewRow.Item("ReturnMsg") = ErrorMessage
                        dtTable.Rows.Add(NewRow)
                        Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                    Else
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(StockTransfer)
                        Dim ObjType As String = G_DI_Company.GetNewObjectType.ToString.TrimEnd
                        Dim DLEntry As String = G_DI_Company.GetNewObjectKey.Trim.ToString
                        Dim ErrMsg As String = ""

                        If ObjType = "112" Then
                            qstr = "SELECT B.""SeriesName"",CAST(A.""DocNum"" AS VARCHAR) ""DocNum"",B.""SeriesName""||'/'||CAST(A.""DocNum"" AS VARCHAR) ""StrDocNum"" " & vbNewLine &
                               " From ""ODRF"" A " & vbNewLine &
                               " INNER JOIN ""NNM1"" B ON B.""ObjectCode""=A.""ObjType"" AND B.""Series""=A.""Series"" " & vbNewLine &
                               " WHERE A.""DocEntry""='" + DLEntry + "' "
                        ElseIf ObjType = "67" Then
                            qstr = "SELECT B.""SeriesName"",CAST(A.""DocNum"" AS VARCHAR) ""DocNum"",B.""SeriesName""||'/'||CAST(A.""DocNum"" AS VARCHAR) ""StrDocNum"" " & vbNewLine &
                               " From ""OWTR"" A " & vbNewLine &
                               " INNER JOIN ""NNM1"" B ON B.""ObjectCode""=A.""ObjType"" AND B.""Series""=A.""Series"" " & vbNewLine &
                               " WHERE A.""DocEntry""='" + DLEntry + "' "
                        End If
                        Dim PostrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                        PostrSet.DoQuery(qstr)
                        Dim ReturnDocNo = PostrSet.Fields.Item("StrDocNum").Value
                        G_DI_Company.Disconnect()
                        NewRow = dtTable.NewRow
                        NewRow.Item("ReturnCode") = "0000"
                        NewRow.Item("ReturnDocEntry") = DLEntry
                        NewRow.Item("ReturnObjType") = ObjType
                        NewRow.Item("ReturnSeries") = PostrSet.Fields.Item("SeriesName").Value
                        NewRow.Item("ReturnDocNum") = PostrSet.Fields.Item("DocNum").Value
                        If ObjType = "112" Then
                            NewRow.Item("ReturnMsg") = "Your Request No for Approval. " + ReturnDocNo + " successfully submitted"
                        Else
                            NewRow.Item("ReturnMsg") = "Your Request No. " + ReturnDocNo + " successfully submitted"
                        End If

                        dtTable.Rows.Add(NewRow)
                        Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                    End If
                Else
                    G_DI_Company.Disconnect()
                    NewRow = dtTable.NewRow
                    NewRow.Item("ReturnCode") = "-2222"
                    NewRow.Item("ReturnDocEntry") = "-1"
                    NewRow.Item("ReturnObjType") = "-1"
                    NewRow.Item("ReturnSeries") = "-1"
                    NewRow.Item("ReturnDocNum") = "-1"
                    NewRow.Item("ReturnMsg") = "Series not found"
                    dtTable.Rows.Add(NewRow)
                    Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                End If

            Catch __unusedException1__ As Exception
                Try
                    G_DI_Company.Disconnect()
                Catch ex1 As Exception
                End Try
                NewRow = dtTable.NewRow
                NewRow.Item("ReturnCode") = "-2222"
                NewRow.Item("ReturnMsg") = __unusedException1__.Message.ToString
                NewRow.Item("ReturnDocEntry") = "-1"
                NewRow.Item("ReturnObjType") = "-1"
                NewRow.Item("ReturnSeries") = "-1"
                NewRow.Item("ReturnDocNum") = "-1"
                dtTable.Rows.Add(NewRow)
                Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
            End Try

        End Function

        <Route("Api/PostStockTransfer")>
        <HttpPost>
        Public Function PostStockTransfer(ByVal TransferDetails As DTS_MODEL_TRNS_HEADER) As HttpResponseMessage

            Dim G_DI_Company As SAPbobsCOM.Company = Nothing
            Dim strRegnNo As String = ""
            dtTable.Columns.Add("ReturnCode")
            dtTable.Columns.Add("ReturnDocEntry")
            dtTable.Columns.Add("ReturnObjType")
            dtTable.Columns.Add("ReturnSeries")
            dtTable.Columns.Add("ReturnDocNum")
            dtTable.Columns.Add("ReturnMsg")


            Try
                Dim Branch As String = ""
                Dim UserID As String = ""

                If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                    Return Request.CreateResponse(HttpStatusCode.OK, dtError)
                End If
                'UserID = "SAPUser1"


                Dim qstr As String = ""
                qstr = "SELECT ""U_SAPUNAME"" ,""U_SAPPWD"" " & vbNewLine &
                       " FROM ""OUSR"" A " & vbNewLine &
                       " WHERE A.""USER_CODE"" ='" + UserID + "'"
                Dim dRow As Data.DataRow
                dRow = DBSQL.getQueryDataRow(qstr, "")
                Dim SAPUserId As String = ""
                Dim SAPPassword As String = ""
                Try
                    SAPUserId = dRow.Item("U_SAPUNAME")
                    SAPPassword = dRow.Item("U_SAPPWD")
                Catch ex As Exception
                End Try
                Connection.ConnectSAP(G_DI_Company, SAPUserId, SAPPassword)

                rSet = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                qstr = "Select Top 1 N.""Series"",B.""BPLId"",B.""DfltResWhs"" ""WhsCode"" ,TO_CHAR(NOW(),'YYYYYMMDD') ""DocDate"" " & vbNewLine &
                                "  From ""NNM1"" N  " & vbNewLine &
                                "       Inner Join ""OFPR"" O On O.""Indicator""=N.""Indicator"" " & vbNewLine &
                                "       Inner Join ""OBPL"" B On B.""BPLId""=N.""BPLId"" " & vbNewLine &
                                " Where N.""ObjectCode""='67' " & vbNewLine &
                                "   And O.""PeriodStat"" In ('N','C') And N.""Locked""='N' AND N.""Remark""='" + Branch + "'  " & vbNewLine &
                                "   And TO_CHAR('" + TransferDetails.PostingDate + "','YYYYYMMDD') Between TO_CHAR(O.""F_RefDate"",'YYYYYMMDD') And TO_CHAR(O.""T_RefDate"",'YYYYYMMDD')"
                rSet.DoQuery(qstr)

                If rSet.RecordCount > 0 Then
                    Dim DocDate As String = TransferDetails.PostingDate
                    Dim DueDate As String = TransferDetails.DueDate
                    Dim StockTransfer As SAPbobsCOM.StockTransfer = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oStockTransfer)
                    StockTransfer.DocObjectCode = SAPbobsCOM.BoObjectTypes.oStockTransfer

                    StockTransfer.CardCode = IIf(TransferDetails.CardCode Is Nothing, "", TransferDetails.CardCode)
                    'StockTransfer.BPL_IDAssignedToInvoice = TransferDetails.Branch
                    StockTransfer.Series = rSet.Fields.Item("Series").Value
                    StockTransfer.DocDate = New Date(Mid(DocDate, 1, 4), Mid(DocDate, 5, 2), Mid(DocDate, 7, 2))
                    'StockTransfer.DocDueDate = New Date(Mid(DueDate, 1, 4), Mid(DueDate, 5, 2), Mid(DueDate, 7, 2))
                    StockTransfer.TaxDate = New Date(Mid(TransferDetails.RefDate, 1, 4), Mid(TransferDetails.RefDate, 5, 2), Mid(TransferDetails.RefDate, 7, 2))
                    'StockTransfer.ContactPerson = Convert.ToString(TransferDetails.ContactPerson)
                    If TransferDetails.ShiptoCode <> Nothing Then
                        StockTransfer.ShipToCode = TransferDetails.ShiptoCode
                    End If
                    If TransferDetails.ContactPerson <> Nothing Then
                        StockTransfer.ContactPerson = TransferDetails.ContactPerson
                    End If
                    If TransferDetails.SalesEmployee <> Nothing Then
                        StockTransfer.SalesPersonCode = TransferDetails.SalesEmployee
                    End If
                    StockTransfer.Comments = IIf(TransferDetails.Remarks Is Nothing, "", TransferDetails.Remarks)
                    StockTransfer.FromWarehouse = TransferDetails.FromWarehouse
                    StockTransfer.ToWarehouse = TransferDetails.ToWareHouse
                    StockTransfer.UserFields.Fields.Item("U_CRTDBY").Value = Convert.ToString(UserID)
                    StockTransfer.UserFields.Fields.Item("U_BRANCH").Value = Branch

                    For Each Item As DTS_MODEL_TRNS_ITEMS In TransferDetails.Items
                        If Item.BaseType <> Nothing Then
                            StockTransfer.Lines.BaseType = SAPbobsCOM.InvBaseDocTypeEnum.InventoryTransferRequest
                            StockTransfer.Lines.BaseEntry = Item.BaseEntry
                            StockTransfer.Lines.BaseLine = Item.BaseLine
                        End If
                        StockTransfer.Lines.ItemCode = Item.ItemCode
                        StockTransfer.Lines.FromWarehouseCode = Item.FromWareHouse
                        StockTransfer.Lines.WarehouseCode = Item.ToWareHouse
                        'Delivery.Lines.UserFields.Fields.Item("U_PACKQTY").Value = Item.PackingQuantity
                        'Delivery.Lines.UserFields.Fields.Item("U_PACKSIZE").Value = Item.PackingSize
                        StockTransfer.Lines.Quantity = Item.Quantity
                        StockTransfer.Lines.Price = Item.Price
                        StockTransfer.Lines.DistributionRule = Branch
                        StockTransfer.Lines.UserFields.Fields.Item("U_REMARKS").Value = IIf(Item.Remarks Is Nothing, "", Item.Remarks)
                        Dim oItems As SAPbobsCOM.Items = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oItems)
                        oItems.GetByKey(Item.ItemCode)
                        If oItems.ManageBatchNumbers = SAPbobsCOM.BoYesNoEnum.tYES Then
                            Dim i As Integer = 0
                            For Each Batches As DTS_MODEL_TRNS_BATCH In Item.Batches
                                If Batches.VisOrder = Item.VisOrder Then
                                    StockTransfer.Lines.BatchNumbers.SetCurrentLine(i)
                                    StockTransfer.Lines.BatchNumbers.BatchNumber = Batches.BatchNo
                                    StockTransfer.Lines.BatchNumbers.Quantity = Batches.BatchQuantity
                                    StockTransfer.Lines.BatchNumbers.Add()

                                    i = i + 1
                                End If
                            Next
                        End If
                        If oItems.ManageSerialNumbers = SAPbobsCOM.BoYesNoEnum.tYES Then
                            Dim i As Integer = 0
                            For Each Serial As DTS_MODEL_TRNS_SERIAL In Item.Serial
                                If Serial.VisOrder = Item.VisOrder Then
                                    StockTransfer.Lines.SerialNumbers.SetCurrentLine(i)
                                    StockTransfer.Lines.SerialNumbers.InternalSerialNumber = Serial.InternalSerialNumber
                                    StockTransfer.Lines.SerialNumbers.SystemSerialNumber = Serial.SystemSerialNumber
                                    StockTransfer.Lines.SerialNumbers.ManufacturerSerialNumber = Serial.ManufacturerSerialNumber
                                    StockTransfer.Lines.SerialNumbers.Add()
                                    i = i + 1
                                End If
                            Next
                        End If
                        StockTransfer.Lines.Add()
                    Next

                    Dim lRetCode As Integer
                    'Dim s = Delivery.GetAsXML
                    lRetCode = StockTransfer.Add
                    Dim ErrorMessage As String = ""
                    If lRetCode <> 0 Then
                        Dim sErrMsg As String = G_DI_Company.GetLastErrorDescription()
                        G_DI_Company.GetLastError(lRetCode, sErrMsg)
                        ErrorMessage = sErrMsg
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(StockTransfer)
                        G_DI_Company.Disconnect()
                        NewRow = dtTable.NewRow
                        NewRow.Item("ReturnCode") = "-2222"
                        NewRow.Item("ReturnDocEntry") = "-1"
                        NewRow.Item("ReturnObjType") = "-1"
                        NewRow.Item("ReturnSeries") = "-1"
                        NewRow.Item("ReturnDocNum") = "-1"
                        NewRow.Item("ReturnMsg") = ErrorMessage
                        dtTable.Rows.Add(NewRow)
                        Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                    Else
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(StockTransfer)
                        Dim ObjType As String = G_DI_Company.GetNewObjectType.ToString.TrimEnd
                        Dim DLEntry As String = G_DI_Company.GetNewObjectKey.Trim.ToString
                        Dim ErrMsg As String = ""

                        If ObjType = "112" Then
                            qstr = "SELECT B.""SeriesName"",CAST(A.""DocNum"" AS VARCHAR) ""DocNum"",B.""SeriesName""||'/'||CAST(A.""DocNum"" AS VARCHAR) ""StrDocNum"" " & vbNewLine &
                               " From ""ODRF"" A " & vbNewLine &
                               " INNER JOIN ""NNM1"" B ON B.""ObjectCode""=A.""ObjType"" AND B.""Series""=A.""Series"" " & vbNewLine &
                               " WHERE A.""DocEntry""='" + DLEntry + "' "
                        ElseIf ObjType = "67" Then
                            qstr = "SELECT B.""SeriesName"",CAST(A.""DocNum"" AS VARCHAR) ""DocNum"",B.""SeriesName""||'/'||CAST(A.""DocNum"" AS VARCHAR) ""StrDocNum"" " & vbNewLine &
                               " From ""OWTR"" A " & vbNewLine &
                               " INNER JOIN ""NNM1"" B ON B.""ObjectCode""=A.""ObjType"" AND B.""Series""=A.""Series"" " & vbNewLine &
                               " WHERE A.""DocEntry""='" + DLEntry + "' "
                        End If
                        Dim PostrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                        PostrSet.DoQuery(qstr)
                        Dim ReturnDocNo = PostrSet.Fields.Item("StrDocNum").Value
                        G_DI_Company.Disconnect()
                        NewRow = dtTable.NewRow
                        NewRow.Item("ReturnCode") = "0000"
                        NewRow.Item("ReturnDocEntry") = DLEntry
                        NewRow.Item("ReturnObjType") = ObjType
                        NewRow.Item("ReturnSeries") = PostrSet.Fields.Item("SeriesName").Value
                        NewRow.Item("ReturnDocNum") = PostrSet.Fields.Item("DocNum").Value
                        If ObjType = "112" Then
                            NewRow.Item("ReturnMsg") = "Your Request No for Approval. " + ReturnDocNo + " successfully submitted"
                        Else
                            NewRow.Item("ReturnMsg") = "Your Request No. " + ReturnDocNo + " successfully submitted"
                        End If
                        dtTable.Rows.Add(NewRow)
                        Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                    End If
                Else
                    G_DI_Company.Disconnect()
                    NewRow = dtTable.NewRow
                    NewRow.Item("ReturnCode") = "-2222"
                    NewRow.Item("ReturnDocEntry") = "-1"
                    NewRow.Item("ReturnObjType") = "-1"
                    NewRow.Item("ReturnSeries") = "-1"
                    NewRow.Item("ReturnDocNum") = "-1"
                    NewRow.Item("ReturnMsg") = "Series not found"
                    dtTable.Rows.Add(NewRow)
                    Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                End If

            Catch __unusedException1__ As Exception
                Try
                    G_DI_Company.Disconnect()
                Catch ex1 As Exception
                End Try
                NewRow = dtTable.NewRow
                NewRow.Item("ReturnCode") = "-2222"
                NewRow.Item("ReturnMsg") = __unusedException1__.Message.ToString
                NewRow.Item("ReturnDocEntry") = "-1"
                NewRow.Item("ReturnObjType") = "-1"
                NewRow.Item("ReturnSeries") = "-1"
                NewRow.Item("ReturnDocNum") = "-1"
                dtTable.Rows.Add(NewRow)
                Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
            End Try

        End Function

        <Route("Api/PostDoctorsPrescription")>
        <HttpPost>
        Public Function PostDoctorsPrescription(ByVal Prescription As DTS_MODEL_SQ_HEADER) As HttpResponseMessage

            Dim G_DI_Company As SAPbobsCOM.Company = Nothing
            Dim strRegnNo As String = ""
            dtTable.Columns.Add("ReturnCode")
            dtTable.Columns.Add("ReturnDocEntry")
            dtTable.Columns.Add("ReturnObjType")
            dtTable.Columns.Add("ReturnSeries")
            dtTable.Columns.Add("ReturnDocNum")
            dtTable.Columns.Add("ReturnMsg")
            Try
                Dim Branch As String = ""
                Dim UserID As String = ""

                If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                    Return Request.CreateResponse(HttpStatusCode.OK, dtError)
                End If

                Dim qstr As String = ""
                qstr = "SELECT ""U_SAPUNAME"" ,""U_SAPPWD"" " & vbNewLine &
                       " FROM ""OUSR"" A " & vbNewLine &
                       " WHERE A.""USER_CODE"" ='" + UserID + "'"
                Dim dRow As Data.DataRow
                dRow = DBSQL.getQueryDataRow(qstr, "")
                Dim SAPUserId As String = ""
                Dim SAPPassword As String = ""
                Try
                    SAPUserId = dRow.Item("U_SAPUNAME")
                    SAPPassword = dRow.Item("U_SAPPWD")
                Catch ex As Exception
                End Try
                Connection.ConnectSAP(G_DI_Company, SAPUserId, SAPPassword)

                rSet = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                qstr = "Select Top 1 N.""Series"",B.""BPLId"",C.""WhsCode"" ""WhsCode"" ,TO_CHAR(NOW(),'YYYYYMMDD') ""DocDate"" " & vbNewLine &
                                "  From ""NNM1"" N  " & vbNewLine &
                                "       Inner Join ""OFPR"" O On O.""Indicator""=N.""Indicator"" " & vbNewLine &
                                "       Inner Join ""OBPL"" B On B.""BPLId""=N.""BPLId"" " & vbNewLine &
                                "       Inner Join ""OWHS"" C On C.""U_BUSUNIT""='" + Branch + "' AND C.""U_WHSTYPE""='N' " & vbNewLine &
                                " Where N.""ObjectCode""='23' " & vbNewLine &
                                "   And O.""PeriodStat"" In ('N','C') And N.""Locked""='N' AND N.""Remark""='" + Branch + "'  " & vbNewLine &
                                "   And TO_CHAR('" + Prescription.PostingDate + "','YYYYYMMDD') Between TO_CHAR(O.""F_RefDate"",'YYYYYMMDD') And TO_CHAR(O.""T_RefDate"",'YYYYYMMDD')"
                rSet.DoQuery(qstr)

                If rSet.RecordCount > 0 Then
                    G_DI_Company.StartTransaction()
                    Dim DocDate As String = Prescription.PostingDate
                    Dim Whscode As String = rSet.Fields.Item("WhsCode").Value
                    'Dim SlpCode As String = rSet.Fields.Item("SlpCode").Value
                    Dim SalesQt As SAPbobsCOM.Documents = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oQuotations)
                    SalesQt.DocObjectCode = SAPbobsCOM.BoObjectTypes.oQuotations

                    SalesQt.CardCode = Prescription.CardCode
                    'SalesOrder.UserFields.Fields.Item("U_USERID").Value = OrderDetails.UserId
                    'SalesOrder.SalesPersonCode = SlpCode
                    SalesQt.BPL_IDAssignedToInvoice = rSet.Fields.Item("BPLId").Value
                    SalesQt.Series = rSet.Fields.Item("Series").Value
                    SalesQt.DocDate = New Date(Mid(DocDate, 1, 4), Mid(DocDate, 5, 2), Mid(DocDate, 7, 2))
                    SalesQt.DocDueDate = New Date(Mid(DocDate, 1, 4), Mid(DocDate, 5, 2), Mid(DocDate, 7, 2))
                    SalesQt.TaxDate = New Date(Mid(Prescription.RefDate, 1, 4), Mid(Prescription.RefDate, 5, 2), Mid(Prescription.RefDate, 7, 2))
                    SalesQt.NumAtCard = Prescription.RefNo.ToString
                    'SalesQt.ShipToCode = Prescription.ShiptoCode.ToString
                    'SalesQt.PayToCode = Prescription.BilltoCode.ToString
                    SalesQt.Comments = Prescription.Remarks.ToString
                    SalesQt.UserFields.Fields.Item("U_CRTDBY").Value = UserID.ToString
                    SalesQt.UserFields.Fields.Item("U_PATIEAGE").Value = Prescription.PatientAge
                    SalesQt.UserFields.Fields.Item("U_PATCONCE").Value = Prescription.PatientConcern.ToString
                    SalesQt.UserFields.Fields.Item("U_DOCCOMNT").Value = Prescription.DoctorsComment.ToString
                    SalesQt.UserFields.Fields.Item("U_DOCSUGST").Value = Prescription.DoctorSuggestion.ToString
                    SalesQt.UserFields.Fields.Item("U_DOCOBSV").Value = Prescription.DoctorObservation.ToString
                    SalesQt.UserFields.Fields.Item("U_DOCNAME").Value = Prescription.DoctorsCode.ToString
                    SalesQt.UserFields.Fields.Item("U_PHNO").Value = Prescription.PhoneNo.ToString
                    SalesQt.UserFields.Fields.Item("U_REFRENCE").Value = Prescription.ExternalDoctorsRef.ToString
                    SalesQt.UserFields.Fields.Item("U_BRANCH").Value = Branch

                    If Prescription.FollowupDate <> Nothing Then
                        SalesQt.UserFields.Fields.Item("U_FOLLOWUP").Value = New Date(Mid(Prescription.FollowupDate, 1, 4), Mid(Prescription.FollowupDate, 5, 2), Mid(Prescription.FollowupDate, 7, 2))
                    End If
                    If Prescription.InvoiceNo <> Nothing Then
                        SalesQt.UserFields.Fields.Item("U_INVNUM").Value = Prescription.InvoiceNo
                    End If

                    For Each Item As DTS_MODEL_SQ_ITEMS In Prescription.Items
                        SalesQt.Lines.ItemCode = Item.ItemCode
                        SalesQt.Lines.WarehouseCode = Whscode
                        If Item.Quantity <> Nothing Then
                            SalesQt.Lines.Quantity = Item.Quantity
                        Else
                            SalesQt.Lines.Quantity = 1
                        End If

                        If Item.Price <> 0 Then
                            SalesQt.Lines.Price = Item.Price
                        End If
                        'SalesQt.Lines.uo = Item.TaxCode
                        If Item.UOM <> "" Then
                            SalesQt.Lines.MeasureUnit = Item.UOM
                        End If
                        SalesQt.Lines.CostingCode = Branch
                        'SalesQt.Lines.UserFields.Fields.Item("U_TIMESDAY").Value = Item.TimesPerDay.ToString
                        'SalesQt.Lines.UserFields.Fields.Item("U_DAYS").Value = Item.Day.ToString
                        'SalesQt.Lines.UserFields.Fields.Item("U_DINNER").Value = Item.Dinner.ToString
                        'SalesQt.Lines.UserFields.Fields.Item("U_LUNCH").Value = Item.Lunch.ToString
                        'SalesQt.Lines.UserFields.Fields.Item("U_BRKFAST").Value = Item.Breakfast.ToString
                        SalesQt.Lines.UserFields.Fields.Item("U_DETAILS").Value = IIf(Item.Details.ToString Is Nothing, "", Item.Details.ToString)

                        SalesQt.Lines.Add()
                    Next

                    Dim lRetCode As Integer
                    lRetCode = SalesQt.Add

                    Dim ErrorMessage As String = ""
                    If lRetCode <> 0 Then
                        Dim sErrMsg As String = G_DI_Company.GetLastErrorDescription()
                        G_DI_Company.GetLastError(lRetCode, sErrMsg)
                        ErrorMessage = sErrMsg
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(SalesQt)
                        If G_DI_Company.InTransaction Then
                            G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                        End If
                        G_DI_Company.Disconnect()
                        NewRow = dtTable.NewRow
                        NewRow.Item("ReturnCode") = "-2222"
                        NewRow.Item("ReturnDocEntry") = "-1"
                        NewRow.Item("ReturnObjType") = "-1"
                        NewRow.Item("ReturnSeries") = "-1"
                        NewRow.Item("ReturnDocNum") = "-1"
                        NewRow.Item("ReturnMsg") = ErrorMessage
                        dtTable.Rows.Add(NewRow)
                        Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                    Else
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(SalesQt)
                        Dim ObjType As String = G_DI_Company.GetNewObjectType.ToString.TrimEnd
                        Dim SOEntry As String = G_DI_Company.GetNewObjectKey.Trim.ToString
                        qstr = "Select ""VisOrder"",""LineNum"",""ItemCode"" FROM ""QUT1"" WHERE ""DocEntry""='" + SOEntry + "' ORDER BY ""VisOrder"" "
                        Dim SoLinerSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                        SoLinerSet.DoQuery(qstr)
                        While Not SoLinerSet.EoF
                            qstr = "SELECT ""Code"" FROM ""ITT1"" WHERE ""Father""='" + SoLinerSet.Fields.Item("ItemCode").Value + "'"
                            Dim SoItemrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                            SoItemrSet.DoQuery(qstr)
                            If SoItemrSet.RecordCount > 0 Then
                                While Not SoItemrSet.EoF
                                    qstr = "Select ""VisOrder"",""ItemCode"" FROM ""QUT1"" WHERE ""DocEntry""='" + SOEntry + "' AND ""ItemCode""='" + SoItemrSet.Fields.Item("Code").Value + "'  AND ""VisOrder"">" + SoLinerSet.Fields.Item("VisOrder").Value.ToString + " ORDER BY ""VisOrder"" "
                                    Dim SoItemrSet1 As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                                    SoItemrSet1.DoQuery(qstr)
                                    qstr = "UPDATE ""QUT1"" SET ""U_SERVLINE""='" + SoLinerSet.Fields.Item("LineNum").Value.ToString + "',""U_ITEMHIDE""='Y' WHERE ""DocEntry""='" + SOEntry + "' and ""VisOrder""='" + SoItemrSet1.Fields.Item("VisOrder").Value.ToString + "' "
                                    rSet.DoQuery(qstr)

                                    SoItemrSet.MoveNext()
                                End While
                            Else
                                'Continue While
                            End If
                            SoLinerSet.MoveNext()
                        End While
                        Dim ErrMsg As String = ""

                        If ObjType = "112" Then
                            qstr = "SELECT B.""SeriesName"",CAST(A.""DocNum"" AS VARCHAR) ""DocNum"",B.""SeriesName""||'/'||CAST(A.""DocNum"" AS VARCHAR) ""StrDocNum"" " & vbNewLine &
                           " From ""ODRF"" A " & vbNewLine &
                           " INNER JOIN ""NNM1"" B ON B.""ObjectCode""=A.""ObjType"" AND B.""Series""=A.""Series"" " & vbNewLine &
                           " WHERE A.""DocEntry""='" + SOEntry + "' "
                        ElseIf ObjType = "23" Then
                            qstr = "SELECT B.""SeriesName"",CAST(A.""DocNum"" AS VARCHAR) ""DocNum"",B.""SeriesName""||'/'||CAST(A.""DocNum"" AS VARCHAR) ""StrDocNum"" " & vbNewLine &
                           " From ""OQUT"" A " & vbNewLine &
                           " INNER JOIN ""NNM1"" B ON B.""ObjectCode""=A.""ObjType"" AND B.""Series""=A.""Series"" " & vbNewLine &
                           " WHERE A.""DocEntry""='" + SOEntry + "' "
                        End If
                        Dim PostrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                        PostrSet.DoQuery(qstr)
                        'rSet.DoQuery(qstr)
                        Dim ReturnDocNo = PostrSet.Fields.Item("StrDocNum").Value
                        Try
                            Dim oGeneralService As SAPbobsCOM.GeneralService
                            Dim oGeneralData As SAPbobsCOM.GeneralData
                            Dim oChild As SAPbobsCOM.GeneralData
                            Dim oChildren As SAPbobsCOM.GeneralDataCollection
                            Dim oGeneralParams As SAPbobsCOM.GeneralDataParams
                            Dim oCompService As SAPbobsCOM.CompanyService = G_DI_Company.GetCompanyService()
                            oGeneralService = oCompService.GetGeneralService("PATHIST")
                            oGeneralData = oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData)



                            oGeneralData.SetProperty("Code", SOEntry)
                            'oGeneralData.SetProperty("U_CUSTID", Prescription.CardCode)
                            oGeneralData.SetProperty("U_BRANCH", Branch)
                            Dim VisOrder As Integer = 1
                            Try
                                If Prescription.Testing Is Nothing Then

                                Else
                                    For Each Item As DTS_MODEL_SQ_HSTDTLS In Prescription.Testing
                                        oChildren = oGeneralData.Child("DTS_HIST_DTL")
                                        oChild = oChildren.Add()
                                        'oChild.SetProperty("VisOrder", VisOrder)
                                        VisOrder = VisOrder + 1
                                        oChild.SetProperty("U_VITALSIGN", Item.TestCode)
                                        oChild.SetProperty("U_VALUE", Item.Value)

                                    Next
                                End If

                            Catch ex As Exception
                            End Try


                            Dim lrtCode = oGeneralService.Add(oGeneralData)

                            'Dim g = oGeneralService.GetDataInterface("DocEntry")
                            oGeneralParams = oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralDataParams)
                            oGeneralParams.SetProperty("Code", lrtCode.GetProperty("Code"))
                            oGeneralData = oGeneralService.GetByParams(oGeneralParams)

                            If lrtCode.GetProperty("Code").ToString <> SOEntry Then
                                Dim lErrCode As Integer
                                Dim erMessage = G_DI_Company.GetLastErrorDescription()
                                G_DI_Company.GetLastError(lErrCode, erMessage)
                                NewRow = dtTable.NewRow
                                NewRow.Item("ReturnCode") = "-2222"
                                NewRow.Item("ReturnDocEntry") = "-1"
                                NewRow.Item("ReturnObjType") = "-1"
                                NewRow.Item("ReturnSeries") = "-1"
                                NewRow.Item("ReturnDocNum") = "-1"
                                NewRow.Item("ReturnMsg") = erMessage
                                dtTable.Rows.Add(NewRow)
                                If G_DI_Company.InTransaction Then
                                    G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                                End If
                                G_DI_Company.Disconnect()
                                Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                            Else

                            End If
                        Catch ex As Exception
                            NewRow = dtTable.NewRow
                            NewRow.Item("ReturnCode") = "-2222"
                            NewRow.Item("ReturnDocEntry") = "-1"
                            NewRow.Item("ReturnObjType") = "-1"
                            NewRow.Item("ReturnSeries") = "-1"
                            NewRow.Item("ReturnDocNum") = "-1"
                            NewRow.Item("ReturnMsg") = ex.Message
                            dtTable.Rows.Add(NewRow)
                            If G_DI_Company.InTransaction Then
                                G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                            End If
                            G_DI_Company.Disconnect()
                            Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                        End Try
                        qstr = "UPDATE ""OQUT"" SET ""U_PATHIST""='" + SOEntry.ToString + "' WHERE ""DocEntry""='" + SOEntry + "'"
                        rSet.DoQuery(qstr)
                        G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit)
                        G_DI_Company.Disconnect()
                        NewRow = dtTable.NewRow
                        NewRow.Item("ReturnCode") = "0000"
                        NewRow.Item("ReturnDocEntry") = SOEntry
                        NewRow.Item("ReturnObjType") = ObjType
                        NewRow.Item("ReturnSeries") = PostrSet.Fields.Item("SeriesName").Value
                        NewRow.Item("ReturnDocNum") = PostrSet.Fields.Item("DocNum").Value
                        NewRow.Item("ReturnMsg") = "Your Request No. " + ReturnDocNo + " successfully submitted"
                        dtTable.Rows.Add(NewRow)
                        Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                    End If
                Else
                    If G_DI_Company.InTransaction Then
                        G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                    End If
                    G_DI_Company.Disconnect()
                    NewRow = dtTable.NewRow
                    NewRow.Item("ReturnCode") = "-2222"
                    NewRow.Item("ReturnDocEntry") = "-1"
                    NewRow.Item("ReturnObjType") = "-1"
                    NewRow.Item("ReturnSeries") = "-1"
                    NewRow.Item("ReturnDocNum") = "-1"
                    NewRow.Item("ReturnMsg") = "Series not found"
                    dtTable.Rows.Add(NewRow)
                    Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                End If

            Catch __unusedException1__ As Exception
                Try
                    If G_DI_Company.InTransaction Then
                        G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                    End If
                Catch ex As Exception

                End Try

                Try
                    G_DI_Company.Disconnect()
                Catch ex1 As Exception
                End Try
                NewRow = dtTable.NewRow
                NewRow.Item("ReturnCode") = "-2222"
                NewRow.Item("ReturnDocEntry") = "-1"
                NewRow.Item("ReturnObjType") = "-1"
                NewRow.Item("ReturnSeries") = "-1"
                NewRow.Item("ReturnDocNum") = "-1"
                NewRow.Item("ReturnMsg") = __unusedException1__.Message.ToString
                dtTable.Rows.Add(NewRow)
                Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
            End Try

        End Function

        <Route("Api/PostGoodsIssue")>
        <HttpPost>
        Public Function PostGoodsIssue(ByVal IssueDetails As DTS_MODEL_GI_HEADER) As HttpResponseMessage

            Dim G_DI_Company As SAPbobsCOM.Company = Nothing
            Dim strRegnNo As String = ""
            dtTable.Columns.Add("ReturnCode")
            dtTable.Columns.Add("ReturnDocEntry")
            dtTable.Columns.Add("ReturnObjType")
            dtTable.Columns.Add("ReturnSeries")
            dtTable.Columns.Add("ReturnDocNum")
            dtTable.Columns.Add("ReturnMsg")
            Try
                Dim Branch As String = ""
                Dim UserID As String = ""

                If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                    Return Request.CreateResponse(HttpStatusCode.OK, dtError)
                End If

                Dim qstr As String = ""
                qstr = "SELECT ""U_SAPUNAME"" ,""U_SAPPWD"" " & vbNewLine &
                       " FROM ""OUSR"" A " & vbNewLine &
                       " WHERE A.""USER_CODE"" ='" + UserID + "'"
                Dim dRow As Data.DataRow
                dRow = DBSQL.getQueryDataRow(qstr, "")
                Dim SAPUserId As String = ""
                Dim SAPPassword As String = ""
                Try
                    SAPUserId = dRow.Item("U_SAPUNAME")
                    SAPPassword = dRow.Item("U_SAPPWD")
                Catch ex As Exception
                End Try
                Connection.ConnectSAP(G_DI_Company, SAPUserId, SAPPassword)

                rSet = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                'qstr = "Select Top 1 N.Series,B.BPLId,B.DfltResWhs 'WhsCode' ,CONVERT(VARCHAR(10),GETDATE(),112) 'DocDate' " & vbNewLine &
                '                "  From NNM1 N  " & vbNewLine &
                '                "       Inner Join OFPR O On O.Indicator=N.Indicator " & vbNewLine &
                '                "       Inner Join OBPL B On B.BPLId=N.BPLId And B.BPLId=N.BPLId AND B.BPLId='" + IssueDetails.Branch.ToString + "' " & vbNewLine &
                '                " Where N.ObjectCode='60' " & vbNewLine &
                '                "   And O.PeriodStat In ('N','C') And N.Locked='N'  " & vbNewLine &
                '                "   And CONVERT(VARCHAR(10),'" + IssueDetails.PostingDate + "',112) Between Convert(Varchar(10), O.F_RefDate,112) And Convert(Varchar(10), O.T_RefDate,112)"

                qstr = "Select Top 1 N.""Series"",B.""BPLId"",C.""WhsCode"" ""WhsCode"" ,TO_CHAR(NOW(),'YYYYYMMDD') ""DocDate"" " & vbNewLine &
                                "  From ""NNM1"" N  " & vbNewLine &
                                "       Inner Join ""OFPR"" O On O.""Indicator""=N.""Indicator"" " & vbNewLine &
                                "       Inner Join ""OBPL"" B On B.""BPLId""=N.""BPLId"" " & vbNewLine &
                                "       Inner Join ""OWHS"" C On C.""U_BUSUNIT""='" + Branch + "' AND C.""U_WHSTYPE""='N' " & vbNewLine &
                                " Where N.""ObjectCode""='60' " & vbNewLine &
                                "   And O.""PeriodStat"" In ('N','C') And N.""Locked""='N' AND N.""Remark""='" + Branch + "'  " & vbNewLine &
                                "   And TO_CHAR('" + IssueDetails.PostingDate + "','YYYYYMMDD') Between TO_CHAR(O.""F_RefDate"",'YYYYYMMDD') And TO_CHAR(O.""T_RefDate"",'YYYYYMMDD')"
                rSet.DoQuery(qstr)

                If rSet.RecordCount > 0 Then
                    Dim DocDate As String = IssueDetails.PostingDate
                    Dim Whscode As String = rSet.Fields.Item("WhsCode").Value
                    Dim oGIIssue As SAPbobsCOM.Documents = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInventoryGenExit)
                    'oGIIssue.DocObjectCode = SAPbobsCOM.BoObjectTypes.oInventoryGenExit

                    'oGIIssue.BPL_IDAssignedToInvoice
                    oGIIssue.BPL_IDAssignedToInvoice = rSet.Fields.Item("BPLId").Value
                    oGIIssue.Series = rSet.Fields.Item("Series").Value
                    oGIIssue.DocDate = New Date(Mid(DocDate, 1, 4), Mid(DocDate, 5, 2), Mid(DocDate, 7, 2))
                    'StockTransfer.DocDueDate = New Date(Mid(DocDate, 1, 4), Mid(DocDate, 5, 2), Mid(DocDate, 7, 2))
                    oGIIssue.TaxDate = New Date(Mid(IssueDetails.RefDate, 1, 4), Mid(IssueDetails.RefDate, 5, 2), Mid(IssueDetails.RefDate, 7, 2))
                    oGIIssue.Comments = IssueDetails.Remarks

                    oGIIssue.UserFields.Fields.Item("U_CRTDBY").Value = UserID
                    oGIIssue.UserFields.Fields.Item("U_BRANCH").Value = Branch

                    For Each Item As DTS_MODEL_GI_ITEMS In IssueDetails.Items
                        oGIIssue.Lines.ItemCode = Item.ItemCode
                        oGIIssue.Lines.WarehouseCode = Whscode
                        oGIIssue.Lines.Quantity = Item.Quantity
                        If Item.UOM <> Nothing Then
                            oGIIssue.Lines.MeasureUnit = Item.UOM
                        End If
                        oGIIssue.Lines.Price = Item.Price
                        oGIIssue.Lines.CostingCode = Branch
                        If Item.EmployeeCostCenter <> Nothing Then
                            oGIIssue.Lines.CostingCode2 = Item.EmployeeCostCenter
                        End If
                        If Item.DepartmentCostCenter <> Nothing Then
                            oGIIssue.Lines.CostingCode3 = Item.DepartmentCostCenter
                        End If
                        If Item.MachineCostCenter <> Nothing Then
                            oGIIssue.Lines.CostingCode4 = Item.MachineCostCenter
                        End If
                        'If Item.CostCenter5 <> "" Then
                        '    oGIIssue.Lines.CostingCode5 = Item.CostCenter5
                        'End If
                        oGIIssue.Lines.UserFields.Fields.Item("U_REMARKS").Value = IIf(Item.Remarks Is Nothing, "", Item.Remarks)

                        Dim oItems As SAPbobsCOM.Items = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oItems)
                        oItems.GetByKey(Item.ItemCode)
                        If oItems.ManageBatchNumbers = SAPbobsCOM.BoYesNoEnum.tYES Then
                            Dim i As Integer = 0
                            For Each Batches As DTS_MODEL_GI_BATCH In Item.Batches
                                If Batches.VisOrder = Item.VisOrder Then
                                    'oGIIssue.Lines.BatchNumbers.SetCurrentLine(i)
                                    oGIIssue.Lines.BatchNumbers.BatchNumber = Batches.BatchNo
                                    oGIIssue.Lines.BatchNumbers.Quantity = Batches.BatchQuantity
                                    'If i <> 0 Then
                                    '    Delivery.Lines.BatchNumbers.Add()
                                    'End If
                                    oGIIssue.Lines.BatchNumbers.Add()

                                    i = i + 1
                                End If
                            Next
                        End If
                        If oItems.ManageSerialNumbers = SAPbobsCOM.BoYesNoEnum.tYES Then
                            Dim i As Integer = 0
                            For Each Serial As DTS_MODEL_GI_SERIAL In Item.Serial
                                If Serial.VisOrder = Item.VisOrder Then
                                    oGIIssue.Lines.SerialNumbers.SetCurrentLine(i)
                                    oGIIssue.Lines.SerialNumbers.InternalSerialNumber = Serial.InternalSerialNumber
                                    oGIIssue.Lines.SerialNumbers.SystemSerialNumber = Serial.SystemSerialNumber
                                    oGIIssue.Lines.SerialNumbers.Add()
                                    i = i + 1
                                End If
                            Next
                        End If
                        oGIIssue.Lines.Add()
                    Next

                    Dim lRetCode As Integer
                    'Dim s = Delivery.GetAsXML
                    lRetCode = oGIIssue.Add
                    Dim ErrorMessage As String = ""
                    If lRetCode <> 0 Then
                        Dim sErrMsg As String = G_DI_Company.GetLastErrorDescription()
                        G_DI_Company.GetLastError(lRetCode, sErrMsg)
                        ErrorMessage = sErrMsg
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(oGIIssue)
                        G_DI_Company.Disconnect()
                        NewRow = dtTable.NewRow
                        NewRow.Item("ReturnCode") = "-2222"
                        NewRow.Item("ReturnDocEntry") = "-1"
                        NewRow.Item("ReturnObjType") = "-1"
                        NewRow.Item("ReturnSeries") = "-1"
                        NewRow.Item("ReturnDocNum") = "-1"
                        NewRow.Item("ReturnMsg") = ErrorMessage
                        dtTable.Rows.Add(NewRow)
                        Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                    Else
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(oGIIssue)
                        Dim ObjType As String = G_DI_Company.GetNewObjectType.ToString.TrimEnd
                        Dim DLEntry As String = G_DI_Company.GetNewObjectKey.Trim.ToString
                        Dim ErrMsg As String = ""

                        If ObjType = "112" Then
                            qstr = "SELECT B.""SeriesName"",CAST(A.""DocNum"" AS VARCHAR) ""DocNum"",B.""SeriesName""||'/'||CAST(A.""DocNum"" AS VARCHAR) ""StrDocNum"" " & vbNewLine &
                               " From ""ODRF"" A " & vbNewLine &
                               " INNER JOIN ""NNM1"" B ON B.""ObjectCode""=A.""ObjType"" AND B.""Series""=A.""Series"" " & vbNewLine &
                               " WHERE A.""DocEntry""='" + DLEntry + "' "
                        ElseIf ObjType = "60" Then
                            qstr = "SELECT B.""SeriesName"",CAST(A.""DocNum"" AS VARCHAR) ""DocNum"",B.""SeriesName""||'/'||CAST(A.""DocNum"" AS VARCHAR) ""StrDocNum"" " & vbNewLine &
                               " From ""OIGE"" A " & vbNewLine &
                               " INNER JOIN ""NNM1"" B ON B.""ObjectCode""=A.""ObjType"" AND B.""Series""=A.""Series"" " & vbNewLine &
                               " WHERE A.""DocEntry""='" + DLEntry + "' "
                        End If
                        Dim PostrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                        PostrSet.DoQuery(qstr)
                        rSet.DoQuery(qstr)
                        Dim ReturnDocNo = rSet.Fields.Item("StrDocNum").Value
                        G_DI_Company.Disconnect()
                        NewRow = dtTable.NewRow
                        NewRow.Item("ReturnCode") = "0000"
                        NewRow.Item("ReturnDocEntry") = DLEntry
                        NewRow.Item("ReturnObjType") = ObjType
                        NewRow.Item("ReturnSeries") = rSet.Fields.Item("SeriesName").Value
                        NewRow.Item("ReturnDocNum") = rSet.Fields.Item("DocNum").Value
                        NewRow.Item("ReturnMsg") = "Your Request No. " + ReturnDocNo + " successfully submitted"
                        dtTable.Rows.Add(NewRow)
                        Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                    End If
                Else
                    G_DI_Company.Disconnect()
                    NewRow = dtTable.NewRow
                    NewRow.Item("ReturnCode") = "-2222"
                    NewRow.Item("ReturnDocEntry") = "-1"
                    NewRow.Item("ReturnObjType") = "-1"
                    NewRow.Item("ReturnSeries") = "-1"
                    NewRow.Item("ReturnDocNum") = "-1"
                    NewRow.Item("ReturnMsg") = "Series not found"
                    dtTable.Rows.Add(NewRow)
                    Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                End If

            Catch __unusedException1__ As Exception
                Try
                    G_DI_Company.Disconnect()
                Catch ex1 As Exception
                End Try
                NewRow = dtTable.NewRow
                NewRow.Item("ReturnCode") = "-2222"
                NewRow.Item("ReturnMsg") = __unusedException1__.Message.ToString
                NewRow.Item("ReturnDocEntry") = "-1"
                NewRow.Item("ReturnObjType") = "-1"
                NewRow.Item("ReturnSeries") = "-1"
                NewRow.Item("ReturnDocNum") = "-1"
                dtTable.Rows.Add(NewRow)
                Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
            End Try

        End Function

        <Route("Api/PostGoodsReceipt")>
        <HttpPost>
        Public Function PostGoodsReceipt(ByVal ReceiptDetails As DTS_MODEL_GR_HEADER) As HttpResponseMessage

            Dim G_DI_Company As SAPbobsCOM.Company = Nothing
            Dim strRegnNo As String = ""
            dtTable.Columns.Add("ReturnCode")
            dtTable.Columns.Add("ReturnDocEntry")
            dtTable.Columns.Add("ReturnObjType")
            dtTable.Columns.Add("ReturnSeries")
            dtTable.Columns.Add("ReturnDocNum")
            dtTable.Columns.Add("ReturnMsg")
            Try
                Dim Branch As String = ""
                Dim UserID As String = ""

                If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                    Return Request.CreateResponse(HttpStatusCode.OK, dtError)
                End If

                Dim qstr As String = ""
                qstr = "SELECT ""U_SAPUNAME"" ,""U_SAPPWD"" " & vbNewLine &
                       " FROM ""OUSR"" A " & vbNewLine &
                       " WHERE A.""USER_CODE"" ='" + UserID + "'"
                Dim dRow As Data.DataRow
                dRow = DBSQL.getQueryDataRow(qstr, "")
                Dim SAPUserId As String = ""
                Dim SAPPassword As String = ""
                Try
                    SAPUserId = dRow.Item("U_SAPUNAME")
                    SAPPassword = dRow.Item("U_SAPPWD")
                Catch ex As Exception
                End Try
                Connection.ConnectSAP(G_DI_Company, SAPUserId, SAPPassword)

                rSet = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                'qstr = "Select Top 1 N.Series,B.BPLId,B.DfltResWhs 'WhsCode' ,CONVERT(VARCHAR(10),GETDATE(),112) 'DocDate' " & vbNewLine &
                '                "  From NNM1 N  " & vbNewLine &
                '                "       Inner Join OFPR O On O.Indicator=N.Indicator " & vbNewLine &
                '                "       Inner Join OBPL B On B.BPLId=N.BPLId And B.BPLId=N.BPLId AND B.BPLId='" + IssueDetails.Branch.ToString + "' " & vbNewLine &
                '                " Where N.ObjectCode='60' " & vbNewLine &
                '                "   And O.PeriodStat In ('N','C') And N.Locked='N'  " & vbNewLine &
                '                "   And CONVERT(VARCHAR(10),'" + IssueDetails.PostingDate + "',112) Between Convert(Varchar(10), O.F_RefDate,112) And Convert(Varchar(10), O.T_RefDate,112)"

                qstr = "Select Top 1 N.""Series"",B.""BPLId"",C.""WhsCode"" ""WhsCode"" ,TO_CHAR(NOW(),'YYYYYMMDD') ""DocDate"" " & vbNewLine &
                                "  From ""NNM1"" N  " & vbNewLine &
                                "       Inner Join ""OFPR"" O On O.""Indicator""=N.""Indicator"" " & vbNewLine &
                                "       Inner Join ""OBPL"" B On B.""BPLId""=N.""BPLId"" " & vbNewLine &
                                "       Inner Join ""OWHS"" C On C.""U_BUSUNIT""='" + Branch + "' AND C.""U_WHSTYPE""='N' " & vbNewLine &
                                " Where N.""ObjectCode""='59' " & vbNewLine &
                                "   And O.""PeriodStat"" In ('N','C') And N.""Locked""='N' AND N.""Remark""='" + Branch + "'  " & vbNewLine &
                                "   And TO_CHAR('" + ReceiptDetails.PostingDate + "','YYYYYMMDD') Between TO_CHAR(O.""F_RefDate"",'YYYYYMMDD') And TO_CHAR(O.""T_RefDate"",'YYYYYMMDD')"
                rSet.DoQuery(qstr)

                If rSet.RecordCount > 0 Then
                    Dim DocDate As String = ReceiptDetails.PostingDate
                    Dim Whscode As String = rSet.Fields.Item("WhsCode").Value
                    Dim oGIIssue As SAPbobsCOM.Documents = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInventoryGenEntry)
                    'oGIIssue.DocObjectCode = SAPbobsCOM.BoObjectTypes.oInventoryGenExit

                    'oGIIssue.BPL_IDAssignedToInvoice
                    oGIIssue.BPL_IDAssignedToInvoice = rSet.Fields.Item("BPLId").Value
                    oGIIssue.Series = rSet.Fields.Item("Series").Value
                    oGIIssue.DocDate = New Date(Mid(DocDate, 1, 4), Mid(DocDate, 5, 2), Mid(DocDate, 7, 2))
                    'StockTransfer.DocDueDate = New Date(Mid(DocDate, 1, 4), Mid(DocDate, 5, 2), Mid(DocDate, 7, 2))
                    oGIIssue.TaxDate = New Date(Mid(ReceiptDetails.RefDate, 1, 4), Mid(ReceiptDetails.RefDate, 5, 2), Mid(ReceiptDetails.RefDate, 7, 2))
                    oGIIssue.Comments = ReceiptDetails.Remarks

                    oGIIssue.UserFields.Fields.Item("U_CRTDBY").Value = UserID
                    oGIIssue.UserFields.Fields.Item("U_BRANCH").Value = Branch

                    For Each Item As DTS_MODEL_GR_ITEMS In ReceiptDetails.Items
                        oGIIssue.Lines.ItemCode = Item.ItemCode
                        oGIIssue.Lines.WarehouseCode = Whscode
                        oGIIssue.Lines.Quantity = Item.Quantity
                        If Item.UOM <> Nothing Then
                            oGIIssue.Lines.MeasureUnit = Item.UOM
                        End If
                        oGIIssue.Lines.Price = Item.Price
                        oGIIssue.Lines.CostingCode = Branch
                        If Item.EmployeeCostCenter <> Nothing Then
                            oGIIssue.Lines.CostingCode2 = Item.EmployeeCostCenter
                        End If
                        If Item.DepartmentCostCenter <> Nothing Then
                            oGIIssue.Lines.CostingCode3 = Item.DepartmentCostCenter
                        End If
                        If Item.MachineCostCenter <> Nothing Then
                            oGIIssue.Lines.CostingCode4 = Item.MachineCostCenter
                        End If
                        'If Item.CostCenter5 <> "" Then
                        '    oGIIssue.Lines.CostingCode5 = Item.CostCenter5
                        'End If
                        oGIIssue.Lines.UserFields.Fields.Item("U_REMARKS").Value = IIf(Item.Remarks Is Nothing, "", Item.Remarks)

                        Dim oItems As SAPbobsCOM.Items = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oItems)
                        oItems.GetByKey(Item.ItemCode)
                        If oItems.ManageBatchNumbers = SAPbobsCOM.BoYesNoEnum.tYES Then
                            Dim i As Integer = 0
                            For Each Batches As DTS_MODEL_GR_BATCH In Item.Batches
                                If Batches.VisOrder = Item.VisOrder Then
                                    'oGIIssue.Lines.BatchNumbers.SetCurrentLine(i)
                                    oGIIssue.Lines.BatchNumbers.BatchNumber = Batches.BatchNo
                                    oGIIssue.Lines.BatchNumbers.Quantity = Batches.BatchQuantity
                                    'If i <> 0 Then
                                    '    Delivery.Lines.BatchNumbers.Add()
                                    'End If
                                    oGIIssue.Lines.BatchNumbers.Add()

                                    i = i + 1
                                End If
                            Next
                        End If
                        If oItems.ManageSerialNumbers = SAPbobsCOM.BoYesNoEnum.tYES Then
                            Dim i As Integer = 0
                            For Each Serial As DTS_MODEL_GR_SERIAL In Item.Serial
                                If Serial.VisOrder = Item.VisOrder Then
                                    oGIIssue.Lines.SerialNumbers.SetCurrentLine(i)
                                    oGIIssue.Lines.SerialNumbers.InternalSerialNumber = Serial.InternalSerialNumber
                                    oGIIssue.Lines.SerialNumbers.ManufacturerSerialNumber = Serial.ManufacturerSerialNumber
                                    oGIIssue.Lines.SerialNumbers.Add()
                                    i = i + 1
                                End If
                            Next
                        End If
                        oGIIssue.Lines.Add()
                    Next

                    Dim lRetCode As Integer
                    'Dim s = Delivery.GetAsXML
                    lRetCode = oGIIssue.Add
                    Dim ErrorMessage As String = ""
                    If lRetCode <> 0 Then
                        Dim sErrMsg As String = G_DI_Company.GetLastErrorDescription()
                        G_DI_Company.GetLastError(lRetCode, sErrMsg)
                        ErrorMessage = sErrMsg
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(oGIIssue)
                        G_DI_Company.Disconnect()
                        NewRow = dtTable.NewRow
                        NewRow.Item("ReturnCode") = "-2222"
                        NewRow.Item("ReturnDocEntry") = "-1"
                        NewRow.Item("ReturnObjType") = "-1"
                        NewRow.Item("ReturnSeries") = "-1"
                        NewRow.Item("ReturnDocNum") = "-1"
                        NewRow.Item("ReturnMsg") = ErrorMessage
                        dtTable.Rows.Add(NewRow)
                        Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                    Else
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(oGIIssue)
                        Dim ObjType As String = G_DI_Company.GetNewObjectType.ToString.TrimEnd
                        Dim DLEntry As String = G_DI_Company.GetNewObjectKey.Trim.ToString
                        Dim ErrMsg As String = ""

                        If ObjType = "112" Then
                            qstr = "SELECT B.""SeriesName"",CAST(A.""DocNum"" AS VARCHAR) ""DocNum"",B.""SeriesName""||'/'||CAST(A.""DocNum"" AS VARCHAR) ""StrDocNum"" " & vbNewLine &
                               " From ""ODRF"" A " & vbNewLine &
                               " INNER JOIN ""NNM1"" B ON B.""ObjectCode""=A.""ObjType"" AND B.""Series""=A.""Series"" " & vbNewLine &
                               " WHERE A.""DocEntry""='" + DLEntry + "' "
                        ElseIf ObjType = "59" Then
                            qstr = "SELECT B.""SeriesName"",CAST(A.""DocNum"" AS VARCHAR) ""DocNum"",B.""SeriesName""||'/'||CAST(A.""DocNum"" AS VARCHAR) ""StrDocNum"" " & vbNewLine &
                               " From ""OIGN"" A " & vbNewLine &
                               " INNER JOIN ""NNM1"" B ON B.""ObjectCode""=A.""ObjType"" AND B.""Series""=A.""Series"" " & vbNewLine &
                               " WHERE A.""DocEntry""='" + DLEntry + "' "
                        End If
                        Dim PostrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                        PostrSet.DoQuery(qstr)
                        rSet.DoQuery(qstr)
                        Dim ReturnDocNo = rSet.Fields.Item("StrDocNum").Value
                        G_DI_Company.Disconnect()
                        NewRow = dtTable.NewRow
                        NewRow.Item("ReturnCode") = "0000"
                        NewRow.Item("ReturnDocEntry") = DLEntry
                        NewRow.Item("ReturnObjType") = ObjType
                        NewRow.Item("ReturnSeries") = rSet.Fields.Item("SeriesName").Value
                        NewRow.Item("ReturnDocNum") = rSet.Fields.Item("DocNum").Value
                        NewRow.Item("ReturnMsg") = "Your Request No. " + ReturnDocNo + " successfully submitted"
                        dtTable.Rows.Add(NewRow)
                        Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                    End If
                Else
                    G_DI_Company.Disconnect()
                    NewRow = dtTable.NewRow
                    NewRow.Item("ReturnCode") = "-2222"
                    NewRow.Item("ReturnDocEntry") = "-1"
                    NewRow.Item("ReturnObjType") = "-1"
                    NewRow.Item("ReturnSeries") = "-1"
                    NewRow.Item("ReturnDocNum") = "-1"
                    NewRow.Item("ReturnMsg") = "Series not found"
                    dtTable.Rows.Add(NewRow)
                    Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                End If

            Catch __unusedException1__ As Exception
                Try
                    G_DI_Company.Disconnect()
                Catch ex1 As Exception
                End Try
                NewRow = dtTable.NewRow
                NewRow.Item("ReturnCode") = "-2222"
                NewRow.Item("ReturnMsg") = __unusedException1__.Message.ToString
                NewRow.Item("ReturnDocEntry") = "-1"
                NewRow.Item("ReturnObjType") = "-1"
                NewRow.Item("ReturnSeries") = "-1"
                NewRow.Item("ReturnDocNum") = "-1"
                dtTable.Rows.Add(NewRow)
                Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
            End Try

        End Function

        <Route("Api/PostBP")>
        <HttpPost>
        Public Function PostBP(ByVal BPDetails As SIL_MODEL_BP_HEADER) As HttpResponseMessage
            Dim G_DI_Company As SAPbobsCOM.Company = Nothing

            dtTable.Columns.Add("ReturnCode")
            dtTable.Columns.Add("ReturnDocEntry")
            dtTable.Columns.Add("ReturnObjType")
            dtTable.Columns.Add("ReturnSeries")
            dtTable.Columns.Add("ReturnDocNum")
            dtTable.Columns.Add("ReturnMsg")
            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim qstr As String = ""
            'Dim qstr As String = ""
            qstr = "SELECT ""U_SAPUNAME"" ,""U_SAPPWD"" " & vbNewLine &
                       " FROM ""OUSR"" A " & vbNewLine &
                       " WHERE A.""USER_CODE"" ='" + UserID + "'"
            Dim dRow As Data.DataRow
            dRow = DBSQL.getQueryDataRow(qstr, "")
            Dim SAPUserId As String = ""
            Dim SAPPassword As String = ""
            Try
                SAPUserId = dRow.Item("U_SAPUNAME")
                SAPPassword = dRow.Item("U_SAPPWD")
            Catch ex As Exception
            End Try
            Connection.ConnectSAP(G_DI_Company, SAPUserId, SAPPassword)
            Dim ErrMsg As String = ""
            Dim NewBPCode As String = ""
            Try

                If BPDetails.BPCode = "" Then
                    G_DI_Company.StartTransaction()
                    If BPAdd(G_DI_Company, BPDetails, UserID, Branch, NewBPCode, ErrMsg) = False Then
                        If G_DI_Company.InTransaction Then
                            G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                        End If
                        If G_DI_Company.Connected Then
                            G_DI_Company.Disconnect()
                        End If
                        NewRow = dtTable.NewRow
                        NewRow.Item("ReturnCode") = "2222"
                        NewRow.Item("ReturnDocEntry") = ""
                        NewRow.Item("ReturnObjType") = ""
                        NewRow.Item("ReturnSeries") = ""
                        NewRow.Item("ReturnDocNum") = ""
                        NewRow.Item("ReturnDocNum") = ""
                        NewRow.Item("ReturnMsg") = ErrMsg
                        dtTable.Rows.Add(NewRow)
                        Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                    Else
                        G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit)
                        If G_DI_Company.Connected Then
                            G_DI_Company.Disconnect()
                        End If
                        NewRow = dtTable.NewRow
                        NewRow.Item("ReturnCode") = "0000"
                        NewRow.Item("ReturnDocEntry") = NewBPCode
                        NewRow.Item("ReturnObjType") = ""
                        NewRow.Item("ReturnSeries") = ""
                        NewRow.Item("ReturnDocNum") = ""
                        NewRow.Item("ReturnDocNum") = ""
                        NewRow.Item("ReturnMsg") = "Buisness Partner creation successful with BPCode " + NewBPCode
                        dtTable.Rows.Add(NewRow)
                        Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                    End If
                ElseIf BPDetails.BPCode <> "" Then
                    'G_DI_Company.StartTransaction()
                    NewBPCode = BPDetails.BPCode
                    If BPUpdate(G_DI_Company, BPDetails, UserID, Branch, NewBPCode, ErrMsg) = False Then
                        If G_DI_Company.InTransaction Then
                            G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                        End If
                        If G_DI_Company.Connected Then
                            G_DI_Company.Disconnect()
                        End If
                        NewRow = dtTable.NewRow
                        NewRow.Item("ReturnCode") = "2222"
                        NewRow.Item("ReturnDocEntry") = ""
                        NewRow.Item("ReturnObjType") = ""
                        NewRow.Item("ReturnSeries") = ""
                        NewRow.Item("ReturnDocNum") = ""
                        NewRow.Item("ReturnDocNum") = ""
                        NewRow.Item("ReturnMsg") = ErrMsg
                        dtTable.Rows.Add(NewRow)
                        Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                    Else
                        ' G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit)


                        If G_DI_Company.Connected Then
                            G_DI_Company.Disconnect()
                        End If
                        NewRow = dtTable.NewRow
                        NewRow.Item("ReturnCode") = "0000"
                        NewRow.Item("ReturnDocEntry") = NewBPCode
                        NewRow.Item("ReturnObjType") = ""
                        NewRow.Item("ReturnSeries") = ""
                        NewRow.Item("ReturnDocNum") = ""
                        NewRow.Item("ReturnDocNum") = ""
                        If BPDetails.BPCode = "" Then
                            NewRow.Item("ReturnMsg") = "Buisness Partner creation successful with BPCode " + NewBPCode
                        Else
                            NewRow.Item("ReturnMsg") = NewBPCode + " updation successful"
                        End If

                        dtTable.Rows.Add(NewRow)
                        Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                    End If
                End If
            Catch ex As Exception
                If G_DI_Company.InTransaction Then
                    G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                End If
                If G_DI_Company.Connected Then
                    G_DI_Company.Disconnect()
                End If
                NewRow = dtTable.NewRow
                NewRow.Item("ReturnCode") = "-2222"
                NewRow.Item("ReturnMsg") = ex.Message
                dtTable.Rows.Add(NewRow)
                Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
            End Try


        End Function

        Public Function BPAdd(ByVal G_DI_Company As SAPbobsCOM.Company, ByVal BPDetails As SIL_MODEL_BP_HEADER, ByVal UserId As String, ByVal Branch As String,
                              ByRef NewBPCode As String, ByRef erMessage As String) As Boolean
            Try
                rSet = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                Dim Qstr As String = ""

                Dim oBPPartner As SAPbobsCOM.BusinessPartners = Nothing
                Dim oBPAddress As SAPbobsCOM.BPAddresses = Nothing
                Qstr = ""
                oBPPartner = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oBusinessPartners)
                oBPPartner.CardName = BPDetails.BPName
                If BPDetails.BPType = "L" Then
                    oBPPartner.CardType = SAPbobsCOM.BoCardTypes.cLid
                ElseIf BPDetails.BPType = "C" Then
                    oBPPartner.CardType = SAPbobsCOM.BoCardTypes.cCustomer
                Else
                    oBPPartner.CardType = SAPbobsCOM.BoCardTypes.cSupplier
                End If
                'oBPPartner.UserFields.Fields.Item("U_USERID").Value = UserId
                'oBPPartner.CardForeignName = BPDetails.BPForeignName
                Qstr = "SELECT A.""GroupCode"" " & vbNewLine &
                       " FROM ""OCRG"" A " & vbNewLine &
                       " WHERE A.""U_CUSTTYPE""='L' "
                rSet.DoQuery(Qstr)
                If BPDetails.BPType = "L" Then
                    oBPPartner.GroupCode = rSet.Fields.Item("GroupCode").Value
                    BPDetails.BPGroupCode = rSet.Fields.Item("GroupCode").Value
                Else
                    oBPPartner.GroupCode = BPDetails.BPGroupCode
                End If

                oBPPartner.Currency = "BDT"
                oBPPartner.AliasName = Branch
                oBPPartner.Notes = BPDetails.Remarks
                oBPPartner.UserFields.Fields.Item("U_CRTDBY").Value = UserId
                Try
                    If BPDetails.BirthDate <> "" Then
                        oBPPartner.UserFields.Fields.Item("U_BIRTHDT").Value = New Date(Mid(BPDetails.BirthDate, 1, 4), Mid(BPDetails.BirthDate, 5, 2), Mid(BPDetails.BirthDate, 7, 2))
                    End If
                Catch ex As Exception
                End Try

                oBPPartner.Phone1 = IIf(BPDetails.MobileNo Is Nothing, "", BPDetails.MobileNo)
                oBPPartner.Cellular = IIf(BPDetails.Emergency Is Nothing, "", BPDetails.Emergency)
                oBPPartner.EmailAddress = IIf(BPDetails.Email Is Nothing, "", BPDetails.Email)
                oBPPartner.Website = IIf(BPDetails.WebSite Is Nothing, "", BPDetails.WebSite)
                oBPPartner.PayTermsGrpCode = IIf(BPDetails.PaymentTerms Is Nothing, "", BPDetails.PaymentTerms)
                oBPPartner.CreditLimit = BPDetails.CreditLimit
                If BPDetails.Connected <> Nothing Then
                    oBPPartner.UserFields.Fields.Item("U_CONNECT").Value = BPDetails.Connected
                End If
                If BPDetails.Contact <> Nothing Then
                    oBPPartner.UserFields.Fields.Item("U_CNTCTPER").Value = BPDetails.Contact
                End If
                If BPDetails.Occupation <> "" Then
                    oBPPartner.UserFields.Fields.Item("U_OCUPTION").Value = BPDetails.Occupation
                End If
                If BPDetails.RelationShip <> "" Then
                    oBPPartner.UserFields.Fields.Item("U_RELATION").Value = BPDetails.RelationShip
                End If
                If BPDetails.HowDoYouHear <> Nothing Then
                    oBPPartner.UserFields.Fields.Item("U_HWABUS").Value = BPDetails.HowDoYouHear
                End If
                If BPDetails.ReasonBranchVisit <> Nothing Then
                    oBPPartner.UserFields.Fields.Item("U_RSBRVS").Value = BPDetails.ReasonBranchVisit
                End If
                If BPDetails.SalesEmployee <> "" Then
                    oBPPartner.SalesPersonCode = BPDetails.SalesEmployee
                End If
                If BPDetails.Gender <> Nothing Then
                    oBPPartner.UserFields.Fields.Item("U_GENDER").Value = BPDetails.Gender
                End If
                Dim Brnchctn As Integer = 1
                Dim Qstrbrnch As String = ""
                'For Each Brnch As SIL_MODEL_BP_BRANCH In BPDetails.Branch
                '    Qstrbrnch = Qstrbrnch + vbNewLine + "INSERT INTO [@SIL_MR_CNSBRNCH](Code ,LineId,U_BRANCH ) " & vbNewLine &
                '                " VALUES('CARDCODE','" + Brnchctn.ToString + "','" + Brnch.BrnchId + "')"
                '    Brnchctn = Brnchctn + 1
                'Next
                oBPPartner.BPBranchAssignment.BPLID = "1"
                'oBPPartner.BPBranchAssignment.Add()
                'oBPPartner.DefaultBranch = BPDetails.Branch
                Try
                    If BPDetails.BankCode <> "" Then
                        oBPPartner.BPBankAccounts.BankCode = BPDetails.BankCode
                        oBPPartner.BPBankAccounts.Country = "BD"
                    End If
                    If BPDetails.AccountHolderName <> "" Then
                        oBPPartner.BPBankAccounts.AccountName = BPDetails.AccountHolderName
                    End If

                    If BPDetails.BankAccountNo <> "" Then
                        oBPPartner.BPBankAccounts.AccountNo = BPDetails.BankAccountNo
                    End If
                    If BPDetails.BankSwiftCode <> "" Then
                        oBPPartner.BPBankAccounts.BICSwiftCode = BPDetails.BankSwiftCode
                    End If

                Catch ex As Exception
                End Try




                'oBPPartner.BPBankAccounts.BuildingFloorRoom = BPDetails.BankAddress
                'oBPPartner.BPBankAccounts.City = BPDetails.BankCity
                'oBPPartner.BPBankAccounts.State = BPDetails.BankState
                'oBPPartner.BPBankAccounts.ZipCode = BPDetails.BankPin

                oBPPartner.DownPaymentClearAct = "21010203"
                oBPPartner.DownPaymentInterimAccount = "21020207"

                Dim AddressCounter As Integer = 0
                Dim DefaultBillto As String = "N"
                Dim DefaultShipTo As String = "N"
                Dim DefaultContact As String = "N"
                Try
                    For Each Add As SIL_MODEL_BP_ADDRESS In BPDetails.Addresses
                        If AddressCounter > 0 Then
                            oBPPartner.Addresses.Add()
                        End If
                        If Add.AddressType = "S" Then
                            oBPPartner.Addresses.AddressType = SAPbobsCOM.BoAddressType.bo_ShipTo
                        ElseIf Add.AddressType = "B" Then
                            oBPPartner.Addresses.AddressType = SAPbobsCOM.BoAddressType.bo_BillTo
                        Else
                            'ErrorMsg = "AddressType must be S for ShipTo/ShipFrom B for PayTo/Bill To "
                            'Throw New Exception(ErrorMsg)
                            'Return False
                        End If

                        oBPPartner.Addresses.AddressName = "A1"
                        oBPPartner.Addresses.AddressName2 = IIf(Add.AddressLine2 Is Nothing, "", Add.AddressLine2)
                        oBPPartner.Addresses.AddressName3 = IIf(Add.AddressLine3 Is Nothing, "", Add.AddressLine3)
                        'oBPPartner.Addresses.BuildingFloorRoom = IIf(Add.AddressLine1 Is Nothing, "", Add.AddressLine1)
                        oBPPartner.Addresses.UserFields.Fields.Item("U_CUSTADD").Value = IIf(Add.AddressLine1 Is Nothing, "", Add.AddressLine1)
                        oBPPartner.Addresses.City = Add.City

                        oBPPartner.Addresses.ZipCode = IIf(Add.PinCode Is Nothing, "", Add.PinCode)
                        oBPPartner.Addresses.Country = "BD"
                        oBPPartner.Addresses.State = IIf(Add.State Is Nothing, "", Add.State)
                        'If Add.GSTN <> "" Then
                        '    oBPPartner.Addresses.GSTIN = Add.GSTN
                        '    oBPPartner.Addresses.GstType = SAPbobsCOM.BoGSTRegnTypeEnum.gstRegularTDSISD
                        'End If


                        'If Add.GSTTYPE = "2" Then
                        '    oBPPartner.Addresses.GstType = SAPbobsCOM.BoGSTRegnTypeEnum.gstCasualTaxablePerson
                        'ElseIf Add.GSTTYPE = "3" Then
                        '    oBPPartner.Addresses.GstType = SAPbobsCOM.BoGSTRegnTypeEnum.gstCompositionLevy
                        'ElseIf Add.GSTTYPE = "4" Then
                        '    oBPPartner.Addresses.GstType = SAPbobsCOM.BoGSTRegnTypeEnum.gstGoverDepartPSU
                        'ElseIf Add.GSTTYPE = "5" Then
                        '    oBPPartner.Addresses.GstType = SAPbobsCOM.BoGSTRegnTypeEnum.gstNonResidentTaxablePerson
                        'ElseIf Add.GSTTYPE = "1" Then
                        '    oBPPartner.Addresses.GstType = SAPbobsCOM.BoGSTRegnTypeEnum.gstRegularTDSISD
                        'ElseIf Add.GSTTYPE = "6" Then
                        '    oBPPartner.Addresses.GstType = SAPbobsCOM.BoGSTRegnTypeEnum.gstUNAgencyEmbassy
                        'End If

                        If Add.AddressType = "S" And Add.Defalult = "Y" Then
                            DefaultShipTo = Add.AddressLine1
                        ElseIf Add.AddressType = "B" And Add.Defalult = "Y" Then
                            DefaultBillto = Add.AddressLine1
                        End If
                        AddressCounter = AddressCounter + 1
                        'If BPDetails.Addresses.Count = 1 Then
                        '    oBPPartner.Addresses.Add()
                        'End If
                    Next
                Catch ex As Exception
                End Try

                If DefaultBillto <> "N" Then
                    oBPPartner.BilltoDefault = DefaultBillto
                End If
                If DefaultShipTo <> "N" Then
                    oBPPartner.ShipToDefault = DefaultShipTo
                End If


                Dim ContactEmployeeCounter As Integer = 0
                Try
                    For Each Cnt As SIL_MODEL_BP_CONTACTS In BPDetails.Contacts
                        If ContactEmployeeCounter > 0 Then
                            oBPPartner.ContactEmployees.Add()
                        End If
                        oBPPartner.ContactEmployees.Name = Cnt.ContactId
                        oBPPartner.ContactEmployees.Title = Cnt.Title
                        oBPPartner.ContactEmployees.FirstName = Cnt.FirstName
                        oBPPartner.ContactEmployees.LastName = Cnt.LastName
                        oBPPartner.ContactEmployees.E_Mail = Cnt.EmailId
                        oBPPartner.ContactEmployees.Phone1 = Cnt.MobileNo
                        oBPPartner.ContactEmployees.Phone2 = Cnt.AlternateMobileNo

                        oBPPartner.ContactEmployees.Position = Cnt.Position

                        'oBPPartner.ContactEmployees.E_Mail = Cnt.E_Mail
                        'oBPPartner.ContactEmployees.Remarks1 = Cnt.AadharNo
                        If Cnt.Defalult <> "N" Then
                            DefaultContact = Cnt.ContactId
                            oBPPartner.Phone1 = Cnt.MobileNo
                            oBPPartner.Phone2 = Cnt.AlternateMobileNo
                            oBPPartner.EmailAddress = Cnt.EmailId
                        End If
                        ContactEmployeeCounter = ContactEmployeeCounter + 1
                    Next
                Catch ex As Exception
                End Try

                If DefaultContact <> "N" Then
                    oBPPartner.ContactPerson = DefaultContact
                End If

                'oBPPartner.AgentCode = BPDetails.ConnectedVendor

                Qstr = "SELECT A.""U_ACCTCODE"",B.""Series"" " & vbNewLine &
                       " FROM ""OCRG"" A " & vbNewLine &
                       " INNER JOIN ""NNM1"" B ON B.""ObjectCode""='2' AND B.""Remark""=A.""U_CUSTTYPE"" " & vbNewLine &
                       " WHERE A.""GroupType""='C' " & vbNewLine &
                       "     AND A.""GroupCode""='" + BPDetails.BPGroupCode + "'"
                rSet.DoQuery(Qstr)
                oBPPartner.DebitorAccount = rSet.Fields.Item("U_ACCTCODE").Value
                oBPPartner.Series = rSet.Fields.Item("Series").Value


                'NewBPCode = rSet.Fields.Item("NEWBPCODE").Value
                'oBPPartner.CardCode = NewBPCode
                Dim lrtCode As Integer = 0
                lrtCode = oBPPartner.Add
                If lrtCode <> 0 Then
                    Dim sErrMs As String = G_DI_Company.GetLastErrorDescription()
                    G_DI_Company.GetLastError(lrtCode, sErrMs)
                    erMessage = sErrMs

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oBPPartner)
                    Return False
                Else
                    NewBPCode = G_DI_Company.GetNewObjectKey.Trim.ToString
                    'Qstr = "UPDATE ""OCRD"" " & vbNewLine &
                    '   " SET ""Phone1""='" + BPDetails.MobileNo + "',""E_Mail""='" + BPDetails.Email + "' " & vbNewLine &
                    '   " WHERE ""CardCode""='" + NewBPCode + "' "
                    'rSet.DoQuery(Qstr)
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oBPPartner)

                    oBPPartner = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oBusinessPartners)
                    If oBPPartner.GetByKey(NewBPCode) Then
                        oBPPartner.Phone1 = BPDetails.MobileNo
                        oBPPartner.EmailAddress = BPDetails.Email
                        Dim lrtCode1 As Integer = oBPPartner.Update
                        If lrtCode1 <> 0 Then
                            Dim sErrMs As String = G_DI_Company.GetLastErrorDescription()
                            G_DI_Company.GetLastError(lrtCode, sErrMs)
                            erMessage = sErrMs

                            System.Runtime.InteropServices.Marshal.ReleaseComObject(oBPPartner)
                            Return False
                        Else
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(oBPPartner)
                            Return True
                        End If

                    End If
                    Return True
                End If

            Catch __unusedException1__ As Exception

                erMessage = __unusedException1__.Message
                Return False
            End Try
        End Function

        Public Function BPUpdate(ByVal G_DI_Company As SAPbobsCOM.Company, ByVal BPDetails As SIL_MODEL_BP_HEADER, ByVal UserId As String, ByVal Branch As String,
                                 ByVal NewBPCode As String, ByRef erMessage As String) As Boolean
            Try
                rSet = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)

                Dim oBPPartner As SAPbobsCOM.BusinessPartners = Nothing
                Dim oBPAddress As SAPbobsCOM.BPAddresses = Nothing
                Dim Qstr As String


                oBPPartner = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oBusinessPartners)
                If oBPPartner.GetByKey(NewBPCode) = False Then
                    erMessage = "BP not Found"
                    Return True
                End If
                oBPPartner.ShipToDefault = Nothing
                oBPPartner.BilltoDefault = Nothing
                Dim lrtCode1 As Integer = 0
                For I As Integer = oBPPartner.Addresses.Count - 1 To 0 Step -1
                    oBPPartner.Addresses.SetCurrentLine(I)
                    oBPPartner.Addresses.Delete()
                Next

                'For I As Integer = oBPPartner.ContactEmployees.Count - 1 To 0 Step -1
                '    oBPPartner.ContactEmployees.SetCurrentLine(I)
                '    oBPPartner.ContactEmployees.Delete()
                'Next

                lrtCode1 = oBPPartner.Update
                If lrtCode1 <> 0 Then
                    Dim sErrMs As String = G_DI_Company.GetLastErrorDescription()
                    G_DI_Company.GetLastError(lrtCode1, sErrMs)
                    erMessage = sErrMs
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oBPPartner)
                    'G_DI_Company.Disconnect()
                    Return False
                End If

                'If BPDetails.PanNo <> "" Then
                '    'oBPPartner.FiscalTaxID.SetCurrentLine(0)
                '    'oBPPartner.FiscalTaxID.Address = DefaultShipTo

                '    ' oBPPartner.FiscalTaxID.AddrType = SAPbobsCOM.BoAddressType.bo_ShipTo
                '    oBPPartner.FiscalTaxID.TaxId0 = BPDetails.PanNo
                '    'oBPPartner.FiscalTaxID.Add()
                'End If

                'lrtCode1 = oBPPartner.Update
                'If lrtCode1 <> 0 Then
                '    Dim sErrMs As String = G_DI_Company.GetLastErrorDescription()
                '    G_DI_Company.GetLastError(lrtCode1, sErrMs)
                '    erMessage = sErrMs
                '    System.Runtime.InteropServices.Marshal.ReleaseComObject(oBPPartner)
                '    'G_DI_Company.Disconnect()
                '    Return False
                'End If

                'For I As Integer = oBPPartner.BPBranchAssignment.Count - 1 To 0 Step -1
                '    oBPPartner.BPBranchAssignment.SetCurrentLine(I)
                '    oBPPartner.BPBranchAssignment.Delete()
                'Next
                ' Dim x = oBPPartner.GetAsXML


                'System.Runtime.InteropServices.Marshal.ReleaseComObject(oBPPartner)
                'oBPPartner.GetByKey(NewBPCode)

                'For I As Integer = oBPPartner.Addresses.Count - 1 To 0 Step -1
                '    oBPPartner.Addresses.SetCurrentLine(I)
                '    oBPPartner.Addresses.Delete()
                'Next

                'For I As Integer = oBPPartner.ContactEmployees.Count - 1 To 0 Step -1
                '    oBPPartner.ContactEmployees.SetCurrentLine(I)
                '    oBPPartner.ContactEmployees.Delete()
                'Next

                oBPPartner.CardName = BPDetails.BPName
                'oBPPartner.UserFields.Fields.Item("U_USERID").Value = UserId
                If BPDetails.BPType = "L" Then
                    oBPPartner.CardType = SAPbobsCOM.BoCardTypes.cLid
                ElseIf BPDetails.BPType = "C" Then
                    oBPPartner.CardType = SAPbobsCOM.BoCardTypes.cCustomer
                Else
                    oBPPartner.CardType = SAPbobsCOM.BoCardTypes.cSupplier
                End If

                'oBPPartner.CardForeignName = BPDetails.BPForeignName
                oBPPartner.GroupCode = BPDetails.BPGroupCode
                oBPPartner.Currency = "BDT"

                oBPPartner.AliasName = Branch
                oBPPartner.Notes = BPDetails.Remarks
                oBPPartner.UserFields.Fields.Item("U_UPDTBY").Value = UserId

                oBPPartner.DownPaymentClearAct = "21010203"
                oBPPartner.DownPaymentInterimAccount = "21020207"
                oBPPartner.UserFields.Fields.Item("U_UPDTBY").Value = UserId
                Try
                    If BPDetails.BirthDate <> "" Then
                        oBPPartner.UserFields.Fields.Item("U_BIRTHDT").Value = New Date(Mid(BPDetails.BirthDate, 1, 4), Mid(BPDetails.BirthDate, 5, 2), Mid(BPDetails.BirthDate, 7, 2))
                    End If
                Catch ex As Exception
                End Try

                Try
                    If BPDetails.LeadtoCustomerDate <> Nothing Then
                        oBPPartner.UserFields.Fields.Item("U_LDTOCSDT").Value = New Date(Mid(BPDetails.LeadtoCustomerDate, 1, 4), Mid(BPDetails.LeadtoCustomerDate, 5, 2), Mid(BPDetails.LeadtoCustomerDate, 7, 2))
                        oBPPartner.UserFields.Fields.Item("U_LDTOCSBY").Value = UserId
                    End If
                Catch ex As Exception
                End Try


                oBPPartner.Phone1 = BPDetails.MobileNo
                oBPPartner.Cellular = BPDetails.Emergency
                oBPPartner.EmailAddress = BPDetails.Email
                oBPPartner.Website = BPDetails.WebSite
                oBPPartner.PayTermsGrpCode = BPDetails.PaymentTerms
                oBPPartner.CreditLimit = BPDetails.CreditLimit
                If BPDetails.Connected <> Nothing Then
                    oBPPartner.UserFields.Fields.Item("U_CONNECT").Value = BPDetails.Connected
                End If
                If BPDetails.Contact <> Nothing Then
                    oBPPartner.UserFields.Fields.Item("U_CNTCTPER").Value = BPDetails.Contact
                End If
                If BPDetails.Occupation <> Nothing Then
                    oBPPartner.UserFields.Fields.Item("U_OCUPTION").Value = BPDetails.Occupation
                End If
                If BPDetails.RelationShip <> Nothing Then
                    oBPPartner.UserFields.Fields.Item("U_RELATION").Value = BPDetails.RelationShip
                End If
                If BPDetails.HowDoYouHear <> Nothing Then
                    oBPPartner.UserFields.Fields.Item("U_HWABUS").Value = BPDetails.HowDoYouHear
                End If
                If BPDetails.ReasonBranchVisit <> Nothing Then
                    oBPPartner.UserFields.Fields.Item("U_RSBRVS").Value = BPDetails.ReasonBranchVisit
                End If
                If BPDetails.Gender <> Nothing Then
                    oBPPartner.UserFields.Fields.Item("U_GENDER").Value = BPDetails.Gender
                End If
                oBPPartner.SalesPersonCode = BPDetails.SalesEmployee
                Dim Brnchctn As Integer = 1
                Dim Qstrbrnch As String = ""


                Dim AddressCounter As Integer = 0
                Dim DefaultBillto As String = "N"
                Dim DefaultShipTo As String = "N"
                Dim DefaultContact As String = "N"
                Dim LOOPCount As Integer = 0
                Dim lOOPMatched As Boolean = False
                Try
                    For Each Add As SIL_MODEL_BP_ADDRESS In BPDetails.Addresses
                        If AddressCounter > 0 Then
                            oBPPartner.Addresses.Add()
                        End If
                        If Add.AddressType = "S" Then
                            oBPPartner.Addresses.AddressType = SAPbobsCOM.BoAddressType.bo_ShipTo
                        ElseIf Add.AddressType = "B" Then
                            oBPPartner.Addresses.AddressType = SAPbobsCOM.BoAddressType.bo_BillTo
                        Else
                            'ErrorMsg = "AddressType must be S for ShipTo/ShipFrom B for PayTo/Bill To "
                            'Throw New Exception(ErrorMsg)
                            'Return False
                        End If

                        oBPPartner.Addresses.AddressName = "A1"
                        oBPPartner.Addresses.AddressName2 = IIf(Add.AddressLine2 Is Nothing, "", Add.AddressLine2)
                        oBPPartner.Addresses.AddressName3 = IIf(Add.AddressLine3 Is Nothing, "", Add.AddressLine3)
                        'oBPPartner.Addresses.BuildingFloorRoom = IIf(Add.AddressLine1 Is Nothing, "", Add.AddressLine1)
                        oBPPartner.Addresses.UserFields.Fields.Item("U_CUSTADD").Value = IIf(Add.AddressLine1 Is Nothing, "", Add.AddressLine1)
                        oBPPartner.Addresses.City = Add.City

                        oBPPartner.Addresses.ZipCode = IIf(Add.PinCode Is Nothing, "", Add.PinCode)
                        oBPPartner.Addresses.Country = "BD"
                        oBPPartner.Addresses.State = IIf(Add.State Is Nothing, "", Add.State)
                        'If Add.GSTN <> "" Then
                        '    oBPPartner.Addresses.GSTIN = Add.GSTN
                        '    oBPPartner.Addresses.GstType = SAPbobsCOM.BoGSTRegnTypeEnum.gstRegularTDSISD
                        'End If


                        'If Add.GSTTYPE = "2" Then
                        '    oBPPartner.Addresses.GstType = SAPbobsCOM.BoGSTRegnTypeEnum.gstCasualTaxablePerson
                        'ElseIf Add.GSTTYPE = "3" Then
                        '    oBPPartner.Addresses.GstType = SAPbobsCOM.BoGSTRegnTypeEnum.gstCompositionLevy
                        'ElseIf Add.GSTTYPE = "4" Then
                        '    oBPPartner.Addresses.GstType = SAPbobsCOM.BoGSTRegnTypeEnum.gstGoverDepartPSU
                        'ElseIf Add.GSTTYPE = "5" Then
                        '    oBPPartner.Addresses.GstType = SAPbobsCOM.BoGSTRegnTypeEnum.gstNonResidentTaxablePerson
                        'ElseIf Add.GSTTYPE = "1" Then
                        '    oBPPartner.Addresses.GstType = SAPbobsCOM.BoGSTRegnTypeEnum.gstRegularTDSISD
                        'ElseIf Add.GSTTYPE = "6" Then
                        '    oBPPartner.Addresses.GstType = SAPbobsCOM.BoGSTRegnTypeEnum.gstUNAgencyEmbassy
                        'End If

                        If Add.AddressType = "S" And Add.Defalult = "Y" Then
                            DefaultShipTo = Add.AddressLine1
                        ElseIf Add.AddressType = "B" And Add.Defalult = "Y" Then
                            DefaultBillto = Add.AddressLine1
                        End If
                        AddressCounter = AddressCounter + 1
                        'If BPDetails.Addresses.Count = 1 Then
                        '    oBPPartner.Addresses.Add()
                        'End If
                    Next
                Catch ex As Exception
                End Try

                If DefaultBillto <> "N" Then
                    oBPPartner.BilltoDefault = DefaultBillto
                End If
                If DefaultShipTo <> "N" Then
                    oBPPartner.ShipToDefault = DefaultShipTo
                End If
                'Try
                '    For Each Add As SIL_MODEL_BP_ADDRESS In BPDetails.Addresses
                '        lOOPMatched = False
                '        'For Each Addr As SAPbobsCOM.BusinessPartners.In oBPPartner.Addresses

                '        'Next
                '        For i As Integer = 0 To oBPPartner.Addresses.Count - 1
                '            oBPPartner.Addresses.SetCurrentLine(i)
                '            If IIf(oBPPartner.Addresses.AddressType = 0, "S", "B") = Add.AddressType And oBPPartner.Addresses.AddressName = Add.AddressLine1 Then
                '                LOOPCount = LOOPCount + 1
                '                lOOPMatched = True

                '                'If AddressCounter > 0 Then
                '                '    oBPPartner.Addresses.Add()
                '                'End If
                '                If Add.AddressType = "S" Then
                '                    oBPPartner.Addresses.AddressType = SAPbobsCOM.BoAddressType.bo_ShipTo

                '                ElseIf Add.AddressType = "B" Then
                '                    oBPPartner.Addresses.AddressType = SAPbobsCOM.BoAddressType.bo_BillTo
                '                Else
                '                    'ErrorMsg = "AddressType must be S for ShipTo/ShipFrom B for PayTo/Bill To "
                '                    'Throw New Exception(ErrorMsg)
                '                    'Return False
                '                End If
                '                oBPPartner.Addresses.AddressName2 = Add.AddressLine2
                '                oBPPartner.Addresses.AddressName3 = Add.AddressLine3
                '                oBPPartner.Addresses.City = Add.City

                '                oBPPartner.Addresses.ZipCode = Add.PinCode
                '                oBPPartner.Addresses.Country = "BD"
                '                oBPPartner.Addresses.State = Add.State
                '                'oBPPartner.Addresses.GSTIN = Add.GSTN
                '                ' oBPPartner.Addresses.GstType = SAPbobsCOM.BoGSTRegnTypeEnum.gstRegularTDSISD


                '                If Add.AddressType = "S" And Add.Defalult = "Y" Then
                '                    DefaultShipTo = Add.AddressLine1
                '                ElseIf Add.AddressType = "B" And Add.Defalult = "Y" Then
                '                    DefaultBillto = Add.AddressLine1
                '                End If
                '                AddressCounter = AddressCounter + 1
                '                Exit For
                '            End If
                '        Next
                '        If LOOPCount = BPDetails.Addresses.Count Then
                '            Exit For
                '        End If
                '        If lOOPMatched = True Then
                '            Continue For
                '        End If
                '        If AddressCounter > 0 Then
                '            oBPPartner.Addresses.Add()
                '        End If
                '        If Add.AddressType = "S" Then
                '            oBPPartner.Addresses.AddressType = SAPbobsCOM.BoAddressType.bo_ShipTo

                '        ElseIf Add.AddressType = "B" Then
                '            oBPPartner.Addresses.AddressType = SAPbobsCOM.BoAddressType.bo_BillTo
                '        Else
                '            'ErrorMsg = "AddressType must be S for ShipTo/ShipFrom B for PayTo/Bill To "
                '            'Throw New Exception(ErrorMsg)
                '            'Return False
                '        End If

                '        oBPPartner.Addresses.AddressName = Add.AddressLine1
                '        oBPPartner.Addresses.AddressName2 = Add.AddressLine2
                '        oBPPartner.Addresses.AddressName3 = Add.AddressLine3
                '        oBPPartner.Addresses.City = Add.City

                '        oBPPartner.Addresses.ZipCode = Add.PinCode
                '        oBPPartner.Addresses.Country = "BD"
                '        oBPPartner.Addresses.State = Add.State
                '        'oBPPartner.Addresses.GSTIN = Add.GSTN
                '        ' oBPPartner.Addresses.GstType = SAPbobsCOM.BoGSTRegnTypeEnum.gstRegularTDSISD


                '        If Add.AddressType = "S" And Add.Defalult = "Y" Then
                '            DefaultShipTo = Add.AddressLine1
                '        ElseIf Add.AddressType = "B" And Add.Defalult = "Y" Then
                '            DefaultBillto = Add.AddressLine1
                '        End If

                '        AddressCounter = AddressCounter + 1
                '        'oBPPartner.Addresses.Add()
                '    Next
                'Catch ex As Exception
                'End Try

                'If DefaultBillto <> "N" Then
                '    oBPPartner.BilltoDefault = DefaultBillto
                'End If
                'If DefaultShipTo <> "N" Then
                '    oBPPartner.ShipToDefault = DefaultShipTo
                'End If

                Dim ContactEmployeeCounter As Integer = 0
                Dim ContactLoopMatched As Boolean = False
                Try
                    For Each Cnt As SIL_MODEL_BP_CONTACTS In BPDetails.Contacts
                        ContactLoopMatched = False
                        For i As Integer = 0 To oBPPartner.ContactEmployees.Count - 1
                            oBPPartner.ContactEmployees.SetCurrentLine(i)
                            If oBPPartner.ContactEmployees.Name = Cnt.ContactId Then
                                ContactEmployeeCounter = ContactEmployeeCounter + 1
                                ContactLoopMatched = True
                                oBPPartner.ContactEmployees.Title = Cnt.Title
                                oBPPartner.ContactEmployees.FirstName = Cnt.FirstName
                                oBPPartner.ContactEmployees.LastName = Cnt.LastName
                                oBPPartner.ContactEmployees.E_Mail = Cnt.EmailId
                                oBPPartner.ContactEmployees.Phone1 = Cnt.MobileNo
                                oBPPartner.ContactEmployees.Phone2 = Cnt.AlternateMobileNo
                                oBPPartner.ContactEmployees.Position = Cnt.Position
                                Exit For
                            End If
                        Next
                        If ContactLoopMatched = True Then
                            Continue For
                        End If
                        If ContactEmployeeCounter > 0 Then
                            oBPPartner.ContactEmployees.Add()
                        End If
                        oBPPartner.ContactEmployees.Name = Cnt.ContactId
                        oBPPartner.ContactEmployees.Title = Cnt.Title
                        oBPPartner.ContactEmployees.FirstName = Cnt.FirstName
                        oBPPartner.ContactEmployees.LastName = Cnt.LastName
                        oBPPartner.ContactEmployees.E_Mail = Cnt.EmailId
                        oBPPartner.ContactEmployees.Phone1 = Cnt.MobileNo
                        oBPPartner.ContactEmployees.Phone2 = Cnt.AlternateMobileNo

                        oBPPartner.ContactEmployees.Position = Cnt.Position

                        'oBPPartner.ContactEmployees.E_Mail = Cnt.E_Mail
                        'oBPPartner.ContactEmployees.Remarks1 = Cnt.AadharNo
                        If Cnt.Defalult <> "N" Then
                            DefaultContact = Cnt.ContactId
                            oBPPartner.Phone1 = Cnt.MobileNo
                            oBPPartner.Phone2 = Cnt.AlternateMobileNo
                            oBPPartner.EmailAddress = Cnt.EmailId
                        End If

                        'oBPPartner.ContactEmployees.
                        ContactEmployeeCounter = ContactEmployeeCounter + 1
                    Next
                Catch ex As Exception
                End Try

                If DefaultContact <> "N" Then
                    oBPPartner.ContactPerson = DefaultContact
                End If

                oBPPartner.BPBankAccounts.Country = "BD"
                oBPPartner.BPBankAccounts.BankCode = BPDetails.BankCode
                If BPDetails.AccountHolderName <> "" Then
                    oBPPartner.BPBankAccounts.AccountName = BPDetails.AccountHolderName
                End If

                oBPPartner.BPBankAccounts.AccountNo = BPDetails.BankAccountNo
                oBPPartner.BPBankAccounts.BICSwiftCode = BPDetails.BankSwiftCode
                'oBPPartner.BPBankAccounts.BuildingFloorRoom = BPDetails.BankAddress
                'oBPPartner.BPBankAccounts.City = BPDetails.BankCity
                'oBPPartner.BPBankAccounts.State = BPDetails.BankState
                'oBPPartner.BPBankAccounts.ZipCode = BPDetails.BankPin

                'oBPPartner.DownPaymentClearAct = "20010011"
                'oBPPartner.DownPaymentInterimAccount = "20010012"
                'Qstr = "SELECT A.""U_ACCTCODE"",B.""Series"" " & vbNewLine &
                '       " FROM ""OCRG"" A " & vbNewLine &
                '       " INNER JOIN ""NNM1"" B ON B.""ObjectCode""='2' AND B.""Remark""=A.""U_CUSTTYPE"" " & vbNewLine &
                '       " WHERE A.""GroupType""='C' " & vbNewLine &
                '       "     AND A.""GroupCode""='" + BPDetails.BPGroupCode + "'"
                'rSet.DoQuery(Qstr)
                'oBPPartner.DebitorAccount = rSet.Fields.Item("U_ACCTCODE").Value
                Dim lrtCode As Integer = 0
                ' Dim x = oBPPartner.GetAsXML
                lrtCode = oBPPartner.Update
                If lrtCode <> 0 Then
                    Dim sErrMs As String = G_DI_Company.GetLastErrorDescription()
                    G_DI_Company.GetLastError(lrtCode, sErrMs)
                    erMessage = sErrMs

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oBPPartner)
                    Return False
                Else
                    'For I As Integer = oBPPartner.FiscalTaxID.Count - 1 To 0 Step -1
                    '    oBPPartner.FiscalTaxID.SetCurrentLine(I)
                    '    If oBPPartner.FiscalTaxID.Address = DefaultShipTo And oBPPartner.FiscalTaxID.AddrType = SAPbobsCOM.BoAddressType.bo_ShipTo Then
                    '        oBPPartner.FiscalTaxID.SetCurrentLine(I)
                    '        If BPDetails.PanNo <> "" Then
                    '            oBPPartner.FiscalTaxID.TaxId0 = BPDetails.PanNo
                    '            'oBPPartner.FiscalTaxID.Add()
                    '            lrtCode = oBPPartner.Update
                    '            Return True
                    '        End If
                    '    End If
                    'Next
                    'For I As Integer = oBPPartner.Addresses.Count - 1 To 0 Step -1
                    '    oBPPartner.Addresses.SetCurrentLine(I)
                    '    If oBPPartner.Addresses.AddressName = DefaultShipTo And oBPPartner.Addresses.AddressType = SAPbobsCOM.BoAddressType.bo_ShipTo Then
                    '        If BPDetails.PanNo <> "" Then
                    '            'oBPPartner.FiscalTaxID.SetCurrentLine(0)
                    '            oBPPartner.FiscalTaxID.Address = DefaultShipTo

                    '            ' oBPPartner.FiscalTaxID.AddrType = SAPbobsCOM.BoAddressType.bo_ShipTo
                    '            oBPPartner.FiscalTaxID.TaxId0 = BPDetails.PanNo
                    '            'oBPPartner.FiscalTaxID.()
                    '            oBPPartner.FiscalTaxID.Add()
                    '        End If
                    '    End If
                    'Next


                    'lrtCode = oBPPartner.Update


                    Return True
                End If

            Catch __unusedException1__ As Exception

                erMessage = __unusedException1__.Message.ToString
                Return False
            End Try
        End Function

        <Route("Api/PostSalesOrder")>
        <HttpPost>
        Public Function PostSalesOrder(ByVal OrderDetails As SIL_MODEL_SORDER_HEADER) As HttpResponseMessage

            Dim G_DI_Company As SAPbobsCOM.Company = Nothing
            Dim strRegnNo As String = ""
            dtTable.Columns.Add("ReturnCode")
            dtTable.Columns.Add("ReturnDocEntry")
            dtTable.Columns.Add("ReturnObjType")
            dtTable.Columns.Add("ReturnSeries")
            dtTable.Columns.Add("ReturnDocNum")
            dtTable.Columns.Add("ReturnMsg")
            Try
                Dim Branch As String = ""
                Dim UserID As String = ""


                If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                    Return Request.CreateResponse(HttpStatusCode.OK, dtError)
                End If
                Dim qstr As String = ""
                'Dim qstr As String = ""
                qstr = "SELECT ""U_SAPUNAME"" ,""U_SAPPWD"" " & vbNewLine &
                       " FROM ""OUSR"" A " & vbNewLine &
                       " WHERE A.""USER_CODE"" ='" + UserID + "'"
                Dim dRow As Data.DataRow
                dRow = DBSQL.getQueryDataRow(qstr, "")
                Dim SAPUserId As String = ""
                Dim SAPPassword As String = ""
                Try
                    SAPUserId = dRow.Item("U_SAPUNAME")
                    SAPPassword = dRow.Item("U_SAPPWD")
                Catch ex As Exception
                End Try
                Connection.ConnectSAP(G_DI_Company, SAPUserId, SAPPassword)

                rSet = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                qstr = "Select Top 1 N.""Series"",B.""BPLId"",C.""WhsCode"" ""WhsCode"" ,TO_CHAR(NOW(),'YYYYYMMDD') ""DocDate"" " & vbNewLine &
                                "  From ""NNM1"" N  " & vbNewLine &
                                "       Inner Join ""OFPR"" O On O.""Indicator""=N.""Indicator"" " & vbNewLine &
                                "       Inner Join ""OBPL"" B On B.""BPLId""=N.""BPLId"" " & vbNewLine &
                                "       Inner Join ""OWHS"" C On C.""U_BUSUNIT""='" + Branch + "' AND C.""U_WHSTYPE""='N' " & vbNewLine &
                                " Where N.""ObjectCode""='17' " & vbNewLine &
                                "   And O.""PeriodStat"" In ('N','C') And N.""Locked""='N' AND N.""Remark""='" + Branch + "'  " & vbNewLine &
                                "   And TO_CHAR('" + OrderDetails.PostingDate + "','YYYYYMMDD') Between TO_CHAR(O.""F_RefDate"",'YYYYYMMDD') And TO_CHAR(O.""T_RefDate"",'YYYYYMMDD')"
                rSet.DoQuery(qstr)

                If rSet.RecordCount > 0 Then
                    Dim DocDate As String = OrderDetails.PostingDate
                    Dim DocDueDate As String = OrderDetails.DocDueDate
                    Dim Whscode As String = rSet.Fields.Item("WhsCode").Value
                    'Dim SlpCode As String = rSet.Fields.Item("SlpCode").Value
                    Dim SalesOrder As SAPbobsCOM.Documents = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oOrders)
                    SalesOrder.DocObjectCode = SAPbobsCOM.BoObjectTypes.oOrders

                    SalesOrder.CardCode = OrderDetails.CardCode
                    'SalesOrder.UserFields.Fields.Item("U_USERID").Value = OrderDetails.UserId
                    SalesOrder.SalesPersonCode = OrderDetails.SalesEmployee.ToString
                    SalesOrder.BPL_IDAssignedToInvoice = rSet.Fields.Item("BPLId").Value
                    SalesOrder.Series = rSet.Fields.Item("Series").Value
                    SalesOrder.DocDate = New Date(Mid(DocDate, 1, 4), Mid(DocDate, 5, 2), Mid(DocDate, 7, 2))
                    SalesOrder.DocDueDate = New Date(Mid(DocDueDate, 1, 4), Mid(DocDueDate, 5, 2), Mid(DocDueDate, 7, 2))
                    SalesOrder.TaxDate = New Date(Mid(OrderDetails.RefDate, 1, 4), Mid(OrderDetails.RefDate, 5, 2), Mid(OrderDetails.RefDate, 7, 2))
                    SalesOrder.NumAtCard = OrderDetails.RefNo
                    'SalesOrder.Expenses.ExpenseCode = ""
                    ''SalesOrder.Expenses.LineGross = ""
                    'SalesOrder.Expenses.LineTotal = ""
                    'SalesOrder.Expenses.TaxCode = ""
                    'SalesOrder.Expenses.Add()
                    'SalesOrder.pay
                    SalesOrder.Comments = OrderDetails.Remarks
                    SalesOrder.UserFields.Fields.Item("U_CRTDBY").Value = UserID
                    SalesOrder.UserFields.Fields.Item("U_BRANCH").Value = Branch
                    For Each Item As SIL_MODEL_SORDER_ITEMS In OrderDetails.Items
                        If Convert.ToString(Item.BaseType) <> "" Then
                            SalesOrder.Lines.BaseType = SAPbobsCOM.BoObjectTypes.oQuotations
                            SalesOrder.Lines.BaseEntry = Item.BaseEntry
                            SalesOrder.Lines.BaseLine = Item.BaseLine
                        End If
                        SalesOrder.Lines.ItemCode = Item.ItemCode
                        SalesOrder.Lines.WarehouseCode = Whscode
                        SalesOrder.Lines.Quantity = Item.Quantity
                        'SalesOrder.Lines.UnitsOfMeasurment = 1.25
                        SalesOrder.Lines.Quantity = Item.Quantity ' Math.Round(Item.Quantity * 1.25, 3)
                        SalesOrder.Lines.UnitPrice = Item.PriceBeforeDiscount
                        SalesOrder.Lines.DiscountPercent = Item.DiscountPercentage
                        SalesOrder.Lines.TaxCode = Item.TaxCode
                        SalesOrder.Lines.MeasureUnit = Item.UOM
                        SalesOrder.Lines.UserFields.Fields.Item("U_DISCPER").Value = Item.Discountamount
                        SalesOrder.Lines.CostingCode = Branch
                        SalesOrder.Lines.ShipDate = New Date(Mid(Item.DocDueDate, 1, 4), Mid(Item.DocDueDate, 5, 2), Mid(Item.DocDueDate, 7, 2))
                        'If Item.Discountamount > 0 Then
                        '    SalesOrder.Lines.Expenses.LineTotal = (Item.Discountamount) * (-1)
                        '    SalesOrder.Lines.Expenses.ExpenseCode = 1
                        '    SalesOrder.Lines.Expenses.Add()
                        'End If
                        SalesOrder.Lines.Add()
                    Next

                    Dim lRetCode As Integer
                    lRetCode = SalesOrder.Add
                    Dim ErrorMessage As String = ""
                    If lRetCode <> 0 Then
                        Dim sErrMsg As String = G_DI_Company.GetLastErrorDescription()
                        G_DI_Company.GetLastError(lRetCode, sErrMsg)
                        ErrorMessage = sErrMsg
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(SalesOrder)
                        G_DI_Company.Disconnect()
                        NewRow = dtTable.NewRow
                        NewRow.Item("ReturnCode") = "-2222"
                        NewRow.Item("ReturnDocEntry") = "-1"
                        NewRow.Item("ReturnObjType") = "-1"
                        NewRow.Item("ReturnSeries") = "-1"
                        NewRow.Item("ReturnDocNum") = "-1"
                        NewRow.Item("ReturnMsg") = ErrorMessage
                        dtTable.Rows.Add(NewRow)
                        Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                    Else
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(SalesOrder)
                        Dim ObjType As String = G_DI_Company.GetNewObjectType.ToString.TrimEnd
                        Dim SOEntry As String = G_DI_Company.GetNewObjectKey.Trim.ToString
                        Dim ErrMsg As String = ""

                        If ObjType = "112" Then
                            qstr = "SELECT B.""SeriesName"",CAST(A.""DocNum"" AS VARCHAR) ""DocNum"",B.""SeriesName""||'/'||CAST(A.""DocNum"" AS VARCHAR) ""StrDocNum"" " & vbNewLine &
                               " From ""ODRF"" A " & vbNewLine &
                               " INNER JOIN ""NNM1"" B ON B.""ObjectCode""=A.""ObjType"" AND B.""Series""=A.""Series"" " & vbNewLine &
                               " WHERE A.""DocEntry""='" + SOEntry + "' "
                        ElseIf ObjType = "17" Then
                            qstr = "SELECT B.""SeriesName"",CAST(A.""DocNum"" AS VARCHAR) ""DocNum"",B.""SeriesName""||'/'||CAST(A.""DocNum"" AS VARCHAR) ""StrDocNum"" " & vbNewLine &
                               " From ""ORDR"" A " & vbNewLine &
                               " INNER JOIN ""NNM1"" B ON B.""ObjectCode""=A.""ObjType"" AND B.""Series""=A.""Series"" " & vbNewLine &
                               " WHERE A.""DocEntry""='" + SOEntry + "' "
                        End If
                        rSet.DoQuery(qstr)
                        Dim ReturnDocNo = rSet.Fields.Item("StrDocNum").Value
                        Dim DPMEntry As String = ""
                        Dim ermsg As String = ""
                        'DPMAdd(G_DI_Company, SOEntry, UserID, "1", DPMEntry, ermsg)

                        G_DI_Company.Disconnect()
                        NewRow = dtTable.NewRow
                        NewRow.Item("ReturnCode") = "0000"
                        NewRow.Item("ReturnDocEntry") = SOEntry
                        NewRow.Item("ReturnObjType") = ObjType
                        NewRow.Item("ReturnSeries") = rSet.Fields.Item("SeriesName").Value
                        NewRow.Item("ReturnDocNum") = rSet.Fields.Item("DocNum").Value
                        NewRow.Item("ReturnMsg") = "Your Request No. " + ReturnDocNo + " successfully submitted"
                        dtTable.Rows.Add(NewRow)
                        Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                    End If
                Else
                    G_DI_Company.Disconnect()
                    NewRow = dtTable.NewRow
                    NewRow.Item("ReturnCode") = "-2222"
                    NewRow.Item("ReturnDocEntry") = "-1"
                    NewRow.Item("ReturnObjType") = "-1"
                    NewRow.Item("ReturnSeries") = "-1"
                    NewRow.Item("ReturnDocNum") = "-1"
                    NewRow.Item("ReturnMsg") = "Series not found"
                    dtTable.Rows.Add(NewRow)
                    Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                End If

            Catch __unusedException1__ As Exception
                Try
                    G_DI_Company.Disconnect()
                Catch ex1 As Exception
                End Try
                NewRow = dtTable.NewRow
                NewRow.Item("ReturnCode") = "-2222"
                NewRow.Item("ReturnDocEntry") = "-1"
                NewRow.Item("ReturnObjType") = "-1"
                NewRow.Item("ReturnSeries") = "-1"
                NewRow.Item("ReturnDocNum") = "-1"
                NewRow.Item("ReturnMsg") = __unusedException1__.Message.ToString
                dtTable.Rows.Add(NewRow)
                Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
            End Try

        End Function

        <Route("Api/PostSalesOrderWithAdvancePayment")>
        <HttpPost>
        Public Function PostSalesOrderWithAdvancePayment(ByVal OrderDetails As SIL_MODEL_SORDER_HEADER) As HttpResponseMessage

            Dim G_DI_Company As SAPbobsCOM.Company = Nothing
            Dim strRegnNo As String = ""
            dtTable.Columns.Add("ReturnCode")
            dtTable.Columns.Add("ReturnDocEntry")
            dtTable.Columns.Add("ReturnObjType")
            dtTable.Columns.Add("ReturnSeries")
            dtTable.Columns.Add("ReturnDocNum")
            dtTable.Columns.Add("ReturnMsg")
            Try
                Dim Branch As String = ""
                Dim UserID As String = ""


                If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                    Return Request.CreateResponse(HttpStatusCode.OK, dtError)
                End If
                Dim qstr As String = ""
                'Dim qstr As String = ""
                qstr = "SELECT ""U_SAPUNAME"" ,""U_SAPPWD"" " & vbNewLine &
                       " FROM ""OUSR"" A " & vbNewLine &
                       " WHERE A.""USER_CODE"" ='" + UserID + "'"
                Dim dRow As Data.DataRow
                dRow = DBSQL.getQueryDataRow(qstr, "")
                Dim SAPUserId As String = ""
                Dim SAPPassword As String = ""
                Try
                    SAPUserId = dRow.Item("U_SAPUNAME")
                    SAPPassword = dRow.Item("U_SAPPWD")
                Catch ex As Exception
                End Try
                Connection.ConnectSAP(G_DI_Company, SAPUserId, SAPPassword)

                rSet = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                qstr = "Select Top 1 N.""Series"",B.""BPLId"",C.""WhsCode"" ""WhsCode"" ,TO_CHAR(NOW(),'YYYYYMMDD') ""DocDate"" " & vbNewLine &
                                "  From ""NNM1"" N  " & vbNewLine &
                                "       Inner Join ""OFPR"" O On O.""Indicator""=N.""Indicator"" " & vbNewLine &
                                "       Inner Join ""OBPL"" B On B.""BPLId""=N.""BPLId"" " & vbNewLine &
                                "       Inner Join ""OWHS"" C On C.""U_BUSUNIT""='" + Branch + "' AND C.""U_WHSTYPE""='N' " & vbNewLine &
                                " Where N.""ObjectCode""='17' " & vbNewLine &
                                "   And O.""PeriodStat"" In ('N','C') And N.""Locked""='N' AND N.""Remark""='" + Branch + "'  " & vbNewLine &
                                "   And TO_CHAR('" + OrderDetails.PostingDate + "','YYYYYMMDD') Between TO_CHAR(O.""F_RefDate"",'YYYYYMMDD') And TO_CHAR(O.""T_RefDate"",'YYYYYMMDD')"
                rSet.DoQuery(qstr)

                If rSet.RecordCount > 0 Then
                    G_DI_Company.StartTransaction()
                    Dim DiscountExists As Boolean = False
                    Dim DocDate As String = OrderDetails.PostingDate
                    Dim DocDuedate As String = OrderDetails.DocDueDate
                    Dim Whscode As String = rSet.Fields.Item("WhsCode").Value
                    'Dim SlpCode As String = rSet.Fields.Item("SlpCode").Value
                    Dim SalesOrder As SAPbobsCOM.Documents = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oOrders)
                    SalesOrder.DocObjectCode = SAPbobsCOM.BoObjectTypes.oOrders

                    SalesOrder.CardCode = OrderDetails.CardCode
                    'SalesOrder.SalesPersonCode = OrderDetails.SalesEmployee.ToString
                    SalesOrder.BPL_IDAssignedToInvoice = rSet.Fields.Item("BPLId").Value
                    SalesOrder.Series = rSet.Fields.Item("Series").Value
                    SalesOrder.DocDate = New Date(Mid(DocDate, 1, 4), Mid(DocDate, 5, 2), Mid(DocDate, 7, 2))
                    SalesOrder.DocDueDate = New Date(Mid(DocDuedate, 1, 4), Mid(DocDuedate, 5, 2), Mid(DocDuedate, 7, 2))
                    SalesOrder.TaxDate = New Date(Mid(OrderDetails.RefDate, 1, 4), Mid(OrderDetails.RefDate, 5, 2), Mid(OrderDetails.RefDate, 7, 2))
                    SalesOrder.NumAtCard = OrderDetails.RefNo
                    If OrderDetails.ItemType <> Nothing Then
                        SalesOrder.UserFields.Fields.Item("U_ITMTYPE").Value = OrderDetails.ItemType
                    End If

                    SalesOrder.Comments = OrderDetails.Remarks
                    SalesOrder.UserFields.Fields.Item("U_CRTDBY").Value = UserID
                    SalesOrder.UserFields.Fields.Item("U_BRANCH").Value = Branch
                    For Each Item As SIL_MODEL_SORDER_ITEMS In OrderDetails.Items

                        If Convert.ToString(Item.BaseType) <> "" Then
                            SalesOrder.Lines.BaseType = SAPbobsCOM.BoObjectTypes.oQuotations
                            SalesOrder.Lines.BaseEntry = Item.BaseEntry
                            SalesOrder.Lines.BaseLine = Item.BaseLine
                        End If
                        SalesOrder.Lines.ItemCode = Item.ItemCode

                        SalesOrder.Lines.WarehouseCode = Whscode
                        SalesOrder.Lines.Quantity = Item.Quantity
                        'SalesOrder.Lines.UnitsOfMeasurment = 1.25
                        'SalesOrder.Lines.Quantity = Item.Quantity ' Math.Round(Item.Quantity * 1.25, 3)
                        SalesOrder.Lines.UnitPrice = Item.PriceBeforeDiscount
                        SalesOrder.Lines.DiscountPercent = Item.DiscountPercentage
                        SalesOrder.Lines.TaxCode = Item.TaxCode
                        SalesOrder.Lines.MeasureUnit = Item.UOM
                        SalesOrder.Lines.UserFields.Fields.Item("U_DISCPER").Value = Item.Discountamount.ToString
                        SalesOrder.Lines.CostingCode = Branch
                        SalesOrder.Lines.ShipDate = New Date(Mid(Item.DocDueDate, 1, 4), Mid(Item.DocDueDate, 5, 2), Mid(Item.DocDueDate, 7, 2))

                        If Item.Discountamount > 0 Then
                            DiscountExists = True
                            'SalesOrder.Lines.Expenses.LineTotal = (Item.Discountamount) * (-1)
                            'SalesOrder.Lines.Expenses.ExpenseCode = 1
                            'SalesOrder.Lines.Expenses.Add()
                        End If
                        SalesOrder.Lines.Add()
                    Next

                    Dim lRetCode As Integer
                    lRetCode = SalesOrder.Add

                    Dim ErrorMessage As String = ""
                    If lRetCode <> 0 Then
                        Dim sErrMsg As String = G_DI_Company.GetLastErrorDescription()
                        G_DI_Company.GetLastError(lRetCode, sErrMsg)
                        ErrorMessage = sErrMsg
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(SalesOrder)
                        If G_DI_Company.InTransaction Then
                            G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                        End If
                        G_DI_Company.Disconnect()
                        NewRow = dtTable.NewRow
                        NewRow.Item("ReturnCode") = "-2222"
                        NewRow.Item("ReturnDocEntry") = "-1"
                        NewRow.Item("ReturnObjType") = "-1"
                        NewRow.Item("ReturnSeries") = "-1"
                        NewRow.Item("ReturnDocNum") = "-1"
                        NewRow.Item("ReturnMsg") = ErrorMessage
                        dtTable.Rows.Add(NewRow)
                        Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                    Else
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(SalesOrder)
                        Dim ObjType As String = G_DI_Company.GetNewObjectType.ToString.TrimEnd
                        Dim SOEntry As String = G_DI_Company.GetNewObjectKey.Trim.ToString
                        Dim ErrMsg As String = ""
                        qstr = "UPDATE ""RDR1"" SET ""OcrCode""='" + Branch + "',""WhsCode""='" + Whscode + "' WHERE ""DocEntry""='" + SOEntry + "' and IFNULL(""OcrCode"",'')='' "
                        rSet.DoQuery(qstr)
                        If ObjType = "112" Then
                            qstr = "SELECT B.""SeriesName"",CAST(A.""DocNum"" AS VARCHAR) ""DocNum"",B.""SeriesName""||'/'||CAST(A.""DocNum"" AS VARCHAR) ""StrDocNum"" " & vbNewLine &
                               " From ""ODRF"" A " & vbNewLine &
                               " INNER JOIN ""NNM1"" B ON B.""ObjectCode""=A.""ObjType"" AND B.""Series""=A.""Series"" " & vbNewLine &
                               " WHERE A.""DocEntry""='" + SOEntry + "' "
                        ElseIf ObjType = "17" Then
                            qstr = "SELECT B.""SeriesName"",CAST(A.""DocNum"" AS VARCHAR) ""DocNum"",B.""SeriesName""||'/'||CAST(A.""DocNum"" AS VARCHAR) ""StrDocNum"" " & vbNewLine &
                               " From ""ORDR"" A " & vbNewLine &
                               " INNER JOIN ""NNM1"" B ON B.""ObjectCode""=A.""ObjType"" AND B.""Series""=A.""Series"" " & vbNewLine &
                               " WHERE A.""DocEntry""='" + SOEntry + "' "
                        End If
                        rSet.DoQuery(qstr)
                        Dim ReturnDocNo = rSet.Fields.Item("StrDocNum").Value
                        Dim SeriesName = rSet.Fields.Item("SeriesName").Value
                        Dim DocNum = rSet.Fields.Item("DocNum").Value
                        Dim DPMEntry As String = ""
                        Dim ermsg As String = ""
                        Try
                            If DPMAdd(G_DI_Company, OrderDetails, SOEntry, UserID, Branch, DiscountExists, DPMEntry, ermsg) = False Then
                                If G_DI_Company.InTransaction Then
                                    G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                                End If

                                G_DI_Company.Disconnect()
                                NewRow = dtTable.NewRow
                                NewRow.Item("ReturnCode") = "-3333"
                                NewRow.Item("ReturnDocEntry") = "-1"
                                NewRow.Item("ReturnObjType") = "-1"
                                NewRow.Item("ReturnSeries") = "-1"
                                NewRow.Item("ReturnDocNum") = "-1"
                                NewRow.Item("ReturnMsg") = ermsg
                                dtTable.Rows.Add(NewRow)
                                Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                            End If
                        Catch ex As Exception
                            If G_DI_Company.InTransaction Then
                                G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                            End If

                            G_DI_Company.Disconnect()
                            NewRow = dtTable.NewRow
                            NewRow.Item("ReturnCode") = "-2222"
                            NewRow.Item("ReturnDocEntry") = "-1"
                            NewRow.Item("ReturnObjType") = "-1"
                            NewRow.Item("ReturnSeries") = "-1"
                            NewRow.Item("ReturnDocNum") = "-1"
                            NewRow.Item("ReturnMsg") = ex.Message
                            dtTable.Rows.Add(NewRow)
                            Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                        End Try

                        G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit)
                        G_DI_Company.Disconnect()
                        NewRow = dtTable.NewRow
                        NewRow.Item("ReturnCode") = "0000"
                        NewRow.Item("ReturnDocEntry") = SOEntry
                        NewRow.Item("ReturnObjType") = ObjType
                        NewRow.Item("ReturnSeries") = SeriesName
                        NewRow.Item("ReturnDocNum") = DocNum
                        NewRow.Item("ReturnMsg") = "Your Request No. " + ReturnDocNo + " successfully submitted"
                        dtTable.Rows.Add(NewRow)
                        Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                    End If
                Else
                    If G_DI_Company.InTransaction Then
                        G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                    End If
                    G_DI_Company.Disconnect()
                    NewRow = dtTable.NewRow
                    NewRow.Item("ReturnCode") = "-2222"
                    NewRow.Item("ReturnDocEntry") = "-1"
                    NewRow.Item("ReturnObjType") = "-1"
                    NewRow.Item("ReturnSeries") = "-1"
                    NewRow.Item("ReturnDocNum") = "-1"
                    NewRow.Item("ReturnMsg") = "Series not found"
                    dtTable.Rows.Add(NewRow)
                    Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                End If

            Catch __unusedException1__ As Exception
                Try
                    If G_DI_Company.InTransaction Then
                        G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                    End If
                Catch ex As Exception
                End Try
                Try
                    G_DI_Company.Disconnect()
                Catch ex1 As Exception
                End Try
                NewRow = dtTable.NewRow
                NewRow.Item("ReturnCode") = "-2222"
                NewRow.Item("ReturnDocEntry") = "-1"
                NewRow.Item("ReturnObjType") = "-1"
                NewRow.Item("ReturnSeries") = "-1"
                NewRow.Item("ReturnDocNum") = "-1"
                NewRow.Item("ReturnMsg") = __unusedException1__.Message.ToString
                dtTable.Rows.Add(NewRow)
                Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
            End Try

        End Function

        <Route("Api/PostECommerceSalesOrderWithAdvancePayment")>
        <HttpPost>
        Public Function PostECommerceSalesOrderWithAdvancePayment(ByVal OrderDetails As SIL_MODEL_SORDER_HEADER) As HttpResponseMessage

            Dim G_DI_Company As SAPbobsCOM.Company = Nothing
            Dim strRegnNo As String = ""
            dtTable.Columns.Add("ReturnCode")
            dtTable.Columns.Add("ReturnDocEntry")
            dtTable.Columns.Add("ReturnObjType")
            dtTable.Columns.Add("ReturnSeries")
            dtTable.Columns.Add("ReturnDocNum")
            dtTable.Columns.Add("ReturnMsg")
            Try
                Dim Branch As String = ""
                Dim UserID As String = ""


                If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                    Return Request.CreateResponse(HttpStatusCode.OK, dtError)
                End If
                Dim qstr As String = ""
                'Dim qstr As String = ""
                qstr = "SELECT ""U_SAPUNAME"" ,""U_SAPPWD"" " & vbNewLine &
                       " FROM ""OUSR"" A " & vbNewLine &
                       " WHERE A.""USER_CODE"" ='" + UserID + "'"
                Dim dRow As Data.DataRow
                dRow = DBSQL.getQueryDataRow(qstr, "")
                Dim SAPUserId As String = ""
                Dim SAPPassword As String = ""
                Try
                    SAPUserId = dRow.Item("U_SAPUNAME")
                    SAPPassword = dRow.Item("U_SAPPWD")
                Catch ex As Exception
                End Try
                Connection.ConnectSAP(G_DI_Company, SAPUserId, SAPPassword)

                rSet = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                qstr = "Select Top 1 N.""Series"",B.""BPLId"",D.""WhsCode"" ""WhsCode"",E.""WhsCode"" ""WIPWhsCode"" ,TO_CHAR(NOW(),'YYYYYMMDD') ""DocDate"" " & vbNewLine &
                                "  From ""NNM1"" N  " & vbNewLine &
                                "       Inner Join ""OFPR"" O On O.""Indicator""=N.""Indicator"" " & vbNewLine &
                                "       Inner Join ""OBPL"" B On B.""BPLId""=N.""BPLId"" " & vbNewLine &
                                "       Inner Join ""OWHS"" C On C.""U_BUSUNIT""='" + Branch + "' AND C.""U_WHSTYPE""='N' " & vbNewLine &
                                "       LEFT OUTER Join ""OWHS"" D On D.""U_BUSUNIT""='" + OrderDetails.ToBranch + "' AND D.""U_WHSTYPE""='N' " & vbNewLine &
                                "       LEFT OUTER Join ""OWHS"" E On E.""U_BUSUNIT""='" + OrderDetails.ToBranch + "' AND E.""U_WHSTYPE""='W' " & vbNewLine &
                                " Where N.""ObjectCode""='17' " & vbNewLine &
                                "   And O.""PeriodStat"" In ('N','C') And N.""Locked""='N' AND N.""Remark""='" + Branch + "'  " & vbNewLine &
                                "   And TO_CHAR('" + OrderDetails.PostingDate + "','YYYYYMMDD') Between TO_CHAR(O.""F_RefDate"",'YYYYYMMDD') And TO_CHAR(O.""T_RefDate"",'YYYYYMMDD')"
                rSet.DoQuery(qstr)

                If rSet.RecordCount > 0 Then
                    ' System.Runtime.InteropServices.Marshal.ReleaseComObject(SalesOrder)


                    G_DI_Company.StartTransaction()

                    Dim DiscountExists As Boolean = False
                    Dim DocDate As String = OrderDetails.PostingDate
                    Dim DocDuedate As String = OrderDetails.DocDueDate
                    Dim Whscode As String = rSet.Fields.Item("WhsCode").Value
                    Dim WIPWhscode As String = rSet.Fields.Item("WIPWhsCode").Value
                    'Dim SlpCode As String = rSet.Fields.Item("SlpCode").Value
                    Dim SalesOrder As SAPbobsCOM.Documents = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oOrders)
                    SalesOrder.DocObjectCode = SAPbobsCOM.BoObjectTypes.oOrders

                    SalesOrder.CardCode = OrderDetails.CardCode
                    'SalesOrder.SalesPersonCode = OrderDetails.SalesEmployee.ToString
                    SalesOrder.BPL_IDAssignedToInvoice = rSet.Fields.Item("BPLId").Value
                    SalesOrder.Series = rSet.Fields.Item("Series").Value
                    SalesOrder.DocDate = New Date(Mid(DocDate, 1, 4), Mid(DocDate, 5, 2), Mid(DocDate, 7, 2))
                    SalesOrder.DocDueDate = New Date(Mid(DocDuedate, 1, 4), Mid(DocDuedate, 5, 2), Mid(DocDuedate, 7, 2))
                    SalesOrder.TaxDate = New Date(Mid(OrderDetails.RefDate, 1, 4), Mid(OrderDetails.RefDate, 5, 2), Mid(OrderDetails.RefDate, 7, 2))
                    SalesOrder.NumAtCard = OrderDetails.RefNo
                    Try
                        SalesOrder.SalesPersonCode = OrderDetails.SalesEmployee
                    Catch ex As Exception
                    End Try
                    Try
                        SalesOrder.Address2 = OrderDetails.DeliveryAddress
                    Catch ex As Exception
                    End Try
                    'Dim PaymentExists As Boolean = False
                    'For Each Item As DTS_MODEL_PMNT_DTLS In OrderDetails.PaymentDetails
                    '    If Item.PaymentType = "P" Then
                    '        SalesOrder.UserFields.Fields.Item("U_PREPCARD").Value = "Y"
                    '        SalesOrder.UserFields.Fields.Item("U_TRANSVAL").Value = Item.Amount.ToString
                    '    Else
                    '        PaymentExists = True
                    '    End If
                    'Next
                    Dim payment_amt As Decimal = 0
                    Try
                        For Each pay As DTS_MODEL_PMNT_DTLS In OrderDetails.PaymentDetails
                            payment_amt = payment_amt + pay.Amount
                        Next
                    Catch ex As Exception

                    End Try
                    If OrderDetails.ItemType <> Nothing Then
                        SalesOrder.UserFields.Fields.Item("U_ITMTYPE").Value = OrderDetails.ItemType
                    End If
                    SalesOrder.Comments = OrderDetails.Remarks
                    SalesOrder.UserFields.Fields.Item("U_TOBUNIT").Value = OrderDetails.ToBranch
                    SalesOrder.UserFields.Fields.Item("U_CRTDBY").Value = UserID
                    SalesOrder.UserFields.Fields.Item("U_BRANCH").Value = Branch
                    SalesOrder.UserFields.Fields.Item("U_BASENTR").Value = IIf(OrderDetails.BaseEntry Is Nothing, "", OrderDetails.BaseEntry)
                    Try
                        SalesOrder.UserFields.Fields.Item("U_SOCHNL").Value = IIf(OrderDetails.saleschannel Is Nothing, "", OrderDetails.saleschannel)
                    Catch ex As Exception

                    End Try
                    Dim SequenceNo As Integer = 0
                    For Each Item As SIL_MODEL_SORDER_ITEMS In OrderDetails.Items
                        qstr = "SELECT IFNULL(A.""LeadTime"",0) ""LeadTime"" " & vbNewLine &
                               " FROM ""OITM"" A " & vbNewLine &
                               "    INNER JOIN ""OITB"" B ON A.""ItmsGrpCod""=B.""ItmsGrpCod"" AND B.""ItemClass""=1 " & vbNewLine &
                               " WHERE A.""ItemCode""='" + Item.ItemCode + "'"
                        Dim ItemrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                        ItemrSet.DoQuery(qstr)
                        Dim Leadtime As Integer = 0
                        If ItemrSet.RecordCount > 0 Then
                            Leadtime = ItemrSet.Fields.Item("LeadTime").Value
                        End If
                        SequenceNo = 1
                        If Leadtime = 0 Then
                            If Convert.ToString(Item.BaseType) <> "" Then
                                SalesOrder.Lines.BaseType = SAPbobsCOM.BoObjectTypes.oQuotations
                                SalesOrder.Lines.BaseEntry = Item.BaseEntry
                                SalesOrder.Lines.BaseLine = Item.BaseLine
                            End If
                            SalesOrder.Lines.ItemCode = Item.ItemCode

                            SalesOrder.Lines.WarehouseCode = Whscode
                            SalesOrder.Lines.Quantity = Item.Quantity
                            'SalesOrder.Lines.UnitsOfMeasurment = 1.25
                            'SalesOrder.Lines.Quantity = Item.Quantity ' Math.Round(Item.Quantity * 1.25, 3)
                            SalesOrder.Lines.UnitPrice = Item.PriceBeforeDiscount
                            SalesOrder.Lines.DiscountPercent = Item.DiscountPercentage
                            SalesOrder.Lines.TaxCode = Item.TaxCode
                            SalesOrder.Lines.MeasureUnit = Item.UOM
                            SalesOrder.Lines.UserFields.Fields.Item("U_DISCPER").Value = Item.Discountamount.ToString
                            SalesOrder.Lines.UserFields.Fields.Item("U_SEQNO").Value = SequenceNo.ToString
                            SalesOrder.Lines.CostingCode = Branch
                            SalesOrder.Lines.ShipDate = New Date(Mid(Item.DocDueDate, 1, 4), Mid(Item.DocDueDate, 5, 2), Mid(Item.DocDueDate, 7, 2))

                            If Item.Discountamount > 0 Then
                                DiscountExists = True
                                'SalesOrder.Lines.Expenses.LineTotal = (Item.Discountamount) * (-1)
                                'SalesOrder.Lines.Expenses.ExpenseCode = 1
                                'SalesOrder.Lines.Expenses.Add()
                            End If
                            SalesOrder.Lines.Add()
                        Else
                            SequenceNo = 1
                            'Dim DiscAmt As Decimal = Item.Discountamount
                            For x As Integer = 1 To CType(Item.Quantity, Integer)
                                If Convert.ToString(Item.BaseType) <> "" Then
                                    SalesOrder.Lines.BaseType = SAPbobsCOM.BoObjectTypes.oQuotations
                                    SalesOrder.Lines.BaseEntry = Item.BaseEntry
                                    SalesOrder.Lines.BaseLine = Item.BaseLine
                                End If
                                SalesOrder.Lines.ItemCode = Item.ItemCode
                                SalesOrder.Lines.UserFields.Fields.Item("U_SEQNO").Value = SequenceNo.ToString
                                SequenceNo = SequenceNo + 1
                                SalesOrder.Lines.WarehouseCode = Whscode
                                SalesOrder.Lines.Quantity = 1 ' Item.Quantity
                                'SalesOrder.Lines.UnitsOfMeasurment = 1.25
                                'SalesOrder.Lines.Quantity = Item.Quantity ' Math.Round(Item.Quantity * 1.25, 3)
                                SalesOrder.Lines.UnitPrice = Item.PriceBeforeDiscount
                                SalesOrder.Lines.DiscountPercent = Item.DiscountPercentage
                                SalesOrder.Lines.TaxCode = Item.TaxCode
                                SalesOrder.Lines.MeasureUnit = Item.UOM
                                SalesOrder.Lines.UserFields.Fields.Item("U_DISCPER").Value = Math.Round((Item.Discountamount / Item.Quantity), 2).ToString
                                SalesOrder.Lines.CostingCode = Branch
                                If x = 1 Then
                                    SalesOrder.Lines.ShipDate = New Date(Mid(Item.DocDueDate, 1, 4), Mid(Item.DocDueDate, 5, 2), Mid(Item.DocDueDate, 7, 2))
                                Else
                                    SalesOrder.Lines.ShipDate = New Date(Mid(Item.DocDueDate, 1, 4), Mid(Item.DocDueDate, 5, 2), Mid(Item.DocDueDate, 7, 2)).AddDays(Leadtime * (x - 1))
                                End If


                                'If Item.Discountamount > 0 Then
                                '    If x = CType(Item.Quantity, Integer) Then
                                '        SalesOrder.Lines.Expenses.LineTotal = DiscAmt * (-1)
                                '    Else
                                '        SalesOrder.Lines.Expenses.LineTotal = ((Item.Discountamount / Item.Quantity)) * (-1)
                                '    End If

                                '    DiscAmt = DiscAmt - ((Item.Discountamount / Item.Quantity))
                                '    SalesOrder.Lines.Expenses.ExpenseCode = 1
                                '    SalesOrder.Lines.Expenses.Add()
                                'End If
                                'x = x + 1
                                SalesOrder.Lines.Add()
                            Next
                        End If


                    Next
                    Try
                        If OrderDetails.freight > 0 Then
                            SalesOrder.Lines.ItemCode = "FREIGHT"
                            SalesOrder.Lines.WarehouseCode = Whscode
                            SalesOrder.Lines.Quantity = 1
                            SalesOrder.Lines.UnitPrice = OrderDetails.freight
                            SalesOrder.Lines.TaxCode = "VAT@0"
                            SalesOrder.Lines.Add()
                        End If
                    Catch ex As Exception
                    End Try

                    Dim lRetCode As Integer
                    lRetCode = SalesOrder.Add

                    Dim ErrorMessage As String = ""
                    If lRetCode <> 0 Then
                        Dim sErrMsg As String = G_DI_Company.GetLastErrorDescription()
                        G_DI_Company.GetLastError(lRetCode, sErrMsg)
                        ErrorMessage = sErrMsg
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(SalesOrder)
                        If G_DI_Company.InTransaction Then
                            G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                        End If
                        G_DI_Company.Disconnect()
                        NewRow = dtTable.NewRow
                        NewRow.Item("ReturnCode") = "-2222"
                        NewRow.Item("ReturnDocEntry") = "-1"
                        NewRow.Item("ReturnObjType") = "-1"
                        NewRow.Item("ReturnSeries") = "-1"
                        NewRow.Item("ReturnDocNum") = "-1"
                        NewRow.Item("ReturnMsg") = ErrorMessage
                        dtTable.Rows.Add(NewRow)
                        Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                    Else
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(SalesOrder)
                        Dim ObjType As String = G_DI_Company.GetNewObjectType.ToString.TrimEnd
                        Dim SOEntry As String = G_DI_Company.GetNewObjectKey.Trim.ToString
                        Dim ErrMsg As String = ""
                        qstr = "UPDATE ""RDR1"" SET ""OcrCode""='" + Branch + "',""CogsOcrCod""='" + Branch + "',""WhsCode""='" + Whscode + "' WHERE ""DocEntry""='" + SOEntry + "' and IFNULL(""OcrCode"",'')='' "
                        rSet.DoQuery(qstr)
                        'qstr = "SELECT ""VisOrder"",""ItemCode"" " & vbNewLine &
                        '       " FROM ""OITM"" A " & vbNewLine &
                        '       "    INNER JOIN ""OITB"" B ON A.""ItmsGrpCod""=B.""ItmsGrpCod"" AND B.""ItemClass""=1 " & vbNewLine &
                        '       "    INNER JOIN ""RDR1"" C ON C.""ItemCode""=A.""ItemCode"" " & vbNewLine &
                        '       " WHERE C.""DocEntry""='" + SOEntry.ToString + "' " & vbNewLine &
                        '       " ORDER BY C.""VisOrder"" "
                        qstr = "Select ""VisOrder"",""LineNum"",""ItemCode"",""U_SEQNO"",TO_CHAR(""ShipDate"",'YYYYMMDD') ""ShipDate"" FROM ""RDR1"" WHERE ""DocEntry""='" + SOEntry + "' ORDER BY ""VisOrder"" "
                        Dim SoLinerSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                        SoLinerSet.DoQuery(qstr)
                        While Not SoLinerSet.EoF
                            qstr = "SELECT ""Code"" FROM ""ITT1"" WHERE ""Father""='" + SoLinerSet.Fields.Item("ItemCode").Value + "'"
                            Dim SoItemrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                            SoItemrSet.DoQuery(qstr)
                            If SoItemrSet.RecordCount > 0 Then
                                While Not SoItemrSet.EoF
                                    qstr = "Select ""VisOrder"",""ItemCode"",TO_CHAR(""ShipDate"",'YYYYMMDD') ""ShipDate"" FROM ""RDR1"" WHERE ""DocEntry""='" + SOEntry + "' AND ""ItemCode""='" + SoItemrSet.Fields.Item("Code").Value + "'  AND ""VisOrder"">" + SoLinerSet.Fields.Item("VisOrder").Value.ToString + " ORDER BY ""VisOrder"" "
                                    Dim SoItemrSet1 As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                                    SoItemrSet1.DoQuery(qstr)
                                    qstr = "UPDATE ""RDR1"" SET ""U_SERVLINE""='" + SoLinerSet.Fields.Item("LineNum").Value.ToString + "',""WhsCode""='" + WIPWhscode + "',""U_ITEMHIDE""='Y',""U_SEQNO""='" + SoLinerSet.Fields.Item("U_SEQNO").Value.ToString + "',""ShipDate""='" + SoLinerSet.Fields.Item("ShipDate").Value + "' WHERE ""DocEntry""='" + SOEntry + "' and ""VisOrder""='" + SoItemrSet1.Fields.Item("VisOrder").Value.ToString + "' "
                                    rSet.DoQuery(qstr)

                                    SoItemrSet.MoveNext()
                                End While
                            Else
                                'Continue While
                            End If
                            SoLinerSet.MoveNext()
                        End While


                        Dim SalesOrder2 As SAPbobsCOM.Documents = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oOrders)
                        SalesOrder2.DocObjectCode = SAPbobsCOM.BoObjectTypes.oOrders
                        If SalesOrder2.GetByKey(SOEntry) Then
                            qstr = "SELECT * FROM ""RDR1"" WHERE ""DocEntry""='" + SOEntry.ToString + "' AND ""LineStatus""='O' AND IFNULL(""U_ITEMHIDE"",'N')='Y' "
                            rSet.DoQuery(qstr)
                            If rSet.RecordCount > 0 Then
                                While Not rSet.EoF
                                    SalesOrder2.Lines.SetCurrentLine(rSet.Fields.Item("VisOrder").Value)
                                    SalesOrder2.Lines.UnitPrice = 0
                                    'SalesOrder2.Lines.LineStatus = SAPbobsCOM.BoStatus.bost_Close

                                    rSet.MoveNext()
                                End While
                                Dim lRetCode1 As Integer = SalesOrder2.Update
                                If lRetCode1 <> 0 Then
                                    Dim sErrMsg As String = G_DI_Company.GetLastErrorDescription()
                                    G_DI_Company.GetLastError(lRetCode, sErrMsg)
                                    ErrorMessage = sErrMsg
                                    System.Runtime.InteropServices.Marshal.ReleaseComObject(SalesOrder2)
                                    If G_DI_Company.InTransaction Then
                                        G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                                    End If
                                    G_DI_Company.Disconnect()
                                    NewRow = dtTable.NewRow
                                    NewRow.Item("ReturnCode") = "-2222"
                                    NewRow.Item("ReturnDocEntry") = "-1"
                                    NewRow.Item("ReturnObjType") = "-1"
                                    NewRow.Item("ReturnSeries") = "-1"
                                    NewRow.Item("ReturnDocNum") = "-1"
                                    NewRow.Item("ReturnMsg") = ErrorMessage
                                    dtTable.Rows.Add(NewRow)
                                    Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                                End If
                            End If
                        Else
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(SalesOrder2)
                        End If


                        If ObjType = "112" Then
                            qstr = "SELECT B.""SeriesName"",CAST(A.""DocNum"" AS VARCHAR) ""DocNum"",B.""SeriesName""||'/'||CAST(A.""DocNum"" AS VARCHAR) ""StrDocNum"" " & vbNewLine &
                               " From ""ODRF"" A " & vbNewLine &
                               " INNER JOIN ""NNM1"" B ON B.""ObjectCode""=A.""ObjType"" AND B.""Series""=A.""Series"" " & vbNewLine &
                               " WHERE A.""DocEntry""='" + SOEntry + "' "
                        ElseIf ObjType = "17" Then
                            qstr = "SELECT B.""SeriesName"",CAST(A.""DocNum"" AS VARCHAR) ""DocNum"",B.""SeriesName""||'/'||CAST(A.""DocNum"" AS VARCHAR) ""StrDocNum"" " & vbNewLine &
                               " From ""ORDR"" A " & vbNewLine &
                               " INNER JOIN ""NNM1"" B ON B.""ObjectCode""=A.""ObjType"" AND B.""Series""=A.""Series"" " & vbNewLine &
                               " WHERE A.""DocEntry""='" + SOEntry + "' "
                        End If
                        rSet.DoQuery(qstr)
                        Dim ReturnDocNo = rSet.Fields.Item("StrDocNum").Value
                        Dim SeriesName = rSet.Fields.Item("SeriesName").Value
                        Dim DocNum = rSet.Fields.Item("DocNum").Value
                        Dim DPMEntry As String = ""
                        Dim ermsg As String = ""
                        Try
                            If payment_amt > 0 Then
                                If DPMAdd(G_DI_Company, OrderDetails, SOEntry, UserID, Branch, DiscountExists, DPMEntry, ermsg) = False Then
                                    If G_DI_Company.InTransaction Then
                                        G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                                    End If

                                    G_DI_Company.Disconnect()
                                    NewRow = dtTable.NewRow
                                    NewRow.Item("ReturnCode") = "-3333"
                                    NewRow.Item("ReturnDocEntry") = "-1"
                                    NewRow.Item("ReturnObjType") = "-1"
                                    NewRow.Item("ReturnSeries") = "-1"
                                    NewRow.Item("ReturnDocNum") = "-1"
                                    NewRow.Item("ReturnMsg") = ermsg
                                    dtTable.Rows.Add(NewRow)
                                    Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                                End If
                            End If
                        Catch ex As Exception
                            If G_DI_Company.InTransaction Then
                                G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                            End If

                            G_DI_Company.Disconnect()
                            NewRow = dtTable.NewRow
                            NewRow.Item("ReturnCode") = "-2222"
                            NewRow.Item("ReturnDocEntry") = "-1"
                            NewRow.Item("ReturnObjType") = "-1"
                            NewRow.Item("ReturnSeries") = "-1"
                            NewRow.Item("ReturnDocNum") = "-1"
                            NewRow.Item("ReturnMsg") = ex.Message
                            dtTable.Rows.Add(NewRow)
                            Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                        End Try

                        G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit)
                        G_DI_Company.Disconnect()
                        NewRow = dtTable.NewRow
                        NewRow.Item("ReturnCode") = "0000"
                        NewRow.Item("ReturnDocEntry") = SOEntry
                        NewRow.Item("ReturnObjType") = ObjType
                        NewRow.Item("ReturnSeries") = SeriesName
                        NewRow.Item("ReturnDocNum") = DocNum
                        NewRow.Item("ReturnMsg") = "Your Request No. " + ReturnDocNo + " successfully submitted"
                        dtTable.Rows.Add(NewRow)
                        Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                    End If
                Else
                    If G_DI_Company.InTransaction Then
                        G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                    End If
                    G_DI_Company.Disconnect()
                    NewRow = dtTable.NewRow
                    NewRow.Item("ReturnCode") = "-2222"
                    NewRow.Item("ReturnDocEntry") = "-1"
                    NewRow.Item("ReturnObjType") = "-1"
                    NewRow.Item("ReturnSeries") = "-1"
                    NewRow.Item("ReturnDocNum") = "-1"
                    NewRow.Item("ReturnMsg") = "Series not found"
                    dtTable.Rows.Add(NewRow)
                    Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                End If

            Catch __unusedException1__ As Exception
                Try
                    If G_DI_Company.InTransaction Then
                        G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                    End If
                Catch ex As Exception
                End Try
                Try
                    G_DI_Company.Disconnect()
                Catch ex1 As Exception
                End Try
                NewRow = dtTable.NewRow
                NewRow.Item("ReturnCode") = "-2222"
                NewRow.Item("ReturnDocEntry") = "-1"
                NewRow.Item("ReturnObjType") = "-1"
                NewRow.Item("ReturnSeries") = "-1"
                NewRow.Item("ReturnDocNum") = "-1"
                NewRow.Item("ReturnMsg") = __unusedException1__.Message.ToString
                dtTable.Rows.Add(NewRow)
                Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
            End Try

        End Function

        Public Function DPMAdd(ByVal G_DI_Company As SAPbobsCOM.Company, ByVal OrderDetails As SIL_MODEL_SORDER_HEADER, ByVal SalesOrderEntry As String, ByVal UserId As String, ByVal Branch As String,
                               ByVal DiscountExists As Boolean, ByRef DpmEntry As String, ByRef erMessage As String) As Boolean
            Try
                rSet = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                Dim qstr As String
                qstr = "Select Top 1 N.""Series"",B.""BPLId"",C.""WhsCode"" ""WhsCode"" ,TO_CHAR(NOW(),'YYYYYMMDD') ""DocDate"" " & vbNewLine &
                                "  From ""NNM1"" N  " & vbNewLine &
                                "       Inner Join ""OFPR"" O On O.""Indicator""=N.""Indicator"" " & vbNewLine &
                                "       Inner Join ""OBPL"" B On B.""BPLId""=N.""BPLId"" " & vbNewLine &
                                "       Inner Join ""OWHS"" C On C.""U_BUSUNIT""='" + Branch + "' AND C.""U_WHSTYPE""='N' " & vbNewLine &
                                " Where N.""ObjectCode""='203' " & vbNewLine &
                                "   And O.""PeriodStat"" In ('N','C') And N.""Locked""='N' AND N.""Remark""='" + Branch + "'  " & vbNewLine &
                                "   And TO_CHAR('" + OrderDetails.PostingDate + "','YYYYYMMDD') Between TO_CHAR(O.""F_RefDate"",'YYYYYMMDD') And TO_CHAR(O.""T_RefDate"",'YYYYYMMDD')"
                rSet.DoQuery(qstr)
                If rSet.RecordCount > 0 Then
                Else
                    erMessage = "Down Payment Series not Found"
                    Return False
                End If
                Dim ARDownDraft As SAPbobsCOM.Documents = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDownPayments)
                ARDownDraft.DocObjectCode = SAPbobsCOM.BoObjectTypes.oDownPayments
                qstr = "SELECT * FROM  ""ORDR"" WHERE ""DocEntry""='" + SalesOrderEntry.ToString + "'"
                Dim SoHrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                SoHrSet.DoQuery(qstr)
                'AND ""LineTotal"">0
                qstr = "SELECT * FROM  ""RDR1"" WHERE ""DocEntry""='" + SalesOrderEntry.ToString + "'  "
                Dim SoDrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                SoDrSet.DoQuery(qstr)
                Dim cardcode = SoHrSet.Fields.Item("CardCode").Value
                ARDownDraft.CardCode = SoHrSet.Fields.Item("CardCode").Value
                ARDownDraft.BPL_IDAssignedToInvoice = rSet.Fields.Item("BPLId").Value
                ARDownDraft.Series = rSet.Fields.Item("Series").Value
                ARDownDraft.Comments = OrderDetails.Remarks
                ARDownDraft.DocDate = New Date(Mid(OrderDetails.PostingDate, 1, 4), Mid(OrderDetails.PostingDate, 5, 2), Mid(OrderDetails.PostingDate, 7, 2))
                ARDownDraft.TaxDate = New Date(Mid(OrderDetails.RefDate, 1, 4), Mid(OrderDetails.RefDate, 5, 2), Mid(OrderDetails.RefDate, 7, 2))
                ARDownDraft.DownPaymentType = SAPbobsCOM.DownPaymentTypeEnum.dptRequest
                'ARDownDraft.BPL_IDAssignedToInvoice = "1"
                ARDownDraft.NumAtCard = OrderDetails.RefNo
                ARDownDraft.UserFields.Fields.Item("U_BRANCH").Value = Branch
                'ARDownDraft.UserFields.Fields.Item("U_ORDERID").Value = DownPaymentDetails.OrderId
                'ARDownDraft.UserFields.Fields.Item("U_STATUS").Value = DownPaymentDetails.Status
                'ARDownDraft.UserFields.Fields.Item("U_RESPCODE").Value = DownPaymentDetails.ResponseCode
                'ARDownDraft.UserFields.Fields.Item("U_BANKTXID").Value = DownPaymentDetails.BankTransID
                'ARDownDraft.UserFields.Fields.Item("U_USERCODE").Value = UserId
                Dim TotalValue As Decimal = 0

                While Not SoDrSet.EoF
                    'erMessage = TotalValue.ToString
                    'Return False
                    ARDownDraft.Lines.BaseType = "17"
                    ARDownDraft.Lines.BaseEntry = SoDrSet.Fields.Item("DocEntry").Value
                    ARDownDraft.Lines.BaseLine = SoDrSet.Fields.Item("LineNum").Value
                    'ARDownDraft.Lines.WarehouseCode = SoDrSet.Fields.Item("WhsCode").Value
                    qstr = "SELECT * FROM  ""RDR1"" WHERE ""DocEntry""='" + SalesOrderEntry.ToString + "' " & vbNewLine &
                           "    And ""LineNum""='" + SoDrSet.Fields.Item("LineNum").Value.ToString + "' "

                    Dim SoFRGDrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                    SoFRGDrSet.DoQuery(qstr)
                    If SoFRGDrSet.RecordCount > 0 Then
                        TotalValue = TotalValue + CType(SoDrSet.Fields.Item("LineTotal").Value, Decimal) + CType(SoDrSet.Fields.Item("VatSum").Value, Decimal) + (SoFRGDrSet.Fields.Item("LineTotal").Value)
                        ARDownDraft.Lines.LineTotal = CType(SoDrSet.Fields.Item("LineTotal").Value, Decimal) + CType(SoDrSet.Fields.Item("VatSum").Value, Decimal) + (SoFRGDrSet.Fields.Item("LineTotal").Value)
                        ARDownDraft.Lines.TaxCode = ""
                    Else
                        TotalValue = TotalValue + CType(SoDrSet.Fields.Item("LineTotal").Value, Decimal) + CType(SoDrSet.Fields.Item("VatSum").Value, Decimal)
                        ARDownDraft.Lines.LineTotal = CType(SoDrSet.Fields.Item("LineTotal").Value, Decimal) + CType(SoDrSet.Fields.Item("VatSum").Value, Decimal)
                        ARDownDraft.Lines.TaxCode = ""
                    End If


                    ARDownDraft.Lines.CostingCode = Branch
                    ARDownDraft.Lines.Add()
                    SoDrSet.MoveNext()
                End While

                Dim DocTotal As Decimal = 0
                For Each Item As DTS_MODEL_PMNT_DTLS In OrderDetails.PaymentDetails
                    DocTotal = DocTotal + Item.Amount
                Next
                If DocTotal <> 0 Then
                    ARDownDraft.DocTotal = DocTotal
                End If

                Dim lRetCode As Integer
                lRetCode = ARDownDraft.Add
                Dim ErrorMessage As String = ""
                If lRetCode <> 0 Then
                    Dim sErrMsg As String = G_DI_Company.GetLastErrorDescription()
                    G_DI_Company.GetLastError(lRetCode, sErrMsg)
                    erMessage = sErrMsg
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(ARDownDraft)
                    Return False
                Else
                    Dim VoucherPaymentNo As String = ""
                    Dim ObjType As String = G_DI_Company.GetNewObjectType.ToString.TrimEnd
                    DpmEntry = G_DI_Company.GetNewObjectKey.Trim.ToString

                    Dim InPay As SAPbobsCOM.Payments = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oIncomingPayments)
                    'erMessage = "t1"
                    'Return False
                    qstr = "Select Top 1 N.""Series"",B.""BPLId"",C.""WhsCode"" ""WhsCode"" ,TO_CHAR(NOW(),'YYYYYMMDD') ""DocDate"" " & vbNewLine &
                            "  From ""NNM1"" N  " & vbNewLine &
                            "       Inner Join ""OFPR"" O On O.""Indicator""=N.""Indicator"" " & vbNewLine &
                            "       Inner Join ""OBPL"" B On B.""BPLId""=N.""BPLId"" " & vbNewLine &
                            "       Inner Join ""OWHS"" C On C.""U_BUSUNIT""='" + Branch + "' AND C.""U_WHSTYPE""='N' " & vbNewLine &
                            " Where N.""ObjectCode""='24' " & vbNewLine &
                            "   And O.""PeriodStat"" In ('N','C') And N.""Locked""='N' AND N.""Remark""='" + Branch + "'  " & vbNewLine &
                            "   And TO_CHAR('" + OrderDetails.PostingDate + "','YYYYYMMDD') Between TO_CHAR(O.""F_RefDate"",'YYYYYMMDD') And TO_CHAR(O.""T_RefDate"",'YYYYYMMDD')"

                    Dim IncPayment As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                    IncPayment.DoQuery(qstr)
                    If IncPayment.RecordCount > 0 Then
                    Else
                        erMessage = "Incoimng Payment Series not Found"
                        Return False
                    End If
                    InPay.CardCode = SoHrSet.Fields.Item("CardCode").Value
                    'InPay.DocDate = New Date(Mid(getServerDate, 1, 4), Mid(getServerDate, 5, 2), Mid(getServerDate, 7, 2))
                    InPay.BPLID = SoHrSet.Fields.Item("BPLId").Value
                    InPay.DocDate = New Date(Mid(OrderDetails.PostingDate, 1, 4), Mid(OrderDetails.PostingDate, 5, 2), Mid(OrderDetails.PostingDate, 7, 2))
                    InPay.Series = IncPayment.Fields.Item("Series").Value
                    InPay.UserFields.Fields.Item("U_BRANCH").Value = Branch
                    InPay.UserFields.Fields.Item("U_CRTDBY").Value = UserId
                    InPay.Invoices.DocEntry = Convert.ToInt32(DpmEntry)
                    InPay.Invoices.InvoiceType = SAPbobsCOM.BoRcptInvTypes.it_DownPayment
                    InPay.Invoices.DistributionRule = Branch
                    InPay.Invoices.Add()
                    Dim ARDown As SAPbobsCOM.Documents = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDownPayments)
                    ARDown = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDownPayments)
                    ARDown.DocObjectCode = SAPbobsCOM.BoObjectTypes.oDownPayments
                    ARDown.GetByKey(Convert.ToInt32(DpmEntry))
                    For Each Item As DTS_MODEL_PMNT_DTLS In OrderDetails.PaymentDetails
                        If Item.PaymentType = "S" Then
                            InPay.CashSum = Item.Amount
                            qstr = "SELECT ""OverCode"",""FrgnName"",""AcctCode"" FROM OACT  WHERE ""OverCode""='" + Branch + "' AND ""FrgnName""='S'"
                            Dim CashPayment As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                            CashPayment.DoQuery(qstr)
                            InPay.CashAccount = CashPayment.Fields.Item("AcctCode").Value
                            'erMessage = Item.Amount.ToString
                            'Return False
                        End If
                        If Item.PaymentType = "S" Then
                        Else
                            'qstr = "SELECT ""OverCode"",""FrgnName"",""AcctCode"" FROM OACT  WHERE ""OverCode""='" + Branch + "' AND ""FrgnName""='C'"
                            qstr = "SELECT ""CreditCard""  FROM ""OCRC"" where ""U_BANKCODE""='" + Item.Bank + "' AND ""U_PMNTP""='" + Item.PaymentType + "'"
                            Dim CreditPayment As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                            CreditPayment.DoQuery(qstr)
                            If CreditPayment.RecordCount > 0 Then
                                InPay.CreditCards.CreditCard = CreditPayment.Fields.Item("CreditCard").Value
                                'InPay.CreditCards.CreditCardNumber = IIf(Item.CardNo.ToString = "", "1111", Right(Item.CardNo, 4))
                                If Item.PaymentType = "2" Or Item.PaymentType = "8" Then
                                    If DiscountExists = True Then
                                        erMessage = "Discount can not be done for Voucher type Payment please Remove Item Discount"
                                        Return False
                                    End If
                                    qstr = "SELECT ""PrcCode"",""PrcName"" FROM ""OPRC"" WHERE ""PrcCode""='" + Item.CardNo + "' and ""DimCode""='5' and ""U_CARDCODE""='" + cardcode + "'"
                                    Dim VouchNameStr As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                                    VouchNameStr.DoQuery(qstr)
                                    If VouchNameStr.RecordCount > 0 Then
                                        InPay.CreditCards.CreditCardNumber = VouchNameStr.Fields.Item("PrcCode").Value
                                        InPay.CreditCards.UserFields.Fields.Item("U_CARDNO").Value = VouchNameStr.Fields.Item("PrcCode").Value
                                        VoucherPaymentNo = IIf(VoucherPaymentNo = "", Item.CardNo + ":" + Item.Amount.ToString, VoucherPaymentNo + ";" + Item.CardNo + ":" + Item.Amount.ToString)
                                    Else
                                        erMessage = "No Voucher found for " + Item.CardNo
                                        Return False
                                    End If

                                Else
                                    Try
                                        InPay.CreditCards.CreditCardNumber = IIf(Item.CardNo Is Nothing, "1111", Right(Item.CardNo.ToString, 4))
                                        InPay.CreditCards.UserFields.Fields.Item("U_CARDNO").Value = IIf(Item.CardNo Is Nothing, "1111", Item.CardNo.ToString)
                                    Catch ex As Exception
                                        InPay.CreditCards.CreditCardNumber = "1111"
                                        InPay.CreditCards.UserFields.Fields.Item("U_CARDNO").Value = "1111"
                                    End Try

                                End If
                                InPay.CreditCards.CardValidUntil = New Date(Mid("29991231", 1, 4), Mid("29991231", 5, 2), Mid("29991231", 7, 2))
                                InPay.CreditCards.PaymentMethodCode = 1
                                InPay.CreditCards.CreditSum = Item.Amount
                                'InPay.CreditCards.FirstPaymentSum = CreditCards.CardAmount
                                Try
                                    InPay.CreditCards.VoucherNum = IIf(Item.Tranid.ToString = "", "111", Item.Tranid.ToString)
                                Catch ex As Exception
                                    InPay.CreditCards.VoucherNum = "111"
                                End Try

                                InPay.CreditCards.CreditType = SAPbobsCOM.BoRcptCredTypes.cr_Regular
                                InPay.CreditCards.Add()
                            Else
                                erMessage = "No Payment Method found for " + Item.Bank + " and " + Item.PaymentType
                                Return False
                            End If

                            'InPay.TransferAccount = CreditPayment.Fields.Item("AcctCode").Value
                            'InPay.TransferReference = Item.Tranid.ToString
                            'InPay.UserFields.Fields.Item("U_CRDTRNID").Value = Item.Tranid.ToString
                            'InPay.UserFields.Fields.Item("U_CRDONAME").Value = Item.CardHolderName.ToString
                            'InPay.UserFields.Fields.Item("U_CRDCNO").Value = Item.CardNo.ToString
                            'InPay.TransferDate = New Date(Mid(Date.Now.ToString("yyyyMMdd"), 1, 4), Mid(Date.Now.ToString("yyyyMMdd"), 5, 2), Mid(Date.Now.ToString("yyyyMMdd"), 7, 2))
                            'InPay.TransferSum = Item.Amount
                        End If
                        'If Item.PaymentType = "C" Then
                        '    qstr = "SELECT ""OverCode"",""FrgnName"",""AcctCode"" FROM OACT  WHERE ""OverCode""='" + Branch + "' AND ""FrgnName""='C'"
                        '    Dim CreditPayment As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                        '    CreditPayment.DoQuery(qstr)
                        '    InPay.TransferAccount = CreditPayment.Fields.Item("AcctCode").Value
                        '    InPay.TransferReference = Item.Tranid.ToString
                        '    InPay.UserFields.Fields.Item("U_CRDTRNID").Value = Item.Tranid.ToString
                        '    InPay.UserFields.Fields.Item("U_CRDONAME").Value = Item.CardHolderName.ToString
                        '    InPay.UserFields.Fields.Item("U_CRDCNO").Value = Item.CardNo.ToString
                        '    InPay.TransferDate = New Date(Mid(Date.Now.ToString("yyyyMMdd"), 1, 4), Mid(Date.Now.ToString("yyyyMMdd"), 5, 2), Mid(Date.Now.ToString("yyyyMMdd"), 7, 2))
                        '    InPay.TransferSum = Item.Amount
                        'End If
                        'If Item.PaymentType = "U" Then
                        '    qstr = "SELECT ""OverCode"",""FrgnName"",""AcctCode"" FROM OACT  WHERE ""OverCode""='" + Branch + "' AND ""FrgnName""='U'"
                        '    Dim UPIPayment As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                        '    UPIPayment.DoQuery(qstr)
                        '    InPay.CheckAccount = UPIPayment.Fields.Item("AcctCode").Value
                        '    InPay.Checks.CountryCode = "BD"
                        '    InPay.Checks.CheckSum = Item.Amount
                        '    InPay.Checks.UserFields.Fields.Item("U_TRNSID").Value = Item.Tranid.ToString
                        '    InPay.Checks.DueDate = New Date(Mid(Date.Now.ToString("yyyyMMdd"), 1, 4), Mid(Date.Now.ToString("yyyyMMdd"), 5, 2), Mid(Date.Now.ToString("yyyyMMdd"), 7, 2))
                        '    qstr = "SELECT A.""BankCode"" " & vbNewLine &
                        '           " FROM ""ODSC"" A " & vbNewLine &
                        '           "    INNER JOIN ""CUFD"" B ON B.""TableID""='ODSC' AND B.""AliasID""='TYPE' " & vbNewLine &
                        '           "    INNER JOIN ""UFD1"" C ON C.""TableID""=B.""TableID"" AND B.""FieldID""=C.""FieldID"" AND A.""U_TYPE""=C.""FldValue"" " & vbNewLine &
                        '       " WHERE A.""U_TYPE""='" + Item.UpiName + "' "
                        '    Dim UPIBankPayment As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                        '    UPIBankPayment.DoQuery(qstr)
                        '    InPay.Checks.BankCode = UPIBankPayment.Fields.Item("BankCode").Value
                        '    InPay.Checks.CheckNumber = 1
                        '    'InPay.Checks.
                        '    InPay.Checks.Add()
                        'End If
                    Next
                    lRetCode = InPay.Add
                    If lRetCode <> 0 Then
                        Dim sErrMsg As String = G_DI_Company.GetLastErrorDescription()
                        G_DI_Company.GetLastError(lRetCode, sErrMsg)
                        erMessage = sErrMsg
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(InPay)
                        Return False
                    End If
                    Dim IncDocEntry = G_DI_Company.GetNewObjectKey.Trim.ToString
                    If VoucherPaymentNo <> "" Then
                        For Each Dtls As String In VoucherPaymentNo.Split(New String() {";"}, StringSplitOptions.None)
                            Dim PrcCode = Dtls.Split(":")(0)
                            Dim Value As Decimal = Dtls.Split(":")(1)

                            qstr = "UPDATE A SET ""OcrCode5""='" + PrcCode.ToString + "' " & vbNewLine &
                           " FROM ""JDT1"" A " & vbNewLine &
                           "    INNER JOIN ""OJDT"" B ON A.""TransId""=B.""TransId"" " & vbNewLine &
                           "    INNER JOIN ""ORCT"" C ON C.""TransId""=B.""TransId"" AND C.""Canceled""='N' --AND C.""CardCode"" =A.""ShortName"" " & vbNewLine &
                           "    INNER JOIN ""OACT"" E ON E.""AcctCode""=A.""Account"" AND E.""FrgnName""='VS' " & vbNewLine &
                           "    INNER JOIN ""OPRC"" F ON F.""PrcCode""='" + PrcCode.ToString + "' " & vbNewLine &
                           " WHERE A.""Debit""<>0 " & vbNewLine &
                           " and F.""PrcCode""='" + PrcCode.ToString + "' " & vbNewLine &
                           " AND A.""Debit""='" + Value.ToString + "' " & vbNewLine &
                           " AND C.""DocEntry""='" + IncDocEntry.ToString + "' " & vbNewLine &
                           "  AND IFNULL(A.""OcrCode5"",'')='' "
                            Dim IncomingVchUpdt As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                            IncomingVchUpdt.DoQuery(qstr)
                        Next

                    End If


                    Return True

                End If
            Catch ex As Exception
                erMessage = ex.Message
                Return False
            End Try
        End Function

        <Route("Api/PostInvoice")>
        <HttpPost>
        Public Function PostInvoice(ByVal InvoiceDetails As SIL_MODEL_INVC_HEADER) As HttpResponseMessage

            Dim G_DI_Company As SAPbobsCOM.Company = Nothing
            Dim strRegnNo As String = ""
            dtTable.Columns.Add("ReturnCode")
            dtTable.Columns.Add("ReturnDocEntry")
            dtTable.Columns.Add("ReturnObjType")
            dtTable.Columns.Add("ReturnSeries")
            dtTable.Columns.Add("ReturnDocNum")
            dtTable.Columns.Add("ReturnMsg")
            Try
                Dim Branch As String = ""
                Dim UserID As String = ""


                If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                    Return Request.CreateResponse(HttpStatusCode.OK, dtError)
                End If
                Dim qstr As String = ""
                'Dim qstr As String = ""
                qstr = "SELECT ""U_SAPUNAME"" ,""U_SAPPWD"" " & vbNewLine &
                       " FROM ""OUSR"" A " & vbNewLine &
                       " WHERE A.""USER_CODE"" ='" + UserID + "'"
                Dim dRow As Data.DataRow
                dRow = DBSQL.getQueryDataRow(qstr, "")
                Dim SAPUserId As String = ""
                Dim SAPPassword As String = ""
                Try
                    SAPUserId = dRow.Item("U_SAPUNAME")
                    SAPPassword = dRow.Item("U_SAPPWD")
                Catch ex As Exception
                End Try
                Connection.ConnectSAP(G_DI_Company, SAPUserId, SAPPassword)

                rSet = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                qstr = "Select Top 1 N.""Series"",B.""BPLId"",C.""WhsCode"" ""WhsCode"" ,TO_CHAR(NOW(),'YYYYYMMDD') ""DocDate"" " & vbNewLine &
                                "  From ""NNM1"" N  " & vbNewLine &
                                "       Inner Join ""OFPR"" O On O.""Indicator""=N.""Indicator"" " & vbNewLine &
                                "       Inner Join ""OBPL"" B On B.""BPLId""=N.""BPLId"" " & vbNewLine &
                                "       Inner Join ""OWHS"" C On C.""U_BUSUNIT""='" + Branch + "' AND C.""U_WHSTYPE""='N' " & vbNewLine &
                                " Where N.""ObjectCode""='13' " & vbNewLine &
                                "   And O.""PeriodStat"" In ('N','C') And N.""Locked""='N' AND N.""Remark""='" + Branch + "'  " & vbNewLine &
                                "   And TO_CHAR('" + InvoiceDetails.PostingDate + "','YYYYYMMDD') Between TO_CHAR(O.""F_RefDate"",'YYYYYMMDD') And TO_CHAR(O.""T_RefDate"",'YYYYYMMDD')"
                rSet.DoQuery(qstr)

                If rSet.RecordCount > 0 Then
                    Dim DocDate As String = InvoiceDetails.PostingDate
                    Dim DocDueDate As String = InvoiceDetails.DocDueDate
                    Dim Invoice As SAPbobsCOM.Documents = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices)
                    Invoice.DocObjectCode = SAPbobsCOM.BoObjectTypes.oInvoices

                    Invoice.CardCode = InvoiceDetails.CardCode
                    Invoice.GSTTransactionType = SAPbobsCOM.GSTTransactionTypeEnum.gsttrantyp_BillOfSupply
                    'Delivery.BPL_IDAssignedToInvoice = InvoiceDetails.Branch
                    Invoice.BPL_IDAssignedToInvoice = rSet.Fields.Item("BPLId").Value
                    Invoice.Series = rSet.Fields.Item("Series").Value
                    Invoice.DocDate = New Date(Mid(DocDate, 1, 4), Mid(DocDate, 5, 2), Mid(DocDate, 7, 2))
                    Invoice.DocDueDate = New Date(Mid(DocDueDate, 1, 4), Mid(DocDueDate, 5, 2), Mid(DocDueDate, 7, 2))
                    Invoice.TaxDate = New Date(Mid(InvoiceDetails.RefDate, 1, 4), Mid(InvoiceDetails.RefDate, 5, 2), Mid(InvoiceDetails.RefDate, 7, 2))
                    Invoice.NumAtCard = InvoiceDetails.RefNo
                    'Invoice.SalesPersonCode = InvoiceDetails.SalesEmployee.ToString
                    Invoice.Comments = InvoiceDetails.Remarks.ToString
                    Invoice.UserFields.Fields.Item("U_CRTDBY").Value = UserID
                    Invoice.UserFields.Fields.Item("U_BRANCH").Value = Branch
                    Dim TotalValue As Double = 0
                    Dim docentry As Integer = -1
                    For Each Item As SIL_MODEL_INVC_ITEMS In InvoiceDetails.Items
                        If Item.BaseType = "17" Or Item.BaseType = "15" Then
                            Invoice.Lines.BaseEntry = Item.BaseEntry
                            Invoice.Lines.BaseLine = Item.BaseLine
                            docentry = Item.BaseEntry
                            If Item.BaseType = "17" Then
                                Invoice.Lines.BaseType = SAPbobsCOM.BoObjectTypes.oOrders
                            ElseIf Item.BaseType = "15" Then
                                Invoice.Lines.BaseType = SAPbobsCOM.BoObjectTypes.oDeliveryNotes
                            End If
                        Else
                            Invoice.Lines.ItemCode = Item.ItemCode
                            Invoice.Lines.Quantity = Item.Quantity
                            Invoice.Lines.UnitPrice = Item.PriceBeforeDiscount
                            Invoice.Lines.DiscountPercent = Item.DiscountPercentage
                            Invoice.Lines.TaxCode = Item.TaxCode
                            Invoice.Lines.WarehouseCode = rSet.Fields.Item("WhsCode").Value
                            Invoice.Lines.UserFields.Fields.Item("U_DISCPER").Value = Item.Discountamount
                        End If

                        'If Item.Discountamount > 0 Then
                        '    Invoice.Lines.Expenses.LineTotal = (Item.Discountamount) * (-1)
                        '    Invoice.Lines.Expenses.ExpenseCode = 1
                        '    Invoice.Lines.Expenses.Add()
                        'End If

                        qstr = "SELECT ""LineTotal""+""VatSum"" ""TOTAL"",""Quantity"" FROM ""RDR1"" WHERE ""DocEntry""='" + Item.BaseEntry.ToString + "' AND ""LineNum""='" + Item.BaseLine.ToString + "'"
                        Dim SOrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                        SOrSet.DoQuery(qstr)
                        Dim LineValue As Decimal = SOrSet.Fields.Item("TOTAL").Value / CType(SOrSet.Fields.Item("Quantity").Value, Decimal)
                        TotalValue = TotalValue + (LineValue * Item.Quantity) '+ ((Item.Discountamount) * (-1))
                        'Invoice.Lines.TaxCode = Item.TaxCode
                        Invoice.Lines.Quantity = Item.Quantity
                        Invoice.Lines.CostingCode = Branch

                        Dim oItems As SAPbobsCOM.Items = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oItems)
                        oItems.GetByKey(Item.ItemCode)


                        If oItems.ManageBatchNumbers = SAPbobsCOM.BoYesNoEnum.tYES Then
                            Dim i As Integer = 0
                            For Each Batches As DTS_MODEL_INVC_BATCH In Item.Batches
                                If Batches.VisOrder = Item.VisOrder Then
                                    'oGIIssue.Lines.BatchNumbers.SetCurrentLine(i)
                                    Invoice.Lines.BatchNumbers.BatchNumber = Batches.BatchNo
                                    Invoice.Lines.BatchNumbers.Quantity = Batches.BatchQuantity
                                    'If i <> 0 Then
                                    '    Delivery.Lines.BatchNumbers.Add()
                                    'End If
                                    Invoice.Lines.BatchNumbers.Add()

                                    i = i + 1
                                End If
                            Next
                        End If
                        If oItems.ManageSerialNumbers = SAPbobsCOM.BoYesNoEnum.tYES Then
                            Dim i As Integer = 0

                            For Each Serial As DTS_MODEL_INVC_SERIAL In Item.Serial
                                If Serial.VisOrder = Item.VisOrder Then
                                    Invoice.Lines.SerialNumbers.SetCurrentLine(i)
                                    Invoice.Lines.SerialNumbers.InternalSerialNumber = Serial.InternalSerialNumber
                                    Invoice.Lines.SerialNumbers.SystemSerialNumber = Serial.SystemSerialNumber
                                    Invoice.Lines.SerialNumbers.Add()
                                    i = i + 1
                                End If
                            Next
                        End If

                        Invoice.Lines.Add()
                    Next
                    qstr = "SELECT DISTINCT A.""DocEntry"",B.""DocTotal""-IFNULL(V1.""DrawnSum"",0) ""Balance"" " & vbNewLine &
                           " FROM ""DPI1"" A " & vbNewLine &
                           "    INNER JOIN ""ODPI"" B ON A.""DocEntry"" =B.""DocEntry""  " & vbNewLine &
                           " LEFT OUTER JOIN " + vbNewLine &
                                       "            ( " + vbNewLine &
                                       "                SELECT A.""BaseAbs"",SUM(A.""DrawnSum"") ""DrawnSum"" " + vbNewLine &
                                       "                FROM ""INV9"" A " + vbNewLine &
                                       "                    INNER JOIN ""OINV"" B ON A.""DocEntry"" =B.""DocEntry"" " + vbNewLine &
                                       "            WHERE B.""CardCode"" ='" + InvoiceDetails.CardCode + "' " + vbNewLine &
                                       "            AND B.""CANCELED"" ='N' " + vbNewLine &
                                       "            GROUP BY A.""BaseAbs"" " + vbNewLine &
                                       "            )V1 ON V1.""BaseAbs""=B.""DocEntry"" " + vbNewLine &
                           " WHERE A.""BaseEntry"" ='" + docentry.ToString + "' "
                    Dim DPMrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                    DPMrSet.DoQuery(qstr)
                    If DPMrSet.RecordCount > 0 Then
                        Dim i As Integer = 0
                        Dim BalnaceAmt As Double = 0
                        While Not DPMrSet.EoF
                            If TotalValue > 0 Then
                                Dim dpToDraw As SAPbobsCOM.DownPaymentsToDraw = Invoice.DownPaymentsToDraw
                                dpToDraw.SetCurrentLine(i)
                                dpToDraw.DocEntry = DPMrSet.Fields.Item("DocEntry").Value
                                If CType(DPMrSet.Fields.Item("Balance").Value, Double) >= TotalValue Then
                                    dpToDraw.AmountToDraw = TotalValue
                                    TotalValue = TotalValue - TotalValue
                                Else
                                    TotalValue = TotalValue - CType(DPMrSet.Fields.Item("Balance").Value, Double)
                                    dpToDraw.AmountToDraw = CType(DPMrSet.Fields.Item("Balance").Value, Double)
                                End If

                                dpToDraw.Add()
                                i = i + 1
                            Else
                                Exit While
                            End If
                            DPMrSet.MoveNext()
                        End While
                    End If
                    Dim lRetCode As Integer
                    lRetCode = Invoice.Add
                    Dim ErrorMessage As String = ""
                    If lRetCode <> 0 Then
                        Dim sErrMsg As String = G_DI_Company.GetLastErrorDescription()
                        G_DI_Company.GetLastError(lRetCode, sErrMsg)
                        ErrorMessage = sErrMsg
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(Invoice)
                        G_DI_Company.Disconnect()
                        NewRow = dtTable.NewRow
                        NewRow.Item("ReturnCode") = "-2222"
                        NewRow.Item("ReturnDocEntry") = "-1"
                        NewRow.Item("ReturnObjType") = "-1"
                        NewRow.Item("ReturnSeries") = "-1"
                        NewRow.Item("ReturnDocNum") = "-1"
                        NewRow.Item("ReturnMsg") = ErrorMessage
                        dtTable.Rows.Add(NewRow)
                        Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                    Else
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(Invoice)
                        Dim ObjType As String = G_DI_Company.GetNewObjectType.ToString.TrimEnd
                        Dim DLEntry As String = G_DI_Company.GetNewObjectKey.Trim.ToString
                        Dim ErrMsg As String = ""

                        If ObjType = "112" Then
                            qstr = "SELECT B.""SeriesName"",CAST(A.""DocNum"" AS VARCHAR) ""DocNum"",B.""SeriesName""||'/'||CAST(A.""DocNum"" AS VARCHAR) ""StrDocNum"" " & vbNewLine &
                               " From ""ODRF"" A " & vbNewLine &
                               " INNER JOIN ""NNM1"" B ON B.""ObjectCode""=A.""ObjType"" AND B.""Series""=A.""Series"" " & vbNewLine &
                               " WHERE A.""DocEntry""='" + DLEntry + "' "
                        ElseIf ObjType = "13" Then
                            qstr = "SELECT B.""SeriesName"",CAST(A.""DocNum"" AS VARCHAR) ""DocNum"",B.""SeriesName""||'/'||CAST(A.""DocNum"" AS VARCHAR) ""StrDocNum"" " & vbNewLine &
                               " From ""OINV"" A " & vbNewLine &
                               " INNER JOIN ""NNM1"" B ON B.""ObjectCode""=A.""ObjType"" AND B.""Series""=A.""Series"" " & vbNewLine &
                               " WHERE A.""DocEntry""='" + DLEntry + "' "
                        End If
                        rSet.DoQuery(qstr)
                        Dim ReturnDocNo = rSet.Fields.Item("StrDocNum").Value
                        G_DI_Company.Disconnect()
                        NewRow = dtTable.NewRow
                        NewRow.Item("ReturnCode") = "0000"
                        NewRow.Item("ReturnDocEntry") = DLEntry
                        NewRow.Item("ReturnObjType") = ObjType
                        NewRow.Item("ReturnSeries") = rSet.Fields.Item("SeriesName").Value
                        NewRow.Item("ReturnDocNum") = rSet.Fields.Item("DocNum").Value
                        NewRow.Item("ReturnMsg") = "Your Request No. " + ReturnDocNo + " successfully submitted"
                        dtTable.Rows.Add(NewRow)
                        Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                    End If
                Else
                    G_DI_Company.Disconnect()
                    NewRow = dtTable.NewRow
                    NewRow.Item("ReturnCode") = "-2222"
                    NewRow.Item("ReturnDocEntry") = "-1"
                    NewRow.Item("ReturnObjType") = "-1"
                    NewRow.Item("ReturnSeries") = "-1"
                    NewRow.Item("ReturnDocNum") = "-1"
                    NewRow.Item("ReturnMsg") = "Series not found"
                    dtTable.Rows.Add(NewRow)
                    Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                End If

            Catch __unusedException1__ As Exception
                Try
                    G_DI_Company.Disconnect()
                Catch ex1 As Exception
                End Try
                NewRow = dtTable.NewRow
                NewRow.Item("ReturnCode") = "-2222"
                NewRow.Item("ReturnMsg") = __unusedException1__.Message.ToString
                NewRow.Item("ReturnDocEntry") = "-1"
                NewRow.Item("ReturnObjType") = "-1"
                NewRow.Item("ReturnSeries") = "-1"
                NewRow.Item("ReturnDocNum") = "-1"
                dtTable.Rows.Add(NewRow)
                Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
            End Try

        End Function

        <Route("Api/PostInvoiceWithPayment")>
        <HttpPost>
        Public Function PostInvoiceWithPayment(ByVal InvoiceDetails As SIL_MODEL_INVC_HEADER) As HttpResponseMessage

            Dim G_DI_Company As SAPbobsCOM.Company = Nothing
            Dim strRegnNo As String = ""
            dtTable.Columns.Add("ReturnCode")
            dtTable.Columns.Add("ReturnDocEntry")
            dtTable.Columns.Add("ReturnObjType")
            dtTable.Columns.Add("ReturnSeries")
            dtTable.Columns.Add("ReturnDocNum")
            dtTable.Columns.Add("ReturnMsg")
            Try
                Dim Branch As String = ""
                Dim UserID As String = ""


                If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                    Return Request.CreateResponse(HttpStatusCode.OK, dtError)
                End If
                Dim qstr As String = ""
                'Dim qstr As String = ""
                qstr = "SELECT ""U_SAPUNAME"" ,""U_SAPPWD"" " & vbNewLine &
                       " FROM ""OUSR"" A " & vbNewLine &
                       " WHERE A.""USER_CODE"" ='" + UserID + "'"
                Dim dRow As Data.DataRow
                dRow = DBSQL.getQueryDataRow(qstr, "")
                Dim SAPUserId As String = ""
                Dim SAPPassword As String = ""
                Try
                    SAPUserId = dRow.Item("U_SAPUNAME")
                    SAPPassword = dRow.Item("U_SAPPWD")
                Catch ex As Exception
                End Try
                Connection.ConnectSAP(G_DI_Company, SAPUserId, SAPPassword)

                rSet = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                qstr = "Select Top 1 N.""Series"",B.""BPLId"",C.""WhsCode"" ""WhsCode"" ,TO_CHAR(NOW(),'YYYYYMMDD') ""DocDate"" " & vbNewLine &
                                "  From ""NNM1"" N  " & vbNewLine &
                                "       Inner Join ""OFPR"" O On O.""Indicator""=N.""Indicator"" " & vbNewLine &
                                "       Inner Join ""OBPL"" B On B.""BPLId""=N.""BPLId"" " & vbNewLine &
                                "       Inner Join ""OWHS"" C On C.""U_BUSUNIT""='" + Branch + "' AND C.""U_WHSTYPE""='N' " & vbNewLine &
                                " Where N.""ObjectCode""='13' " & vbNewLine &
                                "   And O.""PeriodStat"" In ('N','C') And N.""Locked""='N' AND N.""Remark""='" + Branch + "'  " & vbNewLine &
                                "   And TO_CHAR('" + InvoiceDetails.PostingDate + "','YYYYYMMDD') Between TO_CHAR(O.""F_RefDate"",'YYYYYMMDD') And TO_CHAR(O.""T_RefDate"",'YYYYYMMDD')"
                rSet.DoQuery(qstr)

                If rSet.RecordCount > 0 Then
                    G_DI_Company.StartTransaction()
                    Dim DiscountExists As Boolean = False
                    Dim DocDate As String = InvoiceDetails.PostingDate
                    Dim Invoice As SAPbobsCOM.Documents = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices)
                    Invoice.DocObjectCode = SAPbobsCOM.BoObjectTypes.oInvoices

                    Invoice.CardCode = InvoiceDetails.CardCode
                    'Delivery.BPL_IDAssignedToInvoice = InvoiceDetails.Branch
                    Invoice.BPL_IDAssignedToInvoice = rSet.Fields.Item("BPLId").Value
                    Invoice.Series = rSet.Fields.Item("Series").Value
                    Invoice.DocDate = New Date(Mid(DocDate, 1, 4), Mid(DocDate, 5, 2), Mid(DocDate, 7, 2))
                    Invoice.DocDueDate = New Date(Mid(DocDate, 1, 4), Mid(DocDate, 5, 2), Mid(DocDate, 7, 2))
                    Invoice.TaxDate = New Date(Mid(InvoiceDetails.RefDate, 1, 4), Mid(InvoiceDetails.RefDate, 5, 2), Mid(InvoiceDetails.RefDate, 7, 2))
                    Invoice.NumAtCard = InvoiceDetails.RefNo
                    Try
                        Invoice.SalesPersonCode = InvoiceDetails.SalesEmployee.ToString
                    Catch ex As Exception
                    End Try

                    Invoice.UserFields.Fields.Item("U_BASENTR").Value = IIf(InvoiceDetails.BaseEntry Is Nothing, "", InvoiceDetails.BaseEntry)
                    Invoice.Comments = InvoiceDetails.Remarks
                    Invoice.UserFields.Fields.Item("U_TOBUNIT").Value = InvoiceDetails.ToBranch
                    Invoice.UserFields.Fields.Item("U_CRTDBY").Value = UserID
                    Invoice.UserFields.Fields.Item("U_BRANCH").Value = Branch
                    If InvoiceDetails.ItemType <> Nothing Then
                        Invoice.UserFields.Fields.Item("U_ITMTYPE").Value = InvoiceDetails.ItemType
                    End If

                    Dim TotalValue As Decimal = 0
                    Dim TaxValue As Decimal = 0
                    Dim SoEntry As String = "-1"
                    Dim PrepaidCrad As String = ""
                    Dim TransactionVal As Decimal = 0

                    For Each Item As SIL_MODEL_INVC_ITEMS In InvoiceDetails.Items
                        Dim BaseType As String = IIf(Item.BaseType Is Nothing, "", Item.BaseType)
                        If BaseType = "17" Or BaseType = "15" Then
                            'qstr = "SELECT A.""CardCode"",A.""DocEntry"",IFNULL(A.""U_TRANSVAL"",0)-IFNULL(V1.""DrawnSum"",0) ""Balance"" " & vbNewLine &
                            '           "        FROM ""ORDR"" A " & vbNewLine &
                            '            "            INNER JOIN ""RDR1"" B ON A.""DocEntry""=B.""DocEntry"" " & vbNewLine &
                            '            "             LEFT OUTER JOIN ""INV1"" C ON C.""BaseEntry""=B.""DocEntry"" AND C.""BaseLine""=B.""LineNum"" AND C.""BaseType""=B.""ObjType"" " & vbNewLine &
                            '            "            LEFT OUTER JOIN  " & vbNewLine &
                            '           "            (  " & vbNewLine &
                            '           "            	SELECT V2.""DocEntry"",SUM(V2.""DrawnSum"") ""DrawnSum"" " & vbNewLine &
                            '           "            	FROM " & vbNewLine &
                            '           "             	( " & vbNewLine &
                            '           "                    SELECT A.""DocEntry"",A.""BaseAbs"",A.""DrawnSum"" " & vbNewLine &
                            '           "                     FROM ""INV9"" A " & vbNewLine &
                            '           "                         INNER JOIN ""OINV""B ON A.""DocEntry""=B.""DocEntry"" " & vbNewLine &
                            '           "                        INNER JOIN ""DPI1"" C ON C.""DocEntry""=A.""BaseAbs"" AND IFNULL(C.""BaseType"",-1)=-1 " & vbNewLine &
                            '           "                     	WHERE B.""CANCELED""='N' " & vbNewLine &
                            '           "                     	GROUP BY A.""DocEntry"",A.""BaseAbs"",A.""DrawnSum"" " & vbNewLine &
                            '           "           	)V2 " & vbNewLine &
                            '           "     	group by V2.""DocEntry"" " & vbNewLine &
                            '           "            )V1 ON V1.""DocEntry""=C.""DocEntry"" " & vbNewLine &
                            '           " WHERE IFNULL(A.""U_PREPCARD"",'N')='Y' and A.""DocEntry""='" + Item.BaseEntry.ToString + "' " & vbNewLine &
                            '           "  GROUP BY A.""CardCode"",A.""DocEntry"",IFNULL(A.""U_TRANSVAL"",0)-IFNULL(V1.""DrawnSum"",0) "


                            'rSet.DoQuery(qstr)
                            'TransactionVal = rSet.Fields.Item("Balance").Value
                            'qstr = "SELECT IFNULL(""U_PREPCARD"",'N') ""U_PREPCARD"",IFNULL(""U_TRANSVAL"",0) ""U_TRANSVAL""  FROM ""ORDR"" WHERE ""DocEntry""='" + Item.BaseEntry.ToString + "'"
                            'rSet.DoQuery(qstr)
                            'PrepaidCrad = rSet.Fields.Item("U_PREPCARD").Value



                            Invoice.Lines.BaseEntry = Item.BaseEntry
                            Invoice.Lines.BaseLine = Item.BaseLine


                            If Item.BaseType = "17" Then
                                Invoice.Lines.BaseType = SAPbobsCOM.BoObjectTypes.oOrders
                                SoEntry = Item.BaseEntry
                            ElseIf Item.BaseType = "15" Then
                                Invoice.Lines.BaseType = SAPbobsCOM.BoObjectTypes.oDeliveryNotes
                            End If
                            qstr = "SELECT ""LineTotal"" ""TOTAL"",""VatSum"" ,""Quantity"" FROM ""RDR1"" WHERE ""DocEntry""='" + Item.BaseEntry.ToString + "' AND ""LineNum""='" + Item.BaseLine.ToString + "'"
                            Dim SOrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                            SOrSet.DoQuery(qstr)
                            Dim LineValue As Decimal = SOrSet.Fields.Item("TOTAL").Value '/ CType(SOrSet.Fields.Item("Quantity").Value, Decimal)
                            Dim LineTaxValue As Decimal = SOrSet.Fields.Item("VatSum").Value
                            TaxValue = TaxValue + LineTaxValue
                            TotalValue = TotalValue + LineValue '* Item.Quantity) '- Item.Discountamount)



                            If Item.Discountamount > 0 Then
                                DiscountExists = True
                                'Invoice.Lines.Expenses.LineTotal = (Item.Discountamount) * (-1)
                                'Invoice.Lines.Expenses.ExpenseCode = 1
                                'Invoice.Lines.Expenses.Add()
                            End If
                            Invoice.Lines.WarehouseCode = Item.WhsCode

                            'Invoice.Lines.TaxCode = Item.TaxCode
                            Invoice.Lines.Quantity = Item.Quantity
                            Invoice.Lines.CostingCode = Branch

                            Dim oItems As SAPbobsCOM.Items = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oItems)
                            oItems.GetByKey(Item.ItemCode)
                            If oItems.ManageBatchNumbers = SAPbobsCOM.BoYesNoEnum.tYES Then
                                Dim i As Integer = 0
                                For Each Batches As DTS_MODEL_INVC_BATCH In Item.Batches
                                    If Batches.VisOrder = Item.VisOrder Then
                                        'oGIIssue.Lines.BatchNumbers.SetCurrentLine(i)
                                        Invoice.Lines.BatchNumbers.BatchNumber = Batches.BatchNo
                                        Invoice.Lines.BatchNumbers.Quantity = Batches.BatchQuantity
                                        'If i <> 0 Then
                                        '    Delivery.Lines.BatchNumbers.Add()
                                        'End If
                                        Invoice.Lines.BatchNumbers.Add()

                                        i = i + 1
                                    End If
                                Next
                            End If

                            If oItems.ManageSerialNumbers = SAPbobsCOM.BoYesNoEnum.tYES Then
                                Dim i As Integer = 0
                                For Each Serial As DTS_MODEL_INVC_SERIAL In Item.Serial
                                    If Serial.VisOrder = Item.VisOrder Then
                                        Invoice.Lines.SerialNumbers.SetCurrentLine(i)
                                        Invoice.Lines.SerialNumbers.InternalSerialNumber = Serial.InternalSerialNumber
                                        Invoice.Lines.SerialNumbers.SystemSerialNumber = Serial.SystemSerialNumber
                                        Invoice.Lines.SerialNumbers.Add()
                                        i = i + 1
                                    End If
                                Next
                            End If

                            Invoice.Lines.Add()



                            qstr = "SELECT * FROM  ""RDR1"" WHERE ""DocEntry""='" + Item.BaseEntry.ToString + "' AND ""LineNum""='" + Item.BaseLine.ToString + "'"
                            Dim SoDrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                            SoDrSet.DoQuery(qstr)
                            Dim ServiceLine As Integer = IIf(SoDrSet.Fields.Item("LineNum").Value.ToString = "", -1, SoDrSet.Fields.Item("LineNum").Value.ToString)
                            Dim ServiceItem As String = SoDrSet.Fields.Item("ItemCode").Value
                            qstr = "SELECT * FROM  ""RDR1"" WHERE ""DocEntry""='" + Item.BaseEntry.ToString + "' AND ""U_SERVLINE""='" + ServiceLine.ToString + "' AND ""ItemCode""<>'" + ServiceItem + "' "
                            SoDrSet.DoQuery(qstr)
                            While Not SoDrSet.EoF
                                Invoice.Lines.BaseType = SAPbobsCOM.BoObjectTypes.oOrders
                                Invoice.Lines.BaseEntry = SoDrSet.Fields.Item("DocEntry").Value
                                Invoice.Lines.BaseLine = SoDrSet.Fields.Item("LineNum").Value
                                Invoice.Lines.UserFields.Fields.Item("U_ITEMHIDE").Value = "Y"

                                'qstr = "SELECT ""LineTotal"" FROM ""RDR2"" WHERE ""DocEntry""='" + SoDrSet.Fields.Item("DocEntry").Value.ToString + "' AND ""LineNum""='" + SoDrSet.Fields.Item("LineNum").Value.ToString + "'"
                                'Dim SOrSet2 As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                                'SOrSet2.DoQuery(qstr)
                                'Dim LineDiscountAmt As Decimal = 0
                                'If SOrSet2.RecordCount > 0 Then
                                '    LineDiscountAmt = SOrSet2.Fields.Item("LineTotal").Value * (-1)
                                'End If

                                'qstr = "SELECT ""LineTotal""+""VatSum"" ""TOTAL"",""Quantity"" FROM ""RDR1"" WHERE ""DocEntry""='" + SoDrSet.Fields.Item("DocEntry").Value.ToString + "' AND ""LineNum""='" + SoDrSet.Fields.Item("LineNum").Value.ToString + "'"
                                'Dim SOrSet1 As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                                'SOrSet1.DoQuery(qstr)
                                'Dim LineValue1 As Decimal = SOrSet1.Fields.Item("TOTAL").Value / CType(SOrSet1.Fields.Item("Quantity").Value, Decimal)
                                'TotalValue = TotalValue + ((LineValue1 * CType(SOrSet1.Fields.Item("Quantity").Value, Decimal)) - LineDiscountAmt)


                                'If LineDiscountAmt > 0 Then
                                '    Invoice.Lines.Expenses.LineTotal = (LineDiscountAmt) * (-1)
                                '    Invoice.Lines.Expenses.ExpenseCode = 1
                                '    Invoice.Lines.Expenses.Add()
                                'End If
                                Dim oItems1 As SAPbobsCOM.Items = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oItems)
                                oItems1.GetByKey(SoDrSet.Fields.Item("ItemCode").Value)

                                If oItems1.ManageBatchNumbers = SAPbobsCOM.BoYesNoEnum.tYES Then
                                    qstr = "SELECT ""BatchNum"" ,""Quantity"" FROM ""OIBT"" WHERE ""ItemCode"" ='" + SoDrSet.Fields.Item("ItemCode").Value + "' AND ""WhsCode"" ='" + SoDrSet.Fields.Item("WhsCode").Value + "' AND ""Quantity"" <>0 ORDER BY ""InDate"" "
                                    Dim btrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)

                                    btrSet.DoQuery(qstr)
                                    If btrSet.RecordCount > 0 Then
                                        Dim LineQty As Decimal = SoDrSet.Fields.Item("Quantity").Value

                                        While Not btrSet.EoF
                                            Invoice.Lines.BatchNumbers.BatchNumber = btrSet.Fields.Item("BatchNum").Value
                                            If LineQty <= btrSet.Fields.Item("Quantity").Value Then
                                                Invoice.Lines.BatchNumbers.Quantity = LineQty
                                                Invoice.Lines.BatchNumbers.Add()
                                                Exit While
                                            Else
                                                Invoice.Lines.BatchNumbers.Quantity = btrSet.Fields.Item("Quantity").Value
                                                LineQty = LineQty - btrSet.Fields.Item("Quantity").Value
                                                Invoice.Lines.BatchNumbers.Add()
                                            End If
                                            btrSet.MoveNext()
                                        End While

                                    End If
                                End If

                                Invoice.Lines.Add()
                                SoDrSet.MoveNext()
                            End While
                        Else
                            Dim PaymentExists As Boolean = False
                            'For Each Pmnt As DTS_MODEL_PMNT_DTLS In InvoiceDetails.PaymentDetails
                            '    If Pmnt.PaymentType = "P" Then
                            '        Invoice.UserFields.Fields.Item("U_PREPCARD").Value = "Y"
                            '        Invoice.UserFields.Fields.Item("U_TRANSVAL").Value = Pmnt.Amount.ToString
                            '        TransactionVal = Pmnt.Amount.ToString
                            '        PrepaidCrad = "Y"
                            '    Else
                            '        PaymentExists = True
                            '    End If
                            'Next


                            Invoice.Lines.ItemCode = Item.ItemCode
                            qstr = "SELECT A.""ItmsGrpCod"",A.""U_VCHTYPE"" " & vbNewLine &
                                   " FROM ""OITM"" A " & vbNewLine &
                                   "    INNER JOIN ""OITB"" B ON A.""ItmsGrpCod""=B.""ItmsGrpCod"" AND IFNULL(B.""U_VOUCHER"",'N')='Y' " & vbNewLine &
                                   " WHERE A.""ItemCode""='" + Item.ItemCode + "'"
                            Dim ItemgrprSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                            ItemgrprSet.DoQuery(qstr)
                            Dim CostCenterCode As String = ""
                            Dim ErrorMsg As String = ""
                            If ItemgrprSet.RecordCount > 0 Then

                                If VoucherCostCenterAdd(G_DI_Company, InvoiceDetails, Item.VoucherNo, Item.ValidTill, ItemgrprSet.Fields.Item("U_VCHTYPE").Value, CostCenterCode, ErrorMsg) = False Then
                                    If G_DI_Company.InTransaction Then
                                        G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                                    End If
                                    G_DI_Company.Disconnect()
                                    NewRow = dtTable.NewRow
                                    NewRow.Item("ReturnCode") = "-2222"
                                    NewRow.Item("ReturnDocEntry") = "-1"
                                    NewRow.Item("ReturnObjType") = "-1"
                                    NewRow.Item("ReturnSeries") = "-1"
                                    NewRow.Item("ReturnDocNum") = "-1"
                                    NewRow.Item("ReturnMsg") = ErrorMsg
                                    dtTable.Rows.Add(NewRow)
                                    Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                                Else
                                    Invoice.Lines.UserFields.Fields.Item("U_VCHNO").Value = Item.VoucherNo.ToString
                                    Invoice.Lines.UserFields.Fields.Item("U_VCHVUTL").Value = New Date(Mid(Item.ValidTill.ToString, 1, 4), Mid(Item.ValidTill.ToString, 5, 2), Mid(Item.ValidTill.ToString, 7, 2))
                                    Invoice.Lines.CostingCode5 = CostCenterCode.ToString
                                End If

                                Invoice.Lines.UnitPrice = Item.PriceBeforeDiscount
                                'Invoice.Lines.DiscountPercent = Item.DiscountPercentage
                                Invoice.Lines.UserFields.Fields.Item("U_DISCPER").Value = Item.DiscountPercentage.ToString
                                Invoice.Lines.TaxCode = IIf(Item.TaxCode Is Nothing, "", Item.TaxCode)
                                Invoice.Lines.MeasureUnit = IIf(Item.UOM Is Nothing, "", Item.UOM)
                                Invoice.Lines.ShipDate = New Date(Mid(Item.DocDueDate, 1, 4), Mid(Item.DocDueDate, 5, 2), Mid(Item.DocDueDate, 7, 2))
                                qstr = "SELECT ""Rate"" FROM ""OSTC"" WHERE ""Code""='" + Item.TaxCode + "'"
                                Dim TAXrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                                TAXrSet.DoQuery(qstr)
                                Dim TaxAmount As Decimal = Math.Round((Item.PriceBeforeDiscount * Item.Quantity) * (TAXrSet.Fields.Item("Rate").Value / 100), 2)
                                TotalValue = TotalValue + Math.Round((Item.PriceBeforeDiscount * Item.Quantity) + TaxAmount - Item.Discountamount, 2)

                                If Item.Discountamount > 0 Then
                                    DiscountExists = True
                                    Invoice.Lines.Expenses.LineTotal = (Item.Discountamount) * (-1)
                                    Invoice.Lines.Expenses.ExpenseCode = 1
                                    Invoice.Lines.Expenses.TaxCode = Item.TaxCode
                                    Invoice.Lines.Expenses.Add()
                                End If
                            Else
                                Invoice.Lines.UnitPrice = Item.PriceBeforeDiscount
                                Invoice.Lines.DiscountPercent = Item.DiscountPercentage
                                Invoice.Lines.UserFields.Fields.Item("U_DISCPER").Value = Item.Discountamount.ToString
                                Invoice.Lines.TaxCode = IIf(Item.TaxCode Is Nothing, "", Item.TaxCode)
                                Invoice.Lines.MeasureUnit = IIf(Item.UOM Is Nothing, "", Item.UOM)
                                Invoice.Lines.ShipDate = New Date(Mid(Item.DocDueDate, 1, 4), Mid(Item.DocDueDate, 5, 2), Mid(Item.DocDueDate, 7, 2))
                                qstr = "SELECT ""Rate"" FROM ""OSTC"" WHERE ""Code""='" + Item.TaxCode + "'"
                                Dim TAXrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                                TAXrSet.DoQuery(qstr)
                                Dim TaxAmount As Decimal = Math.Round(((Item.PriceBeforeDiscount * Item.Quantity) - Item.Discountamount) * (TAXrSet.Fields.Item("Rate").Value / 100), 2)
                                TotalValue = TotalValue + Math.Round(((Item.PriceBeforeDiscount * Item.Quantity) - Item.Discountamount) + TaxAmount, 2)

                                If Item.Discountamount > 0 Then
                                    DiscountExists = True
                                    'Invoice.Lines.Expenses.LineTotal = (Item.Discountamount) * (-1)
                                    'Invoice.Lines.Expenses.ExpenseCode = 1
                                    'Invoice.Lines.Expenses.Add()
                                End If
                            End If
                            'SalesOrder.Lines.WarehouseCode = Whscode
                            'SalesOrder.Lines.UnitsOfMeasurment = 1.25
                            'SalesOrder.Lines.Quantity = Item.Quantity ' Math.Round(Item.Quantity * 1.25, 3)
                            'Invoice.Lines.UnitPrice = Item.PriceBeforeDiscount
                            ''Invoice.Lines.DiscountPercent = Item.DiscountPercentage
                            'Invoice.Lines.TaxCode = IIf(Item.TaxCode Is Nothing, "", Item.TaxCode)
                            'Invoice.Lines.MeasureUnit = IIf(Item.UOM Is Nothing, "", Item.UOM)
                            'Invoice.Lines.ShipDate = New Date(Mid(Item.DocDueDate, 1, 4), Mid(Item.DocDueDate, 5, 2), Mid(Item.DocDueDate, 7, 2))
                            'qstr = "SELECT ""Rate"" FROM ""OSTC"" WHERE ""Code""='" + Item.TaxCode + "'"
                            'Dim TAXrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                            'TAXrSet.DoQuery(qstr)
                            'Dim TaxAmount As Decimal = Math.Round((Item.PriceBeforeDiscount * Item.Quantity) * (TAXrSet.Fields.Item("Rate").Value / 100), 2)
                            'TotalValue = TotalValue + Math.Round((Item.PriceBeforeDiscount * Item.Quantity) + TaxAmount - Item.Discountamount, 2)

                            'If Item.Discountamount > 0 Then
                            '    DiscountExists = True
                            '    Invoice.Lines.Expenses.LineTotal = (Item.Discountamount) * (-1)
                            '    Invoice.Lines.Expenses.ExpenseCode = 1
                            '    Invoice.Lines.Expenses.Add()
                            'End If
                            Invoice.Lines.WarehouseCode = Item.WhsCode

                            'Invoice.Lines.TaxCode = Item.TaxCode
                            Invoice.Lines.Quantity = Item.Quantity
                            Invoice.Lines.CostingCode = Branch
                            Dim oItems As SAPbobsCOM.Items = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oItems)
                            oItems.GetByKey(Item.ItemCode)
                            If oItems.ManageBatchNumbers = SAPbobsCOM.BoYesNoEnum.tYES Then
                                Dim i As Integer = 0
                                For Each Batches As DTS_MODEL_INVC_BATCH In Item.Batches
                                    If Batches.VisOrder = Item.VisOrder Then
                                        'oGIIssue.Lines.BatchNumbers.SetCurrentLine(i)
                                        Invoice.Lines.BatchNumbers.BatchNumber = Batches.BatchNo
                                        Invoice.Lines.BatchNumbers.Quantity = Batches.BatchQuantity
                                        'If i <> 0 Then
                                        '    Delivery.Lines.BatchNumbers.Add()
                                        'End If
                                        Invoice.Lines.BatchNumbers.Add()

                                        i = i + 1
                                    End If
                                Next
                            End If

                            If oItems.ManageSerialNumbers = SAPbobsCOM.BoYesNoEnum.tYES Then
                                Dim i As Integer = 0
                                For Each Serial As DTS_MODEL_INVC_SERIAL In Item.Serial
                                    If Serial.VisOrder = Item.VisOrder Then
                                        Invoice.Lines.SerialNumbers.SetCurrentLine(i)
                                        Invoice.Lines.SerialNumbers.InternalSerialNumber = Serial.InternalSerialNumber
                                        Invoice.Lines.SerialNumbers.SystemSerialNumber = Serial.SystemSerialNumber
                                        Invoice.Lines.SerialNumbers.Add()
                                        i = i + 1
                                    End If
                                Next
                            End If

                            Invoice.Lines.Add()
                        End If


                    Next



                    Dim DPRow As Integer = 0
                    Dim dpToDraw As SAPbobsCOM.DownPaymentsToDraw = Invoice.DownPaymentsToDraw
                    Dim DownPaymentVal As Decimal = 0
                    Dim TVal As String = ""
                    qstr = "SELECT DISTINCT A.""DocEntry"",(B.""DocTotal""-B.""VatSum"")-IFNULL(V1.""DrawnSum"",0) ""Balance"" " & vbNewLine &
                           " FROM ""DPI1"" A " & vbNewLine &
                           "    INNER JOIN ""ODPI"" B ON A.""DocEntry"" =B.""DocEntry""  " & vbNewLine &
                           " LEFT OUTER JOIN " + vbNewLine &
                                       "            ( " + vbNewLine &
                                       "                SELECT A.""BaseAbs"",SUM(A.""DrawnSum"") ""DrawnSum"" " + vbNewLine &
                                       "                FROM ""INV9"" A " + vbNewLine &
                                       "                    INNER JOIN ""OINV"" B ON A.""DocEntry"" =B.""DocEntry"" " + vbNewLine &
                                       "            WHERE B.""CardCode"" ='" + InvoiceDetails.CardCode.ToString.TrimEnd + "'" + vbNewLine &
                                       "            AND B.""CANCELED"" ='N' " + vbNewLine &
                                       "            GROUP BY A.""BaseAbs"" " + vbNewLine &
                                       "            )V1 ON V1.""BaseAbs""=B.""DocEntry"" " + vbNewLine &
                           " WHERE A.""BaseEntry"" ='" + SoEntry + "'" + vbNewLine &
                           "      AND  B.""DocTotal""-IFNULL(V1.""DrawnSum"",0) >0 "

                    Dim DPMrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                    DPMrSet.DoQuery(qstr)
                    If DPMrSet.RecordCount > 0 Then
                        'Dim i As Integer = 0
                        DownPaymentVal = TransactionVal
                        'Dim BalnaceAmt As Double = 0
                        TotalValue = TotalValue + TaxValue

                        While Not DPMrSet.EoF
                            If TotalValue > 0 Then
                                'Dim dpToDraw As SAPbobsCOM.DownPaymentsToDraw = Invoice.DownPaymentsToDraw
                                dpToDraw.SetCurrentLine(DPRow)
                                dpToDraw.DocEntry = DPMrSet.Fields.Item("DocEntry").Value
                                If CType(DPMrSet.Fields.Item("Balance").Value, Double) >= TotalValue Then


                                    dpToDraw.AmountToDraw = Math.Round((TotalValue), 2, MidpointRounding.AwayFromZero)
                                    'dpToDraw.GrossAmountToDraw = Math.Round((TotalValue), 2)
                                    TotalValue = Math.Round((TotalValue - TotalValue), 2, MidpointRounding.AwayFromZero)
                                    'Exit While
                                Else
                                    TotalValue = TotalValue - Math.Round(CType(DPMrSet.Fields.Item("Balance").Value, Double), 2, MidpointRounding.AwayFromZero) 'TotalValue - (CType(DPMrSet.Fields.Item("Balance").Value, Double))

                                    dpToDraw.AmountToDraw = CType(DPMrSet.Fields.Item("Balance").Value, Double)

                                End If

                                dpToDraw.Add()
                                DPRow = DPRow + 1
                            Else
                                Exit While
                            End If
                            DPMrSet.MoveNext()
                        End While
                    End If


                    Dim InvcAmt As Double = 0
                    Dim BalnaceAmt As Double = 0


                    Dim lRetCode As Integer

                    lRetCode = Invoice.Add


                    Dim ErrorMessage As String = ""
                    If lRetCode <> 0 Then
                        Dim sErrMsg As String = G_DI_Company.GetLastErrorDescription()
                        G_DI_Company.GetLastError(lRetCode, sErrMsg)
                        ErrorMessage = sErrMsg
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(Invoice)
                        If G_DI_Company.InTransaction Then
                            G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                        End If
                        G_DI_Company.Disconnect()
                        NewRow = dtTable.NewRow
                        NewRow.Item("ReturnCode") = "-2222"
                        NewRow.Item("ReturnDocEntry") = "-1"
                        NewRow.Item("ReturnObjType") = "-1"
                        NewRow.Item("ReturnSeries") = "-1"
                        NewRow.Item("ReturnDocNum") = "-1"
                        NewRow.Item("ReturnMsg") = ErrorMessage
                        dtTable.Rows.Add(NewRow)
                        Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                    Else
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(Invoice)
                        Dim ObjType As String = G_DI_Company.GetNewObjectType.ToString.TrimEnd
                        Dim DLEntry As String = G_DI_Company.GetNewObjectKey.Trim.ToString
                        Dim ErrMsg As String = ""


                        If ObjType = "112" Then
                            qstr = "SELECT B.""SeriesName"",CAST(A.""DocNum"" AS VARCHAR) ""DocNum"",B.""SeriesName""||'/'||CAST(A.""DocNum"" AS VARCHAR) ""StrDocNum"" " & vbNewLine &
                               " From ""ODRF"" A " & vbNewLine &
                               " INNER JOIN ""NNM1"" B ON B.""ObjectCode""=A.""ObjType"" AND B.""Series""=A.""Series"" " & vbNewLine &
                               " WHERE A.""DocEntry""='" + DLEntry + "' "
                        ElseIf ObjType = "13" Then
                            qstr = "SELECT A.""DocTotal"",B.""SeriesName"",CAST(A.""DocNum"" AS VARCHAR) ""DocNum"",B.""SeriesName""||'/'||CAST(A.""DocNum"" AS VARCHAR) ""StrDocNum"" " & vbNewLine &
                               " From ""OINV"" A " & vbNewLine &
                               " INNER JOIN ""NNM1"" B ON B.""ObjectCode""=A.""ObjType"" AND B.""Series""=A.""Series"" " & vbNewLine &
                               " WHERE A.""DocEntry""='" + DLEntry + "' "
                        End If
                        rSet.DoQuery(qstr)
                        Dim ReturnDocNo = rSet.Fields.Item("StrDocNum").Value
                        Dim Series = rSet.Fields.Item("SeriesName").Value
                        Dim DocNum = rSet.Fields.Item("DocNum").Value
                        Dim doctotal As Double = rSet.Fields.Item("DocTotal").Value

                        If CType(rSet.Fields.Item("DocTotal").Value, Double) > 0 Then
                            Try
                                Dim IncomingPAymentEntry As String = ""
                                Dim ermsg As String = ""
                                Dim CashAmount As Decimal = 0
                                If CType(rSet.Fields.Item("DocTotal").Value, Double) <= 0.5 Then
                                    CashAmount = CType(rSet.Fields.Item("DocTotal").Value, Double)
                                End If
                                If IncomingPaymentAdd(G_DI_Company, InvoiceDetails, DLEntry, UserID, Branch, DiscountExists, CashAmount, IncomingPAymentEntry, ermsg) = False Then
                                    If G_DI_Company.InTransaction Then
                                        G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                                    End If

                                    G_DI_Company.Disconnect()
                                    NewRow = dtTable.NewRow
                                    NewRow.Item("ReturnCode") = "-3333"
                                    NewRow.Item("ReturnDocEntry") = "-1"
                                    NewRow.Item("ReturnObjType") = "-1"
                                    NewRow.Item("ReturnSeries") = "-1"
                                    NewRow.Item("ReturnDocNum") = "-1"
                                    NewRow.Item("ReturnMsg") = ermsg
                                    dtTable.Rows.Add(NewRow)
                                    Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                                End If
                            Catch ex As Exception
                                If G_DI_Company.InTransaction Then
                                    G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                                End If

                                G_DI_Company.Disconnect()
                                NewRow = dtTable.NewRow
                                NewRow.Item("ReturnCode") = "-2222"
                                NewRow.Item("ReturnDocEntry") = "-1"
                                NewRow.Item("ReturnObjType") = "-1"
                                NewRow.Item("ReturnSeries") = "-1"
                                NewRow.Item("ReturnDocNum") = "-1"
                                NewRow.Item("ReturnMsg") = ex.Message
                                dtTable.Rows.Add(NewRow)
                                Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                            End Try
                        End If


                        G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit)
                        G_DI_Company.Disconnect()
                        NewRow = dtTable.NewRow
                        NewRow.Item("ReturnCode") = "0000"
                        NewRow.Item("ReturnDocEntry") = DLEntry
                        NewRow.Item("ReturnObjType") = ObjType
                        NewRow.Item("ReturnSeries") = Series
                        NewRow.Item("ReturnDocNum") = DocNum
                        NewRow.Item("ReturnMsg") = "Your Request No. " + ReturnDocNo + " successfully submitted"
                        dtTable.Rows.Add(NewRow)
                        Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                    End If
                Else
                    If G_DI_Company.InTransaction Then
                        G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                    End If
                    G_DI_Company.Disconnect()
                    NewRow = dtTable.NewRow
                    NewRow.Item("ReturnCode") = "-2222"
                    NewRow.Item("ReturnDocEntry") = "-1"
                    NewRow.Item("ReturnObjType") = "-1"
                    NewRow.Item("ReturnSeries") = "-1"
                    NewRow.Item("ReturnDocNum") = "-1"
                    NewRow.Item("ReturnMsg") = "Series not found"
                    dtTable.Rows.Add(NewRow)
                    Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                End If

            Catch __unusedException1__ As Exception
                Try
                    If G_DI_Company.InTransaction Then
                        G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                    End If
                Catch ex As Exception
                End Try
                Try
                    G_DI_Company.Disconnect()
                Catch ex1 As Exception
                End Try
                NewRow = dtTable.NewRow
                NewRow.Item("ReturnCode") = "-2222"
                NewRow.Item("ReturnMsg") = __unusedException1__.Message.ToString
                NewRow.Item("ReturnDocEntry") = "-1"
                NewRow.Item("ReturnObjType") = "-1"
                NewRow.Item("ReturnSeries") = "-1"
                NewRow.Item("ReturnDocNum") = "-1"
                dtTable.Rows.Add(NewRow)
                Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
            End Try

        End Function

        Public Function IncomingPaymentAdd(ByVal G_DI_Company As SAPbobsCOM.Company, ByVal InvoiceDetails As SIL_MODEL_INVC_HEADER, ByVal InvoiceEntry As String, ByVal UserId As String, ByVal Branch As String,
                               ByVal DiscountExists As Boolean, ByVal CashAmount As Decimal, ByRef DpmEntry As String, ByRef erMessage As String) As Boolean
            Try
                Dim VoucherPaymentNo As String = ""
                rSet = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                Dim qstr As String
                qstr = "Select Top 1 N.""Series"",B.""BPLId"",C.""WhsCode"" ""WhsCode"" ,TO_CHAR(NOW(),'YYYYYMMDD') ""DocDate"" " & vbNewLine &
                                "  From ""NNM1"" N  " & vbNewLine &
                                "       Inner Join ""OFPR"" O On O.""Indicator""=N.""Indicator"" " & vbNewLine &
                                "       Inner Join ""OBPL"" B On B.""BPLId""=N.""BPLId"" " & vbNewLine &
                                "       Inner Join ""OWHS"" C On C.""U_BUSUNIT""='" + Branch + "' AND C.""U_WHSTYPE""='N' " & vbNewLine &
                                " Where N.""ObjectCode""='24' " & vbNewLine &
                                "   And O.""PeriodStat"" In ('N','C') And N.""Locked""='N' AND N.""Remark""='" + Branch + "'  " & vbNewLine &
                                "   And TO_CHAR('" + InvoiceDetails.PostingDate + "','YYYYYMMDD') Between TO_CHAR(O.""F_RefDate"",'YYYYYMMDD') And TO_CHAR(O.""T_RefDate"",'YYYYYMMDD')"

                Dim IncPayment As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                IncPayment.DoQuery(qstr)
                If IncPayment.RecordCount > 0 Then
                Else
                    erMessage = "Incoimng Payment Series not Found"
                    Return False
                End If
                Dim InPay As SAPbobsCOM.Payments = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oIncomingPayments)

                qstr = "SELECT * FROM  ""OINV"" WHERE ""DocEntry""='" + InvoiceEntry.ToString + "'"
                Dim SoHrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                SoHrSet.DoQuery(qstr)

                qstr = "SELECT * FROM  ""INV1"" WHERE ""DocEntry""='" + InvoiceEntry.ToString + "'"
                Dim SoDrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                SoDrSet.DoQuery(qstr)
                Dim cardcode = SoHrSet.Fields.Item("CardCode").Value
                InPay.CardCode = SoHrSet.Fields.Item("CardCode").Value
                InPay.UserFields.Fields.Item("U_BRANCH").Value = Branch
                InPay.UserFields.Fields.Item("U_CRTDBY").Value = UserId
                'InPay.DocDate = New Date(Mid(getServerDate, 1, 4), Mid(getServerDate, 5, 2), Mid(getServerDate, 7, 2))
                InPay.BPLID = SoHrSet.Fields.Item("BPLId").Value
                InPay.DocDate = New Date(Mid(InvoiceDetails.PostingDate, 1, 4), Mid(InvoiceDetails.PostingDate, 5, 2), Mid(InvoiceDetails.PostingDate, 7, 2))
                InPay.Series = IncPayment.Fields.Item("Series").Value
                InPay.Invoices.DocEntry = Convert.ToInt32(InvoiceEntry)
                InPay.Invoices.InvoiceType = SAPbobsCOM.BoRcptInvTypes.it_Invoice
                InPay.Invoices.DistributionRule = Branch
                InPay.Invoices.Add()
                Dim ARDown As SAPbobsCOM.Documents = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices)
                ARDown = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices)
                ARDown.DocObjectCode = SAPbobsCOM.BoObjectTypes.oInvoices
                ARDown.GetByKey(Convert.ToInt32(InvoiceEntry))
                Dim Paymentexists As Boolean = False
                For Each Item As DTS_MODEL_PMNT_DTLS In InvoiceDetails.PaymentDetails
                    If Item.Amount = 0 Then
                        Continue For
                    End If
                    If Item.PaymentType = "S" Then
                        Paymentexists = True
                        InPay.CashSum = Item.Amount
                        qstr = "SELECT ""OverCode"",""FrgnName"",""AcctCode"" FROM OACT  WHERE ""OverCode""='" + Branch + "' AND ""FrgnName""='S'"
                        Dim CashPayment As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                        CashPayment.DoQuery(qstr)
                        InPay.CashAccount = CashPayment.Fields.Item("AcctCode").Value

                    End If

                    If Item.PaymentType = "S" Then
                    Else
                        Paymentexists = True
                        'qstr = "SELECT ""OverCode"",""FrgnName"",""AcctCode"" FROM OACT  WHERE ""OverCode""='" + Branch + "' AND ""FrgnName""='C'"
                        qstr = "SELECT ""CreditCard""  FROM ""OCRC"" where ""U_BANKCODE""='" + Item.Bank + "' AND ""U_PMNTP""='" + Item.PaymentType + "'"
                        Dim CreditPayment As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                        CreditPayment.DoQuery(qstr)
                        If CreditPayment.RecordCount > 0 Then
                            InPay.CreditCards.CreditCard = CreditPayment.Fields.Item("CreditCard").Value
                            If Item.PaymentType = "2" Or Item.PaymentType = "8" Then
                                If DiscountExists = True Then
                                    erMessage = "Discount can not be done for Voucher type Payment please Remove Item Discount"
                                    Return False
                                End If
                                qstr = "SELECT ""PrcCode"",""PrcName"" FROM ""OPRC"" WHERE ""PrcCode""='" + Item.CardNo + "' and ""DimCode""='5' and ""U_CARDCODE""='" + cardcode + "'"
                                Dim VouchNameStr As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                                VouchNameStr.DoQuery(qstr)
                                If VouchNameStr.RecordCount > 0 Then
                                    InPay.CreditCards.CreditCardNumber = VouchNameStr.Fields.Item("PrcCode").Value
                                    InPay.CreditCards.UserFields.Fields.Item("U_CARDNO").Value = VouchNameStr.Fields.Item("PrcCode").Value
                                    VoucherPaymentNo = IIf(VoucherPaymentNo = "", Item.CardNo + ":" + Item.Amount.ToString, VoucherPaymentNo + ";" + Item.CardNo + ":" + Item.Amount.ToString)
                                Else
                                    erMessage = "No Voucher found for " + Item.CardNo
                                    Return False
                                End If

                            Else
                                Try
                                    InPay.CreditCards.CreditCardNumber = IIf(Item.CardNo Is Nothing, "1111", Right(Item.CardNo.ToString, 4))
                                    InPay.CreditCards.UserFields.Fields.Item("U_CARDNO").Value = IIf(Item.CardNo Is Nothing, "1111", Item.CardNo.ToString)
                                Catch ex As Exception
                                    InPay.CreditCards.CreditCardNumber = "1111"
                                    InPay.CreditCards.UserFields.Fields.Item("U_CARDNO").Value = "1111"
                                End Try
                            End If
                            'IIf(Item.CardNo.ToString = "", "1111", Right(Item.CardNo.ToString, 4))

                            InPay.CreditCards.CardValidUntil = New Date(Mid("29991231", 1, 4), Mid("29991231", 5, 2), Mid("29991231", 7, 2))
                            InPay.CreditCards.PaymentMethodCode = 1
                            InPay.CreditCards.CreditSum = Item.Amount
                            'InPay.CreditCards.FirstPaymentSum = CreditCards.CardAmount
                            Try
                                InPay.CreditCards.VoucherNum = IIf(Item.Tranid Is Nothing, "111", Item.Tranid.ToString)
                            Catch ex As Exception
                                InPay.CreditCards.VoucherNum = "111"
                            End Try

                            InPay.CreditCards.CreditType = SAPbobsCOM.BoRcptCredTypes.cr_Regular
                            InPay.CreditCards.Add()
                        Else
                            erMessage = "No Payment Method found for " + Item.Bank + " and " + Item.PaymentType
                            Return False
                        End If

                        'InPay.TransferAccount = CreditPayment.Fields.Item("AcctCode").Value
                        'InPay.TransferReference = Item.Tranid.ToString
                        'InPay.UserFields.Fields.Item("U_CRDTRNID").Value = Item.Tranid.ToString
                        'InPay.UserFields.Fields.Item("U_CRDONAME").Value = Item.CardHolderName.ToString
                        'InPay.UserFields.Fields.Item("U_CRDCNO").Value = Item.CardNo.ToString
                        'InPay.TransferDate = New Date(Mid(Date.Now.ToString("yyyyMMdd"), 1, 4), Mid(Date.Now.ToString("yyyyMMdd"), 5, 2), Mid(Date.Now.ToString("yyyyMMdd"), 7, 2))
                        'InPay.TransferSum = Item.Amount
                    End If


                    'If Item.PaymentType = "C" Then
                    '    qstr = "SELECT ""OverCode"",""FrgnName"",""AcctCode"" FROM OACT  WHERE ""OverCode""='" + Branch + "' AND ""FrgnName""='C'"
                    '    Dim CreditPayment As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                    '    CreditPayment.DoQuery(qstr)
                    '    InPay.TransferAccount = CreditPayment.Fields.Item("AcctCode").Value
                    '    InPay.TransferReference = Item.Tranid.ToString
                    '    InPay.UserFields.Fields.Item("U_CRDTRNID").Value = Item.Tranid.ToString
                    '    InPay.UserFields.Fields.Item("U_CRDONAME").Value = Item.CardHolderName.ToString
                    '    InPay.UserFields.Fields.Item("U_CRDCNO").Value = Item.CardNo.ToString
                    '    InPay.TransferDate = New Date(Mid(Date.Now.ToString("yyyyMMdd"), 1, 4), Mid(Date.Now.ToString("yyyyMMdd"), 5, 2), Mid(Date.Now.ToString("yyyyMMdd"), 7, 2))
                    '    InPay.TransferSum = Item.Amount
                    'End If
                    'If Item.PaymentType = "U" Then
                    '    qstr = "SELECT ""OverCode"",""FrgnName"",""AcctCode"" FROM OACT  WHERE ""OverCode""='" + Branch + "' AND ""FrgnName""='U'"
                    '    Dim UPIPayment As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                    '    UPIPayment.DoQuery(qstr)
                    '    InPay.CheckAccount = UPIPayment.Fields.Item("AcctCode").Value
                    '    InPay.Checks.CountryCode = "BD"
                    '    InPay.Checks.CheckSum = Item.Amount
                    '    InPay.Checks.UserFields.Fields.Item("U_TRNSID").Value = Item.Tranid.ToString
                    '    InPay.Checks.DueDate = New Date(Mid(Date.Now.ToString("yyyyMMdd"), 1, 4), Mid(Date.Now.ToString("yyyyMMdd"), 5, 2), Mid(Date.Now.ToString("yyyyMMdd"), 7, 2))
                    '    qstr = "SELECT A.""BankCode"" " & vbNewLine &
                    '               " FROM ""ODSC"" A " & vbNewLine &
                    '               "    INNER JOIN ""CUFD"" B ON B.""TableID""='ODSC' AND B.""AliasID""='TYPE' " & vbNewLine &
                    '               "    INNER JOIN ""UFD1"" C ON C.""TableID""=B.""TableID"" AND B.""FieldID""=C.""FieldID"" AND A.""U_TYPE""=C.""FldValue"" " & vbNewLine &
                    '           " WHERE A.""U_TYPE""='" + Item.UpiName + "' "
                    '    Dim UPIBankPayment As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                    '    UPIBankPayment.DoQuery(qstr)
                    '    InPay.Checks.BankCode = UPIBankPayment.Fields.Item("BankCode").Value
                    '    InPay.Checks.CheckNumber = 1
                    '    'InPay.Checks.
                    '    InPay.Checks.Add()
                    'End If
                Next
                If Paymentexists = False Then
                    InPay.CashSum = CashAmount
                    qstr = "SELECT ""OverCode"",""FrgnName"",""AcctCode"" FROM OACT  WHERE ""OverCode""='" + Branch + "' AND ""FrgnName""='S'"
                    Dim CashPayment As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                    CashPayment.DoQuery(qstr)
                    InPay.CashAccount = CashPayment.Fields.Item("AcctCode").Value
                End If
                Dim lRetCode As Integer = InPay.Add
                If lRetCode <> 0 Then
                    Dim sErrMsg As String = G_DI_Company.GetLastErrorDescription()
                    G_DI_Company.GetLastError(lRetCode, sErrMsg)
                    erMessage = sErrMsg
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(InPay)
                    Return False
                End If
                Dim IncDocEntry = G_DI_Company.GetNewObjectKey.Trim.ToString
                If VoucherPaymentNo <> "" Then
                    For Each Dtls As String In VoucherPaymentNo.Split(New String() {";"}, StringSplitOptions.None)
                        Dim PrcCode = Dtls.Split(":")(0)
                        Dim Value As Decimal = Dtls.Split(":")(1)
                        ' qstr ="SELECT ""PrcName"" FROM ""OPRC"" WHERE ""PrcCode"""
                        qstr = "UPDATE A SET ""OcrCode5""='" + PrcCode.ToString + "' " & vbNewLine &
                               " FROM ""JDT1"" A " & vbNewLine &
                               "    INNER JOIN ""OJDT"" B ON A.""TransId""=B.""TransId"" " & vbNewLine &
                               "    INNER JOIN ""ORCT"" C ON C.""TransId""=B.""TransId"" AND C.""Canceled""='N' --AND C.""CardCode"" =A.""ShortName"" " & vbNewLine &
                               "    INNER JOIN ""OACT"" E ON E.""AcctCode""=A.""Account"" AND E.""FrgnName""='VS' " & vbNewLine &
                               "    INNER JOIN ""OPRC"" F ON F.""PrcCode""='" + PrcCode.ToString + "' " & vbNewLine &
                               " WHERE A.""Debit""<>0 " & vbNewLine &
                               " and F.""PrcCode""='" + PrcCode.ToString + "' " & vbNewLine &
                               " AND A.""Debit""='" + Value.ToString + "' " & vbNewLine &
                               " AND C.""DocEntry""='" + IncDocEntry.ToString + "' " & vbNewLine &
                               "  AND IFNULL(A.""OcrCode5"",'')='' "
                        Dim IncomingVchUpdt As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                        IncomingVchUpdt.DoQuery(qstr)
                    Next

                End If
                Return True
            Catch ex As Exception
                erMessage = ex.Message
                Return False
            End Try
        End Function

        Public Function VoucherCostCenterAdd(ByVal G_DI_Company As SAPbobsCOM.Company, ByVal InvoiceDetails As SIL_MODEL_INVC_HEADER, ByVal VoucherNo As String,
                               ByVal ValidTill As String, ByVal VoucherType As String, ByRef CostCenterCode As String, ByRef erMessage As String) As Boolean
            Try

                rSet = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                Dim qstr As String
                Dim oCmpSrv As SAPbobsCOM.CompanyService
                'oCmpSrv = oCompany.GetCompanyService()
                Dim oPCService As SAPbobsCOM.IProfitCentersService
                Dim oPC As SAPbobsCOM.IProfitCenter
                Dim oPCParams As SAPbobsCOM.IProfitCenterParams
                Dim oPCsParams As SAPbobsCOM.IProfitCentersParams
                oCmpSrv = G_DI_Company.GetCompanyService()
                oPCService = oCmpSrv.GetBusinessService(SAPbobsCOM.ServiceTypes.ProfitCentersService)
                oPCParams = oPCService.GetDataInterface(SAPbobsCOM.DimensionsServiceDataInterfaces.dsDimensionParams)
                qstr = "SELECT IFNULL(MAX(CAST(""PrcCode"" AS INT))+1,1) ""PrcCode"" FROM ""OPRC"" WHERE ""DimCode""=5 AND ""PrcCode""<>'Centr_z5'"
                Dim COSTrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                COSTrSet.DoQuery(qstr)

                oPC = oPCService.GetDataInterface(SAPbobsCOM.ProfitCentersServiceDataInterfaces.pcsProfitCenter)
                oPC.CenterCode = COSTrSet.Fields.Item("PrcCode").Value.ToString
                CostCenterCode = COSTrSet.Fields.Item("PrcCode").Value.ToString
                'erMessage = COSTrSet.Fields.Item("PrcCode").Value.ToString
                'Return False
                oPC.CenterName = Left(VoucherNo.ToString, 30)
                oPC.UserFields.Item("U_VCHNO").Value = VoucherNo.ToString
                oPC.UserFields.Item("U_CARDCODE").Value = InvoiceDetails.CardCode.ToString
                oPC.UserFields.Item("U_VCHTYPE").Value = VoucherType
                oPC.Effectivefrom = New Date(Mid(InvoiceDetails.PostingDate, 1, 4), Mid(InvoiceDetails.PostingDate, 5, 2), Mid(InvoiceDetails.PostingDate, 7, 2))
                oPC.EffectiveTo = New Date(Mid(ValidTill, 1, 4), Mid(ValidTill, 5, 2), Mid(ValidTill, 7, 2))
                oPC.InWhichDimension = 5

                oPCParams = oPCService.AddProfitCenter(oPC)
                'Dim lRetCode As Integer
                'lRetCode = oPCParams.Add

                Return True
            Catch ex As Exception
                erMessage = ex.Message
                Return False
            End Try
        End Function

        <Route("Api/PostECommerceInvoiceWithPayment")>
        <HttpPost>
        Public Function PostECommerceInvoiceWithPayment(ByVal InvoiceDetails As SIL_MODEL_INVC_HEADER) As HttpResponseMessage

            Dim G_DI_Company As SAPbobsCOM.Company = Nothing
            Dim strRegnNo As String = ""
            dtTable.Columns.Add("ReturnCode")
            dtTable.Columns.Add("ReturnDocEntry")
            dtTable.Columns.Add("ReturnObjType")
            dtTable.Columns.Add("ReturnSeries")
            dtTable.Columns.Add("ReturnDocNum")
            dtTable.Columns.Add("ReturnMsg")
            Try
                Dim Branch As String = ""
                Dim UserID As String = ""


                If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                    Return Request.CreateResponse(HttpStatusCode.OK, dtError)
                End If
                Dim qstr As String = ""
                'Dim qstr As String = ""
                qstr = "SELECT ""U_SAPUNAME"" ,""U_SAPPWD"" " & vbNewLine &
                       " FROM ""OUSR"" A " & vbNewLine &
                       " WHERE A.""USER_CODE"" ='" + UserID + "'"
                Dim dRow As Data.DataRow
                dRow = DBSQL.getQueryDataRow(qstr, "")
                Dim SAPUserId As String = ""
                Dim SAPPassword As String = ""
                Try
                    SAPUserId = dRow.Item("U_SAPUNAME")
                    SAPPassword = dRow.Item("U_SAPPWD")
                Catch ex As Exception
                End Try
                Connection.ConnectSAP(G_DI_Company, SAPUserId, SAPPassword)

                rSet = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                qstr = "Select Top 1 N.""Series"",B.""BPLId"",D.""WhsCode"" ""WhsCode"" ,TO_CHAR(NOW(),'YYYYYMMDD') ""DocDate"" " & vbNewLine &
                                "  From ""NNM1"" N  " & vbNewLine &
                                "       Inner Join ""OFPR"" O On O.""Indicator""=N.""Indicator"" " & vbNewLine &
                                "       Inner Join ""OBPL"" B On B.""BPLId""=N.""BPLId"" " & vbNewLine &
                                "       Inner Join ""OWHS"" C On C.""U_BUSUNIT""='" + Branch + "' AND C.""U_WHSTYPE""='N' " & vbNewLine &
                                "       Inner Join ""OWHS"" D On D.""U_BUSUNIT""='" + InvoiceDetails.ToBranch + "' AND C.""U_WHSTYPE""='N' " & vbNewLine &
                                " Where N.""ObjectCode""='13' " & vbNewLine &
                                "   And O.""PeriodStat"" In ('N','C') And N.""Locked""='N' AND N.""Remark""='" + Branch + "'  " & vbNewLine &
                                "   And TO_CHAR('" + InvoiceDetails.PostingDate + "','YYYYYMMDD') Between TO_CHAR(O.""F_RefDate"",'YYYYYMMDD') And TO_CHAR(O.""T_RefDate"",'YYYYYMMDD')"
                rSet.DoQuery(qstr)

                If rSet.RecordCount > 0 Then
                    Dim DocDate As String = InvoiceDetails.PostingDate
                    Dim Invoice As SAPbobsCOM.Documents = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices)
                    Invoice.DocObjectCode = SAPbobsCOM.BoObjectTypes.oInvoices

                    Invoice.CardCode = InvoiceDetails.CardCode
                    'Delivery.BPL_IDAssignedToInvoice = InvoiceDetails.Branch
                    Invoice.BPL_IDAssignedToInvoice = rSet.Fields.Item("BPLId").Value
                    Invoice.Series = rSet.Fields.Item("Series").Value
                    Invoice.DocDate = New Date(Mid(DocDate, 1, 4), Mid(DocDate, 5, 2), Mid(DocDate, 7, 2))
                    Invoice.DocDueDate = New Date(Mid(DocDate, 1, 4), Mid(DocDate, 5, 2), Mid(DocDate, 7, 2))
                    Invoice.TaxDate = New Date(Mid(InvoiceDetails.RefDate, 1, 4), Mid(InvoiceDetails.RefDate, 5, 2), Mid(InvoiceDetails.RefDate, 7, 2))
                    Invoice.NumAtCard = InvoiceDetails.RefNo
                    'Invoice.SalesPersonCode = InvoiceDetails.SalesEmployee.ToString
                    Invoice.Comments = InvoiceDetails.Remarks
                    Invoice.UserFields.Fields.Item("U_TOBUNIT").Value = InvoiceDetails.ToBranch
                    Invoice.UserFields.Fields.Item("U_CRTDBY").Value = UserID
                    Invoice.UserFields.Fields.Item("U_BRANCH").Value = Branch
                    Dim TotalValue As Double = 0
                    For Each Item As SIL_MODEL_INVC_ITEMS In InvoiceDetails.Items
                        If Item.BaseType = "17" Or Item.BaseType = "15" Then
                            Invoice.Lines.BaseEntry = Item.BaseEntry
                            Invoice.Lines.BaseLine = Item.BaseLine
                            If Item.BaseType = "17" Then
                                Invoice.Lines.BaseType = SAPbobsCOM.BoObjectTypes.oOrders
                            ElseIf Item.BaseType = "15" Then
                                Invoice.Lines.BaseType = SAPbobsCOM.BoObjectTypes.oDeliveryNotes
                            End If
                        Else
                            Invoice.Lines.UserFields.Fields.Item("U_DISCPER").Value = Item.Discountamount
                            'Invoice.Lines.Quantity = Item.Quantity
                        End If
                        Invoice.Lines.WarehouseCode = Item.WhsCode
                        qstr = "SELECT ""LineTotal""+""VatSum"" ""TOTAL"",""Quantity"" FROM ""RDR1"" WHERE ""DocEntry""='" + Item.BaseEntry.ToString + "' AND ""LineNum""='" + Item.BaseLine.ToString + "'"
                        Dim SOrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                        SOrSet.DoQuery(qstr)
                        Dim LineValue As Decimal = SOrSet.Fields.Item("TOTAL").Value / CType(SOrSet.Fields.Item("Quantity").Value, Decimal)
                        TotalValue = TotalValue + (LineValue * Item.Quantity)
                        'Invoice.Lines.TaxCode = Item.TaxCode
                        Invoice.Lines.Quantity = Item.Quantity
                        Invoice.Lines.CostingCode = Branch
                        Invoice.Lines.COGSCostingCode = InvoiceDetails.ToBranch
                        Dim oItems As SAPbobsCOM.Items = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oItems)
                        oItems.GetByKey(Item.ItemCode)
                        If oItems.ManageBatchNumbers = SAPbobsCOM.BoYesNoEnum.tYES Then
                            Dim i As Integer = 0
                            For Each Batches As DTS_MODEL_INVC_BATCH In Item.Batches
                                If Batches.VisOrder = Item.VisOrder Then
                                    'oGIIssue.Lines.BatchNumbers.SetCurrentLine(i)
                                    Invoice.Lines.BatchNumbers.BatchNumber = Batches.BatchNo
                                    Invoice.Lines.BatchNumbers.Quantity = Batches.BatchQuantity
                                    'If i <> 0 Then
                                    '    Delivery.Lines.BatchNumbers.Add()
                                    'End If
                                    Invoice.Lines.BatchNumbers.Add()

                                    i = i + 1
                                End If
                            Next
                        End If
                        If oItems.ManageSerialNumbers = SAPbobsCOM.BoYesNoEnum.tYES Then
                            Dim i As Integer = 0
                            For Each Serial As DTS_MODEL_INVC_SERIAL In Item.Serial
                                If Serial.VisOrder = Item.VisOrder Then
                                    Invoice.Lines.SerialNumbers.SetCurrentLine(i)
                                    Invoice.Lines.SerialNumbers.InternalSerialNumber = Serial.InternalSerialNumber
                                    Invoice.Lines.SerialNumbers.SystemSerialNumber = Serial.SystemSerialNumber
                                    Invoice.Lines.SerialNumbers.Add()
                                    i = i + 1
                                End If
                            Next
                        End If

                        Invoice.Lines.Add()
                    Next

                    qstr = "SELECT A.""DocEntry"",B.""DocTotal""-IFNULL(V1.""DrawnSum"",0) ""Balance"" " & vbNewLine &
                           " FROM ""DPI1"" A " & vbNewLine &
                           "    INNER JOIN ""ODPI"" B ON A.""DocEntry"" =B.""DocEntry""  " & vbNewLine &
                           " LEFT OUTER JOIN " + vbNewLine &
                                       "            ( " + vbNewLine &
                                       "                SELECT A.""BaseAbs"",SUM(A.""DrawnSum"") ""DrawnSum"" " + vbNewLine &
                                       "                FROM ""INV9"" A " + vbNewLine &
                                       "                    INNER JOIN ""OINV"" B ON A.""DocEntry"" =B.""DocEntry"" " + vbNewLine &
                                       "            WHERE B.""CardCode"" ='" + InvoiceDetails.CardCode + "' " + vbNewLine &
                                       "            AND B.""CANCELED"" ='N' " + vbNewLine &
                                       "            GROUP BY A.""BaseAbs"" " + vbNewLine &
                                       "            )V1 ON V1.""BaseAbs""=B.""DocEntry"" " + vbNewLine &
                           " WHERE A.""BaseEntry"" =27 "
                    Dim DPMrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                    DPMrSet.DoQuery(qstr)
                    If DPMrSet.RecordCount > 0 Then
                        Dim i As Integer = 0
                        Dim BalnaceAmt As Double = 0
                        While Not DPMrSet.EoF
                            If TotalValue > 0 Then
                                Dim dpToDraw As SAPbobsCOM.DownPaymentsToDraw = Invoice.DownPaymentsToDraw
                                dpToDraw.SetCurrentLine(i)
                                dpToDraw.DocEntry = DPMrSet.Fields.Item("DocEntry").Value
                                If CType(DPMrSet.Fields.Item("Balance").Value, Double) >= TotalValue Then
                                    dpToDraw.AmountToDraw = TotalValue
                                    TotalValue = TotalValue - TotalValue
                                Else
                                    TotalValue = TotalValue - CType(DPMrSet.Fields.Item("Balance").Value, Double)
                                    dpToDraw.AmountToDraw = CType(DPMrSet.Fields.Item("Balance").Value, Double)
                                End If

                                dpToDraw.Add()
                                i = i + 1
                            Else
                                Exit While
                            End If
                            DPMrSet.MoveNext()
                        End While
                    End If
                    Dim lRetCode As Integer
                    lRetCode = Invoice.Add
                    Dim ErrorMessage As String = ""
                    If lRetCode <> 0 Then
                        Dim sErrMsg As String = G_DI_Company.GetLastErrorDescription()
                        G_DI_Company.GetLastError(lRetCode, sErrMsg)
                        ErrorMessage = sErrMsg
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(Invoice)
                        G_DI_Company.Disconnect()
                        NewRow = dtTable.NewRow
                        NewRow.Item("ReturnCode") = "-2222"
                        NewRow.Item("ReturnDocEntry") = "-1"
                        NewRow.Item("ReturnObjType") = "-1"
                        NewRow.Item("ReturnSeries") = "-1"
                        NewRow.Item("ReturnDocNum") = "-1"
                        NewRow.Item("ReturnMsg") = ErrorMessage
                        dtTable.Rows.Add(NewRow)
                        Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                    Else
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(Invoice)
                        Dim ObjType As String = G_DI_Company.GetNewObjectType.ToString.TrimEnd
                        Dim DLEntry As String = G_DI_Company.GetNewObjectKey.Trim.ToString
                        Dim ErrMsg As String = ""

                        If ObjType = "112" Then
                            qstr = "SELECT B.""SeriesName"",CAST(A.""DocNum"" AS VARCHAR) ""DocNum"",B.""SeriesName""||'/'||CAST(A.""DocNum"" AS VARCHAR) ""StrDocNum"" " & vbNewLine &
                               " From ""ODRF"" A " & vbNewLine &
                               " INNER JOIN ""NNM1"" B ON B.""ObjectCode""=A.""ObjType"" AND B.""Series""=A.""Series"" " & vbNewLine &
                               " WHERE A.""DocEntry""='" + DLEntry + "' "
                        ElseIf ObjType = "13" Then
                            qstr = "SELECT B.""SeriesName"",CAST(A.""DocNum"" AS VARCHAR) ""DocNum"",B.""SeriesName""||'/'||CAST(A.""DocNum"" AS VARCHAR) ""StrDocNum"" " & vbNewLine &
                               " From ""OINV"" A " & vbNewLine &
                               " INNER JOIN ""NNM1"" B ON B.""ObjectCode""=A.""ObjType"" AND B.""Series""=A.""Series"" " & vbNewLine &
                               " WHERE A.""DocEntry""='" + DLEntry + "' "
                        End If
                        rSet.DoQuery(qstr)
                        Dim ReturnDocNo = rSet.Fields.Item("StrDocNum").Value
                        G_DI_Company.Disconnect()
                        NewRow = dtTable.NewRow
                        NewRow.Item("ReturnCode") = "0000"
                        NewRow.Item("ReturnDocEntry") = DLEntry
                        NewRow.Item("ReturnObjType") = ObjType
                        NewRow.Item("ReturnSeries") = rSet.Fields.Item("SeriesName").Value
                        NewRow.Item("ReturnDocNum") = rSet.Fields.Item("DocNum").Value
                        NewRow.Item("ReturnMsg") = "Your Request No. " + ReturnDocNo + " successfully submitted"
                        dtTable.Rows.Add(NewRow)
                        Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                    End If
                Else
                    G_DI_Company.Disconnect()
                    NewRow = dtTable.NewRow
                    NewRow.Item("ReturnCode") = "-2222"
                    NewRow.Item("ReturnDocEntry") = "-1"
                    NewRow.Item("ReturnObjType") = "-1"
                    NewRow.Item("ReturnSeries") = "-1"
                    NewRow.Item("ReturnDocNum") = "-1"
                    NewRow.Item("ReturnMsg") = "Series not found"
                    dtTable.Rows.Add(NewRow)
                    Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                End If

            Catch __unusedException1__ As Exception
                Try
                    G_DI_Company.Disconnect()
                Catch ex1 As Exception
                End Try
                NewRow = dtTable.NewRow
                NewRow.Item("ReturnCode") = "-2222"
                NewRow.Item("ReturnMsg") = __unusedException1__.Message.ToString
                NewRow.Item("ReturnDocEntry") = "-1"
                NewRow.Item("ReturnObjType") = "-1"
                NewRow.Item("ReturnSeries") = "-1"
                NewRow.Item("ReturnDocNum") = "-1"
                dtTable.Rows.Add(NewRow)
                Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
            End Try

        End Function


        <Route("Api/PostActivity")>
        <HttpPost>
        Public Function PostActivity(ByVal Activity As DTS_MODEL_USER_ACTIVITYLIST) As HttpResponseMessage
            Dim G_DI_Company As SAPbobsCOM.Company = Nothing
            Dim strRegnNo As String = ""
            dtTable.Columns.Add("ReturnCode")
            dtTable.Columns.Add("ReturnDocEntry")
            dtTable.Columns.Add("ReturnObjType")
            dtTable.Columns.Add("ReturnSeries")
            dtTable.Columns.Add("ReturnDocNum")
            dtTable.Columns.Add("ReturnMsg")
            Try
                Dim Branch As String = ""
                Dim UserID As String = ""


                If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                    Return Request.CreateResponse(HttpStatusCode.OK, dtError)
                End If
                Dim qstr As String = ""
                'Dim qstr As String = ""
                qstr = "SELECT ""U_SAPUNAME"" ,""U_SAPPWD"" " & vbNewLine &
                       " FROM ""OUSR"" A " & vbNewLine &
                       " WHERE A.""USER_CODE"" ='" + UserID + "'"
                Dim dRow As Data.DataRow
                dRow = DBSQL.getQueryDataRow(qstr, "")
                Dim SAPUserId As String = ""
                Dim SAPPassword As String = ""
                Try
                    SAPUserId = dRow.Item("U_SAPUNAME")
                    SAPPassword = dRow.Item("U_SAPPWD")
                Catch ex As Exception
                End Try
                Connection.ConnectSAP(G_DI_Company, SAPUserId, SAPPassword)
                rSet = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                'Dim oCompServ As SAPbobsCOM.CompanyService = Nothing
                'Dim oActivityServ As SAPbobsCOM.ActivitiesService = Nothing
                'Dim vNewActivity As SAPbobsCOM.Activity = Nothing
                'oCompServ = G_DI_Company.GetCompanyService
                'oActivityServ = oCompServ.GetBusinessService(SAPbobsCOM.ServiceTypes.ActivitiesService)
                'vNewActivity = oActivityServ.GetDataInterface(SAPbobsCOM.ActivitiesServiceDataInterfaces.asActivity)
                'vNewActivity.CardCode = "C0000003"
                'vNewActivity.HandledBy = dsOrders.Tables(0).Rows(iterOrders).Item("HandledBy")
                'oActivityServ.AddActivity(vNewActivity)
                Dim ActivityID As String = ""
                Try
                    G_DI_Company.StartTransaction()
                    For Each Item As DTS_MODEL_USER_ACTIVITY In Activity.Activity
                        Dim vContact As SAPbobsCOM.Contacts = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oContacts)
                        'If vContact.GetByKey("1") Then
                        '    vContact.EndDuedate = New Date(Mid(PaymentDetails.RefDate, 1, 4), Mid(PaymentDetails.RefDate, 5, 2), Mid(PaymentDetails.RefDate, 7, 2))
                        '    vContact.EndTime = New DateTime(Mid(PaymentDetails.RefDate, 1, 4), Mid(PaymentDetails.RefDate, 5, 2), Mid(PaymentDetails.RefDate, 7, 2), "21", "03", "00")
                        '    vContact.Closed = SAPbobsCOM.BoYesNoEnum.tYES

                        'Else

                        'End If
                        qstr = "SELECT ""U_CARDCODE"" FROM ""OPRC"" WHERE ""PrcCode""='" + Branch + "'"
                        rSet.DoQuery(qstr)
                        vContact.Activity = SAPbobsCOM.BoActivities.cn_Task
                        vContact.ActivityType = Item.ActivityType
                        If Item.Activity <> "" Then
                            vContact.Subject = Item.Activity
                        End If

                        'vContact.CardCode = rSet.Fields.Item("U_CARDCODE").Value
                        Try
                            If Item.CardCode <> Nothing Then
                                vContact.CardCode = Item.CardCode
                            Else
                                vContact.CardCode = rSet.Fields.Item("U_CARDCODE").Value
                            End If
                        Catch ex As Exception
                            vContact.CardCode = rSet.Fields.Item("U_CARDCODE").Value
                        End Try
                        Try
                            If Item.SoEntry <> Nothing Then
                                vContact.DocType = Item.ObjType
                                vContact.DocEntry = Item.SoEntry
                                vContact.UserFields.Fields.Item("U_LINENUM").Value = Item.LineId
                            End If
                        Catch ex As Exception
                        End Try
                        Try
                            vContact.Notes = IIf(Item.Comments Is Nothing, "", Item.Comments)
                        Catch ex As Exception
                        End Try

                        'vContact.DurationType = SAPbobsCOM.BoDurations.du_Days
                        'vContact.Duration = 1
                        vContact.Details = Item.TaskDetails
                        'vContact.HandledBy = 1
                        'vContact

                        vContact.StartDate = New Date(Mid(Activity.ActivityDate, 1, 4), Mid(Activity.ActivityDate, 5, 2), Mid(Activity.ActivityDate, 7, 2))
                        Try
                            If vContact.StartTime <> "" Then
                                Try
                                    vContact.StartTime = New DateTime(Mid(Activity.ActivityDate, 1, 4), Mid(Activity.ActivityDate, 5, 2), Mid(Activity.ActivityDate, 7, 2), Item.FromTime.ToString.Split(":")(0), Item.FromTime.ToString.Split(":")(1), "00").ToString("dd/MM/yyyy HH:mm:ss")
                                Catch ex As Exception
                                End Try

                            End If
                        Catch ex As Exception
                        End Try


                        'vContact.ContactDate = New Date(Mid(PaymentDetails.RefDate, 1, 4), Mid(PaymentDetails.RefDate, 5, 2), Mid(PaymentDetails.RefDate, 7, 2))
                        'vContact.ContactTime = New DateTime(Mid(PaymentDetails.RefDate, 1, 4), Mid(PaymentDetails.RefDate, 5, 2), Mid(PaymentDetails.RefDate, 7, 2), "21", "03", "00")
                        Try
                            If Item.ToTime <> "" Then
                                vContact.EndDuedate = New Date(Mid(Activity.ActivityDate, 1, 4), Mid(Activity.ActivityDate, 5, 2), Mid(Activity.ActivityDate, 7, 2))
                                Try
                                    vContact.EndTime = New DateTime(Mid(Activity.ActivityDate, 1, 4), Mid(Activity.ActivityDate, 5, 2), Mid(Activity.ActivityDate, 7, 2), Item.ToTime.ToString.Split(":")(0), Item.ToTime.ToString.Split(":")(0), "00").ToString("dd/MM/yyyy HH:mm:ss")
                                Catch ex As Exception
                                End Try

                            End If
                        Catch ex As Exception
                        End Try

                        'If G_DI_Company.InTransaction Then
                        '    G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                        'End If
                        'G_DI_Company.Disconnect()
                        'NewRow = dtTable.NewRow
                        'NewRow.Item("ReturnCode") = "-2222"
                        'NewRow.Item("ReturnDocEntry") = "-1"
                        'NewRow.Item("ReturnObjType") = "-1"
                        'NewRow.Item("ReturnSeries") = "-1"
                        'NewRow.Item("ReturnDocNum") = "-1"
                        'NewRow.Item("ReturnMsg") = "T1"
                        'dtTable.Rows.Add(NewRow)
                        'Return Request.CreateResponse(HttpStatusCode.OK, dtTable)

                        Try
                            vContact.Status = "-3"
                        Catch ex As Exception
                        End Try
                        Try
                            vContact.UserFields.Fields.Item("U_SLPCODE").Value = IIf(Item.AgentCode Is Nothing, "", Item.AgentCode)
                        Catch ex As Exception
                        End Try
                        Try
                            vContact.UserFields.Fields.Item("U_BUNITCD").Value = IIf(Item.VisitingBranch Is Nothing, "", Item.VisitingBranch)
                        Catch ex As Exception
                        End Try
                        'Try
                        vContact.UserFields.Fields.Item("U_DOSTATUS").Value = IIf(Item.DailyOperationStatus Is Nothing, "", Item.DailyOperationStatus)
                        'Catch ex As Exception
                        'End Try

                        vContact.UserFields.Fields.Item("U_BUNIT").Value = Branch
                        vContact.UserFields.Fields.Item("U_CRTDBY").Value = UserID
                        vContact.UserFields.Fields.Item("U_SESSION").Value = IIf(Item.Session Is Nothing, "", Item.Session)
                        Try
                            vContact.UserFields.Fields.Item("U_DOCTOR").Value = IIf(Item.Doctor Is Nothing, "", Item.Doctor)
                        Catch ex As Exception
                        End Try
                        Try
                            vContact.UserFields.Fields.Item("U_THERPIST").Value = IIf(Item.Therapist Is Nothing, "", Item.Therapist)
                        Catch ex As Exception
                        End Try
                        Try
                            vContact.UserFields.Fields.Item("U_HWMYSTK").Value = IIf(Item.HowManySession Is Nothing, "", Item.HowManySession)
                        Catch ex As Exception
                        End Try

                        Dim lRetCode As Integer = vContact.Add()

                        'Dim lRetCode As Integer = vContact.Update()
                        Dim ErrorMessage As String = ""
                        If lRetCode <> 0 Then
                            Dim sErrMsg As String = G_DI_Company.GetLastErrorDescription()
                            G_DI_Company.GetLastError(lRetCode, sErrMsg)
                            ErrorMessage = sErrMsg
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(vContact)
                            If G_DI_Company.InTransaction Then
                                G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                            End If
                            G_DI_Company.Disconnect()
                            NewRow = dtTable.NewRow
                            NewRow.Item("ReturnCode") = "-2222"
                            NewRow.Item("ReturnDocEntry") = "-1"
                            NewRow.Item("ReturnObjType") = "-1"
                            NewRow.Item("ReturnSeries") = "-1"
                            NewRow.Item("ReturnDocNum") = "-1"
                            NewRow.Item("ReturnMsg") = ErrorMessage
                            dtTable.Rows.Add(NewRow)
                            Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                        Else
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(vContact)
                            Dim ObjType As String = G_DI_Company.GetNewObjectType.ToString.TrimEnd
                            Dim DLEntry As String = G_DI_Company.GetNewObjectKey.Trim.ToString
                            ActivityID = ActivityID + "," + DLEntry
                            Dim ErrMsg As String = ""
                            'qstr = "UPDATE OCLG SET AttendUser =NULL,AssigneeTy ='171',AttendEmpl =1 WHERE ClgCode ='" + DLEntry.ToString + "'"
                            qstr = "UPDATE ""OCLG"" SET ""AttendEmpl""='" + Item.AssignedEmployeeId + "',""AttendUser""=NULL,""AssigneeTy""=171 WHERE ""ClgCode"" ='" + DLEntry.ToString + "'"
                            rSet.DoQuery(qstr)

                        End If
                    Next

                Catch ex As Exception
                    If G_DI_Company.InTransaction Then
                        G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                    End If
                    G_DI_Company.Disconnect()
                    NewRow = dtTable.NewRow
                    NewRow.Item("ReturnCode") = "-2222"
                    NewRow.Item("ReturnDocEntry") = "-1"
                    NewRow.Item("ReturnObjType") = "-1"
                    NewRow.Item("ReturnSeries") = "-1"
                    NewRow.Item("ReturnDocNum") = "-1"
                    NewRow.Item("ReturnMsg") = ex.Message
                    dtTable.Rows.Add(NewRow)
                    Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                End Try
                G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit)
                G_DI_Company.Disconnect()
                NewRow = dtTable.NewRow
                NewRow.Item("ReturnCode") = "0000"
                NewRow.Item("ReturnDocEntry") = ActivityID
                NewRow.Item("ReturnObjType") = ActivityID
                NewRow.Item("ReturnSeries") = ""
                NewRow.Item("ReturnDocNum") = ActivityID
                NewRow.Item("ReturnMsg") = "Your Request No. " + ActivityID + " successfully submitted"
                dtTable.Rows.Add(NewRow)
                Return Request.CreateResponse(HttpStatusCode.OK, dtTable)

            Catch ex As Exception
                Try
                    G_DI_Company.Disconnect()
                Catch ex1 As Exception
                End Try
                NewRow = dtTable.NewRow
                NewRow.Item("ReturnCode") = "-2222"
                NewRow.Item("ReturnMsg") = ex.Message.ToString
                NewRow.Item("ReturnDocEntry") = "-1"
                NewRow.Item("ReturnObjType") = "-1"
                NewRow.Item("ReturnSeries") = "-1"
                NewRow.Item("ReturnDocNum") = "-1"
                dtTable.Rows.Add(NewRow)
                Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
            End Try
        End Function

        <Route("Api/UpdateActivity")>
        <HttpPost>
        Public Function UpdateActivity(ByVal Activity As DTS_MODEL_USER_ACTIVITYLISTUPDATE) As HttpResponseMessage
            Dim G_DI_Company As SAPbobsCOM.Company = Nothing
            Dim strRegnNo As String = ""
            dtTable.Columns.Add("ReturnCode")
            dtTable.Columns.Add("ReturnDocEntry")
            dtTable.Columns.Add("ReturnObjType")
            dtTable.Columns.Add("ReturnSeries")
            dtTable.Columns.Add("ReturnDocNum")
            dtTable.Columns.Add("ReturnMsg")
            Try
                Dim Branch As String = ""
                Dim UserID As String = ""


                If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                    Return Request.CreateResponse(HttpStatusCode.OK, dtError)
                End If
                Dim qstr As String = ""
                'Dim qstr As String = ""
                qstr = "SELECT ""U_SAPUNAME"" ,""U_SAPPWD"" " & vbNewLine &
                       " FROM ""OUSR"" A " & vbNewLine &
                       " WHERE A.""USER_CODE"" ='" + UserID + "'"
                Dim dRow As Data.DataRow
                dRow = DBSQL.getQueryDataRow(qstr, "")
                Dim SAPUserId As String = ""
                Dim SAPPassword As String = ""
                Try
                    SAPUserId = dRow.Item("U_SAPUNAME")
                    SAPPassword = dRow.Item("U_SAPPWD")
                Catch ex As Exception
                End Try
                Connection.ConnectSAP(G_DI_Company, SAPUserId, SAPPassword)
                rSet = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                'Dim oCompServ As SAPbobsCOM.CompanyService = Nothing
                'Dim oActivityServ As SAPbobsCOM.ActivitiesService = Nothing
                'Dim vNewActivity As SAPbobsCOM.Activity = Nothing
                'oCompServ = G_DI_Company.GetCompanyService
                'oActivityServ = oCompServ.GetBusinessService(SAPbobsCOM.ServiceTypes.ActivitiesService)
                'vNewActivity = oActivityServ.GetDataInterface(SAPbobsCOM.ActivitiesServiceDataInterfaces.asActivity)
                'vNewActivity.CardCode = "C0000003"
                'vNewActivity.HandledBy = dsOrders.Tables(0).Rows(iterOrders).Item("HandledBy")
                'oActivityServ.AddActivity(vNewActivity)
                Dim ActivityID As String = ""
                Try
                    G_DI_Company.StartTransaction()
                    For Each Item As DTS_MODEL_USER_ACTIVITYUPDATE In Activity.Activity
                        Dim vContact As SAPbobsCOM.Contacts = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oContacts)
                        'If vContact.GetByKey("1") Then
                        '    vContact.EndDuedate = New Date(Mid(PaymentDetails.RefDate, 1, 4), Mid(PaymentDetails.RefDate, 5, 2), Mid(PaymentDetails.RefDate, 7, 2))
                        '    vContact.EndTime = New DateTime(Mid(PaymentDetails.RefDate, 1, 4), Mid(PaymentDetails.RefDate, 5, 2), Mid(PaymentDetails.RefDate, 7, 2), "21", "03", "00")
                        '    vContact.Closed = SAPbobsCOM.BoYesNoEnum.tYES

                        'Else

                        'End If
                        If vContact.GetByKey(Item.ActivityId) Then
                            If Item.ToTime <> "" Then
                                vContact.EndDuedate = New Date(Mid(Activity.ActivityDate, 1, 4), Mid(Activity.ActivityDate, 5, 2), Mid(Activity.ActivityDate, 7, 2))
                                vContact.EndTime = New DateTime(Mid(Activity.ActivityDate, 1, 4), Mid(Activity.ActivityDate, 5, 2), Mid(Activity.ActivityDate, 7, 2), Item.ToTime.ToString.Split(":")(0), Item.ToTime.ToString.Split(":")(0), "00")
                            End If

                            vContact.UserFields.Fields.Item("U_DOSTATUS").Value = Item.DailyOperationStatus
                            vContact.UserFields.Fields.Item("U_UPDTBY").Value = UserID
                            Dim lRetCode As Integer = vContact.Update()
                            Dim ErrorMessage As String = ""
                            If lRetCode <> 0 Then
                                Dim sErrMsg As String = G_DI_Company.GetLastErrorDescription()
                                G_DI_Company.GetLastError(lRetCode, sErrMsg)
                                ErrorMessage = sErrMsg
                                System.Runtime.InteropServices.Marshal.ReleaseComObject(vContact)
                                If G_DI_Company.InTransaction Then
                                    G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                                End If
                                G_DI_Company.Disconnect()
                                NewRow = dtTable.NewRow
                                NewRow.Item("ReturnCode") = "-2222"
                                NewRow.Item("ReturnDocEntry") = "-1"
                                NewRow.Item("ReturnObjType") = "-1"
                                NewRow.Item("ReturnSeries") = "-1"
                                NewRow.Item("ReturnDocNum") = "-1"
                                NewRow.Item("ReturnMsg") = ErrorMessage
                                dtTable.Rows.Add(NewRow)
                                Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                            Else
                                System.Runtime.InteropServices.Marshal.ReleaseComObject(vContact)
                                Dim ObjType As String = G_DI_Company.GetNewObjectType.ToString.TrimEnd
                                Dim DLEntry As String = G_DI_Company.GetNewObjectKey.Trim.ToString
                                ActivityID = ActivityID + "," + DLEntry


                            End If
                        Else
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(vContact)
                            If G_DI_Company.InTransaction Then
                                G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                            End If
                            G_DI_Company.Disconnect()
                            NewRow = dtTable.NewRow
                            NewRow.Item("ReturnCode") = "-2222"
                            NewRow.Item("ReturnDocEntry") = "-1"
                            NewRow.Item("ReturnObjType") = "-1"
                            NewRow.Item("ReturnSeries") = "-1"
                            NewRow.Item("ReturnDocNum") = "-1"
                            NewRow.Item("ReturnMsg") = "Activity not Found"
                            dtTable.Rows.Add(NewRow)
                            Return Request.CreateResponse(HttpStatusCode.OK, dtTable)

                        End If


                    Next

                Catch ex As Exception
                    If G_DI_Company.InTransaction Then
                        G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                    End If
                End Try
                G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit)
                G_DI_Company.Disconnect()
                NewRow = dtTable.NewRow
                NewRow.Item("ReturnCode") = "0000"
                NewRow.Item("ReturnDocEntry") = ActivityID
                NewRow.Item("ReturnObjType") = ActivityID
                NewRow.Item("ReturnSeries") = ""
                NewRow.Item("ReturnDocNum") = ActivityID
                NewRow.Item("ReturnMsg") = "Your Request No. " + ActivityID + " successfully submitted"
                dtTable.Rows.Add(NewRow)
                Return Request.CreateResponse(HttpStatusCode.OK, dtTable)

            Catch ex As Exception
                Try
                    G_DI_Company.Disconnect()
                Catch ex1 As Exception
                End Try
                NewRow = dtTable.NewRow
                NewRow.Item("ReturnCode") = "-2222"
                NewRow.Item("ReturnMsg") = ex.Message.ToString
                NewRow.Item("ReturnDocEntry") = "-1"
                NewRow.Item("ReturnObjType") = "-1"
                NewRow.Item("ReturnSeries") = "-1"
                NewRow.Item("ReturnDocNum") = "-1"
                dtTable.Rows.Add(NewRow)
                Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
            End Try
        End Function

        <Route("Api/PostDayClosing")>
        <HttpPost>
        Public Function PostDayClosing(ByVal DayClose As DTS_MODEL_DAYCLOSE_HEADER) As HttpResponseMessage
            Dim G_DI_Company As SAPbobsCOM.Company = Nothing
            Dim strRegnNo As String = ""
            dtTable.Columns.Add("ReturnCode")
            dtTable.Columns.Add("ReturnDocEntry")
            dtTable.Columns.Add("ReturnObjType")
            dtTable.Columns.Add("ReturnSeries")
            dtTable.Columns.Add("ReturnDocNum")
            dtTable.Columns.Add("ReturnMsg")
            Try
                Dim Branch As String = ""
                Dim UserID As String = ""


                If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                    Return Request.CreateResponse(HttpStatusCode.OK, dtError)
                End If
                Dim qstr As String = ""
                'Dim qstr As String = ""
                qstr = "SELECT ""U_SAPUNAME"" ,""U_SAPPWD"" " & vbNewLine &
                       " FROM ""OUSR"" A " & vbNewLine &
                       " WHERE A.""USER_CODE"" ='" + UserID + "'"
                Dim dRow As Data.DataRow
                dRow = DBSQL.getQueryDataRow(qstr, "")
                Dim SAPUserId As String = ""
                Dim SAPPassword As String = ""
                Try
                    SAPUserId = dRow.Item("U_SAPUNAME")
                    SAPPassword = dRow.Item("U_SAPPWD")
                Catch ex As Exception
                End Try
                Connection.ConnectSAP(G_DI_Company, SAPUserId, SAPPassword)

                rSet = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                Dim oGeneralService As SAPbobsCOM.GeneralService
                Dim oGeneralData As SAPbobsCOM.GeneralData
                Dim oChild As SAPbobsCOM.GeneralData
                Dim oChildren As SAPbobsCOM.GeneralDataCollection
                Dim oGeneralParams As SAPbobsCOM.GeneralDataParams
                Dim oCompService As SAPbobsCOM.CompanyService = G_DI_Company.GetCompanyService()
                oGeneralService = oCompService.GetGeneralService("DTS_UDO_MD_DAYCLOSE")
                oGeneralData = oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData)
                'Dim qstr As String

                'New Date(Mid(Date.Now.ToString("yyyyMMdd"), 1, 4), Mid(Date.Now.ToString("yyyyMMdd"), 5, 2), Mid(Date.Now.ToString("yyyyMMdd"), 7, 2))
                oGeneralData.SetProperty("Code", DayClose.PostingDate + "/" + Branch)
                oGeneralData.SetProperty("U_DATE", New Date(Mid(DayClose.PostingDate, 1, 4), Mid(DayClose.PostingDate, 5, 2), Mid(DayClose.PostingDate, 7, 2)))
                oGeneralData.SetProperty("U_DAYCLOSE", "Y")
                oGeneralData.SetProperty("U_CRTDBY", UserID)
                oGeneralData.SetProperty("U_BUNIT", Branch)
                oGeneralData.SetProperty("U_CASHAMT", DayClose.CashAmount.ToString)
                oGeneralData.SetProperty("U_OTHAMT", DayClose.OthersAmount.ToString)
                oGeneralData.SetProperty("U_EXTRCASH", DayClose.ExtraCashAmount.ToString)
                oGeneralData.SetProperty("U_RMKS", DayClose.Remarks.ToString)
                For Each Item As DTS_MODEL_DAYCLOSE_ITEMS In DayClose.Items
                    oChildren = oGeneralData.Child("DTS_MR_DAYCLOSE")
                    oChild = oChildren.Add()
                    'oChild.SetProperty("VisOrder", VisOrder)

                    oChild.SetProperty("U_SLNO", Item.SerialNo.ToString)
                    oChild.SetProperty("U_CURRNCY", Item.Currency.ToString)
                    oChild.SetProperty("U_TYPECODE", Item.TypeCode.ToString)
                    oChild.SetProperty("U_NO", Item.No.ToString)
                    oChild.SetProperty("U_TOTAMT", Item.TotalAmount.ToString)
                    oChild.SetProperty("U_RMKS", IIf(Item.Remarks Is Nothing, "", Item.Remarks))
                Next
                Dim lrtCode = oGeneralService.Add(oGeneralData)

                'Dim g = oGeneralService.GetDataInterface("DocEntry")
                oGeneralParams = oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralDataParams)
                oGeneralParams.SetProperty("Code", lrtCode.GetProperty("Code"))
                oGeneralData = oGeneralService.GetByParams(oGeneralParams)

                G_DI_Company.Disconnect()
                NewRow = dtTable.NewRow
                NewRow.Item("ReturnCode") = "0000"
                NewRow.Item("ReturnDocEntry") = lrtCode.GetProperty("Code")
                NewRow.Item("ReturnObjType") = lrtCode.GetProperty("Code")
                NewRow.Item("ReturnSeries") = lrtCode.GetProperty("Code")
                NewRow.Item("ReturnDocNum") = lrtCode.GetProperty("Code")
                NewRow.Item("ReturnMsg") = "Your Request No. " + lrtCode.GetProperty("Code") + " successfully submitted"
                dtTable.Rows.Add(NewRow)
                Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
            Catch ex As Exception
                Try
                    G_DI_Company.Disconnect()
                Catch ex1 As Exception
                End Try
                NewRow = dtTable.NewRow
                NewRow.Item("ReturnCode") = "-2222"
                NewRow.Item("ReturnMsg") = ex.Message.ToString
                NewRow.Item("ReturnDocEntry") = "-1"
                NewRow.Item("ReturnObjType") = "-1"
                NewRow.Item("ReturnSeries") = "-1"
                NewRow.Item("ReturnDocNum") = "-1"
                dtTable.Rows.Add(NewRow)
                Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
            End Try
        End Function

        <Route("Api/UpdateSalesOrder")>
        <HttpPost>
        Public Function UpdateSalesOrder(ByVal SoUpdate As DTS_SOUPDATE) As HttpResponseMessage
            Dim G_DI_Company As SAPbobsCOM.Company = Nothing
            Dim strRegnNo As String = ""
            dtTable.Columns.Add("ReturnCode")
            dtTable.Columns.Add("ReturnDocEntry")
            dtTable.Columns.Add("ReturnObjType")
            dtTable.Columns.Add("ReturnSeries")
            dtTable.Columns.Add("ReturnDocNum")
            dtTable.Columns.Add("ReturnMsg")
            Try
                Dim Branch As String = ""
                Dim UserID As String = ""


                If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                    Return Request.CreateResponse(HttpStatusCode.OK, dtError)
                End If
                Dim qstr As String = ""
                'Dim qstr As String = ""
                qstr = "SELECT ""U_SAPUNAME"" ,""U_SAPPWD"" " & vbNewLine &
                       " FROM ""OUSR"" A " & vbNewLine &
                       " WHERE A.""USER_CODE"" ='" + UserID + "'"
                Dim dRow As Data.DataRow
                dRow = DBSQL.getQueryDataRow(qstr, "")
                Dim SAPUserId As String = ""
                Dim SAPPassword As String = ""
                Try
                    SAPUserId = dRow.Item("U_SAPUNAME")
                    SAPPassword = dRow.Item("U_SAPPWD")
                Catch ex As Exception
                End Try
                Connection.ConnectSAP(G_DI_Company, SAPUserId, SAPPassword)
                rSet = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)

                Dim DocNum As String = ""
                Dim SoEntry As String = SoUpdate.DocEntry
                Dim ObjType As String = ""
                Dim SeriesName As String = ""
                Dim ReturnDocNo As String = ""
                Try
                    ' G_DI_Company.StartTransaction()
                    Dim SalesOrder As SAPbobsCOM.Documents = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oOrders)
                    'SoEntry = SalesOrder.DocEntry
                    If SalesOrder.GetByKey(SoEntry) Then
                        SalesOrder.UserFields.Fields.Item("U_TOBUNIT").Value = SoUpdate.ToBranch
                        SalesOrder.Comments = SoUpdate.Remarks
                        SalesOrder.UserFields.Fields.Item("U_FRMPORT").Value = "Y"
                    End If
                    Dim lRetCode As Integer = SalesOrder.Update()
                    Dim ErrorMessage As String = ""
                    If lRetCode <> 0 Then
                        Dim sErrMsg As String = G_DI_Company.GetLastErrorDescription()
                        G_DI_Company.GetLastError(lRetCode, sErrMsg)
                        ErrorMessage = sErrMsg
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(SalesOrder)
                        If G_DI_Company.InTransaction Then
                            G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                        End If
                        G_DI_Company.Disconnect()
                        NewRow = dtTable.NewRow
                        NewRow.Item("ReturnCode") = "-2222"
                        NewRow.Item("ReturnDocEntry") = "-1"
                        NewRow.Item("ReturnObjType") = "-1"
                        NewRow.Item("ReturnSeries") = "-1"
                        NewRow.Item("ReturnDocNum") = "-1"
                        NewRow.Item("ReturnMsg") = ErrorMessage
                        dtTable.Rows.Add(NewRow)
                        Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                    Else
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(SalesOrder)
                        ObjType = G_DI_Company.GetNewObjectType.ToString.TrimEnd
                        qstr = "SELECT B.""SeriesName"",CAST(A.""DocNum"" AS VARCHAR) ""DocNum"",D.""WhsCode"" ""WhsCode"",E.""WhsCode"" ""WIPWhsCode"",B.""SeriesName""||'/'||CAST(A.""DocNum"" AS VARCHAR) ""StrDocNum"" " & vbNewLine &
                               " From ""ORDR"" A " & vbNewLine &
                               " INNER JOIN ""NNM1"" B ON B.""ObjectCode""=A.""ObjType"" AND B.""Series""=A.""Series"" " & vbNewLine &
                               "       LEFT OUTER Join ""OWHS"" D On D.""U_BUSUNIT""='" + SoUpdate.ToBranch + "' AND D.""U_WHSTYPE""='N' " & vbNewLine &
                               "       LEFT OUTER Join ""OWHS"" E On E.""U_BUSUNIT""='" + SoUpdate.ToBranch + "' AND E.""U_WHSTYPE""='W' " & vbNewLine &
                               " WHERE A.""DocEntry""='" + SoEntry + "' "
                        rSet.DoQuery(qstr)
                        Dim WIPWhscode As String = rSet.Fields.Item("WIPWhsCode").Value
                        Dim Whscode As String = rSet.Fields.Item("WhsCode").Value
                        ReturnDocNo = rSet.Fields.Item("StrDocNum").Value
                        SeriesName = rSet.Fields.Item("SeriesName").Value
                        DocNum = rSet.Fields.Item("DocNum").Value
                        qstr = "UPDATE ""RDR1"" SET ""WhsCode""='" + WIPWhscode + "' WHERE ""DocEntry""='" + SoEntry + "' and ""U_ITEMHIDE""='Y' and ""LineStatus""='O' "
                        rSet.DoQuery(qstr)
                        qstr = "UPDATE ""RDR1"" SET ""WhsCode""='" + Whscode + "' WHERE ""DocEntry""='" + SoEntry + "' and ""U_ITEMHIDE""='N' and ""LineStatus""='O' "
                        rSet.DoQuery(qstr)
                    End If
                Catch ex As Exception
                    If G_DI_Company.InTransaction Then
                        G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                    End If
                End Try
                'G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit)
                G_DI_Company.Disconnect()
                NewRow = dtTable.NewRow
                NewRow.Item("ReturnCode") = "0000"
                NewRow.Item("ReturnDocEntry") = SoEntry
                NewRow.Item("ReturnObjType") = ObjType
                NewRow.Item("ReturnSeries") = SeriesName
                NewRow.Item("ReturnDocNum") = DocNum
                NewRow.Item("ReturnMsg") = "Your Request No. " + ReturnDocNo + " successfully updated"
                dtTable.Rows.Add(NewRow)
                Return Request.CreateResponse(HttpStatusCode.OK, dtTable)

            Catch ex As Exception
                Try
                    G_DI_Company.Disconnect()
                Catch ex1 As Exception
                End Try
                NewRow = dtTable.NewRow
                NewRow.Item("ReturnCode") = "-2222"
                NewRow.Item("ReturnMsg") = ex.Message.ToString
                NewRow.Item("ReturnDocEntry") = "-1"
                NewRow.Item("ReturnObjType") = "-1"
                NewRow.Item("ReturnSeries") = "-1"
                NewRow.Item("ReturnDocNum") = "-1"
                dtTable.Rows.Add(NewRow)
                Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
            End Try
        End Function

        <Route("Api/PostDownPaymentPrepaid")>
        <HttpPost>
        Public Function PostDownPaymentPrepaid(ByVal DownPayDetails As DTS_DWNPYMT_HEADER) As HttpResponseMessage
            Dim G_DI_Company As SAPbobsCOM.Company = Nothing
            Dim strRegnNo As String = ""
            dtTable.Columns.Add("ReturnCode")
            dtTable.Columns.Add("ReturnDocEntry")
            dtTable.Columns.Add("ReturnObjType")
            dtTable.Columns.Add("ReturnSeries")
            dtTable.Columns.Add("ReturnDocNum")
            dtTable.Columns.Add("ReturnMsg")
            Try
                Dim Branch As String = ""
                Dim UserID As String = ""


                If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                    Return Request.CreateResponse(HttpStatusCode.OK, dtError)
                End If
                Dim qstr As String = ""
                'Dim qstr As String = ""
                qstr = "SELECT ""U_SAPUNAME"" ,""U_SAPPWD"" " & vbNewLine &
                       " FROM ""OUSR"" A " & vbNewLine &
                       " WHERE A.""USER_CODE"" ='" + UserID + "'"
                Dim dRow As Data.DataRow
                dRow = DBSQL.getQueryDataRow(qstr, "")
                Dim SAPUserId As String = ""
                Dim SAPPassword As String = ""
                Try
                    SAPUserId = dRow.Item("U_SAPUNAME")
                    SAPPassword = dRow.Item("U_SAPPWD")
                Catch ex As Exception
                End Try
                Connection.ConnectSAP(G_DI_Company, SAPUserId, SAPPassword)
                rSet = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                qstr = "Select Top 1 N.""Series"",B.""BPLId"",C.""WhsCode"" ""WhsCode"" ,TO_CHAR(NOW(),'YYYYYMMDD') ""DocDate"" " & vbNewLine &
                                "  From ""NNM1"" N  " & vbNewLine &
                                "       Inner Join ""OFPR"" O On O.""Indicator""=N.""Indicator"" " & vbNewLine &
                                "       Inner Join ""OBPL"" B On B.""BPLId""=N.""BPLId"" " & vbNewLine &
                                "       Inner Join ""OWHS"" C On C.""U_BUSUNIT""='" + Branch + "' AND C.""U_WHSTYPE""='N' " & vbNewLine &
                                " Where N.""ObjectCode""='203' " & vbNewLine &
                                "   And O.""PeriodStat"" In ('N','C') And N.""Locked""='N' AND N.""Remark""='" + Branch + "'  " & vbNewLine &
                                "   And TO_CHAR('" + DownPayDetails.PostingDate + "','YYYYYMMDD') Between TO_CHAR(O.""F_RefDate"",'YYYYYMMDD') And TO_CHAR(O.""T_RefDate"",'YYYYYMMDD')"
                rSet.DoQuery(qstr)


                If rSet.RecordCount > 0 Then
                    G_DI_Company.StartTransaction()
                    Dim ARDownDraft As SAPbobsCOM.Documents = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDownPayments)
                    ARDownDraft.DocObjectCode = SAPbobsCOM.BoObjectTypes.oDownPayments
                    ARDownDraft.CardCode = DownPayDetails.CardCode
                    ARDownDraft.BPL_IDAssignedToInvoice = rSet.Fields.Item("BPLId").Value
                    ARDownDraft.Series = rSet.Fields.Item("Series").Value
                    ARDownDraft.Comments = "Prepaid Rechadge @" + Date.Now.ToString("dd/MM/yyyy")
                    ARDownDraft.DocDate = New Date(Mid(DownPayDetails.PostingDate, 1, 4), Mid(DownPayDetails.PostingDate, 5, 2), Mid(DownPayDetails.PostingDate, 7, 2))
                    ARDownDraft.TaxDate = New Date(Mid(DownPayDetails.PostingDate, 1, 4), Mid(DownPayDetails.PostingDate, 5, 2), Mid(DownPayDetails.PostingDate, 7, 2))
                    ARDownDraft.DownPaymentType = SAPbobsCOM.DownPaymentTypeEnum.dptRequest
                    ARDownDraft.NumAtCard = "Prepaid Recharge"
                    ARDownDraft.UserFields.Fields.Item("U_BRANCH").Value = Branch
                    ARDownDraft.DocTotal = DownPayDetails.FullAmount
                    ARDownDraft.Lines.ItemCode = "DOWNPayment"
                    ARDownDraft.Lines.Quantity = 1
                    ARDownDraft.Lines.UnitPrice = DownPayDetails.FullAmount


                    Dim lRetCode As Integer

                    lRetCode = ARDownDraft.Add
                    Dim ErrorMessage As String = ""
                    If lRetCode <> 0 Then
                        Dim sErrMsg As String = G_DI_Company.GetLastErrorDescription()
                        G_DI_Company.GetLastError(lRetCode, sErrMsg)
                        ErrorMessage = sErrMsg
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(ARDownDraft)
                        If G_DI_Company.InTransaction Then
                            G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                        End If
                        G_DI_Company.Disconnect()
                        NewRow = dtTable.NewRow
                        NewRow.Item("ReturnCode") = "-2222"
                        NewRow.Item("ReturnDocEntry") = "-1"
                        NewRow.Item("ReturnObjType") = "-1"
                        NewRow.Item("ReturnSeries") = "-1"
                        NewRow.Item("ReturnDocNum") = "-1"
                        NewRow.Item("ReturnMsg") = ErrorMessage
                        dtTable.Rows.Add(NewRow)
                        Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                    Else
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(ARDownDraft)
                        Dim ObjType As String = G_DI_Company.GetNewObjectType.ToString.TrimEnd
                        Dim DLEntry As String = G_DI_Company.GetNewObjectKey.Trim.ToString
                        Dim ErrMsg As String = ""

                        If ObjType = "112" Then
                            qstr = "SELECT B.""SeriesName"",CAST(A.""DocNum"" AS VARCHAR) ""DocNum"",B.""SeriesName""||'/'||CAST(A.""DocNum"" AS VARCHAR) ""StrDocNum"" " & vbNewLine &
                               " From ""ODRF"" A " & vbNewLine &
                               " INNER JOIN ""NNM1"" B ON B.""ObjectCode""=A.""ObjType"" AND B.""Series""=A.""Series"" " & vbNewLine &
                               " WHERE A.""DocEntry""='" + DLEntry + "' "
                        ElseIf ObjType = "203" Then
                            qstr = "SELECT A.""DocTotal"",B.""SeriesName"",CAST(A.""DocNum"" AS VARCHAR) ""DocNum"",B.""SeriesName""||'/'||CAST(A.""DocNum"" AS VARCHAR) ""StrDocNum"" " & vbNewLine &
                               " From ""ODPI"" A " & vbNewLine &
                               " INNER JOIN ""NNM1"" B ON B.""ObjectCode""=A.""ObjType"" AND B.""Series""=A.""Series"" " & vbNewLine &
                               " WHERE A.""DocEntry""='" + DLEntry + "' "
                        End If
                        rSet.DoQuery(qstr)
                        Dim ReturnDocNo = rSet.Fields.Item("StrDocNum").Value
                        Dim Series = rSet.Fields.Item("SeriesName").Value
                        Dim DocNum = rSet.Fields.Item("DocNum").Value
                        If CType(rSet.Fields.Item("DocTotal").Value, Double) > 0 Then
                            Try
                                Dim IncomingPAymentEntry As String = ""
                                Dim ermsg As String = ""
                                If DPIncomingPaymentAdd(G_DI_Company, DownPayDetails, DLEntry, UserID, Branch, IncomingPAymentEntry, ermsg) = False Then
                                    If G_DI_Company.InTransaction Then
                                        G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                                    End If

                                    G_DI_Company.Disconnect()
                                    NewRow = dtTable.NewRow
                                    NewRow.Item("ReturnCode") = "-3333"
                                    NewRow.Item("ReturnDocEntry") = "-1"
                                    NewRow.Item("ReturnObjType") = "-1"
                                    NewRow.Item("ReturnSeries") = "-1"
                                    NewRow.Item("ReturnDocNum") = "-1"
                                    NewRow.Item("ReturnMsg") = ermsg
                                    dtTable.Rows.Add(NewRow)
                                    Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                                End If
                            Catch ex As Exception
                                If G_DI_Company.InTransaction Then
                                    G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                                End If

                                G_DI_Company.Disconnect()
                                NewRow = dtTable.NewRow
                                NewRow.Item("ReturnCode") = "-2222"
                                NewRow.Item("ReturnDocEntry") = "-1"
                                NewRow.Item("ReturnObjType") = "-1"
                                NewRow.Item("ReturnSeries") = "-1"
                                NewRow.Item("ReturnDocNum") = "-1"
                                NewRow.Item("ReturnMsg") = ex.Message
                                dtTable.Rows.Add(NewRow)
                                Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                            End Try
                        End If

                        G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit)
                        G_DI_Company.Disconnect()
                        NewRow = dtTable.NewRow
                        NewRow.Item("ReturnCode") = "0000"
                        NewRow.Item("ReturnDocEntry") = DLEntry
                        NewRow.Item("ReturnObjType") = ObjType
                        NewRow.Item("ReturnSeries") = Series
                        NewRow.Item("ReturnDocNum") = DocNum
                        NewRow.Item("ReturnMsg") = "Prepaid Card " + ReturnDocNo + " successfully added"
                        dtTable.Rows.Add(NewRow)
                        Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                    End If
                Else
                    If G_DI_Company.InTransaction Then
                        G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                    End If
                    G_DI_Company.Disconnect()
                    NewRow = dtTable.NewRow
                    NewRow.Item("ReturnCode") = "-2222"
                    NewRow.Item("ReturnDocEntry") = "-1"
                    NewRow.Item("ReturnObjType") = "-1"
                    NewRow.Item("ReturnSeries") = "-1"
                    NewRow.Item("ReturnDocNum") = "-1"
                    NewRow.Item("ReturnMsg") = "Series not found"
                    dtTable.Rows.Add(NewRow)
                    Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                End If

            Catch ex As Exception
                Try
                    If G_DI_Company.InTransaction Then
                        G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                    End If
                Catch ex2 As Exception
                End Try
                Try
                    G_DI_Company.Disconnect()
                Catch ex1 As Exception
                End Try
                NewRow = dtTable.NewRow
                NewRow.Item("ReturnCode") = "-2222"
                NewRow.Item("ReturnMsg") = ex.Message.ToString
                NewRow.Item("ReturnDocEntry") = "-1"
                NewRow.Item("ReturnObjType") = "-1"
                NewRow.Item("ReturnSeries") = "-1"
                NewRow.Item("ReturnDocNum") = "-1"
                dtTable.Rows.Add(NewRow)
                Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
            End Try
        End Function

        Public Function DPIncomingPaymentAdd(ByVal G_DI_Company As SAPbobsCOM.Company, ByVal DPMDetails As DTS_DWNPYMT_HEADER, ByVal DpmEntry As String, ByVal UserId As String, ByVal Branch As String,
                               ByRef InComingPayEntry As String, ByRef erMessage As String) As Boolean
            Try
                rSet = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                Dim qstr As String
                qstr = "Select Top 1 N.""Series"",B.""BPLId"",C.""WhsCode"" ""WhsCode"" ,TO_CHAR(NOW(),'YYYYYMMDD') ""DocDate"" " & vbNewLine &
                                "  From ""NNM1"" N  " & vbNewLine &
                                "       Inner Join ""OFPR"" O On O.""Indicator""=N.""Indicator"" " & vbNewLine &
                                "       Inner Join ""OBPL"" B On B.""BPLId""=N.""BPLId"" " & vbNewLine &
                                "       Inner Join ""OWHS"" C On C.""U_BUSUNIT""='" + Branch + "' AND C.""U_WHSTYPE""='N' " & vbNewLine &
                                " Where N.""ObjectCode""='24' " & vbNewLine &
                                "   And O.""PeriodStat"" In ('N','C') And N.""Locked""='N' AND N.""Remark""='" + Branch + "'  " & vbNewLine &
                                "   And TO_CHAR('" + DPMDetails.PostingDate + "','YYYYYMMDD') Between TO_CHAR(O.""F_RefDate"",'YYYYYMMDD') And TO_CHAR(O.""T_RefDate"",'YYYYYMMDD')"

                Dim IncPayment As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                IncPayment.DoQuery(qstr)
                If IncPayment.RecordCount > 0 Then
                Else
                    erMessage = "Incoimng Payment Series not Found"
                    Return False
                End If
                Dim InPay As SAPbobsCOM.Payments = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oIncomingPayments)

                InPay.CardCode = DPMDetails.CardCode
                'InPay.DocDate = New Date(Mid(getServerDate, 1, 4), Mid(getServerDate, 5, 2), Mid(getServerDate, 7, 2))
                InPay.BPLID = IncPayment.Fields.Item("BPLId").Value
                InPay.UserFields.Fields.Item("U_BRANCH").Value = Branch
                InPay.UserFields.Fields.Item("U_CRTDBY").Value = UserId
                InPay.DocDate = New Date(Mid(DPMDetails.PostingDate, 1, 4), Mid(DPMDetails.PostingDate, 5, 2), Mid(DPMDetails.PostingDate, 7, 2))
                InPay.Series = IncPayment.Fields.Item("Series").Value
                InPay.Invoices.DocEntry = Convert.ToInt32(DpmEntry)
                InPay.Invoices.InvoiceType = SAPbobsCOM.BoRcptInvTypes.it_DownPayment
                InPay.Invoices.DistributionRule = Branch
                InPay.Invoices.Add()
                Dim ARDown As SAPbobsCOM.Documents = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDownPayments)
                ARDown = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDownPayments)
                ARDown.DocObjectCode = SAPbobsCOM.BoObjectTypes.oDownPayments
                ARDown.GetByKey(Convert.ToInt32(DpmEntry))
                For Each Item As DTS_MODEL_PMNT_DTLS In DPMDetails.PaymentDetails
                    If Item.PaymentType = "S" Then
                        InPay.CashSum = Item.Amount
                        qstr = "SELECT ""OverCode"",""FrgnName"",""AcctCode"" FROM OACT  WHERE ""OverCode""='" + Branch + "' AND ""FrgnName""='S'"
                        Dim CashPayment As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                        CashPayment.DoQuery(qstr)
                        InPay.CashAccount = CashPayment.Fields.Item("AcctCode").Value
                    End If
                    If Item.PaymentType = "S" Or Item.PaymentType = "P" Then
                    Else
                        'qstr = "SELECT ""OverCode"",""FrgnName"",""AcctCode"" FROM OACT  WHERE ""OverCode""='" + Branch + "' AND ""FrgnName""='C'"
                        qstr = "SELECT ""CreditCard""  FROM ""OCRC"" where ""U_BANKCODE""='" + Item.Bank + "' AND ""U_PMNTP""='" + Item.PaymentType + "'"
                        Dim CreditPayment As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                        CreditPayment.DoQuery(qstr)
                        If CreditPayment.RecordCount > 0 Then
                            InPay.CreditCards.CreditCard = CreditPayment.Fields.Item("CreditCard").Value
                            Try
                                InPay.CreditCards.CreditCardNumber = IIf(Item.CardNo.ToString = "", "1111", Right(Item.CardNo, 4))
                                InPay.CreditCards.UserFields.Fields.Item("U_CARDNO").Value = IIf(Item.CardNo.ToString = "", "1111", Item.CardNo.ToString)
                            Catch ex As Exception
                                InPay.CreditCards.CreditCardNumber = "1111"
                                InPay.CreditCards.UserFields.Fields.Item("U_CARDNO").Value = "1111"
                            End Try
                            InPay.CreditCards.CardValidUntil = New Date(Mid("29991231", 1, 4), Mid("29991231", 5, 2), Mid("29991231", 7, 2))
                            InPay.CreditCards.PaymentMethodCode = 1
                            InPay.CreditCards.CreditSum = Item.Amount
                            'InPay.CreditCards.FirstPaymentSum = CreditCards.CardAmount
                            Try
                                InPay.CreditCards.VoucherNum = IIf(Item.Tranid.ToString = "", "111", Item.Tranid.ToString)
                            Catch ex As Exception
                                InPay.CreditCards.VoucherNum = "111"
                            End Try

                            InPay.CreditCards.CreditType = SAPbobsCOM.BoRcptCredTypes.cr_Regular
                            InPay.CreditCards.Add()
                        Else
                            erMessage = "No Payment Method found for " + Item.Bank + " and " + Item.PaymentType
                            Return False
                        End If

                        'InPay.TransferAccount = CreditPayment.Fields.Item("AcctCode").Value
                        'InPay.TransferReference = Item.Tranid.ToString
                        'InPay.UserFields.Fields.Item("U_CRDTRNID").Value = Item.Tranid.ToString
                        'InPay.UserFields.Fields.Item("U_CRDONAME").Value = Item.CardHolderName.ToString
                        'InPay.UserFields.Fields.Item("U_CRDCNO").Value = Item.CardNo.ToString
                        'InPay.TransferDate = New Date(Mid(Date.Now.ToString("yyyyMMdd"), 1, 4), Mid(Date.Now.ToString("yyyyMMdd"), 5, 2), Mid(Date.Now.ToString("yyyyMMdd"), 7, 2))
                        'InPay.TransferSum = Item.Amount
                    End If
                    'If Item.PaymentType = "U" Then
                    '    qstr = "SELECT ""OverCode"",""FrgnName"",""AcctCode"" FROM OACT  WHERE ""OverCode""='" + Branch + "' AND ""FrgnName""='U'"
                    '    Dim UPIPayment As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                    '    UPIPayment.DoQuery(qstr)
                    '    InPay.CheckAccount = UPIPayment.Fields.Item("AcctCode").Value
                    '    InPay.Checks.CountryCode = "BD"
                    '    InPay.Checks.CheckSum = Item.Amount
                    '    InPay.Checks.UserFields.Fields.Item("U_TRNSID").Value = Item.Tranid.ToString
                    '    InPay.Checks.DueDate = New Date(Mid(Date.Now.ToString("yyyyMMdd"), 1, 4), Mid(Date.Now.ToString("yyyyMMdd"), 5, 2), Mid(Date.Now.ToString("yyyyMMdd"), 7, 2))
                    '    qstr = "SELECT A.""BankCode"" " & vbNewLine &
                    '               " FROM ""ODSC"" A " & vbNewLine &
                    '               "    INNER JOIN ""CUFD"" B ON B.""TableID""='ODSC' AND B.""AliasID""='TYPE' " & vbNewLine &
                    '               "    INNER JOIN ""UFD1"" C ON C.""TableID""=B.""TableID"" AND B.""FieldID""=C.""FieldID"" AND A.""U_TYPE""=C.""FldValue"" " & vbNewLine &
                    '           " WHERE A.""U_TYPE""='" + Item.UpiName + "' "
                    '    Dim UPIBankPayment As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                    '    UPIBankPayment.DoQuery(qstr)
                    '    InPay.Checks.BankCode = UPIBankPayment.Fields.Item("BankCode").Value
                    '    InPay.Checks.CheckNumber = 1
                    '    'InPay.Checks.
                    '    InPay.Checks.Add()
                    'End If
                Next
                Dim lRetCode As Integer = InPay.Add
                If lRetCode <> 0 Then
                    Dim sErrMsg As String = G_DI_Company.GetLastErrorDescription()
                    G_DI_Company.GetLastError(lRetCode, sErrMsg)
                    erMessage = sErrMsg
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(InPay)
                    Return False
                End If
                Return True
            Catch ex As Exception
                erMessage = ex.Message
                Return False
            End Try
        End Function

        <Route("Api/UpdateStockTransferECommerce")>
        <HttpPost>
        Public Function UpdateStockTransferECommerce(ByVal TransUpdate As DTS_MODEL_STTRANSECOM_UPDATE) As HttpResponseMessage
            Dim G_DI_Company As SAPbobsCOM.Company = Nothing
            Dim strRegnNo As String = ""
            dtTable.Columns.Add("ReturnCode")
            dtTable.Columns.Add("ReturnDocEntry")
            dtTable.Columns.Add("ReturnObjType")
            dtTable.Columns.Add("ReturnSeries")
            dtTable.Columns.Add("ReturnDocNum")
            dtTable.Columns.Add("ReturnMsg")
            Try
                Dim Branch As String = ""
                Dim UserID As String = ""


                If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                    Return Request.CreateResponse(HttpStatusCode.OK, dtError)
                End If
                Dim qstr As String = ""
                'Dim qstr As String = ""
                qstr = "SELECT ""U_SAPUNAME"" ,""U_SAPPWD"" " & vbNewLine &
                       " FROM ""OUSR"" A " & vbNewLine &
                       " WHERE A.""USER_CODE"" ='" + UserID + "'"
                Dim dRow As Data.DataRow
                dRow = DBSQL.getQueryDataRow(qstr, "")
                Dim SAPUserId As String = ""
                Dim SAPPassword As String = ""
                Try
                    SAPUserId = dRow.Item("U_SAPUNAME")
                    SAPPassword = dRow.Item("U_SAPPWD")
                Catch ex As Exception
                End Try
                Connection.ConnectSAP(G_DI_Company, SAPUserId, SAPPassword)
                rSet = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)

                Dim DocNum As String = ""
                Dim SoEntry As String = TransUpdate.DocEntry
                Dim ObjType As String = ""
                Dim SeriesName As String = ""
                Dim ReturnDocNo As String = ""
                Dim ConfDate As String = TransUpdate.ConfDate
                Try
                    ' G_DI_Company.StartTransaction()
                    'Dim StockTrans As SAPbobsCOM.Documents = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oStockTransfer)
                    'SoEntry = SalesOrder.DocEntry
                    Dim StockTrans As SAPbobsCOM.StockTransfer = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oStockTransfer)
                    StockTrans.DocObjectCode = SAPbobsCOM.BoObjectTypes.oStockTransfer
                    If StockTrans.GetByKey(SoEntry) Then
                        StockTrans.UserFields.Fields.Item("U_TRCKID").Value = TransUpdate.TrackingID
                        StockTrans.UserFields.Fields.Item("U_COURCOM").Value = IIf(TransUpdate.CourierCompany Is Nothing, "", TransUpdate.CourierCompany)
                        StockTrans.UserFields.Fields.Item("U_DELCONST").Value = TransUpdate.StatusCode
                        Try
                            If ConfDate <> Nothing Then
                                StockTrans.UserFields.Fields.Item("U_CNFDATE").Value = New Date(Mid(ConfDate, 1, 4), Mid(ConfDate, 5, 2), Mid(ConfDate, 7, 2))
                            End If
                        Catch ex As Exception
                        End Try
                        Try
                            StockTrans.UserFields.Fields.Item("U_ITCHNL").Value = TransUpdate.DelChannel
                        Catch ex As Exception
                        End Try
                        Try
                            StockTrans.UserFields.Fields.Item("U_REMARKS").Value = TransUpdate.Remarks
                        Catch ex As Exception
                        End Try
                        Try
                            StockTrans.UserFields.Fields.Item("U_DELAGENT").Value = TransUpdate.DelAgent
                        Catch ex As Exception
                        End Try
                        Try
                            StockTrans.UserFields.Fields.Item("U_AREACODE").Value = TransUpdate.Area
                        Catch ex As Exception
                        End Try
                    End If
                    Dim lRetCode As Integer = StockTrans.Update()
                    Dim ErrorMessage As String = ""
                    If lRetCode <> 0 Then
                        Dim sErrMsg As String = G_DI_Company.GetLastErrorDescription()
                        G_DI_Company.GetLastError(lRetCode, sErrMsg)
                        ErrorMessage = sErrMsg
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(StockTrans)
                        'If G_DI_Company.InTransaction Then
                        '    G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                        'End If
                        G_DI_Company.Disconnect()
                        NewRow = dtTable.NewRow
                        NewRow.Item("ReturnCode") = "-2222"
                        NewRow.Item("ReturnDocEntry") = "-1"
                        NewRow.Item("ReturnObjType") = "-1"
                        NewRow.Item("ReturnSeries") = "-1"
                        NewRow.Item("ReturnDocNum") = "-1"
                        NewRow.Item("ReturnMsg") = ErrorMessage
                        dtTable.Rows.Add(NewRow)
                        Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                    Else
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(StockTrans)
                        ObjType = G_DI_Company.GetNewObjectType.ToString.TrimEnd
                        qstr = "SELECT B.""SeriesName"",CAST(A.""DocNum"" AS VARCHAR) ""DocNum"",B.""SeriesName""||'/'||CAST(A.""DocNum"" AS VARCHAR) ""StrDocNum"" " & vbNewLine &
                               " From ""OWTR"" A " & vbNewLine &
                               " INNER JOIN ""NNM1"" B ON B.""ObjectCode""=A.""ObjType"" AND B.""Series""=A.""Series"" " & vbNewLine &
                               " WHERE A.""DocEntry""='" + SoEntry + "' "
                        rSet.DoQuery(qstr)
                        ReturnDocNo = rSet.Fields.Item("StrDocNum").Value
                        SeriesName = rSet.Fields.Item("SeriesName").Value
                        DocNum = rSet.Fields.Item("DocNum").Value
                    End If
                Catch ex As Exception
                    'If G_DI_Company.InTransaction Then
                    '    G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                    'End If
                End Try
                'G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit)
                G_DI_Company.Disconnect()
                NewRow = dtTable.NewRow
                NewRow.Item("ReturnCode") = "0000"
                NewRow.Item("ReturnDocEntry") = SoEntry
                NewRow.Item("ReturnObjType") = ObjType
                NewRow.Item("ReturnSeries") = SeriesName
                NewRow.Item("ReturnDocNum") = DocNum
                NewRow.Item("ReturnMsg") = "Your Request No. " + ReturnDocNo + " successfully updated"
                dtTable.Rows.Add(NewRow)
                Return Request.CreateResponse(HttpStatusCode.OK, dtTable)

            Catch ex As Exception
                Try
                    G_DI_Company.Disconnect()
                Catch ex1 As Exception
                End Try
                NewRow = dtTable.NewRow
                NewRow.Item("ReturnCode") = "-2222"
                NewRow.Item("ReturnMsg") = ex.Message.ToString
                NewRow.Item("ReturnDocEntry") = "-1"
                NewRow.Item("ReturnObjType") = "-1"
                NewRow.Item("ReturnSeries") = "-1"
                NewRow.Item("ReturnDocNum") = "-1"
                dtTable.Rows.Add(NewRow)
                Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
            End Try
        End Function

        <Route("Api/PostCashExpenseJournal")>
        <HttpPost>
        Public Function PostCashExpenseJournal(ByVal Journal As SIL_MODEL_JOURNAL_HEADER) As HttpResponseMessage
            Dim G_DI_Company As SAPbobsCOM.Company = Nothing
            Dim strRegnNo As String = ""
            dtTable.Columns.Add("ReturnCode")
            dtTable.Columns.Add("ReturnDocEntry")
            dtTable.Columns.Add("ReturnObjType")
            dtTable.Columns.Add("ReturnSeries")
            dtTable.Columns.Add("ReturnDocNum")
            dtTable.Columns.Add("ReturnMsg")
            Try
                Dim Branch As String = ""
                Dim UserID As String = ""


                If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                    Return Request.CreateResponse(HttpStatusCode.OK, dtError)
                End If
                Dim qstr As String = ""
                'Dim qstr As String = ""
                qstr = "SELECT ""U_SAPUNAME"" ,""U_SAPPWD"" " & vbNewLine &
                       " FROM ""OUSR"" A " & vbNewLine &
                       " WHERE A.""USER_CODE"" ='" + UserID + "'"
                Dim dRow As Data.DataRow
                dRow = DBSQL.getQueryDataRow(qstr, "")
                Dim SAPUserId As String = ""
                Dim SAPPassword As String = ""
                Try
                    SAPUserId = dRow.Item("U_SAPUNAME")
                    SAPPassword = dRow.Item("U_SAPPWD")
                Catch ex As Exception
                End Try
                Connection.ConnectSAP(G_DI_Company, SAPUserId, SAPPassword)
                rSet = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)

                Dim DocNum As String = ""
                Dim SoEntry As String = ""
                Dim ObjType As String = ""
                Dim SeriesName As String = ""
                Dim ReturnDocNo As String = ""
                Try

                    Dim oJournal As SAPbobsCOM.JournalEntries = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oJournalEntries)
                    oJournal.ReferenceDate = New Date(Mid(Journal.PostingDate, 1, 4), Mid(Journal.PostingDate, 5, 2), Mid(Journal.PostingDate, 7, 2))
                    oJournal.TaxDate = New Date(Mid(Journal.PostingDate, 1, 4), Mid(Journal.PostingDate, 5, 2), Mid(Journal.PostingDate, 7, 2))
                    oJournal.DueDate = New Date(Mid(Journal.PostingDate, 1, 4), Mid(Journal.PostingDate, 5, 2), Mid(Journal.PostingDate, 7, 2))

                    'G_DI_Company.Disconnect()
                    'NewRow = dtTable.NewRow
                    'NewRow.Item("ReturnCode") = "0000"
                    'NewRow.Item("ReturnMsg") = qstr
                    'dtTable.Rows.Add(NewRow)
                    'Return Request.CreateResponse(HttpStatusCode.OK, dtTable)

                    rSet = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                    qstr = "Select Top 1 N.""Series"",B.""BPLId"",TO_CHAR(NOW(),'YYYYYMMDD') ""DocDate"" " & vbNewLine &
                                "  From ""NNM1"" N  " & vbNewLine &
                                "       Inner Join ""OFPR"" O On O.""Indicator""=N.""Indicator"" " & vbNewLine &
                                "       Inner Join ""OBPL"" B On B.""BPLId""=N.""BPLId"" " & vbNewLine &
                                " Where N.""ObjectCode""='30' " & vbNewLine &
                                "   And O.""PeriodStat"" In ('N','C') And N.""Locked""='N'   " & vbNewLine &
                                "   And TO_CHAR('" + Journal.PostingDate + "','YYYYYMMDD') Between TO_CHAR(O.""F_RefDate"",'YYYYYMMDD') And TO_CHAR(O.""T_RefDate"",'YYYYYMMDD')"

                    rSet.DoQuery(qstr)
                    Dim BPLID As String = rSet.Fields.Item("BPLId").Value

                    oJournal.Memo = IIf(Journal.Remarks Is Nothing, "", Journal.Remarks)
                    oJournal.UserFields.Fields.Item("U_CRTDBY").Value = UserID.ToString
                    oJournal.UserFields.Fields.Item("U_BUNIT").Value = Branch.ToString
                    For Each Item As SIL_MODEL_JOURNAL_DETAILS In Journal.Items
                        oJournal.Lines.AccountCode = Item.AccountCode
                        If Item.Anount > 0 Then
                            oJournal.Lines.Debit = Item.Anount
                        Else
                            oJournal.Lines.Credit = Item.Anount
                        End If
                        oJournal.Lines.BPLID = BPLID
                        oJournal.Lines.CostingCode = Branch
                        oJournal.Lines.CostingCode2 = Item.EmployeeCode
                        oJournal.Lines.LineMemo = IIf(Item.Remarks Is Nothing, "", Item.Remarks)
                        oJournal.Lines.Add()
                    Next

                    oJournal.Lines.AccountCode = Journal.AccountCode
                    If Journal.TotalValue > 0 Then
                        oJournal.Lines.Credit = Journal.TotalValue
                    Else
                        oJournal.Lines.Debit = Journal.TotalValue
                    End If
                    oJournal.Lines.BPLID = BPLID
                    oJournal.Lines.CostingCode = Branch
                    'oJournal.Lines.CostingCode2 = Item.EmployeeCode
                    'oJournal.Lines.LineMemo = Item.Remarks
                    oJournal.Lines.Add()

                    Dim lRetCode As Integer
                    lRetCode = oJournal.Add
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oJournal)
                    Dim ErrorMessage As String = ""
                    If lRetCode <> 0 Then
                        Dim sErrMsg As String = G_DI_Company.GetLastErrorDescription()
                        G_DI_Company.GetLastError(lRetCode, sErrMsg)
                        ErrorMessage = sErrMsg
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(oJournal)
                        'If G_DI_Company.InTransaction Then
                        '    G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                        'End If
                        G_DI_Company.Disconnect()
                        NewRow = dtTable.NewRow
                        NewRow.Item("ReturnCode") = "-2222"
                        NewRow.Item("ReturnDocEntry") = "-1"
                        NewRow.Item("ReturnObjType") = "-1"
                        NewRow.Item("ReturnSeries") = "-1"
                        NewRow.Item("ReturnDocNum") = "-1"
                        NewRow.Item("ReturnMsg") = ErrorMessage
                        dtTable.Rows.Add(NewRow)
                        Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                    Else
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(oJournal)
                        ObjType = G_DI_Company.GetNewObjectType.ToString.TrimEnd
                        Dim DLEntry As String = G_DI_Company.GetNewObjectKey.Trim.ToString
                        Dim ErrMsg As String = ""

                        qstr = "SELECT B.""SeriesName"",CAST(A.""Number"" AS VARCHAR) ""DocNum"",B.""SeriesName""||'/'||CAST(A.""Number"" AS VARCHAR) ""StrDocNum"" " & vbNewLine &
                               " From ""OJDT"" A " & vbNewLine &
                               " INNER JOIN ""NNM1"" B ON B.""ObjectCode""=A.""ObjType"" AND B.""Series""=A.""Series"" " & vbNewLine &
                               " WHERE A.""TransId""='" + DLEntry + "' "
                        Dim PostrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                        PostrSet.DoQuery(qstr)
                        rSet.DoQuery(qstr)
                        ReturnDocNo = rSet.Fields.Item("StrDocNum").Value
                        G_DI_Company.Disconnect()
                        NewRow = dtTable.NewRow
                        NewRow.Item("ReturnCode") = "0000"
                        NewRow.Item("ReturnDocEntry") = DLEntry
                        NewRow.Item("ReturnObjType") = ObjType
                        NewRow.Item("ReturnSeries") = PostrSet.Fields.Item("SeriesName").Value
                        NewRow.Item("ReturnDocNum") = PostrSet.Fields.Item("DocNum").Value
                        NewRow.Item("ReturnMsg") = "Your Request No. " + ReturnDocNo + " successfully submitted"
                        dtTable.Rows.Add(NewRow)
                        Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                    End If
                Catch ex As Exception
                    Try
                        G_DI_Company.Disconnect()
                    Catch ex1 As Exception
                    End Try
                    NewRow = dtTable.NewRow
                    NewRow.Item("ReturnCode") = "-2222"
                    NewRow.Item("ReturnMsg") = ex.Message.ToString
                    NewRow.Item("ReturnDocEntry") = "-1"
                    NewRow.Item("ReturnObjType") = "-1"
                    NewRow.Item("ReturnSeries") = "-1"
                    NewRow.Item("ReturnDocNum") = "-1"
                    dtTable.Rows.Add(NewRow)
                    Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                End Try
                'G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit)


            Catch ex As Exception
                Try
                    G_DI_Company.Disconnect()
                Catch ex1 As Exception
                End Try
                NewRow = dtTable.NewRow
                NewRow.Item("ReturnCode") = "-2222"
                NewRow.Item("ReturnMsg") = ex.Message.ToString
                NewRow.Item("ReturnDocEntry") = "-1"
                NewRow.Item("ReturnObjType") = "-1"
                NewRow.Item("ReturnSeries") = "-1"
                NewRow.Item("ReturnDocNum") = "-1"
                dtTable.Rows.Add(NewRow)
                Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
            End Try
        End Function

        <Route("Api/PostCreditMemoandInvoice")>
        <HttpPost>
        Public Function PostCreditMemoandInvoice(ByVal CreditmemoDetails As DTS_MODEL_CRDT_HEADER) As HttpResponseMessage

            Dim G_DI_Company As SAPbobsCOM.Company = Nothing
            Dim strRegnNo As String = ""
            dtTable.Columns.Add("ReturnCode")
            dtTable.Columns.Add("ReturnDocEntry")
            dtTable.Columns.Add("ReturnObjType")
            dtTable.Columns.Add("ReturnSeries")
            dtTable.Columns.Add("ReturnDocNum")
            dtTable.Columns.Add("ReturnMsg")
            Try
                Dim Branch As String = ""
                Dim UserID As String = ""


                If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                    Return Request.CreateResponse(HttpStatusCode.OK, dtError)
                End If
                Dim qstr As String = ""
                'Dim qstr As String = ""
                qstr = "SELECT ""U_SAPUNAME"" ,""U_SAPPWD"" " & vbNewLine &
                       " FROM ""OUSR"" A " & vbNewLine &
                       " WHERE A.""USER_CODE"" ='" + UserID + "'"
                Dim dRow As Data.DataRow
                dRow = DBSQL.getQueryDataRow(qstr, "")
                Dim SAPUserId As String = ""
                Dim SAPPassword As String = ""
                Try
                    SAPUserId = dRow.Item("U_SAPUNAME")
                    SAPPassword = dRow.Item("U_SAPPWD")
                Catch ex As Exception
                End Try
                Connection.ConnectSAP(G_DI_Company, SAPUserId, SAPPassword)

                rSet = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)


                qstr = "Select Top 1 N.""Series"",B.""BPLId"",C.""WhsCode"" ""WhsCode"" ,TO_CHAR(NOW(),'YYYYYMMDD') ""DocDate"" " & vbNewLine &
                                "  From ""NNM1"" N  " & vbNewLine &
                                "       Inner Join ""OFPR"" O On O.""Indicator""=N.""Indicator"" " & vbNewLine &
                                "       Inner Join ""OBPL"" B On B.""BPLId""=N.""BPLId"" " & vbNewLine &
                                "       Inner Join ""OWHS"" C On C.""U_BUSUNIT""='" + Branch + "' AND C.""U_WHSTYPE""='N' " & vbNewLine &
                                " Where N.""ObjectCode""='14' " & vbNewLine &
                                "   And O.""PeriodStat"" In ('N','C') And N.""Locked""='N' AND N.""Remark""='" + Branch + "'  " & vbNewLine &
                                "   And TO_CHAR('" + CreditmemoDetails.PostingDate + "','YYYYYMMDD') Between TO_CHAR(O.""F_RefDate"",'YYYYYMMDD') And TO_CHAR(O.""T_RefDate"",'YYYYYMMDD')"
                rSet.DoQuery(qstr)


                If rSet.RecordCount > 0 Then
                    G_DI_Company.StartTransaction()
                    Dim DocDate As String = CreditmemoDetails.PostingDate
                    Dim oCreditMemo As SAPbobsCOM.Documents = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oCreditNotes)
                    oCreditMemo.DocObjectCode = SAPbobsCOM.BoObjectTypes.oCreditNotes

                    oCreditMemo.CardCode = CreditmemoDetails.CardCode
                    'Delivery.BPL_IDAssignedToInvoice = InvoiceDetails.Branch
                    oCreditMemo.BPL_IDAssignedToInvoice = "1"
                    oCreditMemo.Series = rSet.Fields.Item("Series").Value
                    oCreditMemo.DocDate = New Date(Mid(DocDate, 1, 4), Mid(DocDate, 5, 2), Mid(DocDate, 7, 2))
                    oCreditMemo.DocDueDate = New Date(Mid(DocDate, 1, 4), Mid(DocDate, 5, 2), Mid(DocDate, 7, 2))
                    oCreditMemo.TaxDate = New Date(Mid(CreditmemoDetails.RefDate, 1, 4), Mid(CreditmemoDetails.RefDate, 5, 2), Mid(CreditmemoDetails.RefDate, 7, 2))
                    oCreditMemo.NumAtCard = CreditmemoDetails.RefNo
                    Dim TotalValue As Double = 0
                    Dim BaseEntry As Integer = 0
                    For Each Item As DTS_MODEL_CRDT_ITEMS In CreditmemoDetails.Items
                        If Item.Type = "C" Then
                            BaseEntry = Item.BaseEntry
                            oCreditMemo.Lines.UserFields.Fields.Item("U_BASETYPE").Value = Item.BaseType.ToString
                            oCreditMemo.Lines.UserFields.Fields.Item("U_BASENTR").Value = Item.BaseEntry.ToString
                            oCreditMemo.Lines.UserFields.Fields.Item("U_BASELINE").Value = Item.BaseLine.ToString


                            If Item.Discountamount > 0 Then
                                oCreditMemo.Lines.Expenses.LineTotal = (Item.Discountamount) * (-1)
                                oCreditMemo.Lines.Expenses.ExpenseCode = 1
                                oCreditMemo.Lines.Expenses.Add()
                            End If
                            'Else
                            oCreditMemo.Lines.ItemCode = Item.ItemCode
                            oCreditMemo.Lines.Quantity = Item.Quantity
                            oCreditMemo.Lines.UnitPrice = Item.PriceBeforeDiscount
                            'oCreditMemo.Lines.Price = Item.PriceBeforeDiscount
                            oCreditMemo.Lines.TaxCode = Item.TaxCode
                            oCreditMemo.Lines.WarehouseCode = Item.WhsCode
                            oCreditMemo.Lines.TaxCode = IIf(Item.TaxCode Is Nothing, "", Item.TaxCode)
                            oCreditMemo.Lines.MeasureUnit = IIf(Item.UOM Is Nothing, "", Item.UOM)
                            oCreditMemo.Lines.UserFields.Fields.Item("U_DISCPER").Value = Item.DiscountPercentage.ToString
                            oCreditMemo.Lines.CostingCode = Branch
                            'End If
                            'qstr = "SELECT ""LineTotal""+""VatSum"" ""TOTAL"",""Quantity"" FROM ""RIN1"" WHERE ""DocEntry""='" + Item.BaseEntry.ToString + "' AND ""LineNum""='" + Item.BaseLine.ToString + "'"
                            'Dim SOrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                            'SOrSet.DoQuery(qstr)
                            'Dim LineValue As Decimal = SOrSet.Fields.Item("TOTAL").Value / CType(SOrSet.Fields.Item("Quantity").Value, Decimal)
                            'TotalValue = TotalValue + (LineValue * Item.Quantity) + ((Item.Discountamount) * (-1))
                            Dim oItems As SAPbobsCOM.Items = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oItems)
                            oItems.GetByKey(Item.ItemCode)
                            Dim TotQty As Decimal = Item.Quantity
                            If oItems.ManageBatchNumbers = SAPbobsCOM.BoYesNoEnum.tYES Then
                                qstr = "SELECT A.""BatchNum"",A.""Quantity"" " & vbNewLine &
                                       " FROM ""IBT1"" A " & vbNewLine &
                                       " WHERE A.""BaseType""='13' " & vbNewLine &
                                       "    AND A.""BaseEntry"" ='" + Item.BaseEntry.ToString + "' " & vbNewLine &
                                       "    AND A.""BaseLinNum"" ='" + Item.BaseLine.ToString + "'"
                                Dim BatchrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                                BatchrSet.DoQuery(qstr)
                                Dim i As Integer = 0

                                While Not BatchrSet.EoF
                                    If TotQty > 0 Then
                                        If TotQty > CType(BatchrSet.Fields.Item("Quantity").Value.ToString, Decimal) Then
                                            oCreditMemo.Lines.BatchNumbers.BatchNumber = BatchrSet.Fields.Item("BatchNum").Value
                                            oCreditMemo.Lines.BatchNumbers.Quantity = BatchrSet.Fields.Item("Quantity").Value.ToString
                                            oCreditMemo.Lines.BatchNumbers.Add()
                                            TotQty = TotQty - CType(BatchrSet.Fields.Item("Quantity").Value.ToString, Decimal)
                                        Else
                                            oCreditMemo.Lines.BatchNumbers.BatchNumber = BatchrSet.Fields.Item("BatchNum").Value
                                            oCreditMemo.Lines.BatchNumbers.Quantity = TotQty.ToString
                                            oCreditMemo.Lines.BatchNumbers.Add()
                                            TotQty = TotQty - TotQty
                                        End If
                                    Else
                                        Exit While
                                    End If

                                    BatchrSet.MoveNext()
                                End While
                            End If
                            If oItems.ManageSerialNumbers = SAPbobsCOM.BoYesNoEnum.tYES Then
                                Dim i As Integer = 0
                                qstr = "SELECT A.""SysSerial"", " & vbNewLine &
                                       "    B.""IntrSerial"", " & vbNewLine &
                                       "    B.""SuppSerial"" " & vbNewLine &
                                       " FROM ""SRI1"" A " & vbNewLine &
                                       "    INNER JOIN ""OSRI"" B ON A.""SysSerial""=B.""SysSerial"" " & vbNewLine &
                                       " WHERE A.""BaseType""='13' " & vbNewLine &
                                       "    AND A.""BaseEntry"" ='" + Item.BaseEntry.ToString + "' " & vbNewLine &
                                       "    AND A.""BaseLinNum"" ='" + Item.BaseLine.ToString + "'"
                                Dim SerialrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                                SerialrSet.DoQuery(qstr)
                                While Not SerialrSet.EoF
                                    If TotQty > 0 Then
                                        oCreditMemo.Lines.SerialNumbers.SetCurrentLine(i)
                                        oCreditMemo.Lines.SerialNumbers.InternalSerialNumber = SerialrSet.Fields.Item("IntrSerial").Value
                                        oCreditMemo.Lines.SerialNumbers.ManufacturerSerialNumber = SerialrSet.Fields.Item("SuppSerial").Value
                                        'oGIIssue.Lines.SerialNumbers = Serial.ManufacturerSerialNumber
                                        oCreditMemo.Lines.SerialNumbers.Add()
                                        TotQty = TotQty - 1
                                    Else
                                        Exit While
                                    End If

                                    SerialrSet.MoveNext()
                                End While
                            End If

                            oCreditMemo.Lines.Add()
                        End If
                    Next
                    oCreditMemo.DocumentReferences.ReferencedObjectType = SAPbobsCOM.ReferencedObjectTypeEnum.rot_SalesInvoice
                    oCreditMemo.DocumentReferences.ReferencedDocEntry = BaseEntry.ToString
                    'oCreditMemo.DocumentReferences.Remark = "TEST"
                    oCreditMemo.JournalMemo = "Exchange"
                    Dim lRetCode As Integer
                    lRetCode = oCreditMemo.Add
                    Dim ErrorMessage As String = ""
                    If lRetCode <> 0 Then
                        Dim sErrMsg As String = G_DI_Company.GetLastErrorDescription()
                        G_DI_Company.GetLastError(lRetCode, sErrMsg)
                        ErrorMessage = sErrMsg
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(oCreditMemo)
                        If G_DI_Company.InTransaction Then
                            G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                        End If
                        G_DI_Company.Disconnect()
                        NewRow = dtTable.NewRow
                        NewRow.Item("ReturnCode") = "-22225"
                        NewRow.Item("ReturnDocEntry") = "-1"
                        NewRow.Item("ReturnObjType") = "-1"
                        NewRow.Item("ReturnSeries") = "-1"
                        NewRow.Item("ReturnDocNum") = "-1"
                        NewRow.Item("ReturnMsg") = ErrorMessage
                        dtTable.Rows.Add(NewRow)
                        Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                    Else
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(oCreditMemo)
                        Dim ObjType As String = G_DI_Company.GetNewObjectType.ToString.TrimEnd
                        Dim DLEntry As String = G_DI_Company.GetNewObjectKey.Trim.ToString
                        Dim ErrMsg As String = ""
                        Dim AREntry As String = ""
                        If PostDirectCreditInvoiceWithPayment(G_DI_Company, CreditmemoDetails, DLEntry, UserID, Branch, AREntry, ErrMsg) = False Then
                            If G_DI_Company.InTransaction Then
                                G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                            End If

                            G_DI_Company.Disconnect()
                            NewRow = dtTable.NewRow
                            NewRow.Item("ReturnCode") = "-22226"
                            NewRow.Item("ReturnDocEntry") = "-1"
                            NewRow.Item("ReturnObjType") = "-1"
                            NewRow.Item("ReturnSeries") = "-1"
                            NewRow.Item("ReturnDocNum") = "-1"
                            NewRow.Item("ReturnMsg") = ErrMsg
                            dtTable.Rows.Add(NewRow)
                            Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                        End If
                        Try
                            Dim Invoice As SAPbobsCOM.Documents = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices)
                            Invoice.DocObjectCode = SAPbobsCOM.BoObjectTypes.oInvoices
                            If Invoice.GetByKey(BaseEntry) Then
                                Invoice.DocumentReferences.ReferencedObjectType = SAPbobsCOM.ReferencedObjectTypeEnum.rot_SalesCreditNote
                                Invoice.DocumentReferences.ReferencedDocEntry = DLEntry
                                Dim lRetCode1 As Integer = Invoice.Update
                                If lRetCode1 <> 0 Then
                                    Dim sErrMsg As String = G_DI_Company.GetLastErrorDescription()
                                    G_DI_Company.GetLastError(lRetCode, sErrMsg)
                                    ErrorMessage = sErrMsg
                                    System.Runtime.InteropServices.Marshal.ReleaseComObject(Invoice)
                                    If G_DI_Company.InTransaction Then
                                        G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                                    End If
                                    G_DI_Company.Disconnect()
                                    NewRow = dtTable.NewRow
                                    NewRow.Item("ReturnCode") = "-22229"
                                    NewRow.Item("ReturnDocEntry") = "-1"
                                    NewRow.Item("ReturnObjType") = "-1"
                                    NewRow.Item("ReturnSeries") = "-1"
                                    NewRow.Item("ReturnDocNum") = "-1"
                                    NewRow.Item("ReturnMsg") = ErrorMessage
                                    dtTable.Rows.Add(NewRow)
                                    Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                                End If
                            End If
                        Catch ex As Exception
                            If G_DI_Company.InTransaction Then
                                G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                            End If

                            G_DI_Company.Disconnect()
                            NewRow = dtTable.NewRow
                            NewRow.Item("ReturnCode") = "-22226"
                            NewRow.Item("ReturnDocEntry") = "-1"
                            NewRow.Item("ReturnObjType") = "-1"
                            NewRow.Item("ReturnSeries") = "-1"
                            NewRow.Item("ReturnDocNum") = "-1"
                            NewRow.Item("ReturnMsg") = ex.Message
                            dtTable.Rows.Add(NewRow)
                            Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                        End Try

                        If ObjType = "112" Then
                            qstr = "SELECT B.""SeriesName"",CAST(A.""DocNum"" AS VARCHAR) ""DocNum"",B.""SeriesName""||'/'||CAST(A.""DocNum"" AS VARCHAR) ""StrDocNum"" " & vbNewLine &
                               " From ""ODRF"" A " & vbNewLine &
                               " INNER JOIN ""NNM1"" B ON B.""ObjectCode""=A.""ObjType"" AND B.""Series""=A.""Series"" " & vbNewLine &
                               " WHERE A.""DocEntry""='" + DLEntry + "' "
                        ElseIf ObjType = "14" Then
                            qstr = "SELECT B.""SeriesName"",CAST(A.""DocNum"" AS VARCHAR) ""DocNum"",B.""SeriesName""||'/'||CAST(A.""DocNum"" AS VARCHAR) ""StrDocNum"" " & vbNewLine &
                               " From ""ORIN"" A " & vbNewLine &
                               " INNER JOIN ""NNM1"" B ON B.""ObjectCode""=A.""ObjType"" AND B.""Series""=A.""Series"" " & vbNewLine &
                               " WHERE A.""DocEntry""='" + DLEntry + "' "
                        End If
                        rSet.DoQuery(qstr)
                        Dim ReturnDocNo = rSet.Fields.Item("StrDocNum").Value
                        Dim SeriesName = rSet.Fields.Item("SeriesName").Value
                        Dim DocNum = rSet.Fields.Item("DocNum").Value
                        G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit)
                        G_DI_Company.Disconnect()
                        NewRow = dtTable.NewRow
                        NewRow.Item("ReturnCode") = "0000"
                        NewRow.Item("ReturnDocEntry") = DLEntry
                        NewRow.Item("ReturnObjType") = ObjType
                        NewRow.Item("ReturnSeries") = rSet.Fields.Item("SeriesName").Value
                        NewRow.Item("ReturnDocNum") = rSet.Fields.Item("DocNum").Value
                        NewRow.Item("ReturnMsg") = "Your Request No. " + ReturnDocNo + " successfully submitted"
                        dtTable.Rows.Add(NewRow)
                        Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                    End If
                Else
                    If G_DI_Company.InTransaction Then
                        G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                    End If
                    G_DI_Company.Disconnect()
                    NewRow = dtTable.NewRow
                    NewRow.Item("ReturnCode") = "-22227"
                    NewRow.Item("ReturnDocEntry") = "-1"
                    NewRow.Item("ReturnObjType") = "-1"
                    NewRow.Item("ReturnSeries") = "-1"
                    NewRow.Item("ReturnDocNum") = "-1"
                    NewRow.Item("ReturnMsg") = "Series not found"
                    dtTable.Rows.Add(NewRow)
                    Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                End If

            Catch __unusedException1__ As Exception
                Try
                    If G_DI_Company.InTransaction Then
                        G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                    End If
                Catch ex As Exception
                End Try
                Try
                    G_DI_Company.Disconnect()
                Catch ex1 As Exception
                End Try
                NewRow = dtTable.NewRow
                NewRow.Item("ReturnCode") = "-22228"
                NewRow.Item("ReturnMsg") = __unusedException1__.Message.ToString
                NewRow.Item("ReturnDocEntry") = "-1"
                NewRow.Item("ReturnObjType") = "-1"
                NewRow.Item("ReturnSeries") = "-1"
                NewRow.Item("ReturnDocNum") = "-1"
                dtTable.Rows.Add(NewRow)
                Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
            End Try

        End Function

        Public Function PostDirectCreditInvoiceWithPayment(ByVal G_DI_Company As SAPbobsCOM.Company, ByVal CreditmemoDetails As DTS_MODEL_CRDT_HEADER,
                                                     ByVal CreditmemoNtry As String, ByVal UserId As String, ByVal Branch As String,
                                                    ByRef ARInvoiceEntry As String, ByRef erMessage As String) As Boolean

            Dim strRegnNo As String = ""
            Dim qstr As String
            'dtTable.Columns.Add("ReturnCode")
            'dtTable.Columns.Add("ReturnDocEntry")
            'dtTable.Columns.Add("ReturnObjType")
            'dtTable.Columns.Add("ReturnSeries")
            'dtTable.Columns.Add("ReturnDocNum")
            'dtTable.Columns.Add("ReturnMsg")
            Try

                rSet = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                qstr = "Select Top 1 N.""Series"",B.""BPLId"",C.""WhsCode"" ""WhsCode"" ,TO_CHAR(NOW(),'YYYYYMMDD') ""DocDate"" " & vbNewLine &
                                "  From ""NNM1"" N  " & vbNewLine &
                                "       Inner Join ""OFPR"" O On O.""Indicator""=N.""Indicator"" " & vbNewLine &
                                "       Inner Join ""OBPL"" B On B.""BPLId""=N.""BPLId"" " & vbNewLine &
                                "       Inner Join ""OWHS"" C On C.""U_BUSUNIT""='" + Branch + "' AND C.""U_WHSTYPE""='N' " & vbNewLine &
                                " Where N.""ObjectCode""='13' " & vbNewLine &
                                "   And O.""PeriodStat"" In ('N','C') And N.""Locked""='N' AND N.""Remark""='" + Branch + "'  " & vbNewLine &
                                "   And TO_CHAR('" + CreditmemoDetails.PostingDate + "','YYYYYMMDD') Between TO_CHAR(O.""F_RefDate"",'YYYYYMMDD') And TO_CHAR(O.""T_RefDate"",'YYYYYMMDD')"
                rSet.DoQuery(qstr)



                If rSet.RecordCount > 0 Then
                    Dim DocDate As String = CreditmemoDetails.PostingDate
                    Dim Invoice As SAPbobsCOM.Documents = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices)
                    Invoice.DocObjectCode = SAPbobsCOM.BoObjectTypes.oInvoices

                    Invoice.CardCode = CreditmemoDetails.CardCode
                    Invoice.BPL_IDAssignedToInvoice = "1"
                    Invoice.Series = rSet.Fields.Item("Series").Value
                    Invoice.DocDate = New Date(Mid(DocDate, 1, 4), Mid(DocDate, 5, 2), Mid(DocDate, 7, 2))
                    Invoice.DocDueDate = New Date(Mid(DocDate, 1, 4), Mid(DocDate, 5, 2), Mid(DocDate, 7, 2))
                    Invoice.TaxDate = New Date(Mid(CreditmemoDetails.RefDate, 1, 4), Mid(CreditmemoDetails.RefDate, 5, 2), Mid(CreditmemoDetails.RefDate, 7, 2))
                    Invoice.NumAtCard = CreditmemoDetails.RefNo
                    'Invoice.DocumentReferences.ReferencedObjectType = SAPbobsCOM.ReferencedObjectTypeEnum.rot_SalesCreditNote
                    'Invoice.DocumentReferences.ReferencedDocEntry = CreditmemoNtry.ToString

                    For Each Item As DTS_MODEL_CRDT_ITEMS In CreditmemoDetails.Items
                        If Item.Type = "I" Then
                            Invoice.Lines.UserFields.Fields.Item("U_BASETYPE").Value = Item.BaseType.ToString
                            Invoice.Lines.UserFields.Fields.Item("U_BASENTR").Value = Item.BaseEntry.ToString
                            Invoice.Lines.UserFields.Fields.Item("U_BASELINE").Value = Item.BaseLine.ToString

                            'Invoice.Lines.Quantity = Item.Quantity
                            'qstr = "SELECT * FROM  ""RDR2"" WHERE ""DocEntry""='" + SalesOrderEntry.ToString + "' " & vbNewLine &
                            '       "    And ""LineNum""='" + SoDrSet.Fields.Item("LineNum").Value.ToString + "' " & vbNewLine &
                            '       "    AND ""ExpnsCode""='1'"
                            'Dim SoFRGDrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                            'SoFRGDrSet.DoQuery(qstr)
                            If Item.Discountamount > 0 Then
                                Invoice.Lines.Expenses.LineTotal = (Item.Discountamount) * (-1)
                                Invoice.Lines.Expenses.ExpenseCode = 1
                                Invoice.Lines.Expenses.Add()
                            End If
                            'Else
                            Invoice.Lines.ItemCode = Item.ItemCode
                            Invoice.Lines.Quantity = Item.Quantity
                            Invoice.Lines.UnitPrice = Item.PriceBeforeDiscount
                            'Invoice.Lines.Price = Item.PriceBeforeDiscount
                            Invoice.Lines.TaxCode = Item.TaxCode
                            Invoice.Lines.WarehouseCode = Item.WhsCode
                            Invoice.Lines.TaxCode = IIf(Item.TaxCode Is Nothing, "", Item.TaxCode)
                            Invoice.Lines.MeasureUnit = IIf(Item.UOM Is Nothing, "", Item.UOM)
                            Invoice.Lines.UserFields.Fields.Item("U_DISCPER").Value = Item.DiscountPercentage.ToString
                            Invoice.Lines.CostingCode = Branch
                            'End If
                            'qstr = "SELECT ""LineTotal""+""VatSum"" ""TOTAL"",""Quantity"" FROM ""RDR1"" WHERE ""DocEntry""='" + Item.BaseEntry.ToString + "' AND ""LineNum""='" + Item.BaseLine.ToString + "'"
                            'Dim SOrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                            'SOrSet.DoQuery(qstr)
                            'Dim LineValue As Decimal = SOrSet.Fields.Item("TOTAL").Value / CType(SOrSet.Fields.Item("Quantity").Value, Decimal)
                            'TotalValue = TotalValue + (LineValue * Item.Quantity) + ((Item.Discountamount) * (-1))
                            Dim oItems As SAPbobsCOM.Items = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oItems)
                            oItems.GetByKey(Item.ItemCode)
                            If oItems.ManageBatchNumbers = SAPbobsCOM.BoYesNoEnum.tYES Then
                                Dim i As Integer = 0
                                For Each Batches As DTS_MODEL_CRDT_BATCH In Item.Batches
                                    If Batches.VisOrder = Item.VisOrder Then
                                        Invoice.Lines.BatchNumbers.SetCurrentLine(i)
                                        Invoice.Lines.BatchNumbers.BatchNumber = Batches.BatchNo
                                        Invoice.Lines.BatchNumbers.Quantity = Batches.BatchQuantity
                                        Invoice.Lines.BatchNumbers.Add()
                                        i = i + 1
                                    End If
                                Next
                            End If
                            If oItems.ManageSerialNumbers = SAPbobsCOM.BoYesNoEnum.tYES Then
                                Dim i As Integer = 0
                                For Each Serial As DTS_MODEL_CRDT_SERIAL In Item.Serial
                                    If Serial.VisOrder = Item.VisOrder Then
                                        Invoice.Lines.SerialNumbers.SetCurrentLine(i)
                                        Invoice.Lines.SerialNumbers.InternalSerialNumber = Serial.InternalSerialNumber
                                        Invoice.Lines.SerialNumbers.SystemSerialNumber = Serial.SystemSerialNumber
                                        Invoice.Lines.SerialNumbers.Add()
                                        i = i + 1
                                    End If
                                Next
                            End If
                            Invoice.Lines.Add()
                        End If
                    Next
                    Invoice.JournalMemo = "Exchange"
                    Dim lRetCode As Integer
                    lRetCode = Invoice.Add
                    Dim ErrorMessage As String = ""
                    If lRetCode <> 0 Then
                        Dim sErrMsg As String = G_DI_Company.GetLastErrorDescription()
                        G_DI_Company.GetLastError(lRetCode, sErrMsg)
                        erMessage = sErrMsg
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(Invoice)
                        Return False
                    Else

                        System.Runtime.InteropServices.Marshal.ReleaseComObject(Invoice)
                        Dim ObjType As String = G_DI_Company.GetNewObjectType.ToString.TrimEnd
                        Dim INVEntry As String = G_DI_Company.GetNewObjectKey.Trim.ToString

                        Try
                            Dim oCredit As SAPbobsCOM.Documents = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oCreditNotes)
                            oCredit.DocObjectCode = SAPbobsCOM.BoObjectTypes.oCreditNotes
                            If oCredit.GetByKey(CreditmemoNtry) Then
                                oCredit.DocumentReferences.ReferencedObjectType = SAPbobsCOM.ReferencedObjectTypeEnum.rot_SalesInvoice
                                oCredit.DocumentReferences.ReferencedDocEntry = INVEntry
                                Dim lRetCode1 As Integer = oCredit.Update
                                If lRetCode1 <> 0 Then
                                    Dim sErrMsg As String = G_DI_Company.GetLastErrorDescription()
                                    G_DI_Company.GetLastError(lRetCode, sErrMsg)
                                    erMessage = sErrMsg
                                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oCredit)
                                    Return False
                                End If
                            End If
                        Catch ex As Exception
                            erMessage = ex.Message
                            Return False
                        End Try

                        Dim InPay As SAPbobsCOM.Payments = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oIncomingPayments)
                        Dim VoucherPaymentNo As String = ""
                        qstr = "Select Top 1 N.""Series"",B.""BPLId"",C.""WhsCode"" ""WhsCode"" ,TO_CHAR(NOW(),'YYYYYMMDD') ""DocDate"" " & vbNewLine &
                                "  From ""NNM1"" N  " & vbNewLine &
                                "       Inner Join ""OFPR"" O On O.""Indicator""=N.""Indicator"" " & vbNewLine &
                                "       Inner Join ""OBPL"" B On B.""BPLId""=N.""BPLId"" " & vbNewLine &
                                "       Inner Join ""OWHS"" C On C.""U_BUSUNIT""='" + Branch + "' AND C.""U_WHSTYPE""='N' " & vbNewLine &
                                " Where N.""ObjectCode""='24' " & vbNewLine &
                                "   And O.""PeriodStat"" In ('N','C') And N.""Locked""='N' AND N.""Remark""='" + Branch + "'  " & vbNewLine &
                                "   And TO_CHAR('" + CreditmemoDetails.PostingDate + "','YYYYYMMDD') Between TO_CHAR(O.""F_RefDate"",'YYYYYMMDD') And TO_CHAR(O.""T_RefDate"",'YYYYYMMDD')"
                        'qstr = "Select Top 1 N.Series,CONVERT(VARCHAR(10),GETDATE(),112) 'DocDate' " & vbNewLine &
                        '        "  From NNM1 N  " & vbNewLine &
                        '        "       Inner Join OFPR O On O.Indicator=N.Indicator " & vbNewLine &
                        '        " Where N.ObjectCode='24' " & vbNewLine &
                        '        "   And O.PeriodStat In ('N','C') And N.Locked='N'   " & vbNewLine &
                        '        "   And CONVERT(VARCHAR(10),'" + InvoiceDetails.PostingDate + "',112) Between Convert(Varchar(10), O.F_RefDate,112) And Convert(Varchar(10), O.T_RefDate,112)"
                        rSet.DoQuery(qstr)
                        If rSet.RecordCount = 0 Then

                            erMessage = "Incoming Payment Series not found"
                            Return False
                        End If
                        Dim PaymentSeries As String = rSet.Fields.Item("Series").Value

                        Dim ARInv As SAPbobsCOM.Documents = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices)
                        ARInv = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices)
                        ARInv.DocObjectCode = SAPbobsCOM.BoObjectTypes.oInvoices
                        ARInv.GetByKey(Convert.ToInt32(INVEntry))
                        InPay.BPLID = "1"
                        InPay.CardCode = CreditmemoDetails.CardCode
                        'InPay.DocDate = New Date(Mid(getServerDate, 1, 4), Mid(getServerDate, 5, 2), Mid(getServerDate, 7, 2))
                        'InPay.BPLID = IncPayment.Fields.Item("BPLId").Value

                        InPay.Series = PaymentSeries
                        InPay.Invoices.DocEntry = Convert.ToInt32(INVEntry)
                        InPay.Invoices.InvoiceType = SAPbobsCOM.BoRcptInvTypes.it_Invoice
                        InPay.Invoices.Add()
                        InPay.Invoices.DocEntry = Convert.ToInt32(CreditmemoNtry)
                        InPay.Invoices.InvoiceType = SAPbobsCOM.BoRcptInvTypes.it_CredItnote
                        InPay.Invoices.Add()
                        'InPay.CashAccount = "12502001"
                        'InPay.CashSum = 18
                        For Each Item As DTS_MODEL_PMNT_DTLS In CreditmemoDetails.PaymentDetails
                            If Item.PaymentType = "S" Then
                                InPay.CashSum = Item.Amount
                                qstr = "SELECT ""OverCode"",""FrgnName"",""AcctCode"" FROM OACT  WHERE ""OverCode""='" + Branch + "' AND ""FrgnName""='S'"
                                Dim CashPayment As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                                CashPayment.DoQuery(qstr)
                                InPay.CashAccount = CashPayment.Fields.Item("AcctCode").Value

                            End If

                            If Item.PaymentType = "S" Then
                            Else
                                'qstr = "SELECT ""OverCode"",""FrgnName"",""AcctCode"" FROM OACT  WHERE ""OverCode""='" + Branch + "' AND ""FrgnName""='C'"
                                qstr = "SELECT ""CreditCard""  FROM ""OCRC"" where ""U_BANKCODE""='" + Item.Bank + "' AND ""U_PMNTP""='" + Item.PaymentType + "'"
                                Dim CreditPayment As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                                CreditPayment.DoQuery(qstr)
                                If CreditPayment.RecordCount > 0 Then
                                    InPay.CreditCards.CreditCard = CreditPayment.Fields.Item("CreditCard").Value
                                    If Item.PaymentType = "2" Or Item.PaymentType = "8" Then
                                        qstr = "SELECT ""PrcCode"",""PrcName"" FROM ""OPRC"" WHERE ""PrcCode""='" + Item.CardNo + "' and ""DimCode""='5' and ""U_CARDCODE""='" + CreditmemoDetails.CardCode + "'"
                                        Dim VouchNameStr As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                                        VouchNameStr.DoQuery(qstr)
                                        If VouchNameStr.RecordCount > 0 Then
                                            InPay.CreditCards.CreditCardNumber = VouchNameStr.Fields.Item("PrcCode").Value
                                            InPay.CreditCards.UserFields.Fields.Item("U_CARDNO").Value = VouchNameStr.Fields.Item("PrcCode").Value
                                            VoucherPaymentNo = IIf(VoucherPaymentNo = "", Item.CardNo + ":" + Item.Amount.ToString, VoucherPaymentNo + ";" + Item.CardNo + ":" + Item.Amount.ToString)
                                        Else
                                            erMessage = "No Voucher found for " + Item.CardNo
                                            Return False
                                        End If

                                    Else
                                        Try
                                            InPay.CreditCards.CreditCardNumber = IIf(Item.CardNo Is Nothing, "1111", Right(Item.CardNo.ToString, 4))
                                            InPay.CreditCards.UserFields.Fields.Item("U_CARDNO").Value = IIf(Item.CardNo Is Nothing, "1111", Item.CardNo.ToString)
                                        Catch ex As Exception
                                            InPay.CreditCards.CreditCardNumber = "1111"
                                            InPay.CreditCards.UserFields.Fields.Item("U_CARDNO").Value = "1111"
                                        End Try

                                    End If
                                    'IIf(Item.CardNo.ToString = "", "1111", Right(Item.CardNo.ToString, 4))

                                    InPay.CreditCards.CardValidUntil = New Date(Mid("29991231", 1, 4), Mid("29991231", 5, 2), Mid("29991231", 7, 2))
                                    InPay.CreditCards.PaymentMethodCode = 1
                                    InPay.CreditCards.CreditSum = Item.Amount
                                    'InPay.CreditCards.FirstPaymentSum = CreditCards.CardAmount
                                    Try
                                        InPay.CreditCards.VoucherNum = IIf(Item.Tranid Is Nothing, "111", Item.Tranid.ToString)
                                    Catch ex As Exception
                                        InPay.CreditCards.VoucherNum = "111"
                                    End Try

                                    InPay.CreditCards.CreditType = SAPbobsCOM.BoRcptCredTypes.cr_Regular
                                    InPay.CreditCards.Add()
                                Else
                                    erMessage = "No Payment Method found for " + Item.Bank + " and " + Item.PaymentType
                                    Return False
                                End If

                                'InPay.TransferAccount = CreditPayment.Fields.Item("AcctCode").Value
                                'InPay.TransferReference = Item.Tranid.ToString
                                'InPay.UserFields.Fields.Item("U_CRDTRNID").Value = Item.Tranid.ToString
                                'InPay.UserFields.Fields.Item("U_CRDONAME").Value = Item.CardHolderName.ToString
                                'InPay.UserFields.Fields.Item("U_CRDCNO").Value = Item.CardNo.ToString
                                'InPay.TransferDate = New Date(Mid(Date.Now.ToString("yyyyMMdd"), 1, 4), Mid(Date.Now.ToString("yyyyMMdd"), 5, 2), Mid(Date.Now.ToString("yyyyMMdd"), 7, 2))
                                'InPay.TransferSum = Item.Amount
                            End If



                        Next
                        lRetCode = InPay.Add()

                        If lRetCode <> 0 Then
                            Dim sErrMsg As String = G_DI_Company.GetLastErrorDescription()
                            G_DI_Company.GetLastError(lRetCode, sErrMsg)
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(InPay)
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(ARInv)
                            erMessage = sErrMsg
                            Return False

                        Else
                            'Dim ObjType As String = G_DI_Company.GetNewObjectType.ToString.TrimEnd
                            'Dim IncomingEntry = G_DI_Company.GetNewObjectKey.Trim.ToString
                            Dim IncDocEntry = G_DI_Company.GetNewObjectKey.Trim.ToString
                            If VoucherPaymentNo <> "" Then
                                For Each Dtls As String In VoucherPaymentNo.Split(New String() {";"}, StringSplitOptions.None)
                                    Dim PrcCode = Dtls.Split(":")(0)
                                    Dim Value As Decimal = Dtls.Split(":")(1)

                                    qstr = "UPDATE A SET ""OcrCode5""='" + PrcCode.ToString + "' " & vbNewLine &
                                           " FROM ""JDT1"" A " & vbNewLine &
                                           "    INNER JOIN ""OJDT"" B ON A.""TransId""=B.""TransId"" " & vbNewLine &
                                           "    INNER JOIN ""ORCT"" C ON C.""TransId""=B.""TransId"" AND C.""Canceled""='N' --AND C.""CardCode"" =A.""ShortName"" " & vbNewLine &
                                           "    INNER JOIN ""OACT"" E ON E.""AcctCode""=A.""Account"" AND E.""FrgnName""='VS' " & vbNewLine &
                                           "    INNER JOIN ""OPRC"" F ON F.""PrcCode""='" + PrcCode.ToString + "' " & vbNewLine &
                                           " WHERE A.""Debit""<>0 " & vbNewLine &
                                           " and F.""PrcCode""='" + PrcCode.ToString + "' " & vbNewLine &
                                           " AND A.""Debit""='" + Value.ToString + "' " & vbNewLine &
                                           " AND C.""DocEntry""='" + IncDocEntry.ToString + "' " & vbNewLine &
                                           "  AND IFNULL(A.""OcrCode5"",'')='' "
                                    Dim IncomingVchUpdt As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                                    IncomingVchUpdt.DoQuery(qstr)
                                Next

                            End If
                        End If

                        System.Runtime.InteropServices.Marshal.ReleaseComObject(InPay)
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(ARInv)


                        Return True
                    End If
                Else
                    erMessage = "Series not found"
                    Return False
                End If

            Catch __unusedException1__ As Exception
                erMessage = __unusedException1__.Message
                Return False
            End Try

        End Function

        <Route("Api/PostItemExchangeWithPayment")>
        <HttpPost>
        Public Function PostItemExchangeWithPayment(ByVal CreditMemoDetails As DTS_MODEL_CRDT_HEADER) As HttpResponseMessage

            Dim G_DI_Company As SAPbobsCOM.Company = Nothing
            Dim strRegnNo As String = ""
            dtTable.Columns.Add("ReturnCode")
            dtTable.Columns.Add("ReturnDocEntry")
            dtTable.Columns.Add("ReturnObjType")
            dtTable.Columns.Add("ReturnSeries")
            dtTable.Columns.Add("ReturnDocNum")
            dtTable.Columns.Add("ReturnMsg")
            Try
                Dim Branch As String = ""
                Dim UserID As String = ""


                If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                    Return Request.CreateResponse(HttpStatusCode.OK, dtError)
                End If
                Dim qstr As String = ""
                'Dim qstr As String = ""
                qstr = "SELECT ""U_SAPUNAME"" ,""U_SAPPWD"" " & vbNewLine &
                       " FROM ""OUSR"" A " & vbNewLine &
                       " WHERE A.""USER_CODE"" ='" + UserID + "'"
                Dim dRow As Data.DataRow
                dRow = DBSQL.getQueryDataRow(qstr, "")
                Dim SAPUserId As String = ""
                Dim SAPPassword As String = ""
                Try
                    SAPUserId = dRow.Item("U_SAPUNAME")
                    SAPPassword = dRow.Item("U_SAPPWD")
                Catch ex As Exception
                End Try
                Connection.ConnectSAP(G_DI_Company, SAPUserId, SAPPassword)

                rSet = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                qstr = "Select Top 1 N.""Series"",B.""BPLId"",C.""WhsCode"" ""WhsCode"" ,TO_CHAR(NOW(),'YYYYYMMDD') ""DocDate"" " & vbNewLine &
                                "  From ""NNM1"" N  " & vbNewLine &
                                "       Inner Join ""OFPR"" O On O.""Indicator""=N.""Indicator"" " & vbNewLine &
                                "       Inner Join ""OBPL"" B On B.""BPLId""=N.""BPLId"" " & vbNewLine &
                                "       Inner Join ""OWHS"" C On C.""U_BUSUNIT""='" + Branch + "' AND C.""U_WHSTYPE""='N' " & vbNewLine &
                                " Where N.""ObjectCode""='13' " & vbNewLine &
                                "   And O.""PeriodStat"" In ('N','C') And N.""Locked""='N' AND N.""Remark""='" + Branch + "'  " & vbNewLine &
                                "   And TO_CHAR('" + CreditMemoDetails.PostingDate + "','YYYYYMMDD') Between TO_CHAR(O.""F_RefDate"",'YYYYYMMDD') And TO_CHAR(O.""T_RefDate"",'YYYYYMMDD')"
                rSet.DoQuery(qstr)

                If rSet.RecordCount > 0 Then
                    G_DI_Company.StartTransaction()
                    Dim DiscountExists As Boolean = False
                    Dim DocDate As String = CreditMemoDetails.PostingDate
                    Dim Invoice As SAPbobsCOM.Documents = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices)
                    Invoice.DocObjectCode = SAPbobsCOM.BoObjectTypes.oInvoices

                    Invoice.CardCode = CreditMemoDetails.CardCode
                    'Delivery.BPL_IDAssignedToInvoice = InvoiceDetails.Branch
                    Invoice.BPL_IDAssignedToInvoice = rSet.Fields.Item("BPLId").Value
                    Invoice.Series = rSet.Fields.Item("Series").Value
                    Invoice.DocDate = New Date(Mid(DocDate, 1, 4), Mid(DocDate, 5, 2), Mid(DocDate, 7, 2))
                    Invoice.DocDueDate = New Date(Mid(DocDate, 1, 4), Mid(DocDate, 5, 2), Mid(DocDate, 7, 2))
                    Invoice.TaxDate = New Date(Mid(CreditMemoDetails.RefDate, 1, 4), Mid(CreditMemoDetails.RefDate, 5, 2), Mid(CreditMemoDetails.RefDate, 7, 2))
                    Invoice.NumAtCard = CreditMemoDetails.RefNo
                    'Invoice.SalesPersonCode = InvoiceDetails.SalesEmployee.ToString
                    'Invoice.UserFields.Fields.Item("U_BASENTR").Value = IIf(CreditMemoDetails.BaseEntry Is Nothing, "", CreditMemoDetails.BaseEntry)
                    Invoice.Comments = CreditMemoDetails.Remarks
                    'Invoice.UserFields.Fields.Item("U_TOBUNIT").Value = CreditMemoDetails.ToBranch
                    Invoice.UserFields.Fields.Item("U_CRTDBY").Value = UserID
                    Invoice.UserFields.Fields.Item("U_BRANCH").Value = Branch
                    If CreditMemoDetails.ItemType <> Nothing Then
                        Invoice.UserFields.Fields.Item("U_ITMTYPE").Value = CreditMemoDetails.ItemType
                    End If
                    Dim TotalValue As Double = 0
                    Dim SoEntry As String = "-1"
                    Dim PrepaidCrad As String = ""
                    Dim TransactionVal As Decimal = 0
                    Dim BaseEntry As String = ""
                    For Each Item As DTS_MODEL_CRDT_ITEMS In CreditMemoDetails.Items
                        Dim BaseType As String = IIf(Item.BaseType Is Nothing, "", Item.BaseType)

                        Dim PaymentExists As Boolean = False
                        Invoice.Lines.UserFields.Fields.Item("U_DISCPER").Value = Item.Discountamount.ToString
                        Invoice.Lines.ItemCode = Item.ItemCode
                        Invoice.Lines.TaxCode = Item.TaxCode
                        If Item.Type = "C" Then
                            Invoice.Lines.UserFields.Fields.Item("U_BASETYPE").Value = Item.BaseType.ToString
                            BaseEntry = Item.BaseEntry.ToString
                            Invoice.Lines.UserFields.Fields.Item("U_BASENTR").Value = Item.BaseEntry.ToString
                            Invoice.Lines.UserFields.Fields.Item("U_BASELINE").Value = Item.BaseLine.ToString

                            Invoice.Lines.Quantity = Item.Quantity * (-1)
                            Dim oItems1 As SAPbobsCOM.Items = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oItems)
                            oItems1.GetByKey(Item.ItemCode)
                            Dim TotQty As Decimal = Item.Quantity
                            If oItems1.ManageBatchNumbers = SAPbobsCOM.BoYesNoEnum.tYES Then
                                qstr = "SELECT A.""BatchNum"",A.""Quantity"" " & vbNewLine &
                                       " FROM ""IBT1"" A " & vbNewLine &
                                       " WHERE A.""BaseType""='13' " & vbNewLine &
                                       "    AND A.""BaseEntry"" ='" + Item.BaseEntry.ToString + "' " & vbNewLine &
                                       "    AND A.""BaseLinNum"" ='" + Item.BaseLine.ToString + "'"
                                Dim BatchrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                                BatchrSet.DoQuery(qstr)
                                Dim i As Integer = 0

                                While Not BatchrSet.EoF
                                    If TotQty > 0 Then
                                        If TotQty > CType(BatchrSet.Fields.Item("Quantity").Value.ToString, Decimal) Then
                                            Invoice.Lines.BatchNumbers.BatchNumber = BatchrSet.Fields.Item("BatchNum").Value
                                            Invoice.Lines.BatchNumbers.Quantity = BatchrSet.Fields.Item("Quantity").Value.ToString
                                            Invoice.Lines.BatchNumbers.Add()
                                            TotQty = TotQty - CType(BatchrSet.Fields.Item("Quantity").Value.ToString, Decimal)
                                        Else
                                            Invoice.Lines.BatchNumbers.BatchNumber = BatchrSet.Fields.Item("BatchNum").Value
                                            Invoice.Lines.BatchNumbers.Quantity = TotQty.ToString
                                            Invoice.Lines.BatchNumbers.Add()
                                            TotQty = TotQty - TotQty
                                        End If
                                    Else
                                        Exit While
                                    End If

                                    BatchrSet.MoveNext()
                                End While
                            End If
                            If oItems1.ManageSerialNumbers = SAPbobsCOM.BoYesNoEnum.tYES Then
                                Dim i As Integer = 0
                                qstr = "SELECT A.""SysSerial"", " & vbNewLine &
                                       "    B.""IntrSerial"", " & vbNewLine &
                                       "    B.""SuppSerial"" " & vbNewLine &
                                       " FROM ""SRI1"" A " & vbNewLine &
                                       "    INNER JOIN ""OSRI"" B ON A.""SysSerial""=B.""SysSerial"" " & vbNewLine &
                                       " WHERE A.""BaseType""='13' " & vbNewLine &
                                       "    AND A.""BaseEntry"" ='" + Item.BaseEntry.ToString + "' " & vbNewLine &
                                       "    AND A.""BaseLinNum"" ='" + Item.BaseLine.ToString + "'"
                                Dim SerialrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                                SerialrSet.DoQuery(qstr)
                                While Not SerialrSet.EoF
                                    If TotQty > 0 Then
                                        Invoice.Lines.SerialNumbers.SetCurrentLine(i)
                                        Invoice.Lines.SerialNumbers.InternalSerialNumber = SerialrSet.Fields.Item("IntrSerial").Value
                                        Invoice.Lines.SerialNumbers.ManufacturerSerialNumber = SerialrSet.Fields.Item("SuppSerial").Value
                                        'oGIIssue.Lines.SerialNumbers = Serial.ManufacturerSerialNumber
                                        Invoice.Lines.SerialNumbers.Add()
                                        TotQty = TotQty - 1
                                    Else
                                        Exit While
                                    End If

                                    SerialrSet.MoveNext()
                                End While
                            End If

                            If Item.Discountamount > 0 Then
                                DiscountExists = True
                                '    Invoice.Lines.Expenses.LineTotal = (Item.Discountamount)
                                '    Invoice.Lines.Expenses.ExpenseCode = 1
                                '    Invoice.Lines.Expenses.Add()
                            End If
                        Else
                            Invoice.Lines.Quantity = Item.Quantity
                            Dim oItems As SAPbobsCOM.Items = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oItems)
                            oItems.GetByKey(Item.ItemCode)
                            If oItems.ManageBatchNumbers = SAPbobsCOM.BoYesNoEnum.tYES Then
                                Dim i As Integer = 0
                                For Each Batches As DTS_MODEL_CRDT_BATCH In Item.Batches
                                    If Batches.VisOrder = Item.VisOrder Then
                                        'oGIIssue.Lines.BatchNumbers.SetCurrentLine(i)
                                        Invoice.Lines.BatchNumbers.BatchNumber = Batches.BatchNo
                                        Invoice.Lines.BatchNumbers.Quantity = Batches.BatchQuantity
                                        'If i <> 0 Then
                                        '    Delivery.Lines.BatchNumbers.Add()
                                        'End If
                                        Invoice.Lines.BatchNumbers.Add()

                                        i = i + 1
                                    End If
                                Next
                            End If

                            If oItems.ManageSerialNumbers = SAPbobsCOM.BoYesNoEnum.tYES Then
                                Dim i As Integer = 0
                                For Each Serial As DTS_MODEL_CRDT_SERIAL In Item.Serial
                                    If Serial.VisOrder = Item.VisOrder Then
                                        Invoice.Lines.SerialNumbers.SetCurrentLine(i)
                                        Invoice.Lines.SerialNumbers.InternalSerialNumber = Serial.InternalSerialNumber
                                        Invoice.Lines.SerialNumbers.SystemSerialNumber = Serial.SystemSerialNumber
                                        Invoice.Lines.SerialNumbers.Add()
                                        i = i + 1
                                    End If
                                Next
                            End If
                            If Item.Discountamount > 0 Then
                                DiscountExists = True
                                'Invoice.Lines.Expenses.LineTotal = (Item.Discountamount) * (-1)
                                'Invoice.Lines.Expenses.ExpenseCode = 1
                                'Invoice.Lines.Expenses.Add()
                            End If
                        End If
                        Invoice.Lines.UnitPrice = Item.PriceBeforeDiscount
                        Invoice.Lines.DiscountPercent = Item.DiscountPercentage
                        Invoice.Lines.TaxCode = IIf(Item.TaxCode Is Nothing, "", Item.TaxCode)
                        Invoice.Lines.MeasureUnit = IIf(Item.UOM Is Nothing, "", Item.UOM)
                        Invoice.Lines.ShipDate = New Date(Mid(Item.DocDueDate, 1, 4), Mid(Item.DocDueDate, 5, 2), Mid(Item.DocDueDate, 7, 2))
                        'qstr = "SELECT ""Rate"" FROM ""OSTC"" WHERE ""Code""='" + Item.TaxCode + "'"
                        'Dim TAXrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                        'TAXrSet.DoQuery(qstr)
                        'Dim TaxAmount As Decimal = Math.Round((Item.PriceBeforeDiscount * Item.Quantity) * (TAXrSet.Fields.Item("Rate").Value / 100), 2)
                        'TotalValue = TotalValue + Math.Round((Item.PriceBeforeDiscount * Item.Quantity) + TaxAmount - Item.Discountamount, 2)
                        Invoice.Lines.WarehouseCode = Item.WhsCode
                        'Invoice.Lines.TaxCode = Item.TaxCode
                        Invoice.Lines.CostingCode = Branch
                        Invoice.Lines.Add()
                    Next
                    Dim InvcAmt As Double = 0
                    Dim BalnaceAmt As Double = 0
                    Invoice.DocumentReferences.ReferencedObjectType = SAPbobsCOM.ReferencedObjectTypeEnum.rot_SalesInvoice
                    Invoice.DocumentReferences.ReferencedDocEntry = BaseEntry.ToString
                    Dim lRetCode As Integer
                    lRetCode = Invoice.Add


                    Dim ErrorMessage As String = ""
                    If lRetCode <> 0 Then
                        Dim sErrMsg As String = G_DI_Company.GetLastErrorDescription()
                        G_DI_Company.GetLastError(lRetCode, sErrMsg)
                        ErrorMessage = sErrMsg
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(Invoice)
                        If G_DI_Company.InTransaction Then
                            G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                        End If
                        G_DI_Company.Disconnect()
                        NewRow = dtTable.NewRow
                        NewRow.Item("ReturnCode") = "-2222"
                        NewRow.Item("ReturnDocEntry") = "-1"
                        NewRow.Item("ReturnObjType") = "-1"
                        NewRow.Item("ReturnSeries") = "-1"
                        NewRow.Item("ReturnDocNum") = "-1"
                        NewRow.Item("ReturnMsg") = ErrorMessage
                        dtTable.Rows.Add(NewRow)
                        Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                    Else
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(Invoice)
                        Dim ObjType As String = G_DI_Company.GetNewObjectType.ToString.TrimEnd
                        Dim DLEntry As String = G_DI_Company.GetNewObjectKey.Trim.ToString
                        Dim ErrMsg As String = ""


                        If ObjType = "112" Then
                            qstr = "SELECT B.""SeriesName"",CAST(A.""DocNum"" AS VARCHAR) ""DocNum"",B.""SeriesName""||'/'||CAST(A.""DocNum"" AS VARCHAR) ""StrDocNum"" " & vbNewLine &
                               " From ""ODRF"" A " & vbNewLine &
                               " INNER JOIN ""NNM1"" B ON B.""ObjectCode""=A.""ObjType"" AND B.""Series""=A.""Series"" " & vbNewLine &
                               " WHERE A.""DocEntry""='" + DLEntry + "' "
                        ElseIf ObjType = "13" Then
                            qstr = "SELECT A.""DocTotal"",B.""SeriesName"",CAST(A.""DocNum"" AS VARCHAR) ""DocNum"",B.""SeriesName""||'/'||CAST(A.""DocNum"" AS VARCHAR) ""StrDocNum"" " & vbNewLine &
                               " From ""OINV"" A " & vbNewLine &
                               " INNER JOIN ""NNM1"" B ON B.""ObjectCode""=A.""ObjType"" AND B.""Series""=A.""Series"" " & vbNewLine &
                               " WHERE A.""DocEntry""='" + DLEntry + "' "
                        End If
                        rSet.DoQuery(qstr)
                        Dim ReturnDocNo = rSet.Fields.Item("StrDocNum").Value
                        Dim Series = rSet.Fields.Item("SeriesName").Value
                        Dim DocNum = rSet.Fields.Item("DocNum").Value
                        If CType(rSet.Fields.Item("DocTotal").Value, Double) > 0 Then
                            Try
                                Dim IncomingPAymentEntry As String = ""
                                Dim ermsg As String = ""

                                If IncomingPaymentExchangeAdd(G_DI_Company, CreditMemoDetails, DLEntry, UserID, Branch, IncomingPAymentEntry, ermsg, DiscountExists) = False Then
                                    If G_DI_Company.InTransaction Then
                                        G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                                    End If

                                    G_DI_Company.Disconnect()
                                    NewRow = dtTable.NewRow
                                    NewRow.Item("ReturnCode") = "-3333"
                                    NewRow.Item("ReturnDocEntry") = "-1"
                                    NewRow.Item("ReturnObjType") = "-1"
                                    NewRow.Item("ReturnSeries") = "-1"
                                    NewRow.Item("ReturnDocNum") = "-1"
                                    NewRow.Item("ReturnMsg") = ermsg
                                    dtTable.Rows.Add(NewRow)
                                    Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                                End If
                            Catch ex As Exception
                                If G_DI_Company.InTransaction Then
                                    G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                                End If

                                G_DI_Company.Disconnect()
                                NewRow = dtTable.NewRow
                                NewRow.Item("ReturnCode") = "-2222"
                                NewRow.Item("ReturnDocEntry") = "-1"
                                NewRow.Item("ReturnObjType") = "-1"
                                NewRow.Item("ReturnSeries") = "-1"
                                NewRow.Item("ReturnDocNum") = "-1"
                                NewRow.Item("ReturnMsg") = ex.Message
                                dtTable.Rows.Add(NewRow)
                                Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                            End Try
                        End If


                        G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit)
                        G_DI_Company.Disconnect()
                        NewRow = dtTable.NewRow
                        NewRow.Item("ReturnCode") = "0000"
                        NewRow.Item("ReturnDocEntry") = DLEntry
                        NewRow.Item("ReturnObjType") = ObjType
                        NewRow.Item("ReturnSeries") = Series
                        NewRow.Item("ReturnDocNum") = DocNum
                        NewRow.Item("ReturnMsg") = "Your Request No. " + ReturnDocNo + " successfully submitted"
                        dtTable.Rows.Add(NewRow)
                        Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                    End If
                Else
                    If G_DI_Company.InTransaction Then
                        G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                    End If
                    G_DI_Company.Disconnect()
                    NewRow = dtTable.NewRow
                    NewRow.Item("ReturnCode") = "-2222"
                    NewRow.Item("ReturnDocEntry") = "-1"
                    NewRow.Item("ReturnObjType") = "-1"
                    NewRow.Item("ReturnSeries") = "-1"
                    NewRow.Item("ReturnDocNum") = "-1"
                    NewRow.Item("ReturnMsg") = "Series not found"
                    dtTable.Rows.Add(NewRow)
                    Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                End If

            Catch __unusedException1__ As Exception
                Try
                    If G_DI_Company.InTransaction Then
                        G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                    End If
                Catch ex As Exception
                End Try
                Try
                    G_DI_Company.Disconnect()
                Catch ex1 As Exception
                End Try
                NewRow = dtTable.NewRow
                NewRow.Item("ReturnCode") = "-2222"
                NewRow.Item("ReturnMsg") = __unusedException1__.Message.ToString
                NewRow.Item("ReturnDocEntry") = "-1"
                NewRow.Item("ReturnObjType") = "-1"
                NewRow.Item("ReturnSeries") = "-1"
                NewRow.Item("ReturnDocNum") = "-1"
                dtTable.Rows.Add(NewRow)
                Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
            End Try

        End Function

        Public Function IncomingPaymentExchangeAdd(ByVal G_DI_Company As SAPbobsCOM.Company, ByVal CreditmemoDetails As DTS_MODEL_CRDT_HEADER, ByVal InvoiceEntry As String, ByVal UserId As String, ByVal Branch As String,
                               ByRef DpmEntry As String, ByRef erMessage As String, ByVal DiscountExists As Boolean) As Boolean
            Try
                Dim VoucherPaymentNo As String = ""
                rSet = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                Dim qstr As String
                qstr = "Select Top 1 N.""Series"",B.""BPLId"",C.""WhsCode"" ""WhsCode"" ,TO_CHAR(NOW(),'YYYYYMMDD') ""DocDate"" " & vbNewLine &
                                "  From ""NNM1"" N  " & vbNewLine &
                                "       Inner Join ""OFPR"" O On O.""Indicator""=N.""Indicator"" " & vbNewLine &
                                "       Inner Join ""OBPL"" B On B.""BPLId""=N.""BPLId"" " & vbNewLine &
                                "       Inner Join ""OWHS"" C On C.""U_BUSUNIT""='" + Branch + "' AND C.""U_WHSTYPE""='N' " & vbNewLine &
                                " Where N.""ObjectCode""='24' " & vbNewLine &
                                "   And O.""PeriodStat"" In ('N','C') And N.""Locked""='N' AND N.""Remark""='" + Branch + "'  " & vbNewLine &
                                "   And TO_CHAR('" + CreditmemoDetails.PostingDate + "','YYYYYMMDD') Between TO_CHAR(O.""F_RefDate"",'YYYYYMMDD') And TO_CHAR(O.""T_RefDate"",'YYYYYMMDD')"

                Dim IncPayment As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                IncPayment.DoQuery(qstr)
                If IncPayment.RecordCount > 0 Then
                Else
                    erMessage = "Incoimng Payment Series not Found"
                    Return False
                End If
                Dim InPay As SAPbobsCOM.Payments = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oIncomingPayments)

                qstr = "SELECT * FROM  ""OINV"" WHERE ""DocEntry""='" + InvoiceEntry.ToString + "'"
                Dim SoHrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                SoHrSet.DoQuery(qstr)

                qstr = "SELECT * FROM  ""INV1"" WHERE ""DocEntry""='" + InvoiceEntry.ToString + "'"
                Dim SoDrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                SoDrSet.DoQuery(qstr)
                InPay.CardCode = SoHrSet.Fields.Item("CardCode").Value
                InPay.UserFields.Fields.Item("U_BRANCH").Value = Branch
                InPay.UserFields.Fields.Item("U_CRTDBY").Value = UserId
                'InPay.DocDate = New Date(Mid(getServerDate, 1, 4), Mid(getServerDate, 5, 2), Mid(getServerDate, 7, 2))
                InPay.BPLID = SoHrSet.Fields.Item("BPLId").Value
                InPay.DocDate = New Date(Mid(CreditmemoDetails.PostingDate, 1, 4), Mid(CreditmemoDetails.PostingDate, 5, 2), Mid(CreditmemoDetails.PostingDate, 7, 2))
                InPay.Series = IncPayment.Fields.Item("Series").Value
                InPay.Invoices.DocEntry = Convert.ToInt32(InvoiceEntry)
                InPay.Invoices.InvoiceType = SAPbobsCOM.BoRcptInvTypes.it_Invoice
                InPay.Invoices.DistributionRule = Branch
                InPay.Invoices.Add()
                Dim ARDown As SAPbobsCOM.Documents = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices)
                ARDown = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices)
                ARDown.DocObjectCode = SAPbobsCOM.BoObjectTypes.oInvoices
                ARDown.GetByKey(Convert.ToInt32(InvoiceEntry))

                For Each Item As DTS_MODEL_PMNT_DTLS In CreditmemoDetails.PaymentDetails
                    If Item.PaymentType = "S" Then
                        InPay.CashSum = Item.Amount
                        qstr = "SELECT ""OverCode"",""FrgnName"",""AcctCode"" FROM OACT  WHERE ""OverCode""='" + Branch + "' AND ""FrgnName""='S'"
                        Dim CashPayment As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                        CashPayment.DoQuery(qstr)
                        InPay.CashAccount = CashPayment.Fields.Item("AcctCode").Value

                    End If

                    If Item.PaymentType = "S" Then
                    Else
                        'qstr = "SELECT ""OverCode"",""FrgnName"",""AcctCode"" FROM OACT  WHERE ""OverCode""='" + Branch + "' AND ""FrgnName""='C'"
                        qstr = "SELECT ""CreditCard""  FROM ""OCRC"" where ""U_BANKCODE""='" + Item.Bank + "' AND ""U_PMNTP""='" + Item.PaymentType + "'"
                        Dim CreditPayment As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                        CreditPayment.DoQuery(qstr)
                        If CreditPayment.RecordCount > 0 Then
                            InPay.CreditCards.CreditCard = CreditPayment.Fields.Item("CreditCard").Value
                            If Item.PaymentType = "2" Or Item.PaymentType = "8" Then
                                If DiscountExists = True Then
                                    erMessage = "Discount can not be done for Voucher type Payment please Remove Item Discount"
                                    Return False
                                End If
                                qstr = "SELECT ""PrcCode"",""PrcName"" FROM ""OPRC"" WHERE ""PrcCode""='" + Item.CardNo + "' and ""DimCode""='5' and ""U_CARDCODE""='" + SoHrSet.Fields.Item("CardCode").Value + "'"
                                Dim VouchNameStr As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                                VouchNameStr.DoQuery(qstr)
                                If VouchNameStr.RecordCount > 0 Then
                                    InPay.CreditCards.CreditCardNumber = VouchNameStr.Fields.Item("PrcCode").Value
                                    InPay.CreditCards.UserFields.Fields.Item("U_CARDNO").Value = VouchNameStr.Fields.Item("PrcCode").Value
                                    VoucherPaymentNo = IIf(VoucherPaymentNo = "", Item.CardNo + ":" + Item.Amount.ToString, VoucherPaymentNo + ";" + Item.CardNo + ":" + Item.Amount.ToString)
                                Else
                                    erMessage = "No Voucher found for " + Item.CardNo
                                    Return False
                                End If

                            Else
                                Try
                                    InPay.CreditCards.CreditCardNumber = IIf(Item.CardNo Is Nothing, "1111", Right(Item.CardNo.ToString, 4))
                                    InPay.CreditCards.UserFields.Fields.Item("U_CARDNO").Value = IIf(Item.CardNo Is Nothing, "1111", Item.CardNo.ToString)
                                Catch ex As Exception
                                    InPay.CreditCards.CreditCardNumber = "1111"
                                    InPay.CreditCards.UserFields.Fields.Item("U_CARDNO").Value = "1111"
                                End Try
                            End If
                            'IIf(Item.CardNo.ToString = "", "1111", Right(Item.CardNo.ToString, 4))

                            InPay.CreditCards.CardValidUntil = New Date(Mid("29991231", 1, 4), Mid("29991231", 5, 2), Mid("29991231", 7, 2))
                            InPay.CreditCards.PaymentMethodCode = 1
                            InPay.CreditCards.CreditSum = Item.Amount
                            'InPay.CreditCards.FirstPaymentSum = CreditCards.CardAmount
                            Try
                                InPay.CreditCards.VoucherNum = IIf(Item.Tranid Is Nothing, "111", Item.Tranid.ToString)
                            Catch ex As Exception
                                InPay.CreditCards.VoucherNum = "111"
                            End Try

                            InPay.CreditCards.CreditType = SAPbobsCOM.BoRcptCredTypes.cr_Regular
                            InPay.CreditCards.Add()
                        Else
                            erMessage = "No Payment Method found for " + Item.Bank + " and " + Item.PaymentType
                            Return False
                        End If

                    End If
                Next
                Dim lRetCode As Integer = InPay.Add
                If lRetCode <> 0 Then
                    Dim sErrMsg As String = G_DI_Company.GetLastErrorDescription()
                    G_DI_Company.GetLastError(lRetCode, sErrMsg)
                    erMessage = sErrMsg
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(InPay)
                    Return False
                End If
                Dim IncDocEntry = G_DI_Company.GetNewObjectKey.Trim.ToString
                If VoucherPaymentNo <> "" Then
                    For Each Dtls As String In VoucherPaymentNo.Split(New String() {";"}, StringSplitOptions.None)
                        Dim PrcCode = Dtls.Split(":")(0)
                        Dim Value As Decimal = Dtls.Split(":")(1)

                        qstr = "UPDATE A SET ""OcrCode5""='" + PrcCode.ToString + "' " & vbNewLine &
                               " FROM ""JDT1"" A " & vbNewLine &
                               "    INNER JOIN ""OJDT"" B ON A.""TransId""=B.""TransId"" " & vbNewLine &
                               "    INNER JOIN ""ORCT"" C ON C.""TransId""=B.""TransId"" AND C.""Canceled""='N' --AND C.""CardCode"" =A.""ShortName"" " & vbNewLine &
                               "    INNER JOIN ""OACT"" E ON E.""AcctCode""=A.""Account"" AND E.""FrgnName""='VS' " & vbNewLine &
                               "    INNER JOIN ""OPRC"" F ON F.""PrcCode""='" + PrcCode.ToString + "' " & vbNewLine &
                               " WHERE A.""Debit""<>0 " & vbNewLine &
                               " and F.""PrcCode""='" + PrcCode.ToString + "' " & vbNewLine &
                               " AND A.""Debit""='" + Value.ToString + "' " & vbNewLine &
                               " AND C.""DocEntry""='" + IncDocEntry.ToString + "' " & vbNewLine &
                               "  AND IFNULL(A.""OcrCode5"",'')='' "
                        Dim IncomingVchUpdt As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                        IncomingVchUpdt.DoQuery(qstr)
                    Next

                End If
                Return True
            Catch ex As Exception
                erMessage = ex.Message
                Return False
            End Try
        End Function

        'Public Function IncomingPaymentExchangeAdd(ByVal G_DI_Company As SAPbobsCOM.Company, ByVal CreditmemoDetails As DTS_MODEL_CRDT_HEADER, ByVal InvoiceEntry As String, ByVal UserId As String, ByVal Branch As String,
        '                       ByRef DpmEntry As String, ByRef erMessage As String, ByVal DiscountExists As Boolean) As Boolean
        '    Try
        '        Dim VoucherPaymentNo As String = ""
        '        rSet = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
        '        Dim qstr As String
        '        qstr = "Select Top 1 N.""Series"",B.""BPLId"",C.""WhsCode"" ""WhsCode"" ,TO_CHAR(NOW(),'YYYYYMMDD') ""DocDate"" " & vbNewLine &
        '                        "  From ""NNM1"" N  " & vbNewLine &
        '                        "       Inner Join ""OFPR"" O On O.""Indicator""=N.""Indicator"" " & vbNewLine &
        '                        "       Inner Join ""OBPL"" B On B.""BPLId""=N.""BPLId"" " & vbNewLine &
        '                        "       Inner Join ""OWHS"" C On C.""U_BUSUNIT""='" + Branch + "' AND C.""U_WHSTYPE""='N' " & vbNewLine &
        '                        " Where N.""ObjectCode""='24' " & vbNewLine &
        '                        "   And O.""PeriodStat"" In ('N','C') And N.""Locked""='N' AND N.""Remark""='" + Branch + "'  " & vbNewLine &
        '                        "   And TO_CHAR('" + CreditmemoDetails.PostingDate + "','YYYYYMMDD') Between TO_CHAR(O.""F_RefDate"",'YYYYYMMDD') And TO_CHAR(O.""T_RefDate"",'YYYYYMMDD')"

        '        Dim IncPayment As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
        '        IncPayment.DoQuery(qstr)
        '        If IncPayment.RecordCount > 0 Then
        '        Else
        '            erMessage = "Incoimng Payment Series not Found"
        '            Return False
        '        End If
        '        Dim InPay As SAPbobsCOM.Payments = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oIncomingPayments)

        '        qstr = "SELECT * FROM  ""OINV"" WHERE ""DocEntry""='" + InvoiceEntry.ToString + "'"
        '        Dim SoHrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
        '        SoHrSet.DoQuery(qstr)

        '        qstr = "SELECT * FROM  ""INV1"" WHERE ""DocEntry""='" + InvoiceEntry.ToString + "'"
        '        Dim SoDrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
        '        SoDrSet.DoQuery(qstr)
        '        InPay.CardCode = SoHrSet.Fields.Item("CardCode").Value
        '        InPay.UserFields.Fields.Item("U_BRANCH").Value = Branch
        '        InPay.UserFields.Fields.Item("U_CRTDBY").Value = UserId
        '        'InPay.DocDate = New Date(Mid(getServerDate, 1, 4), Mid(getServerDate, 5, 2), Mid(getServerDate, 7, 2))
        '        InPay.BPLID = SoHrSet.Fields.Item("BPLId").Value
        '        InPay.DocDate = New Date(Mid(CreditmemoDetails.PostingDate, 1, 4), Mid(CreditmemoDetails.PostingDate, 5, 2), Mid(CreditmemoDetails.PostingDate, 7, 2))
        '        InPay.Series = IncPayment.Fields.Item("Series").Value
        '        InPay.Invoices.DocEntry = Convert.ToInt32(InvoiceEntry)
        '        InPay.Invoices.InvoiceType = SAPbobsCOM.BoRcptInvTypes.it_Invoice
        '        InPay.Invoices.DistributionRule = Branch
        '        InPay.Invoices.Add()
        '        Dim ARDown As SAPbobsCOM.Documents = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices)
        '        ARDown = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices)
        '        ARDown.DocObjectCode = SAPbobsCOM.BoObjectTypes.oInvoices
        '        ARDown.GetByKey(Convert.ToInt32(InvoiceEntry))

        '        For Each Item As DTS_MODEL_PMNT_DTLS In CreditmemoDetails.PaymentDetails
        '            If Item.PaymentType = "S" Then
        '                InPay.CashSum = Item.Amount
        '                qstr = "SELECT ""OverCode"",""FrgnName"",""AcctCode"" FROM OACT  WHERE ""OverCode""='" + Branch + "' AND ""FrgnName""='S'"
        '                Dim CashPayment As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
        '                CashPayment.DoQuery(qstr)
        '                InPay.CashAccount = CashPayment.Fields.Item("AcctCode").Value

        '            End If

        '            If Item.PaymentType = "S" Then
        '            Else
        '                'qstr = "SELECT ""OverCode"",""FrgnName"",""AcctCode"" FROM OACT  WHERE ""OverCode""='" + Branch + "' AND ""FrgnName""='C'"
        '                qstr = "SELECT ""CreditCard""  FROM ""OCRC"" where ""U_BANKCODE""='" + Item.Bank + "' AND ""U_PMNTP""='" + Item.PaymentType + "'"
        '                Dim CreditPayment As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
        '                CreditPayment.DoQuery(qstr)
        '                If CreditPayment.RecordCount > 0 Then
        '                    InPay.CreditCards.CreditCard = CreditPayment.Fields.Item("CreditCard").Value
        '                    If Item.PaymentType = "2" Or Item.PaymentType = "8" Then
        '                        If DiscountExists = True Then
        '                            erMessage = "Discount can not be done for Voucher type Payment please Remove Item Discount"
        '                            Return False
        '                        End If
        '                        qstr = "SELECT ""PrcCode"",""PrcName"" FROM ""OPRC"" WHERE ""PrcCode""='" + Item.CardNo + "' and ""DimCode""='5' and ""U_CARDCODE""='" + SoHrSet.Fields.Item("CardCode").Value + "'"
        '                        Dim VouchNameStr As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
        '                        VouchNameStr.DoQuery(qstr)
        '                        If VouchNameStr.RecordCount > 0 Then
        '                            InPay.CreditCards.CreditCardNumber = VouchNameStr.Fields.Item("PrcCode").Value
        '                            InPay.CreditCards.UserFields.Fields.Item("U_CARDNO").Value = VouchNameStr.Fields.Item("PrcCode").Value
        '                            VoucherPaymentNo = IIf(VoucherPaymentNo = "", Item.CardNo + ":" + Item.Amount.ToString, VoucherPaymentNo + ";" + Item.CardNo + ":" + Item.Amount.ToString)
        '                        Else
        '                            erMessage = "No Voucher found for " + Item.CardNo
        '                            Return False
        '                        End If

        '                    Else
        '                        Try
        '                            InPay.CreditCards.CreditCardNumber = IIf(Item.CardNo Is Nothing, "1111", Right(Item.CardNo.ToString, 4))
        '                            InPay.CreditCards.UserFields.Fields.Item("U_CARDNO").Value = IIf(Item.CardNo Is Nothing, "1111", Item.CardNo.ToString)
        '                        Catch ex As Exception
        '                            InPay.CreditCards.CreditCardNumber = "1111"
        '                            InPay.CreditCards.UserFields.Fields.Item("U_CARDNO").Value = "1111"
        '                        End Try
        '                    End If
        '                    'IIf(Item.CardNo.ToString = "", "1111", Right(Item.CardNo.ToString, 4))

        '                    InPay.CreditCards.CardValidUntil = New Date(Mid("29991231", 1, 4), Mid("29991231", 5, 2), Mid("29991231", 7, 2))
        '                    InPay.CreditCards.PaymentMethodCode = 1
        '                    InPay.CreditCards.CreditSum = Item.Amount
        '                    'InPay.CreditCards.FirstPaymentSum = CreditCards.CardAmount
        '                    Try
        '                        InPay.CreditCards.VoucherNum = IIf(Item.Tranid Is Nothing, "111", Item.Tranid.ToString)
        '                    Catch ex As Exception
        '                        InPay.CreditCards.VoucherNum = "111"
        '                    End Try

        '                    InPay.CreditCards.CreditType = SAPbobsCOM.BoRcptCredTypes.cr_Regular
        '                    InPay.CreditCards.Add()
        '                Else
        '                    erMessage = "No Payment Method found for " + Item.Bank + " and " + Item.PaymentType
        '                    Return False
        '                End If

        '            End If
        '        Next
        '        Dim lRetCode As Integer = InPay.Add
        '        If lRetCode <> 0 Then
        '            Dim sErrMsg As String = G_DI_Company.GetLastErrorDescription()
        '            G_DI_Company.GetLastError(lRetCode, sErrMsg)
        '            erMessage = sErrMsg
        '            System.Runtime.InteropServices.Marshal.ReleaseComObject(InPay)
        '            Return False
        '        End If
        '        Dim IncDocEntry = G_DI_Company.GetNewObjectKey.Trim.ToString
        '        If VoucherPaymentNo <> "" Then
        '            For Each Dtls As String In VoucherPaymentNo.Split(New String() {";"}, StringSplitOptions.None)
        '                Dim PrcCode = Dtls.Split(":")(0)
        '                Dim Value As Decimal = Dtls.Split(":")(1)

        '                qstr = "UPDATE A SET ""OcrCode5""='" + PrcCode.ToString + "' " & vbNewLine &
        '                       " FROM ""JDT1"" A " & vbNewLine &
        '                       "    INNER JOIN ""OJDT"" B ON A.""TransId""=B.""TransId"" " & vbNewLine &
        '                       "    INNER JOIN ""ORCT"" C ON C.""TransId""=B.""TransId"" AND C.""Canceled""='N' --AND C.""CardCode"" =A.""ShortName"" " & vbNewLine &
        '                       "    INNER JOIN ""OACT"" E ON E.""AcctCode""=A.""Account"" AND E.""FrgnName""='VS' " & vbNewLine &
        '                       "    INNER JOIN ""OPRC"" F ON F.""PrcCode""='" + PrcCode.ToString + "' " & vbNewLine &
        '                       " WHERE A.""Debit""<>0 " & vbNewLine &
        '                       " and F.""PrcCode""='" + PrcCode.ToString + "' " & vbNewLine &
        '                       " AND A.""Debit""='" + Value.ToString + "' " & vbNewLine &
        '                       " AND C.""DocEntry""='" + IncDocEntry.ToString + "' " & vbNewLine &
        '                       "  AND IFNULL(A.""OcrCode5"",'')='' "
        '                Dim IncomingVchUpdt As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
        '                IncomingVchUpdt.DoQuery(qstr)
        '            Next

        '        End If
        '        Return True
        '    Catch ex As Exception
        '        erMessage = ex.Message
        '        Return False
        '    End Try
        'End Function

        <Route("Api/PostServiceExchangeWithPayment")>
        <HttpPost>
        Public Function PostServiceExchangeWithPayment(ByVal OrderDetails As DTS_MODEL_SOUPRDER_HEADER) As HttpResponseMessage
            Dim G_DI_Company As SAPbobsCOM.Company = Nothing
            Dim strRegnNo As String = ""
            dtTable.Columns.Add("ReturnCode")
            dtTable.Columns.Add("ReturnDocEntry")
            dtTable.Columns.Add("ReturnObjType")
            dtTable.Columns.Add("ReturnSeries")
            dtTable.Columns.Add("ReturnDocNum")
            dtTable.Columns.Add("ReturnMsg")
            Try
                Dim Branch As String = ""
                Dim UserID As String = ""


                If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                    Return Request.CreateResponse(HttpStatusCode.OK, dtError)
                End If
                Dim qstr As String = ""
                'Dim qstr As String = ""
                qstr = "SELECT ""U_SAPUNAME"" ,""U_SAPPWD"" " & vbNewLine &
                       " FROM ""OUSR"" A " & vbNewLine &
                       " WHERE A.""USER_CODE"" ='" + UserID + "'"
                Dim dRow As Data.DataRow
                dRow = DBSQL.getQueryDataRow(qstr, "")
                Dim SAPUserId As String = ""
                Dim SAPPassword As String = ""
                Try
                    SAPUserId = dRow.Item("U_SAPUNAME")
                    SAPPassword = dRow.Item("U_SAPPWD")
                Catch ex As Exception
                End Try
                Connection.ConnectSAP(G_DI_Company, SAPUserId, SAPPassword)
                rSet = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)

                qstr = "SELECT TO_CHAR(""DocDate"",'YYYYMMDD') ""DocDate"",""U_TOBUNIT"" FROM ""ORDR"" WHERE ""DocEntry""='" + OrderDetails.BaseEntry + "'"
                rSet.DoQuery(qstr)
                qstr = "SELECT ""WhsCode"" ""WhsCode"" FROM ""OWHS"" WHERE ""U_BUSUNIT""='" + rSet.Fields.Item("U_TOBUNIT").Value + "' AND ""U_WHSTYPE""='N' "
                Dim NWHSRrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                NWHSRrSet.DoQuery(qstr)
                Dim Whscode As String = NWHSRrSet.Fields.Item("WhsCode").Value
                qstr = "SELECT ""WhsCode"" ""WhsCode"" FROM ""OWHS"" WHERE ""U_BUSUNIT""='" + rSet.Fields.Item("U_TOBUNIT").Value + "' AND  ""U_WHSTYPE""='W' "
                Dim WIWHSRrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                WIWHSRrSet.DoQuery(qstr)
                Dim WIPWhscode As String = WIWHSRrSet.Fields.Item("WhsCode").Value
                'qstr = "Select Top 1 N.""Series"",B.""BPLId"",D.""WhsCode"" ""WhsCode"",E.""WhsCode"" ""WIPWhsCode"" ,TO_CHAR(NOW(),'YYYYYMMDD') ""DocDate"" " & vbNewLine &
                '                "  From ""NNM1"" N  " & vbNewLine &
                '                "       Inner Join ""OFPR"" O On O.""Indicator""=N.""Indicator"" " & vbNewLine &
                '                "       Inner Join ""OBPL"" B On B.""BPLId""=N.""BPLId"" " & vbNewLine &
                '                "       Inner Join ""OWHS"" C On C.""U_BUSUNIT""='" + Branch + "' AND C.""U_WHSTYPE""='N' " & vbNewLine &
                '                "       LEFT OUTER Join ""OWHS"" D On D.""U_BUSUNIT""='" + rSet.Fields.Item("U_TOBUNIT").Value + "' AND D.""U_WHSTYPE""='N' " & vbNewLine &
                '                "       LEFT OUTER Join ""OWHS"" E On E.""U_BUSUNIT""='" + rSet.Fields.Item("U_TOBUNIT").Value + "' AND E.""U_WHSTYPE""='W' " & vbNewLine &
                '                " Where N.""ObjectCode""='17' " & vbNewLine &
                '                "   And O.""PeriodStat"" In ('N','C') And N.""Locked""='N' AND N.""Remark""='" + Branch + "'  " & vbNewLine &
                '                "   And TO_CHAR('" + rSet.Fields.Item("DocDate").Value + "','YYYYYMMDD') Between TO_CHAR(O.""F_RefDate"",'YYYYYMMDD') And TO_CHAR(O.""T_RefDate"",'YYYYYMMDD')"
                'rSet.DoQuery(qstr)

                'Dim WIPWhscode As String = rSet.Fields.Item("WIPWhsCode").Value
                Dim SalesOrder As SAPbobsCOM.Documents = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oOrders)
                SalesOrder.DocObjectCode = SAPbobsCOM.BoObjectTypes.oOrders
                Dim VisOreder As String = ""
                If SalesOrder.GetByKey(OrderDetails.BaseEntry) Then
                    G_DI_Company.StartTransaction()
                    Dim DiscountExists As Boolean = False
                    SalesOrder.UserFields.Fields.Item("U_UPDTBY").Value = UserID
                    SalesOrder.UserFields.Fields.Item("U_FRMPORT").Value = "Y"
                    For Each Item As DTS_MODEL_SOUPRDER_ITEMS In OrderDetails.Items
                        If Item.Type = "C" Then
                            'qstr = "SELECT A.""ItemCode"",A.""WhsCode"",A.""LineTotal"",A.""OpenQty"", " & vbNewLine &
                            '      "    A.""TaxCode"",IFNULL(A.""UseBaseUn"",'') ""UseBaseUn"" ,IFNULL(A.""U_DISCPER"",0) ""U_DISCPER"",A.""U_SEQNO"", " & vbNewLine &
                            '      "    TO_CHAR(A.""ShipDate"",'YYYYMMDD') ""ShipDate"", " & vbNewLine &
                            '      "    IFNULL(B.""LineTotal"",0) ""DisCountAmount"" " & vbNewLine &
                            '      " FROM ""RDR1"" A " & vbNewLine &
                            '      "    LEFT OUTER JOIN ""RDR2"" B ON A.""DocEntry""=B.""DocEntry"" AND A.""LineNum""=B.""LineNum"" AND B.""ExpnsCode""=1 " & vbNewLine &
                            '      " WHERE A.""DocEntry""='" + Item.BaseEntry.ToString + "' " & vbNewLine &
                            '      "        AND A.""LineNum""='" + Item.BaseLine.ToString + "' " & vbNewLine &
                            '      "        AND A.""LineStatus""='O'  "
                            'Dim SoItemrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                            'SoItemrSet.DoQuery(qstr)
                            qstr = "SELECT A.""ItemCode"",A.""WhsCode"",A.""LineTotal"",A.""OpenQty"", " & vbNewLine &
                                  "    A.""TaxCode"",IFNULL(A.""UseBaseUn"",'') ""UseBaseUn"" ,IFNULL(A.""DiscPrcnt"",0) ""DiscPer"",A.""U_SEQNO"", " & vbNewLine &
                                  "    TO_CHAR(A.""ShipDate"",'YYYYMMDD') ""ShipDate"", " & vbNewLine &
                                  "    IFNULL(A.""U_DISCPER"",0) ""DisCountAmount"" " & vbNewLine &
                                  " FROM ""RDR1"" A " & vbNewLine &
                                  "    LEFT OUTER JOIN ""RDR2"" B ON A.""DocEntry""=B.""DocEntry"" AND A.""LineNum""=B.""LineNum"" AND B.""ExpnsCode""=1 " & vbNewLine &
                                  " WHERE A.""DocEntry""='" + Item.BaseEntry.ToString + "' " & vbNewLine &
                                  "        AND A.""LineNum""='" + Item.BaseLine.ToString + "' " & vbNewLine &
                                  "        AND A.""LineStatus""='O'  "
                            Dim SoItemrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                            SoItemrSet.DoQuery(qstr)

                            While Not SoItemrSet.EoF
                                SalesOrder.Lines.Add()
                                SalesOrder.Lines.ItemCode = SoItemrSet.Fields.Item("ItemCode").Value.ToString
                                SalesOrder.Lines.WarehouseCode = SoItemrSet.Fields.Item("WhsCode").Value.ToString
                                SalesOrder.Lines.UnitPrice = SoItemrSet.Fields.Item("LineTotal").Value.ToString
                                SalesOrder.Lines.Quantity = (SoItemrSet.Fields.Item("OpenQty").Value.ToString) * (-1)
                                SalesOrder.Lines.TaxCode = SoItemrSet.Fields.Item("TaxCode").Value.ToString
                                SalesOrder.Lines.MeasureUnit = SoItemrSet.Fields.Item("UseBaseUn").Value.ToString
                                SalesOrder.Lines.DiscountPercent = SoItemrSet.Fields.Item("DiscPer").Value.ToString
                                SalesOrder.Lines.UserFields.Fields.Item("U_DISCPER").Value = SoItemrSet.Fields.Item("DisCountAmount").Value.ToString
                                SalesOrder.Lines.UserFields.Fields.Item("U_SEQNO").Value = SoItemrSet.Fields.Item("U_SEQNO").Value.ToString
                                SalesOrder.Lines.UserFields.Fields.Item("U_CRDBSLIN").Value = Item.BaseLine
                                SalesOrder.Lines.ShipDate = New Date(Mid(SoItemrSet.Fields.Item("ShipDate").Value.ToString, 1, 4), Mid(SoItemrSet.Fields.Item("ShipDate").Value.ToString, 5, 2), Mid(SoItemrSet.Fields.Item("ShipDate").Value.ToString, 7, 2))
                                SalesOrder.Lines.CostingCode = Branch

                                If CType(SoItemrSet.Fields.Item("DisCountAmount").Value.ToString, Decimal) <> 0 Then
                                    DiscountExists = True
                                    'SalesOrder.Lines.Expenses.LineTotal = (SoItemrSet.Fields.Item("DisCountAmount").Value.ToString) * (-1)
                                    'SalesOrder.Lines.Expenses.ExpenseCode = 1
                                    'SalesOrder.Lines.Expenses.Add()
                                End If



                                'SalesOrder.Lines.LineStatus = SAPbobsCOM.BoStatus.bost_Close

                                SoItemrSet.MoveNext()
                            End While
                            'SalesOrder.Lines.ItemCode = Item.ItemCode
                            'SalesOrder.Lines.WarehouseCode = Item.WareHouse
                            'SalesOrder.Lines.UnitPrice = Item.PriceBeforeDiscount
                            'SalesOrder.Lines.Quantity = Item.Quantity * (-1)
                            'SalesOrder.Lines.TaxCode = Item.TaxCode
                            'SalesOrder.Lines.MeasureUnit = Item.UOM
                            'SalesOrder.Lines.UserFields.Fields.Item("U_DISCPER").Value = Item.DiscountPercentage.ToString
                            'SalesOrder.Lines.UserFields.Fields.Item("U_CRDBSLIN").Value = Item.BaseLine.ToString
                            'SalesOrder.Lines.CostingCode = Branch
                            'SalesOrder.Lines.UserFields.Fields.Item("U_SEQNO").Value = Item.SequenceNo
                            'SalesOrder.Lines.ShipDate = New Date(Mid("20991231", 1, 4), Mid("20991231", 5, 2), Mid("20991231", 7, 2))

                            'If CType(Item.Discountamount, Decimal) <> 0 Then
                            '    SalesOrder.Lines.Expenses.LineTotal = Item.Discountamount.ToString * (-1)
                            '    SalesOrder.Lines.Expenses.ExpenseCode = 1
                            '    SalesOrder.Lines.Expenses.Add()
                            'End If

                        Else
                            SalesOrder.Lines.Add()
                            SalesOrder.Lines.ItemCode = Item.ItemCode
                            'new API
                            SalesOrder.Lines.WarehouseCode = Item.WhsCode
                            SalesOrder.Lines.UnitPrice = Item.PriceBeforeDiscount
                            SalesOrder.Lines.Quantity = Item.Quantity
                            SalesOrder.Lines.TaxCode = Item.TaxCode
                            SalesOrder.Lines.MeasureUnit = Item.UOM
                            SalesOrder.Lines.DiscountPercent = Item.DiscountPercentage
                            SalesOrder.Lines.UserFields.Fields.Item("U_DISCPER").Value = Item.Discountamount.ToString
                            SalesOrder.Lines.CostingCode = Branch
                            SalesOrder.Lines.UserFields.Fields.Item("U_SEQNO").Value = Item.SequenceNo
                            SalesOrder.Lines.ShipDate = New Date(Mid(Item.DocDueDate, 1, 4), Mid(Item.DocDueDate, 5, 2), Mid(Item.DocDueDate, 7, 2))
                            SalesOrder.Lines.SerialNum = "Exchange Line"
                            If CType(Item.Discountamount, Decimal) <> 0 Then
                                DiscountExists = True
                                'SalesOrder.Lines.Expenses.LineTotal = Item.Discountamount.ToString * (-1)
                                'SalesOrder.Lines.Expenses.ExpenseCode = 1
                                'SalesOrder.Lines.Expenses.Add()
                            End If

                        End If
                    Next

                    Dim lRetCode As Integer
                    lRetCode = SalesOrder.Update

                    Dim ErrorMessage As String = ""
                    If lRetCode <> 0 Then
                        Dim sErrMsg As String = G_DI_Company.GetLastErrorDescription()
                        G_DI_Company.GetLastError(lRetCode, sErrMsg)
                        ErrorMessage = sErrMsg
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(SalesOrder)
                        If G_DI_Company.InTransaction Then
                            G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                        End If
                        G_DI_Company.Disconnect()
                        NewRow = dtTable.NewRow
                        NewRow.Item("ReturnCode") = "-3333"
                        NewRow.Item("ReturnDocEntry") = "-1"
                        NewRow.Item("ReturnObjType") = "-1"
                        NewRow.Item("ReturnSeries") = "-1"
                        NewRow.Item("ReturnDocNum") = "-1"
                        NewRow.Item("ReturnMsg") = ErrorMessage
                        dtTable.Rows.Add(NewRow)
                        Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                    Else
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(SalesOrder)
                        Dim ObjType As String = G_DI_Company.GetNewObjectType.ToString.TrimEnd
                        Dim SOEntry As String = G_DI_Company.GetNewObjectKey.Trim.ToString
                        Dim ErrMsg As String = ""
                        qstr = "UPDATE ""RDR1"" SET ""OcrCode""='" + Branch + "',""CogsOcrCod""='" + Branch + "',""WhsCode""='" + Whscode + "' WHERE ""DocEntry""='" + SOEntry + "' and IFNULL(""OcrCode"",'')='' "
                        rSet.DoQuery(qstr)
                        qstr = "Select ""VisOrder"",""LineNum"",""ItemCode"",IFNULL(""U_CRDBSLIN"",-1)""U_CRDBSLIN"",""U_SEQNO"",TO_CHAR(""ShipDate"",'YYYYMMDD') ""ShipDate"" FROM ""RDR1"" WHERE ""DocEntry""='" + SOEntry + "' ORDER BY ""VisOrder"" "
                        Dim SoLinerSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                        SoLinerSet.DoQuery(qstr)
                        While Not SoLinerSet.EoF
                            qstr = "SELECT ""Code"" FROM ""ITT1"" WHERE ""Father""='" + SoLinerSet.Fields.Item("ItemCode").Value + "'"
                            Dim SoItemrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                            SoItemrSet.DoQuery(qstr)
                            If SoItemrSet.RecordCount > 0 Then
                                While Not SoItemrSet.EoF
                                    qstr = "Select ""VisOrder"",""ItemCode"",TO_CHAR(""ShipDate"",'YYYYMMDD') ""ShipDate"" FROM ""RDR1"" WHERE ""DocEntry""='" + SOEntry + "' AND ""ItemCode""='" + SoItemrSet.Fields.Item("Code").Value + "'  AND ""VisOrder"">" + SoLinerSet.Fields.Item("VisOrder").Value.ToString + " ORDER BY ""VisOrder"" "
                                    Dim SoItemrSet1 As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                                    SoItemrSet1.DoQuery(qstr)

                                    'qstr = "UPDATE ""RDR1"" SET ""U_SERVLINE""='" + SoLinerSet.Fields.Item("LineNum").Value.ToString + "', " & vbNewLine &
                                    '        " ""U_CRDBSLIN""='" + SoLinerSet.Fields.Item("U_CRDBSLIN").Value.ToString + "' " & vbNewLine &
                                    '       "    ""WhsCode""='" + WIPWhscode + "',""U_ITEMHIDE""='Y', " & vbNewLine &
                                    '       "    ""U_SEQNO""='" + SoLinerSet.Fields.Item("U_SEQNO").Value.ToString + "', " & vbNewLine &
                                    '       "    ""ShipDate""='" + SoLinerSet.Fields.Item("ShipDate").Value + "' " & vbNewLine &
                                    '       " WHERE ""DocEntry""='" + SOEntry + "' " & vbNewLine &
                                    '       "    And ""VisOrder""='" + SoItemrSet1.Fields.Item("VisOrder").Value.ToString + "' " 
                                    'rSet.DoQuery(qstr)

                                    qstr = "UPDATE ""RDR1"" SET ""U_SERVLINE""='" + SoLinerSet.Fields.Item("LineNum").Value.ToString + "', " & vbNewLine &
                                            " ""U_CRDBSLIN""='" + SoLinerSet.Fields.Item("U_CRDBSLIN").Value.ToString + "', " & vbNewLine &
                                           "    ""WhsCode""='" + WIPWhscode + "',""U_ITEMHIDE""='Y', " & vbNewLine &
                                           "    ""U_SEQNO""='" + SoLinerSet.Fields.Item("U_SEQNO").Value.ToString + "', " & vbNewLine &
                                           "    ""ShipDate""='" + SoLinerSet.Fields.Item("ShipDate").Value + "' " & vbNewLine &
                                           " WHERE ""DocEntry""='" + SOEntry + "' " & vbNewLine &
                                           "    And ""VisOrder""='" + SoItemrSet1.Fields.Item("VisOrder").Value.ToString + "' " & vbNewLine &
                                           "    AND IFNULL(""U_SERVLINE"",-1)=-1"
                                    rSet.DoQuery(qstr)

                                    SoItemrSet.MoveNext()
                                End While
                            Else
                                'Continue While
                            End If
                            SoLinerSet.MoveNext()
                        End While

                        Dim SalesOrder5 As SAPbobsCOM.Documents = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oOrders)
                        SalesOrder5.DocObjectCode = SAPbobsCOM.BoObjectTypes.oOrders
                        If SalesOrder5.GetByKey(SOEntry) Then
                            qstr = "SELECT * FROM ""RDR1"" WHERE ""DocEntry""='" + OrderDetails.BaseEntry.ToString + "' AND ""LineStatus""='O' AND IFNULL(""U_ITEMHIDE"",'N')='Y' "
                            rSet.DoQuery(qstr)
                            If rSet.RecordCount > 0 Then
                                While Not rSet.EoF
                                    SalesOrder5.Lines.SetCurrentLine(rSet.Fields.Item("VisOrder").Value)
                                    SalesOrder5.Lines.UnitPrice = 0
                                    'SalesOrder2.Lines.LineStatus = SAPbobsCOM.BoStatus.bost_Close

                                    rSet.MoveNext()
                                End While
                                Dim lRetCode1 As Integer = SalesOrder5.Update
                                If lRetCode1 <> 0 Then
                                    Dim sErrMsg As String = G_DI_Company.GetLastErrorDescription()
                                    G_DI_Company.GetLastError(lRetCode, sErrMsg)
                                    ErrorMessage = sErrMsg
                                    System.Runtime.InteropServices.Marshal.ReleaseComObject(SalesOrder5)
                                    If G_DI_Company.InTransaction Then
                                        G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                                    End If
                                    G_DI_Company.Disconnect()
                                    NewRow = dtTable.NewRow
                                    NewRow.Item("ReturnCode") = "-5555" + WIPWhscode
                                    NewRow.Item("ReturnDocEntry") = "-1"
                                    NewRow.Item("ReturnObjType") = "-1"
                                    NewRow.Item("ReturnSeries") = "-1"
                                    NewRow.Item("ReturnDocNum") = "-1"
                                    NewRow.Item("ReturnMsg") = ErrorMessage
                                    dtTable.Rows.Add(NewRow)
                                    Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                                End If
                            End If
                        Else
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(SalesOrder5)
                        End If

                        Dim SalesOrder1 As SAPbobsCOM.Documents = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oOrders)
                        SalesOrder1.DocObjectCode = SAPbobsCOM.BoObjectTypes.oOrders
                        If SalesOrder1.GetByKey(OrderDetails.BaseEntry) Then
                            qstr = "SELECT A.""VisOrder"" " & vbNewLine &
                                  " FROM ""RDR1"" A " & vbNewLine &
                                  " WHERE A.""DocEntry""='" + OrderDetails.BaseEntry.ToString + "' " & vbNewLine &
                                  "        AND A.""U_SERVLINE"" in (Select ""U_CRDBSLIN"" from ""RDR1"" where ""DocEntry""='" + OrderDetails.BaseEntry.ToString + "') " & vbNewLine &
                                  "        AND A.""LineStatus""='O' "
                            Dim SoItemrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                            SoItemrSet.DoQuery(qstr)
                            While Not SoItemrSet.EoF
                                SalesOrder1.Lines.SetCurrentLine(SoItemrSet.Fields.Item("VisOrder").Value.ToString)
                                SalesOrder1.Lines.LineStatus = SAPbobsCOM.BoStatus.bost_Close
                                SoItemrSet.MoveNext()
                            End While
                            'For Each Item As DTS_MODEL_SOUPRDER_ITEMS In OrderDetails.Items
                            '    SalesOrder1.Lines.SetCurrentLine(Item.VisOrder)
                            '    SalesOrder1.Lines.LineStatus = SAPbobsCOM.BoStatus.bost_Close

                            'Next
                            Dim lRetCode1 As Integer = SalesOrder1.Update
                            If lRetCode1 <> 0 Then
                                Dim sErrMsg As String = G_DI_Company.GetLastErrorDescription()
                                G_DI_Company.GetLastError(lRetCode, sErrMsg)
                                ErrorMessage = sErrMsg
                                System.Runtime.InteropServices.Marshal.ReleaseComObject(SalesOrder1)
                                If G_DI_Company.InTransaction Then
                                    G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                                End If
                                G_DI_Company.Disconnect()
                                NewRow = dtTable.NewRow
                                NewRow.Item("ReturnCode") = "-6666"
                                NewRow.Item("ReturnDocEntry") = "-1"
                                NewRow.Item("ReturnObjType") = "-1"
                                NewRow.Item("ReturnSeries") = "-1"
                                NewRow.Item("ReturnDocNum") = "-1"
                                NewRow.Item("ReturnMsg") = ErrorMessage
                                dtTable.Rows.Add(NewRow)
                                Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                            End If
                        Else
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(SalesOrder1)
                        End If
                        Dim SalesOrder2 As SAPbobsCOM.Documents = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oOrders)
                        SalesOrder2.DocObjectCode = SAPbobsCOM.BoObjectTypes.oOrders
                        If SalesOrder2.GetByKey(OrderDetails.BaseEntry) Then
                            qstr = "SELECT * FROM ""RDR1"" WHERE ""DocEntry""='" + OrderDetails.BaseEntry.ToString + "' AND ""LineStatus""='O' AND IFNULL(""U_CRDBSLIN"",-1)<>-1 "
                            rSet.DoQuery(qstr)
                            If rSet.RecordCount > 0 Then
                                While Not rSet.EoF
                                    SalesOrder2.Lines.SetCurrentLine(rSet.Fields.Item("VisOrder").Value)
                                    If CType(rSet.Fields.Item("Quantity").Value, Decimal) > 0 Then
                                        SalesOrder2.Lines.Quantity = rSet.Fields.Item("Quantity").Value * (-1)
                                    End If

                                    'SalesOrder2.Lines.LineStatus = SAPbobsCOM.BoStatus.bost_Close

                                    rSet.MoveNext()
                                End While
                                Dim lRetCode1 As Integer = SalesOrder2.Update
                                If lRetCode1 <> 0 Then
                                    Dim sErrMsg As String = G_DI_Company.GetLastErrorDescription()
                                    G_DI_Company.GetLastError(lRetCode, sErrMsg)
                                    ErrorMessage = sErrMsg
                                    System.Runtime.InteropServices.Marshal.ReleaseComObject(SalesOrder2)
                                    If G_DI_Company.InTransaction Then
                                        G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                                    End If
                                    G_DI_Company.Disconnect()
                                    NewRow = dtTable.NewRow
                                    NewRow.Item("ReturnCode") = "-7777"
                                    NewRow.Item("ReturnDocEntry") = "-1"
                                    NewRow.Item("ReturnObjType") = "-1"
                                    NewRow.Item("ReturnSeries") = "-1"
                                    NewRow.Item("ReturnDocNum") = "-1"
                                    NewRow.Item("ReturnMsg") = ErrorMessage
                                    dtTable.Rows.Add(NewRow)
                                    Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                                End If
                            End If
                        Else
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(SalesOrder2)
                        End If

                        Dim SalesOrder3 As SAPbobsCOM.Documents = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oOrders)
                        SalesOrder3.DocObjectCode = SAPbobsCOM.BoObjectTypes.oOrders
                        If SalesOrder3.GetByKey(OrderDetails.BaseEntry) Then
                            qstr = "SELECT * FROM ""RDR1"" WHERE ""DocEntry""='" + OrderDetails.BaseEntry.ToString + "' AND ""LineStatus""='O' AND IFNULL(""U_CRDBSLIN"",-1)<>-1 " & vbNewLine &
                                    "UNION ALL " & vbNewLine &
                                    "SELECT * FROM ""RDR1"" WHERE ""DocEntry""='" + OrderDetails.BaseEntry.ToString + "' AND ""LineStatus""='O' AND ""LineNum"" in (select  ""U_CRDBSLIN"" from ""RDR1"" WHERE ""DocEntry""='" + OrderDetails.BaseEntry.ToString + "' and IFNULL(""U_CRDBSLIN"",-1)<>'-1') "
                            rSet.DoQuery(qstr)
                            If rSet.RecordCount > 0 Then
                                While Not rSet.EoF
                                    SalesOrder3.Lines.SetCurrentLine(rSet.Fields.Item("VisOrder").Value)

                                    SalesOrder3.Lines.LineStatus = SAPbobsCOM.BoStatus.bost_Close

                                    rSet.MoveNext()
                                End While
                                Dim lRetCode1 As Integer = SalesOrder3.Update
                                If lRetCode1 <> 0 Then
                                    Dim sErrMsg As String = G_DI_Company.GetLastErrorDescription()
                                    G_DI_Company.GetLastError(lRetCode, sErrMsg)
                                    ErrorMessage = sErrMsg
                                    System.Runtime.InteropServices.Marshal.ReleaseComObject(SalesOrder3)
                                    If G_DI_Company.InTransaction Then
                                        G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                                    End If
                                    G_DI_Company.Disconnect()
                                    NewRow = dtTable.NewRow
                                    NewRow.Item("ReturnCode") = "-8888"
                                    NewRow.Item("ReturnDocEntry") = "-1"
                                    NewRow.Item("ReturnObjType") = "-1"
                                    NewRow.Item("ReturnSeries") = "-1"
                                    NewRow.Item("ReturnDocNum") = "-1"
                                    NewRow.Item("ReturnMsg") = ErrorMessage
                                    dtTable.Rows.Add(NewRow)
                                    Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                                End If
                            End If
                        Else
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(SalesOrder2)
                        End If

                        If ObjType = "112" Then
                            qstr = "SELECT B.""SeriesName"",CAST(A.""DocNum"" AS VARCHAR) ""DocNum"",B.""SeriesName""||'/'||CAST(A.""DocNum"" AS VARCHAR) ""StrDocNum"" " & vbNewLine &
                           " From ""ODRF"" A " & vbNewLine &
                           " INNER JOIN ""NNM1"" B ON B.""ObjectCode""=A.""ObjType"" AND B.""Series""=A.""Series"" " & vbNewLine &
                           " WHERE A.""DocEntry""='" + SOEntry + "' "
                        ElseIf ObjType = "17" Then
                            qstr = "SELECT B.""SeriesName"",CAST(A.""DocNum"" AS VARCHAR) ""DocNum"",B.""SeriesName""||'/'||CAST(A.""DocNum"" AS VARCHAR) ""StrDocNum"" " & vbNewLine &
                           " From ""ORDR"" A " & vbNewLine &
                           " INNER JOIN ""NNM1"" B ON B.""ObjectCode""=A.""ObjType"" AND B.""Series""=A.""Series"" " & vbNewLine &
                           " WHERE A.""DocEntry""='" + SOEntry + "' "
                        End If
                        rSet.DoQuery(qstr)
                        Dim ReturnDocNo = rSet.Fields.Item("StrDocNum").Value
                        Dim SeriesName = rSet.Fields.Item("SeriesName").Value
                        Dim DocNum = rSet.Fields.Item("DocNum").Value
                        Dim DPMEntry As String = ""
                        Dim ermsg As String = ""
                        Try
                            If OrderDetails.ExcessAmount > 0 Then
                                If DPMUpdtOrdrAdd(G_DI_Company, OrderDetails, SOEntry, UserID, Branch, DiscountExists, DPMEntry, ermsg) = False Then
                                    If G_DI_Company.InTransaction Then
                                        G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                                    End If

                                    G_DI_Company.Disconnect()
                                    NewRow = dtTable.NewRow
                                    NewRow.Item("ReturnCode") = "-9999"
                                    NewRow.Item("ReturnDocEntry") = "-1"
                                    NewRow.Item("ReturnObjType") = "-1"
                                    NewRow.Item("ReturnSeries") = "-1"
                                    NewRow.Item("ReturnDocNum") = "-1"
                                    NewRow.Item("ReturnMsg") = ermsg
                                    dtTable.Rows.Add(NewRow)
                                    Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                                End If
                            End If

                            'End If
                        Catch ex As Exception
                            If G_DI_Company.InTransaction Then
                                G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                            End If

                            G_DI_Company.Disconnect()
                            NewRow = dtTable.NewRow
                            NewRow.Item("ReturnCode") = "-10000"
                            NewRow.Item("ReturnDocEntry") = "-1"
                            NewRow.Item("ReturnObjType") = "-1"
                            NewRow.Item("ReturnSeries") = "-1"
                            NewRow.Item("ReturnDocNum") = "-1"
                            NewRow.Item("ReturnMsg") = ex.Message
                            dtTable.Rows.Add(NewRow)
                            Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                        End Try

                        G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit)
                        G_DI_Company.Disconnect()
                        NewRow = dtTable.NewRow
                        NewRow.Item("ReturnCode") = "0000"
                        NewRow.Item("ReturnDocEntry") = SOEntry
                        NewRow.Item("ReturnObjType") = ObjType
                        NewRow.Item("ReturnSeries") = SeriesName
                        NewRow.Item("ReturnDocNum") = DocNum
                        NewRow.Item("ReturnMsg") = "Your Request No. " + ReturnDocNo + " successfully submitted"
                        dtTable.Rows.Add(NewRow)
                        Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                    End If
                End If
            Catch ex As Exception
                Try
                    If G_DI_Company.InTransaction Then
                        G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                    End If
                Catch ex2 As Exception
                End Try
                Try
                    G_DI_Company.Disconnect()
                Catch ex1 As Exception
                End Try

                NewRow = dtTable.NewRow
                NewRow.Item("ReturnCode") = "-2222"
                NewRow.Item("ReturnDocEntry") = "-1"
                NewRow.Item("ReturnObjType") = "-1"
                NewRow.Item("ReturnSeries") = "-1"
                NewRow.Item("ReturnDocNum") = "-1"
                NewRow.Item("ReturnMsg") = ex.Message
                dtTable.Rows.Add(NewRow)
                Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
            End Try

        End Function

        Public Function DPMUpdtOrdrAdd(ByVal G_DI_Company As SAPbobsCOM.Company, ByVal OrderDetails As DTS_MODEL_SOUPRDER_HEADER, ByVal SalesOrderEntry As String, ByVal UserId As String, ByVal Branch As String,
                               ByVal DiscountExists As Boolean, ByRef DpmEntry As String, ByRef erMessage As String) As Boolean
            Try
                rSet = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                Dim qstr As String
                qstr = "SELECT TO_CHAR(""DocDate"",'YYYYMMDD') ""DocDate"",TO_CHAR(""TaxDate"",'YYYYMMDD') ""TaxDate"",""NumAtCard"" FROM  ""ORDR"" WHERE ""DocEntry""='" + SalesOrderEntry.ToString + "'"
                Dim SoHrSet1 As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                SoHrSet1.DoQuery(qstr)

                qstr = "Select Top 1 N.""Series"",B.""BPLId"",C.""WhsCode"" ""WhsCode"" ,TO_CHAR(NOW(),'YYYYYMMDD') ""DocDate"" " & vbNewLine &
                                "  From ""NNM1"" N  " & vbNewLine &
                                "       Inner Join ""OFPR"" O On O.""Indicator""=N.""Indicator"" " & vbNewLine &
                                "       Inner Join ""OBPL"" B On B.""BPLId""=N.""BPLId"" " & vbNewLine &
                                "       Inner Join ""OWHS"" C On C.""U_BUSUNIT""='" + Branch + "' AND C.""U_WHSTYPE""='N' " & vbNewLine &
                                " Where N.""ObjectCode""='203' " & vbNewLine &
                                "   And O.""PeriodStat"" In ('N','C') And N.""Locked""='N' AND N.""Remark""='" + Branch + "'  " & vbNewLine &
                                "   And TO_CHAR('" + OrderDetails.UpdateDate + "','YYYYYMMDD') Between TO_CHAR(O.""F_RefDate"",'YYYYYMMDD') And TO_CHAR(O.""T_RefDate"",'YYYYYMMDD')"
                rSet.DoQuery(qstr)
                If rSet.RecordCount > 0 Then
                Else
                    erMessage = "Douwn Payment Series not Found"
                    Return False
                End If
                Dim ARDownDraft As SAPbobsCOM.Documents = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDownPayments)
                ARDownDraft.DocObjectCode = SAPbobsCOM.BoObjectTypes.oDownPayments

                qstr = "SELECT * FROM  ""ORDR"" WHERE ""DocEntry""='" + SalesOrderEntry.ToString + "'"
                Dim SoHrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                SoHrSet.DoQuery(qstr)
                'AND ""LineTotal"">0
                qstr = "SELECT * FROM  ""RDR1"" WHERE ""DocEntry""='" + SalesOrderEntry.ToString + "'and ifnull(""SerialNum"",'')='Exchange Line'  "
                Dim SoDrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                SoDrSet.DoQuery(qstr)
                Dim cardcode = SoHrSet.Fields.Item("CardCode").Value
                ARDownDraft.CardCode = SoHrSet.Fields.Item("CardCode").Value
                ARDownDraft.BPL_IDAssignedToInvoice = rSet.Fields.Item("BPLId").Value
                ARDownDraft.Series = rSet.Fields.Item("Series").Value
                'ARDownDraft.Comments = OrderDetails.Remarks
                ARDownDraft.DocDate = New Date(Mid(OrderDetails.UpdateDate, 1, 4), Mid(OrderDetails.UpdateDate, 5, 2), Mid(OrderDetails.UpdateDate, 7, 2))
                ARDownDraft.TaxDate = New Date(Mid(OrderDetails.UpdateDate, 1, 4), Mid(OrderDetails.UpdateDate, 5, 2), Mid(OrderDetails.UpdateDate, 7, 2))
                ARDownDraft.DownPaymentType = SAPbobsCOM.DownPaymentTypeEnum.dptRequest
                'ARDownDraft.BPL_IDAssignedToInvoice = "1"
                ARDownDraft.NumAtCard = SoHrSet1.Fields.Item("NumAtCard").Value
                ARDownDraft.UserFields.Fields.Item("U_BRANCH").Value = Branch
                'ARDownDraft.UserFields.Fields.Item("U_ORDERID").Value = DownPaymentDetails.OrderId
                'ARDownDraft.UserFields.Fields.Item("U_STATUS").Value = DownPaymentDetails.Status
                'ARDownDraft.UserFields.Fields.Item("U_RESPCODE").Value = DownPaymentDetails.ResponseCode
                'ARDownDraft.UserFields.Fields.Item("U_BANKTXID").Value = DownPaymentDetails.BankTransID
                'ARDownDraft.UserFields.Fields.Item("U_USERCODE").Value = UserId
                Dim TotalValue As Decimal = 0

                'While Not SoDrSet.EoF
                '    'erMessage = TotalValue.ToString
                '    'Return False
                '    ARDownDraft.Lines.BaseType = "17"
                '    ARDownDraft.Lines.BaseEntry = SoDrSet.Fields.Item("DocEntry").Value
                '    ARDownDraft.Lines.BaseLine = SoDrSet.Fields.Item("LineNum").Value
                '    ARDownDraft.Lines.WarehouseCode = SoDrSet.Fields.Item("WhsCode").Value
                '    qstr = "SELECT * FROM  ""RDR2"" WHERE ""DocEntry""='" + SalesOrderEntry.ToString + "' " & vbNewLine &
                '           "    And ""LineNum""='" + SoDrSet.Fields.Item("LineNum").Value.ToString + "' " & vbNewLine &
                '           "    AND ""ExpnsCode""='1'"
                '    Dim SoFRGDrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                '    SoFRGDrSet.DoQuery(qstr)
                '    If SoFRGDrSet.RecordCount > 0 Then
                '        TotalValue = TotalValue + CType(SoDrSet.Fields.Item("LineTotal").Value, Decimal) + CType(SoDrSet.Fields.Item("VatSum").Value, Decimal) + (SoFRGDrSet.Fields.Item("LineTotal").Value)
                '        ARDownDraft.Lines.LineTotal = CType(SoDrSet.Fields.Item("LineTotal").Value, Decimal) + CType(SoDrSet.Fields.Item("VatSum").Value, Decimal) + (SoFRGDrSet.Fields.Item("LineTotal").Value)
                '        ARDownDraft.Lines.TaxCode = ""
                '    Else
                '        TotalValue = TotalValue + CType(SoDrSet.Fields.Item("LineTotal").Value, Decimal) + CType(SoDrSet.Fields.Item("VatSum").Value, Decimal)
                '        ARDownDraft.Lines.LineTotal = CType(SoDrSet.Fields.Item("LineTotal").Value, Decimal) + CType(SoDrSet.Fields.Item("VatSum").Value, Decimal)
                '        ARDownDraft.Lines.TaxCode = ""
                '    End If
                While Not SoDrSet.EoF
                    'erMessage = TotalValue.ToString
                    'Return False
                    ARDownDraft.Lines.BaseType = "17"
                    ARDownDraft.Lines.BaseEntry = SoDrSet.Fields.Item("DocEntry").Value
                    ARDownDraft.Lines.BaseLine = SoDrSet.Fields.Item("LineNum").Value
                    'ARDownDraft.Lines.WarehouseCode = SoDrSet.Fields.Item("WhsCode").Value
                    qstr = "SELECT * FROM  ""RDR1"" WHERE ""DocEntry""='" + SalesOrderEntry.ToString + "' " & vbNewLine &
                           " And ""LineNum""='" + SoDrSet.Fields.Item("LineNum").Value.ToString + "' " & vbNewLine &
                           "And ""LineStatus""='O' and ifnull(""SerialNum"",'')='Exchange Line'"

                    Dim SoFRGDrSet As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                    SoFRGDrSet.DoQuery(qstr)
                    If SoFRGDrSet.RecordCount > 0 Then
                        TotalValue = TotalValue + CType(SoDrSet.Fields.Item("LineTotal").Value, Decimal) + CType(SoDrSet.Fields.Item("VatSum").Value, Decimal) + (SoFRGDrSet.Fields.Item("LineTotal").Value)
                        ARDownDraft.Lines.LineTotal = CType(SoDrSet.Fields.Item("LineTotal").Value, Decimal) + CType(SoDrSet.Fields.Item("VatSum").Value, Decimal) + (SoFRGDrSet.Fields.Item("LineTotal").Value)
                        ARDownDraft.Lines.TaxCode = ""
                    Else
                        TotalValue = TotalValue + CType(SoDrSet.Fields.Item("LineTotal").Value, Decimal) + CType(SoDrSet.Fields.Item("VatSum").Value, Decimal)
                        ARDownDraft.Lines.LineTotal = CType(SoDrSet.Fields.Item("LineTotal").Value, Decimal) + CType(SoDrSet.Fields.Item("VatSum").Value, Decimal)
                        ARDownDraft.Lines.TaxCode = ""
                    End If


                    ARDownDraft.Lines.CostingCode = Branch
                    ARDownDraft.Lines.Add()
                    SoDrSet.MoveNext()
                End While

                Dim DocTotal As Decimal = 0
                For Each Item As DTS_MODEL_PMNT_DTLS In OrderDetails.PaymentDetails
                    DocTotal = DocTotal + Item.Amount
                Next
                If DocTotal <> 0 Then
                    ARDownDraft.DocTotal = DocTotal
                End If

                'ARDownDraft.Lines.CostingCode = Branch
                'ARDownDraft.Lines.Add()
                '    SoDrSet.MoveNext()
                'End While
                'If SoHrSet.Fields.Item("U_PREPCARD").Value.ToString = "Y" Then
                '    ARDownDraft.DocTotal = TotalValue - CType(SoHrSet.Fields.Item("U_TRANSVAL").Value.ToString, Decimal)
                'End If
                Dim lRetCode As Integer
                lRetCode = ARDownDraft.Add
                Dim ErrorMessage As String = ""
                If lRetCode <> 0 Then
                    Dim sErrMsg As String = G_DI_Company.GetLastErrorDescription()
                    G_DI_Company.GetLastError(lRetCode, sErrMsg)
                    erMessage = sErrMsg
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(ARDownDraft)
                    Return False
                Else
                    Dim VoucherPaymentNo As String = ""
                    Dim ObjType As String = G_DI_Company.GetNewObjectType.ToString.TrimEnd
                    DpmEntry = G_DI_Company.GetNewObjectKey.Trim.ToString
                    Dim InPay As SAPbobsCOM.Payments = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oIncomingPayments)
                    'erMessage = "t1"
                    'Return False
                    qstr = "Select Top 1 N.""Series"",B.""BPLId"",C.""WhsCode"" ""WhsCode"" ,TO_CHAR(NOW(),'YYYYYMMDD') ""DocDate"" " & vbNewLine &
                                "  From ""NNM1"" N  " & vbNewLine &
                                "       Inner Join ""OFPR"" O On O.""Indicator""=N.""Indicator"" " & vbNewLine &
                                "       Inner Join ""OBPL"" B On B.""BPLId""=N.""BPLId"" " & vbNewLine &
                                "       Inner Join ""OWHS"" C On C.""U_BUSUNIT""='" + Branch + "' AND C.""U_WHSTYPE""='N' " & vbNewLine &
                                " Where N.""ObjectCode""='24' " & vbNewLine &
                                "   And O.""PeriodStat"" In ('N','C') And N.""Locked""='N' AND N.""Remark""='" + Branch + "'  " & vbNewLine &
                                "   And TO_CHAR('" + OrderDetails.UpdateDate + "','YYYYYMMDD') Between TO_CHAR(O.""F_RefDate"",'YYYYYMMDD') And TO_CHAR(O.""T_RefDate"",'YYYYYMMDD')"

                    Dim IncPayment As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                    IncPayment.DoQuery(qstr)
                    If IncPayment.RecordCount > 0 Then
                    Else
                        erMessage = "Incoimng Payment Series not Found"
                        Return False
                    End If
                    InPay.CardCode = SoHrSet.Fields.Item("CardCode").Value
                    'InPay.DocDate = New Date(Mid(getServerDate, 1, 4), Mid(getServerDate, 5, 2), Mid(getServerDate, 7, 2))
                    InPay.BPLID = SoHrSet.Fields.Item("BPLId").Value
                    InPay.DocDate = New Date(Mid(OrderDetails.UpdateDate, 1, 4), Mid(OrderDetails.UpdateDate, 5, 2), Mid(OrderDetails.UpdateDate, 7, 2))
                    InPay.Series = IncPayment.Fields.Item("Series").Value
                    InPay.UserFields.Fields.Item("U_BRANCH").Value = Branch
                    InPay.UserFields.Fields.Item("U_CRTDBY").Value = UserId
                    InPay.Invoices.DocEntry = Convert.ToInt32(DpmEntry)
                    InPay.Invoices.InvoiceType = SAPbobsCOM.BoRcptInvTypes.it_DownPayment
                    InPay.Invoices.DistributionRule = Branch
                    InPay.Invoices.Add()
                    Dim ARDown As SAPbobsCOM.Documents = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDownPayments)
                    ARDown = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDownPayments)
                    ARDown.DocObjectCode = SAPbobsCOM.BoObjectTypes.oDownPayments
                    ARDown.GetByKey(Convert.ToInt32(DpmEntry))
                    For Each Item As DTS_MODEL_PMNT_DTLS In OrderDetails.PaymentDetails
                        If Item.PaymentType = "S" Then
                            InPay.CashSum = Item.Amount
                            qstr = "SELECT ""OverCode"",""FrgnName"",""AcctCode"" FROM OACT  WHERE ""OverCode""='" + Branch + "' AND ""FrgnName""='S'"
                            Dim CashPayment As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                            CashPayment.DoQuery(qstr)
                            InPay.CashAccount = CashPayment.Fields.Item("AcctCode").Value
                            'erMessage = Item.Amount.ToString
                            'Return False
                        End If
                        If Item.PaymentType = "S" Then
                        Else
                            'qstr = "SELECT ""OverCode"",""FrgnName"",""AcctCode"" FROM OACT  WHERE ""OverCode""='" + Branch + "' AND ""FrgnName""='C'"
                            qstr = "SELECT ""CreditCard""  FROM ""OCRC"" where ""U_BANKCODE""='" + Item.Bank + "' AND ""U_PMNTP""='" + Item.PaymentType + "'"
                            Dim CreditPayment As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                            CreditPayment.DoQuery(qstr)
                            If CreditPayment.RecordCount > 0 Then
                                InPay.CreditCards.CreditCard = CreditPayment.Fields.Item("CreditCard").Value
                                'InPay.CreditCards.CreditCardNumber = IIf(Item.CardNo.ToString = "", "1111", Right(Item.CardNo, 4))
                                If Item.PaymentType = "2" Or Item.PaymentType = "8" Then
                                    If DiscountExists = True Then
                                        erMessage = "Discount can not be done for Voucher type Payment please Remove Item Discount"
                                        Return False
                                    End If
                                    qstr = "SELECT ""PrcCode"",""PrcName"" FROM ""OPRC"" WHERE ""PrcCode""='" + Item.CardNo + "' and ""DimCode""='5' and ""U_CARDCODE""='" + cardcode + "'"
                                    Dim VouchNameStr As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                                    VouchNameStr.DoQuery(qstr)
                                    If VouchNameStr.RecordCount > 0 Then
                                        InPay.CreditCards.CreditCardNumber = VouchNameStr.Fields.Item("PrcCode").Value
                                        InPay.CreditCards.UserFields.Fields.Item("U_CARDNO").Value = VouchNameStr.Fields.Item("PrcCode").Value
                                        VoucherPaymentNo = IIf(VoucherPaymentNo = "", Item.CardNo + ":" + Item.Amount.ToString, VoucherPaymentNo + ";" + Item.CardNo + ":" + Item.Amount.ToString)
                                    Else
                                        erMessage = "No Voucher found for " + Item.CardNo
                                        Return False
                                    End If

                                Else
                                    Try
                                        InPay.CreditCards.CreditCardNumber = IIf(Item.CardNo Is Nothing, "1111", Right(Item.CardNo.ToString, 4))
                                        InPay.CreditCards.UserFields.Fields.Item("U_CARDNO").Value = IIf(Item.CardNo Is Nothing, "1111", Item.CardNo.ToString)
                                    Catch ex As Exception
                                        InPay.CreditCards.CreditCardNumber = "1111"
                                        InPay.CreditCards.UserFields.Fields.Item("U_CARDNO").Value = "1111"
                                    End Try

                                End If
                                InPay.CreditCards.CardValidUntil = New Date(Mid("29991231", 1, 4), Mid("29991231", 5, 2), Mid("29991231", 7, 2))
                                InPay.CreditCards.PaymentMethodCode = 1
                                InPay.CreditCards.CreditSum = Item.Amount
                                'InPay.CreditCards.FirstPaymentSum = CreditCards.CardAmount
                                Try
                                    InPay.CreditCards.VoucherNum = IIf(Item.Tranid.ToString = "", "111", Item.Tranid.ToString)
                                Catch ex As Exception
                                    InPay.CreditCards.VoucherNum = "111"
                                End Try

                                InPay.CreditCards.CreditType = SAPbobsCOM.BoRcptCredTypes.cr_Regular
                                InPay.CreditCards.Add()
                            Else
                                erMessage = "No Payment Method found for " + Item.Bank + " and " + Item.PaymentType
                                Return False
                            End If

                            'InPay.TransferAccount = CreditPayment.Fields.Item("AcctCode").Value
                            'InPay.TransferReference = Item.Tranid.ToString
                            'InPay.UserFields.Fields.Item("U_CRDTRNID").Value = Item.Tranid.ToString
                            'InPay.UserFields.Fields.Item("U_CRDONAME").Value = Item.CardHolderName.ToString
                            'InPay.UserFields.Fields.Item("U_CRDCNO").Value = Item.CardNo.ToString
                            'InPay.TransferDate = New Date(Mid(Date.Now.ToString("yyyyMMdd"), 1, 4), Mid(Date.Now.ToString("yyyyMMdd"), 5, 2), Mid(Date.Now.ToString("yyyyMMdd"), 7, 2))
                            'InPay.TransferSum = Item.Amount
                        End If
                        'If Item.PaymentType = "C" Then
                        '    qstr = "SELECT ""OverCode"",""FrgnName"",""AcctCode"" FROM OACT  WHERE ""OverCode""='" + Branch + "' AND ""FrgnName""='C'"
                        '    Dim CreditPayment As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                        '    CreditPayment.DoQuery(qstr)
                        '    InPay.TransferAccount = CreditPayment.Fields.Item("AcctCode").Value
                        '    InPay.TransferReference = Item.Tranid.ToString
                        '    InPay.UserFields.Fields.Item("U_CRDTRNID").Value = Item.Tranid.ToString
                        '    InPay.UserFields.Fields.Item("U_CRDONAME").Value = Item.CardHolderName.ToString
                        '    InPay.UserFields.Fields.Item("U_CRDCNO").Value = Item.CardNo.ToString
                        '    InPay.TransferDate = New Date(Mid(Date.Now.ToString("yyyyMMdd"), 1, 4), Mid(Date.Now.ToString("yyyyMMdd"), 5, 2), Mid(Date.Now.ToString("yyyyMMdd"), 7, 2))
                        '    InPay.TransferSum = Item.Amount
                        'End If
                        'If Item.PaymentType = "U" Then
                        '    qstr = "SELECT ""OverCode"",""FrgnName"",""AcctCode"" FROM OACT  WHERE ""OverCode""='" + Branch + "' AND ""FrgnName""='U'"
                        '    Dim UPIPayment As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                        '    UPIPayment.DoQuery(qstr)
                        '    InPay.CheckAccount = UPIPayment.Fields.Item("AcctCode").Value
                        '    InPay.Checks.CountryCode = "BD"
                        '    InPay.Checks.CheckSum = Item.Amount
                        '    InPay.Checks.UserFields.Fields.Item("U_TRNSID").Value = Item.Tranid.ToString
                        '    InPay.Checks.DueDate = New Date(Mid(Date.Now.ToString("yyyyMMdd"), 1, 4), Mid(Date.Now.ToString("yyyyMMdd"), 5, 2), Mid(Date.Now.ToString("yyyyMMdd"), 7, 2))
                        '    qstr = "SELECT A.""BankCode"" " & vbNewLine &
                        '           " FROM ""ODSC"" A " & vbNewLine &
                        '           "    INNER JOIN ""CUFD"" B ON B.""TableID""='ODSC' AND B.""AliasID""='TYPE' " & vbNewLine &
                        '           "    INNER JOIN ""UFD1"" C ON C.""TableID""=B.""TableID"" AND B.""FieldID""=C.""FieldID"" AND A.""U_TYPE""=C.""FldValue"" " & vbNewLine &
                        '       " WHERE A.""U_TYPE""='" + Item.UpiName + "' "
                        '    Dim UPIBankPayment As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                        '    UPIBankPayment.DoQuery(qstr)
                        '    InPay.Checks.BankCode = UPIBankPayment.Fields.Item("BankCode").Value
                        '    InPay.Checks.CheckNumber = 1
                        '    'InPay.Checks.
                        '    InPay.Checks.Add()
                        'End If
                    Next
                    lRetCode = InPay.Add
                    If lRetCode <> 0 Then
                        Dim sErrMsg As String = G_DI_Company.GetLastErrorDescription()
                        G_DI_Company.GetLastError(lRetCode, sErrMsg)
                        erMessage = sErrMsg
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(InPay)
                        Return False
                    End If
                    Dim IncDocEntry = G_DI_Company.GetNewObjectKey.Trim.ToString
                    If VoucherPaymentNo <> "" Then
                        For Each Dtls As String In VoucherPaymentNo.Split(New String() {";"}, StringSplitOptions.None)
                            Dim PrcCode = Dtls.Split(":")(0)
                            Dim Value As Decimal = Dtls.Split(":")(1)

                            qstr = "UPDATE A SET ""OcrCode5""='" + PrcCode.ToString + "' " & vbNewLine &
                               " FROM ""JDT1"" A " & vbNewLine &
                               "    INNER JOIN ""OJDT"" B ON A.""TransId""=B.""TransId"" " & vbNewLine &
                               "    INNER JOIN ""ORCT"" C ON C.""TransId""=B.""TransId"" AND C.""Canceled""='N' --AND C.""CardCode"" =A.""ShortName"" " & vbNewLine &
                               "    INNER JOIN ""OACT"" E ON E.""AcctCode""=A.""Account"" AND E.""FrgnName""='VS' " & vbNewLine &
                               "    INNER JOIN ""OPRC"" F ON F.""PrcCode""='" + PrcCode.ToString + "' " & vbNewLine &
                               " WHERE A.""Debit""<>0 " & vbNewLine &
                               " and F.""PrcCode""='" + PrcCode.ToString + "' " & vbNewLine &
                               " AND A.""Debit""='" + Value.ToString + "' " & vbNewLine &
                               " AND C.""DocEntry""='" + IncDocEntry.ToString + "' " & vbNewLine &
                               "  AND IFNULL(A.""OcrCode5"",'')='' "
                            Dim IncomingVchUpdt As SAPbobsCOM.Recordset = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                            IncomingVchUpdt.DoQuery(qstr)
                        Next

                    End If

                    Return True

                End If
            Catch ex As Exception
                erMessage = ex.Message
                Return False
            End Try
        End Function

        <Route("Api/UpdateECommerceCourierInfo")>
        <HttpPost>
        Public Function UpdateECommerceCourierInfo(ByVal TransUpdate As DTS_MODEL_STTRANSECOM_UPDATE) As HttpResponseMessage
            Dim G_DI_Company As SAPbobsCOM.Company = Nothing
            Dim strRegnNo As String = ""
            dtTable.Columns.Add("ReturnCode")
            dtTable.Columns.Add("ReturnDocEntry")
            dtTable.Columns.Add("ReturnObjType")
            dtTable.Columns.Add("ReturnSeries")
            dtTable.Columns.Add("ReturnDocNum")
            dtTable.Columns.Add("ReturnMsg")
            Try
                Dim Branch As String = ""
                Dim UserID As String = ""


                If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                    Return Request.CreateResponse(HttpStatusCode.OK, dtError)
                End If
                Dim qstr As String = ""
                'Dim qstr As String = ""
                qstr = "SELECT ""U_SAPUNAME"" ,""U_SAPPWD"" " & vbNewLine &
                       " FROM ""OUSR"" A " & vbNewLine &
                       " WHERE A.""USER_CODE"" ='" + UserID + "'"
                Dim dRow As Data.DataRow
                dRow = DBSQL.getQueryDataRow(qstr, "")
                Dim SAPUserId As String = ""
                Dim SAPPassword As String = ""
                Try
                    SAPUserId = dRow.Item("U_SAPUNAME")
                    SAPPassword = dRow.Item("U_SAPPWD")
                Catch ex As Exception
                End Try
                Connection.ConnectSAP(G_DI_Company, SAPUserId, SAPPassword)
                rSet = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)

                Dim DocNum As String = ""
                Dim SoEntry As String = TransUpdate.DocEntry
                Dim ObjType As String = ""
                Dim SeriesName As String = ""
                Dim ReturnDocNo As String = ""
                Dim ConfDate As String = TransUpdate.ConfDate
                Try
                    ' G_DI_Company.StartTransaction()
                    'Dim StockTrans As SAPbobsCOM.Documents = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oStockTransfer)
                    'SoEntry = SalesOrder.DocEntry
                    Dim StockTrans As SAPbobsCOM.Documents = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oOrders)
                    StockTrans.DocObjectCode = SAPbobsCOM.BoObjectTypes.oStockTransfer
                    If StockTrans.GetByKey(SoEntry) Then
                        StockTrans.UserFields.Fields.Item("U_TRCKID").Value = TransUpdate.TrackingID
                        StockTrans.UserFields.Fields.Item("U_COURCOM").Value = IIf(TransUpdate.CourierCompany Is Nothing, "", TransUpdate.CourierCompany)
                        StockTrans.UserFields.Fields.Item("U_DELCONST").Value = TransUpdate.StatusCode
                        Try
                            If ConfDate <> Nothing Then
                                StockTrans.UserFields.Fields.Item("U_CNFDATE").Value = New Date(Mid(ConfDate, 1, 4), Mid(ConfDate, 5, 2), Mid(ConfDate, 7, 2))
                            End If
                        Catch ex As Exception
                        End Try
                        Try
                            StockTrans.UserFields.Fields.Item("U_ITCHNL").Value = TransUpdate.DelChannel
                        Catch ex As Exception
                        End Try
                        Try
                            StockTrans.UserFields.Fields.Item("U_REMARKS").Value = TransUpdate.Remarks
                        Catch ex As Exception
                        End Try
                        Try
                            StockTrans.UserFields.Fields.Item("U_DELAGENT").Value = TransUpdate.DelAgent
                        Catch ex As Exception
                        End Try
                        Try
                            StockTrans.UserFields.Fields.Item("U_AREACODE").Value = TransUpdate.Area
                        Catch ex As Exception
                        End Try
                        StockTrans.UserFields.Fields.Item("U_FRMPORT").Value = "Y"
                    End If
                    Dim lRetCode As Integer = StockTrans.Update()
                    Dim ErrorMessage As String = ""
                    If lRetCode <> 0 Then
                        Dim sErrMsg As String = G_DI_Company.GetLastErrorDescription()
                        G_DI_Company.GetLastError(lRetCode, sErrMsg)
                        ErrorMessage = sErrMsg
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(StockTrans)
                        'If G_DI_Company.InTransaction Then
                        '    G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                        'End If
                        G_DI_Company.Disconnect()
                        NewRow = dtTable.NewRow
                        NewRow.Item("ReturnCode") = "-2222"
                        NewRow.Item("ReturnDocEntry") = "-1"
                        NewRow.Item("ReturnObjType") = "-1"
                        NewRow.Item("ReturnSeries") = "-1"
                        NewRow.Item("ReturnDocNum") = "-1"
                        NewRow.Item("ReturnMsg") = ErrorMessage
                        dtTable.Rows.Add(NewRow)
                        Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                    Else
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(StockTrans)
                        ObjType = G_DI_Company.GetNewObjectType.ToString.TrimEnd
                        qstr = "SELECT B.""SeriesName"",CAST(A.""DocNum"" AS VARCHAR) ""DocNum"",B.""SeriesName""||'/'||CAST(A.""DocNum"" AS VARCHAR) ""StrDocNum"" " & vbNewLine &
                               " From ""ORDR"" A " & vbNewLine &
                               " INNER JOIN ""NNM1"" B ON B.""ObjectCode""=A.""ObjType"" AND B.""Series""=A.""Series"" " & vbNewLine &
                               " WHERE A.""DocEntry""='" + SoEntry + "' "
                        rSet.DoQuery(qstr)
                        ReturnDocNo = rSet.Fields.Item("StrDocNum").Value
                        SeriesName = rSet.Fields.Item("SeriesName").Value
                        DocNum = rSet.Fields.Item("DocNum").Value
                    End If
                Catch ex As Exception
                    'If G_DI_Company.InTransaction Then
                    '    G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                    'End If
                End Try
                'G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit)
                G_DI_Company.Disconnect()
                NewRow = dtTable.NewRow
                NewRow.Item("ReturnCode") = "0000"
                NewRow.Item("ReturnDocEntry") = SoEntry
                NewRow.Item("ReturnObjType") = ObjType
                NewRow.Item("ReturnSeries") = SeriesName
                NewRow.Item("ReturnDocNum") = DocNum
                NewRow.Item("ReturnMsg") = "Your Request No. " + ReturnDocNo + " successfully updated"
                dtTable.Rows.Add(NewRow)
                Return Request.CreateResponse(HttpStatusCode.OK, dtTable)

            Catch ex As Exception
                Try
                    G_DI_Company.Disconnect()
                Catch ex1 As Exception
                End Try
                NewRow = dtTable.NewRow
                NewRow.Item("ReturnCode") = "-2222"
                NewRow.Item("ReturnMsg") = ex.Message.ToString
                NewRow.Item("ReturnDocEntry") = "-1"
                NewRow.Item("ReturnObjType") = "-1"
                NewRow.Item("ReturnSeries") = "-1"
                NewRow.Item("ReturnDocNum") = "-1"
                dtTable.Rows.Add(NewRow)
                Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
            End Try
        End Function
        'new API
        <Route("Api/UpdateDoctorAppoinment")>
        <HttpPost>
        Public Function UpdateDoctorAppoinment(ByVal DocApp As DTS_DOCAPPUPDATE) As HttpResponseMessage
            Dim G_DI_Company As SAPbobsCOM.Company = Nothing
            Dim strRegnNo As String = ""
            dtTable.Columns.Add("ReturnCode")
            dtTable.Columns.Add("ReturnDocEntry")
            dtTable.Columns.Add("ReturnObjType")
            dtTable.Columns.Add("ReturnSeries")
            dtTable.Columns.Add("ReturnDocNum")
            dtTable.Columns.Add("ReturnMsg")
            Try
                Dim Branch As String = ""
                Dim UserID As String = ""


                If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                    Return Request.CreateResponse(HttpStatusCode.OK, dtError)
                End If
                Dim qstr As String = ""
                'Dim qstr As String = ""
                qstr = "SELECT ""U_SAPUNAME"" ,""U_SAPPWD"" " & vbNewLine &
                       " FROM ""OUSR"" A " & vbNewLine &
                       " WHERE A.""USER_CODE"" ='" + UserID + "'"
                Dim dRow As Data.DataRow
                dRow = DBSQL.getQueryDataRow(qstr, "")
                Dim SAPUserId As String = ""
                Dim SAPPassword As String = ""
                Try
                    SAPUserId = dRow.Item("U_SAPUNAME")
                    SAPPassword = dRow.Item("U_SAPPWD")
                Catch ex As Exception
                End Try
                Connection.ConnectSAP(G_DI_Company, SAPUserId, SAPPassword)
                rSet = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                Try
                    Dim vContact As SAPbobsCOM.Contacts = G_DI_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oContacts)

                    If vContact.GetByKey(DocApp.ActivityId) Then

                        vContact.UserFields.Fields.Item("U_ACVISDT").Value = New Date(Mid(DocApp.ActualVisitDate, 1, 4), Mid(DocApp.ActualVisitDate, 5, 2), Mid(DocApp.ActualVisitDate, 7, 2))

                        vContact.UserFields.Fields.Item("U_DOSTATUS").Value = DocApp.DocStatus
                        vContact.UserFields.Fields.Item("U_CHKEDBY").Value = IIf(DocApp.CheckedBy Is Nothing, "", DocApp.CheckedBy)
                        vContact.UserFields.Fields.Item("U_REMARKS").Value = IIf(DocApp.Remarks Is Nothing, "", DocApp.Remarks)
                        Dim lRetCode As Integer = vContact.Update()
                        Dim ErrorMessage As String = ""
                        If lRetCode <> 0 Then
                            Dim sErrMsg As String = G_DI_Company.GetLastErrorDescription()
                            G_DI_Company.GetLastError(lRetCode, sErrMsg)
                            ErrorMessage = sErrMsg
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(vContact)
                            'If G_DI_Company.InTransaction Then
                            '    G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                            'End If
                            G_DI_Company.Disconnect()
                            NewRow = dtTable.NewRow
                            NewRow.Item("ReturnCode") = "-2222"
                            NewRow.Item("ReturnDocEntry") = "-1"
                            NewRow.Item("ReturnObjType") = "-1"
                            NewRow.Item("ReturnSeries") = "-1"
                            NewRow.Item("ReturnDocNum") = "-1"
                            NewRow.Item("ReturnMsg") = ErrorMessage
                            dtTable.Rows.Add(NewRow)
                            Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                        Else
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(vContact)
                            Dim ObjType As String = G_DI_Company.GetNewObjectType.ToString.TrimEnd
                            Dim DLEntry As String = G_DI_Company.GetNewObjectKey.Trim.ToString
                            'ActivityId = ActivityId + "," + DLEntry


                        End If
                    Else
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(vContact)
                        'If G_DI_Company.InTransaction Then
                        '    G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                        'End If
                        G_DI_Company.Disconnect()
                        NewRow = dtTable.NewRow
                        NewRow.Item("ReturnCode") = "-2222"
                        NewRow.Item("ReturnDocEntry") = "-1"
                        NewRow.Item("ReturnObjType") = "-1"
                        NewRow.Item("ReturnSeries") = "-1"
                        NewRow.Item("ReturnDocNum") = "-1"
                        NewRow.Item("ReturnMsg") = "Activity not Found"
                        dtTable.Rows.Add(NewRow)
                        Return Request.CreateResponse(HttpStatusCode.OK, dtTable)

                    End If




                Catch ex As Exception
                    'If G_DI_Company.InTransaction Then
                    '    G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                    'End If
                End Try
                'G_DI_Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit)
                G_DI_Company.Disconnect()
                NewRow = dtTable.NewRow
                NewRow.Item("ReturnCode") = "0000"
                NewRow.Item("ReturnDocEntry") = DocApp.ActivityId
                NewRow.Item("ReturnObjType") = DocApp.ActivityId
                NewRow.Item("ReturnSeries") = ""
                NewRow.Item("ReturnDocNum") = DocApp.ActivityId
                NewRow.Item("ReturnMsg") = "Your Request No. " + DocApp.ActivityId + " successfully submitted"
                dtTable.Rows.Add(NewRow)
                Return Request.CreateResponse(HttpStatusCode.OK, dtTable)

            Catch ex As Exception
                Try
                    G_DI_Company.Disconnect()
                Catch ex1 As Exception
                End Try
                NewRow = dtTable.NewRow
                NewRow.Item("ReturnCode") = "-2222"
                NewRow.Item("ReturnMsg") = ex.Message.ToString
                NewRow.Item("ReturnDocEntry") = "-1"
                NewRow.Item("ReturnObjType") = "-1"
                NewRow.Item("ReturnSeries") = "-1"
                NewRow.Item("ReturnDocNum") = "-1"
                dtTable.Rows.Add(NewRow)
                Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
            End Try
        End Function



    End Class
End Namespace