namespace OpeicoCompiler.Lexer
{
    public enum KeyWordsEnum
    {
        PubC,
        PrivC,
        InterC,
        ProcC,
        DOMAIN,
        DOMAIN2,
        DOMAIN3,
        RUN,
        Kind,
        IMP,
        ConsoleWriteLine,
        Try,
        Nxt,
        Brk,
        Blck,
        Iff,
        Elss,
        Swch,
        Cs,
        Dflt,
        Whl,
        DWhl,
        For,
        Excp,
        Catch,
        Finally,
        Ret,
        This,
        Base,
        Nl,
        Tru,
        Fls,
        New,
        As,
        Is,
        SizeOf,
        TypeOf,
        abs,
        as_,
        Base2,
        bl,
        split,
        bplus
    };

    public class KeyWords
    {
        public const string PubC = "PubC";  // Public class
        public const string PrivC = "PrivC";  // Private class
        public const string InterC = "InterC";  // Internal class
        public const string ProcC = "ProcC";  // Protected class
        public const string DOMAIN = "DOMAIN";  // Public void
        public const string DOMAIN2 = "DOMAIN2";  // Additional methods with numbers
        public const string DOMAIN3 = "DOMAIN3";  // Additional methods with numbers
        public const string RUN = "RUN";  // Variable declaration and initialization
        public const string Kind = "Kind";  // Data type declaration
        public const string IMP = "IMP";  // Using NamespaceName
        public const string ConsoleWriteLine = "?{\"Text\"}@";  // Console.WriteLine
        public const string Try = "$t_";  // try { ... } catch { ... } finally { ... }
        public const string Nxt = "Nxt";  // Next loop iteration
        public const string Brk = "Brk";  // Break loop
        public const string Blck = "Blck";  // Define a block
        public const string Iff = "Iff";  // if statement
        public const string Elss = "Elss";  // else statement
        public const string Swch = "Swch";  // switch statement
        public const string Cs = "Cs";  // case statement
        public const string Dflt = "Dflt";  // default statement
        public const string Whl = "Whl";  // while loop
        public const string DWhl = "DWhl";  // do-while loop
        public const string For = "For";  // for loop
        public const string Excp = "Excp";  // try-catch block
        public const string Catch = "$c_";  // catch block
        public const string Finally = "$f_";  // finally block
        public const string Ret = "Ret";  // return statement
        public const string This = "This";  // this keyword
        public const string Base = "Base";  // base keyword
        public const string Nl = "Nl";  // null keyword
        public const string Tru = "Tru";  // true keyword
        public const string Fls = "Fls";  // false keyword
        public const string New = "New";  // new keyword
        public const string As = "As";  // as keyword
        public const string Is = "Is";  // is keyword
        public const string SizeOf = "SizeOf";  // sizeof keyword
        public const string TypeOf = "TypeOf";  // typeof keyword
        public const string abs = "!abs~";  // abstract
        public const string as_ = "^as";  // as
        public const string Base2 = "$Base";  // base
        public const string bl = "?bl>";  // bool
        public const string split = "?split";  // break
        public const string bplus = "b+";  // byte
    }
 public enum KeyWordsEnum
    {
        PubC,
        PrivC,
        InterC,
        ProcC,
        DOMAIN,
        DOMAIN2,
        DOMAIN3,
        RUN,
        Kind,
        IMP,
        ConsoleWriteLine,
        Try,
        Nxt,
        Brk,
        Blck,
        Iff,
        Elss,
        Swch,
        Cs,
        Dflt,
        Whl,
        DWhl,
        For,
        Excp,
        Catch,
        Finally,
        Ret,
        This,
        Base,
        Nl,
        Tru,
        Fls,
        New,
        As,
        Is,
        SizeOf,
        TypeOf,
        abs,
        as_,
        Base2,
        bl,
        split,
        bplus
    };
}
