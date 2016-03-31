using System.Collections.Generic;

namespace ScoreC.Compile.SyntaxTree
{
    using Source;

    class QualifiedTypeInfo : TypeInfo
    {
        public List<Token> Path;

        public QualifiedTypeInfo(List<Token> path)
        {
            Path = path;
        }

        public override string ToString() => string.Join(".", Path);
    }
}
