Imports System.IO

Namespace Common
    ''' <summary>
    ''' メッセージソース
    ''' </summary>
    Public Class CMMessageManager
        Private Shared s_messageTable As CMMessageDataSet.MessageDataTable

        ''' <summary>
        ''' メッセージファイル
        ''' </summary>
        Public Shared Property MessageFileDir As String

        ''' <summary>
        ''' メッセージ定義文字列を返す。
        ''' </summary>
        ''' <param name="argMessageCode">メッセージコード</param>
        ''' <param name="argParams">パラメータ</param>
        ''' <returns>メッセージ</returns>
        Public Shared Function GetMessage(argMessageCode As String, ParamArray argParams As Object()) As String
            ' メッセージ定義の読み込み
            If s_messageTable Is Nothing Then
                s_messageTable = New CMMessageDataSet.MessageDataTable()

                Dim files As String() = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + MessageFileDir, "Message*.xml")
                For Each file As String In files
                    s_messageTable.ReadXml(file)
                Next
            End If

            ' メッセージ定義の取得
            Dim rows As DataRow() = s_messageTable.[Select]("Code = '" & argMessageCode & "'")
            If rows.Length = 0 Then
                Throw New Exception("Message.xmlに""" & argMessageCode & """が登録されていません。")
            End If
            Return String.Format(rows(0)("Format").ToString(), argParams)
        End Function
    End Class
End Namespace
