using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Protocol.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Protocol
{
    public interface IHost
    {

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
            get { return executedMethod; }
            set { executedMethod = value; }
        }


        public async Task<bool> SendResponseToTerminal(byte[] result, CommandHeader command)
        {
            executedMethod.ResponseValue = result;
            var response = executedMethod.CreateResponse();
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



        private async Task ProcessControllerMethodAsync()
        {
            switch (executedMethod.MethodInfo.CommandHeader)
            {
                case CommandHeader.FingerTimeoutCurrent:
                    // executedMethod.ResponseHeader = header;
                    //var response = executedMethod.CreateResponse();
                    //var result = false;
                    while (!executedMethod.IsCompleted & executedMethod.MethodInfo.CommandHeader == CommandHeader.FingerTimeoutCurrent)
                    {
                        Console.WriteLine("FingerTimeoutCurrent switch case");
                       // await transport.WriteMessageAsync(Protocol.CreateCommand(CommandHeader.FingerTimeoutCurrent));
                        executedMethod.RepeatCount++;
                        await Task.Delay(200);
                    }
                    //  return result;
                    break;
            }
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
                
                        executedMethod.IsFired = true;
                        //ProcessTerminalMethodAsync();
             
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
                // logger.LogWarning($"message type: {method.MethodInfo.CommandHeader.GetDisplayName()}");
                if (res)
                {
                    if (executedMethod != null)
                    {
                        if (executedMethod.IsCompleted)
                        {
                            if (message.Type == MessageType.REQ)
                            {
                                ExecuteMethod(message.Method.CommandHeader);
                                if (message.Method.HasCommandValue)
                                {
                                    executedMethod.CommandValue = message.Value;
                                }
                            }
                        }
                        else
                        {
                            switch (message.Type)
                            {
                                case MessageType.REQ:
                                    logger.LogWarning($"Interrupted method: {executedMethod.MethodInfo.CommandHeader}");
                                    ExecuteMethod(message.Method.CommandHeader);
                                    if (message.Method.HasCommandValue)
                                    {
                                        executedMethod.CommandValue = message.Value;
                                    }
                                    break;
                                case MessageType.RES:
                                    //executedMethod.Re
                                    break;
                                case MessageType.ACK:
                                    executedMethod.IsCompleted = true;
                                    break;
                                default: break;
                            }


                        }
                    }
                    else
                    {
                        // if (mtype==MessageType.REQ)
                        //{
                        ExecuteMethod(message.Method.CommandHeader);
                        if (message.Method.HasCommandValue)
                        {
                            executedMethod.CommandValue = message.Value;
                        }
                        //}
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
        private void ProcessTerminalMethodAsync()
        {
            switch (executedMethod.MethodInfo.CommandHeader)
            {
                case CommandHeader.Card:
                    PushCardCommandEvent(executedMethod.CommandValue);
                    break;
                case CommandHeader.Finger:
                    PushFingerCommandEvent(executedMethod.CommandValue);
                    break;
                    //case ProtocolCommands.
            }
        }


    }
}
