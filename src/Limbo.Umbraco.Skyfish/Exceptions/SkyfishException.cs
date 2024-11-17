using System;

namespace Limbo.Umbraco.Skyfish.Exceptions;

public class SkyfishException : Exception {

    public SkyfishException() : base("An error occured on the server.") { }

    public SkyfishException(Exception? innerException) : base("An error occured on the server.", innerException) { }

    public SkyfishException(string message) : base(message) { }

    public SkyfishException(string message, Exception? innerException) : base(message, innerException) { }

}