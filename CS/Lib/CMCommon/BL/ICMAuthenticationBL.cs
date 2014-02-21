using Seasar.Quill.Attrs;

using NEXS.ERP.CM.Common;

namespace NEXS.ERP.CM.BL
{
    //************************************************************************
    /// <summary>
    /// �F�؊֘A�̋@�\��񋟂���t�@�T�[�h�̃C���^�[�t�F�C�X�ł��B
    /// </summary>
    //************************************************************************
    [Implementation(typeof(CMAuthenticationBL))]
    public interface ICMAuthenticationBL
    {
        //************************************************************************
        /// <summary>
        /// ���[�UID�ƃp�X���[�h�ɂ��F�؂����s���܂��B
        /// ���̃��\�b�h�́A���[�J���Ăяo���Ń��[�UID�ƃp�X���[�h�̌��؂������s���A
        /// Web�A�v���P�[�V�������p�̃��\�b�h�ł��B
        /// </summary>
        /// <param name="userId">���[�UID�B</param>
        /// <param name="password">�p�X���[�h�B</param>
        /// <returns>���[�U���F�؂ł����ꍇ�� true �B</returns>
        //************************************************************************
        bool Authenticate(string userId, string password);

        //************************************************************************
        /// <summary>
        /// ���[�UID�Ŏw�肳�ꂽ���[�U�̃��[�U�����擾���܂��B
        /// ���̃��\�b�h�́A��ɃT�[�o���ŔF�؍ς݂̃��[�U�����擾���邽�߂�
        /// �g�p����܂��B
        /// </summary>
        /// <param name="userId">���[�UID�B</param>
        /// <returns>���[�U���B���[�U��񂪎擾�ł��Ȃ������ꍇ�� null�Q�ƁB</returns>
        //************************************************************************
        CMUserInfo GetUserInfo(string userId);
    }
}
