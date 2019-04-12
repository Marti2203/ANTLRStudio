using System;
using Antlr4.Runtime.Misc;
using static RailwayPortPy;
using Microsoft.FSharp.Core;
using System.Linq;
using ANTLRStudio.Parser;
using System.Collections.Generic;
using Item = System.ValueTuple<string, RailwayPortPy.Diagram>;
using ItemSequence = System.Collections.Generic.IEnumerable<(string Name, RailwayPortPy.Diagram Diagram)>;
using Antlr4.Runtime;
namespace Parser
{
    public class ListDiagramVisitor : ANTLRv4ParserBaseVisitor<ItemSequence>
    {
        protected override ItemSequence DefaultResult
        => new Item[] { ("Default", DiagramOf(new NonTerminal("DEFAULT RESULT", noneString, noneString))) };

        private static readonly FSharpOption<string> noneString = FSharpOption<string>.None;

        private static Diagram DiagramOf(DiagramItem item)
        => new Diagram(new[] { FSharpChoice<DiagramItem, string>.NewChoice1Of2(item) }, FSharpOption<ComplexityType>.None, noneString);

        private static ItemSequence SequenceOf(string name, DiagramItem item)
        => new Item[] { (name, DiagramOf(item)) };

        private Comment CreateComment(string v) => new Comment(v, noneString, noneString);

        public override ItemSequence VisitGrammarSpec([NotNull] ANTLRv4Parser.GrammarSpecContext context)
        {
            string VisitGrammarDecl(ANTLRv4Parser.GrammarDeclContext decl)
            {
                string type = decl.grammarType().LEXER() != null ? "Lexer" : decl.grammarType().PARSER() != null ? "Parser" : "Combined";
                string name = decl.identifier().GetText();
                return $"{type} Grammar {name }";
            }
            string grammarName = VisitGrammarDecl(context.grammarDecl());
            var comments = context.DOC_COMMENT().Select(x => x.GetText()).Select(CreateComment).Select(x => ("Grammar Comment", DiagramOf(x)));
            //var prequel = context.prequelConstruct(); NOT YET
            var rules = context.rules().ruleSpec().SelectMany(VisitRuleSpec);
            var modeSpecs = context.modeSpec().SelectMany(VisitModeSpec);

            return comments.Concat(rules).Concat(modeSpecs);
        }


        private static void CheckAdd(ParserRuleContext context, string cssClass, List<DiagramItem> items)
        {
            if (context != null)
            {
                var item = new Terminal(context.GetText(), noneString, noneString);
                item.attrs["class"] = cssClass;
                items.Add(item);
            }
        }
        public override ItemSequence VisitParserRuleSpec([NotNull] ANTLRv4Parser.ParserRuleSpecContext context)
        {
            var displayName = context.ruleModifiers()?.GetText() + context.RULE_REF();
            var comments = context.DOC_COMMENT().Select(x => x.GetText()).Select(CreateComment).Select(x => ($"{context.RULE_REF().GetText()} Comment", DiagramOf(x)));

            List<DiagramItem> items = new List<DiagramItem>();

            CheckAdd(context.argActionBlock(), "actions", items);
            CheckAdd(context.localsSpec(), "locals", items);
            CheckAdd(context.throwsSpec(), "throws", items);
            foreach (var prequel in context.rulePrequel())
                CheckAdd(prequel, "prequels", items);

            var alternatives = context.ruleBlock().ruleAltList().labeledAlt();
            if (alternatives.Length == 1)
                items.Add(new Terminal(alternatives[0].GetText(), noneString, noneString));
            else
                items.Add(HorizontalChoice(alternatives.Select(x => x.GetText()).Select(FSharpChoice<DiagramItem, string>.NewChoice2Of2)));

            if (context.exceptionGroup() != null)
            {
                foreach (var handler in context.exceptionGroup().exceptionHandler())
                    CheckAdd(handler, "handlers", items);
                CheckAdd(context.exceptionGroup().finallyClause(), "finally", items);
            }

            CheckAdd(context.ruleReturns()?.argActionBlock(), "returns", items);


            return comments.Concat(SequenceOf(displayName, new Diagram(items.Select(FSharpChoice<DiagramItem, string>.NewChoice1Of2), FSharpOption<ComplexityType>.None, noneString)))
                           ;
        }

        public override ItemSequence VisitModeSpec([NotNull] ANTLRv4Parser.ModeSpecContext context)
        => SequenceOf($"Start Mode {context.identifier().GetText()}", new Skip())
           .Concat(context.lexerRuleSpec().SelectMany(VisitLexerRuleSpec))
           .Concat(SequenceOf($"End Mode {context.identifier().GetText()}", new Skip()));

        public override ItemSequence VisitRuleSpec([NotNull] ANTLRv4Parser.RuleSpecContext context)
        => context.lexerRuleSpec() == null ? VisitLexerRuleSpec(context.lexerRuleSpec()) : VisitParserRuleSpec(context.parserRuleSpec());

    }
}
