Imports Seasar.Quill.Attrs

Imports NEXS.ERP.CM.Common

Namespace BL
    ''' <summary>
    ''' 認証関連の機能を提供するファサードのインターフェイスです。
    ''' </summary>
    <Implementation(GetType(CMAuthenticationBL))> _
    Public Interface ICMAuthenticationBL
        ''' <summary>
        ''' ユーザIDとパスワードによる認証を実行します。
        ''' このメソッドは、ローカル呼び出しでユーザIDとパスワードの検証だけを行う、
        ''' Webアプリケーション等用のメソッドです。
        ''' </summary>
        ''' <param name="userId">ユーザID。</param>
        ''' <param name="password">パスワード。</param>
        ''' <returns>ユーザが認証できた場合は true 。</returns>
        Function Authenticate(userId As String, password As String) As Boolean

        ''' <summary>
        ''' ユーザIDで指定されたユーザのユーザ情報を取得します。
        ''' このメソッドは、主にサーバ側で認証済みのユーザ情報を取得するために
        ''' 使用されます。
        ''' </summary>
        ''' <param name="userId">ユーザID。</param>
        ''' <returns>ユーザ情報。ユーザ情報が取得できなかった場合は null参照。</returns>
        Function GetUserInfo(userId As String) As CMUserInfo
    End Interface
End Namespace
