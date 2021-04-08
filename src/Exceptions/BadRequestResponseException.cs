using System;

namespace DetectorApi.Exceptions
{
    public class BadRequestResponseException : Exception
    {
        public BadRequestResponseException()
        {
        }

        public BadRequestResponseException(string message) : base(message)
        {
        }

        public BadRequestResponseException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}