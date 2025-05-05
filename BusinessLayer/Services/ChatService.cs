using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BusinessLayer.Interfaces;
using BusinessLayer.Models;

namespace BusinessLayer.Services
{
    internal class ChatService
    {
        public static string GetLocalIpAddress()
        {
            if (ChatConstants.DEBUG_MODE)
            {
                // In a real implementation, you might want to generate unique IDs
                // or have a way to specify which debug IP to use for each instance
                return ChatConstants.DEBUG_HOST_IP;
            }

            try
            {
                string hostName = Dns.GetHostName();
                IPAddress[] ipAddresses = Dns.GetHostEntry(hostName).AddressList;

                foreach (IPAddress ipAddress in ipAddresses)
                {
                    if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return ipAddress.ToString();
                    }
                }

                throw new Exception("No IP address found");
            }
            catch (Exception)
            {
                return ChatConstants.GET_IP_REPLACER;
            }
        }
    }
}
