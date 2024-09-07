Imports System.Data.SqlClient
Imports System.Linq
Imports System.Net
Imports System.Net.Http
Imports System.Web.Http
Imports Sap.Data.Hana

Imports System.Data

Imports System.Data.OleDb
Imports System.Data.Odbc
Imports System.Data.SqlClient.SqlDataAdapter


Namespace Controllers
    Public Class DTS_GetController
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

        <Route("Api/CheckPassword")>
        <HttpGet>
        Public Function CheckPassword(ByVal UserId As String, ByVal Password As String) As HttpResponseMessage
            Try
                If Not Mod_Main.AuthenticateLogIn(Request) Then
                    Return Request.CreateResponse(HttpStatusCode.BadRequest, "AccessDenied !! ")
                End If
                Dim Qstr As String = ""

                Dim ErrorMsg As String = ""
                If Mod_Main.isUserAllreadyConnected(UserId, ErrorMsg) = True Then
                    dt_Error.Columns.Add("ReturnCode")
                    dt_Error.Columns.Add("ReturnMsg")
                    NewRow = dt_Error.NewRow
                    NewRow.Item("ReturnCode") = "-2222"
                    NewRow.Item("ReturnMsg") = ErrorMsg
                    dt_Error.Rows.Add(NewRow)
                    Return Request.CreateResponse(HttpStatusCode.OK, dt_Error)
                End If


                Qstr = "SELECT A.""U_USERTYPE"",A.""U_SAPUNAME"",A.""U_SAPPWD"",A.""U_BUNIT"",A.""U_USERPWD"",IFNULL(B.""empID"",0) ""empID"",IFNULL(C.""U_POSTNTP"",'O') ""U_POSTNTP"" " & vbNewLine &
                "FROM ""OUSR"" A" & vbNewLine &
                "LEFT OUTER JOIN ""OHEM"" B ON A.""USERID"" = B.""userId""" & vbNewLine &
                "LEFT OUTER JOIN ""OHPS"" C ON B.""position"" = C.""posID""" & vbNewLine &
                "WHERE A.""USER_CODE""='" + UserId + "'"
                Dim dRow As Data.DataRow
                dRow = DBSQL.getQueryDataRow(Qstr, "")

                Dim SAPUserCode As String = dRow.Item("U_SAPUNAME")
                Dim SAPUserType As String = dRow.Item("U_USERTYPE")
                Dim SAPUserPWD As String = dRow.Item("U_SAPPWD")
                Dim SAPUserBUnit As String = dRow.Item("U_BUNIT")
                Dim SAPEmpId As String = dRow.Item("empID")
                Dim SAPEmpType As String = dRow.Item("U_POSTNTP")
                Dim LoginUserPwd = dRow.Item("U_USERPWD")
                If LoginUserPwd.ToString <> Password.ToString Then
                    dtTable.Columns.Add("ReturnCode")
                    dtTable.Columns.Add("ReturnMsg")
                    NewRow = dtTable.NewRow
                    NewRow.Item("ReturnCode") = "-2222"
                    NewRow.Item("ReturnMsg") = "User Log In Failed"
                    dtTable.Rows.Add(NewRow)
                    Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                End If


                Try
                    Connection.ConnectSAP(G_DI_Company, "", "")
                Catch ex As Exception
                    dtTable.Columns.Add("ReturnCode")
                    dtTable.Columns.Add("ReturnMsg")
                    NewRow = dtTable.NewRow
                    NewRow.Item("ReturnCode") = "-2222"
                    NewRow.Item("ReturnMsg") = ex.Message
                    dtTable.Rows.Add(NewRow)
                    Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                End Try

                Dim oResult As SAPbobsCOM.AuthenticateUserResultsEnum = G_DI_Company.AuthenticateUser(SAPUserCode, SAPUserPWD)
                If oResult <> 0 Then
                    dtTable.Columns.Add("ReturnCode")
                    dtTable.Columns.Add("ReturnMsg")
                    NewRow = dtTable.NewRow
                    NewRow.Item("ReturnCode") = "-2222"
                    NewRow.Item("ReturnMsg") = "SAP Log In Failed Contact Admin"
                    dtTable.Rows.Add(NewRow)
                    Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
                End If

                Dim AuthToken As String = SIL_Encryption.Cryptography.Encrypt(UserId + "_" + CStr((DateTime.UtcNow - New DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds).Replace(".", ""))

                Qstr = "DELETE FROM ""$DTS_TRACK_USER"" " &
                        " WHERE ""APPCODE""='BXN' AND ""USERCODE""='" + UserId + "' "
                DBSQL.executeQuery(Qstr, "")

                Qstr = "INSERT INTO ""$DTS_TRACK_USER""(""APPCODE"",""USERCODE"",""SAPCODE"",""USERPASSWORD"",""SAPUSERTYPE"",""LASTUPDT"",""LOGINIP"",""AUTHTOKEN"") " &
                        "VALUES('BXN','" + UserId + "','" + SAPUserCode + "','" + SAPUserPWD + "','" + SAPUserType + "',NOW(),NULL,'" + AuthToken + "');"
                DBSQL.executeQuery(Qstr, "")

                dt_Error.Columns.Add("ReturnCode")
                dt_Error.Columns.Add("ReturnMsg")
                dt_Error.Columns.Add("UserType")
                dt_Error.Columns.Add("Branch")
                dt_Error.Columns.Add("AuthToken")
                dt_Error.Columns.Add("EmpId")
                dt_Error.Columns.Add("EmpType")
                NewRow = dt_Error.NewRow
                NewRow.Item("ReturnCode") = "0000"
                NewRow.Item("ReturnMsg") = "Login successful"
                NewRow.Item("UserType") = SAPUserType
                NewRow.Item("Branch") = SAPUserBUnit
                NewRow.Item("AuthToken") = AuthToken
                NewRow.Item("EmpId") = SAPEmpId
                NewRow.Item("EmpType") = SAPEmpType
                dt_Error.Rows.Add(NewRow)
                G_DI_Company.Disconnect()
                Return Request.CreateResponse(HttpStatusCode.OK, dt_Error)
            Catch ex As Exception
                dt_Error.Columns.Add("ReturnCode")
                dt_Error.Columns.Add("ReturnMsg")
                NewRow = dt_Error.NewRow
                NewRow.Item("ReturnCode") = "-2222"
                NewRow.Item("ReturnMsg") = ex.Message
                dt_Error.Rows.Add(NewRow)
                G_DI_Company.Disconnect()
                Return Request.CreateResponse(HttpStatusCode.OK, dt_Error)
            End Try
        End Function
        <Route("Api/LogOut")>
        <HttpGet>
        Public Function LogOut() As HttpResponseMessage
            Try

                Dim Branch As String = ""
                Dim UserID As String = ""


                If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                    Return Request.CreateResponse(HttpStatusCode.OK, dtError)
                End If

                Dim Qstr As String = "DELETE FROM ""$DTS_TRACK_USER"" " &
                        " WHERE ""APPCODE""='BXN' AND ""USERCODE""='" + UserID + "' "
                DBSQL.executeQuery(Qstr, "")

                dtTable.Columns.Add("ReturnCode")
                dtTable.Columns.Add("ReturnMsg")
                NewRow = dtTable.NewRow
                NewRow.Item("ReturnCode") = "0000"
                NewRow.Item("ReturnMsg") = "Successfully LogOut"
                dtTable.Rows.Add(NewRow)
                Return Request.CreateResponse(HttpStatusCode.OK, dtTable)

            Catch ex As Exception
                dtTable.Columns.Add("ReturnCode")
                dtTable.Columns.Add("ReturnMsg")
                NewRow = dtTable.NewRow
                NewRow.Item("ReturnCode") = "-2222"
                NewRow.Item("ReturnMsg") = ex.Message
                dtTable.Rows.Add(NewRow)
                Return Request.CreateResponse(HttpStatusCode.OK, dtTable)
            End Try
        End Function

        <Route("Api/GetUserWiseMenuDetails")>
        <HttpGet>
        Public Function GetUserWiseMenuDetails() As HttpResponseMessage
            Dim Branch As String = ""
            Dim UserID As String = ""

            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_UserWiseMenuDetails")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_UserID", UserID)

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetMasterCompanyDetails")>
        <HttpGet>
        Public Function GetMasterCompanyDetails() As HttpResponseMessage
            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_MasterCmpanyDetails")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure


            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetBuisnessUnitDetails")>
        <HttpGet>
        Public Function GetBuisnessUnitDetails() As HttpResponseMessage
            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_BuisnessUnitDetails")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure


            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetWarehouseDetails")>
        <HttpGet>
        Public Function GetWarehouseDetails(Optional ByVal WhsCode As String = "", Optional ByVal WhsType As String = "") As HttpResponseMessage
            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_WarehouseDetails")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_BUCode", Branch)
            HanaDbCommand_SP.Parameters.AddWithValue("P_WhsCode", IIf(WhsCode Is Nothing, "", WhsCode))
            HanaDbCommand_SP.Parameters.AddWithValue("P_WhsType", IIf(WhsType Is Nothing, "", WhsType))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetWarehouseAllDetails")>
        <HttpGet>
        Public Function GetWarehouseAllDetails(Optional ByVal BuisnessUnit As String = "", Optional ByVal WhsCode As String = "", Optional ByVal WhsType As String = "") As HttpResponseMessage
            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_WarehouseDetails")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_BUCode", IIf(BuisnessUnit Is Nothing, "", BuisnessUnit))
            HanaDbCommand_SP.Parameters.AddWithValue("P_WhsCode", IIf(WhsCode Is Nothing, "", WhsCode))
            HanaDbCommand_SP.Parameters.AddWithValue("P_WhsType", IIf(WhsType Is Nothing, "", WhsType))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetItemeGroupDetails")>
        <HttpGet>
        Public Function GetItemeGroupDetails() As HttpResponseMessage
            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_ItemeGroupDetails")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetItemDetails")>
        <HttpGet>
        Public Function GetItemDetails(Optional ByVal ItemCode As String = "", Optional ByVal ItemGroup As String = "",
                                       Optional ByVal SelleItem As String = "", Optional ByVal PurchaseItem As String = "", Optional ByVal InventoryItem As String = "",
                                       Optional ByVal ItemCodeSearch As String = "", Optional ByVal ItemNameSearch As String = "",
                                       Optional ByVal CardCode As String = "", Optional ByVal HappyHrs As String = "N",
                                       Optional ByVal FromPage As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_ItemeDetails")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_ItemCode", IIf(ItemCode Is Nothing, "", ItemCode))
            HanaDbCommand_SP.Parameters.AddWithValue("P_ItemGroupCode", IIf(ItemGroup Is Nothing, "", ItemGroup))
            HanaDbCommand_SP.Parameters.AddWithValue("P_SellItem", IIf(SelleItem Is Nothing, "", SelleItem))
            HanaDbCommand_SP.Parameters.AddWithValue("P_PurchaseItem", IIf(PurchaseItem Is Nothing, "", PurchaseItem))
            HanaDbCommand_SP.Parameters.AddWithValue("P_InventoryItem", IIf(InventoryItem Is Nothing, "", InventoryItem))
            HanaDbCommand_SP.Parameters.AddWithValue("P_Branch", Branch)
            HanaDbCommand_SP.Parameters.AddWithValue("P_ItemCodeSearch", IIf(ItemCodeSearch Is Nothing, "", ItemCodeSearch))
            HanaDbCommand_SP.Parameters.AddWithValue("P_ItemNameSearch", IIf(ItemNameSearch Is Nothing, "", ItemNameSearch))
            HanaDbCommand_SP.Parameters.AddWithValue("P_CardCode", IIf(CardCode Is Nothing, "", CardCode))
            HanaDbCommand_SP.Parameters.AddWithValue("P_HappyHrs", IIf(HappyHrs Is Nothing, "N", HappyHrs))
            HanaDbCommand_SP.Parameters.AddWithValue("P_from", IIf(FromPage Is Nothing, "N", FromPage))


            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function


        <Route("Api/GetItemeWareHouseWiseStock")>
        <HttpGet>
        Public Function GetItemeWareHouseWiseStock(Optional ByVal ItemCode As String = "", Optional ByVal WhsCode As String = "",
                                      Optional ByVal WhsType As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_ItemeWareHouseWiseStock")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_ItemCode", IIf(ItemCode Is Nothing, "", ItemCode))
            HanaDbCommand_SP.Parameters.AddWithValue("P_WhsCode", IIf(WhsCode Is Nothing, "", WhsCode))
            HanaDbCommand_SP.Parameters.AddWithValue("P_WhsType", IIf(WhsType Is Nothing, "", WhsType))
            HanaDbCommand_SP.Parameters.AddWithValue("P_BUCode", Branch)

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetBPGroupDetails")>
        <HttpGet>
        Public Function GetBPGroupDetails(Optional ByVal P_GroupType As String = "", Optional ByVal P_GroupCode As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_BPGroupDetails")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_GroupType", IIf(P_GroupType Is Nothing, "", P_GroupType))
            HanaDbCommand_SP.Parameters.AddWithValue("P_GroupCode", IIf(P_GroupCode Is Nothing, "", P_GroupCode))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetPaymentTerms")>
        <HttpGet>
        Public Function GetPaymentTerms(Optional ByVal PaymentTermsGrpCode As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_PaymentTerms")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_PaymentTermsGrpCode", IIf(PaymentTermsGrpCode Is Nothing, "", PaymentTermsGrpCode))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetBPDetails")>
        <HttpGet>
        Public Function GetBPDetails(Optional ByVal CardCode As String = "", Optional ByVal CardType As String = "", Optional ByVal GroupCode As String = "",
                                     Optional ByVal Mobile As String = "", Optional ByVal FDate As String = "",
                                      Optional ByVal MobileNoSearch As String = "", Optional ByVal CardNameSearch As String = "", Optional ByVal CardCodeSearch As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_BPDetails")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_CardCode", IIf(CardCode Is Nothing, "", CardCode))
            HanaDbCommand_SP.Parameters.AddWithValue("P_CardType", IIf(CardType Is Nothing, "", CardType))
            HanaDbCommand_SP.Parameters.AddWithValue("P_GroupCode", IIf(GroupCode Is Nothing, "", GroupCode))
            HanaDbCommand_SP.Parameters.AddWithValue("P_Mobile", IIf(Mobile Is Nothing, "", Mobile))
            HanaDbCommand_SP.Parameters.AddWithValue("P_Date", IIf(FDate Is Nothing, "", FDate))
            HanaDbCommand_SP.Parameters.AddWithValue("P_MobileNoSearch", IIf(MobileNoSearch Is Nothing, "", MobileNoSearch))
            HanaDbCommand_SP.Parameters.AddWithValue("P_CardNameSearch", IIf(CardNameSearch Is Nothing, "", CardNameSearch))
            HanaDbCommand_SP.Parameters.AddWithValue("P_CardCodeSearch", IIf(CardCodeSearch Is Nothing, "", CardCodeSearch))
            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetBPShiptoAddressDetails")>
        <HttpGet>
        Public Function GetBPShiptoAddressDetails(Optional ByVal CardCode As String = "", Optional ByVal CardType As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_BPShiptoAddressDetails")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_CardCode", IIf(CardCode Is Nothing, "", CardCode))
            HanaDbCommand_SP.Parameters.AddWithValue("P_CardType", IIf(CardType Is Nothing, "", CardType))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function


        <Route("Api/GetBPBilltoAddressDetails")>
        <HttpGet>
        Public Function GetBPBilltoAddressDetails(Optional ByVal CardCode As String = "", Optional ByVal CardType As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_BPBilltoAddressDetails")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_CardCode", IIf(CardCode Is Nothing, "", CardCode))
            HanaDbCommand_SP.Parameters.AddWithValue("P_CardType", IIf(CardType Is Nothing, "", CardType))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetBPContactPersonDetails")>
        <HttpGet>
        Public Function GetBPContactPersonDetails(Optional ByVal CardCode As String = "", Optional ByVal CardType As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_BPContactPersonDetails")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_CardCode", IIf(CardCode Is Nothing, "", CardCode))
            HanaDbCommand_SP.Parameters.AddWithValue("P_CardType", IIf(CardType Is Nothing, "", CardType))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetItemWiseBatchDetails")>
        <HttpGet>
        Public Function GetItemWiseBatchDetails(Optional ByVal ItemCode As String = "", Optional ByVal BatchNum As String = "",
                                                Optional ByVal WareHouse As String = "", Optional ByVal BranchCode As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_ItemWiseBatchDetails")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_ItemCode", IIf(ItemCode Is Nothing, "", ItemCode))
            HanaDbCommand_SP.Parameters.AddWithValue("P_BatchNum", IIf(BatchNum Is Nothing, "", BatchNum))
            HanaDbCommand_SP.Parameters.AddWithValue("P_WareHouse", IIf(WareHouse Is Nothing, "", WareHouse))
            HanaDbCommand_SP.Parameters.AddWithValue("P_Branch", IIf(BranchCode Is Nothing, "", BranchCode))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetItemWiseBatchDetailsCopy")>
        <HttpGet>
        Public Function GetItemWiseBatchDetailsCopy(Optional ByVal ItemCode As String = "", Optional ByVal BatchNum As String = "",
                                                Optional ByVal WareHouse As String = "", Optional ByVal BranchCode As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            'If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
            '    Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            'End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_ItemWiseBatchDetails_copy")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_ItemCode", IIf(ItemCode Is Nothing, "", ItemCode))
            HanaDbCommand_SP.Parameters.AddWithValue("P_BatchNum", IIf(BatchNum Is Nothing, "", BatchNum))
            HanaDbCommand_SP.Parameters.AddWithValue("P_WareHouse1", IIf(WareHouse Is Nothing, "", WareHouse))
            HanaDbCommand_SP.Parameters.AddWithValue("P_Branch", IIf(BranchCode Is Nothing, "", BranchCode))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetItemWiseSerialDetails")>
        <HttpGet>
        Public Function GetItemWiseSerialDetails(Optional ByVal ItemCode As String = "", Optional ByVal SerialNo As String = "",
                                                 Optional ByVal WareHouse As String = "", Optional ByVal BranchCode As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_ItemWiseSerialDetails")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_ItemCode", IIf(ItemCode Is Nothing, "", ItemCode))
            HanaDbCommand_SP.Parameters.AddWithValue("P_SerialNo", IIf(SerialNo Is Nothing, "", SerialNo))
            HanaDbCommand_SP.Parameters.AddWithValue("P_WareHouse", IIf(WareHouse Is Nothing, "", WareHouse))
            HanaDbCommand_SP.Parameters.AddWithValue("P_Branch", IIf(BranchCode Is Nothing, "", BranchCode))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetSalesEmployeeDetails")>
        <HttpGet>
        Public Function GetSalesEmployeeDetails(Optional ByVal SlpCode As String = "", Optional ByVal BusinessUnit As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_SalesEmployeeDetails")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_SlpCode", IIf(SlpCode Is Nothing, "", SlpCode))
            HanaDbCommand_SP.Parameters.AddWithValue("P_Branch", IIf(BusinessUnit Is Nothing, "", BusinessUnit))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function


        <Route("Api/GetEmployeeDetails")>
        <HttpGet>
        Public Function GetEmployeeDetails(Optional ByVal EmpId As String = "", Optional ByVal BuisnessUnit As String = "",
                                           Optional ByVal Position As String = "", Optional ByVal Vendor As String = "",
                                           Optional ByVal Doctor As String = "N", Optional ByVal Therapist As String = "N") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_EmployeeDetails")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_empId", IIf(EmpId Is Nothing, "", EmpId))
            HanaDbCommand_SP.Parameters.AddWithValue("P_BuisnessUnit", IIf(BuisnessUnit Is Nothing, "", BuisnessUnit))
            HanaDbCommand_SP.Parameters.AddWithValue("P_Position", IIf(Position Is Nothing, "", Position))
            HanaDbCommand_SP.Parameters.AddWithValue("P_Vendor", IIf(Vendor Is Nothing, "", Vendor))
            HanaDbCommand_SP.Parameters.AddWithValue("P_Doctor", IIf(Doctor Is Nothing, "N", Doctor))
            HanaDbCommand_SP.Parameters.AddWithValue("P_Therapist", IIf(Therapist Is Nothing, "N", Therapist))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetTransferRequestApprovalList")>
        <HttpGet>
        Public Function GetTransferRequestApprovalList(Optional ByVal UserCode As String = "", Optional ByVal BuisnessUnit As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_TransferRequestApprovalList")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_UserCode", IIf(UserCode Is Nothing, "", UserCode))
            HanaDbCommand_SP.Parameters.AddWithValue("P_BuisnessUnit", IIf(BuisnessUnit Is Nothing, "", BuisnessUnit))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetTransferRequestApprovedListtoAdded")>
        <HttpGet>
        Public Function GetTransferRequestApprovedListtoAdded(Optional ByVal UserCode As String = "", Optional ByVal BuisnessUnit As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_TransferRequestApprovedListtoAdded")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_UserCode", IIf(UserCode Is Nothing, "", UserCode))
            HanaDbCommand_SP.Parameters.AddWithValue("P_BuisnessUnit", IIf(BuisnessUnit Is Nothing, "", BuisnessUnit))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetStockTransferRequestGetAll")>
        <HttpGet>
        Public Function GetStockTransferRequestGetAll(Optional ByVal BuisnessUnit As String = "", Optional ByVal DocEntry As String = "", Optional ByVal UserCode As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_StockTransferRequestAll")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_Branch", IIf(BuisnessUnit Is Nothing, "", BuisnessUnit))
            HanaDbCommand_SP.Parameters.AddWithValue("P_User", IIf(UserCode Is Nothing, "", UserCode))
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocEntry", IIf(DocEntry Is Nothing, "", DocEntry))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_APIDS(HanaDbCommand_SP))

        End Function

        <Route("Api/GetStockTransferRequestHeader")>
        <HttpGet>
        Public Function GetStockTransferRequestHeader(Optional ByVal BuisnessUnit As String = "", Optional ByVal DocEntry As String = "",
                                                      Optional ByVal UserCode As String = "", Optional ByVal FromDate As String = "",
                                                      Optional ByVal ToDate As String = "", Optional ByVal Status As String = "",
                                                      Optional ByVal DocNum As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_StockTransferRequestHeader")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_Branch", IIf(BuisnessUnit Is Nothing, "", BuisnessUnit))
            HanaDbCommand_SP.Parameters.AddWithValue("P_User", IIf(UserCode Is Nothing, "", UserCode))
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocEntry", IIf(DocEntry Is Nothing, "", DocEntry))
            HanaDbCommand_SP.Parameters.AddWithValue("P_FromDate", IIf(FromDate Is Nothing, "", FromDate))
            HanaDbCommand_SP.Parameters.AddWithValue("P_ToDate", IIf(ToDate Is Nothing, "", ToDate))
            HanaDbCommand_SP.Parameters.AddWithValue("P_Status", IIf(Status Is Nothing, "", Status))
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocNo", IIf(DocNum Is Nothing, "", DocNum))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetStockTransferRequestDetails")>
        <HttpGet>
        Public Function GetStockTransferRequestDetails(Optional ByVal DocEntry As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_StockTransferRequestDetails")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocEntry", IIf(DocEntry Is Nothing, "", DocEntry))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetStockTransferGetAll")>
        <HttpGet>
        Public Function GetStockTransferGetAll(Optional ByVal BuisnessUnit As String = "", Optional ByVal DocEntry As String = "", Optional ByVal UserCode As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_StockTransferAll")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_Branch", IIf(BuisnessUnit Is Nothing, "", BuisnessUnit))
            HanaDbCommand_SP.Parameters.AddWithValue("P_User", IIf(UserCode Is Nothing, "", UserCode))
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocEntry", IIf(DocEntry Is Nothing, "", DocEntry))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_APIDS(HanaDbCommand_SP))

        End Function

        <Route("Api/GetStockTransferHeader")>
        <HttpGet>
        Public Function GetStockTransferHeader(Optional ByVal BuisnessUnit As String = "", Optional ByVal DocEntry As String = "", Optional ByVal UserCode As String = "", Optional ByVal FromDate As String = "", Optional ByVal ToDate As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_StockTransferHeader")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_Branch", IIf(BuisnessUnit Is Nothing, "", BuisnessUnit))
            HanaDbCommand_SP.Parameters.AddWithValue("P_User", IIf(UserCode Is Nothing, "", UserCode))
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocEntry", IIf(DocEntry Is Nothing, "", DocEntry))
            HanaDbCommand_SP.Parameters.AddWithValue("P_FromDate", IIf(FromDate Is Nothing, "", FromDate))
            HanaDbCommand_SP.Parameters.AddWithValue("P_ToDate", IIf(ToDate Is Nothing, "", ToDate))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetStockTransferHeaderECommerce")>
        <HttpGet>
        Public Function GetStockTransferHeaderECommerce(Optional ByVal BuisnessUnit As String = "", Optional ByVal DocEntry As String = "", Optional ByVal UserCode As String = "", Optional ByVal FromDate As String = "", Optional ByVal ToDate As String = "",
                                                        Optional ByVal DelChannel As String = "", Optional ByVal DelAgent As String = "",
                                                        Optional ByVal Mobile As String = "", Optional ByVal Status As String = "",
                                                        Optional ByVal CardCode As String = "", Optional ByVal Area As String = "", Optional ByVal SODocNum As String = "",
                                                        Optional ByVal SODocEntry As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_StockTransferHeaderECommerce")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_Branch", Branch)
            HanaDbCommand_SP.Parameters.AddWithValue("P_User", IIf(UserCode Is Nothing, "", UserCode))
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocEntry", IIf(DocEntry Is Nothing, "", DocEntry))
            HanaDbCommand_SP.Parameters.AddWithValue("P_FromDate", IIf(FromDate Is Nothing, "", FromDate))
            HanaDbCommand_SP.Parameters.AddWithValue("P_ToDate", IIf(ToDate Is Nothing, "", ToDate))
            HanaDbCommand_SP.Parameters.AddWithValue("P_DelChannel", IIf(DelChannel Is Nothing, "", DelChannel))
            HanaDbCommand_SP.Parameters.AddWithValue("P_DelAgent", IIf(DelAgent Is Nothing, "", DelAgent))
            HanaDbCommand_SP.Parameters.AddWithValue("P_Mobile", IIf(Mobile Is Nothing, "", Mobile))
            HanaDbCommand_SP.Parameters.AddWithValue("P_Status", IIf(Status Is Nothing, "", Status))
            HanaDbCommand_SP.Parameters.AddWithValue("P_CardCode", IIf(CardCode Is Nothing, "", CardCode))
            HanaDbCommand_SP.Parameters.AddWithValue("P_Area", IIf(Area Is Nothing, "", Area))
            HanaDbCommand_SP.Parameters.AddWithValue("P_SODocNum", IIf(SODocNum Is Nothing, "", SODocNum))
            HanaDbCommand_SP.Parameters.AddWithValue("P_SODocEntry", IIf(SODocEntry Is Nothing, "", SODocEntry))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetStockTransferDetail")>
        <HttpGet>
        Public Function GetStockTransferDetail(Optional ByVal DocEntry As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_StockTransferDetails")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocEntry", IIf(DocEntry Is Nothing, "", DocEntry))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetStockTransferDetailsBatch")>
        <HttpGet>
        Public Function GetStockTransferDetailsBatch(Optional ByVal DocEntry As String = "", Optional ByVal LineNum As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_StockTransferDetailsBatch")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocEntry", IIf(DocEntry Is Nothing, "", DocEntry))
            HanaDbCommand_SP.Parameters.AddWithValue("P_LineNum", IIf(LineNum Is Nothing, "", LineNum))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetStockTransferDetailsSerial")>
        <HttpGet>
        Public Function GetStockTransferDetailsSerial(Optional ByVal DocEntry As String = "", Optional ByVal LineNum As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_StockTransferDetailsSerial")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocEntry", IIf(DocEntry Is Nothing, "", DocEntry))
            HanaDbCommand_SP.Parameters.AddWithValue("P_LineNum", IIf(LineNum Is Nothing, "", LineNum))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetPrescriptionDinnerList")>
        <HttpGet>
        Public Function GetPrescriptionDinnerList() As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_PrescriptionDinnerList")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function


        <Route("Api/GetPrescriptionLunchList")>
        <HttpGet>
        Public Function GetPrescriptionLunchList() As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_PrescriptionLunchList")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetPrescriptionBreakFastList")>
        <HttpGet>
        Public Function GetPrescriptionBreakFastList() As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_PrescriptionBreakFastList")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetPatientHistoryTestCode")>
        <HttpGet>
        Public Function GetPatientHistoryTestCode() As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_PatientHistoryTestCode")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetDoctorsPrescriptionAll")>
        <HttpGet>
        Public Function GetDoctorsPrescriptionAll(Optional ByVal BuisnessUnit As String = "", Optional ByVal DocEntry As String = "",
                                                  Optional ByVal UserCode As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_SalesQuotationAll")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_Branch", IIf(BuisnessUnit Is Nothing, "", BuisnessUnit))
            HanaDbCommand_SP.Parameters.AddWithValue("P_User", IIf(UserCode Is Nothing, "", UserCode))
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocEntry", IIf(DocEntry Is Nothing, "", DocEntry))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_APIDS(HanaDbCommand_SP))

        End Function

        <Route("Api/GetDoctorsPrescriptionHeader")>
        <HttpGet>
        Public Function GetDoctorsPrescriptionHeader(Optional ByVal BuisnessUnit As String = "",
                                                    Optional ByVal DocEntry As String = "", Optional ByVal UserCode As String = "",
                                                    Optional ByVal FromDate As String = "", Optional ByVal ToDate As String = "",
                                                    Optional ByVal CardCode As String = "", Optional ByVal DocStatus As String = "",
                                                    Optional ByVal DoctorName As String = "", Optional ByVal DocNum As String = "",
                                                    Optional ByVal CardName As String = "", Optional ByVal Mobile As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_SalesQuotationHeader")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_Branch", IIf(BuisnessUnit Is Nothing, "", BuisnessUnit))
            HanaDbCommand_SP.Parameters.AddWithValue("P_User", IIf(UserCode Is Nothing, "", UserCode))
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocEntry", IIf(DocEntry Is Nothing, "", DocEntry))
            HanaDbCommand_SP.Parameters.AddWithValue("P_FromDate", IIf(FromDate Is Nothing, "", FromDate))
            HanaDbCommand_SP.Parameters.AddWithValue("P_ToDate", IIf(ToDate Is Nothing, "", ToDate))
            HanaDbCommand_SP.Parameters.AddWithValue("P_CardCode", IIf(CardCode Is Nothing, "", CardCode))
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocStatus", IIf(DocStatus Is Nothing, "", DocStatus))
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocName", IIf(DoctorName Is Nothing, "", DoctorName))
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocNo", IIf(DocNum Is Nothing, "", DocNum))
            HanaDbCommand_SP.Parameters.AddWithValue("P_CardName", IIf(CardName Is Nothing, "", CardName))
            HanaDbCommand_SP.Parameters.AddWithValue("P_Mobile", IIf(Mobile Is Nothing, "", Mobile))


            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetDoctorsPrescriptionDetail")>
        <HttpGet>
        Public Function GetDoctorsPrescriptionDetail(ByVal DocEntry As String,
                                                    Optional ByVal SelleItem As String = "", Optional ByVal InventoryItem As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_QutotationDetails")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocEntry", IIf(DocEntry Is Nothing, "", DocEntry))
            HanaDbCommand_SP.Parameters.AddWithValue("P_SellItem", IIf(SelleItem Is Nothing, "", SelleItem))
            HanaDbCommand_SP.Parameters.AddWithValue("P_InventoryItem", IIf(InventoryItem Is Nothing, "", InventoryItem))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetPatientHistoryDetails")>
        <HttpGet>
        Public Function GetPatientHistoryDetails(ByVal DocEntry As String) As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_PatientHistoryDetails")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocEntry", IIf(DocEntry Is Nothing, "", DocEntry))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetCostCenterEmployee")>
        <HttpGet>
        Public Function GetCostCenterEmployee() As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_CostCenterEmployee")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetCostCenterDepartment")>
        <HttpGet>
        Public Function GetCostCenterDepartment() As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_CostCenterDepartment")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetCostCenterMachine")>
        <HttpGet>
        Public Function GetCostCenterMachine() As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_CostCenterMachine")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetGoddsIssueAll")>
        <HttpGet>
        Public Function GetGoddsIssueAll(Optional ByVal BuisnessUnit As String = "", Optional ByVal DocEntry As String = "", Optional ByVal UserCode As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_GoddsIssueAll")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_Branch", IIf(BuisnessUnit Is Nothing, "", BuisnessUnit))
            HanaDbCommand_SP.Parameters.AddWithValue("P_User", IIf(UserCode Is Nothing, "", UserCode))
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocEntry", IIf(DocEntry Is Nothing, "", DocEntry))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_APIDS(HanaDbCommand_SP))

        End Function


        <Route("Api/GetGoodsIssueHeader")>
        <HttpGet>
        Public Function GetGoodsIssueHeader(Optional ByVal BuisnessUnit As String = "", Optional ByVal DocEntry As String = "",
                                            Optional ByVal UserCode As String = "", Optional ByVal FromDate As String = "",
                                            Optional ByVal ToDate As String = "", Optional ByVal Status As String = "",
                                            Optional ByVal DocNum As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_GoodsIssueHeader")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_Branch", IIf(BuisnessUnit Is Nothing, "", BuisnessUnit))
            HanaDbCommand_SP.Parameters.AddWithValue("P_User", IIf(UserCode Is Nothing, "", UserCode))
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocEntry", IIf(DocEntry Is Nothing, "", DocEntry))
            HanaDbCommand_SP.Parameters.AddWithValue("P_FromDate", IIf(FromDate Is Nothing, "", FromDate))
            HanaDbCommand_SP.Parameters.AddWithValue("P_ToDate", IIf(ToDate Is Nothing, "", ToDate))
            HanaDbCommand_SP.Parameters.AddWithValue("P_Status", IIf(Status Is Nothing, "", Status))
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocNo", IIf(DocNum Is Nothing, "", DocNum))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetGoodsIssueDetails")>
        <HttpGet>
        Public Function GetGoodsIssueDetails(ByVal DocEntry As String) As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_GoodsIssueDetails")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocEntry", IIf(DocEntry Is Nothing, "", DocEntry))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetGoodsIssueBatchDetails")>
        <HttpGet>
        Public Function GetGoodsIssueBatchDetails(Optional ByVal DocEntry As String = "", Optional ByVal LineNum As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_GoodsIssueBatchDetails")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocEntry", IIf(DocEntry Is Nothing, "", DocEntry))
            HanaDbCommand_SP.Parameters.AddWithValue("P_LineNum", IIf(LineNum Is Nothing, "", LineNum))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetGoodsIssueSerialDetails")>
        <HttpGet>
        Public Function GetGoodsIssueSerialDetails(Optional ByVal DocEntry As String = "", Optional ByVal LineNum As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_GoodsIssueSerialDetails")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocEntry", IIf(DocEntry Is Nothing, "", DocEntry))
            HanaDbCommand_SP.Parameters.AddWithValue("P_LineNum", IIf(LineNum Is Nothing, "", LineNum))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetGoodsReceiptAll")>
        <HttpGet>
        Public Function GetGoodsReceiptAll(Optional ByVal BuisnessUnit As String = "", Optional ByVal DocEntry As String = "", Optional ByVal UserCode As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_GoodsReceiptAll")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_Branch", IIf(BuisnessUnit Is Nothing, "", BuisnessUnit))
            HanaDbCommand_SP.Parameters.AddWithValue("P_User", IIf(UserCode Is Nothing, "", UserCode))
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocEntry", IIf(DocEntry Is Nothing, "", DocEntry))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_APIDS(HanaDbCommand_SP))

        End Function

        <Route("Api/GetGoodsReceiptHeader")>
        <HttpGet>
        Public Function GetGoodsReceiptHeader(Optional ByVal BuisnessUnit As String = "", Optional ByVal DocEntry As String = "",
                                              Optional ByVal UserCode As String = "", Optional ByVal FromDate As String = "",
                                              Optional ByVal ToDate As String = "", Optional ByVal Status As String = "",
                                              Optional ByVal DocNum As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_GoodsReceiptHeader")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_Branch", IIf(BuisnessUnit Is Nothing, "", BuisnessUnit))
            HanaDbCommand_SP.Parameters.AddWithValue("P_User", IIf(UserCode Is Nothing, "", UserCode))
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocEntry", IIf(DocEntry Is Nothing, "", DocEntry))
            HanaDbCommand_SP.Parameters.AddWithValue("P_FromDate", IIf(FromDate Is Nothing, "", FromDate))
            HanaDbCommand_SP.Parameters.AddWithValue("P_ToDate", IIf(ToDate Is Nothing, "", ToDate))
            HanaDbCommand_SP.Parameters.AddWithValue("P_Status", IIf(Status Is Nothing, "", Status))
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocNo", IIf(DocNum Is Nothing, "", DocNum))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetGoodsReceiptDetails")>
        <HttpGet>
        Public Function GetGoodsReceiptDetails(ByVal DocEntry As String) As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_GoodsReceiptDetails")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocEntry", IIf(DocEntry Is Nothing, "", DocEntry))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetGoodsReceiptBatchDetails")>
        <HttpGet>
        Public Function GetGoodsReceiptBatchDetails(Optional ByVal DocEntry As String = "", Optional ByVal LineNum As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_GoodsReceiptBatchDetails")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocEntry", IIf(DocEntry Is Nothing, "", DocEntry))
            HanaDbCommand_SP.Parameters.AddWithValue("P_LineNum", IIf(LineNum Is Nothing, "", LineNum))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetGoodsReceiptSerialDetails")>
        <HttpGet>
        Public Function GetGoodsReceiptSerialDetails(Optional ByVal DocEntry As String = "", Optional ByVal LineNum As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_GoodsReceiptSerialDetails")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocEntry", IIf(DocEntry Is Nothing, "", DocEntry))
            HanaDbCommand_SP.Parameters.AddWithValue("P_LineNum", IIf(LineNum Is Nothing, "", LineNum))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetSalesOrderAll")>
        <HttpGet>
        Public Function GetSalesOrderAll(Optional ByVal BuisnessUnit As String = "", Optional ByVal DocEntry As String = "", Optional ByVal UserCode As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_SalesOrderAll")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_Branch", IIf(BuisnessUnit Is Nothing, "", BuisnessUnit))
            HanaDbCommand_SP.Parameters.AddWithValue("P_User", IIf(UserCode Is Nothing, "", UserCode))
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocEntry", IIf(DocEntry Is Nothing, "", DocEntry))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_APIDS(HanaDbCommand_SP))

        End Function

        <Route("Api/GetSalesOrderHeader")>
        <HttpGet>
        Public Function GetSalesOrderHeader(Optional ByVal BuisnessUnit As String = "", Optional ByVal DocEntry As String = "",
                                            Optional ByVal UserCode As String = "", Optional ByVal FromDate As String = "",
                                            Optional ByVal ToDate As String = "", Optional ByVal CardCode As String = "",
                                            Optional ByVal DocStatus As String = "", Optional ByVal ToBranch As String = "",
                                            Optional ByVal CardName As String = "", Optional ByVal MobileNo As String = "",
                                            Optional ByVal DocNo As String = "", Optional ByVal salesEmployeeId As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_SalesOrderHeader")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_Branch", IIf(BuisnessUnit Is Nothing, "", BuisnessUnit))
            HanaDbCommand_SP.Parameters.AddWithValue("P_User", IIf(UserCode Is Nothing, "", UserCode))
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocEntry", IIf(DocEntry Is Nothing, "", DocEntry))
            HanaDbCommand_SP.Parameters.AddWithValue("P_FromDate", IIf(FromDate Is Nothing, "", FromDate))
            HanaDbCommand_SP.Parameters.AddWithValue("P_ToDate", IIf(ToDate Is Nothing, "", ToDate))
            HanaDbCommand_SP.Parameters.AddWithValue("P_CardCode", IIf(CardCode Is Nothing, "", CardCode))
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocStatus", IIf(DocStatus Is Nothing, "", DocStatus))
            HanaDbCommand_SP.Parameters.AddWithValue("P_CardName", IIf(CardName Is Nothing, "", CardName))
            HanaDbCommand_SP.Parameters.AddWithValue("P_MobileNo", IIf(MobileNo Is Nothing, "", MobileNo))
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocNo", IIf(DocNo Is Nothing, "", DocNo))
            HanaDbCommand_SP.Parameters.AddWithValue("P_SalesEmployee", IIf(salesEmployeeId Is Nothing, "", salesEmployeeId))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetSalesOrderDetails")>
        <HttpGet>
        Public Function GetSalesOrderDetails(Optional ByVal BuisnessUnit As String = "", Optional ByVal DocEntry As String = "",
                                             Optional ByVal UserCode As String = "", Optional ByVal ItemHide As String = "",
                                             Optional ByVal Status As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_SalesOrderDetails")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_Branch", IIf(BuisnessUnit Is Nothing, "", BuisnessUnit))
            HanaDbCommand_SP.Parameters.AddWithValue("P_User", IIf(UserCode Is Nothing, "", UserCode))
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocEntry", IIf(DocEntry Is Nothing, "", DocEntry))
            HanaDbCommand_SP.Parameters.AddWithValue("P_Hide", IIf(ItemHide Is Nothing, "", ItemHide))
            HanaDbCommand_SP.Parameters.AddWithValue("P_Status", IIf(Status Is Nothing, "", Status))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetSalesOrderDetailsCopy")>
        <HttpGet>
        Public Function GetSalesOrderDetailsCopy(Optional ByVal BuisnessUnit As String = "", Optional ByVal DocEntry As String = "", Optional ByVal UserCode As String = "",
        Optional ByVal SelleItem As String = "", Optional ByVal InventoryItem As String = "", Optional ByVal ItemHide As String = "", Optional ByVal Status As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_SalesOrderDetails_Copy")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_Branch", IIf(BuisnessUnit Is Nothing, "", BuisnessUnit))
            HanaDbCommand_SP.Parameters.AddWithValue("P_User", IIf(UserCode Is Nothing, "", UserCode))
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocEntry", IIf(DocEntry Is Nothing, "", DocEntry))
            HanaDbCommand_SP.Parameters.AddWithValue("P_SellItem", IIf(SelleItem Is Nothing, "", SelleItem))
            HanaDbCommand_SP.Parameters.AddWithValue("P_InventoryItem", IIf(InventoryItem Is Nothing, "", InventoryItem))
            HanaDbCommand_SP.Parameters.AddWithValue("P_Hide", IIf(ItemHide Is Nothing, "", ItemHide))
            HanaDbCommand_SP.Parameters.AddWithValue("P_Status", IIf(Status Is Nothing, "", Status))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetSaleInvoiceAll")>
        <HttpGet>
        Public Function GetSaleInvoiceAll(Optional ByVal BuisnessUnit As String = "", Optional ByVal DocEntry As String = "", Optional ByVal UserCode As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_SaleInvoiceAll")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_Branch", IIf(BuisnessUnit Is Nothing, "", BuisnessUnit))
            HanaDbCommand_SP.Parameters.AddWithValue("P_User", IIf(UserCode Is Nothing, "", UserCode))
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocEntry", IIf(DocEntry Is Nothing, "", DocEntry))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_APIDS(HanaDbCommand_SP))

        End Function

        <Route("Api/GetSaleInvoiceHeader")>
        <HttpGet>
        Public Function GetSaleInvoiceHeader(Optional ByVal BuisnessUnit As String = "", Optional ByVal DocEntry As String = "",
                                             Optional ByVal UserCode As String = "", Optional ByVal FromDate As String = "",
                                             Optional ByVal ToDate As String = "", Optional ByVal CardCode As String = "",
                                             Optional ByVal DocStatus As String = "",
                                             Optional ByVal CardName As String = "", Optional ByVal MobileNo As String = "",
                                             Optional ByVal DocNo As String = "", Optional ByVal salesEmployeeId As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_SaleInvoiceHeader")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_Branch", IIf(BuisnessUnit Is Nothing, "", BuisnessUnit))
            HanaDbCommand_SP.Parameters.AddWithValue("P_User", IIf(UserCode Is Nothing, "", UserCode))
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocEntry", IIf(DocEntry Is Nothing, "", DocEntry))
            HanaDbCommand_SP.Parameters.AddWithValue("P_FromDate", IIf(FromDate Is Nothing, "", FromDate))
            HanaDbCommand_SP.Parameters.AddWithValue("P_ToDate", IIf(ToDate Is Nothing, "", ToDate))
            HanaDbCommand_SP.Parameters.AddWithValue("P_CardCode", IIf(CardCode Is Nothing, "", CardCode))
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocStatus", IIf(DocStatus Is Nothing, "", DocStatus))
            HanaDbCommand_SP.Parameters.AddWithValue("P_CardName", IIf(CardName Is Nothing, "", CardName))
            HanaDbCommand_SP.Parameters.AddWithValue("P_MobileNo", IIf(MobileNo Is Nothing, "", MobileNo))
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocNo", IIf(DocNo Is Nothing, "", DocNo))
            HanaDbCommand_SP.Parameters.AddWithValue("P_Sales_Employee", IIf(salesEmployeeId Is Nothing, "", salesEmployeeId))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetSaleInvoiceDetails")>
        <HttpGet>
        Public Function GetSaleInvoiceDetails(Optional ByVal BuisnessUnit As String = "", Optional ByVal DocEntry As String = "", Optional ByVal UserCode As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_SaleInvoiceDetails")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_Branch", IIf(BuisnessUnit Is Nothing, "", BuisnessUnit))
            HanaDbCommand_SP.Parameters.AddWithValue("P_User", IIf(UserCode Is Nothing, "", UserCode))
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocEntry", IIf(DocEntry Is Nothing, "", DocEntry))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetDailyOpertaionStatus")>
        <HttpGet>
        Public Function GetDailyOpertaionStatus() As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_DailyOpertaionStatus")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetActivityStatusMaster")>
        <HttpGet>
        Public Function GetActivityStatusMaster(Optional ByVal StatusCode As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_ActivityStatusMaster")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_StatusCode", IIf(StatusCode Is Nothing, "", StatusCode))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetActivityMaster")>
        <HttpGet>
        Public Function GetActivityMaster(Optional ByVal ActivityTypeCode As String = "", Optional ByVal ActivityCode As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_ActivityMaster")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_ActivityTypeCode", IIf(ActivityTypeCode Is Nothing, "", ActivityTypeCode))
            HanaDbCommand_SP.Parameters.AddWithValue("P_ActivityCode", IIf(ActivityCode Is Nothing, "", ActivityCode))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetActivityTypeMaster")>
        <HttpGet>
        Public Function GetActivityTypeMaster(Optional ByVal ActivityTypeCode As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_ActivityTypeMaster")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_ActivityTypeCode", IIf(ActivityTypeCode Is Nothing, "", ActivityTypeCode))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetActivitySession")>
        <HttpGet>
        Public Function GetActivitySession() As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_ActivitySession")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetActivityList")>
        <HttpGet>
        Public Function GetActivityList(Optional ByVal ActivityCode As String = "", Optional ByVal ActivityType As String = "",
                                        Optional ByVal User As String = "", Optional ByVal BuisnessUnit As String = "",
                                        Optional ByVal EmpId As String = "", Optional ByVal FromDate As String = "",
                                        Optional ByVal ToDate As String = "", Optional ByVal ActivityTypeCode As String = "",
                                        Optional ByVal Status As String = "", Optional ByVal Agent As String = "",
                                        Optional ByVal VisitBranch As String = "", Optional ByVal CardCode As String = "",
                                        Optional ByVal Doctor As String = "", Optional ByVal Therapist As String = "",
                                        Optional ByVal Service As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_ActivityList")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_ActivityCode", IIf(ActivityCode Is Nothing, "", ActivityCode))
            HanaDbCommand_SP.Parameters.AddWithValue("P_ActivityType", IIf(ActivityType Is Nothing, "", ActivityType))
            HanaDbCommand_SP.Parameters.AddWithValue("P_UserId", IIf(User Is Nothing, "", User))
            HanaDbCommand_SP.Parameters.AddWithValue("P_Branch", IIf(BuisnessUnit Is Nothing, "", BuisnessUnit))
            HanaDbCommand_SP.Parameters.AddWithValue("P_FromDate", IIf(FromDate Is Nothing, "", FromDate))
            HanaDbCommand_SP.Parameters.AddWithValue("P_ToDate", IIf(ToDate Is Nothing, "", ToDate))
            HanaDbCommand_SP.Parameters.AddWithValue("P_ActivityTypeCode", IIf(ActivityTypeCode Is Nothing, "", ActivityTypeCode))
            HanaDbCommand_SP.Parameters.AddWithValue("P_EmpId", IIf(EmpId Is Nothing, "", EmpId))
            HanaDbCommand_SP.Parameters.AddWithValue("p_status", IIf(Status Is Nothing, "", Status))
            HanaDbCommand_SP.Parameters.AddWithValue("p_agent", IIf(Agent Is Nothing, "", Agent))
            HanaDbCommand_SP.Parameters.AddWithValue("p_visitBranch", IIf(VisitBranch Is Nothing, "", VisitBranch))
            HanaDbCommand_SP.Parameters.AddWithValue("p_cardcode", IIf(CardCode Is Nothing, "", CardCode))
            HanaDbCommand_SP.Parameters.AddWithValue("p_doctor", IIf(Doctor Is Nothing, "", Doctor))
            HanaDbCommand_SP.Parameters.AddWithValue("p_therapist", IIf(Therapist Is Nothing, "", Therapist))
            HanaDbCommand_SP.Parameters.AddWithValue("p_service", IIf(Service Is Nothing, "", Service))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetStockTransfertoReceiptfromIntransit")>
        <HttpGet>
        Public Function GetStockTransfertoReceiptfromIntransit(Optional ByVal BuisnessUnit As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""

            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_StockTransfertoReceiptfromIntransit")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_Branch", IIf(BuisnessUnit Is Nothing, "", BuisnessUnit))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function


        <Route("Api/GetUPIList")>
        <HttpGet>
        Public Function GetUPIList() As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""

            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_UPIList")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetDayClose")>
        <HttpGet>
        Public Function GetDayClose(Optional ByVal ClosingDate As String = "", Optional ByVal BuisnessUnit As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""

            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_DayClose")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_Date", IIf(ClosingDate Is Nothing, "", ClosingDate))
            HanaDbCommand_SP.Parameters.AddWithValue("P_Branch", IIf(BuisnessUnit Is Nothing, "", BuisnessUnit))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetStateDetails")>
        <HttpGet>
        Public Function GetStateDetails(ByVal CountryCode As String) As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_StateDetails")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_CountryCode", CountryCode)

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetCountryDetails")>
        <HttpGet>
        Public Function GetCountryDetails() As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_CountryDetails")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetInventoryTransType")>
        <HttpGet>
        Public Function GetInventoryTransType() As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_InventoryTransType")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetDeliveryConfirmStatus")>
        <HttpGet>
        Public Function GetDeliveryConfirmStatus() As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_DELCONFSTATUS")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetPaymentBankCodes")>
        <HttpGet>
        Public Function GetPaymentBankCodes(ByVal PaymentMethod As String) As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_PaymentBankCodes")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_PaymentMethod", PaymentMethod)
            HanaDbCommand_SP.Parameters.AddWithValue("P_Branch", Branch)
            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetPaymentMethod")>
        <HttpGet>
        Public Function GetPaymentMethod() As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_PaymentMethod")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetHouseBankDetails")>
        <HttpGet>
        Public Function GetHouseBankDetails() As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_HouseBankDetails")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetCustomerOccupation")>
        <HttpGet>
        Public Function GetCustomerOccupation() As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_OccupationList")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetCustomerRelationShip")>
        <HttpGet>
        Public Function GetCustomerRelationShip() As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_RelationShipList")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetExpenseAccountList")>
        <HttpGet>
        Public Function GetExpenseAccountList() As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_ExpenseAccountList")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetAccountCodeandBalance")>
        <HttpGet>
        Public Function GetAccountCodeandBalance() As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_AccountCodeandBalance")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_Branch", Branch)

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetExpenseJornalHeader")>
        <HttpGet>
        Public Function GetExpenseJornalHeader(Optional ByVal TransId As String = "", Optional ByVal UserCode As String = "",
                                               Optional ByVal FromDate As String = "", Optional ByVal ToDate As String = "",
                                                Optional ByVal DocNum As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_ExpenseJornalHeader")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_Branch", Branch)
            HanaDbCommand_SP.Parameters.AddWithValue("P_User", IIf(UserCode Is Nothing, "", UserCode))
            HanaDbCommand_SP.Parameters.AddWithValue("P_TransId", IIf(TransId Is Nothing, "", TransId))
            HanaDbCommand_SP.Parameters.AddWithValue("P_FromDate", IIf(FromDate Is Nothing, "", FromDate))
            HanaDbCommand_SP.Parameters.AddWithValue("P_ToDate", IIf(ToDate Is Nothing, "", ToDate))
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocNo", IIf(DocNum Is Nothing, "", DocNum))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetExpenseJornalDetails")>
        <HttpGet>
        Public Function GetExpenseJornalDetails(ByVal TransId As String) As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_ExpenseJornalDetails")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_TransId", TransId)

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        '<Route("Api/GetCustomerVoucherBalance")>
        '<HttpGet>
        'Public Function GetCustomerVoucherBalance(ByVal CardCode As String) As HttpResponseMessage

        '    Dim Branch As String = ""
        '    Dim UserID As String = ""


        '    If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
        '        Return Request.CreateResponse(HttpStatusCode.OK, dtError)
        '    End If
        '    Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_CustomerVoucherBalance")
        '    HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
        '    HanaDbCommand_SP.Parameters.AddWithValue("P_CardCode", CardCode)

        '    Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        'End Function

        <Route("Api/GetCustomerVoucherBalance")>
        <HttpGet>
        Public Function GetCustomerVoucherBalance(ByVal CardCode As String, ByVal VoucherType As String, Optional ByVal CardId As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_CustomerVoucherBalance")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_CardCode", CardCode)
            HanaDbCommand_SP.Parameters.AddWithValue("P_VchType", VoucherType)
            HanaDbCommand_SP.Parameters.AddWithValue("P_VchId", IIf(CardId Is Nothing, "", CardId))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function


        <Route("Api/GetActivityTypeforMemu")>
        <HttpGet>
        Public Function GetActivityTypeforMemu(Optional ByVal ActivityType As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_ActivitySelection")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_ActivityType", IIf(ActivityType Is Nothing, "", ActivityType))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetSalesInvoiceCopyforExchangeItem")>
        <HttpGet>
        Public Function GetSalesInvoiceCopyforExchangeItem(Optional ByVal BuisnessUnit As String = "", Optional ByVal User As String = "", Optional ByVal DocEntry As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_SaleInvoiceDetails_CopyCreditNote")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_Branch", IIf(BuisnessUnit Is Nothing, "", BuisnessUnit))
            HanaDbCommand_SP.Parameters.AddWithValue("P_User", IIf(User Is Nothing, "", User))
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocEntry", IIf(DocEntry Is Nothing, "", DocEntry))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetActivityEmpTherapistGroupList")>
        <HttpGet>
        Public Function GetActivityEmpTherapistGroupList(Optional ByVal ActivityType As String = "", Optional ByVal EmpId As String = "", Optional ByVal BuisnessUnit As String = "",
                                                         Optional ByVal User As String = "", Optional ByVal DocEntry As String = "",
                                                         Optional ByVal FromDate As String = "", Optional ByVal ToDate As String = "", Optional ByVal Activity As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_ActivityEmpTherapistGroupList")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_ActivityType", IIf(ActivityType Is Nothing, "", ActivityType))
            HanaDbCommand_SP.Parameters.AddWithValue("P_UserId", IIf(User Is Nothing, "", User))
            HanaDbCommand_SP.Parameters.AddWithValue("P_EmpId", IIf(EmpId Is Nothing, "", EmpId))
            HanaDbCommand_SP.Parameters.AddWithValue("P_Branch", IIf(BuisnessUnit Is Nothing, "", BuisnessUnit))
            HanaDbCommand_SP.Parameters.AddWithValue("P_FromDate", IIf(FromDate Is Nothing, "", FromDate))
            HanaDbCommand_SP.Parameters.AddWithValue("P_ToDate", IIf(ToDate Is Nothing, "", ToDate))
            HanaDbCommand_SP.Parameters.AddWithValue("P_Activity", IIf(Activity Is Nothing, "", Activity))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetActivityEmpTherapistGroupDetailsList")>
        <HttpGet>
        Public Function GetActivityEmpTherapistGroupDetailsList(Optional ByVal ActivityType As String = "", Optional ByVal EmpId As String = "", Optional ByVal BuisnessUnit As String = "",
                                                        Optional ByVal User As String = "", Optional ByVal DocEntry As String = "",
                                                        Optional ByVal FromDate As String = "", Optional ByVal ToDate As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_ActivityEmpTherapistGroupDetailsList")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_ActivityType", IIf(ActivityType Is Nothing, "", ActivityType))
            HanaDbCommand_SP.Parameters.AddWithValue("P_UserId", IIf(User Is Nothing, "", User))
            HanaDbCommand_SP.Parameters.AddWithValue("P_EmpId", IIf(EmpId Is Nothing, "", EmpId))
            HanaDbCommand_SP.Parameters.AddWithValue("P_Branch", IIf(BuisnessUnit Is Nothing, "", BuisnessUnit))
            HanaDbCommand_SP.Parameters.AddWithValue("P_FromDate", IIf(FromDate Is Nothing, "", FromDate))
            HanaDbCommand_SP.Parameters.AddWithValue("P_ToDate", IIf(ToDate Is Nothing, "", ToDate))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetActivityDailyOperationGroupList")>
        <HttpGet>
        Public Function GetActivityDailyOperationGroupList(Optional ByVal ActivityType As String = "", Optional ByVal BuisnessUnit As String = "",
                                                         Optional ByVal User As String = "", Optional ByVal DocEntry As String = "",
                                                         Optional ByVal FromDate As String = "", Optional ByVal ToDate As String = "", Optional ByVal Activity As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_ActivityDailyOperationGroupList")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_ActivityType", IIf(ActivityType Is Nothing, "", ActivityType))
            HanaDbCommand_SP.Parameters.AddWithValue("P_UserId", IIf(User Is Nothing, "", User))
            HanaDbCommand_SP.Parameters.AddWithValue("P_Branch", IIf(BuisnessUnit Is Nothing, "", BuisnessUnit))
            HanaDbCommand_SP.Parameters.AddWithValue("P_FromDate", IIf(FromDate Is Nothing, "", FromDate))
            HanaDbCommand_SP.Parameters.AddWithValue("P_ToDate", IIf(ToDate Is Nothing, "", ToDate))
            HanaDbCommand_SP.Parameters.AddWithValue("P_Activity", IIf(Activity Is Nothing, "", Activity))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetActivityDailyOperationDetailsList")>
        <HttpGet>
        Public Function GetActivityDailyOperationDetailsList(Optional ByVal ActivityType As String = "", Optional ByVal EmpId As String = "", Optional ByVal BuisnessUnit As String = "",
                                                        Optional ByVal User As String = "", Optional ByVal DocEntry As String = "",
                                                        Optional ByVal FromDate As String = "", Optional ByVal ToDate As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_ActivityDailyOperationDetailsList")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_ActivityType", IIf(ActivityType Is Nothing, "", ActivityType))
            HanaDbCommand_SP.Parameters.AddWithValue("P_UserId", IIf(User Is Nothing, "", User))
            HanaDbCommand_SP.Parameters.AddWithValue("P_EmpId", IIf(EmpId Is Nothing, "", EmpId))
            HanaDbCommand_SP.Parameters.AddWithValue("P_Branch", IIf(BuisnessUnit Is Nothing, "", BuisnessUnit))
            HanaDbCommand_SP.Parameters.AddWithValue("P_FromDate", IIf(FromDate Is Nothing, "", FromDate))
            HanaDbCommand_SP.Parameters.AddWithValue("P_ToDate", IIf(ToDate Is Nothing, "", ToDate))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetDailyAllRecivedDetails")>
        <HttpGet>
        Public Function GetDailyAllRecivedDetails(Optional ByVal BuisnessUnit As String = "", Optional ByVal ClosingDate As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_DailyAllRecivedDetails")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_Branch", IIf(BuisnessUnit Is Nothing, "", BuisnessUnit))
            HanaDbCommand_SP.Parameters.AddWithValue("P_Date", IIf(ClosingDate Is Nothing, "", ClosingDate))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetDailyBankRecivedDetails")>
        <HttpGet>
        Public Function GetDailyBankRecivedDetails(Optional ByVal BuisnessUnit As String = "", Optional ByVal ClosingDate As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_DailyBankRecivedDetails")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_Branch", IIf(BuisnessUnit Is Nothing, "", BuisnessUnit))
            HanaDbCommand_SP.Parameters.AddWithValue("P_Date", IIf(ClosingDate Is Nothing, "", ClosingDate))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetDayCloseHeader")>
        <HttpGet>
        Public Function GetDayCloseHeader(Optional ByVal BuisnessUnit As String = "", Optional ByVal ClosingDate As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_DayCloseHeader")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_Branch", IIf(BuisnessUnit Is Nothing, "", BuisnessUnit))
            HanaDbCommand_SP.Parameters.AddWithValue("P_Date", IIf(ClosingDate Is Nothing, "", ClosingDate))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetDayCloseDetails")>
        <HttpGet>
        Public Function GetDayCloseDetails(Optional ByVal BuisnessUnit As String = "", Optional ByVal ClosingDate As String = "", Optional ByVal Code As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_DayCloseDetails")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_Branch", IIf(BuisnessUnit Is Nothing, "", BuisnessUnit))
            HanaDbCommand_SP.Parameters.AddWithValue("P_Date", IIf(ClosingDate Is Nothing, "", ClosingDate))
            HanaDbCommand_SP.Parameters.AddWithValue("P_Code", IIf(Code Is Nothing, "", Code))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetDailyClosingStockNoteDetails")>
        <HttpGet>
        Public Function GetDailyClosingStockNoteDetails() As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_DailyClosingStockNoteDetails")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetThanaMaster")>
        <HttpGet>
        Public Function GetThanaMaster(Optional ByVal DistrictCode As String = "", Optional ByVal ThanaCode As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_THANAMaster")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_ThanaCode", IIf(ThanaCode Is Nothing, "", ThanaCode))
            HanaDbCommand_SP.Parameters.AddWithValue("P_DistrictCode", IIf(DistrictCode Is Nothing, "", DistrictCode))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetItemBalanceList")>
        <HttpGet>
        Public Function GetItemBalanceList(Optional ByVal BusinessUnit As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_ItemBalanceList")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_Branch", IIf(BusinessUnit Is Nothing, "", BusinessUnit))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetSalesOrderCreditDetailsCopy")>
        <HttpGet>
        Public Function GetSalesOrderCreditDetailsCopy(Optional ByVal BuisnessUnit As String = "", Optional ByVal User As String = "", Optional ByVal DocEntry As String = "",
                                                         Optional ByVal SellItem As String = "", Optional ByVal InventoryItem As String = "",
                                                       Optional ByVal Hide As String = "", Optional ByVal Status As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_SalesOrderCreditDetails_Copy")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_Branch", IIf(BuisnessUnit Is Nothing, "", BuisnessUnit))
            HanaDbCommand_SP.Parameters.AddWithValue("P_User", IIf(User Is Nothing, "", User))
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocEntry", IIf(DocEntry Is Nothing, "", DocEntry))
            HanaDbCommand_SP.Parameters.AddWithValue("P_SellItem", IIf(SellItem Is Nothing, "", SellItem))
            HanaDbCommand_SP.Parameters.AddWithValue("P_InventoryItem", IIf(InventoryItem Is Nothing, "", InventoryItem))
            HanaDbCommand_SP.Parameters.AddWithValue("P_Hide", IIf(Hide Is Nothing, "", Hide))
            HanaDbCommand_SP.Parameters.AddWithValue("P_Status", IIf(Status Is Nothing, "", Status))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetSalesChannelData")>
        <HttpGet>
        Public Function GetSalesChannelData() As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_SalesChannelData")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetDeliveryChannelData")>
        <HttpGet>
        Public Function GetDeliveryChannelData() As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_DeliveryChannelData")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetPaymentDetailsInvoice")>
        <HttpGet>
        Public Function GetPaymentDetailsInvoice(ByVal DocEntry As String) As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_Payment_Details_INVOICE")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocEntry", DocEntry)

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetPaymentDetailsOrder")>
        <HttpGet>
        Public Function GetPaymentDetailsOrder(ByVal DocEntry As String) As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_Payment_Details_ORDER")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocEntry", DocEntry)

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/InvoiceForServiceGain")>
        <HttpGet>
        Public Function InvoiceForServiceGain(Optional ByVal DocEntry As String = "", Optional ByVal DocNum As String = "", Optional ByVal BusinessUnit As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_Invoice_for_ServiceGain")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocEntry", IIf(DocEntry Is Nothing, "", DocEntry))
            HanaDbCommand_SP.Parameters.AddWithValue("P_Branch", IIf(BusinessUnit Is Nothing, "", BusinessUnit))
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocNum", IIf(DocNum Is Nothing, "", DocNum))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/InvoiceForServiceGainDetail")>
        <HttpGet>
        Public Function InvoiceForServiceGainDetail(ByVal DocEntry As String) As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_Invoice_for_ServiceGainDetail")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocEntry", DocEntry)

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/TreatmentFolloupDetails")>
        <HttpGet>
        Public Function TreatmentFolloupDetails(Optional ByVal ActivityCode As String = "", Optional ByVal ActivityType As String = "",
                                                Optional ByVal User As String = "", Optional ByVal BusinessUnit As String = "",
                                                Optional ByVal FromDate As String = "", Optional ByVal ToDate As String = "",
                                                Optional ByVal FollowedBy As String = "", Optional ByVal CardCode As String = "",
                                                Optional ByVal Treatment As String = "", Optional ByVal SONO As String = "",
                                                Optional ByVal CardName As String = "", Optional ByVal Mobile As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_TreatmentFolloupDetails")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_ActivityCode", IIf(ActivityCode Is Nothing, "", ActivityCode))
            HanaDbCommand_SP.Parameters.AddWithValue("P_ActivityType", IIf(ActivityType Is Nothing, "", ActivityType))
            HanaDbCommand_SP.Parameters.AddWithValue("P_UserId", IIf(User Is Nothing, "", User))
            HanaDbCommand_SP.Parameters.AddWithValue("P_Branch", IIf(BusinessUnit Is Nothing, "", BusinessUnit))
            HanaDbCommand_SP.Parameters.AddWithValue("P_FromDate", IIf(FromDate Is Nothing, "", FromDate))
            HanaDbCommand_SP.Parameters.AddWithValue("P_ToDate", IIf(ToDate Is Nothing, "", ToDate))
            HanaDbCommand_SP.Parameters.AddWithValue("P_EmpId", IIf(FollowedBy Is Nothing, "", FollowedBy))
            HanaDbCommand_SP.Parameters.AddWithValue("P_CardCode", IIf(CardCode Is Nothing, "", CardCode))
            HanaDbCommand_SP.Parameters.AddWithValue("P_Treatment", IIf(Treatment Is Nothing, "", Treatment))
            HanaDbCommand_SP.Parameters.AddWithValue("P_SONO", IIf(SONO Is Nothing, "", SONO))
            HanaDbCommand_SP.Parameters.AddWithValue("P_CardName", IIf(CardName Is Nothing, "", CardName))
            HanaDbCommand_SP.Parameters.AddWithValue("P_Mobile", IIf(Mobile Is Nothing, "", Mobile))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/DiscBasedOnCustomer")>
        <HttpGet>
        Public Function DiscBasedOnCustomer(ByVal CardCode As String, ByVal BusinessUnit As String, ByVal PostingDate As String) As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_DiscBasedOnCustomer")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_CardCode", CardCode)
            HanaDbCommand_SP.Parameters.AddWithValue("P_Branch", BusinessUnit)
            HanaDbCommand_SP.Parameters.AddWithValue("P_PostDate", PostingDate)

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        'new API
        <Route("Api/DiscBasedOnItem")>
        <HttpPost>
        Public Function DiscBasedOnItem(ByVal Disc_detail As DISC_HEADER) As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim qstr As String = ""
            Dim code As String = DateTime.Now.ToString("yyyyMMddhhmmss")
            'CStr((DateTime.UtcNow - New DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds).Replace(".", "")
            qstr = "do " & vbNewLine &
                    " begin"
            For Each Item As DISC_ITEM In Disc_detail.Item
                qstr = qstr + vbNewLine + "INSERT INTO ""DTS_TEMP_ITEM""(""Code"",""LineNum"",""Item_Code"",""Quantity"") " &
                        "VALUES('" + code + "','" + Item.LineNum + "','" + Item.Item_Code + "','" + Item.Quantity + "');"

            Next
            qstr = qstr + vbNewLine + "end;"
            DBSQL.executeQuery(qstr, "")
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_DiscBasedOnItem")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_Date", Disc_detail.PostingDate)
            HanaDbCommand_SP.Parameters.AddWithValue("P_Time", Disc_detail.PostingTime)
            HanaDbCommand_SP.Parameters.AddWithValue("P_DiscCode", Disc_detail.DiscCode)
            HanaDbCommand_SP.Parameters.AddWithValue("P_HppyHrs", Disc_detail.HappyHrs)
            HanaDbCommand_SP.Parameters.AddWithValue("P_Code", code)
            HanaDbCommand_SP.Parameters.AddWithValue("P_Branch", Disc_detail.BusinesUnit)
            HanaDbCommand_SP.Parameters.AddWithValue("P_Branch", Disc_detail.CardCode)

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/SOInventoryTrns")>
        <HttpGet>
        Public Function SOInventoryTrns(Optional ByVal BusinessUnit As String = "", Optional ByVal FromDate As String = "",
                                        Optional ByVal ToDate As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_SOInventoryTrns")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_Branch", IIf(BusinessUnit Is Nothing, "", BusinessUnit))
            HanaDbCommand_SP.Parameters.AddWithValue("P_FromDate", IIf(FromDate Is Nothing, "", FromDate))
            HanaDbCommand_SP.Parameters.AddWithValue("P_ToDate", IIf(ToDate Is Nothing, "", ToDate))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/SOInventoryTrnsDetails")>
        <HttpGet>
        Public Function SOInventoryTrnsDetails(Optional ByVal DocEntry As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_SOInventoryTrnsDetails")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocEntry", IIf(DocEntry Is Nothing, "", DocEntry))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetBPDetailsAll")>
        <HttpGet>
        Public Function GetBPDetailsAll(Optional ByVal CardCode As String = "", Optional ByVal CardType As String = "",
                                        Optional ByVal GroupCode As String = "", Optional ByVal Mobile As String = "",
                                        Optional ByVal FDate As String = "", Optional ByVal MobileNoSearch As String = "",
                                        Optional ByVal CardNameSearch As String = "", Optional ByVal CardCodeSearch As String = "",
                                        Optional ByVal Doctor As String = "N") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_BPDetailsAll")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_CardCode", IIf(CardCode Is Nothing, "", CardCode))
            HanaDbCommand_SP.Parameters.AddWithValue("P_CardType", IIf(CardType Is Nothing, "", CardType))
            HanaDbCommand_SP.Parameters.AddWithValue("P_GroupCode", IIf(GroupCode Is Nothing, "", GroupCode))
            HanaDbCommand_SP.Parameters.AddWithValue("P_Mobile", IIf(Mobile Is Nothing, "", Mobile))
            HanaDbCommand_SP.Parameters.AddWithValue("P_Date", IIf(FDate Is Nothing, "", FDate))
            HanaDbCommand_SP.Parameters.AddWithValue("P_MobileNoSearch", IIf(MobileNoSearch Is Nothing, "", MobileNoSearch))
            HanaDbCommand_SP.Parameters.AddWithValue("P_CardNameSearch", IIf(CardNameSearch Is Nothing, "", CardNameSearch))
            HanaDbCommand_SP.Parameters.AddWithValue("P_CardCodeSearch", IIf(CardCodeSearch Is Nothing, "", CardCodeSearch))
            HanaDbCommand_SP.Parameters.AddWithValue("P_Doctor", IIf(Doctor Is Nothing, "N", Doctor))
            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetMultiStockTransferRequestHeader")>
        <HttpGet>
        Public Function GetMultiStockTransferRequestHeader(Optional ByVal BuisnessUnit As String = "", Optional ByVal DocEntry As String = "",
                                                      Optional ByVal UserCode As String = "", Optional ByVal FromDate As String = "",
                                                      Optional ByVal ToDate As String = "", Optional ByVal Status As String = "",
                                                      Optional ByVal DocNum As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_MultiStockTransferRequestHeader")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_Branch", IIf(BuisnessUnit Is Nothing, "", BuisnessUnit))
            HanaDbCommand_SP.Parameters.AddWithValue("P_User", IIf(UserCode Is Nothing, "", UserCode))
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocEntry", IIf(DocEntry Is Nothing, "", DocEntry))
            HanaDbCommand_SP.Parameters.AddWithValue("P_FromDate", IIf(FromDate Is Nothing, "", FromDate))
            HanaDbCommand_SP.Parameters.AddWithValue("P_ToDate", IIf(ToDate Is Nothing, "", ToDate))
            HanaDbCommand_SP.Parameters.AddWithValue("P_Status", IIf(Status Is Nothing, "", Status))
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocNo", IIf(DocNum Is Nothing, "", DocNum))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetMultiStockTransferRequestDetails")>
        <HttpGet>
        Public Function GetMultiStockTransferRequestDetails(Optional ByVal DocEntry As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_MultiStockTransferRequestDetails")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocEntry", IIf(DocEntry Is Nothing, "", DocEntry))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetMultiStockTransferRequestSO")>
        <HttpGet>
        Public Function GetMultiStockTransferRequestSO(Optional ByVal DocEntry As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_MultiStockTransferRequestSO")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocEntry", IIf(DocEntry Is Nothing, "", DocEntry))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/SeviveSessionInvoice")>
        <HttpGet>
        Public Function SeviveSessionInvoice(Optional ByVal DocEntry As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_SeviveSessionInvoice")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocEntry", IIf(DocEntry Is Nothing, "", DocEntry))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetECommerceCorrierTrackHeader")>
        <HttpGet>
        Public Function GetECommerceCorrierTrackHeader(Optional ByVal BuisnessUnit As String = "", Optional ByVal DocEntry As String = "",
                                               Optional ByVal UserCode As String = "", Optional ByVal FromDate As String = "",
                                               Optional ByVal ToDate As String = "", Optional ByVal DelChannel As String = "",
                                               Optional ByVal DelAgent As String = "", Optional ByVal Mobile As String = "",
                                               Optional ByVal Status As String = "", Optional ByVal CardCode As String = "",
                                               Optional ByVal Area As String = "", Optional ByVal SODocNum As String = "",
                                               Optional ByVal SODocEntry As String = "", Optional ByVal PageSize As Integer = 50,
                                               Optional ByVal PageNumber As Integer = 1) As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""

            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If

            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_ECommerceCorrierTrackHeaderPagination")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_Branch", Branch)
            HanaDbCommand_SP.Parameters.AddWithValue("P_User", If(UserCode, ""))
            HanaDbCommand_SP.Parameters.AddWithValue("P_DocEntry", If(DocEntry, ""))
            HanaDbCommand_SP.Parameters.AddWithValue("P_FromDate", If(FromDate, ""))
            HanaDbCommand_SP.Parameters.AddWithValue("P_ToDate", If(ToDate, ""))
            HanaDbCommand_SP.Parameters.AddWithValue("P_DelChannel", If(DelChannel, ""))
            HanaDbCommand_SP.Parameters.AddWithValue("P_DelAgent", If(DelAgent, ""))
            HanaDbCommand_SP.Parameters.AddWithValue("P_Mobile", If(Mobile, ""))
            HanaDbCommand_SP.Parameters.AddWithValue("P_Status", If(Status, ""))
            HanaDbCommand_SP.Parameters.AddWithValue("P_CardCode", If(CardCode, ""))
            HanaDbCommand_SP.Parameters.AddWithValue("P_Area", If(Area, ""))
            HanaDbCommand_SP.Parameters.AddWithValue("P_SODocNum", If(SODocNum, ""))
            HanaDbCommand_SP.Parameters.AddWithValue("P_SODocEntry", If(SODocEntry, ""))
            HanaDbCommand_SP.Parameters.AddWithValue("P_PageSize", PageSize)
            HanaDbCommand_SP.Parameters.AddWithValue("P_PageNumber", PageNumber)

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function


        <Route("Api/GetWarehouseDetailsAll")>
        <HttpGet>
        Public Function GetWarehouseDetailsAll(Optional ByVal BusinessUnit As String = "", Optional ByVal WhsCode As String = "", Optional ByVal WhsType As String = "") As HttpResponseMessage
            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_WarehouseDetailsALL")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_BUCode", IIf(BusinessUnit Is Nothing, "", BusinessUnit))
            HanaDbCommand_SP.Parameters.AddWithValue("P_WhsCode", IIf(WhsCode Is Nothing, "", WhsCode))
            HanaDbCommand_SP.Parameters.AddWithValue("P_WhsType", IIf(WhsType Is Nothing, "", WhsType))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function


        'Day close related api
        <Route("Api/GetDayCloseHeaders")>
        <HttpGet>
        Public Function GetStockTransferRequestHeader(Optional ByVal BuisnessUnit As String = "", Optional ByVal FromDate As String = "", Optional ByVal ToDate As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""


            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_DAY_CLOSE_HEADERS")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("P_Branch", IIf(BuisnessUnit Is Nothing, "", BuisnessUnit))
            HanaDbCommand_SP.Parameters.AddWithValue("P_FromDate", IIf(FromDate Is Nothing, "", FromDate))
            HanaDbCommand_SP.Parameters.AddWithValue("P_ToDate", IIf(ToDate Is Nothing, "", ToDate))

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/DayClose/Details")>
        <HttpGet>
        Public Function GetStockTransferRequestHeader(Optional ByVal BuisnessUnit As String = "", Optional ByVal docKey As Long = 0) As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""

            If Not Mod_Main.Authenticate(Request, UserID, Branch, dtError) Then
                Return Request.CreateResponse(HttpStatusCode.OK, dtError)
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_DAY_CLOSE_DETAILS")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure
            HanaDbCommand_SP.Parameters.AddWithValue("DocKey", docKey)

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

        <Route("Api/GetQuestions")>
        <HttpGet>
        Public Function GetGetQuestions(Optional ByVal BuisnessUnit As String = "") As HttpResponseMessage

            Dim Branch As String = ""
            Dim UserID As String = ""

            If Not Mod_Main.VerifiedAccessToken(Request) Then
                Return Request.CreateResponse(HttpStatusCode.Unauthorized, "Invalid request.")
            End If
            Dim HanaDbCommand_SP As HanaCommand = New HanaCommand("DTS_SP_GET_QUESTIONS")
            HanaDbCommand_SP.CommandType = Data.CommandType.StoredProcedure

            Return Request.CreateResponse(HttpStatusCode.OK, Mod_Main.Process_API(HanaDbCommand_SP))

        End Function

    End Class
End Namespace