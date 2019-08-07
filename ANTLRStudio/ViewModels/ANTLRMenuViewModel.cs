using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ANTLRStudio.Views;
using Avalonia.Controls;
using Avalonia;
using Avalonia.LogicalTree;
using ReactiveUI;
using System.Reactive;
using ANTLRStudio.ANTLR;
using ANTLRStudio.Models;
using DynamicData;

namespace ANTLRStudio.ViewModels
{
    public class ANTLRMenuViewModel : ViewModelBase
    {
        #region Events and Delegates
        public delegate void GrammarOpenedHandler(GrammarOpenedEventArgs e);
        public delegate void GrammarClosedHandler(GrammarClosedEventArgs e);
        public delegate void GrammarGenerationLanguageHandler(GrammarGenerationLanguageEventArgs e);

        public static event GrammarOpenedHandler GrammarOpened;
        public static event GrammarClosedHandler GrammarClosed;
        public static event GrammarGenerationLanguageHandler GrammarGenerationLanguageChanged;
        #endregion

        #region Public Properties
        public string LanguageFlag { get; set; }
        public string GrammarPath { get; private set; }
        public string GrammarName { get; private set; }
        public bool HasGrammarOpen => GrammarName != null;

        public IReadOnlyList<MenuItemViewModel> MenuItems { get; set; }
        public ReactiveCommand<Unit, Unit> OpenCommand { get; set; }
        public ReactiveCommand<Unit, Unit> CloseCommand { get; set; }
        public ReactiveCommand<Unit, Unit> GenerateFromGrammarCommand { get; set; }
        public ReactiveCommand<Unit, Unit> ExitCommand { get; set; }
        public ReactiveCommand<CompilerOption, Unit> OptionClickedCommand { get; set; }
        public ReactiveCommand<(string, string), Unit> LanguageSelectedCommand { get; set; }
        #endregion

        #region Static and Read Only Data
        private static readonly FileDialogFilter g4Filter = new FileDialogFilter
        {
            Name = "Antlr4 files (.g4)",
            Extensions = new List<string> { "g4" }
        };

        private static readonly OpenFileDialog grammarDialog = new OpenFileDialog
        {
            AllowMultiple = false,
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            Title = "Open Antlr4 Grammar",
            Filters = new List<FileDialogFilter> { g4Filter }
        };

        private readonly MenuItemViewModel GrammarMenu;
        private readonly MenuItemViewModel LanguagesMenu;
        private readonly MenuItemViewModel OptionsMenu;

        private MenuItemViewModel GenerateFromGrammarMenu => new MenuItemViewModel
        {
            Header = "Generate From Grammar",
            Command = GenerateFromGrammarCommand,
        };

        #endregion

        #region Methods
        public async Task OpenGrammar()
        {
            var res = await grammarDialog.ShowAsync(Application.Current.MainWindow);
            if (res.Length == 1)
            {
                GrammarPath = res[0];
                GrammarName = GrammarPath.Split(Path.PathSeparator).Last();
                GrammarOpened?.Invoke(new GrammarOpenedEventArgs(GrammarName, GrammarPath));
            }
        }
        public void CloseGrammar()
        {
            if (!HasGrammarOpen) return;
            GrammarClosed?.Invoke(new GrammarClosedEventArgs(GrammarName, GrammarPath));
            GrammarName = null;
            GrammarPath = null;
        }

        public async Task GenerateFromGrammar()
        {
            var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            if (!HasGrammarOpen) return;
            var folderDialog = new OpenFolderDialog
            {
                Title = $"Select Folder For File generation. Current Language {LanguageFlag ?? "Java"}",
                DefaultDirectory = Directory.CreateDirectory(Path.Combine(desktop, $"{GrammarName}_{LanguageFlag}")).FullName,
                InitialDirectory = desktop
            };
            var dialogResult = await folderDialog.ShowAsync(Application.Current.MainWindow);
            var options = new[] {
            new CompilerOption(name: "Output Directory Flag", value: true, activeFlag: "-o", inactiveFlag: String.Empty),
            new CompilerOption(name: "Output Directory Location", value: true, activeFlag: dialogResult, inactiveFlag: String.Empty)
            };
            Tooling.GenerateFiles(GrammarPath, LanguageFlag, Data.CompilerOptions.Concat(options));
        }

        public void Exit()
        {
            Application.Current.Exit();
        }

        public void LanguageSelected((string, string) languageTuple)
        {
            (string name, string flag) = languageTuple;

            LanguageFlag = flag;

            (LanguagesMenu.Items.First(it => it.Header == name).Icon as RadioButton).IsChecked = true;

            GrammarGenerationLanguageChanged?.Invoke(new GrammarGenerationLanguageEventArgs(flag));
        }

        private void OptionClicked(CompilerOption option)
        {
            option.InverseValue();
            var checkbox = OptionsMenu.Items.First(x => x.Header == option.Name).Icon as CheckBox;
            checkbox.IsChecked = !checkbox.IsChecked;
        }

        private void OnGrammarClosed(GrammarClosedEventArgs e)
        {
            MenuItems = new List<MenuItemViewModel>
            {
                GrammarMenu
            }.AsReadOnly();

            this.RaisePropertyChanged(nameof(MenuItems));
        }

        public void OnGrammarOpened(GrammarOpenedEventArgs e)
        {
            MenuItems = new List<MenuItemViewModel>
            {
                GrammarMenu,
                OptionsMenu,
                LanguagesMenu,
                GenerateFromGrammarMenu
            }.AsReadOnly();
            this.RaisePropertyChanged(nameof(MenuItems));
        }
        #endregion

        #region Constructor
        public ANTLRMenuViewModel()
        {
            GenerateFromGrammarCommand = ReactiveCommand.CreateFromTask(GenerateFromGrammar);
            LanguageSelectedCommand = ReactiveCommand.Create<(string, string)>(LanguageSelected);
            OptionClickedCommand = ReactiveCommand.Create<CompilerOption>(OptionClicked);
            OpenCommand = ReactiveCommand.CreateFromTask(OpenGrammar);
            CloseCommand = ReactiveCommand.Create(CloseGrammar);
            ExitCommand = ReactiveCommand.Create(Exit);

            LanguagesMenu = new MenuItemViewModel
            {
                Header = "_Language",
                Items = Data.Languages.Select(languageTuple =>
                 new MenuItemViewModel
                 {
                     Header = languageTuple.Name,
                     Command = LanguageSelectedCommand,
                     CommandParameter = languageTuple,
                     Icon = new RadioButton { GroupName = "LanguageGroup" }
                 }).ToList()
            };

            OptionsMenu = new MenuItemViewModel
            {
                Header = "_Options",
                Items = Data.CompilerOptions.Select(option =>
                new MenuItemViewModel
                {
                    Header = option.Name,
                    Command = OptionClickedCommand,
                    Icon = new CheckBox { IsChecked = option.Value },
                    CommandParameter = option
                }).ToList(),
            };

            GrammarMenu = new MenuItemViewModel
            {
                Header = "_Grammar",
                Items = new List<MenuItemViewModel>
            {
                new MenuItemViewModel
                {
                    Header="_Open",
                    Command = OpenCommand,
                },

                new MenuItemViewModel
                {
                    Header="_Close",
                    Command = CloseCommand
                },

                new MenuItemViewModel
                {
                    Header ="Exit",
                    Command = ExitCommand
                }
            }
            };


            MenuItems = new List<MenuItemViewModel>()
            {
                GrammarMenu
            }.AsReadOnly();

            GrammarOpened += OnGrammarOpened;
            GrammarClosed += OnGrammarClosed;
        }

        #endregion

    }
}
