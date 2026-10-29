public interface ISmsService
{
    Task SendAsync(string phone, string message);
}
