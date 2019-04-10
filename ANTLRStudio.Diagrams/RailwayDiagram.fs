module RailwayPortPy
open Eto.Forms
open Eto.Drawing
open System.Text
open System.Text.RegularExpressions
open System.Collections.Generic
open System.IO

let DEBUG = false // if true, writes some debug information into attributes
let VS = 8.f // minimum vertical separation between things. For a 3px stroke, must be at least 4
let AR = 10.f // radius of arcs
let DIAGRAM_CLASS = "railroad-diagram" // class to put on the root <svg>
let STROKE_ODD_PIXEL_LENGTH = true //# is the stroke width an odd (1px, 3px, etc) pixel length?
let INTERNAL_ALIGNMENT = TextAlignment.Center //# how to align items when they have extra space. left/right/center
let CHAR_WIDTH = 8.5f //# width of each monospace character. play until you find the right value for your font
let COMMENT_CHAR_WIDTH = 7.f //# comments are in smaller text by default


let Escape text =
     Regex.Replace(text,"[*_\`\[\]<&]",MatchEvaluator(fun m -> sprintf "&#%i" <| int m.Value.[0]))
     

let determineGaps(outer:float32, inner:float32) =
        let diff = outer - inner
        match INTERNAL_ALIGNMENT with
        | TextAlignment.Left -> (0.f,diff)
        | TextAlignment.Right -> (diff,0.f)
        | TextAlignment.Center -> (diff/2.f,diff/2.f)
        | _ -> failwith "What is this?!"

type DiagramItem(name,?attributes,?text) =
       // up = distance it projects above the entry line
       // height = distance between the entry/exit lines
       // down = distance it projects below the exit line
        let attrsContainer = defaultArg attributes <| Dictionary()
        let childContainer = match text with
                             | Some text -> ResizeArray([text])
                             | None -> ResizeArray()
        member val up = 0.f with get,set
        member val down = 0.f with get,set
        member val width = 0.f with get,set
        member val height = 0.f with get,set
        member val attrs = attrsContainer with get
        member val children = childContainer with get
        member val needsSpace = false with get,set

        abstract format : float32 * float32 * float32 -> DiagramItem
        default self.format (x,y,width) = self
        member self.addTo(parent:#DiagramItem) =
            parent.children.Add(Choice1Of2 self)
            self

        override self.ToString() = sprintf "DiagramItem %f %f %f %f" self.up self.down self.width self.height
        abstract writeSvg : TextWriter -> unit
        default self.writeSvg (writer:TextWriter) =
                writer.Write(sprintf "<%s" name)
                self.attrs
                |> Seq.sortBy (fun x -> x.Key)
                |> Seq.iter (fun x -> writer.Write(sprintf" %s=\"%s\"" x.Key <| Escape x.Value))
                writer.Write(">")
                if Seq.contains name <| ["g"; "svg"] then
                    writer.Write("\n")
                self.children
                |> Seq.iter(fun child -> match child with
                                         | Choice1Of2 c -> c.writeSvg(writer)
                                         | Choice2Of2 x -> writer.Write(Escape x))
                writer.Write(sprintf "</%s>" name)
and DiString = Choice<DiagramItem,string>

let addDebug(el:DiagramItem)=
    if DEBUG then
        el.attrs.["data-x"] <- (sprintf "%s w:%f h:%f/%f/%f" (el.GetType().Name) el.width el.up el.height el.down)

let diDict vs = Dictionary(dict vs)
let dictFromPair (k,v) = diDict [k,v]


type Path(x, y)=
    inherit DiagramItem("path", dictFromPair("d",sprintf "M%f %f" x y))

    member self.m(x, y) =
        self.attrs.["d"] <- self.attrs.["d"] + sprintf "m%f %f" x y
        self

    member self.l(x, y) =
        self.attrs.["d"] <- self.attrs.["d"] + sprintf "l%f %f" x y
        self

    member self.h(value) =
        self.attrs.["d"] <- self.attrs.["d"] + sprintf "h%f" value
        self

    member self.right(value) = self.h(max 0.f value)

    member self.left(value) = self.h(- max 0.f  value)

    member self.v(value) =
        self.attrs.["d"] <- self.attrs.["d"] +  sprintf "v%f" value
        self

    member self.down(value) = self.v( max 0.f value)

    member self.up(value) = self.v(- max 0.f value)

    member self.arc_8(start, dir)=
        // 1/8 of a circle
        let s2 = (1.f / sqrt(2.f)) * AR
        let s2inv = AR - s2
        let path = sprintf "a %f %f 0 0 %i %f %f" AR AR 
                    <|  if dir = "cw" then 1 else 0
                    <|| match start + dir with
                        | "swccw"  
                        | "ncw"   -> (s2, s2inv) 
                        | "wccw"   
                        | "necw"  -> (s2inv, s2) 
                        | "nwccw"  
                        | "ecw"   -> (-s2inv, s2) 
                        | "nccw"  
                        | "secw"  -> (-s2, s2inv) 
                        | "neccw"  
                        | "scw"   -> (-s2, -s2inv) 
                        | "eccw"   
                        | "swcw"  -> (-s2inv, -s2) 
                        | "seccw"  
                        | "wcw"   -> (s2inv, -s2) 
                        | "nwcw"  
                        | "sccw"  -> (s2, -s2inv) 
                        | _ -> failwith "ERROR??"

        self.attrs.["d"] <- self.attrs.["d"] + path
        self

    member self.arc(sweep:string)=
        let x = AR * if sweep.[0] = 'e' || sweep.[1] = 'w' then -1.f else 1.f
        let y = AR * if sweep.[0] = 's' || sweep.[1] = 'n' then -1.f else 1.f
        let cw = if sweep = "ne" || sweep = "es" || sweep = "sw" || sweep = "wn" then 1 else 0
        self.attrs.["d"] <- self.attrs.["d"] + sprintf "a%f %f 0 0 %i %f %f" AR AR cw x y
        self

    override self.format (x,y,width)=
        self.attrs.["d"] <- self.attrs.["d"] + "h.5"
        self :> DiagramItem

    override self.ToString() = sprintf "Path(%f %f)" x y
    

let DEFAULT_STYLE = """
    svg.railroad-diagram {
        background-color:hsl(30,20%,95%);
    }
    svg.railroad-diagram path {
        stroke-width:3;
        stroke:black;
        fill:rgba(0,0,0,0);
    }
    svg.railroad-diagram text {
        font:bold 14px monospace;
        text-anchor:middle;
    }
    svg.railroad-diagram text.label{
        text-anchor:start;
    }
    svg.railroad-diagram text.comment{
        font:italic 12px monospace;
    }
    svg.railroad-diagram rect{
        stroke-width:3;
        stroke:black;
        fill:hsl(120,100%,90%);
    }
"""

type Style(css:string) =
    inherit DiagramItem("style")  
    override self.ToString() = sprintf "Style(%s)" css
    override self.writeSvg(writer:TextWriter) =
        //Write included stylesheet as CDATA. See https:#developer.mozilla.org/en-US/docs/Web/SVG/Element/style
        let cdata = sprintf "/* <![CDATA[ */\n{%s}\n/* ]]> */\n" css
        writer.Write(sprintf "<style>%s</style>" css)

type Terminal(text:string,?href:string,?title:string) as self=
    inherit DiagramItem( "g", dictFromPair("class","terminal"))
    do
        self.width <- float32 text.Length * CHAR_WIDTH + 20.f
        self.up <- 11.f
        self.down <- 11.f
        self.needsSpace <- true
        addDebug(self)

    override self.ToString()= sprintf "Terminal(%s, href=%A, title=%A)" text href title

    override self.format (x,y,width)=
        let leftGap, rightGap = determineGaps(width, self.width)

        //# Hook up the two sides if self is narrower than its stated width.
        Path(x, y).h(leftGap).addTo(self) |> ignore
        Path(x + leftGap + self.width, y).h(rightGap).addTo(self) |> ignore

        DiagramItem("rect", diDict ["x", (x + leftGap).ToString()
                                    "y", (y - 11.f).ToString()
                                    "width", self.width.ToString()
                                    "height", (self.up + self.down).ToString()
                                    "rx","10"
                                    "ry","10"]).addTo(self) |> ignore
        let textItem = DiagramItem("text", diDict ["x",(x + width / 2.f).ToString()
                                                   "y",(y + 4.f).ToString()], Choice2Of2 text)

        textItem.addTo(match href with
                       | Some(href) -> DiagramItem("a", dictFromPair("xlink:href", href), Choice2Of2 text).addTo(self) 
                       | None -> self :> DiagramItem) |> ignore
        match title with
        | Some(title) -> DiagramItem("title", text = Choice2Of2(title)).addTo(self) |> ignore
        | None -> ()
        self :> DiagramItem


let wrap item = match item with
                | Choice1Of2(diagram) -> diagram
                | Choice2Of2(str) -> Terminal(str) :> DiagramItem

type ComplexityType = Simple | Complex
type Start(?startType:ComplexityType,?label:string) as self=
    inherit DiagramItem( "g")
    do
        self.width <- match label with
                      | Some(label) -> max 20.f (float32 label.Length * CHAR_WIDTH + 10.f)
                      | None  -> 20.f

        self.up <- 10.f
        self.down <- 10.f
        addDebug(self)

    override self.format (x,y,width)=
        let path = Path(x, y - 10.f)
        (match (defaultArg startType Simple) with
        | Complex -> path.down(20.f).m(0.f, -10.f).right(self.width)
        | Simple -> path.down(20.f).m(10.f, -20.f).down(20.f).m(-10.f, -10.f).right(self.width)
        ).addTo(self) |> ignore
        match label with
        | Some(label) -> DiagramItem("text", diDict ["x",x.ToString()
                                                     "y",(y - 15.f).ToString()
                                                     "style","text-anchor:start"], Choice2Of2 label).addTo(self) 
                                                     |> ignore
        | None -> ()

        self :> DiagramItem

    override self.ToString() = sprintf "Start(type=%A, label=%A)" startType label

type End(?endType:ComplexityType) as self=
    inherit DiagramItem("path")
    let endType= defaultArg endType Simple
    do    
        self.width <- 20.f
        self.up <- 10.f
        self.down <- 10.f
        addDebug(self)

    override self.format (x,y,width)=
        match endType with 
        | Simple -> self.attrs.["d"] <- sprintf "M %f %f h 20 m -10 -10 v 20 m 10 -20 v 20" x y
        | Complex -> self.attrs.["d"] <- sprintf "M %f %f h 20 m 0 -10 v 20" x y
        self :> DiagramItem

    override self.ToString()= sprintf "End(type=%A)" endType

type Skip() as self=
    inherit DiagramItem("g")
    do
        addDebug(self)

    override self.format (x,y,width) =
        Path(x, y).right(width).addTo(self) |> ignore
        self :> DiagramItem

    override self.ToString() = "Skip()"
type Diagram (items: DiString seq, ?diagramType:ComplexityType, ?css:string) as self =
    inherit DiagramItem("svg", dictFromPair ("class",DIAGRAM_CLASS))
    let formattedContainer = ref false
    let itemContainer = ResizeArray(items |> Seq.map wrap)
    do 
        if not <| (self.items.[0] :? Start) then
            self.items.Insert(0, Start(self.diagramType))
        if not <| (Seq.last self.items :? End) then
            self.items.Add(End(self.diagramType))
        self.items.Insert(0, Style(self.css))
        self.items
        |> Seq.iter(fun item ->
            if item :? Style then ()
            else
                self.width <- self.width + item.width + (if item.needsSpace then 20.f else 0.f)
                self.up <- max self.up (item.up - self.height)
                self.height <- self.height + item.height
                self.down <- max (self.down - item.height) item.down
        )
        if self.items.[0].needsSpace then
            self.width <- self.width - 10.f
        if (self.items |> Seq.last).needsSpace then
            self.width <- self.width - 10.f
    member self.formatted = formattedContainer
    member self.diagramType : ComplexityType = defaultArg diagramType Simple
    member self.items :ResizeArray<DiagramItem> = itemContainer
    member self.css = defaultArg css DEFAULT_STYLE

    override self.ToString() =
        let items =  String.concat ", " <| (self.items |> Seq.skip(2) |> Seq.map(fun x-> x.ToString()))
        let pieces = ResizeArray([items])
        if self.css <> DEFAULT_STYLE then
            pieces.Add(sprintf "css=%s" self.css)
        if self.diagramType <> Simple then
            pieces.Add(sprintf "type=%A" self.diagramType)
        sprintf "Diagram(%s)" <| String.concat ", " pieces

    override self.format (x,y,width) = self.formatBig <| Some(x) <| Some(y) <| Some(width) <| None
    member self.formatBig (paddingTop:float32 option) (paddingRight:float32 option) (paddingBottom:float32 option) (paddingLeft:float32 option) =
        let paddingTop = defaultArg paddingTop 20.f
        let paddingRight = defaultArg paddingRight paddingTop
        let paddingBottom = defaultArg paddingBottom paddingTop
        let paddingLeft = defaultArg paddingLeft paddingRight
        let mutable x = paddingLeft
        let mutable y = paddingTop + self.up
        let g = DiagramItem("g")
        if STROKE_ODD_PIXEL_LENGTH then
            g.attrs.["transform"] <- "translate(.5 .5)"
        self.items
        |> Seq.iter(fun item ->
            if item.needsSpace then
                Path(x, y).h(10.f).addTo(g) |> ignore
                x <- x + 10.f
            item.format(x,y,item.width).addTo(g) |> ignore
            x <- x + item.width
            y <- y + item.height
            if item.needsSpace then
                Path(x, y).h(10.f).addTo(g) |> ignore
                x <- x + 10.f
        )
        self.attrs.["width"] <- (self.width + paddingLeft + paddingRight).ToString()
        self.attrs.["height"] <- (self.up + self.height + self.down + paddingTop + paddingBottom).ToString()
        self.attrs.["viewBox"] <- sprintf "0 0 %s %s" self.attrs.["width"] self.attrs.["height"]
        g.addTo(self) |> ignore
        self.formatted := true
        self :> DiagramItem

    override self.writeSvg(writer)=
        if not !self.formatted then
            self.formatBig None None None None |> ignore
        base.writeSvg(writer)

    //let parseCSSGrammar(text) =
        //let token_patterns = {
        //    "keyword"= r"[\w-]+\(?",
        //    "type"= r"<[\w-]+(\(\))?>",
        //    "char"= r"[/,()]",
        //    "literal"= r""(.)"",
        //    "openbracket"= r"\[",
        //    "closebracket"= r"\]",
        //    "closebracketbang"= r"\]!",
        //    "bar"= r"\|",
        //    "doublebar"= r"\|\|",
        //    "doubleand"= r"&&",
        //    "multstar"= r"\*",
        //    "multplus"= r"\+",
        //    "multhash"= r"#",
        //    "multnum1"= r"{\s*(\d+)\s*}",
        //    "multnum2"= r"{\s*(\d+)\s*,\s*(\d*)\s*}",
        //    "multhashnum1"= r"#{\s*(\d+)\s*}",
        //    "multhashnum2"= r"{\s*(\d+)\s*,\s*(\d*)\s*}"
        //}


type Sequence(items:DiString seq) as self =
    inherit DiagramItem("g")
    let itemContainer = ResizeArray(items |> Seq.map wrap)
    do  
        self.needsSpace <- true
        self.items 
        |> Seq.iter(fun item -> 
            self.width <- self.width + item.width + (if item.needsSpace then 20.f else 0.f)
            self.up <- max self.up (item.up - self.height)
            self.height <- self.height + item.height
            self.down <- max (self.down - item.height) item.down
        )
        if self.items.[0].needsSpace then
            self.width <- self.width - 10.f
        if (self.items |> Seq.last).needsSpace then
            self.width <- self.width - 10.f
        addDebug(self)

    member self.items :ResizeArray<DiagramItem> = itemContainer
    override self.ToString() =
        let items = String.concat ", " <| (self.items |> Seq.map(fun x -> x.ToString()))
        sprintf "Sequence(%s)" items

    override self.format (x,y,width)=
        let leftGap, rightGap = determineGaps(width,self.width)
        Path(x, y).h(leftGap).addTo(self) |> ignore
        Path(x+ leftGap + self.width, y+ self.height).h(rightGap).addTo(self) |> ignore
        let mutable x = x + leftGap
        let mutable y = y
        let last = self.items.Count - 1
        self.items
        |> Seq.iteri(fun i item -> 
            if item.needsSpace && i > 0 then
                Path(x, y).h(10.f).addTo(self) |> ignore
                x <- x + 10.f
            item.format(x,y,item.width).addTo(self) |> ignore
            x <- x + item.width
            y <- y + item.height
            if item.needsSpace && i < last then
                Path(x, y).h(10.f).addTo(self) |> ignore
                x <- x + 10.f
        )
        self :> DiagramItem


type Stack(items: DiString seq) as self=
    inherit DiagramItem("g")
    let itemContainer = ResizeArray(items |> Seq.map wrap)
    do
        self.needsSpace <- true
        self.width <- self.items
                      |> Seq.map (fun item  -> if item.needsSpace then 20.f else 0.f + item.width)
                      |> Seq.max
        if self.items.Count > 1 then
            self.width <- self.width + AR * 2.f
        self.up <- self.items.[0].up
        self.down <- (self.items |> Seq.last).down
        let last = self.items.Count - 1
        self.items
        |> Seq.iteri(fun i item->
        self.height <- self.height + item.height
        if i > 0 then
            self.height <- self.height + max (AR * 2.f) (item.up + VS)
        if i < last then
            self.height <- self.height + max (AR * 2.f) (item.down + VS)
        )
        addDebug(self)
    member self.items :ResizeArray<DiagramItem> = itemContainer

    override self.ToString() = 
        let items = self.items 
                    |> Seq.map(fun x -> x.ToString())
                    |> String.concat ", "
        sprintf "Stack(%s)" items

    override self.format(x,y,width) =
        let leftGap, rightGap = determineGaps(width, self.width)
        Path(x, y).h(leftGap).addTo(self) |> ignore
        let mutable x = x + leftGap
        let mutable y = y
        let xInitial = x
        let innerWidth = 
            if  self.items.Count > 1 then
                Path(x, y).h(AR).addTo(self) |> ignore
                x <- x + AR
                self.width - AR * 2.f
            else self.width
        self.items
        |> Seq.iteri(fun i item -> 
            item.format(x,y,innerWidth).addTo(self) |> ignore
            x <- x + innerWidth
            y <- y + item.height
            if i <> self.items.Count - 1 then
                Path(x,y).arc("ne").down(max 0.f (item.down + VS - AR*2.f))
                          .arc("es").left(innerWidth)
                          .arc("nw").down(max 0.f (self.items.[i+1].up + VS - AR*2.f))
                          .arc("ws").addTo(self) |> ignore
                y <- y + (max (item.down + VS) AR*2.f) + (max (self.items.[i+1].up + VS) AR*2.f)
                x <- xInitial + AR
            if self.items.Count > 1 then
               Path(x, y).h(AR).addTo(self) |> ignore
               x <- x + AR
        )
        Path(x, y).h(rightGap).addTo(self) |> ignore
        self :> DiagramItem


type OptionalSequenceImpl(items:DiString seq) as self=
    inherit DiagramItem("g")
    let itemContainer = ResizeArray(items |> Seq.map wrap)
    do 
        self.height <- self.items |> Seq.sumBy(fun (item : DiagramItem) -> item.height) 
        self.down <- self.items.[0].down
        let mutable heightSoFar = 0.f
        self.items
        |> Seq.iteri( fun i item -> 
            self.up <- max self.up ((max (AR * 2.f) (item.up + VS)) - heightSoFar)
            heightSoFar <- heightSoFar + item.height
            if i > 0 then
                self.down <- (max (self.height + self.down) (heightSoFar + (max (AR * 2.f) (item.down + VS)))) - self.height
            let itemWidth = item.width + (if item.needsSpace then 20.f else 0.f)
            if i = 0 then
                self.width <- self.width +  (AR + max itemWidth AR)
            else
                self.width <- self.width +  (AR * 3.f + max itemWidth AR)
                )
        addDebug(self)

    member self.items :ResizeArray<DiagramItem> = itemContainer

    override self.ToString() =
        let items = self.items |> Seq.map(fun x -> x.ToString()) |> String.concat ", "
        sprintf "OptionalSequence(%s)" items

    override self.format(x,y,width) =
        let leftGap, rightGap = determineGaps(width, self.width)
        Path(x, y).right(leftGap).addTo(self) |> ignore
        Path(x + leftGap + self.width, y + self.height).right(rightGap).addTo(self) |> ignore
        let mutable x = x + leftGap
        let mutable y = y
        let upperLineY = y - self.up
        let last = self.items.Count - 1
        self.items
        |> Seq.iteri(fun i item -> 
            let itemSpace = if item.needsSpace then 10.f else 0.f
            let itemWidth = item.width + itemSpace
            if i = 0 then
                (Path(x,y)
                    .arc("se")
                    .up(y - upperLineY - AR*2.f)
                    .arc("wn")
                    .right(itemWidth - AR)
                    .arc("ne")
                    .down(y + item.height - upperLineY - AR*2.f)
                    .arc("ws")
                    .addTo(self)) |> ignore
                (Path(x, y)
                    .right(itemSpace + AR)
                    .addTo(self)) |> ignore
                item.format(x + itemSpace + AR,y,item.width)
                    .addTo(self) |> ignore
                x <- x + itemWidth + AR
                y <- y + item.height
            elif i < last then
                (Path(x, upperLineY)
                    .right(AR*3.f + (max itemWidth AR))
                    .arc("ne")
                    .down(y - upperLineY + item.height - AR*2.f)
                    .arc("ws")
                    .addTo(self)) |> ignore
                (Path(x,y)
                    .right(AR * 2.f)
                    .addTo(self)) |> ignore
                item.format(x + AR * 2.f,y,item.width)
                    .addTo(self) |> ignore
                (Path(x + item.width + AR*2.f, y + item.height)
                    .right(itemSpace + AR)
                    .addTo(self)) |> ignore
                (Path(x,y)
                    .arc("ne")
                    .down(item.height + (max (item.down + VS) (AR * 2.f)) - AR*2.f)
                    .arc("ws")
                    .right(itemWidth - AR)
                    .arc("se")
                    .up(item.down + VS - AR*2.f)
                    .arc("wn")
                    .addTo(self)) |> ignore
                x <- x + AR*2.f + (max itemWidth AR) + AR
                y <- y + item.height
            else
                (Path(x, y)
                    .right(AR*2.f)
                    .addTo(self)) |> ignore
                item.format(x + AR * 2.f,y,item.width)
                    .addTo(self) |> ignore
                (Path(x + AR*2.f + item.width, y + item.height)
                    .right(itemSpace + AR)
                    .addTo(self)) |> ignore
                (Path(x,y)
                    .arc("ne")
                    .down(item.height + (max (item.down + VS) (AR*2.f)) - AR*2.f)
                    .arc("ws")
                    .right(itemWidth - AR)
                    .arc("se")
                    .up(item.down + VS - AR*2.f)
                    .arc("wn")
                    .addTo(self))|> ignore) 
        self :> DiagramItem
let OptionalSequence(items:DiString seq) =
    if items |> Seq.length <= 1
        then Sequence(items) :> DiagramItem
    else OptionalSequenceImpl(items) :> DiagramItem
    
type AlternatingSequence(first:DiString,second:DiString) as self=
    inherit DiagramItem("g")
    let first = wrap first
    let second = wrap second
    do 
       let arc = AR
       let vert = VS
       let arcX = (1.f / sqrt(2.f)) * arc * 2.f
       let arcY = (1.f - 1.f / sqrt(2.f)) * arc * 2.f
       let crossY = max arc vert
       let crossX = (crossY - arcY) + arcX
       let firstOut = [2.f * arc;crossY / 2.f + arc * 2.f;crossY / 2.f + vert + first.down] |> Seq.max
       self.up <- firstOut + first.height + first.up
       let secondIn = [2.f * arc; crossY / 2.f + 2.f * arc;crossY / 2.f + vert + second.up] |> Seq.max
       self.down <- secondIn + second.height + second.down
       let firstWidth = (if first.needsSpace then 20.f else 0.f) + first.width
       let secondWidth = (if second.needsSpace then 20.f else 0.f) + second.width
       self.width <-  4.f * arc  + Seq.max [firstWidth;crossX;secondWidth]
       addDebug(self)

    override self.ToString() = sprintf "AlternatingSequence(%s %s)" <| first.ToString() <| second.ToString()

    override self.format(x,y,width) =
        let arc = AR
        let gaps = determineGaps(width, self.width)
        Path(x,y).right(fst gaps).addTo(self) |> ignore
        let mutable x = x + fst gaps
        Path(x + self.width, y).right(snd gaps).addTo(self) |> ignore
        //# bounding box
        //# Path(x+gaps[0], y).up(self.up).right(self.width).down(self.up+self.down).left(self.width).up(self.down).addTo(self)

        //# top
        let firstIn = self.up - first.up
        let firstOut = self.up - first.up - first.height
        Path(x,y).arc("se").up(firstIn - 2.f*arc).arc("wn").addTo(self) |> ignore
        first.format(x + 2.f * arc,y - firstIn,self.width - 4.f*arc)
            .addTo(self) |> ignore
        Path(x + self.width - 2.f*arc, y - firstOut).arc("ne").down(firstOut - 2.f * arc).arc("ws").addTo(self) |> ignore

        //# bottom
        let secondIn = self.down - second.down - second.height
        let secondOut = self.down - second.down
        Path(x,y).arc("ne").down(secondIn - 2.f *arc).arc("ws").addTo(self) |> ignore
        second.format(x + 2.f * arc,y + secondIn,self.width - 4.f * arc).addTo(self) |> ignore
        Path(x + self.width - 2.f *arc, y + secondOut).arc("se").up(secondOut - 2.f *arc).arc("wn").addTo(self) |> ignore

        //# crossover
        let arcX = 1.f / sqrt(2.f) * arc * 2.f
        let arcY = (1.f - 1.f / sqrt(2.f) ) * arc * 2.f
        let crossY = max arc VS
        let crossX = (crossY - arcY) + arcX
        let crossBar = (self.width - 4.f * arc - crossX) / 2.f
        (Path(x+arc, y - crossY/ 2.f - arc).arc("ws").right(crossBar)
            .arc_8("n", "cw").l(crossX - arcX, crossY - arcY).arc_8("sw", "ccw")
            .right(crossBar).arc("ne").addTo(self)) |> ignore
        (Path(x+arc, y + crossY/2.f + arc).arc("wn").right(crossBar)
            .arc_8("s", "ccw").l(crossX - arcX, arcY - crossY ).arc_8("nw", "cw")
            .right(crossBar).arc("se").addTo(self)) |> ignore
        self :> DiagramItem


type Choice(defaultChoice:int,items:DiString seq) as self=
    inherit DiagramItem("g")
    let itemContainer = ResizeArray(items |> Seq.map wrap)
    do    
        self.width <- AR * 4.f + (self.items |> Seq.map (fun item -> item.width) |> Seq.max)
        self.up <- self.items.[0].up
        self.down <- (self.items |> Seq.last).down
        self.height <- self.items.[defaultChoice].height
        self.items |> Seq.iteri(fun i item ->
            let arcs = (if ([defaultChoice-1;defaultChoice+1] |> Seq.contains i) then
                            AR * 2.f
                       else 
                            AR)

            if i < defaultChoice then
                self.up <- self.up + max arcs (item.height + item.down + VS + self.items.[i+1].up)
            elif i = defaultChoice then ()
            else
                self.down <- self.down + max arcs (item.up + VS + self.items.[i-1].down + self.items.[i-1].height)
            
        )
        self.down <- self.down - self.items.[defaultChoice].height //# already counted in self.height
        addDebug(self)
    member self.items : ResizeArray<DiagramItem> = itemContainer

    override self.ToString() = 
        let items = self.items |> Seq.map( fun x -> x.ToString()) |> String.concat ", "
        sprintf "Choice(%i, %s)" defaultChoice items
    override self.format(x,y,width) =
        let leftGap, rightGap = determineGaps(width, self.width)

        //# Hook up the two sides if self is narrower than its stated width.
        Path(x, y).h(leftGap).addTo(self) |> ignore
        Path(x + leftGap + self.width, y + self.height).h(rightGap).addTo(self) |> ignore
        let mutable x = x + leftGap

        let innerWidth = self.width - AR * 4.f
        let defaultItem = self.items.[defaultChoice]
        
        //# Do the elements that curve above
        let above = self.items |> Seq.take defaultChoice |> Seq.rev
        if above |> Seq.isEmpty |> not then
            let firstAbove = above |> Seq.item 0
            let mutable distanceFromY = max (AR * 2.f) (defaultItem.up + VS + firstAbove.down + firstAbove.height)
            let length = Seq.length above
            above |> Seq.iteri(fun i item -> 
                let ni = i - length
                Path(x, y).arc("se").up(distanceFromY - AR * 2.f).arc("wn").addTo(self) |> ignore
                item.format(x + AR * 2.f,y - distanceFromY,innerWidth).addTo(self) |> ignore
                Path(x + AR * 2.f + innerWidth, y - distanceFromY + item.height).arc("ne") 
                    .down(distanceFromY - item.height + defaultItem.height - AR*2.f).arc("ws").addTo(self) |> ignore
                if ni < -1 then /// TODO CHECK HERE
                    let nextAbove = above |> Seq.item (i+1)
                    distanceFromY <- distanceFromY + max AR (item.up + VS + nextAbove.down + nextAbove.height)
            )
        //# Do the straight-line path.
        Path(x, y).right(AR * 2.f).addTo(self) |> ignore
        self.items.[defaultChoice].format(x + AR * 2.f,y,innerWidth).addTo(self) |> ignore
        Path(x + AR * 2.f + innerWidth, y+ self.height).right(AR * 2.f).addTo(self) |> ignore

        //# Do the elements that curve below
        let below = self.items |> Seq.skip (defaultChoice + 1)
        if below |> Seq.isEmpty |> not then
            let mutable distanceFromY = max (AR * 2.f) (defaultItem.height + defaultItem.down + VS + (below |> Seq.item 0).up)
            below |> Seq.iteri(fun i item ->
            Path(x, y).arc("ne").down(distanceFromY - AR * 2.f).arc("ws").addTo(self) |> ignore
            item.format(x + AR * 2.f,y + distanceFromY,innerWidth).addTo(self) |> ignore
            Path(x + AR * 2.f + innerWidth, y + distanceFromY + item.height).arc("se")
                .up(distanceFromY - AR * 2.f + item.height - defaultItem.height).arc("wn").addTo(self) |> ignore
            distanceFromY <- distanceFromY + max AR 
                    (item.height + item.down + VS + ( if (i+1 < Seq.length below) then (below |> Seq.item (i + 1)).up else 0.f))
            )
        self :> DiagramItem

type ChoiceType = Any | All
type MultipleChoice(defaultChoice:int,choiceType:ChoiceType,items: DiString seq) as self=
    inherit DiagramItem( "g")
    let itemContainer = ResizeArray(items |> Seq.map wrap)
    let innerWidthValue = itemContainer |> Seq.map (fun item -> item.width) |> Seq.max
    let defaultChoice = max defaultChoice 0
    do
        self.needsSpace <- true
        self.width <- 30.f + AR + self.innerWidth + AR + 20.f
        self.up <- self.items.[0].up
        self.down <- (self.items |> Seq.last).down
        self.height <- self.items.[defaultChoice].height
        self.items
        |> Seq.iteri(fun i item -> 
        let minimum = 
            if [defaultChoice - 1; defaultChoice + 1] |> (Seq.contains i) then
                10.f + AR
            else
                AR
        if i < defaultChoice then
            self.up <- self.up + max minimum (item.height + item.down + VS + self.items.[i+1].up)
        elif i = defaultChoice then
            ()
        else
            self.down <- self.down + max minimum (item.up + VS + self.items.[i-1].down + self.items.[i-1].height)
        )
        self.down <- self.down - self.items.[defaultChoice].height //# already counted in self.height
        addDebug(self)
    member self.innerWidth = innerWidthValue
    member self.items : ResizeArray<DiagramItem> = itemContainer
    override self.ToString()=
        let items = self.items |> Seq.map(fun x -> x.ToString()) |> String.concat ", "
        sprintf "MultipleChoice(%i, %A, %s)" defaultChoice choiceType items

    override self.format(x,y,width) =
        let leftGap, rightGap = determineGaps(width, self.width)

        //# Hook up the two sides if self is narrower than its stated width.
        Path(x, y).h(leftGap).addTo(self) |> ignore
        Path(x + leftGap + self.width, y + self.height).h(rightGap).addTo(self) |> ignore
        let mutable x = x + leftGap

        let defaultItem = self.items.[defaultChoice]

        //# Do the elements that curve above
        let above = self.items |> Seq.skip defaultChoice |> Seq.rev
        if above |> Seq.isEmpty |> not then
            let length = above |> Seq.length
            let aboveFirst = above |> Seq.item 0
            let mutable distanceFromY = max (10.f + AR) (defaultItem.up + VS + aboveFirst.down + aboveFirst.height)
            above |> Seq.iteri(fun i item -> 
            let ni = length - i
            (Path(x + 30.f, y)
                .up(distanceFromY - AR)
                .arc("wn")
                .addTo(self)) |> ignore
            item.format(x + 30.f + AR,y - distanceFromY,self.innerWidth).addTo(self) |> ignore
            (Path(x + 30.f + AR + self.innerWidth, y - distanceFromY + item.height)
                .arc("ne")
                .down(distanceFromY - item.height + defaultItem.height - AR - 10.f)
                .addTo(self)) |> ignore
            if ni < -1 then
                distanceFromY <- distanceFromY + max AR (item.up + VS + (above |> Seq.item (i+1)).down + (above |> Seq.item (i+1)).height)
             )
        //# Do the straight-line path.
        Path(x + 30.f, y).right(AR).addTo(self) |> ignore
        self.items.[defaultChoice].format(x + 30.f + AR,y,self.innerWidth).addTo(self) |> ignore
        Path(x + 30.f + AR + self.innerWidth, y + self.height).right(AR).addTo(self) |> ignore

        //# Do the elements that curve below
        let below = self.items |> Seq.skip defaultChoice  /// TASK DEBUG
        if below |> Seq.isEmpty |> not then
            let length = below |> Seq.length
            let mutable distanceFromY = max (10.f + AR) (defaultItem.height + defaultItem.down + VS + (below|> Seq.item 0).up)
            below |> Seq.iteri(fun i item -> 
            (Path(x+30.f, y)
                .down(distanceFromY - AR)
                .arc("ws")
                .addTo(self)) |> ignore
            item.format(x + 30.f + AR,y + distanceFromY,self.innerWidth).addTo(self) |> ignore
            (Path(x + 30.f + AR + self.innerWidth, y + distanceFromY + item.height)
                .arc("se")
                .up(distanceFromY - AR + item.height - defaultItem.height - 10.f)
                .addTo(self)) |> ignore
            distanceFromY <- distanceFromY + max AR (item.height + item.down + VS + (if i+1 < length then (below |> Seq.item(i + 1)).up else 0.f))
        )
        let text = DiagramItem("g", dictFromPair("class","diagram-text")).addTo(self) 
        DiagramItem("title", text= Choice2Of2(if choiceType = Any then "take one or more branches, once each, in any order" else "take all branches, once each, in any order"))
            .addTo(text) |> ignore
        DiagramItem("path", diDict ["d",sprintf "M %f %f h -26 a 4 4 0 0 0 -4 4 v 12 a 4 4 0 0 0 4 4 h 26 z" (x+30.f) (y-10.f)
                                    "class","diagram-text"
                                   ]).addTo(text) |> ignore
        DiagramItem("text", diDict ["x",(x + 15.f).ToString()
                                    "y",(y + 4.f).ToString()
                                    "class","diagram-text"]
                          , Choice2Of2 <| match choiceType with
                                          | Any -> "1+"
                                          | All -> "all")
                                          .addTo(text) |> ignore
        DiagramItem("path", diDict [
            "d", sprintf "M %f %f h 16 a 4 4 0 0 1 4 4 v 12 a 4 4 0 0 1 -4 4 h -16 z" (x + self.width - 20.f) (y - 10.f)
            "class","diagram-text"
            ]).addTo(text) |> ignore
        DiagramItem("text",  Dictionary(dict [
            "x", (x + self.width - 10.f).ToString()
            "y", (y + 4.f).ToString()
            "class","diagram-arrow"
            ]), Choice2Of2 "↺" ).addTo(text) |> ignore
        self :> DiagramItem

type HorizontalChoiceImpl(items: DiString seq) as self =
    inherit DiagramItem( "g")

    let allButLast = self.items |> Seq.take (self.items.Count - 1) 
    let middles = self.items |> Seq.skip 1
    let first = self.items.[0]
    let last = self.items |> Seq.last
    let upperTrack = [AR*2.f;VS;(allButLast |> Seq.map(fun x -> x.up) |> Seq.max) + VS] |> Seq.max

    let mutable lowerTrackContainer = 0.f
    do
        self.width <- (AR * 4.f * float32 (self.items.Count - 1) //# inbetween tracks
                      + (Seq.sumBy( fun (x:DiagramItem) -> x.width + if x.needsSpace then 20.f else 0.f ) self.items) //#items
                      + (if last.height > 0.f then AR else 0.f)) //# needs space to curve up 
                      //#ending track
        ////# Always exits at entrance height
        ////# All but the last have a track running above them
        self.up <- max upperTrack last.up
        let t = if middles |> Seq.isEmpty then 0.f else middles |> Seq.map (fun x -> x.height + max (x.down + VS) (AR*2.f) ) |> Seq.max 
        self.lowerTrack <- [VS;t;last.height + last.down + VS] |> Seq.max

        ////# All but the first have a track running below them
        ////# Last either straight-lines or curves up, so has different calculation
        if first.height < self.lowerTrack then
            //# Make sure there"s at least 2*AR room between first exit and lower track
            self.lowerTrack <- max self.lowerTrack (first.height + AR*2.f)
        self.down <- max self.lowerTrack (first.height + first.down)
        addDebug(self)

    member val items: ResizeArray<DiagramItem> = ResizeArray(items |> Seq.map wrap) with get

    member val lowerTrack = 0.f with get,set     
                                              
    override self.format(x,y,width) =
        //# Hook up the two sides if self is narrower than its stated width.
        let leftGap, rightGap = determineGaps(width, self.width)
        Path(x, y).h(leftGap).addTo(self) |> ignore
        Path(x + leftGap + self.width, y + self.height).h(rightGap).addTo(self) |> ignore
        let mutable x = x + leftGap

        //# upper track 
        let upperSpan = allButLast 
                        |> Seq.sumBy(fun x -> x.width +  if x.needsSpace then 20.f else 0.f) 
                        |> (+) (float32 (self.items.Count - 2) * AR) 
        (Path(x,y)
            .arc("se")
            .up(upperTrack - AR*2.f)
            .arc("wn")
            .h(upperSpan)
            .addTo(self)) |> ignore

        //# lower track
        let lowerSpan = (self.items
                        |> Seq.skip 1
                        |> Seq.sumBy (fun x -> x.width + (if x.needsSpace then 20.f else 0.f) ))
                        + float32 (self.items.Count - 2) * AR 
                        + (if last.height > 0.f then AR else 0.f)
        let lowerStart = x + AR * 3.f + first.width + (if first.needsSpace then 20.f else 0.f)
        (Path(lowerStart, y+ self.lowerTrack)
            .h(lowerSpan)
            .arc("se")
            .up(self.lowerTrack - AR*2.f)
            .arc("wn")
            .addTo(self)) |> ignore
        self.items
        |> Seq.iteri(fun i item ->
        //# Items
            //# input track
            if i = 0 then
                (Path(x,y).h(AR).addTo(self)) |> ignore
                x <- x + AR
            else
                (Path(x, y - upperTrack)
                    .arc("ne")
                    .v(upperTrack - AR * 2.f)
                    .arc("ws")
                    .addTo(self)) |> ignore
                x <- x + AR * 2.f

            //# item
            let itemWidth = item.width + (if item.needsSpace then 20.f else 0.f)
            item.format(x,y,itemWidth).addTo(self) |> ignore
            x <- x + itemWidth

            //# output track
            if i = self.items.Count-1 then
                if item.height = 0.f then
                    (Path(x,y).h(AR).addTo(self)) |> ignore
                else
                    (Path(x,y + item.height).arc("se").addTo(self)) |> ignore
            elif i = 0 && item.height > self.lowerTrack then
                //# Needs to arc up to meet the lower track, not down.
                if item.height - self.lowerTrack >= AR*2.f then
                    (Path(x, y+ item.height)
                        .arc("se")
                        .v(self.lowerTrack - item.height + AR*2.f)
                        .arc("wn")
                        .addTo(self)) |> ignore
                else
                   // # Not enough space to fit two arcs
                   // # so just bail and draw a straight line for now.
                    (Path(x, y + item.height)
                        .l(AR*2.f, self.lowerTrack - item.height)
                        .addTo(self)) |> ignore
            else
                (Path(x, y + item.height)
                    .arc("ne")
                    .v(self.lowerTrack - item.height - AR*2.f)
                    .arc("ws")
                    .addTo(self)) |> ignore
                    )
        self :> DiagramItem

let HorizontalChoice(items:DiString seq) = 
    if (items |> Seq.length) <= 1
        then Sequence(items) :> DiagramItem
    else HorizontalChoiceImpl(items) :> DiagramItem

let Optional(item, skip) = Choice((if skip then 0 else 1) , [Skip() :> DiagramItem |> Choice1Of2;Choice1Of2 item])


type OneOrMore(item, ?repeat: DiString) as self=
    inherit DiagramItem( "g")
    let repeat = defaultArg repeat <| (Skip() :> DiagramItem |> Choice1Of2)
                 |> wrap
    let item = wrap(item)
    do 
        self.width <- (max item.width repeat.width) + AR * 2.f
        self.height <- item.height
        self.up <- item.up
        self.down <- max (AR * 2.f) (item.down + VS + repeat.up + repeat.height + repeat.down)
        self.needsSpace <- true
        addDebug(self)

    override self.format(x,y,width)=
        let leftGap, rightGap = determineGaps(width, self.width)

        //# Hook up the two sides if self is narrower than its stated width.
        Path(x, y).h(leftGap).addTo(self) |> ignore
        Path(x + leftGap + self.width, y + self.height).h(rightGap).addTo(self) |> ignore
        let mutable x = x + leftGap

        ////# Draw item
        Path(x, y).right(AR).addTo(self) |> ignore
        item.format(x + AR,y,self.width - AR * 2.f).addTo(self) |> ignore
        Path(x + self.width - AR, y + self.height).right(AR).addTo(self) |> ignore

        //# Draw repeat arc
        let distanceFromY = max (AR*2.f) (item.height + item.down + VS + repeat.up)
        Path(x + AR, y).arc("nw").down(distanceFromY - AR * 2.f).arc("ws").addTo(self) |> ignore
        repeat.format(x + AR,y + distanceFromY,self.width - AR * 2.f).addTo(self) |> ignore
        Path(x + self.width - AR, y + distanceFromY + repeat.height).arc("se") 
            .up(distanceFromY - AR * 2.f + repeat.height - item.height).arc("en").addTo(self) |> ignore

        self :> DiagramItem

    override self.ToString()=
        sprintf "OneOrMore(%s, repeat=%s)" <| item.ToString() <| repeat.ToString()


let ZeroOrMore(item, repeat: DiString option, skip) = match repeat with
                                                      | Some(repeat) -> Optional(OneOrMore(item, repeat), skip)
                                                      | None -> Optional(OneOrMore(item),skip)

type NonTerminal(text:string,?href:string,?title:string) as self=
    inherit DiagramItem( "g", dictFromPair("class","non-terminal"))
    do
        self.width <- float32 text.Length * CHAR_WIDTH + 20.f
        self.up <- 11.f
        self.down <- 11.f
        self.needsSpace <- true
        addDebug(self)

    override self.ToString()= sprintf "NonTerminal(%s, href=%A, title=%A)" text href title

    override self.format(x,y,width)=
        let leftGap, rightGap = determineGaps(width, self.width)

        //# Hook up the two sides if self is narrower than its stated width.
        Path(x, y).h(leftGap).addTo(self) |> ignore
        Path(x + leftGap + self.width, y).h(rightGap).addTo(self) |> ignore

        DiagramItem("rect",diDict ["x",(x + leftGap).ToString()
                                   "y",(y - 11.f).ToString()
                                   "width", self.width.ToString()
                                   "height",(self.up + self.down).ToString()])
                                   .addTo(self) |> ignore
        let textItem = DiagramItem("text", diDict ["x",(x + width / 2.f).ToString()
                                                   "y",(y + 4.f).ToString()], Choice2Of2 text)
        textItem.addTo(match href with
                       | Some(href) -> DiagramItem("a", dictFromPair("xlink:href",href), Choice2Of2 text).addTo(self)
                       | None -> self :> DiagramItem) |> ignore
        match title with
        | Some(title) -> DiagramItem("title", Dictionary(), Choice2Of2(title)).addTo(self) |> ignore
        | None -> ()
        self :> DiagramItem


type Comment(text:string,?href:string ,?title:string) as self=
    inherit DiagramItem("g")
    do
        self.width <- float32 text.Length * COMMENT_CHAR_WIDTH + 10.f
        self.up <- 11.f
        self.down <- 11.f
        self.needsSpace <- true
        addDebug(self)

    override self.ToString()=
        sprintf "Comment(%s, href=%A, title=%A)" text (href) (title)

    override self.format(x,y,width)=
        let leftGap, rightGap = determineGaps(width, self.width)

        //# Hook up the two sides if self is narrower than its stated width.
        Path(x, y).h(leftGap).addTo(self) |> ignore
        Path(x + leftGap + self.width, y).h(rightGap).addTo(self) |> ignore

        let textDi = DiagramItem("text", diDict [ "x",(x + width / 2.f).ToString()
                                                  "y",(y + 5.f).ToString()
                                                  "class", "comment"],Choice2Of2 text)
        textDi.addTo(match href with
                     | Some (href) -> DiagramItem("a", dictFromPair("xlink:href",href) , Choice2Of2(text)).addTo(self) 
                     | None -> self :> DiagramItem) |> ignore
        match title with
        | Some(title) -> DiagramItem("title", Dictionary(), Choice2Of2(title)).addTo(self) |> ignore
        | None -> ()
        self :> DiagramItem


let add(name, diagram: DiagramItem, writer: TextWriter option)=
    let writer : TextWriter = defaultArg writer System.Console.Out 
    writer.Write(sprintf "<h1>%s</h1>\n" <| Escape(name))
    diagram.writeSvg(writer)
    writer.Write("\n")