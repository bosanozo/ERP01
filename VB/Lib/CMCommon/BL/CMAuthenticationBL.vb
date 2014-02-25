Imports NEXS.ERP.CM.Common
Imports NEXS.ERP.CM.DA

Namespace BL
    ''' <summary>
    ''' 認証管理ファサード
    ''' </summary>
    Public Class CMAuthenticationBL
        Inherits CMBaseBL
        Implements ICMAuthenticationBL
        Private Const ID As String = "ID"
        Private Const NAME As String = "NAME"
        Private Const PASSWD As String = "PASSWD"
        Private Const ROLE As String = "ROLE"

#Region "インジェクション用フィールド"
        Protected m_dataAccess As CMUserInfoDA
#End Region

#Region "コンストラクタ"
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        Public Sub New()
        End Sub
#End Region

        ''' <summary>
        ''' ユーザIDとパスワードによる認証を実行します。
        ''' このメソッドは、ローカル呼び出しでユーザIDとパスワードの検証だけを行う、
        ''' Webアプリケーション等用のメソッドです。
        ''' </summary>
        ''' <param name="userId">ユーザID。</param>
        ''' <param name="password">パスワード。</param>
        ''' <returns>ユーザが認証できた場合は true 。</returns>
        Public Function Authenticate(userId As String, password As String) As Boolean _
            Implements ICMAuthenticationBL.Authenticate
            Dim row As DataRow = GetUserData(userId, password)

            ' ユーザＩＤとパスワードから社員情報を取得（取得できれば true）
            If row IsNot Nothing Then
                CMInformationManager.UserInfo = CreateUserInfo(row)
            End If

            Return row IsNot Nothing
        End Function

        ''' <summary>
        ''' ユーザIDで指定されたユーザのユーザ情報を取得します。
        ''' このメソッドは、主にサーバ側で認証済みのユーザ情報を取得するために
        ''' 使用されます。
        ''' </summary>
        ''' <param name="userId">ユーザID。</param>
        ''' <returns>ユーザ情報。ユーザ情報が取得できなかった場合は null参照。</returns>
        Public Function GetUserInfo(userId As String) As CMUserInfo _
            Implements ICMAuthenticationBL.GetUserInfo
            ' ユーザＩＤから社員情報を取得
            Return CreateUserInfo(GetUserData(userId))
        End Function

        ''' <summary>
        ''' DataRowから<see cref="CMUserInfo"/> を作成します。
        ''' null を渡すと、例外にはならずに null参照を返します。
        ''' このメソッドは、このファサードの各メソッドから使用される内部メソッドです。
        ''' </summary>
        ''' <param name="userData">ユーザの情報を格納しているDataRow。</param>
        ''' <returns>ユーザ情報を保持する <see cref="CMUserInfo"/>。
        ''' <paramref name="userData"/> が null の場合は null参照。</returns>
        Private Function CreateUserInfo(userData As DataRow) As CMUserInfo
            If userData Is Nothing Then
                Return Nothing
            End If

            Dim userInfo As New CMUserInfo()
            ' ユーザ情報の設定
            userInfo.Id = userData(ID).ToString()
            userInfo.Name = userData(NAME).ToString()
            ' ロールの設定
            userInfo.Roles = userData(ROLE).ToString().Split(New Char() {","c}, StringSplitOptions.RemoveEmptyEntries)

            userInfo.SoshikiCd = userData("組織CD").ToString()
            userInfo.SoshikiName = userData("組織名").ToString()
            userInfo.SoshikiKaisoKbn = userData("組織階層区分").ToString()

            Return userInfo
        End Function

        ''' <summary>
        ''' ユーザIDとパスワードで指定されたユーザの情報を格納しているDataRowを取得します。
        ''' このメソッドは、このファサードの各メソッドから使用される内部メソッドです。
        ''' </summary>
        ''' <param name="userId">ユーザID。</param>
        ''' <param name="password">パスワード。</param>
        ''' <returns>指定されたユーザの情報を格納しているDataRow。
        ''' 対象のユーザ情報が取得できなかった場合は null参照。</returns>
        Private Function GetUserData(userId As String, password As String) As DataRow
            ' ユーザのパスワードチェックをプロジェクトでカスタマイズする場合は、
            ' 主にこのメソッドをカスタマイズします（パスワードを直接チェックする場合のみ）。

            ' ユーザＩＤから社員情報を取得
            Dim userData As DataRow = GetUserData(userId)
            If userData Is Nothing Then
                Return Nothing
            End If

            ' パスワードのチェック（パスワードは常に大文字小文字を区別してチェック）
            If password <> userData(PASSWD).ToString() Then
                Return Nothing
            End If

            Return userData
        End Function

        ''' <summary>
        ''' ユーザIDで指定されたユーザの情報を格納しているDataRowを取得します。
        ''' このメソッドは、このファサードの各メソッドから使用される内部メソッドです。
        ''' </summary>
        ''' <param name="userId">ユーザID。</param>
        ''' <returns>指定されたユーザの情報を格納しているDataRow。
        ''' 対象のユーザ情報が取得できなかった場合は null参照。</returns>
        Private Function GetUserData(userId As String) As DataRow
            ' データアクセス層作成
            m_dataAccess.Connection = Connection
            ' ユーザー情報取得
            Return m_dataAccess.FindById(userId)
        End Function
    End Class
End Namespace
