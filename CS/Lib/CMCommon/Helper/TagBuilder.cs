using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEXS.ERP.CM.Helper
{
    //************************************************************************
    /// <summary>
    /// HTMLタグを生成する。
    /// </summary>
    //************************************************************************
    public class TagBuilder
    {
        #region プロパティ
        public string TagName { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public object Value { get; set; }
        public string Text { get; set; }
        public List<string> CssClass { get; set; }
        public Dictionary<string, object> Attributes { get; set; }
        public List<TagBuilder> Children { get; set; }
        #endregion

        //************************************************************************
        /// <summary>
        /// コンストラクタ
        /// </summary>
        //************************************************************************
        public TagBuilder()
        {
            CssClass = new List<string>();
            Attributes = new Dictionary<string, object>();
            Children = new List<TagBuilder>();
        }

        //************************************************************************
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="argTagName">タグ名</param>
        //************************************************************************
        public TagBuilder(string argTagName) : this() 
        {
            TagName = argTagName;
        }

        //************************************************************************
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="argTagName">タグ名</param>
        /// <param name="argCssClass">CssClass</param>
        //************************************************************************
        public TagBuilder(string argTagName, string argCssClass)
            : this(argTagName)
        {
            CssClass.Add(argCssClass);
        }

        //************************************************************************
        /// <summary>
        /// タグ化された文字列を返す。
        /// </summary>
        /// <param name="argLevel">インデントレベル</param>
        //************************************************************************
        public string ToString(int argLevel = 1)
        {
            var sb = new StringBuilder();

            sb.Append(' ', argLevel * 4).AppendFormat("<{0}", TagName);
            if (!string.IsNullOrEmpty(Id)) sb.AppendFormat(" id=\"{0}\"", Id);
            if (!string.IsNullOrEmpty(Name)) sb.AppendFormat(" name=\"{0}\"", Name);
            if (!string.IsNullOrEmpty(Type)) sb.AppendFormat(" type=\"{0}\"", Type);
            if (Value != null) sb.AppendFormat(" value=\"{0}\"", Value);
            if (CssClass.Count > 0) sb.AppendFormat(" class=\"{0}\"", string.Join(" ", CssClass));
            foreach (var kvp in Attributes) sb.AppendFormat(" {0}=\"{1}\"", kvp.Key, kvp.Value);

            if (Children.Count > 0 || !string.IsNullOrEmpty(Text))
            {
                sb.Append(">");
                sb.Append(Text);
                if (Children.Count > 0) sb.AppendLine();
                Children.ForEach(c => sb.AppendLine(c.ToString(argLevel + 1)));
                if (Children.Count > 0) sb.Append(' ', argLevel * 4);
                sb.AppendFormat("</{0}>", TagName);
            }
            else sb.AppendFormat("></{0}>", TagName);
            //else sb.Append("/>");

            return sb.ToString();
        }
    }
}
