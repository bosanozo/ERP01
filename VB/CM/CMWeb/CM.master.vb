Imports NEXS.ERP.CM.Common

''' <summary>
''' �\���^�C�v
''' </summary>
Public Enum DisplayType
    ''' <summary>
    ''' �e�L�X�g�{�b�N�X
    ''' </summary>
    TextBox
    ''' <summary>
    ''' ���x��
    ''' </summary>
    Label
End Enum

''' <summary>
''' �}�X�^�y�[�W
''' </summary>
Public Partial Class CMMaster
    Inherits System.Web.UI.MasterPage
    Private m_operationTime As DateTime

    #Region "public�v���p�e�B"
    ''' <summary>
    ''' �y�[�W�^�C�g���\���G���A�ցA�^�C�g����\�����܂�
    ''' </summary>
    Public Property Title() As String
        Get
            Return LabelTitle.Text
        End Get
        Set
            LabelTitle.Text = value
        End Set
    End Property

    ''' <summary>���쎞��</summary>
    Public Property OperationTime() As DateTime
        Get
            Return m_operationTime
        End Get
        Set
            m_operationTime = value
            LabelDateTime.Text = value.ToString("yyyy/MM/dd HH:mm")
        End Set
    End Property

    ''' <summary>��ʂh�c</summary>
    Public Property FormId() As String
        Get
            Return LabelFormId.Text
        End Get
        Set
            LabelFormId.Text = value
        End Set
    End Property

    Public ReadOnly Property Body() As HtmlGenericControl
        Get
            Return Body1
        End Get
    End Property
    #End Region

    #Region "public���\�b�h"
    ''' <summary>
    ''' ���b�Z�[�W�\���G���A�ցA���b�Z�[�W��\�����܂��B
    ''' </summary>
    ''' <remarks>
    ''' ���b�Z�[�W��ʂ��G���[�̏ꍇ�͐ԕ����ŕ\���A�K�C�h�̏ꍇ�͐����ŕ\�����܂�
    ''' �\���������HTML�G���R�[�h���܂�
    ''' �܂��A�\�������񒆂ɉ��s���������ꍇ�́A�����^�O�Œu�����܂��B
    ''' </remarks>
    ''' <param name="argType">���b�Z�[�W���</param>
    ''' <param name="argMessage">�\�����郁�b�Z�[�W������</param>
    Public Sub ShowMessage(argType As String, argMessage As String)
        Dim method As String = Nothing

        Select Case argType(0)
            Case "E"C
                method = "MsgError"
                Exit Select
            Case "W"C
                method = "alert"
                Exit Select
            Case "I"C
                method = "MsgInfo"
                Exit Select
        End Select

        ' �_�C�A���O�\��
        'method,
        Body.Attributes.Add("onload", String.Format("{0}('{1}')", "alert", argMessage.Replace(vbCr & vbLf, "\n")))

        #If HtmlMessage Then
        Dim message As String = Server.HtmlEncode(argMessage)
        message = message.Replace(vbCr & vbLf, "<br/>")
        message = message.Replace(vbCr, "<br/>")
        message = message.Replace(vbLf, "<br/>")

        Select Case argType(0)
            Case "E"C
                LabelMessage.ForeColor = Color.Red
                Exit Select
            Case "W"C
                LabelMessage.ForeColor = Color.Yellow
                Exit Select
            Case "I"C
                LabelMessage.ForeColor = Color.Black
                Exit Select
        End Select

        LabelMessage.Text = message
        #End If
    End Sub
    #End Region

    #Region "�C�x���g�n���h��"
    ''' <summary>
    ''' �y�[�W�� ViewStateUserKey ��ݒ肵�܂��B
    ''' </summary>
    Protected Sub Page_Init(sender As Object, e As EventArgs)
        Dim user As CMUserInfo = CMInformationManager.UserInfo
        If user IsNot Nothing Then
            ' ���[�UID�ƃZ�b�V����ID���A�y�[�W�� ViewStateUserKey �ɐݒ�
            ' �i���[�U�ƃZ�b�V���������؂��邽�߁j
            Page.ViewStateUserKey = user.Id + ":" & Session.SessionID
        End If
    End Sub

    ''' <summary>
    ''' ���O�C�����[�U��ID�A���̂�\�����܂�
    ''' </summary>
    Protected Sub Page_Load(sender As Object, e As EventArgs)
        ' ���[�UID�̐ݒ�
        Dim user As CMUserInfo = CMInformationManager.UserInfo

        ' ���[�U���̂̐ݒ�
        LabelUserName.Text = If(user IsNot Nothing, Server.HtmlEncode(user.Name), "")

        ' ���쎞���̐ݒ�
        OperationTime = DateTime.Now

        ' ��ʂh�c�̐ݒ�
        FormId = "�y" & System.IO.Path.GetFileNameWithoutExtension(Page.AppRelativeVirtualPath) & "�z"

        ' ���b�Z�[�W�N���A
        LabelMessage.Text = ""
    End Sub
    #End Region
End Class
