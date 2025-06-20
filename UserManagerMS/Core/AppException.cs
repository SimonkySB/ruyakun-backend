namespace UserManagerMS.Core;

public class AppException : BadHttpRequestException
{
    
    public AppException(string message) : base(message)
    {
    }
    
}