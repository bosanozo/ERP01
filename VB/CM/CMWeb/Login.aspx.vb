Imports Seasar.Quill

Imports NEXS.ERP.CM.Common
Imports NEXS.ERP.CM.BL

''' <summary>
''' ���O�C�����
''' </summary>
''' <remarks>
''' ���O�C���A���O�A�E�g���������{
''' </remarks>
''' <author></author>
''' <date>2006/03/31</date>
''' <version>�V�K�쐬</version>
Public Partial Class Login
    Inherits System.Web.UI.Page
    #Region "�C���W�F�N�V�����p�t�B�[���h"
    Protected m_authenticationBL As ICMAuthenticationBL
    #End Region

    #Region "�R���X�g���N�^"
    ''' <summary>
    ''' �R���X�g���N�^
    ''' </summary>
    Public Sub New()
        ' �C���W�F�N�V�������s
        Dim injector As QuillInjector = QuillInjector.GetInstance()
        injector.Inject(Me)
    End Sub
    #End Region

    #Region "�C�x���g�n���h��"
    ''' <summary>
    ''' Page Init(��ʏ�����)
    ''' </summary>
    Protected Sub Page_Init(sender As Object, e As EventArgs)
        ' ���̃y�[�W�̓��[�U�̃��O�C����Ԃ��m�肵�Ă��Ȃ����߁A���[�U�̑���ɂ���Ă�
        ' ����y�[�W�̕\�����Ƀ��[�U�̃��O�C����Ԃ��ω�����\��������B
        ' ���[�U�̃��O�C����Ԃ��ω�����ƁAViewStateUserKey�̏�Ԃ��s���ƂȂ邽�߁A
        ' ���̃y�[�W�ł͖����I�� Nothing ��ݒ肵�A���[�U��Ԃɂ�錟�؂𖳌��ɂ��Ă���B
        ViewStateUserKey = Nothing
    End Sub

    ''' <summary>
    ''' Page Load(��ʕ\���j
    ''' </summary>
    Protected Sub Page_Load(sender As Object, e As EventArgs)
        ' ��ʃ^�C�g���ݒ�
        Master.Title = "���O�C��"
    End Sub

    ''' <summary>
    ''' �u���O�C���v�{�^���@Click
    ''' </summary>
    Protected Sub ButtonLogin_Click(sender As Object, e As EventArgs)
        ' ���[�U�h�c�A�p�X���[�h�ɂ��F�؊m�F
        Dim userId As String = TextBoxUserId.Text.ToUpper().Trim()
        Dim password As String = TextBoxPassword.Text.Trim()
        Dim authenticated As Boolean = m_authenticationBL.Authenticate(userId, password)
        If authenticated Then
            ' �t�H�[���F�؃`�P�b�g�̔��s
            FormsAuthentication.SetAuthCookie(userId, False)

            ' ���j���[��ʂ֑J��
            'Response.Redirect("~/CM/CMSM010F01.aspx")
            Response.Redirect("Menu.aspx")
        Else
            ' �F�؎��s���G���[���b�Z�[�W�̕\��
            Master.ShowMessage("E", "ID�܂��̓p�X���[�h���s���ł��B")
        End If
    End Sub
    #End Region
End Class
