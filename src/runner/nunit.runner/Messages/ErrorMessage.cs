

namespace NUnit.Runner.Messages
{
    /// <summary>
    /// Represents an error message
    /// </summary>
	public class ErrorMessage
	{
        /// <summary>
        /// The name of this message
        /// </summary>
        public const string Name = nameof(ErrorMessage);

        /// <summary>
        /// Constructs an <see cref="ErrorMessage"/> with a message
        /// </summary>
        /// <param name="message"></param>
        public ErrorMessage(string message)
        {
            Message = message;
        }

        /// <summary>
        /// The error message
        /// </summary>
        public string Message { get; set; }
    }
}
