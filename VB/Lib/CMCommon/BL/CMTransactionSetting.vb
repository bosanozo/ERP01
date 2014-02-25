Imports Seasar.Extension.Tx
Imports Seasar.Extension.Tx.Impl
Imports Seasar.Quill.Database.Tx.Impl

Namespace BL
    Public Class CMTransactionSetting
        Inherits TypicalTransactionSetting
        ''' <summary>
        ''' Transactionハンドラ生成
        ''' </summary>
        ''' <returns>Transactionハンドラ</returns>
        Protected Overrides Function CreateTransactionHandler() As ITransactionHandler
            Return New CMTransactionHandler()
        End Function
    End Class
End Namespace
