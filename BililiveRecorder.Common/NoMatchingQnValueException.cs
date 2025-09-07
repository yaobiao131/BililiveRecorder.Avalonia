namespace BililiveRecorder.Common
{
    public class NoMatchingQnValueException : Exception
    {
        public NoMatchingQnValueException() { }
        public NoMatchingQnValueException(string message) : base(message) { }
        public NoMatchingQnValueException(string message, Exception innerException) : base(message, innerException) { }
    }
}
