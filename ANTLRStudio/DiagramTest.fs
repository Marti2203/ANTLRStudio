module DiagramTest
open RailwayPortPy
let t v = Choice2Of2 v
let test writer = 
        add("comment", Diagram( [t "/*"
                                 ZeroOrMore(Choice1Of2(NonTerminal "anything but * followed by /" :> DiagramItem),None,false) 
                                 :> DiagramItem |> Choice1Of2
                                 t "*/"
                                 ]), writer)

        add("newline", Diagram([Choice1Of2(Choice(0, 
                                                    [t "\\n"
                                                     t "\\r\\n"
                                                     t "\\r"
                                                     t "\\f"]):> DiagramItem)]),writer)

        add("whitespace", Diagram([Choice1Of2(Choice(0, [Choice2Of2("space")
                                                         Choice2Of2("\\t")
                                                         Choice1Of2(NonTerminal("newline") :> DiagramItem)]):> DiagramItem)]),writer)

        add("hex digit", Diagram([Choice1Of2(NonTerminal("0-9 a-f or A-F"):> DiagramItem)]),writer)

        //add("escape", Diagram([Choice2Of2 "\\"
                               //Choice1Of2(Choice(0,[Choice1Of2 NonTerminal "not newline or hex digit"
                                              //      Choice1Of2(Sequence([Choice1Of1 OneOrMore(NonTerminal("hex digit"), Comment("1-6 times"))), 
                                              //Optional(NonTerminal("whitespace"), "skip")])))]]),writer)

        add("<whitespace-token>", Diagram([OneOrMore(NonTerminal("whitespace") :> DiagramItem |> Choice1Of2) 
                                           :> DiagramItem |> Choice1Of2]),writer )

        add("ws*", Diagram([ZeroOrMore(NonTerminal("<whitespace-token>") :> DiagramItem |> Choice1Of2,None,false)
                            :> DiagramItem |> Choice1Of2]),writer)

        //add("<ident-token>", Diagram( Choice(0, Skip(), "-"),
        //                              Choice(0, NonTerminal("a-z A-Z _ or non-ASCII"), NonTerminal("escape")),
        //                              ZeroOrMore(Choice(0,NonTerminal("a-z A-Z 0-9 _ - or non-ASCII"), NonTerminal("escape")))))

        //add("<function-token>", Diagram( NonTerminal("<ident-token>"), "("))

        //add("<at-keyword-token>", Diagram( "@", NonTerminal("<ident-token>")))

        //add("<hash-token>", Diagram( "#", OneOrMore(Choice(0, NonTerminal("a-z A-Z 0-9 _ - or non-ASCII"), NonTerminal("escape")))))

        //add("<string-token>", Diagram( Choice(0,
        //                                Sequence(
        //                                    """,
        //                                    ZeroOrMore(
        //                                        Choice(0,
        //                                            NonTerminal("not " \\ or newline"),
        //                                            NonTerminal("escape"),
        //                                            Sequence("\\", NonTerminal("newline")))),
        //                                    """),
        //                                Sequence("\"",
        //                                            ZeroOrMore(
        //                                                Choice(0,
        //                                                    NonTerminal("not ' \\ or newline"),
        //                                                    NonTerminal("escape"),
        //                                                    Sequence("\\", NonTerminal("newline")))),
        //                                            "\""))))

        //add("<url-token>", Diagram(
        //    NonTerminal("<ident-token "url">"),
        //    "(",
        //    NonTerminal("ws*"),
        //    Optional(Sequence(
        //        Choice(0, NonTerminal("url-unquoted"), NonTerminal("STRING")),
        //        NonTerminal("ws*"))),
        //    ")"))

        //add("url-unquoted", Diagram(OneOrMore(
        //    Choice(0,
        //        NonTerminal("not \" ' ( ) \\ whitespace or non-printable"),
        //        NonTerminal("escape")))))

        //add("<number-token>", Diagram(
        //    Choice(1, "+", Skip(), "-"),
        //    Choice(0,
        //        Sequence(
        //            OneOrMore(NonTerminal("digit")),
        //            ".",
        //            OneOrMore(NonTerminal("digit"))),
        //        OneOrMore(NonTerminal("digit")),
        //        Sequence(
        //            ".",
        //            OneOrMore(NonTerminal("digit")))),
        //    Choice(0,
        //        Skip(),
        //        Sequence(
        //            Choice(0, "e", "E"),
        //            Choice(1, "+", Skip(), "-"),
        //            OneOrMore(NonTerminal("digit"))))))

        //add("<dimension-token>", Diagram(
        //    NonTerminal("<number-token>"), NonTerminal("<ident-token>")))

        //add("<percentage-token>", Diagram(
        //    NonTerminal("<number-token>"), "%"))

        //add("<unicode-range-token>", Diagram(
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

        //add("Stylesheet", Diagram(ZeroOrMore(Choice(3,
        //    NonTerminal("<CDO-token>"), NonTerminal("<CDC-token>"), NonTerminal("<whitespace-token>"),
        //    NonTerminal("Qualified rule"), NonTerminal("At-rule")))))

        //add("Rule list", Diagram(ZeroOrMore(Choice(1,
        //    NonTerminal("<whitespace-token>"), NonTerminal("Qualified rule"), NonTerminal("At-rule")))))

        //add("At-rule", Diagram(
        //    NonTerminal("<at-keyword-token>"), ZeroOrMore(NonTerminal("Component value")),
        //    Choice(0, NonTerminal("{} block"), "")))

        //add("Qualified rule", Diagram(
        //    ZeroOrMore(NonTerminal("Component value")),
        //    NonTerminal("{} block")))

        //add("Declaration list", Diagram(
        //    NonTerminal("ws*"),
        //    Choice(0,
        //        Sequence(
        //            Optional(NonTerminal("Declaration")),
        //            Optional(Sequence("", NonTerminal("Declaration list")))),
        //        Sequence(
        //            NonTerminal("At-rule"),
        //            NonTerminal("Declaration list")))))

        //add("Declaration", Diagram(
        //    NonTerminal("<ident-token>"), NonTerminal("ws*"), ":",
        //    ZeroOrMore(NonTerminal("Component value")), Optional(NonTerminal("!important"))))

        //add("!important", Diagram(
        //    "!", NonTerminal("ws*"), NonTerminal("<ident-token "important">"), NonTerminal("ws*")))

        //add("Component value", Diagram(Choice(0,
        //    NonTerminal("Preserved token"),
        //    NonTerminal("{} block"),
        //    NonTerminal("() block"),
        //    NonTerminal("[] block"),
        //    NonTerminal("Function block"))))


        //add("{} block", Diagram("{", ZeroOrMore(NonTerminal("Component value")), "}"))
        //add("() block", Diagram("(", ZeroOrMore(NonTerminal("Component value")), ")"))
        //add("[] block", Diagram("[", ZeroOrMore(NonTerminal("Component value")), "]"))

        //add("Function block", Diagram(
        //    NonTerminal("<function-token>"),
        //    ZeroOrMore(NonTerminal("Component value")),
        //    ")"))

        //add("glob pattern", Diagram(
        //    AlternatingSequence(
        //        NonTerminal("ident"),
        //        "*")))

        //add("SQL", Diagram(
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