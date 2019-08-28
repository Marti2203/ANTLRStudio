
namespace ANTLRStudio.TreeLayout
{

    public class DumpConfiguration
    {
        /**
         * The text used to indent the output per level.
         */
        public readonly string Indent;
        /**
         * When true the dump also includes the size of each node, otherwise
         * not.
         */
        public readonly bool IncludeNodeSize;
        /**
         * When true, the text as returned by {@link Object#toString()}, is
         * included in the dump, in addition to the text returned by the
         * possibly overridden toString method of the node. When the hashCode
         * method is overridden the output will also include the
         * "identityHashCode".
         */
        public readonly bool IncludeObjectToString;

        /**
         * 
         * @param indent [default: "    "]
         * @param includeNodeSize [default: false]
         * @param includePointer [default: false]
         */
        public DumpConfiguration(string indent, bool includeNodeSize,
                bool includePointer)
        {
            Indent = indent;
            IncludeNodeSize = includeNodeSize;
            IncludeObjectToString = includePointer;
        }

        public DumpConfiguration() : this("    ", false, false)
        {

        }
    }
}
