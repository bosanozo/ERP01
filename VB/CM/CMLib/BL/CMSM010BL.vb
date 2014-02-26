Imports NEXS.ERP.CM.Common
Imports NEXS.ERP.CM.DA

Imports Seasar.Quill.Attrs

Namespace BL
    ''' <summary>
    ''' ファサード層
    ''' </summary>
    <Implementation()> _
    Public Class CMSM010BL
        Inherits CMBaseBL
        Implements ICMBaseBL
#Region "インジェクション用フィールド"
        Protected m_dataAccess As CMSM010DA
#End Region

#Region "コンストラクタ"
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        Public Sub New()
        End Sub
#End Region

#Region "ファサードメソッド"
        ''' <summary>
        ''' 組織マスタを検索する。
        ''' </summary>
        ''' <param name="argParam">検索条件</param>
        ''' <param name="argSelectType">検索種別</param>
        ''' <param name="argOperationTime">操作時刻</param>
        ''' <param name="argMessage">結果メッセージ</param>
        ''' <returns>検索結果</returns>
        Public Function [Select](argParam As List(Of CMSelectParam), argSelectType As CMSelectType,
                                 ByRef argOperationTime As DateTime, ByRef argMessage As CMMessage) As DataSet _
                             Implements ICMBaseBL.Select
            ' 操作時刻を設定
            argOperationTime = DateTime.Now

            ' 最大検索件数取得
            CommonDA.Connection = Connection
            Dim maxRow As Integer = CommonDA.GetMaxRow()

            ' 検索実行
            Dim isOver As Boolean
            m_dataAccess.Connection = Connection
            Dim result As DataSet = m_dataAccess.SelectFromXml(argParam,
                argSelectType, maxRow, isOver, "CMSM組織")

            argMessage = Nothing
            ' 検索結果なし
            If result.Tables(0).Rows.Count = 0 Then
                argMessage = New CMMessage("IV001")
                ' 最大検索件数オーバー
            ElseIf isOver Then
                argMessage = New CMMessage("IV002")
            End If

            Return result
        End Function

        ''' <summary>
        ''' 組織マスタにデータを登録する。
        ''' </summary>
        ''' <param name="argUpdateData">更新データ</param>
        ''' <param name="argOperationTime">操作時刻</param>
        ''' <returns>登録したレコード数</returns>
        <CMTransactionAttribute(Timeout:=100)> _
        Public Overridable Function Update(argUpdateData As DataSet, ByRef argOperationTime As DateTime) As Integer _
        Implements ICMBaseBL.Update
            ' 操作時刻を設定
            CommonDA.Connection = Connection
            argOperationTime = CommonDA.GetSysdate()

            Dim uinfo As CMUserInfo = CMInformationManager.UserInfo

            ' 入力値チェック
            CommonDA.ExistCheckFomXml(argUpdateData.Tables(0))

            ' データアクセス層にコネクション設定
            m_dataAccess.Connection = Connection

            ' 登録実行
            Dim cnt As Integer = m_dataAccess.Update(argUpdateData, argOperationTime)

            'throw new Exception("aaa")

            Return cnt
        End Function

        ''' <summary>
        ''' 組織マスタにデータをアップロードする。
        ''' </summary>
        ''' <param name="argUpdateData">更新データ</param>
        ''' <param name="argOperationTime">操作時刻</param>
        ''' <returns>登録したレコード数</returns>
        <CMTransactionAttribute(Timeout:=100)> _
        Public Overridable Function Upload(argUpdateData As DataSet, ByRef argOperationTime As DateTime) As Integer
            ' 操作時刻を設定
            CommonDA.Connection = Connection
            argOperationTime = CommonDA.GetSysdate()

            ' 入力値チェック
            CommonDA.ExistCheckFomXml(argUpdateData.Tables(0))

            ' データアクセス層にコネクション設定
            m_dataAccess.Connection = Connection

            ' 登録実行
            Dim cnt As Integer = m_dataAccess.Upload(argUpdateData, argOperationTime)

            Return cnt
        End Function
#End Region
    End Class
End Namespace
