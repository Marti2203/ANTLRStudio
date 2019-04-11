using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using static RailwayPortPy;
using System;
using Antlr4.Runtime.Misc;

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
