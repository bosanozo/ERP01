Imports System.IO
Imports System.ComponentModel

Imports NEXS.ERP.CM.Common

Namespace DA
    ''' <summary>
    ''' SqlCommand設定コレクションクラス
    ''' </summary>
    Public Class CMCmdSettings
#Region "プロパティ"
        <Category("共通部品")> _
        <Description("SqlCommand設定のコレクション")> _
        Public Property CmdSettings As List(Of CMCmdSetting)
#End Region

#Region "インデクサ"
        ''' <summary>指定IndexのSqlCommand設定を返します。</summary>
        Default Public ReadOnly Property Item(argIndex As Integer) As CMCmdSetting
            Get
                Return CmdSettings(argIndex)
            End Get
        End Property

        ''' <summary>指定のテーブル名のSqlCommand設定を返します。</summary>
        Default Public ReadOnly Property Item(argName As String) As CMCmdSetting
            Get
                Dim result = From row In CmdSettings Where row.Name = argName
                Return result.First()
            End Get
        End Property
#End Region

#Region "コンストラクタ"
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        Public Sub New()
            CmdSettings = New List(Of CMCmdSetting)()
        End Sub

        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="argFname">読み込むXMLファイル名(拡張子なし)</param>
        Public Sub New(argFname As String)
            Me.New()
            AddFomXml(argFname)
        End Sub
#End Region

        ''' <summary>
        ''' SqlCommand設定をXMLファイルから追加する。
        ''' </summary>
        ''' <param name="argFnames">読み込むXMLファイル名(拡張子なし)</param>
        Public Sub AddFomXml(ParamArray argFnames As String())
            For Each fname As String In argFnames
                ' データセットにファイルを読み込み
                Dim ds As New CMEntityDataSet()
                ds.ReadXml(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Model", fname & ".xml"))

                Dim cmdSetting As New CMCmdSetting()

                ' テーブル名を設定
                cmdSetting.Name = ds.エンティティ(0).テーブル名

                ' パラメータ設定
                Dim paramList As New List(Of CMCmdParam)()
                For Each row As CMEntityDataSet.項目Row In ds.項目
                    ' 更新対象外は無視
                    If row.更新対象外 Then
                        Continue For
                    End If

                    Dim cmdParam As New CMCmdParam()
                    cmdParam.Name = row.項目名
                    cmdParam.DbType = DirectCast([Enum].Parse(GetType(CMDbType), row.項目型), CMDbType)
                    cmdParam.IsKey = row.Key
                    cmdParam.SourceColumn = row.SourceColumn

                    paramList.Add(cmdParam)
                Next
                cmdSetting.ColumnParams = paramList.ToArray()

                ' 設定を追加
                CmdSettings.Add(cmdSetting)
            Next
        End Sub
    End Class
End Namespace
