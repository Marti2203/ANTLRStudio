namespace GrammarGrammar
{
    using System.IO;
    using Antlr4.Runtime;
    using Antlr4.Runtime.Misc;
    using ANTLRStudio.Parser;

#pragma warning disable CA1012 // Abstract types should not have constructors
    public abstract class LexerAdaptor : Lexer
#pragma warning restore CA1012 // But Lexer demands it
    {
        protected LexerAdaptor(ICharStream input)
            : base(input)
        {
        }
        protected LexerAdaptor(ICharStream input, TextWriter output, TextWriter errorOutput)
        : base(input, output, errorOutput)
        { }
        /**
         * Track whether we are inside of a rule and whether it is lexical parser. _currentRuleType==TokenConstants.InvalidType
         * means that we are outside of a rule. At the first sign of a rule name reference and _currentRuleType==invalid, we
         * can assume that we are starting a parser rule. Similarly, seeing a token reference when not already in rule means
         * starting a token rule. The terminating ';' of a rule, flips this back to invalid type.
         *
         * This is not perfect logic but works. For example, "grammar T;" means that we start and stop a lexical rule for
         * the "T;". Dangerous but works.
         *
         * The whole point of this state information is to distinguish between [..arg actions..] and [charsets]. Char sets
         * can only occur in lexical rules and arg actions cannot occur.
         */
        private int _currentRuleType;

        public int getCurrentRuleType()
        {
            return _currentRuleType;
        }

        public void setCurrentRuleType(int ruleType)
        {
            this._currentRuleType = ruleType;
        }

        protected void handleBeginArgument()
        {
            if (inLexerRule())
            {
                PushMode(ANTLRv4Lexer.LexerCharSet);
                More();
            }
            else
            {
                PushMode(ANTLRv4Lexer.Argument);
            }
        }

        protected void handleEndArgument()
        {
            PopMode();
            if (ModeStack.Count > 0)
            {
                setCurrentRuleType(ANTLRv4Lexer.ARGUMENT_CONTENT);
            }
        }

        protected void handleEndAction()
        {
            PopMode();
            if (ModeStack.Count > 0)
            {
                setCurrentRuleType(ANTLRv4Lexer.ACTION_CONTENT);
            }
        }


        public override IToken NextToken()
        {
            if (Type == ANTLRv4Lexer.ID)
            {
                //string firstChar = Text[0];// (Interval.Of(TokenStartCharIndex, TokenStartCharIndex));
                if (char.IsUpper(Text[0]))
                {
                    Type = ANTLRv4Lexer.TOKEN_REF;
                }
                else
                {
                    Type = ANTLRv4Lexer.RULE_REF;
                }

                if (_currentRuleType == TokenConstants.InvalidType)
                { // if outside of rule def
                    _currentRuleType = Type; // set to inside lexer or parser rule
                }
            }
            else if (Type == ANTLRv4Lexer.SEMI)
            { // exit rule def
                _currentRuleType = TokenConstants.InvalidType;
            }

            return base.NextToken();
        }

        private bool inLexerRule()
        {
            return _currentRuleType == ANTLRv4Lexer.TOKEN_REF;
        }


        private bool inParserRule()
        { // not used, but added for clarity
            return _currentRuleType == ANTLRv4Lexer.RULE_REF;
        }
    }
}