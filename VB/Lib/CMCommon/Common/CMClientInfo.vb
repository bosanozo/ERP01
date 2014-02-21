Imports System.Web

Namespace Common
    ''' <summary>
    ''' クライアントからサーバに送信された情報を保持するクラスです。
    ''' </summary>
    Public Class CMClientInfo
        ''' <summary>
        ''' クライアントのマシン名。
        ''' </summary>
        Public ReadOnly Property MachineName As String
            Get
                Return HttpContext.Current.Request.UserHostAddress
            End Get
        End Property

        ''' <summary>
        ''' 今回のサーバ呼び出しリクエストID。
        ''' </summary>
        Public ReadOnly Property FormId As String
            Get
                Return System.IO.Path.GetFileNameWithoutExtension(HttpContext.Current.Request.Path)
            End Get
        End Property
    End Class
End Namespace
