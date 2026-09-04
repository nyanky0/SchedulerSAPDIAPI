using SOLTIUS_Web_API_Add_On.Exceptions;

namespace SOLTIUS_Web_API_Add_On.Exceptions
{
    /// <summary>
    /// Placeholder for database initialization failures.
    /// </summary>
    public class DatabaseInitializationException : Exception
    {
        public DatabaseInitializationException() : base() { }
        public DatabaseInitializationException(string message) : base(message) { }
        public DatabaseInitializationException(string message, Exception inner) : base(message, inner) { }
    }
}
