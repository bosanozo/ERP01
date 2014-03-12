Imports System.Data
Imports System.ServiceModel
Imports System.ServiceModel.Activation
Imports System.ServiceModel.Web

Imports Seasar.Quill

Imports NEXS.ERP.CM.Common
Imports NEXS.ERP.CM.BL

''' <summary>
''' 共通検索AJAXサービス
''' </summary>
<ServiceContract(Namespace:="")>
<AspNetCompatibilityRequirements(RequirementsMode:=AspNetCompatibilityRequirementsMode.Allowed)>
Public Class CMCommonService
#Region "インジェクション用フィールド"
    Protected m_commonBL As ICMCommonBL
#End Region

#Region "コンストラクタ"
    ''' <summary>
    ''' コンストラクタ
    ''' </summary>
    Public Sub New()
        ' インジェクション実行
        Dim injector As QuillInjector = QuillInjector.GetInstance()
        injector.Inject(Me)
    End Sub
#End Region

#Region "サービスメソッド"
    ''' <summary>
    ''' コード値から名称を取得する。
    ''' </summary>
    ''' <param name="argCodeId">コード値要素ID属性</param>
    ''' <param name="argNameId">名称要素ID属性</param>
    ''' <param name="argSelectId">共通検索ID</param>
    ''' <param name="argCode">コード値</param>
    ''' <returns>コード値に対する名称</returns>
    <OperationContract()> _
    <WebGet()> _
    Public Function GetCodeName(argCodeId As String, argNameId As String, argSelectId As String, argCode As String) As CodeName
        Dim name As String = ""

        ' 検索実行
        Dim result As DataTable = m_commonBL.[Select](argSelectId, argCode)
        If result IsNot Nothing AndAlso result.Rows.Count > 0 Then
            name = result.Rows(0)(0).ToString()
        End If

        ' 結果を返却
        Return New CodeName() With {.CodeId = argCodeId, .NameId = argNameId, .Name = name}
    End Function
#End Region

    ''' <summary>
    ''' コード値に対する名称返却クラス
    ''' </summary>
    Public Class CodeName
        ''' <summary>コード値要素ID属性</summary>
        Public Property CodeId() As String

        ''' <summary>名称要素ID属性</summary>
        Public Property NameId() As String

        ''' <summary>コード値に対する名称</summary>
        Public Property Name() As String
    End Class
End Class

