﻿using System.Collections.Generic;
using Antlr4.Runtime;
using static RailwayPortPy;

namespace ANTLRStudio.Parser
{
    public static class AntlrParser
    {
        public static IEnumerable<(string name, Diagram diagram)> ParseFile(string path)
        {
            ANTLRv4Lexer lexer = new ANTLRv4Lexer(CharStreams.fromPath(path));
            CommonTokenStream stream = new CommonTokenStream(lexer);
            ANTLRv4Parser parser = new ANTLRv4Parser(stream);
            var tree = parser.grammarSpec();
            var visitor = new ListDiagramVisitor();
            return visitor.VisitGrammarSpec(tree);
        }
    }
}
