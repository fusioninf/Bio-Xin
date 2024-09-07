Imports System.Xml

Public Class Connection

    Public Class ServerDetails
        'Inherits SoapHeader
        Private _dbServer As String
        Private _dbUid As String
        Private _dbPwd As String
        Private _dbName As String

        Public Property dbServer() As String
            Get
                Return _dbServer
            End Get
            Set(ByVal value As String)
                _dbServer = value
            End Set
        End Property

        Public Property dbUid() As String
            Get
                Return _dbUid
            End Get
            Set(ByVal value As String)
                _dbUid = value
            End Set
        End Property

        Public Property dbPwd() As String
            Get
                Return _dbPwd
            End Get
            Set(ByVal value As String)
                _dbPwd = value
            End Set
        End Property

        Public Property dbName() As String
            Get
                Return _dbName
            End Get
            Set(ByVal value As String)
                _dbName = value
            End Set
        End Property

        Public Sub New()
            MyBase.New()
            If IO.File.Exists(HttpContext.Current.Server.MapPath("~\App_Data\ConnectionInfo.xml")) Then
                Dim xDoc As XmlDocument = New XmlDocument()
                xDoc.Load(HttpContext.Current.Server.MapPath("~\App_Data\ConnectionInfo.xml"))
                Dim xNode As XmlNode = xDoc.SelectSingleNode("//Servers/Server")
                _dbServer = xNode.Attributes.GetNamedItem("dbServer").InnerText
                _dbUid = xNode.Attributes.GetNamedItem("dbUid").InnerText
                _dbPwd = SIL_Encryption.Cryptography.Decrypt(xNode.Attributes.GetNamedItem("dbPwd").InnerText)
                '_dbPwd = ESPL_Encryption.Cryptography.decryptString(xNode.Attributes.GetNamedItem("dbPwd").InnerText)
                _dbName = xNode.Attributes.GetNamedItem("CompanyDB").InnerText
            Else
                Throw New Exception("Connecion File Unavailable !")
            End If
        End Sub

    End Class

    Friend Shared Sub ConnectSAP(ByRef pCompany As SAPbobsCOM.Company, ByVal SAPUId As String, ByVal SAPPwd As String)
        Dim sErrMsg As String = ""
        Dim lErrCode As Integer
        Dim lRetCode As Integer
        Dim password As String

        pCompany = New SAPbobsCOM.Company
        If IO.File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\App_Data\ConnectionInfo.xml") Then
            Dim xDoc As XmlDocument = New XmlDocument()
            xDoc.Load(AppDomain.CurrentDomain.BaseDirectory + "\App_Data\ConnectionInfo.xml")
            Dim xNode As XmlNode = xDoc.SelectSingleNode("//Server")

            Dim sapserver = xNode.Attributes.GetNamedItem("sapdbServer").InnerText
            pCompany.Server = sapserver
            pCompany.DbUserName = xNode.Attributes.GetNamedItem("dbUid").InnerText
            'pCompany.DbPassword = xNode.Attributes.GetNamedItem("dbPwd").InnerText
            pCompany.DbPassword = SIL_Encryption.Cryptography.decryptString(xNode.Attributes.GetNamedItem("dbPwd").InnerText)
            pCompany.UseTrusted = False
            pCompany.CompanyDB = xNode.Attributes.GetNamedItem("CompanyDB").InnerText
            password = SIL_Encryption.Cryptography.encryptString("Bioxin@1234")

            'pCompany.Password = xNode.Attributes.GetNamedItem("SAPPwd").InnerText
            If SAPPwd = "" Then
                pCompany.Password = SIL_Encryption.Cryptography.decryptString(xNode.Attributes.GetNamedItem("SAPPwd").InnerText)
            Else
                pCompany.Password = SAPPwd
            End If

            pCompany.DbServerType = xNode.Attributes.GetNamedItem("dbType").InnerText
            pCompany.LicenseServer = xNode.Attributes.GetNamedItem("licenseServer").InnerText
            If SAPUId = "" Then
                pCompany.UserName = xNode.Attributes.GetNamedItem("SAPUid").InnerText
            Else
                pCompany.UserName = SAPUId
            End If

            lRetCode = pCompany.Connect

            'Dim sapserver = xNode.Attributes.GetNamedItem("sapdbServer").InnerText
            'pCompany.Server = sapserver
            'pCompany.DbUserName = xNode.Attributes.GetNamedItem("dbUid").InnerText
            ''pCompany.DbPassword = xNode.Attributes.GetNamedItem("dbPwd").InnerText
            'pCompany.DbPassword = SIL_Encryption.Cryptography.decryptString(xNode.Attributes.GetNamedItem("dbPwd").InnerText)
            'pCompany.UseTrusted = False
            'pCompany.CompanyDB = xNode.Attributes.GetNamedItem("CompanyDB").InnerText
            ''pCompany.Password = xNode.Attributes.GetNamedItem("SAPPwd").InnerText
            'pCompany.Password = SIL_Encryption.Cryptography.decryptString(xNode.Attributes.GetNamedItem("SAPPwd").InnerText)
            'pCompany.DbServerType = xNode.Attributes.GetNamedItem("dbType").InnerText
            'pCompany.LicenseServer = xNode.Attributes.GetNamedItem("licenseServer").InnerText
            'pCompany.UserName = xNode.Attributes.GetNamedItem("SAPUid").InnerText
            'lRetCode = pCompany.Connect
        Else
            Throw New Exception("Connecion File Unavailable !")
            Environment.Exit(1)
        End If

        If lRetCode <> 0 Then           'error occured
            pCompany.GetLastError(lErrCode, sErrMsg)
            pCompany = Nothing
            Throw New Exception(sErrMsg)
        End If
    End Sub

    Public Shared Function getConnectionString(ByRef DBName As String) As String
        If IO.File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\App_Data\ConnectionInfo.xml") Then
            Dim xDoc As XmlDocument = New XmlDocument()
            xDoc.Load(AppDomain.CurrentDomain.BaseDirectory + "\App_Data\ConnectionInfo.xml")
            Dim xNode As XmlNode = xDoc.SelectSingleNode("//Server")
            DBName = xNode.Attributes.GetNamedItem("CompanyDB").InnerText

            '"Server=" + xNode.Attributes.GetNamedItem("dbServer").InnerText + ";UserID" + xNode.Attributes.GetNamedItem("dbUid").InnerText + ";Password=" + xNode.Attributes.GetNamedItem("dbUid").InnerText + ";Current Schema=" + ESPL_Encryption.Cryptography.decryptString(xNode.Attributes.GetNamedItem("dbPwd").InnerText) + ";"

            Return "Server=" + xNode.Attributes.GetNamedItem("dbServer").InnerText + ";UserID=" + xNode.Attributes.GetNamedItem("dbUid").InnerText + ";Password=" + SIL_Encryption.Cryptography.decryptString(xNode.Attributes.GetNamedItem("dbPwd").InnerText) + ";Current Schema=" + DBName + ";"
        Else
            Throw New Exception("Connecion File Unavailable !")
            Environment.Exit(1)
        End If

        'Return "DATA SOURCE=" + sInfo.dbServer + ";INITIAL CATALOG=" + DBName + ";USER ID=" + sInfo.dbUid + ";PASSWORD=" + sInfo.dbPwd + ";"
    End Function


    Public Shared Function getSessionTime() As String
        If IO.File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\App_Data\ConnectionInfo.xml") Then
            Dim xDoc As XmlDocument = New XmlDocument()
            xDoc.Load(AppDomain.CurrentDomain.BaseDirectory + "\App_Data\ConnectionInfo.xml")
            Dim xNode As XmlNode = xDoc.SelectSingleNode("//Server")
            Return xNode.Attributes.GetNamedItem("SessionTime").InnerText

        Else
            Throw New Exception("Connecion File Unavailable !")
            Environment.Exit(1)
        End If

    End Function



End Class





