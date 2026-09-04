namespace SOLTIUS_Web_API_Add_On.Exceptions
{
    public class ApiNotConfiguredException : Exception
    {
        public ApiNotConfiguredException() : base("Web API is not configured.")
        {
        }

        public ApiNotConfiguredException(string message) : base(message)
        {
        }
    }
}