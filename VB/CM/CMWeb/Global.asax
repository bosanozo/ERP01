<%@ Application Language="VB" %>

<script runat="server">

    Sub Application_Start(ByVal sender As Object, ByVal e As EventArgs)
        ' アプリケーションのスタートアップで実行するコードです
        ' log4net初期化
        log4net.Config.XmlConfigurator.Configure()
    End Sub
    
    Sub Application_End(ByVal sender As Object, ByVal e As EventArgs)
        ' アプリケーションのシャットダウンで実行するコードです
    End Sub
        
    Sub Application_Error(ByVal sender As Object, ByVal e As EventArgs)
        ' ハンドルされていないエラーが発生したときに実行するコードです
    End Sub

    Sub Session_Start(ByVal sender As Object, ByVal e As EventArgs)
        ' 新規セッションを開始したときに実行するコードです
    End Sub

    Sub Session_End(ByVal sender As Object, ByVal e As EventArgs)
        ' セッションが終了したときに実行するコードです 
        ' メモ: Session_End イベントは、Web.config ファイル内で sessionstate モードが
        ' InProc に設定されているときのみ発生します。session モードが StateServer か、または 
        ' SQLServer に設定されている場合、イベントは発生しません。
    End Sub
       
</script>