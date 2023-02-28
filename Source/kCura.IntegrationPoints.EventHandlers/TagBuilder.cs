using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace kCura.IntegrationPoints.EventHandlers
{
    public class TagBuilder : IDisposable
    {
        private const string NULL_OR_EMPTY = "Argument cannot be null or empty";
        private string _innerHtml;

        public TagBuilder(string tagName)
        {
            if (String.IsNullOrEmpty(tagName))
            {
                throw new ArgumentException(NULL_OR_EMPTY, "tagName");
            }

            TagName = tagName;
            Attributes = new SortedDictionary<string, string>(StringComparer.Ordinal);
        }

        public IDictionary<string, string> Attributes { get; private set; }

        public string InnerHtml
        {
            get { return _innerHtml ?? string.Empty; }
            set { _innerHtml = value; }
        }

        public string TagName { get; private set; }

        public void AddCssClass(string value)
        {
            string currentValue;

            if (Attributes.TryGetValue("class", out currentValue))
            {
                Attributes["class"] = value + " " + currentValue;
            }
            else
            {
                Attributes["class"] = value;
            }
        }

        private void AppendAttributes(StringBuilder sb)
        {
            foreach (var attribute in Attributes)
            {
                string key = attribute.Key;
                if (String.Equals(key, "id", StringComparison.Ordinal /* case-sensitive */) && string.IsNullOrEmpty(attribute.Value))
                {
                    continue; // DevDiv Bugs #227595: don't output empty IDs
                }
                string value = HttpUtility.HtmlAttributeEncode(attribute.Value);
                sb.Append(' ')
                        .Append(key)
                        .Append("=\"")
                        .Append(value)
                        .Append('"');
            }
        }

        public void MergeAttribute(string key, string value)
        {
            MergeAttribute(key, value, replaceExisting: false);
        }

        public void MergeAttribute(string key, string value, bool replaceExisting)
        {
            if (String.IsNullOrEmpty(key))
            {
                throw new ArgumentException(NULL_OR_EMPTY, "key");
            }

            if (replaceExisting || !Attributes.ContainsKey(key))
            {
                Attributes[key] = value;
            }
        }

        public void MergeAttributes<TKey, TValue>(IDictionary<TKey, TValue> attributes)
        {
            MergeAttributes(attributes, replaceExisting: false);
        }

        public void MergeAttributes<TKey, TValue>(IDictionary<TKey, TValue> attributes, bool replaceExisting)
        {
            if (attributes != null)
            {
                foreach (var entry in attributes)
                {
                    string key = Convert.ToString(entry.Key);
                    string value = Convert.ToString(entry.Value);
                    MergeAttribute(key, value, replaceExisting);
                }
            }
        }

        public void SetInnerText(string innerText)
        {
            InnerHtml = HttpUtility.HtmlEncode(innerText);
        }

        public override string ToString()
        {
            return ToString(TagRenderMode.Normal);
        }

        public void Dispose()
        {
        }

        public string ToString(TagRenderMode renderMode)
        {
            StringBuilder sb = new StringBuilder();
            switch (renderMode)
            {
                case TagRenderMode.StartTag:
                    sb.Append('<')
                            .Append(TagName);
                    AppendAttributes(sb);
                    sb.Append('>');
                    break;
                case TagRenderMode.EndTag:
                    sb.Append("</")
                            .Append(TagName)
                            .Append('>');
                    break;
                case TagRenderMode.SelfClosing:
                    sb.Append('<')
                            .Append(TagName);
                    AppendAttributes(sb);
                    sb.Append(" />");
                    break;
                default:
                    sb.Append('<')
                            .Append(TagName);
                    AppendAttributes(sb);
                    sb.Append('>')
                            .Append(InnerHtml)
                            .Append("</")
                            .Append(TagName)
                            .Append('>');
                    break;
            }
            return sb.ToString();
        }
    }
}
public enum TagRenderMode
{
    Normal,
    StartTag,
    EndTag,
    SelfClosing
}
