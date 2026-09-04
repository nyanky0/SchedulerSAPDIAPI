namespace SOLTIUS_Web_API_Add_On.Exceptions
{
    /// <summary>
    /// Config.xml ada tetapi isinya tidak valid (node ExternalDatabase hilang,
    /// DatabaseType salah, dsb). Dipetakan ke HTTP 400 oleh GlobalExceptionMiddleware.
    /// </summary>
    public class ApiConfigInvalidException : Exception
    {
        public ApiConfigInvalidException(string message)
            : base(message)
        {
        }
    }
}
