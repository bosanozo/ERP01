Imports System.Data.Common
Imports System.Data.SqlClient

Imports log4net

Imports Seasar.Quill
Imports Seasar.Quill.Database.DataSource.Impl
Imports Seasar.Extension.Tx.Impl
Imports Seasar.Extension.ADO

Imports NEXS.ERP.CM.Common
Imports NEXS.ERP.CM.DA

Namespace BL
    ''' <summary>
    ''' ファサード層の基底クラスです。
    ''' </summary>
    Public Class CMBaseBL
#Region "ロガーフィールド"
        Private m_logger As ILog
#End Region

#Region "インジェクション用フィールド"
        Protected m_commonDA As CMCommonDA
#End Region

        Private m_connection As IDbConnection

#Region "プロパティ"
        ''' <summary>
        ''' ロガー
        ''' </summary>
        Protected ReadOnly Property Log() As ILog
            Get
                Return m_logger
            End Get
        End Property

        ''' <summary>
        ''' 共通処理データアクセス層
        ''' </summary>
        Protected ReadOnly Property CommonDA() As CMCommonDA
            Get
                Return m_commonDA
            End Get
        End Property

        ''' <summary>データソース</summary>
        Public ReadOnly Property DataSource() As SelectableDataSourceProxyWithDictionary
            Get
                Dim inj As QuillInjector = QuillInjector.GetInstance()
                Dim cmp = inj.Container.GetComponent(GetType(SelectableDataSourceProxyWithDictionary))
                Return TryCast(cmp.GetComponentObject(GetType(SelectableDataSourceProxyWithDictionary)), SelectableDataSourceProxyWithDictionary)
            End Get
        End Property

        ''' <summary>コネクション</summary>
        Public Property Connection() As IDbConnection
            Get
                If m_connection Is Nothing Then
                    ' コネクションはfactoryから作成する
                    Dim factory = DbProviderFactories.GetFactory("System.Data.SqlClient")
                    m_connection = factory.CreateConnection()
                    m_connection.ConnectionString = DirectCast(DataSource.GetDataSource(), TxDataSource).ConnectionString
                End If

                Return m_connection
            End Get
            Set(value As IDbConnection)
                m_connection = Value
            End Set
        End Property
#End Region

#Region "コンストラクタ"
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        Public Sub New()
            ' ロガーを取得
            m_logger = LogManager.GetLogger(Me.[GetType]())
        End Sub
#End Region

#Region "protectedメソッド"
#End Region
    End Class
End Namespace
