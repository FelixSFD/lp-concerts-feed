namespace Server.Api.ExceptionHandling;

public class BadRequestException(string message) : Exception(message)
{
    
}