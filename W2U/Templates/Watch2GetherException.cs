using System;

namespace W2U.Templates
{
    public class Watch2GetherException : Exception
    {
        public override string ToString()
        {
            return Message;
        }

        public override string Message { get; }

        public Watch2GetherException()
        {
            this.Message = "An Exception was thrown by a Watch2Gether class";
        }

        public Watch2GetherException(string message)
        {
            this.Message = message;
        }
    }
}