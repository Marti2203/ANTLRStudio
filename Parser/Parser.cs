using System;
using System.IO;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Antlr4.Runtime.Misc;
using static RailwayPortPy;

namespace ANTLRStudio.Parser
{
    public static class Parser
    {
        public static void ParseFile(string path)
        {
            ANTLRv4Lexer lexer = new ANTLRv4Lexer(CharStreams.fromPath(path));
            CommonTokenStream stream = new CommonTokenStream(lexer);
            ANTLRv4Parser parser = new ANTLRv4Parser(stream);
            IParseTree tree = parser.grammarSpec();
            DiagramVisitor visitor = new DiagramVisitor();
            visitor.Visit(tree);
        }
    }
    public class DiagramVisitor : ANTLRv4ParserBaseVisitor<DiagramItem>
    {
        public override DiagramItem VisitGrammarDecl([NotNull] ANTLRv4Parser.GrammarDeclContext context)
        {
            var name = context.identifier().GetText();
            return base.VisitGrammarDecl(context);
        }
        public override DiagramItem VisitEbnfSuffix([NotNull] ANTLRv4Parser.EbnfSuffixContext context)
        {
            return base.VisitEbnfSuffix(context);
        }
        public override DiagramItem VisitAlternative([NotNull] ANTLRv4Parser.AlternativeContext context)
        {
            return base.VisitAlternative(context);
        }
        public override DiagramItem VisitAtom([NotNull] ANTLRv4Parser.AtomContext context)
        {
            return base.VisitAtom(context);
        }
    }
}
