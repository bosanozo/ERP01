Imports System.Text
Imports System.Runtime.Serialization

Namespace Common
    ''' <summary>
    ''' 業務アプリケーション例外クラス
    ''' </summary>
    Public Class CMException
        Inherits Exception
#Region "プロパティ"
        ''' <summary>
        ''' メッセージ
        ''' </summary>
        Public Property CMMessage As CMMessage
            Get
                Return DirectCast(Data("CMMessage"), CMMessage)
            End Get
            Set(value As CMMessage)
                Data.Add("CMMessage", value)
            End Set
        End Property
#End Region

#Region "コンストラクタ"
        ' 親クラスから引き継いだコンストラクタ
        Public Sub New()
        End Sub
        Public Sub New(argMessage As String)
            MyBase.New(argMessage)
        End Sub
        Public Sub New(argMessage As String, argInnerException As Exception)
            MyBase.New(argMessage, argInnerException)
        End Sub
        Public Sub New(argSerializationInfo As SerializationInfo, argStreamingContext As StreamingContext)
            MyBase.New(argSerializationInfo, argStreamingContext)
        End Sub

        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="argCMMessage">メッセージデータ</param>
        Public Sub New(argCMMessage As CMMessage)
            CMMessage = argCMMessage
        End Sub

        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="argCMMessage">メッセージデータ</param>
        ''' <param name="argInnerException">例外発生の元となった例外</param>
        Public Sub New(argCMMessage As CMMessage, argInnerException As Exception)
            MyBase.New(argInnerException.Message, argInnerException)
            CMMessage = argCMMessage
        End Sub
#End Region

        ''' <summary>
        ''' メッセージ文字列を返す。
        ''' </summary>
        Public Overrides Function ToString() As String
            If CMMessage IsNot Nothing Then
                ' 全メッセージ文字列を連結
                Dim builder As New StringBuilder(CMMessage.ToString())

                ' メッセージがエラー以外の場合は簡略化する
                If CMMessage.MessageCd.Length >= 1 AndAlso CMMessage.MessageCd(0) <> "E"c Then
                    If InnerException IsNot Nothing Then
                        builder.AppendLine().Append(InnerException.[GetType]().FullName).Append(": ").Append(InnerException.Message)
                    End If
                Else
                    builder.AppendLine().Append(MyBase.ToString())
                End If

                Return builder.ToString()
            Else
                Return MyBase.ToString()
            End If
        End Function
    End Class
End Namespace
