using Avalonia;
using Avalonia.Markup.Xaml;

namespace ANTLRStudio
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
   }
}