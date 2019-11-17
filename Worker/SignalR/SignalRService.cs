using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Protocol;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Worker.Host.Models;
using Worker.Models;

namespace Worker.Host.SignalR
{
    public class SignalRService : BackgroundService
    {
        private readonly string Domain = "https://dev-3ru57p69.eu.auth0.com/";
        private readonly string Audience = "https://localhost:5001";
        private readonly string Secret = "idcNqrPsARQFI5qeEKOn57SwsloVN-ln1bo-R7aTo_ZTWtnEv2BGAkbuTvm7hq8J";
        private readonly string ClientId = "DYaPShg0nOEptG3AIeDgNBCudk7w3LhI";
        private readonly string ConnectionUri = "https://aa1c1efb.ngrok.io/hubs/controllerhub";

        private string token = null;
        private SecurityToken validatedToken = null;

        private readonly IConfigurationManager<OpenIdConnectConfiguration> configurationManager;
        private readonly OpenIdConnectConfiguration openIdConfig;
        private readonly TokenValidationParameters validationParameters;
        private readonly HubConnection connection;
        //private  HubConnection Connection { get => connection;  set { return; } }

        private readonly InputMessageQueue inputQueue;
        private readonly OutputMessageQueue outputQueue;

        public SignalRService(IOptionsMonitor<SignalROptions> options, InputMessageQueue inputQueue, OutputMessageQueue outputQueue)
        {
            this.inputQueue = inputQueue;
            this.outputQueue = outputQueue;

            if(!(options is null))
            {
                Domain = options.CurrentValue.AuthDomain;
                Audience = options.CurrentValue.Audience;
                Secret = options.CurrentValue.Secret;
                ClientId = options.CurrentValue.Id;
                ConnectionUri = options.CurrentValue.HubUri;
            }

            configurationManager =
                new ConfigurationManager<OpenIdConnectConfiguration>
                    ($"{Domain}.well-known/openid-configuration",
                    new OpenIdConnectConfigurationRetriever());
            openIdConfig = configurationManager.GetConfigurationAsync(CancellationToken.None).Result;
            validationParameters =
                new TokenValidationParameters
                {
                    ValidIssuer = Domain,
                    ValidAudiences = new[] { Audience },
                    IssuerSigningKeys = openIdConfig.SigningKeys,
                    ValidateLifetime = true
                };
            connection = new HubConnectionBuilder()
               .WithUrl(ConnectionUri, options =>
               {
                   options.AccessTokenProvider =  () => Task.FromResult(GetAccessToken());
                   // options.
               })
               .WithAutomaticReconnect()
               .Build();
            connection.Closed += async (error) => await ConnectionClosed(error).ConfigureAwait(false);
            connection.Reconnected += connectionId => ConnectionReconnected(connectionId);
            connection.Reconnecting += error => ConnectionReconnecting(error);

            RegisterSignalRListeners();
            foreach(var item in outputQueue.Dictionary)
            {
                item.Value.EnqueueEvent += OnSignalRresponse;
            }
            
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine($"SignalRService is starting.");

            stoppingToken.Register(() =>
                Console.WriteLine($" SignalRService background task is stopping."));

            if (!stoppingToken.IsCancellationRequested)
            {
                Console.WriteLine($"SignalRService task doing background work.");

                await ConnectWithRetryAsync(connection, stoppingToken).ConfigureAwait(false);
            }

           // Console.WriteLine($"SignalRService background task is stopping.");
        }

        private async Task ConnectionClosed(Exception error)
        {
            await Task.Delay(new Random().Next(0, 5) * 1000).ConfigureAwait(false);
            await connection.StartAsync().ConfigureAwait(false);
        }

        private Task ConnectionReconnected(string connectionId)
        {
            Debug.Assert(connection.State == HubConnectionState.Connected);
            // Notify users the connection was reestablished.
            // Start dequeuing messages queued while reconnecting if any.
            Console.WriteLine("SignalR Reconnected");
            return Task.CompletedTask;
        }

        private Task ConnectionReconnecting(Exception error)
        {
            Debug.Assert(connection.State == HubConnectionState.Reconnecting);
            // Notify users the connection was lost and the client is reconnecting.
            // Start queuing or dropping messages.
            Console.WriteLine("Reconnecting");
            return Task.CompletedTask;
        }

        public string GetAccessToken()
        {
            Console.WriteLine($"Token");
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            if (validatedToken != null & token != null)
            {
                Console.WriteLine($"static {validatedToken.ToString()}");
                var user = handler.ValidateToken(token, validationParameters, out validatedToken);
                return token;
            }
            else
            {
                // Console.WriteLine($"2");
                var client = new RestClient($"{Domain}oauth/token");
                var request = new RestRequest(RestSharp.Method.POST);
                request.AddHeader("content-type", "application/x-www-form-urlencoded");
                var apiId = HttpUtility.UrlEncode(Audience);
                request.AddParameter("application/x-www-form-urlencoded", $"grant_type=client_credentials&client_id={ClientId}&client_secret={Secret}&audience={apiId}", ParameterType.RequestBody);
                var response = client.Execute<Auth0Response>(request);
                Console.WriteLine($"{response.Data.AccessToken}");
                var user = handler.ValidateToken(response.Data.AccessToken, validationParameters, out validatedToken);
                token = response.Data.AccessToken;
                return response.Data.AccessToken;
            }
        }

        public static async Task<bool> ConnectWithRetryAsync(HubConnection connection, CancellationToken token)
        {
            // Keep trying to until we can start or the token is canceled.
            while (true)
            {
                try
                {
                    await connection.StartAsync(token).ConfigureAwait(false);
                    Debug.Assert(connection.State == HubConnectionState.Connected);
                    Console.WriteLine("Connected");
                    return true;
                }
                catch when (token.IsCancellationRequested)
                {
                    return false;
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.ToString());
                    // Failed to connect, trying again in 5000 ms.
                    Debug.Assert(connection.State == HubConnectionState.Disconnected);
                    Console.WriteLine("Trying to connect");
                    await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
                }
            }
        }


        public async void OnSignalRresponse(object sender, MessageQueueEnqueueEventArgs<SignalRresponse> args)
        {
            var flag = outputQueue.Dictionary.TryGetValue((string)args.Port, out ConcurrentMessageBag<SignalRresponse> queue);

            var item = args.Item;
            _= queue.TryDequeue(out SignalRresponse t);//govno
            if (flag == true && !(queue is null))
            {
                switch (item.Method)
                {
                    case SignalRMethod.GetConfig:
                        Console.WriteLine("OnSignalRresponse GetConfig");
                        await connection.InvokeAsync("SendConfigResTo", (string)item.Address, (string)item.JsonString, (string)item.Port);
                        break;
                    default:
                        Console.WriteLine("OnSignalRresponse default");
                        break;
                }
            }
        }

        protected void RegisterSignalRListeners()
        {
            connection.On("GetConfig", (Action<GetConfigReq>)(req =>
            {
                if (req.Port is null)
                {
                    Console.WriteLine("req.Port is null or not exist");
                    return;
                }
                Console.WriteLine($"req.Port is '{req.Port}'");
                var flag = inputQueue.Dictionary.TryGetValue(req.Port, out ConcurrentMessageBag<SignalRMessage> queue);

                if (flag == true && !(queue is null))
                {
                    queue.Enqueue(new SignalRMessage { Port = req.Port, Method = SignalRMethod.GetConfig, Address = req.Address });
                }
            }));

            connection.On("GetFingerTimeoutCurrent", (Action<GetFingerTimeoutReq>)(req =>
            {
                Console.WriteLine("SignalR GetFingerTimeoutCurrent HIT!");

                if (req.Port is null)
                {
                    Console.WriteLine("req.Port is null or not exist");
                    return;
                }
                Console.WriteLine($"req.Port is '{req.Port}'");
                var flag = inputQueue.Dictionary.TryGetValue(req.Port, out ConcurrentMessageBag<SignalRMessage> queue);

                if (flag == true && !(queue is null))
                {
                    queue.Enqueue(new SignalRMessage { Port = req.Port, Method = SignalRMethod.GetFingerTimeoutCurrent });
                }

            }));

            connection.On("AddFinger", (Action<AddFingerReq>)(req =>
            {
                Console.WriteLine("SignalR AddFinger HIT!");

                if (req.Uid == 0)
                {
                    Console.WriteLine("req.Uid is null or 0");
                    return;
                }
                if (req.Privilage == 0)
                {
                    Console.WriteLine("req.Privilage is null or 0");
                    return;
                }
                if (req.Port is null)
                {
                    Console.WriteLine("req.Port is null or not exist");
                    return;
                }
                Console.WriteLine($"req.Port is '{(string)req.Port}'");
                Console.WriteLine($"uid: '{req.Uid}', privilage: '{req.Privilage}' port: '{req.Port}'");

                var flag = inputQueue.Dictionary.TryGetValue(req.Port, out ConcurrentMessageBag<SignalRMessage> queue);

                if (flag == true && !(queue is null))
                {
                    queue.Enqueue(new SignalRMessage { Port = req.Port, Method = SignalRMethod.AddFinger, Uid = req.Uid, Privilage = req.Privilage });
                }

            }));

            connection.On("SendConfig", (Action<SendConfigReq>)(req =>
            {
                Console.WriteLine("SignalR SendConfig HIT!");

                if (req.JsonString is null)
                {
                    Console.WriteLine("req.JsonString is null or not exist");
                    return;
                }
                if (req.Port is null)
                {
                    Console.WriteLine("req.Port is null or not exist");
                    return;
                }

                var flag = inputQueue.Dictionary.TryGetValue(req.Port, out ConcurrentMessageBag<SignalRMessage> queue);

                if (flag == true && !(queue is null))
                {
                    queue.Enqueue(new SignalRMessage { Port = req.Port, Method = SignalRMethod.SendConfig, JsonString = req.JsonString });
                }

            }));

            connection.On("DeleteFingerById", (Action<DeleteFingerByIdReq>)(req =>
            {
                Console.WriteLine("SignalR DeleteFingerById HIT!");
                if (req.Port is null)
                {
                    Console.WriteLine("req.Port is null or not exist");
                    return;
                }
                if (req.Id == 0)
                {
                    Console.WriteLine("req.Id is null or 0");
                    return;
                }

                var flag = inputQueue.Dictionary.TryGetValue(req.Port, out ConcurrentMessageBag<SignalRMessage> queue);

                if (flag == true && !(queue is null))
                {
                    queue.Enqueue((SignalRMessage)new SignalRMessage { Port = req.Port, Method = SignalRMethod.DeleteFingerById, Uid = req.Id });
                }

            }));

            connection.On("AddFingerByBle", (Action<AddFingerByBleReq>)(req =>
            {
                Console.WriteLine("SignalR AddFingerByBle HIT!");
                if (req.Port is null)
                {
                    Console.WriteLine("req.Port is null or not exist");
                    return;
                }
                if (req.UserId is null)
                {
                    Console.WriteLine("req.UserId is null or not exist");
                    return;
                }
                if (req.Ble is null)
                {
                    Console.WriteLine("req.Ble is null or not exist");
                    return;
                }
                if (req.Id == 0)
                {
                    Console.WriteLine("req.Id is null or 0");
                    return;
                }
                if (req.Privilage == 0)
                {
                    Console.WriteLine("req.Privilage is null or 0");
                    return;
                }

                var flag = inputQueue.Dictionary.TryGetValue(req.Port, out ConcurrentMessageBag<SignalRMessage> queue);

                if (flag == true && !(queue is null))
                {
                    queue.Enqueue(new SignalRMessage
                    {
                        Port = req.Port,
                        Method = SignalRMethod.AddFingerByBle,
                        Uid = req.Id,
                        UserId = req.UserId,
                        BleString = req.Ble,
                        Privilage = req.Privilage
                    });
                }
            }));

            connection.On("SetFingerTimeout", (Action<SetFingerTimeoutReq>)(req =>
            {
                Console.WriteLine("SignalR SetFingerTimeout HIT!");
                if (req.Port is null)
                {
                    Console.WriteLine("req.Port is null or not exist");
                    return;
                }

                //if(req.Timeout )
                //{
                //    Console.WriteLine("req.Timeout is null or not exist");
                //    return;
                //}

                inputQueue.Dictionary.TryGetValue(req.Port, out ConcurrentMessageBag<SignalRMessage> queue);

                if (!(queue is null))
                {
                    queue.Enqueue(new SignalRMessage
                    {
                        Method = SignalRMethod.SetFingerTimeout,
                        Port = req.Port,
                        Timeout = req.Timeout
                    });
                }
            }));

            connection.On("DeleteAllFingerprints", (Action<DeleteAllFingerprintsReq>)(req =>
            {
                Console.WriteLine("SignalR DeleteAllFingerprints HIT!");
                if (req.Port is null)
                {
                    Console.WriteLine("req.Port is null or not exist");
                    return;
                }

                inputQueue.Dictionary.TryGetValue(req.Port, out ConcurrentMessageBag<SignalRMessage> queue);

                if (!(queue is null))
                {
                    queue.Enqueue(new SignalRMessage
                    {
                        Method = SignalRMethod.DeleteAllFingerprints,
                        Port = req.Port
                    });
                }
            }));
        }
    }
}