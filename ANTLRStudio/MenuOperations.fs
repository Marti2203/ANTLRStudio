module MenuOperations
open Eto.Forms;
open AntlrTools;
open FormElements;
let RadioMenuItem = Menu.RadioMenuItem
let CheckMenuItem = Menu.CheckMenuItem
let mutable private addedMenus = false


let addMenus (form:Form) (menuItems: MenuItem seq) =
    menuItems |> Seq.iter form.Menu.Items.Add
let removeMenus (form:Form) (menuItems: MenuItem seq) =
    menuItems |> Seq.iter (form.Menu.Items.Remove >> ignore)

let specificMenus =
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
    ActionMenuItem("Generate")  |> action (fun _ -> generate file language options)
    ] 
    |> Seq.map makeMenu

let addSpecificMenus form =
    if not addedMenus then
        addedMenus <- true
        addMenus form specificMenus
let removeSpecificMenus form =
    if addedMenus then
        addedMenus <- false
        addMenus form specificMenus
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
   // loadedFile.Add(fun _ -> addSpecificMenus form)
    form.Menu <- menu
    form
