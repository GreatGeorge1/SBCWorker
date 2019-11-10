using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Protocol.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace Protocol
{
    public interface IHost
    {
        Task ExecuteAsync(CancellationToken stoppingToken);
    }

    public class Host : BackgroundService
    {
        public Host(IByteTransport transport, ILogger logger = null)
        {
            if(transport is null)
            {
                throw new ArgumentNullException(nameof(transport));
            }
            this.transport = transport;
            transport.InputQueue.EnqueueEvent += DataReceived;
            if (logger != null)
            {
                this.Logger = logger;
            }
            else
            {
                this.Logger = new NullLogger<Host>();
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!stoppingToken.IsCancellationRequested)
            {
                if (!(transport.Init()))
                {
                    throw new TransportInitException($"failed to init transport\r\n info:'{transport.GetInfo()}'");
                }
                return Task.CompletedTask;
            }
            return Task.CompletedTask;
        }

        private protected ILogger Logger { get; set; }

        /// <summary>
        /// колекция методов запущенных на контроллере
        /// </summary>
        private protected Queue<ExecutedMethod> HostedMethods { get; set; }
        /// <summary>
        /// метод запущенный на терминале
        /// </summary>
        private protected ExecutedMethod executedMethod { get; set; }
        private protected readonly IByteTransport transport;
        private protected bool IsLive = true;

        public ExecutedMethod ExecutedMethod
        {
            get => executedMethod;
            set => executedMethod = value;
        }


        public async Task<bool> SendResponseToTerminal(byte[] result, CommandHeader command)
        {
            executedMethod.ResponseValue = result;
            var response = executedMethod.CreateResponse(result);
            var res = false;
            while (executedMethod.IsCompleted == false && executedMethod.MethodInfo.CommandHeader == command)
            {
                res = await transport.WriteMessageAsync(response).ConfigureAwait(false);
                Console.WriteLine($"Response sent: '{BitConverter.ToString(response)}'");
                executedMethod.RepeatCount++;
                await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            }
            return res;
        }

        public void ExecuteMethod(CommandHeader command)
        {
            Data.GetMethods().TryGetValue(command, out Method methodInfo);
            executedMethod = null;
            executedMethod = new ExecutedMethod { MethodInfo = methodInfo, IsCompleted = false, IsFired = false };
            executedMethod.PropertyChanged += OnExecuteMethodChange;
            executedMethod.RepeatCountReachedLimit += OnExecuteMethodRepeatReachedLimit;
            executedMethod.RepeatLimit = 3;
            Console.WriteLine($"ExecuteMethod hit:{command.GetDisplayName()}");
        }

        public ExecutedMethod PrepareExecutedMethod(CommandHeader command)
        {
            var flag = Data.GetMethods().TryGetValue(command, out Method methodInfo);
            if (flag == false || methodInfo is null)
            {
                Console.WriteLine("methodInfo is null, panic");
                flag = Data.GetMethods().TryGetValue(command, out methodInfo);
                if (flag == false || methodInfo is null)
                {
                    Console.WriteLine("methodInfo is null, insult");
                    throw new NullReferenceException(nameof(methodInfo));
                }
            }
            //executedMethod = null;
            executedMethod = new ExecutedMethod { MethodInfo = methodInfo, IsCompleted = false, IsFired = false };
            return executedMethod;
        }

        public void ExecuteMethod(ExecutedMethod method)
        {
            //    Protocol.GetMethods().TryGetValue(command, out methodInfo);
            executedMethod = null;
            executedMethod = method;
            if (executedMethod is null) {
                Console.WriteLine("method is null, heartattack");
                return;
            }
            executedMethod.PropertyChanged += OnExecuteMethodChange;
            executedMethod.RepeatCountReachedLimit += OnExecuteMethodRepeatReachedLimit;
            executedMethod.RepeatLimit = 3;
            // logger.LogWarning($"Executed method: {executedMethod.MethodInfo.CommandHeader.GetDisplayName()}");
            Console.WriteLine($"ExecuteMethod hit:{method.MethodInfo.CommandHeader.GetDisplayName()}");
        }

        public async Task<bool> ExecuteControllerMethodAsync(CommandHeader header, byte[] value,string address=null)
        {
            if (executedMethod is null)
            {
                var exMethod = PrepareExecutedMethod(header);
                exMethod.CommandValue = value;
                if (!(String.IsNullOrEmpty(address)))
                {
                    exMethod.ResponseAddress = address;
                }
                if (exMethod is null)
                {
                    Console.WriteLine("exMethod is null, brain cancer");
                    return false;
                }
                ExecuteMethod(exMethod);
                return await ProcessControllerMethodAsync().ConfigureAwait(false);
            }
            else
            {
                while (executedMethod.IsCompleted != true)
                {
                   // Console.WriteLine("Waiting to execute method");
                    await Task.Delay(50).ConfigureAwait(false);
                }
                var exMethod = PrepareExecutedMethod(header);
                if (exMethod is null)
                {
                    Console.WriteLine("exMethod is null, brain cancer");
                    return false;
                }
                exMethod.CommandValue = value;
                if (!(String.IsNullOrEmpty(address)))
                {
                    exMethod.ResponseAddress = address;
                }
                ExecuteMethod(exMethod);
                return await ProcessControllerMethodAsync().ConfigureAwait(false);
            }
        }

        private async Task<bool> ProcessControllerMethodAsync()
        {
            List<byte> list = new List<byte>();
            bool res = false;
            switch (executedMethod.MethodInfo.CommandHeader)
            {
                case CommandHeader.FingerTimeoutCurrent:
                    while (executedMethod.IsCompleted == false && executedMethod.MethodInfo.CommandHeader == CommandHeader.FingerTimeoutCurrent)
                    {
                        Console.WriteLine("FingerTimeoutCurrent switch case");
                        await transport.WriteMessageAsync(new byte[] 
                        { 
                            0x02, 
                            0xd5, 
                            (byte)CommandHeader.FingerTimeoutCurrent, 
                            RequestMiddleware.CalCheckSum(ExecutedMethod.CommandValue, ExecutedMethod.CommandValue.Length),
                            (byte)ExecutedMethod.CommandValue.Length,
                            0x00
                        }).ConfigureAwait(false);
                        executedMethod.RepeatCount++;
                        await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
                    }
                    res=true;
                    break;
                case CommandHeader.FingerWriteInBase:
                    list = new List<byte>();
                    list.AddRange(new byte[] {
                        0x02,
                        0xd5,
                        (byte)CommandHeader.FingerWriteInBase,
                        RequestMiddleware.CalCheckSum(ExecutedMethod.CommandValue, ExecutedMethod.CommandValue.Length),
                        (byte)ExecutedMethod.CommandValue.Length
                    });
                    list.AddRange(ExecutedMethod.CommandValue);

                    var msg = list.ToArray();
                    while (executedMethod.IsCompleted == false && executedMethod.MethodInfo.CommandHeader == CommandHeader.FingerWriteInBase)
                    {
                        Console.WriteLine("FingerWriteInBase switch case");
                        Console.WriteLine(BitConverter.ToString(msg));
                        await transport.WriteMessageAsync(msg).ConfigureAwait(false);
                        executedMethod.RepeatCount++;
                        await Task.Delay(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
                    }
                    res = true;
                    break;
                case CommandHeader.TerminalConf:
                    list = new List<byte>();
                    list.AddRange(new byte[] {
                        0x02,
                        0xd5,
                        (byte)CommandHeader.TerminalConf,
                        RequestMiddleware.CalCheckSum(ExecutedMethod.CommandValue, ExecutedMethod.CommandValue.Length),
                        (byte)ExecutedMethod.CommandValue.Length
                    });
                    list.AddRange(ExecutedMethod.CommandValue);
                    var msg1 = list.ToArray();
                    while (executedMethod.IsCompleted == false && executedMethod.MethodInfo.CommandHeader == CommandHeader.TerminalConf)
                    {
                        Console.WriteLine("TerminalConf switch case");
                        Console.WriteLine(BitConverter.ToString(msg1));
                        await transport.WriteMessageAsync(msg1).ConfigureAwait(false);
                        executedMethod.RepeatCount++;
                        await Task.Delay(TimeSpan.FromSeconds(60)).ConfigureAwait(false);
                    }
                    res = true;
                    break;
                case CommandHeader.TerminalSysInfo:
                    list = new List<byte>();
                    list.AddRange(new byte[] {
                        0x02,
                        0xd5,
                        (byte)CommandHeader.TerminalSysInfo,
                        RequestMiddleware.CalCheckSum(ExecutedMethod.CommandValue, ExecutedMethod.CommandValue.Length),
                        (byte)ExecutedMethod.CommandValue.Length,
                    });
                    list.AddRange(ExecutedMethod.CommandValue);
                    var msg111 = list.ToArray();
                    while (executedMethod.IsCompleted == false && executedMethod.MethodInfo.CommandHeader == CommandHeader.TerminalSysInfo)
                    {
                        Console.WriteLine("TerminalConf switch case");
                        Console.WriteLine(BitConverter.ToString(msg111));
                        await transport.WriteMessageAsync(msg111).ConfigureAwait(false);
                        executedMethod.RepeatCount++;
                        await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
                    }
                    res = true;
                    break;
                case CommandHeader.FingerDeleteId:
                    list = new List<byte>();
                    list.AddRange(new byte[] {
                        0x02,
                        0xd5,
                        (byte)CommandHeader.FingerDeleteId,
                        RequestMiddleware.CalCheckSum(ExecutedMethod.CommandValue, ExecutedMethod.CommandValue.Length),
                        (byte)ExecutedMethod.CommandValue.Length
                    });
                    list.AddRange(ExecutedMethod.CommandValue);
                    var msg2 = list.ToArray();
                    while (executedMethod.IsCompleted == false && executedMethod.MethodInfo.CommandHeader == CommandHeader.FingerDeleteId)
                    {
                        Console.WriteLine("FingerDeleteId  loop");
                        Console.WriteLine(BitConverter.ToString(msg2));
                        await transport.WriteMessageAsync(msg2).ConfigureAwait(false);
                        executedMethod.RepeatCount++;
                        await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
                    }
                    res = true;
                    break;
                case CommandHeader.FingerDeleteAll:
                    list = new List<byte>();
                    list.AddRange(new byte[] {
                        0x02,
                        0xd5,
                        (byte)CommandHeader.FingerDeleteAll,
                        RequestMiddleware.CalCheckSum(ExecutedMethod.CommandValue, ExecutedMethod.CommandValue.Length),
                        (byte)ExecutedMethod.CommandValue.Length
                    });
                    list.AddRange(ExecutedMethod.CommandValue);
                    var msg3 = list.ToArray();
                    while (executedMethod.IsCompleted == false && executedMethod.MethodInfo.CommandHeader == CommandHeader.FingerDeleteAll)
                    {
                        Console.WriteLine("FingerDeleteAll  loop");
                        Console.WriteLine(BitConverter.ToString(msg3));
                        await transport.WriteMessageAsync(msg3).ConfigureAwait(false);
                        executedMethod.RepeatCount++;
                        await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
                    }
                    res = true;
                    break;
                case CommandHeader.FingerSetTimeout:
                    list = new List<byte>();
                    list.AddRange(new byte[] {
                        0x02,
                        0xd5,
                        (byte)CommandHeader.FingerSetTimeout,
                        RequestMiddleware.CalCheckSum(ExecutedMethod.CommandValue, ExecutedMethod.CommandValue.Length),
                        (byte)ExecutedMethod.CommandValue.Length
                    });
                    list.AddRange(ExecutedMethod.CommandValue);
                    var msg4 = list.ToArray();
                    while (executedMethod.IsCompleted == false && executedMethod.MethodInfo.CommandHeader == CommandHeader.FingerSetTimeout)
                    {
                        Console.WriteLine("FingerSetTimeout  loop");
                        Console.WriteLine(BitConverter.ToString(msg4));
                        await transport.WriteMessageAsync(msg4).ConfigureAwait(false);
                        executedMethod.RepeatCount++;
                        await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
                    }
                    res = true;
                    break;
            }
            return res;
        }
        #region protocol host events
        public delegate void GetConfigEventHandler(object sender, GetConfigEventArgs e);
        public event GetConfigEventHandler GetConfigEvent;
        protected void PushGetConfigEvent(byte[] json, string address)
        {
            GetConfigEvent?.Invoke(this, new GetConfigEventArgs(json, address));
        }
 

        public delegate void CardCommandEventHandler(object sender, CardCommandEventArgs e);
        public event CardCommandEventHandler CardCommandEvent;
        protected void PushCardCommandEvent(byte[] card)
        {
            CardCommandEvent?.Invoke(this, new CardCommandEventArgs(card));
        }

        public delegate void FingerCommandEventHandler(object sender, FingerCommandEventArgs e);
        public event FingerCommandEventHandler FingerCommandEvent;
        public void PushFingerCommandEvent(byte[] finger)
        {
            FingerCommandEvent?.Invoke(this, new FingerCommandEventArgs(finger));
        }

        public delegate void BleCommandEventHandler(object sender, BleCommandEventArgs e);
        public event BleCommandEventHandler BleCommandEvent;

        public void PushBleCommandEvent(byte[] ble)
        {
            BleCommandEvent?.Invoke(this, new BleCommandEventArgs(ble));
        }

        protected void OnExecuteMethodRepeatReachedLimit(object sender, RepeatCountReachedLimitArgs args)
        {
            if(args is null)
            {
                throw new ArgumentNullException(nameof(args));
            }
            Logger.LogWarning($"RepeatCount ReachedLimit: {args.Count}");
            executedMethod.IsCompleted = true;
            executedMethod.IsError = true;
        }

        protected async void OnExecuteMethodChange(object sender, PropertyChangedEventArgs args)
        {
            if(args is null)
            {
                throw new ArgumentNullException(nameof(args));
            }
            Console.WriteLine($"Info: Executed method property changed: {args.PropertyName}");
            if (executedMethod.IsFired==false)
            {
                var direction = executedMethod.MethodInfo.DirectionTo;
                if (executedMethod.MethodInfo.HasCommandValue)
                {
                    if(executedMethod.CommandValue!=null && executedMethod.CommandValue.Length > 0)
                    {
                        executedMethod.IsFired = true;
                        _=(direction == Direction.Controller) ? ProcessTerminalMethod() : await ProcessControllerMethodAsync().ConfigureAwait(false);
                    };
                }
                else
                {
                    executedMethod.IsFired = true;
                    _ = (direction == Direction.Controller) ? ProcessTerminalMethod() : await ProcessControllerMethodAsync().ConfigureAwait(false);
                }
             
            }
        }

        public async void DataReceived(object sender, MessageQueueEnqueueEventArgs<byte[]> e)
        {
            bool dequeue = transport.InputQueue.TryDequeue(out byte[] input);
            if (!dequeue)
            {
                Console.WriteLine($"DataReceived action canceled:failed to dequeue");
                return;
            }
            if (input!=null && input.Length>0)
            {
                var message = RequestMiddleware.Process(input);
                Console.WriteLine($"message type: {message.Type.ToString()}");
              //  Console.WriteLine($"message type: {res.ToString()}");
                if (message.IsValid)
                {
                    if (executedMethod == null)
                    {
                        if (message.Type == MessageType.REQ)
                        {
                            ExecuteMethod(message.Method.CommandHeader);
                            if (message.Method.HasCommandValue)
                            {
                                executedMethod.CommandValue = message.Value;
                            }
                        }
                        else
                        {
                            Logger.LogWarning("Protocol flow error: executedMethod is null");
                        }
                    }
                    if (executedMethod != null)
                    {
                        if (message.Method.CommandHeader != executedMethod.MethodInfo.CommandHeader && message.Type != MessageType.REQ)
                        {
                            Logger.LogWarning("Protocol flow warning: CommandHeader");
                            return;
                        }
                        switch (message.Type)
                        {
                            case MessageType.ACK:
                                if (!executedMethod.IsCompleted && executedMethod.MethodInfo.CommandHeader==message.Method.CommandHeader)
                                {
                                    if (ExecutedMethod.MethodInfo.CommandHeader == CommandHeader.TerminalConf)
                                    {
                                        await Task.Delay(TimeSpan.FromSeconds(25)).ConfigureAwait(false);
                                    }
                                    executedMethod.IsCompleted = true;
                                }   
                                break;
                            case MessageType.NACK:
                                if (!executedMethod.IsCompleted && executedMethod.MethodInfo.CommandHeader == message.Method.CommandHeader)
                                {
                                    executedMethod.PushError(message.Value);
                                }     
                                break;
                            case MessageType.RES:
                                ///TODO validation
                                if (executedMethod.MethodInfo.CommandHeader == message.Method.CommandHeader)
                                {
                                    executedMethod.ResponseValue = message.Value;
                                    if (!executedMethod.IsCompleted && executedMethod.MethodInfo.CommandHeader == CommandHeader.TerminalSysInfo)
                                    {
                                        PushGetConfigEvent(message.Value, executedMethod.ResponseAddress);
                                    }
                                    executedMethod.IsCompleted = true;
                                }
                                break;
                            case MessageType.REQ:
                                ExecuteMethod(message.Method.CommandHeader);
                                if (message.Method.HasCommandValue)
                                {
                                    executedMethod.CommandValue = message.Value;
                                }
                                break;                            
                            default:
                                Logger.LogWarning("Protocol flow error: MessageType");
                                break;
                        }
                    }
                }
                else
                {
                    Logger.LogWarning($"Process error valid:{message.IsValid}");
                }

            }
            else
            {
                Logger.LogWarning("EnqueueAction error");
            }
        }

        #endregion

        private bool ProcessTerminalMethod()
        {
            bool res = false;
            switch (executedMethod.MethodInfo.CommandHeader)
            {
                case CommandHeader.Card:
                    PushCardCommandEvent(executedMethod.CommandValue);
                    res = true;
                    break;
                case CommandHeader.Finger:
                    PushFingerCommandEvent(executedMethod.CommandValue);
                    res = true;
                    break;
                case CommandHeader.Ble:
                    PushBleCommandEvent(executedMethod.CommandValue);
                    res = true;
                    break;
                case CommandHeader.Error:
                    Console.WriteLine("Error processed");
                    executedMethod.IsCompleted = true;
                    res = true;
                    break;
                    //case ProtocolCommands.
            }
            return res;
        }
    }

    public class TransportInitException : Exception
    {
        public TransportInitException(string message) : base(message)
        {
        }

        public TransportInitException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public TransportInitException()
        {
        }
    }
}
