using System;
using Antlr4.Runtime.Misc;
using static RailwayPortPy;
using Microsoft.FSharp.Core;
using System.Linq;
using System.Collections.Generic;
using Item = System.Tuple<string, RailwayPortPy.Diagram>;
using ItemSequence = System.Collections.Generic.IEnumerable<System.Tuple<string, RailwayPortPy.Diagram>>;
using Antlr4.Runtime;
namespace ANTLRStudio.Parser
{
    public class ListDiagramVisitor : ANTLRv4ParserBaseVisitor<ItemSequence>
    {
        private static Item Create(string item, Diagram diagram)
        {
            diagram.attrs.Add("id", item);
            return new Item(item, diagram);
        }
        protected override ItemSequence DefaultResult
        => new Item[] { Create("Default", DiagramOf(new NonTerminal("DEFAULT RESULT", noneString, noneString))) };

        private static readonly FSharpOption<string> noneString = FSharpOption<string>.None;

        private static Diagram DiagramOf(DiagramItem item)
        => new Diagram(new[] { FSharpChoice<DiagramItem, string>.NewChoice1Of2(item) }, FSharpOption<ComplexityType>.None, noneString);

        private static ItemSequence SequenceOf(string name, DiagramItem item)
        => new Item[] { Create(name, DiagramOf(item)) };
        private static ParallelQuery<Item> ParallelSequenceOf(string name, DiagramItem item) => SequenceOf(name, item).AsParallel().AsOrdered();

        private Comment CreateComment(string v) => new Comment(v, noneString, noneString);

        public override ItemSequence VisitGrammarSpec([NotNull] ANTLRv4Parser.GrammarSpecContext context)
        {
            string VisitGrammarDecl(ANTLRv4Parser.GrammarDeclContext decl)
            {
                string type = decl.grammarType().LEXER() != null ? "Lexer" : decl.grammarType().PARSER() != null ? "Parser" : "Combined";
                string name = decl.identifier().GetText();
                return $"{type} Grammar {name}";
            }
            string grammarName = VisitGrammarDecl(context.grammarDecl());
            var comments = context.DOC_COMMENT().AsParallel().AsOrdered().Select(x => x.GetText()).Select(CreateComment).Select(x => Create($"{grammarName} Comment", DiagramOf(x)));
            //var prequel = context.prequelConstruct(); NOT YET
            var rules = context.rules().ruleSpec().AsParallel().AsOrdered().SelectMany(VisitRuleSpec);
            var modeSpecs = context.modeSpec().AsParallel().AsOrdered().SelectMany(VisitModeSpec);

            return comments.Concat(rules).Concat(modeSpecs);
        }


        private static Terminal TerminalWithClass(string text, string cssClass)
        {
            var item = new Terminal(text, noneString, noneString);
            item.attrs["class"] = cssClass;
            return item;
        }
        private static void CheckAdd(ParserRuleContext context, string cssClass, List<DiagramItem> items)
        {
            if (context != null)
            {
                items.Add(TerminalWithClass(context.GetText(), cssClass));
            }
        }
        private static DiagramItem EBNFSuffix(ANTLRv4Parser.EbnfSuffixContext ebnfSuffix, DiagramItem element)
        {
            if (ebnfSuffix == null)
                return element;
            switch (ebnfSuffix.GetText())
            {
                case "?":
                    return Optional(element, false);
                case "*":
                case "*?":
                    return ZeroOrMore(FSharpChoice<DiagramItem, string>.NewChoice1Of2(element),
                                      FSharpOption<FSharpChoice<DiagramItem, string>>.None, true);
                case "+":
                    return new OneOrMore(FSharpChoice<DiagramItem, string>.NewChoice1Of2(element),
                                         FSharpOption<FSharpChoice<DiagramItem, string>>.None);
                default:
                    throw new InvalidOperationException("WTF?!");
            }
        }
        private DiagramItem Transform(ANTLRv4Parser.LabeledAltContext context)
        {
            var identifier = context.identifier()?.GetText() ?? string.Empty;
            var alternative = context.alternative();
            if (string.IsNullOrEmpty(alternative.GetText()))
            {
                return new Terminal($"{(string.IsNullOrEmpty(identifier) ? string.Empty : identifier + " = ")} emptyString", noneString, noneString);
            }
            return new Sequence(alternative.element().AsParallel().AsOrdered().Select(Transform).Select(FSharpChoice<DiagramItem, string>.NewChoice1Of2));
        }
        private DiagramItem Transform(ANTLRv4Parser.ElementContext context)
        {
            if (context.labeledElement() != null)
                return EBNFSuffix(context.ebnfSuffix(), Transform(context.labeledElement()));

            if (context.ebnf() != null)
                return EBNFSuffix(context.ebnf().blockSuffix()?.ebnfSuffix(), Transform(context.ebnf().block()));

            if (context.actionBlock() != null)
                return EBNFSuffix(context.ebnfSuffix(), TerminalWithClass(context.actionBlock().GetText(), "actionBlocks"));


            if (context.atom() != null)
                return EBNFSuffix(context.ebnfSuffix(), Transform(context.atom()));

            throw new NotImplementedException();
        }
        private DiagramItem Transform(ANTLRv4Parser.LabeledElementContext context)
        => new Sequence(new DiagramItem[]
                        {
                        new Terminal($"{context.identifier().GetText()} {context.ASSIGN()?.GetText() ?? context.PLUS_ASSIGN()?.GetText() }", noneString, noneString),
                        context.atom() != null ? Transform(context.atom()) : Transform(context.block())
                        }.Select(FSharpChoice<DiagramItem, string>.NewChoice1Of2));
        private DiagramItem Transform(ANTLRv4Parser.AtomContext context)
        {

            if (context.terminal() != null)
            {
                var text = context.terminal().TOKEN_REF()?.GetText() ?? context.terminal().STRING_LITERAL().GetText();
                var href = context.terminal().TOKEN_REF()?.GetText() == null ? noneString : "#"+context.terminal().TOKEN_REF().GetText();
                return new Terminal(text,href , noneString);
            }
            if (context.ruleref() != null)
            {
                List<Terminal> terminals = new List<Terminal>(2) { new Terminal(context.ruleref().RULE_REF().GetText(), "#"+context.ruleref().RULE_REF().GetText(), noneString) };
                if (context.ruleref().argActionBlock() != null)
                    terminals.Add(TerminalWithClass(context.ruleref().argActionBlock().GetText(), "argActionBlocks"));
                return new Sequence(terminals.Select(FSharpChoice<DiagramItem, string>.NewChoice1Of2));
            }
            if (context.notSet() != null)
                return new Sequence(new DiagramItem[] { TerminalWithClass("NOT", "nots"),
                                    context.notSet().setElement() == null ? Transform(context.notSet().blockSet()) : Transform(context.notSet().setElement()) }
                                    .Select(FSharpChoice<DiagramItem, string>.NewChoice1Of2));
            if (context.DOT() != null)
                return new Terminal("Anything", noneString, noneString);
            throw new NotImplementedException();
        }

        private DiagramItem Transform(ANTLRv4Parser.BlockContext context)
        => new Sequence(context.ruleAction()
                               .AsParallel().AsOrdered()
                               .Select(x => x.GetText())
                               .Select(x => TerminalWithClass(x, "ruleActions"))
                               .Concat(new[] { Transform(context.altList()) })
                               .Select(FSharpChoice<DiagramItem, string>.NewChoice1Of2));


        private DiagramItem Transform(ANTLRv4Parser.AltListContext context)
        => context.alternative().Length == 1
                ? Transform(context.alternative()[0])
                : new Choice(0, context.alternative().AsParallel().AsOrdered().Select(Transform).Select(FSharpChoice<DiagramItem, string>.NewChoice1Of2));

        private DiagramItem Transform(ANTLRv4Parser.AlternativeContext context)
        => context.element().Length == 1
                ? Transform(context.element()[0])
                : new Choice(0, context.element().AsParallel().AsOrdered().Select(Transform).Select(FSharpChoice<DiagramItem, string>.NewChoice1Of2));

        private DiagramItem Transform(ANTLRv4Parser.BlockSetContext context)
        => context.setElement().Length == 1
                ? Transform(context.setElement()[0])
                : new Choice(0, context.setElement().AsParallel().AsOrdered().Select(Transform).Select(FSharpChoice<DiagramItem, string>.NewChoice1Of2));

        //TODO May need to rework this
        private DiagramItem Transform(ANTLRv4Parser.SetElementContext context)
        => new Terminal(context.GetText(), noneString, noneString);
        public override ItemSequence VisitParserRuleSpec([NotNull] ANTLRv4Parser.ParserRuleSpecContext context)
        {
            var displayName = context.ruleModifiers()?.GetText() + context.RULE_REF();
            var comments = context.DOC_COMMENT().AsParallel().AsOrdered().Select(x => x.GetText()).Select(CreateComment).Select(x => Create($"{context.RULE_REF().GetText()} Comment", DiagramOf(x)));

            List<DiagramItem> items = new List<DiagramItem>();

            CheckAdd(context.argActionBlock(), "actions", items);
            CheckAdd(context.localsSpec(), "locals", items);
            CheckAdd(context.throwsSpec(), "throws", items);
            foreach (var prequel in context.rulePrequel())
                CheckAdd(prequel, "prequels", items);

            var alternatives = context.ruleBlock().ruleAltList().labeledAlt();
            if (alternatives.Length == 1)
                items.Add(Transform(alternatives[0]));
            else
                items.Add(new Choice(0, alternatives.AsParallel().AsOrdered().Select(Transform).Select(FSharpChoice<DiagramItem, string>.NewChoice1Of2)));

            if (context.exceptionGroup() != null)
            {
                foreach (var handler in context.exceptionGroup().exceptionHandler())
                    CheckAdd(handler, "handlers", items);
                CheckAdd(context.exceptionGroup().finallyClause(), "finally", items);
            }

            CheckAdd(context.ruleReturns()?.argActionBlock(), "returns", items);


            return comments.Concat(ParallelSequenceOf(displayName, new Sequence(items.Select(FSharpChoice<DiagramItem, string>.NewChoice1Of2))));
        }

        public override ItemSequence VisitModeSpec([NotNull] ANTLRv4Parser.ModeSpecContext context)
        => SequenceOf($"Start Mode {context.identifier().GetText()}", new Skip())
           .Concat(context.lexerRuleSpec().AsParallel().AsOrdered().SelectMany(VisitLexerRuleSpec))
           .Concat(SequenceOf($"End Mode {context.identifier().GetText()}", new Skip()));

        public override ItemSequence VisitLexerRuleSpec([NotNull] ANTLRv4Parser.LexerRuleSpecContext context)
        {
            var displayName = (context.FRAGMENT() != null ? (context.FRAGMENT().GetText() + " ") : string.Empty) + context.TOKEN_REF();
            var comments = context.DOC_COMMENT().AsParallel().AsOrdered().Select(x => x.GetText()).Select(CreateComment).Select(x => Create($"{context.TOKEN_REF().GetText()} Comment", DiagramOf(x)));
            return comments.Concat(ParallelSequenceOf(displayName, Transform(context.lexerRuleBlock().lexerAltList())));
        }

        private DiagramItem Transform(ANTLRv4Parser.LexerAltListContext context)
        => context.lexerAlt().Length == 1
                ? Transform(context.lexerAlt()[0])
                : new Choice(0, context.lexerAlt().AsParallel().AsOrdered().Select(Transform).Select(FSharpChoice<DiagramItem, string>.NewChoice1Of2));

        private DiagramItem Transform(ANTLRv4Parser.LexerAltContext context)
        {
            var lexerElements = context.lexerElements().lexerElement().AsParallel().AsOrdered().Select(Transform).Select(FSharpChoice<DiagramItem, string>.NewChoice1Of2);

            if (context.lexerCommands() != null)
                return new Sequence(lexerElements.Concat(new[] { FSharpChoice<DiagramItem, string>.NewChoice1Of2(TerminalWithClass("->", "rightArrows")) }.AsParallel().AsOrdered())
                                                 .Concat(context.lexerCommands()
                                                                .lexerCommand()
                                                                .AsParallel().AsOrdered()
                                                                .Select(x => TerminalWithClass(x.GetText(), "lexerCommands"))
                                                                             .Select(FSharpChoice<DiagramItem, string>.NewChoice1Of2)));
            return new Sequence(lexerElements);
        }

        private DiagramItem Transform(ANTLRv4Parser.LexerElementContext context)
        {
            if (context.labeledLexerElement() != null)
                return EBNFSuffix(context.ebnfSuffix(), Transform(context.labeledLexerElement()));
            if (context.lexerAtom() != null)
                return EBNFSuffix(context.ebnfSuffix(), Transform(context.lexerAtom()));
            if (context.lexerBlock() != null)
                return EBNFSuffix(context.ebnfSuffix(), Transform(context.lexerBlock()));
            if (context.actionBlock() != null)
                return TerminalWithClass(context.actionBlock().GetText() + (context.QUESTION()?.GetText() ?? string.Empty), "actionBlocks");
            throw new InvalidOperationException("WTF?!");
        }

        private DiagramItem Transform(ANTLRv4Parser.LexerBlockContext context)
        {
            if (context.lexerAltList().lexerAlt().Length == 1)
                return Transform(context.lexerAltList().lexerAlt()[0]);
            return new Choice(0, context.lexerAltList().lexerAlt().AsParallel().AsOrdered().Select(Transform).Select(FSharpChoice<DiagramItem, string>.NewChoice1Of2));
        }

        private DiagramItem Transform(ANTLRv4Parser.LexerAtomContext context)
        {
            if (context.terminal() != null)
            {
                var text = context.terminal().TOKEN_REF()?.GetText() ?? context.terminal().STRING_LITERAL().GetText();
                var href = context.terminal().TOKEN_REF()?.GetText() == null ? noneString : "#"+context.terminal().TOKEN_REF().GetText();
                return new Terminal(text,href , noneString);
            }
            if (context.LEXER_CHAR_SET() != null)
            {
                return new Terminal(context.LEXER_CHAR_SET().GetText(), noneString, noneString);
            }
            if (context.notSet() != null)
                return new Sequence(new DiagramItem[] { TerminalWithClass("NOT", "nots"),
                                    context.notSet().setElement() == null ? Transform(context.notSet().blockSet()) : Transform(context.notSet().setElement()) }
                                    .AsParallel().AsOrdered().Select(FSharpChoice<DiagramItem, string>.NewChoice1Of2));
            if (context.DOT() != null)
                return new Terminal("Anything", noneString, noneString);
            if (context.characterRange() != null)
            {
                var startText = context.characterRange().STRING_LITERAL()[0].GetText().Split('\'')[1];
                char start = startText.Length == 1 ? startText[0] :
                             (char)int.Parse(startText.Substring(2).TrimStart('0'), System.Globalization.NumberStyles.HexNumber);
                var endText = context.characterRange().STRING_LITERAL()[1].GetText().Split('\'')[1];
                char end = endText.Length == 1 ? endText[0] :
                             (char)int.Parse(endText.Substring(2).TrimStart('0'), System.Globalization.NumberStyles.HexNumber);
                if (Math.Abs(start - end) <= 40)
                    return new Choice(0, Enumerable.Range(start, end - start + 1)
                             .Select(x => new string((char)x, 1))
                             .AsParallel().AsOrdered()
                             .Select(x => new Terminal(x, noneString, noneString))
                             .Select(FSharpChoice<DiagramItem, string>.NewChoice1Of2)) as DiagramItem;
                return new NonTerminal($"[{start}=\\u{(int)start} - {end}=\\u{(int)end}]", noneString, noneString);
            }
            throw new InvalidOperationException("WTF?!?!");
        }

        private DiagramItem Transform(ANTLRv4Parser.LabeledLexerElementContext context)
        => new Sequence(new DiagramItem[]
                        {
                        new Terminal($"{context.identifier().GetText()} {context.ASSIGN()?.GetText() ?? context.PLUS_ASSIGN()?.GetText() }", noneString, noneString),
                        context.lexerAtom() != null ? Transform(context.lexerAtom()) : Transform(context.lexerBlock())
                        }.Select(FSharpChoice<DiagramItem, string>.NewChoice1Of2));

        public override ItemSequence VisitRuleSpec([NotNull] ANTLRv4Parser.RuleSpecContext context)
        => context.lexerRuleSpec() != null ? VisitLexerRuleSpec(context.lexerRuleSpec()) : VisitParserRuleSpec(context.parserRuleSpec());

    }
}
