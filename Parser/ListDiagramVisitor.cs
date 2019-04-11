using System;
using Antlr4.Runtime.Misc;
using static RailwayPortPy;
using Microsoft.FSharp.Core;
using System.Linq;
using ANTLRStudio.Parser;
using System.Collections.Generic;
using Item = System.ValueTuple<string, RailwayPortPy.Diagram>;
using ItemSequence = System.Collections.Generic.IEnumerable<(string name, RailwayPortPy.Diagram diagram)>;
namespace Parser
{
    public class ListDiagramVisitor : ANTLRv4ParserBaseVisitor<ItemSequence>
    {
        protected override ItemSequence DefaultResult
        => new Item[] { ("Default", DiagramOf(new NonTerminal("DEFAULT RESULT", noneString, noneString))) };

        private static readonly FSharpOption<string> noneString = FSharpOption<string>.None;
        private static Diagram DiagramOf(DiagramItem item)
        => new Diagram(new[] { FSharpChoice<DiagramItem, string>.NewChoice1Of2(item) }, FSharpOption<ComplexityType>.None, noneString);
        private Comment CreateComment(string v) => new Comment(v, noneString, noneString);
    }
}
