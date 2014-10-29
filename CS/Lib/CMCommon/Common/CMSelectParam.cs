/*******************************************************************************
 * �y���ʕ��i�z
 *
 * �쐬��: ���i�e�N�m���W�[�^�c�� �]
 * ���ŗ���:
 * 2014.1.30, �V�K�쐬
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace NEXS.ERP.CM.Common
{
    //************************************************************************
    /// <summary>
    /// ���������N���X
    /// </summary>
    //************************************************************************
    [Serializable]
    public class CMSelectParam
    {
        /// <summary>���ږ�</summary>
        public string name;
        /// <summary>��������SQL</summary>
        public string condtion;
        /// <summary>�v���[�X�t�H���_�ɐݒ肷��From�l</summary>
        public object paramFrom;
        /// <summary>�v���[�X�t�H���_�ɐݒ肷��To�l</summary>
        public object paramTo;
        /// <summary>���Ӎ��ږ�(leftcol = @name)</summary>
        public string leftCol;
        /// <summary>����������ǉ�����e�[�u����</summary>
        /// <remarks>���w��̏ꍇ�͑S�e�[�u���̌����ɏ�����ǉ�����B</remarks>
        public string tableName;

        //************************************************************************
        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        /// <param name="argName">���ږ�</param>
        /// <param name="argCondtion">��������SQL</param>
        /// <param name="argValue">�v���[�X�t�H���_�ɐݒ肷��l</param>
        //************************************************************************
        public CMSelectParam(string argName, string argCondtion, object argValue)
            : this(argName, argCondtion, argValue, null) { }

        //************************************************************************
        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        /// <param name="argName">���ږ�</param>
        /// <param name="argCondtion">��������SQL</param>
        /// <param name="argFrom">�v���[�X�t�H���_�ɐݒ肷��From�l</param>
        /// <param name="argTo">�v���[�X�t�H���_�ɐݒ肷��To�l</param>
        //************************************************************************
        public CMSelectParam(string argName, string argCondtion, object argFrom, object argTo)
            : this(null, argName, argCondtion, argFrom, argTo) { }

        //************************************************************************
        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        /// <param name="argLeftCol">���Ӎ��ږ�</param>
        /// <param name="argRightCol">�E�ύ��ږ�</param>
        /// <param name="argCondtion">��������SQL</param>
        /// <param name="argFrom">�v���[�X�t�H���_�ɐݒ肷��From�l</param>
        /// <param name="argTo">�v���[�X�t�H���_�ɐݒ肷��To�l</param>
        //************************************************************************
        public CMSelectParam(string argLeftCol, string argRightCol,
            string argCondtion, object argFrom, object argTo)
        {
            leftCol = argLeftCol;
            name = argRightCol;
            condtion = argCondtion;
            paramFrom = argFrom;
            paramTo = argTo;
        }

        //************************************************************************
        /// <summary>
        /// �����p�����[�^���쐬����B
        /// </summary>
        /// <param name="argQuery">QueryString</param>
        /// <param name="argName">Xml�t�@�C����</param>
        /// <returns>�����p�����[�^</returns>
        //************************************************************************
        public static List<CMSelectParam> CreateSelectParam(NameValueCollection argQuery, string argName = null)
        {
            // �f�[�^�Z�b�g���擾
            CM����DataSet ds = argName != null ? ds = CM����DataSet.ReadFormXml(argName) : null;

            List<CMSelectParam> param = new List<CMSelectParam>();

            foreach (string key in argQuery)
            {
                // �ȉ��͖���
                if (key.StartsWith("_") || key.StartsWith("ctl00$") || key.EndsWith("To") ||
                    System.Text.RegularExpressions.Regex.IsMatch(key, "(nd|rows|page|sidx|sord|oper)")) continue;

                // From�̏ꍇ
                if (key.EndsWith("From"))
                {
                    // From�Ȃ����̎擾
                    string colName = key.Substring(0, key.IndexOf("From"));
                    string toName = colName + "To";

                    bool isSetFrom = !string.IsNullOrEmpty(argQuery[key]);
                    bool isSetTo = !string.IsNullOrEmpty(argQuery[toName]);

                    // FromTo
                    if (isSetFrom && isSetTo)
                    {
                        param.Add(new CMSelectParam(colName,
                            string.Format("BETWEEN @{0} AND @{1}", key, toName),
                            argQuery[key], argQuery[toName]));
                    }
                    // From or To
                    else if (isSetFrom || isSetTo)
                    {
                        string op = isSetFrom ? ">= @" + key : "<= @" + toName;

                        param.Add(new CMSelectParam(colName, op, isSetFrom ? argQuery[key] : argQuery[toName]));
                    }
                }
                // �P�ꍀ�ڂ̏ꍇ
                else
                {
                    // �ݒ肠��̏ꍇ
                    if (string.IsNullOrEmpty(argQuery[key])) continue;

                    string op = "= @";
                    string value = argQuery[key];

                    if (ds != null)
                    {
                        // LIKE�����̏ꍇ
                        var irows = ds.����.Where(item => item.���ږ� == key);
                        if (irows.Count() > 0 && !string.IsNullOrEmpty(irows.First().��v����))
                        {
                            if (irows.First().��v���� != "�w��Ȃ�") op = "LIKE @";

                            switch (irows.First().��v����)
                            {
                                case "�O��":
                                    value = value + "%";
                                    break;
                                case "����":
                                    value = "%" + value + "%";
                                    break;
                                case "���":
                                    value = "%" + value;
                                    break;
                            }
                        }
                    }

                    param.Add(new CMSelectParam(key, op + key, value));
                }
            }

            return param;
        }

        //************************************************************************
        /// <summary>
        /// �����p�����[�^�쐬
        /// </summary>
        /// <returns>�����p�����[�^</returns>
        //************************************************************************
        public static List<CMSelectParam> CreateSelectParam(
            string Name, string Code, string Params, string DbCodeCol, string DbNameCol, string CodeId)
        {
            // ��ʂ̏������擾
            var formParam = new List<CMSelectParam>();

            if (!string.IsNullOrEmpty(Name))
                formParam.Add(new CMSelectParam("Name", "LIKE @Name", "%" + Name + "%"));

            if (!string.IsNullOrEmpty(Code))
                formParam.Add(new CMSelectParam("Code", "= @Code", Code));

            // �����R�[�h��
            var codeCol = Regex.Replace(CodeId, "(From|To)", "");
            var nameCol = Regex.Replace(codeCol, "(CD|ID)", "��");

            // ���ږ��̒u������
            foreach (var p in formParam)
            {
                if (p.name == "Code") p.name = string.IsNullOrEmpty(DbCodeCol) ?
                    codeCol : DbCodeCol;
                else if (p.name == "Name")
                {
                    p.name = string.IsNullOrEmpty(DbNameCol) ?
                       nameCol : DbNameCol;
                    p.condtion = "LIKE @" + p.name;
                    p.paramFrom = "%" + p.paramFrom + "%";
                }
            }

            // �����p�����[�^�쐬
            var param = new List<CMSelectParam>();

            // �ǉ��p�����[�^������ꍇ�A�ǉ�����
            if (!string.IsNullOrEmpty(Params))
            {
                foreach (string p in Params.Split())
                {
                    object value;

                    // "#"����n�܂�ꍇ��UserInfo����ݒ�
                    if (p[0] == '#')
                    {
                        PropertyInfo pi = CMInformationManager.UserInfo.GetType().GetProperty(p.Substring(1));
                        value = pi.GetValue(CMInformationManager.UserInfo, null);
                    }
                    // �Z���̒l���擾
                    else value = p;

                    // �p�����[�^�ǉ�
                    param.Add(new CMSelectParam(null, null, value));
                }
            }

            // ��ʂ̏�����ǉ�
            param.AddRange(formParam);

            return param;
        }
    }
}
