[<AutoOpen>]
module State
open Antlr4.Runtime
let mutable language : string = null
let mutable file: string = null
let loadedFileInput = new Event<string>()
let loadedFile = loadedFileInput.Publish
let mutable currentParser : Parser = null
let mutable currentLexer  : Lexer = null