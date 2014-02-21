Imports System.Web

Namespace Common
    ''' <summary>
    ''' アプリケーション情報、ユーザ情報等へのアクセスを提供します。
    ''' </summary>
    Public NotInheritable Class CMInformationManager
        Private Shared m_clientInfo As CMClientInfo

        ''' <summary>
        ''' ユーザ情報
        ''' </summary>
        Public Shared Property UserInfo As CMUserInfo
            Get
                If HttpContext.Current.Session("UserInfo") Is Nothing Then
                    Dim uinfo = New CMUserInfo()
                    uinfo.Id = "TEST01"
                    uinfo.Name = "テスト０１"
                    uinfo.SoshikiCd = "0001"
                    uinfo.SoshikiName = "組織0001"
                    uinfo.SoshikiKaisoKbn = CMSoshikiKaiso.ALL
                    Return uinfo
                End If

                Return TryCast(HttpContext.Current.Session("UserInfo"), CMUserInfo)
            End Get
            Set(value As CMUserInfo)
                HttpContext.Current.Session("UserInfo") = value
            End Set
        End Property

        ''' <summary>
        ''' クライアント情報
        ''' </summary>
        Public Shared ReadOnly Property ClientInfo As CMClientInfo
            Get
                If m_clientInfo Is Nothing Then
                    m_clientInfo = New CMClientInfo()
                End If

                Return m_clientInfo
            End Get
        End Property
    End Class
End Namespace
