using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ScoreC.Compile.SyntaxTree
{
    class StructTypeInfo : TypeInfo
    {
        public class FieldInfo
        {
            public string Name;
            public TypeInfo TypeInfo;

            public FieldInfo(string name, TypeInfo typeInfo)
            {
#if DEBUG
                Debug.Assert(name != null);
                Debug.Assert(typeInfo != null);
#endif
                Name = name;
                TypeInfo = typeInfo;
            }

            public override bool Equals(object obj)
            {
                var that = obj as FieldInfo;
                if (that == null)
                    return false;
                return Name == that.Name && TypeInfo.Equals(that.TypeInfo);
            }

            public override int GetHashCode() =>
                Name.GetHashCode() ^ TypeInfo.GetHashCode();

            public override string ToString() =>
                Name + " : " + TypeInfo.ToString();
        }

        /// <summary>
        /// The order matters, here!
        /// </summary>
        public List<FieldInfo> Parameters;
        /// <summary>
        /// The order matters, here!
        /// </summary>
        public List<FieldInfo> Fields;

        public StructTypeInfo()
        {
            Parameters = new List<FieldInfo>();
            Fields = new List<FieldInfo>();
        }

        public void AddParameterInfo(string name, TypeInfo typeInfo) =>
            Parameters.Add(new FieldInfo(name, typeInfo));

        public void AddFieldInfo(string name, TypeInfo typeInfo) =>
            Fields.Add(new FieldInfo(name, typeInfo));

        public override string ToString()
        {
            var buffer = new StringBuilder();

            buffer.Append("struct");
            if (Parameters.Count > 0)
            {
                buffer.Append("(");
                buffer.Append(string.Join(", ", Parameters));
                buffer.Append(")");
            }
            buffer.AppendLine(" {");

            foreach (var field in Fields)
                buffer.AppendLine("   " + field);

            buffer.Append("}");

            return buffer.ToString();
        }
    }
}
