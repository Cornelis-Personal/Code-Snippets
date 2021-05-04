using Marel.LairageScanner.API.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Marel.LairageScanner.API.Services.BackgroundServices.Models
{
    public class AdamListener
    {
        protected string hostname;
        protected int port;
        protected int _timeout_milliseconds;
        protected TcpClient connection;
        protected bool connected;
        protected Exception exception;
        private ConcurrentDictionary<string, TcpClient> _liveConnections { get; }

        public AdamListener(string host, int port, int timeout_milliseconds, ConcurrentDictionary<string, TcpClient> liveConnections)
        {
            hostname = host;
            this.port = port;
            _timeout_milliseconds = timeout_milliseconds;
            _liveConnections = liveConnections;
        }

        public async Task Run(CancellationToken cancellationToken, IServiceScopeFactory serviceProvider)
        {
            if (_liveConnections.ContainsKey(hostname))
            {
                if (_liveConnections[hostname].Connected)
                    return;

                connection = _liveConnections[hostname];
            }
            else
            {
                // Create a new client for it
                connection = new TcpClient();
            }

            try
            {
                // Try to connection
                await connection.ConnectAsync(hostname, port);

                if (connection.Client.Connected && !_liveConnections.ContainsKey(hostname))
                    _liveConnections.TryAdd(hostname, connection);

                // Read the data
                using (var netstream = connection.GetStream())
                using (var reader = new StreamReader(netstream))
                {
                    // Optionally set a timeout
                    netstream.ReadTimeout = _timeout_milliseconds;

                    while (connection.Connected)
                    {
                        // Read server response
                        string response = await reader.ReadLineAsync();
                        if (!string.IsNullOrEmpty(response))
                        {
                            using (var scope = serviceProvider.CreateScope())
                            {
                                var commsService = scope.ServiceProvider.GetService<IScannerCommunicationService>();
                                await commsService.ScanAsync(hostname, port, response);
                            }
                        }

                        // Sleep the thread
                        Thread.Sleep(10);
                    }

                    _liveConnections.TryRemove(new KeyValuePair<string, TcpClient>(hostname, connection));

                    using (var scope = serviceProvider.CreateScope())
                    {
                        var commsService = scope.ServiceProvider.GetService<ISignalRService>();
                        await commsService.SendErrorNotificationAsync($"Scanner {hostname} has been disconnected.");
                    }
                }
            }
            catch (Exception e)
            {
                if (_liveConnections.ContainsKey(hostname))
                    _liveConnections.TryRemove(new KeyValuePair<string, TcpClient>(hostname, connection));

                using (var scope = serviceProvider.CreateScope())
                {
                    var commsService = scope.ServiceProvider.GetService<ISignalRService>();
                    await commsService.SendErrorNotificationAsync($"Scanner {hostname} has a issue with connectivity.");
                }
            }
        }
    }
}