Imports System.Transactions

Imports Seasar.Framework.Aop
Imports Seasar.Framework.Log

Imports Seasar.Extension.Tx
Imports Seasar.Extension.Tx.Impl
Imports Seasar.Quill.Database.Tx.Impl
Imports Seasar.Quill.Attrs

Namespace BL
    ''' <summary>
    ''' BL層のメソッドにつけるTransaction制御のためのAttribute
    ''' </summary>
    Public Class CMTransactionAttribute
        Inherits TransactionAttribute
        Private m_scopeOption As System.Nullable(Of TransactionScopeOption)
        Private m_isolationLevel As System.Nullable(Of IsolationLevel)
        Private m_timeout As System.Nullable(Of Integer)

#Region "プロパティ"
        ''' <summary>TransactionScopeOption</summary>
        Public Property ScopeOption() As TransactionScopeOption
            Get
                Return m_scopeOption.Value
            End Get
            Set(value As TransactionScopeOption)
                m_scopeOption = value
            End Set
        End Property

        ''' <summary>TransactionScopeOption有無</summary>
        Public ReadOnly Property HasScopeOption() As Boolean
            Get
                Return m_scopeOption.HasValue
            End Get
        End Property

        ''' <summary>トランザクション分離レベル</summary>
        Public Property IsolationLevel() As IsolationLevel
            Get
                Return m_isolationLevel.Value
            End Get
            Set(value As IsolationLevel)
                m_isolationLevel = value
            End Set
        End Property

        ''' <summary>トランザクション分離レベル有無</summary>
        Public ReadOnly Property HasIsolationLevel() As Boolean
            Get
                Return m_isolationLevel.HasValue
            End Get
        End Property

        ''' <summary>トランザクションタイムアウト秒数</summary>
        Public Property Timeout() As Integer
            Get
                Return m_timeout.Value
            End Get
            Set(value As Integer)
                m_timeout = value
            End Set
        End Property

        ''' <summary>トランザクションタイムアウト秒数有無</summary>
        Public ReadOnly Property HasTimeout() As Boolean
            Get
                Return m_timeout.HasValue
            End Get
        End Property
#End Region

#Region "コンストラクタ"
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        Public Sub New()
        End Sub

        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="argTimeout">トランザクションタイムアウト秒数</param>
        Public Sub New(argTimeout As Integer)
            Timeout = argTimeout
        End Sub
#End Region
    End Class
End Namespace
