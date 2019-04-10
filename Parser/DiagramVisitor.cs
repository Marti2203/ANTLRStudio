using System;
using Antlr4.Runtime.Misc;
using static RailwayPortPy;
using Microsoft.FSharp.Core;
using System.Linq;
namespace ANTLRStudio.Parser
{
    public class DiagramVisitor : ANTLRv4ParserBaseVisitor<DiagramItem>
    {
        protected override DiagramItem DefaultResult
        => new NonTerminal("DEFAULT RESULT", FSharpOption<string>.None, FSharpOption<string>.None);

        private readonly FSharpOption<string> noneString = FSharpOption<string>.None;
        private Comment CreateComment(string v) => new Comment(v, FSharpOption<string>.None, FSharpOption<string>.None);
        public override DiagramItem VisitGrammarSpec([NotNull] ANTLRv4Parser.GrammarSpecContext context)
        {
            var comments = context.DOC_COMMENT()
            .Select(x => CreateComment(x.GetText()))
            .Select(FSharpChoice<DiagramItem, string>.NewChoice1Of2);
            string name = context.grammarDecl().identifier().GetText();
            var rules = context.rules()
            .ruleSpec()
            .Select(VisitRuleSpec)
            .Select(FSharpChoice<DiagramItem, string>.NewChoice1Of2);

            return new Diagram(comments.Concat(rules), FSharpOption<ComplexityType>.None, noneString);
        }
        public override DiagramItem VisitRuleSpec([NotNull] ANTLRv4Parser.RuleSpecContext context)
        {
            var parserRule = context.parserRuleSpec();
            if (parserRule != null) return VisitParserRuleSpec(parserRule);
            return VisitLexerRuleSpec(context.lexerRuleSpec());
        }
        public override DiagramItem VisitParserRuleSpec([NotNull] ANTLRv4Parser.ParserRuleSpecContext context)
        {
            var comments = context.DOC_COMMENT().Select(x => CreateComment(x.GetText())).Select(FSharpChoice<DiagramItem, string>.NewChoice1Of2);
            Console.WriteLine(context.RULE_REF());
            return new Sequence(comments.Concat(new[] { FSharpChoice<DiagramItem, string>
                        .NewChoice1Of2(VisitRuleAltList(context.ruleBlock().ruleAltList())) }));
        }
        public override DiagramItem VisitRuleAltList([NotNull] ANTLRv4Parser.RuleAltListContext context)
        {
            var labeledAlternatives = context.labeledAlt();
            if (labeledAlternatives.Length == 1)
            {
                return VisitLabeledAlt(labeledAlternatives[0]);
            }
            return new Choice(0, labeledAlternatives
                                             .Select(VisitLabeledAlt)
                                             .Select(FSharpChoice<DiagramItem, string>.NewChoice1Of2));
        }
        public override DiagramItem VisitAlternative([NotNull] ANTLRv4Parser.AlternativeContext context)
        => new Sequence(context.element().Select(VisitElement).Select(FSharpChoice<DiagramItem, string>.NewChoice1Of2));
        public override DiagramItem VisitLabeledAlt([NotNull] ANTLRv4Parser.LabeledAltContext context)
        => VisitAlternative(context.alternative());
        public override DiagramItem VisitAltList([NotNull] ANTLRv4Parser.AltListContext context)
        {
            var alternatives = context.alternative();
            if (alternatives.Length == 1)
                return VisitAlternative(alternatives[0]);
            return new Choice(0, alternatives.Select(VisitAlternative).Select(FSharpChoice<DiagramItem, string>.NewChoice1Of2));
        }
        public override DiagramItem VisitElement([NotNull] ANTLRv4Parser.ElementContext context)
        {
            var atom = context.atom();
            if (atom != null)
            {
                return VisitAtom(atom);
            }
            var ebnf = context.ebnf();
            if (ebnf != null)
            {
                DiagramItem path = VisitAltList(ebnf.block().altList());
                switch (ebnf.blockSuffix().ebnfSuffix().GetText())
                {
                    case "?": //TODO Add ?
                    case "+":
                        return new OneOrMore(FSharpChoice<DiagramItem, string>.NewChoice1Of2(path), FSharpOption<FSharpChoice<DiagramItem, string>>.None);
                    case "*":
                        return ZeroOrMore(FSharpChoice<DiagramItem, string>.NewChoice1Of2(path), FSharpOption<FSharpChoice<DiagramItem, string>>.None, true);

                }
            }
            throw new InvalidOperationException("WTF?!");
        }
        public override DiagramItem VisitLexerRuleSpec([NotNull] ANTLRv4Parser.LexerRuleSpecContext context)
        {
            var alternativeList = context.lexerRuleBlock().lexerAltList();
            return new Choice(0, alternativeList.lexerAlt().Select(VisitLexerAlt).Select(FSharpChoice<DiagramItem, string>.NewChoice1Of2));
        }
        public override DiagramItem VisitLexerAlt([NotNull] ANTLRv4Parser.LexerAltContext context)
        {
            var elements = context.lexerElements().lexerElement().Select(VisitLexerElement);
            var commands = context.lexerCommands();
            return new Sequence(elements.Select(FSharpChoice<DiagramItem, string>.NewChoice1Of2));
        }
        public override DiagramItem VisitLexerElement([NotNull] ANTLRv4Parser.LexerElementContext context)
        {
            return new Terminal(context.lexerAtom().GetText(), noneString, noneString);
        }
        public override DiagramItem VisitAtom([NotNull] ANTLRv4Parser.AtomContext context)
        {
            var ruleRef = context.ruleref();
            var terminal = context.terminal();


            return ruleRef == null ? VisitTerminal(terminal) : VisitRuleref(ruleRef);
        }
        public override DiagramItem VisitTerminal([NotNull] ANTLRv4Parser.TerminalContext context)
        => new Terminal(context.GetText(), noneString, noneString);
        public override DiagramItem VisitRuleref([NotNull] ANTLRv4Parser.RulerefContext context)
        => new NonTerminal(context.GetText(), noneString, noneString);
    }
}
