Imports System.Transactions

Imports Seasar.Framework.Aop
Imports Seasar.Framework.Log

Imports Seasar.Extension.Tx
Imports Seasar.Extension.Tx.Impl
Imports Seasar.Quill.Database.Tx.Impl
Imports Seasar.Quill.Attrs

Namespace BL
    ''' <summary>
    ''' TransactionAttributeがつけられたメソッドの実行をハンドリングし、
    ''' TransactionScopeの設定、及びコネクションの自動オープン/クローズを行う。
    ''' </summary>
    Public Class CMTransactionHandler
        Implements ITransactionHandler
        Private Shared ReadOnly _logger As Logger = Logger.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

#Region "ITransactionHandler メンバ"

        Private Function ITransactionHandler_Handle(invocation As IMethodInvocation, alreadyInTransaction As Boolean) As Object Implements ITransactionHandler.Handle
            Dim began As Boolean = Not alreadyInTransaction

            If began Then
                _logger.Log("DSSR0003", Nothing)
            End If

            ' トランザクション属性を取得
            Dim attrs As Object() = invocation.Method.GetCustomAttributes(GetType(TransactionAttribute), True)

            ' デフォルト値設定
            Dim scopeOption As TransactionScopeOption = TransactionScopeOption.Required
            Dim opt As New TransactionOptions()
            opt.IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted
            opt.Timeout = TimeSpan.FromSeconds(300)

            ' CMTransactionの場合
            If attrs.Length > 0 AndAlso TypeOf attrs(0) Is CMTransactionAttribute Then
                Dim attr As CMTransactionAttribute = TryCast(attrs(0), CMTransactionAttribute)
                If attr.HasScopeOption Then
                    scopeOption = attr.ScopeOption
                End If
                If attr.HasIsolationLevel Then
                    opt.IsolationLevel = attr.IsolationLevel
                End If
                If attr.HasTimeout Then
                    opt.Timeout = TimeSpan.FromSeconds(attr.Timeout)
                End If
            End If

            ' TransactionScope実行
            Using scope As New TransactionScope(scopeOption, opt)
                Try
                    ' コネクション自動オープン
                    Dim bl As CMBaseBL = TryCast(invocation.Target, CMBaseBL)
                    If bl IsNot Nothing AndAlso bl.Connection.State = ConnectionState.Closed Then
                        bl.Connection.Open()
                    End If

                    Dim obj As Object = invocation.Proceed()
                    If began Then
                        _logger.Log("DSSR0004", Nothing)
                    End If
                    scope.Complete()
                    Return obj
                Catch
                    If began Then
                        _logger.Log("DSSR0005", Nothing)
                    End If
                    Throw
                Finally
                    ' コネクション自動クローズ
                    Dim bl As CMBaseBL = TryCast(invocation.Target, CMBaseBL)
                    If bl IsNot Nothing AndAlso bl.Connection.State <> ConnectionState.Closed Then
                        bl.Connection.Close()
                    End If
                End Try
            End Using
        End Function

#End Region
    End Class
End Namespace
