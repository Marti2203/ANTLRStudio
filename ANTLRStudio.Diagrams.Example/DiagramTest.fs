module DiagramTest
open RailwayPortPy
let text v = Choice2Of2 v
let items vs = Diagram vs
let itemsTwo (a,b) = items [a;b]
let itemsThree (a,b,c) = items[a;b;c] 
let inline item (v: DiagramItem)= Choice1Of2 v
let nonterm v = NonTerminal(v)
let nontermItem v = nonterm >> item <| v
let term v = Terminal(v)
let zeroMore v = ZeroOrMore(v,None,false)
let zeroMoreItem v = zeroMore >> item <| v
let oneMore v = OneOrMore(v)
let oneMoreRepeat v r = OneOrMore(v,r)
let oneMoreRepeatItem v r = oneMoreRepeat v r |> item 
let oneMoreItem v = oneMore >> item <| v
let choice i vs = Choice(i,vs)
let choiceItem i vs = (choice i) >> item <| vs
let choiceItemZ vs = choiceItem 0 vs
let sequence vs = Sequence(vs)
let sequenceItem vs = sequence >> item <| vs 
let comment v = Comment(v)
let commentItem v = comment >> item <| v 
let optional v skip = Optional(v,skip)
let optionalItem v skip = (optional v skip) |> item
let skip () = Skip()
let skipItem () = skip >> item <| ()
let caseInsensitiveChoice (c:char) = choiceItemZ [
                                                  c |> System.Char.ToUpper |> string |> text
                                                  c |> System.Char.ToLower |> string |> text
                                                 ]
let test writer = 
        add("comment", items [text "/*"
                              "anything but * followed by /" |> nontermItem |> zeroMoreItem
                              text "*/"
                              ], writer)

        add("newline", items [choiceItemZ [text"\\n"
                                           text"\\r\\n"
                                           text"\\r"
                                           text"\\f"]],writer)

        add("whitespace", items [choiceItemZ [text"space"
                                              text"\\t"
                                              nontermItem "newline"]],writer)

        add("hex digit", items[nontermItem "0-9 a-f or A-F"],writer)

        add("escape", items [text "\\"
                             choiceItemZ [nontermItem"not newline or hex digit"
                                          sequenceItem [oneMoreRepeatItem <| nontermItem "hex digit" <| commentItem "1-6 times" 
                                                        optionalItem <| nonterm "whitespace" <| true]]],writer)

        add("<whitespace-token>", items ["whitespace" |> nontermItem |> oneMoreItem],writer)

        add("ws*", items["<whitespace-token>" |> nontermItem |> zeroMoreItem],writer)

        add("<ident-token>", items[choiceItemZ [skipItem(); text "-"]
                                   choiceItemZ [nontermItem "a-z A-Z _ or non-ASCII"; nontermItem "escape"]
                                   zeroMoreItem <| choiceItemZ[nontermItem "a-z A-Z 0-9 _ - or non-ASCII"; nontermItem "escape" ]],writer)

        add("<function-token>", items[ nontermItem "<ident-token>"; text "("],writer)

        add("<at-keyword-token>", itemsTwo(text "@", nontermItem "<ident-token>"), writer)

        add("<hash-token>", itemsTwo( text "#", [nontermItem "a-z A-Z 0-9 _ - or non-ASCII"
                                                 nontermItem "escape"] |>  choiceItemZ |> oneMoreItem),writer)

        add("<string-token>", items[ choiceItemZ [
                                        sequenceItem [ text "\""
                                                       [
                                                       nontermItem "not \" \\ or newline"
                                                       nontermItem "escape"
                                                       sequenceItem [text "\\" ; nontermItem "newline"]
                                                       ] 
                                                       |> choiceItemZ |> zeroMoreItem
                                                       text "\""
                                                     ]
                                        sequenceItem [ text "'"
                                                       [
                                                        nontermItem "not ' \\ or newline"
                                                        nontermItem "escape"
                                                        sequenceItem [text "\\" ; nontermItem "newline"]
                                                       ] 
                                                       |> choiceItemZ |> zeroMoreItem 
                                                       text"'" 
                                                     ]
                                                    ]],writer)
        add("<url-token>", items [
            nontermItem "<ident-token \"url\">"
            text "("
            nontermItem "ws*"
            optionalItem <| sequence [
                                       choiceItemZ[nontermItem "url-unquoted";nontermItem "STRING"]
                                       nontermItem "ws*"
                                     ] <| false
            text ")"],writer)

        add("url-unquoted", items [oneMoreItem <| choiceItemZ[ nontermItem "not \" ' ( ) \\ whitespace or non-printable";nontermItem "escape"]],writer)

        add("<number-token>", items[
            choiceItem 1 [text "+"; skipItem(); text "-"]
            choiceItemZ [
                         sequenceItem [
                                        oneMoreItem(nontermItem "digit")
                                        text "."
                                        oneMoreItem(nontermItem "digit")
                                      ]
                         oneMoreItem  (nontermItem "digit")
                         sequenceItem [
                                        text "."
                                        oneMoreItem(nontermItem "digit")
                                      ]
                        ]
            choiceItemZ [
                         skipItem()
                         sequenceItem[
                                      caseInsensitiveChoice 'e'
                                      choiceItem 1 [text "+";skipItem();text "-"]
                                      oneMoreItem(nontermItem "digit")
                                      ]
                        ]
        ],writer)

        add("<dimension-token>", items [nontermItem "<number-token>";nontermItem "<ident-token>"],writer)

        add("<percentage-token>", items[nontermItem "<number-token>"; text "%"],writer)

        //add("<unicode-range-token>", items(
        //    Choice(0,
        //        "U",
        //        "u"),
        //    "+",
        //    Choice(0,
        //        Sequence(OneOrMore(NonTerminal("hex digit"), Comment("1-6 times"))),
        //        Sequence(
        //            ZeroOrMore(NonTerminal("hex digit"), Comment("1-5 times")),
        //            OneOrMore("?", Comment("1 to (6 - digits) times"))),
        //        Sequence(
        //            OneOrMore(NonTerminal("hex digit"), Comment("1-6 times")),
        //            "-",
        //            OneOrMore(NonTerminal("hex digit"), Comment("1-6 times"))))))

        //NonTerminal = NonTerminal

        //add("Stylesheet", items(ZeroOrMore(Choice(3,
        //    NonTerminal("<CDO-token>"), NonTerminal("<CDC-token>"), NonTerminal("<whitespace-token>"),
        //    NonTerminal("Qualified rule"), NonTerminal("At-rule")))))

        //add("Rule list", items(ZeroOrMore(Choice(1,
        //    NonTerminal("<whitespace-token>"), NonTerminal("Qualified rule"), NonTerminal("At-rule")))))

        //add("At-rule", items(
        //    NonTerminal("<at-keyword-token>"), ZeroOrMore(NonTerminal("Component value")),
        //    Choice(0, NonTerminal("{} block"), "")))

        add("Qualified rule", items[ "Component value" |> nontermItem |> zeroMoreItem
                                     "{} block" |> nontermItem],writer)

        //add("Declaration list", items(
        //    NonTerminal("ws*"),
        //    Choice(0,
        //        Sequence(
        //            Optional(NonTerminal("Declaration")),
        //            Optional(Sequence("", NonTerminal("Declaration list")))),
        //        Sequence(
        //            NonTerminal("At-rule"),
        //            NonTerminal("Declaration list")))))

        add("Declaration", items [
            nontermItem "<ident-token>"
            nontermItem "ws*"
            text ":"
            ZeroOrMore(nontermItem "Component value", Some("!important" |> nonterm |> optionalItem <| false) ,false) |> item
        ],writer)

        add("!important", items[ text "!";nontermItem "ws*"; nontermItem "<ident-token \"important\">";nontermItem "ws*"],writer)

        add("Component value", items[choiceItemZ [
            "Preserved token" |> nontermItem
            "{} block" |> nontermItem
            "() block" |> nontermItem
            "[] block" |> nontermItem
            "Function block" |> nontermItem
        ]],writer)


        add("{} block", items[text "{"; "Component value" |> nontermItem |> zeroMoreItem; text "}"],writer)
        add("() block", items[text "("; "Component value" |> nontermItem |> zeroMoreItem; text ")"],writer)
        add("[] block", items[text "["; "Component value" |> nontermItem |> zeroMoreItem; text "]"],writer)

        //add("Function block", items(
        //    NonTerminal("<function-token>"),
        //    ZeroOrMore(NonTerminal("Component value")),
        //    ")"))

        //add("glob pattern", items(
        //    AlternatingSequence(
        //        NonTerminal("ident"),
        //        "*")))

        //add("SQL", items(
        //    Stack(
        //        Sequence(
        //            "SELECT",
        //            Optional("DISTINCT", "skip"),
        //            Choice(0,
        //                "*",
        //                OneOrMore(
        //                    Sequence(NonTerminal("expression"), Optional(Sequence("AS", NonTerminal("output_name")))),
        //                    ","
        //                )
        //            ),
        //            "FROM",
        //            OneOrMore(NonTerminal("from_item"), ","),
        //            Optional(Sequence("WHERE", NonTerminal("condition")))
        //        ),
        //        Sequence(
        //            Optional(Sequence("GROUP BY", NonTerminal("expression"))),
        //            Optional(Sequence("HAVING", NonTerminal("condition"))),
        //            Optional(Sequence(
        //                Choice(0, "UNION", "INTERSECT", "EXCEPT"),
        //                Optional("ALL"),
        //                NonTerminal("select")
        //            ))
        //        ),
        //        Sequence(
        //            Optional(Sequence(
        //                "ORDER BY",
        //                OneOrMore(Sequence(NonTerminal("expression"), Choice(0, Skip(), "ASC", "DESC")), ","))
        //            ),
        //            Optional(Sequence(
        //                "LIMIT",
        //                Choice(0, NonTerminal("count"), "ALL")
        //            )),
        //            Optional(Sequence("OFFSET", NonTerminal("start"), Optional("ROWS")))
        //))))