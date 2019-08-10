using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Protocol.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        public Host(ITransport transport, ILogger logger = null)
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
        private protected ExecutedMethod executedMethod { get; set; }
        private protected readonly ITransport transport;

        public ExecutedMethod ExecutedMethod
        {
            get { return executedMethod; }
            set { executedMethod = value; }
        }


        public async Task<bool> SendResponseToTerminal(ResponseHeader header, CommandHeader command)
        {
            executedMethod.ResponseHeader = header;
            var response = executedMethod.CreateResponse();
            var result = false;
            while (!executedMethod.IsCompleted & executedMethod.MethodInfo.CommandHeader == command)
            {
                result = await transport.WriteMessageAsync(response);
                executedMethod.RepeatCount++;
                await Task.Delay(200);
            }
            return result;
        }

        public void ExecuteMethod(CommandHeader command)
        {
            Method methodInfo;
            Console.WriteLine($"ExecuteMethod hit:{command.GetDisplayName()}");
            Protocol.GetMethods().TryGetValue(command, out methodInfo);
            executedMethod = null;
            executedMethod = new ExecutedMethod { MethodInfo = methodInfo, IsCompleted = false, IsFired = false, ResponseHeader = ResponseHeader.NotSet };
            executedMethod.PropertyChanged += OnExecuteMethodChange;
            executedMethod.RepeatCountReachedLimit += OnExecuteMethodRepeatReachedLimit;
            executedMethod.RepeatLimit = 25;
            logger.LogWarning($"Executed method: {executedMethod.MethodInfo.CommandHeader.GetDisplayName()}");
            Console.WriteLine($"ExecuteMethod hit:{command.GetDisplayName()}");
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
                        await transport.WriteMessageAsync(Protocol.CreateCommand(CommandHeader.FingerTimeoutCurrent));
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
        protected void PushCardCommandEvent(string card, string md5)
        {
            CardCommandEvent?.Invoke(this, new CardCommandEventArgs(card, md5));
        }

        public delegate void FingerCommandEventHandler(object sender, FingerCommandEventArgs e);
        public event FingerCommandEventHandler FingerCommandEvent;
        public void PushFingerCommandEvent(string finger, string md5)
        {
            FingerCommandEvent?.Invoke(this, new FingerCommandEventArgs(finger, md5));
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
                if (!executedMethod.MethodInfo.IsControllerHosted)
                {
                    if (Protocol.CheckReadyTerminalHosted(executedMethod))
                    {
                        executedMethod.IsFired = true;
                        ProcessTerminalMethodAsync();
                    }
                }
                else
                {
                    if (Protocol.CheckReadyControllerHosted(executedMethod))
                    {
                        executedMethod.IsFired = true;
                        await ProcessControllerMethodAsync();
                    }
                }
            }
        }

        public void DataReceived(object sender, MessageQueueEnqueueEventArgs<string> e)
        {
            string input;
            bool dequeue = transport.InputQueue.TryDequeue(out input);
            if (dequeue && !String.IsNullOrWhiteSpace(input))
            {
                ProcessMessage(input);
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
                    PushCardCommandEvent(executedMethod.CommandValue, executedMethod.Hash);
                    break;
                case CommandHeader.Finger:
                    PushFingerCommandEvent(executedMethod.CommandValue, executedMethod.Hash);
                    break;
                    //case ProtocolCommands.
            }
        }


        private protected CommandHeader ReadCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                logger.LogInformation($"Command is null {command}");
                return CommandHeader.NotSet;
            }
            logger.LogInformation($"Command is {command}");

            var res = Protocol.GetCommandHeader(command);
            return res;
        }

        private protected ResponseHeader ReadResponse(string response)
        {
            if (string.IsNullOrEmpty(response))
            {
                logger.LogInformation($"Command is null {response}");
                return ResponseHeader.NotSet;
            }
            logger.LogInformation($"Message is {response}");

            var res = Protocol.GetResponseHeader(response);
            return res;
        }

        public void ProcessMessage(string input)
        {
            var message = input;
            CommandHeader command = CommandHeader.NotSet;

            if (message.Equals("COMPLETED"))
            {
                logger.LogWarning("COMPLETED");
                executedMethod.IsCompleted = true;
                return;
            }

            try
            {
                command = ReadCommand(message);

            }
            catch (CommandHeaderNotFoundException ex)
            {
                logger.LogInformation(ex.Message);
            }

            if (command == CommandHeader.NotSet)
            {
                if (executedMethod != null)
                {
                    if (!executedMethod.MethodInfo.IsControllerHosted)
                    {
                        if (executedMethod.MethodInfo.HasCommandValue && string.IsNullOrWhiteSpace(executedMethod.CommandValue))
                        {
                            executedMethod.CommandValue = message;
                        }
                        else if (executedMethod.MethodInfo.IsHashable && string.IsNullOrWhiteSpace(executedMethod.Hash))
                        {
                            executedMethod.Hash = message;
                        }
                    }
                    else
                    {
                        if (executedMethod.MethodInfo.HasResponseHeader && executedMethod.ResponseHeader == ResponseHeader.NotSet)
                        {
                            try
                            {
                                var responseHeader = ReadResponse(message);
                            }
                            catch (ResponseHeaderNotFoundException ex)
                            {

                                logger.LogWarning($"Flow warning, recieved message : {message}");
                                executedMethod = null;
                                logger.LogWarning("Executed method set to NULL");
                            }
                        }
                        else if (executedMethod.MethodInfo.HasResponseValue && string.IsNullOrWhiteSpace(executedMethod.ResponseValue))
                        {
                            executedMethod.ResponseValue = message;
                        }
                    }
                }
                else
                {
                    logger.LogWarning($"Flow warning, recieved message : {message}");
                }
            }
            else
            {
                if (executedMethod != null)
                {
                    if (executedMethod.IsCompleted)
                    {
                        ExecuteMethod(command);
                    }
                    else
                    {
                        logger.LogWarning($"Interrupted method: {executedMethod.MethodInfo.CommandHeader.GetDisplayName()}");
                        //   executedMethod.IsCompleted = true;//fix response
                        ExecuteMethod(command);
                    }
                }
                else
                {
                    ExecuteMethod(command);
                }
            }
        }
    }
}
