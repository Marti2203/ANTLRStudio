[<AutoOpen>]
module Utilites
open Eto.Forms;
open Eto.Drawing;
open System;
type TCell =
| El of Control
| StretchedEl of Control
| EmptyElement
| TableEl of Table
and TRow =
| Row of TCell seq
| StretchedRow of TCell seq
| Spacing of Size
| Pad of Padding
and Table = Tbl of TRow seq
let rec makeLayout (Tbl t) =
    let ret = new TableLayout()
    for r in t  do
        let makeTd (tds:TCell seq) =
            let row = new TableRow()
            for td in tds do
                match td with
                | El c -> row.Cells.Add(new TableCell(c, false))
                | StretchedEl c -> row.Cells.Add(new TableCell(c, true))
                | EmptyElement -> row.Cells.Add(new TableCell(null, true))
                | TableEl t -> row.Cells.Add(new TableCell(makeLayout t, true))
            row
        match r with
        | Row tds -> let r = makeTd tds in ret.Rows.Add(r)
        | StretchedRow tds -> let r = makeTd tds in r.ScaleHeight <- true; ret.Rows.Add(r)
        | Spacing sz -> ret.Spacing <- sz
        | Pad pad -> ret.Padding <- pad
    ret
type Menu =
    | Item of MenuItem
    | ActionMenuItem of string
    | RadioMenuItem of string*string
    | CheckMenuItem of string
    | SubMenu of string*Menu seq
    | Action of Menu*(MenuItem -> unit)
    | Check of Menu*bool
    | Shortcut of Menu*Keys
    member m.WithAction cb = Action(m, cb)
    member m.WithCheck () = Check(m, true)
    member m.WithShortcut k = Shortcut(m, k)
let private radioGroup = new System.Collections.Generic.Dictionary<string, RadioMenuItem>()
let rec makeMenu (menu) =
    match menu with
    | Item m -> m
    | ActionMenuItem lbl ->
        let m = new ButtonMenuItem(Text=lbl)
        m :> _
    | RadioMenuItem (group, lbl) ->
        let m = if radioGroup.ContainsKey(group) then
                    new RadioMenuItem(radioGroup.[group], Text=lbl)
                else
                    let g = new RadioMenuItem(Text=lbl)
                    radioGroup.[group] <- g
                    g
        m :> _
    | CheckMenuItem lbl ->
        let m = new CheckMenuItem(Text=lbl)
        m :> _
    | SubMenu (lbl, lst) ->
        let m = new ButtonMenuItem(Text=lbl)
        for el in lst do
            m.Items.Add(makeMenu el)
        m :> _
    | Action (m, cb) ->
        let ret = makeMenu m
        ret.Click.Add(fun _ -> cb(ret))
        ret
    | Check (m, def) ->
        let ret = makeMenu m
        match ret with
        | :? RadioMenuItem as r -> r.Checked <- def
        | :? CheckMenuItem as c -> c.Checked <- def
        | _ -> ()
        ret
    | Shortcut (m, k) ->
        let ret = makeMenu m
        ret.Shortcut <- k
        ret
let action cb (m:Menu) = m.WithAction cb
let check (m:Menu) = m.WithCheck ()
let shortcut (k:Keys) (m:Menu) = m.WithShortcut k
let rec displayException (e :Exception) =
    printfn "%s %s %s" e.Message Environment.NewLine e.StackTrace
    if e.InnerException <> null then displayException e.InnerException