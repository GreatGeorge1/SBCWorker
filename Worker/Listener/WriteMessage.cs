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
                try
                {
                    stream.Write(message, 0, message.Count());
                    logger.LogInformation($"wrote to port {port.PortName}: {Encoding.ASCII.GetString(message)}");        
                }
                catch (Exception ex)
                {
                    logger.LogWarning($"ex {port.PortName}");
                    logger.LogWarning(ex.ToString());
                 //   if (!stream.DtrEnable) stream.DtrEnable = true;
                 //   if (!stream.RtsEnable) stream.RtsEnable = true;
                    throw new Exception(ex.Message);
                }
            }
            else
            {
                logger.LogCritical($"Cannot write to port: {port.PortName}, port closed");
            }
           // if (!stream.DtrEnable) stream.DtrEnable = true;
           // if (!stream.RtsEnable) stream.RtsEnable = true;
            return Task.CompletedTask;
        }

        private async Task WriteMessage(string message)
        {
            if (stream.IsOpen)
            {
                try
                {
                    if (port.IsRS485)
                    {
                        stream.RtsEnable = false;
                        await Task.Delay(50);
                    }
                    stream.Write(message);
                    if (port.IsRS485)
                    {
                        await Task.Delay(50);
                        stream.RtsEnable = true;
                    }
                    var bytes=stream.BytesToWrite;
                    var size=stream.WriteBufferSize;
                    logger.LogInformation($"wrote to port {port.PortName}: {message}, bytes {bytes}, buff_size {size}");      
                }
                catch (Exception ex)
                {
                    logger.LogWarning($"ex {port.PortName}");
                    logger.LogWarning(ex.ToString());
                    if (port.IsRS485)
                    {
                        stream.RtsEnable = true;
                    }
                    throw new Exception(ex.Message);
                }
            }
            else
            {
                logger.LogCritical($"Cannot write to port: {port.PortName}, port closed");
            }
            return;
        }
    }
}
