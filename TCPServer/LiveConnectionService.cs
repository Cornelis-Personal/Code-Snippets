 public class LiveConnectionService : ILiveConnectionService
    {
        /// <summary>
        /// Connect to a tcp client
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public async Task ConnectAsync(string host, int port, IServiceScopeFactory serviceProvider, CancellationToken cancellationToken)
        {
            Thread t = new Thread(DoWork);

            t.Start((host, port, serviceProvider, cancellationToken));
        }

        public static async void DoWork(object? data)
        {
            var x = ((string host, int port, IServiceScopeFactory serviceProvider, CancellationToken cancellationToken))data;

            var listener = new AdamListener(x.host, x.port, 5000, GlobalVariables.LiveConnections);
            await listener.Run(x.cancellationToken, x.serviceProvider);
        }

        public List<string> GetActiveConnections()
        {
            return GlobalVariables.LiveConnections.Select(x => x.Key).ToList();
        }
    }