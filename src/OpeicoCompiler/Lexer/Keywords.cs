namespace OpeicoCompiler.Lexer
{
    public enum KeyWordsEnum
    {
        Array,
        Break,
        Do,
        Typeof,
        Case,
        Else,
        New,
        Var,
        Catch,
        Finally,
        Return,
        Void,
        Continue,
        For,
        Switch,
        While,
        Func,
        This,
        Default,
        If,
        Throw,
        In,
        Try,
        Int,
        Float,
        Double,
        Long,
        Boolean,
        String,
    };
    public class KeyWords
    {
        public const string ARRAY = "List";
        public const string BREAK = "?split";
        public const string DO = "DWhl";
        public const string TYPEOF = "TypeOf";
        public const string CASE = "Cs";
        public const string ELSE = "Elss";
        public const string NEW = "New";
        public const string VAR = "RUN";
        public const string CATCH = "$c_";
        public const string FINALLY = "$f_";
        public const string RETURN = "Ret";
        public const string VOID = "DOMAIN";
        public const string CONTINUE = "Nxt";
        public const string FOR = "FOR";
        public const string SWITCH = "Swch";
        public const string WHILE = "Whl";
        public const string FUNC = "DOMAIN";
        public const string THIS = "This";
        public const string DEFAULT = "Dflt";
        public const string IF = "Iff";
        public const string THROW = "Throw";
        public const string IN = "IN";
        public const string TRY = "$t_";
        public const string INT = "int";
        public const string FLOAT = "float";
        public const string DOUBLE = "double";
        public const string LONG = "long";
        public const string BOOLEAN = "?bl>";
        public const string STRING = "string";

        public const string LPAREN = "(";
        public const string RPAREN = ")";



        public static List<string> keyWordNames = new()
{
    {"List"},
    {"?split"},
    {"DWhl"},
    {"TypeOf"},
    {"Cs"},
    {"Elss"},
    {"New"},
    {"RUN"},
    {"$c_"},
    {"$f_"},
    {"Ret"},
    {"DOMAIN"},
    {"Nxt"},
    {"FOR"},
    {"Swch"},
    {"Whl"},
    {"debugger"},
    {"DOMAIN"},
    {"This"},
    {"with"},
    {"Dflt"},
    {"Iff"},
    {"Throw"},
    {"IN"},
    {"$t_"},
    {"int"},
    {"float"},
    {"double"},
    {"long"},
    {"?bl>"},
    {"string"}
};


        public static Dictionary<string, KeyWordsEnum> keyValuePairs = new()
{
    { "List",       KeyWordsEnum.Array },
    { "?split",     KeyWordsEnum.Break },
    { "DWhl",       KeyWordsEnum.Do },
    { "TypeOf",     KeyWordsEnum.Typeof },
    { "Cs",         KeyWordsEnum.Case },
    { "Elss",       KeyWordsEnum.Else },
    { "New",        KeyWordsEnum.New },
    { "RUN",        KeyWordsEnum.Var },
    { "$c_",        KeyWordsEnum.Catch },
    { "$f_",        KeyWordsEnum.Finally },
    { "Ret",        KeyWordsEnum.Return },
    { "DOMAIN",     KeyWordsEnum.Void },
    { "Nxt",        KeyWordsEnum.Continue },
    { "FOR",        KeyWordsEnum.For },
    { "Swch",       KeyWordsEnum.Switch },
    { "Whl",        KeyWordsEnum.While },
    { "DOMAIN",     KeyWordsEnum.Func },
    { "This",       KeyWordsEnum.This },
    { "Dflt",       KeyWordsEnum.Default },
    { "Iff",        KeyWordsEnum.If },
    { "Throw",      KeyWordsEnum.Throw },
    { "IN",         KeyWordsEnum.In },
    { "$t_",        KeyWordsEnum.Try },
    { "int",        KeyWordsEnum.Int },
    { "float",      KeyWordsEnum.Float },
    { "double",     KeyWordsEnum.Double },
    { "long",       KeyWordsEnum.Long },
    { "?bl>",       KeyWordsEnum.Boolean },
    { "string",     KeyWordsEnum.String }
};

    }
}
