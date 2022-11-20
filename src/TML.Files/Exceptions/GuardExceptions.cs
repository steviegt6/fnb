using System;
using System.IO;
using System.Runtime.Serialization;

namespace TML.Files.Exceptions;

/// <summary>
///     The exception that is thrown when an attempt to access a file that does not exist on disk fails in a <c>TML.Files</c> callsite. 
/// </summary>
[Serializable]
public class TModFileNotFoundException : FileNotFoundException
{
    public TModFileNotFoundException() { }
    public TModFileNotFoundException(string message) : base(message) { }
    public TModFileNotFoundException(string message, Exception inner) : base(message, inner) { }

    protected TModFileNotFoundException(
        SerializationInfo info,
        StreamingContext context
    ) : base(info, context) { }
}

/// <summary>
///     The exception that is thrown when a <see cref="TModFile"/> has an invalid header.
/// </summary>
[Serializable]
public class TModFileInvalidHeaderException : IOException
{
    public TModFileInvalidHeaderException() { }
    public TModFileInvalidHeaderException(string message) : base(message) { }
    public TModFileInvalidHeaderException(string message, Exception inner) : base(message, inner) { }

    protected TModFileInvalidHeaderException(
        SerializationInfo info,
        StreamingContext context
    ) : base(info, context) { }
}

/// <summary>
///     The exception that is thrown when a <see cref="TModFileEntry"/> is invalid in some form.
/// </summary>
[Serializable]
public class TModFileInvalidFileEntryException : IOException
{
    public TModFileInvalidFileEntryException() { }
    public TModFileInvalidFileEntryException(string message) : base(message) { }
    public TModFileInvalidFileEntryException(string message, Exception inner) : base(message, inner) { }

    protected TModFileInvalidFileEntryException(
        SerializationInfo info,
        StreamingContext context
    ) : base(info, context) { }
}

/// <summary>
///     The exception that is thrown when a directory already exists in a <c>TML.Files</c> callsite.
/// </summary>
[Serializable]
public class TModFileDirectoryAlreadyExistsException : IOException
{
    public TModFileDirectoryAlreadyExistsException() { }
    public TModFileDirectoryAlreadyExistsException(string message) : base(message) { }
    public TModFileDirectoryAlreadyExistsException(string message, Exception inner) : base(message, inner) { }

    protected TModFileDirectoryAlreadyExistsException(
        SerializationInfo info,
        StreamingContext context
    ) : base(info, context) { }
}

/// <summary>
///     The exception that is thrown when part of a file or directory cannot be found in a <c>TML.Files</c> callsite.
/// </summary>
[Serializable]
public class TModFileDirectoryNotFoundException : DirectoryNotFoundException
{
    public TModFileDirectoryNotFoundException() { }
    public TModFileDirectoryNotFoundException(string message) : base(message) { }
    public TModFileDirectoryNotFoundException(string message, Exception inner) : base(message, inner) { }

    protected TModFileDirectoryNotFoundException(
        SerializationInfo info,
        StreamingContext context
    ) : base(info, context) { }
}