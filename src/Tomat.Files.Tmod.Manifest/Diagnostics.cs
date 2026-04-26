namespace Tomat.Files.Tmod.Manifest;

public enum DiagnosticCode : ushort
{
    // Syntax
    UnexpectedToken = 1000,
    ExpectedEquals = 1001,
    ExpectedValue = 1002,

    // Semantic
    DuplicateProperty = 2000,
    UnknownProperty = 2001,

    // Types
    InvalidBoolean = 3000,
    NonCanonicalBoolean = 3001,

    InvalidModSide = 3002,
    NonCanonicalModSide = 3003,

    InvalidVersion = 3004,
    TooManyVersionParts = 3005,
}

public enum DiagnosticSeverity : byte
{
    Info,
    Warning,
    Error,
}

public struct Diagnostic
{
    public DiagnosticCode Code { get; set; }
    
    public DiagnosticSeverity Severity { get; set; }
    
    public int Position { get; set; }
    
    public string Message { get; set; }
    
    public string? Hint { get; set; }
}
