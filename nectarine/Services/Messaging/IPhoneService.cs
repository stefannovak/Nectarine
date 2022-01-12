namespace nectarineAPI.Services.Messaging;

public interface IPhoneService
{
    public void SendMessage(string message, string destinationNumber);
}