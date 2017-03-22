using System.ServiceModel;

namespace ServerInterfaces
{
    /// <summary>
    /// interface for callback function of client
    /// </summary>
    [ServiceContract]
    public interface IServerServiceCallback
    {
        [OperationContract(IsOneWay = true)]
        void SendCallback(Event e);
    }
}