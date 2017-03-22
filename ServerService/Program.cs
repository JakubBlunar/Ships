using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using ServerInterfaces;

namespace ServerService
{
    internal static class Program
    {
        /// <summary>
        /// Entry point of server, creates service and start server.
        /// </summary>
        private static void Main()
        {
            Console.Title = "Server";

            var baseAddress = new Uri("http://localhost:7777/Ships/");
            var service = new ServiceHost(typeof(Server), baseAddress);
            try
            {
                service.AddServiceEndpoint(typeof(IServerService),
                    new WSDualHttpBinding(WSDualHttpSecurityMode.None), "ServerService");

                var smb = new ServiceMetadataBehavior {HttpGetEnabled = true};
                service.Description.Behaviors.Add(smb);
                service.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName,
                    MetadataExchangeBindings.CreateMexHttpBinding(), baseAddress + "mex");

                service.Open();
                Console.WriteLine("The server is running.");
                Console.WriteLine("Listening at: {0}", baseAddress);
                Console.WriteLine("Press key to exit.");
                Console.ReadLine();
                service.Close();
            }
            catch (CommunicationException e)
            {
                Console.WriteLine("Exception : {0}", e.Message);
                service.Abort();
            }
        }
    }
}