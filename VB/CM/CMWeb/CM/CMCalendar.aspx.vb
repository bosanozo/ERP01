''' <summary>
''' ���t�I�����
''' </summary>
Public Partial Class CMCalender
    Inherits System.Web.UI.Page
    ''' <summary>
    ''' ��������
    ''' </summary>
    Protected Sub DdlMonth_Init(sender As Object, e As EventArgs)
        ' ���X�g�ݒ�
        For i As Integer = 1 To 12
            DdlMonth.Items.Add(i.ToString())
        Next
    End Sub

    ''' <summary>
    ''' �y�[�W���[�h
    ''' </summary>
    Protected Sub Page_Load(sender As Object, e As EventArgs)
        If Not IsPostBack Then
            Dim selectDate As DateTime
            ' ���t�����͂���Ă���ΑI�����ĕ\��
            If DateTime.TryParse(Request.Params("value"), selectDate) Then
                Calendar1.VisibleDate = selectDate
            Else
                selectDate = DateTime.Now.[Date]
            End If

            ' �N������
            For i As Integer = -5 To 5
                DdlYear.Items.Add((selectDate.Year + i).ToString())
            Next
            DdlYear.SelectedValue = selectDate.Year.ToString()
            DdlMonth.SelectedValue = selectDate.Month.ToString()
            ' ���t�I��
            Calendar1.SelectedDate = selectDate
        End If
    End Sub

    ''' <summary>
    ''' ���t�I�����C�x���g�n���h��
    ''' </summary>
    Protected Sub Calendar1_SelectionChanged(sender As Object, e As EventArgs)
        Body1.Attributes.Add("onload", "window.returnValue = '" & Calendar1.SelectedDate.ToString("yyyy/MM/dd") & "'; window.close()")
    End Sub

    ''' <summary>
    ''' �N���ύX
    ''' </summary>
    Protected Sub DdlSelectedIndexChanged(sender As Object, e As EventArgs)
        Refresh()
    End Sub

    ''' <summary>
    ''' �O��
    ''' </summary>
    Protected Sub LbPrev_Click(sender As Object, e As EventArgs)
        If DdlMonth.SelectedIndex = 0 Then
            DdlMonth.SelectedIndex = DdlMonth.Items.Count - 1
            DdlYear.SelectedIndex -= 1
        Else
            DdlMonth.SelectedIndex -= 1
        End If

        Refresh()
    End Sub

    ''' <summary>
    ''' ����
    ''' </summary>
    Protected Sub LbNext_Click(sender As Object, e As EventArgs)
        If DdlMonth.SelectedIndex = DdlMonth.Items.Count - 1 Then
            DdlMonth.SelectedIndex = 0
            DdlYear.SelectedIndex += 1
        Else
            DdlMonth.SelectedIndex += 1
        End If

        Refresh()
    End Sub

    ''' <summary>
    ''' �\���X�V
    ''' </summary>
    Private Sub Refresh()
        ' �N�h���b�v�_�E�����X�g�X�V
        Dim year As Integer = Convert.ToInt32(DdlYear.SelectedValue)
        DdlYear.Items.Clear()
        For i As Integer = -5 To 5
            DdlYear.Items.Add((year + i).ToString())
        Next
        DdlYear.SelectedValue = year.ToString()

        ' �J�����_�[�X�V
        Dim [date] As New DateTime(year, Convert.ToInt32(DdlMonth.SelectedValue), Calendar1.SelectedDate.Day)
        Calendar1.VisibleDate = [date]
        Calendar1.SelectedDate = [date]
    End Sub
End Class
