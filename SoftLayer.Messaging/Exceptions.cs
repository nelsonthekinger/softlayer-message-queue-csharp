using System;

namespace SoftLayer.Messaging
{
    public class MessagingException : Exception
    {
        public MessagingException()
            : base()
        {
        }

        public MessagingException(string message)
            : base(message)
        {
        }
    }

    public class BadValueException : MessagingException
    {
        public BadValueException() : base() { }
        public BadValueException(string message) : base(message) { }
    }

    public class TooManyItemsException : MessagingException
    {
        public TooManyItemsException() : base() { }
        public TooManyItemsException(string message) : base(message) { }
    }

    public class InvalidDatacenterException : Exception
    {
        public InvalidDatacenterException() : base() { }
        public InvalidDatacenterException(string message, Exception innerException = null) : base(message, innerException) { }
    }

    public class NoSuitableEndpointException : MessagingException
    {
        public NoSuitableEndpointException() : base() { }
        public NoSuitableEndpointException(string message) : base(message) { }
    }

    /**
     * General HTTP-level exceptions
     */
    public class ServerUnreachableException : MessagingException
    {
        public ServerUnreachableException() : base() { }
        public ServerUnreachableException(string message) : base(message) { }
    }

    public class UnexpectedResponseException : MessagingException
    {
        public UnexpectedResponseException() : base() { }
        public UnexpectedResponseException(string message) : base(message) { }
    }

    public class UnauthorizedException : MessagingException
    {
        public UnauthorizedException() : base() { }
        public UnauthorizedException(string message) : base(message) { }
    }

    public class BadRequestException : MessagingException
    {
        public BadRequestException() : base() { }
        public BadRequestException(string message) : base(message) { }
    }

    public class NotFoundException : MessagingException
    {
        public NotFoundException() : base() { }
        public NotFoundException(string message) : base(message) { }
    }

    public class ServiceUnavailableException : MessagingException
    {
        public ServiceUnavailableException() : base() { }
        public ServiceUnavailableException(string message) : base(message) { }
    }

    public class ServerErrorException : MessagingException
    {
        public ServerErrorException() : base() { }
        public ServerErrorException(string message) : base(message) { }
    }

    /**
     * Service-level exceptions
     */
    public class InvalidTokenException : MessagingException
    {
        public InvalidTokenException() : base() { }
        public InvalidTokenException(string message) : base(message) { }
    }

    public class TokenInvalidOrExpiredException : MessagingException
    {
        public TokenInvalidOrExpiredException() : base() { }
        public TokenInvalidOrExpiredException(string message) : base(message) { }
    }

    public class QueueNotFoundException : MessagingException
    {
        public QueueNotFoundException() : base() { }
        public QueueNotFoundException(string message) : base(message) { }
    }

    public class QueueNotEmptyException : MessagingException
    {
        public QueueNotEmptyException() : base() { }
        public QueueNotEmptyException(string message) : base(message) { }
    }

    public class TopicHasSubscriptionsException : MessagingException
    {
        public TopicHasSubscriptionsException() : base() { }
        public TopicHasSubscriptionsException(string message) : base(message) { }
    }

    public class TopicNotFoundException : MessagingException
    {
        public TopicNotFoundException() : base() { }
        public TopicNotFoundException(string message) : base(message) { }
    }

    public class TopicSubscriptionNotFoundException : MessagingException
    {
        public TopicSubscriptionNotFoundException() : base() { }
        public TopicSubscriptionNotFoundException(string message) : base(message) { }
    }

    public class MessageNotFoundException : MessagingException
    {
        public MessageNotFoundException() : base() { }
        public MessageNotFoundException(string message) : base(message) { }
    }
}