using System;
using System.ServiceModel;

namespace ServerInterfaces
{
    /// <summary>
    /// interface that define server functions
    /// </summary>
    [ServiceContract(CallbackContract = typeof(IServerServiceCallback))]
    public interface IServerService
    {
        [OperationContract(IsOneWay = true)]
        void Connect(string name);

        [OperationContract(IsOneWay = true)]
        void Disconnect();

        [OperationContract(IsOneWay = true)]
        void NewGame(string player, string opponent);

        [OperationContract(IsOneWay = true)]
        void AskForGame(string player, string oponent);

        [OperationContract(IsOneWay = true)]
        void FieldChoose(string playerName, Guid gameId, int x, int y);

        [OperationContract(IsOneWay = true)]
        void GetGameHistory(Guid guid, string player);

        [OperationContract(IsOneWay = true)]
        void SaveGame(Guid gameId, string player);

        [OperationContract(IsOneWay = true)]
        void LoadGame(Guid guid);

        [OperationContract(IsOneWay = true)]
        void GetSaves(string playerName);

        [OperationContract(IsOneWay = true)]
        void GetPlayers(string player);
    }
}