using ANTLRStudio.Editor;

namespace ANTLRStudio.Editor.Commands
{
    public abstract class Command
    {
        public TextSource ts;
        public abstract void Execute();
    }
}