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

    public class Host : IHost
    {
        public Host(IByteTransport transport, ILogger logger = null)
        {
            this.transport = transport;
            transport.InputQueue.EnqueueEvent += DataReceived;
            if (logger != null)
            {
                this.logger = logger;
            }
            else
            {
                this.logger = new NullLogger<Host>();
            }
        }

        public Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!(transport.Init()))
            {
                throw new Exception("failed to init transport");
            }
            return Task.CompletedTask;
        }

        private protected ILogger logger { get; set; }

        /// <summary>
        /// колекция методов запущенных на контроллере
        /// </summary>
        private protected Queue<ExecutedMethod> hostedMethods { get; set; }
        /// <summary>
        /// метод запущенный на терминале
        /// </summary>
        private protected ExecutedMethod executedMethod { get; set; }
        private protected readonly IByteTransport transport;

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
            while (!executedMethod.IsCompleted & executedMethod.MethodInfo.CommandHeader == command)
            {
                res = await transport.WriteMessageAsync(response).ConfigureAwait(false);
                Console.WriteLine("Response sent");
                executedMethod.RepeatCount++;
                await Task.Delay(200).ConfigureAwait(false);
            }
            return res;
        }

        public void ExecuteMethod(CommandHeader command)
        {
            Method methodInfo;
            Console.WriteLine($"ExecuteMethod hit:{command.GetDisplayName()}");
            Static.GetMethods().TryGetValue(command, out methodInfo);
            executedMethod = null;
            executedMethod = new ExecutedMethod { MethodInfo = methodInfo, IsCompleted = false, IsFired = false };
            executedMethod.PropertyChanged += OnExecuteMethodChange;
            executedMethod.RepeatCountReachedLimit += OnExecuteMethodRepeatReachedLimit;
            executedMethod.RepeatLimit = 25;
            logger.LogWarning($"Executed method: {executedMethod.MethodInfo.CommandHeader.GetDisplayName()}");
            Console.WriteLine($"ExecuteMethod hit:{command.GetDisplayName()}");
        }

        public void ExecuteMethod (ExecutedMethod method)
        {
            Console.WriteLine($"ExecuteMethod hit:{method.MethodInfo.CommandHeader.GetDisplayName()}");
        //    Protocol.GetMethods().TryGetValue(command, out methodInfo);
            executedMethod = null;
            executedMethod = method;
            executedMethod.PropertyChanged += OnExecuteMethodChange;
            executedMethod.RepeatCountReachedLimit += OnExecuteMethodRepeatReachedLimit;
            executedMethod.RepeatLimit = 25;
            logger.LogWarning($"Executed method: {executedMethod.MethodInfo.CommandHeader.GetDisplayName()}");
            Console.WriteLine($"ExecuteMethod hit:{method.MethodInfo.CommandHeader.GetDisplayName()}");
        }

        public async Task ExecuteControllerMethodAsync(CommandHeader header, byte[] value)
        {
            if(executedMethod is null)
            {
                ExecuteMethod(header);
                executedMethod.CommandValue = value;
                await ProcessControllerMethodAsync();
            }
            else
            {
                while(executedMethod.IsCompleted != true)
                {
                    Console.WriteLine("Waiting to execute method");
                    await Task.Delay(50);
                }
                ExecuteMethod(header);
                executedMethod.CommandValue = value;
                await ProcessControllerMethodAsync();
            }
        }

        private async Task<bool> ProcessControllerMethodAsync()
        {
            switch (executedMethod.MethodInfo.CommandHeader)
            {
                case CommandHeader.FingerTimeoutCurrent:
                   
                    while (!executedMethod.IsCompleted & executedMethod.MethodInfo.CommandHeader == CommandHeader.FingerTimeoutCurrent)
                    {
                        Console.WriteLine("FingerTimeoutCurrent switch case");
                        await transport.WriteMessageAsync(new byte[] { 0x02, 0xd5,(byte)CommandHeader.FingerTimeoutCurrent, 0x02,0x00,0x01,0x01,0x03,0x0d,0x0a});
                        executedMethod.RepeatCount++;
                        await Task.Delay(200);
                    }
                    return true;
                    break;
                case CommandHeader.FingerWriteInBase:

                    var list = new List<byte>();
                 //   list.AddRange(new byte[] { 0x02, 0xd5,
                 //           (byte)CommandHeader.FingerWriteInBase,
                 //           (byte)ExecutedMethod.CommandValue.Length });
                 //   list.AddRange(ExecutedMethod.CommandValue);
                 //   list.Add(RequestMiddleware.CalCheckSum(ExecutedMethod.CommandValue, ExecutedMethod.CommandValue.Length));
                 //   list.AddRange(new byte[] { 0x03, 0x0d, 0x0a });
                 //   var msg = list.ToArray();
                    while (!executedMethod.IsCompleted & executedMethod.MethodInfo.CommandHeader == CommandHeader.FingerWriteInBase)
                    {
                        Console.WriteLine("FingerWriteInBase switch case");
                        //Console.WriteLine(BitConverter.ToString(msg));
                        await transport.WriteMessageAsync(new byte[] { 0x02, 0xd5,
                            (byte)CommandHeader.FingerWriteInBase,
                            0x02, 0x09, 0x01, 0x0B, 0x03, 0x0d, 0x0a });
                       // await transport.WriteMessageAsync(msg);
                        executedMethod.RepeatCount++;
                        await Task.Delay(200);
                    }
                    return true;
                    break;
            }
            return false;
        }
        #region protocol host events
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
            logger.LogWarning($"RepeatCount ReachedLimit: {args.Count}");
            executedMethod.IsCompleted = true;
            executedMethod.IsError = true;
        }

        protected async void OnExecuteMethodChange(object sender, PropertyChangedEventArgs args)
        {
            logger.LogInformation($"Executed method property changed: {args.PropertyName}");
            if (!executedMethod.IsFired)
            {
                var direction = executedMethod.MethodInfo.DirectionTo;
                if (executedMethod.MethodInfo.HasCommandValue)
                {
                    if(executedMethod.CommandValue!=null && executedMethod.CommandValue.Length > 0)
                    {
                        executedMethod.IsFired = true;
                        _=(direction == Direction.Controller) ? ProcessTerminalMethodAsync() : await ProcessControllerMethodAsync().ConfigureAwait(false);
                    };
                }
                else
                {
                    executedMethod.IsFired = true;
                    _ = (direction == Direction.Controller) ? ProcessTerminalMethodAsync() : await ProcessControllerMethodAsync().ConfigureAwait(false);
                }
             
            }
        }

        public void DataReceived(object sender, MessageQueueEnqueueEventArgs<byte[]> e)
        {
            byte[] input;
            bool dequeue = transport.InputQueue.TryDequeue(out input);
            if (input!=null && input.Length>0)
            {
                Message message;
                var res = RequestMiddleware.Process(input, out message);
                Console.WriteLine($"message type: {message.Type.ToString()}");
                Console.WriteLine($"message type: {res.ToString()}");
                if (res)
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
                            logger.LogWarning("Protocol flow error: executedMethod is null");
                        }
                    }
                    if (executedMethod != null)
                    {
                        if (message.Method.CommandHeader != executedMethod.MethodInfo.CommandHeader && message.Type != MessageType.REQ)
                        {
                            logger.LogWarning("Protocol flow warning: CommandHeader");
                            return;
                        }
                        switch (message.Type)
                        {
                            case MessageType.ACK:
                                executedMethod.IsCompleted = true;
                                break;
                            case MessageType.NACK:
                                executedMethod.PushError(message.Value);     
                                break;
                            case MessageType.RES:
                                executedMethod.ResponseValue = message.Value;
                                ///TODO validation
                                if (executedMethod.MethodInfo.DirectionTo == Direction.Terminal)
                                {
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
                                logger.LogWarning("Protocol flow error: MessageType");
                                break;
                        }
                    }
                }
                else
                {
                    logger.LogWarning($"Process error valid:{res.ToString()}");
                }

            }
            else
            {
                logger.LogWarning("EnqueueAction error");
            }
        }

        #endregion

        private bool ProcessTerminalMethodAsync()
        {
            switch (executedMethod.MethodInfo.CommandHeader)
            {
                case CommandHeader.Card:
                    PushCardCommandEvent(executedMethod.CommandValue);
                    return true;
                    break;
                case CommandHeader.Finger:
                    PushFingerCommandEvent(executedMethod.CommandValue);
                    return true;
                    break;
                case CommandHeader.Ble:
                    PushBleCommandEvent(executedMethod.CommandValue);
                    return true;
                    break;
                    //case ProtocolCommands.
            }
            return false;
        }


    }
}
