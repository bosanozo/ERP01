Namespace Common
    ''' <summary>
    ''' ユーザ情報を格納するためのクラス
    ''' </summary>
    Public Class CMUserInfo
        ''' <summary>ユーザＩＤ</summary>
        Public Property Id As String

        ''' <summary>ユーザ名</summary>
        Public Property Name As String

        ''' <summary>ロール</summary>
        Public Property Roles As String()

        ''' <summary>組織コード</summary>
        Public Property SoshikiCd As String

        ''' <summary>組織名</summary>
        Public Property SoshikiName As String

        ''' <summary>組織階層区分</summary>
        Public Property SoshikiKaisoKbn As String
    End Class
End Namespace
