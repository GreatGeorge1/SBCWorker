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


        public async Task<bool> SendResponseToTerminal(ResponseHeader header, CommandHeader command)
        {
            executedMethod.ResponseHeader = header;
            var response = executedMethod.CreateResponse();
            var result = false;
            while (!executedMethod.IsCompleted & executedMethod.MethodInfo.CommandHeader == command)
            {
                result = await transport.WriteMessageAsync(response);
                Console.WriteLine("Response sent");
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
                if (!executedMethod.MethodInfo.DirectionTo)
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

        public static byte[] ConvertHexStringToByteArray(string hexString)
        {
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "The binary key cannot have an odd number of digits: {0}", hexString));
            }

            byte[] data = new byte[hexString.Length / 2];
            for (int index = 0; index < data.Length; index++)
            {
                string byteValue = hexString.Substring(index * 2, 2);
                data[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return data;
        }

        public void DataReceived(object sender, MessageQueueEnqueueEventArgs<byte[]> e)
        {
            byte[] input;
            bool dequeue = transport.InputQueue.TryDequeue(out input);
            if (input!=null && input.Length>0)
            {
                // ProcessMessage(input);
                ExecutedMethod method;
                MessageType mtype;

                var res = RequestMiddleware.Process(input, out method, out _, out mtype);
                Console.WriteLine($"message type: {mtype.ToString()}");
                Console.WriteLine($"message type: {res.ToString()}");
                // logger.LogWarning($"message type: {method.MethodInfo.CommandHeader.GetDisplayName()}");
                if (res)
                {
                    if (executedMethod != null)
                    {
                        if (executedMethod.IsCompleted)
                        {
                            if (mtype == MessageType.REQ)
                            {
                                ExecuteMethod(method);
                            }
                        }
                        else
                        {
                            switch (mtype)
                            {
                                case MessageType.REQ:
                                    logger.LogWarning($"Interrupted method: {executedMethod.MethodInfo.CommandHeader.GetDisplayName()}");
                                    ExecuteMethod(method);
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
                        ExecuteMethod(method);
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

        //public void DataReceived(object sender, MessageQueueEnqueueEventArgs<string> e)
        //{
        //    string input;
        //    bool dequeue = transport.InputQueue.TryDequeue(out input);
        //    if (dequeue && !String.IsNullOrEmpty(input))
        //    {
        //        // ProcessMessage(input);
        //        ExecutedMethod method;
        //        MessageType mtype;
                
        //        //  input = new string(input.Where(c => !char.IsControl(c)).ToArray());
        //        //string[] hexValuesSplit = input.Split(new char[] {'x'}, StringSplitOptions.RemoveEmptyEntries);
        //        //List<byte> list = new List<byte>();
        //        //foreach (string hex in hexValuesSplit)
        //        //{
        //        //    // Convert the number expressed in base-16 to an integer.
        //        //    int value = Convert.ToInt32(hex, 16);
        //        //    // Get the character corresponding to the integral value.
        //        //    // string stringValue = Char.ConvertFromUtf32(value);
        //        //    //char charValue = (char)value;
        //        //    //Console.WriteLine("hexadecimal value = {0}, int value = {1}, char value = {2} or {3}",
        //        //    //                  hex, value, stringValue, charValue);
        //        //    list.Add((byte)value);
        //        //}
        //        //var arr = list.ToArray();

        //        var res = RequestMiddleware.Process(Encoding.ASCII.GetBytes(input), out method, out _, out mtype);
        //        Console.WriteLine($"message type: {mtype.ToString()}");
        //        Console.WriteLine($"message type: {res.ToString()}");
        //        // logger.LogWarning($"message type: {method.MethodInfo.CommandHeader.GetDisplayName()}");
        //        if (res)
        //        {
        //            if (executedMethod != null)
        //            {
        //                if (executedMethod.IsCompleted)
        //                {
        //                    if (mtype == MessageType.REQ)
        //                    {
        //                        ExecuteMethod(method);
        //                    }
        //                }
        //                else
        //                {
        //                    switch (mtype)
        //                    {
        //                        case MessageType.REQ:
        //                            logger.LogWarning($"Interrupted method: {executedMethod.MethodInfo.CommandHeader.GetDisplayName()}");
        //                            ExecuteMethod(method);
        //                            break;
        //                        case MessageType.RES:
        //                            //executedMethod.Re
        //                            break;
        //                        case MessageType.ACK:
        //                            executedMethod.IsCompleted = true;
        //                            break;
        //                        default: break;
        //                    }
                            
                            
        //                }
        //            }
        //            else
        //            {
        //               // if (mtype==MessageType.REQ)
        //                //{
        //                    ExecuteMethod(method);
        //                //}
        //            }
        //        }
        //        else
        //        {
        //            logger.LogWarning($"Process error valid:{res.ToString()}");
        //        }

        //    }
        //    else
        //    {
        //        logger.LogWarning("EnqueueAction error");
        //    }
        //}
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
                    if (!executedMethod.MethodInfo.DirectionTo)
                    {
                        if (executedMethod.MethodInfo.HasCommandValue && string.IsNullOrWhiteSpace(executedMethod.CommandValue))
                        {
                            executedMethod.CommandValue = message;
                        }
                        else if (executedMethod.MethodInfo.HasCheckSum && string.IsNullOrWhiteSpace(executedMethod.Hash))
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
