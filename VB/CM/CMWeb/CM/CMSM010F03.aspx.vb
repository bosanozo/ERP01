Imports System.Data

Imports DocumentFormat.OpenXml
Imports SpreadsheetLight

Imports NEXS.ERP.CM
Imports NEXS.ERP.CM.Common
Imports NEXS.ERP.CM.WEB
Imports NEXS.ERP.CM.DA
Imports NEXS.ERP.CM.BL

''' <summary>
''' 組織マスタEXCEL入力
''' </summary>
Public Partial Class CM_CMSM010F03
    Inherits CMBaseForm
    #Region "BLインジェクション用フィールド"
    Protected m_facade As CMSM010BL
    #End Region

    #Region "イベントハンドラ"
    ''' <summary>
    ''' ページロード
    ''' </summary>
    Protected Sub Page_Load(sender As Object, e As EventArgs)
        ' 組織ＣＤ、組織階層区分を設定
        KaishaCd.Value = CMInformationManager.UserInfo.SoshikiCd
        SoshikiLayer.Value = CMInformationManager.UserInfo.SoshikiKaisoKbn

        ' 画面ヘッダ初期化
        Master.Title = "組織マスタEXCEL入力"

        ' 初期表示以外は処理しない
        If IsPostBack Then
            Return
        End If

        ' 操作履歴を出力
        WriteOperationLog()
    End Sub

    ''' <summary>
    ''' EXCEL入力ボタン押下
    ''' </summary>
    Protected Sub BtnExcelInput_Click(sender As Object, e As EventArgs)
        If Not FileUpload1.HasFile Then
            Return
        End If

        Try
            ' アップロードファイルからデータを取り込み
            Dim ds As DataSet = ImportExcel(FileUpload1.PostedFile.InputStream)

            ' データセットを記憶
            Session("ImportDataSet") = ds

            ' DataSource設定
            GridView1.DataSource = ds.Tables(0)
            ' ページセット
            GridView1.PageIndex = 0
            ' バインド
            GridView1.DataBind()
        Catch ex As Exception
            ShowError(ex)
        End Try
    End Sub


    ''' <summary>
    ''' 登録ボタン押下
    ''' </summary>
    Protected Sub BtnUpdate_Click(sender As Object, e As EventArgs)
        If Session("ImportDataSet") Is Nothing Then
            Return
        End If

        ' データセットを取得
        Dim ds As DataSet = DirectCast(Session("ImportDataSet"), DataSet)

        Try
            ' ファサードの呼び出し
            Dim operationTime As DateTime
            m_facade.Upload(ds, operationTime)
        Catch ex As Exception
            ShowError(ex)
        End Try
    End Sub
    #End Region
End Class
