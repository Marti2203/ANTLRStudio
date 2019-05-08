module MenuOperations
open Eto.Forms;
open AntlrTools;
open FormElements;
open System.IO
open System

let RadioMenuItem = Menu.RadioMenuItem
let CheckMenuItem = Menu.CheckMenuItem
let mutable private addedMenus = false


let addMenus (form:Form) (menuItems: MenuItem seq) =
    menuItems |> Seq.iter form.Menu.Items.Add
let removeMenus (form:Form) (menuItems: MenuItem seq) =
    menuItems |> Seq.iter (form.Menu.Items.Remove >> ignore)

let generateIn (form:Form) =
    let dir = Directory.GetCurrentDirectory() // Eto.GTK changes the directory...
    
    use dialog = new SelectFolderDialog(Title = "Select Folder For File generation",
                                        Directory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop))
    let dialogResult = dialog.ShowDialog(form.ParentWindow)
    Directory.SetCurrentDirectory(dir) // Eto.GTK changes the directory...
    let output directory = 
        [{
         Name="Output Directory Flag"; Value= true; ActiveFlag="-o";InactiveFlag=String.Empty
         }
         {
         Name="Output Directory Location"; Value= true; ActiveFlag=(sprintf "\"%s\"" directory);InactiveFlag=String.Empty
         }]
    match dialogResult with
    | DialogResult.Ok -> if dialog.Directory <> null 
                         then
                            generate file language (options |> Seq.append(output dialog.Directory))
    | v -> printf "User pressed %O" v

let createSpecificMenus (form:Form) =
    let transformCheckItem option =
        let item = CheckMenuItem(option.Name)
                   |> action (fun _ -> option.Value <- not option.Value )
        if not option.Value 
            then item 
        else (item |> check)
    [
    SubMenu("Language", languages 
                        |> Seq.map(fun (name,value) -> RadioMenuItem("Languages",name) 
                                                       |> action (fun _ -> language <- value)))
    SubMenu("Options",  options |> Seq.map transformCheckItem)
    ActionMenuItem("Generate")  |> action (fun _ -> generateIn form)
    ActionMenuItem("Edit Grammar") |> action (fun _ -> MessageBox.Show("WIP","Editor",MessageBoxType.Information) |> ignore)
    ] 
    |> Seq.map makeMenu
let mutable specificMenus = null
let addSpecificMenus form =
    if not addedMenus then
        addedMenus <- true
        specificMenus <- (createSpecificMenus form)
        addMenus form specificMenus
let removeSpecificMenus form =
    if addedMenus then
        addedMenus <- false
        removeMenus form specificMenus
let setupInitialMenus (app:Application) (form:Form) =
    let menu = new MenuBar()
    let menus = [
                SubMenu("File",
                    [
                     ActionMenuItem("Open Grammar") |> action (fun _ -> openGrammar(form))
                     ActionMenuItem("Close Grammar")|> action (fun _ -> printfn """ "Closing" grammar """)
                     ActionMenuItem("Quit") |> action (fun _ -> app.Quit())
                    ])
                ]
    menus |> Seq.iter (menu.Items.Add << makeMenu)
    loadedFile.Add(fun _ -> addSpecificMenus form)
    form.Menu <- menu
    form
