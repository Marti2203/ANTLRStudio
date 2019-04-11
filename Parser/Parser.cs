using Antlr4.Runtime;
using static RailwayPortPy;

namespace ANTLRStudio.Parser
{
    public static class AntlrParser
    {
        public static Diagram ParseFile(string path)
        {
            ANTLRv4Lexer lexer = new ANTLRv4Lexer(CharStreams.fromPath(path));
            CommonTokenStream stream = new CommonTokenStream(lexer);
            ANTLRv4Parser parser = new ANTLRv4Parser(stream);
            var tree = parser.grammarSpec();
            SingletonDiagramVisitor visitor = new SingletonDiagramVisitor();
            return visitor.VisitGrammarSpec(tree) as Diagram;
        }
    }
}
