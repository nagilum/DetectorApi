using System;

namespace DetectorApi.Exceptions
{
    public class NotFoundResponseException : Exception
    {
        public NotFoundResponseException()
        {
        }

        public NotFoundResponseException(string message) : base(message)
        {
        }

        public NotFoundResponseException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}