using System;

namespace ChemistryGame.EditorTools.ContentEditor
{
    public enum Severity { Error, Warning, Info }

    /// <summary>One validation finding produced by Validator.</summary>
    public struct ValidationIssue
    {
        public Severity Severity;
        public string Code;           // e.g. "SUB_ID_EMPTY"
        public string Message;        // user-facing Vietnamese
        public Action QuickFix;       // null = no auto-fix
        public string QuickFixLabel;

        public static ValidationIssue Error(string code, string msg, Action quickFix = null, string fixLabel = null)
            => new ValidationIssue { Severity = Severity.Error, Code = code, Message = msg, QuickFix = quickFix, QuickFixLabel = fixLabel };

        public static ValidationIssue Warning(string code, string msg, Action quickFix = null, string fixLabel = null)
            => new ValidationIssue { Severity = Severity.Warning, Code = code, Message = msg, QuickFix = quickFix, QuickFixLabel = fixLabel };

        public static ValidationIssue Info(string code, string msg)
            => new ValidationIssue { Severity = Severity.Info, Code = code, Message = msg };
    }
}
