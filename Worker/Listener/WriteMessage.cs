using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Worker.Host
{
    public partial class Listener
    {
        private Task WriteMessage(byte[] message)
        {
            if (stream.IsOpen)
            {
              //  if (stream.DtrEnable) stream.DtrEnable = false;
               // if (stream.RtsEnable) stream.RtsEnable = false;
                //Thread.Sleep(1000);
                try
                {
                    //   await stream.WriteAsync(message, 0, message.Count());
                    stream.Write(message, 0, message.Count());
                    //await stream.FlushAsync();
                    logger.LogInformation($"wrote to port {portName}: {Encoding.ASCII.GetString(message)}");
                    //logger.LogInformation($"CTS is false");         
                }
                catch (Exception ex)
                {
                    logger.LogWarning($"ex {portName}");
                    logger.LogWarning(ex.ToString());
                 //   if (!stream.DtrEnable) stream.DtrEnable = true;
                 //   if (!stream.RtsEnable) stream.RtsEnable = true;
                    throw new Exception(ex.Message);
                }
            }
            else
            {
                logger.LogInformation($"Cannot write to port: {portName}, port closed");
            }
           // if (!stream.DtrEnable) stream.DtrEnable = true;
           // if (!stream.RtsEnable) stream.RtsEnable = true;
            // Thread.Sleep(1000);
            // stream.DiscardInBuffer();
           // stream.DiscardOutBuffer();
            return Task.CompletedTask;
        }

        private Task WriteMessage(string message)
        {
            if (stream.IsOpen)
            {
              //  if (stream.DtrEnable) stream.DtrEnable = false;
              //  if (stream.RtsEnable) stream.RtsEnable = false;
             
                try
                { 
                    stream.Write(message);
                    var bytes=stream.BytesToWrite;
                    var size=stream.WriteBufferSize;
                    logger.LogInformation($"wrote to port {portName}: {message}, bytes {bytes}, buff_size {size}");      
                }
                catch (Exception ex)
                {
                    logger.LogWarning($"ex {portName}");
                    logger.LogWarning(ex.ToString());
               //     if (!stream.DtrEnable) stream.DtrEnable = true;
               //     if (!stream.RtsEnable) stream.RtsEnable = true;
                    throw new Exception(ex.Message);
                }
            }
            else
            {
                logger.LogInformation($"Cannot write to port: {portName}, port closed");
            }
           // if (!stream.DtrEnable) stream.DtrEnable = true;
           // if (!stream.RtsEnable) stream.RtsEnable = true;
            return Task.CompletedTask;
        }
    }
}
