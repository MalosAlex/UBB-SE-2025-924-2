using System;
using System.Net;
using Microsoft.UI.Dispatching;
using BusinessLayer.Interfaces;
using BusinessLayer.Models;
using SteamProfile.Services;

namespace Steam_Community.DirectMessages.Services
{
    /// <summary>
    /// Provides chat services for connecting users, sending messages, and managing user statuses.
    /// </summary>
    public class ChatService : IChatService
    {
        private INetworkClient networkClient;
        private DispatcherQueue uiDispatcherQueue;
        private INetworkServer networkServer;

        public event EventHandler<MessageEventArgs> MessageReceived;
        public event EventHandler<UserStatusEventArgs> UserStatusChanged;
        public event EventHandler<ExceptionEventArgs> ExceptionOccurred;

        private string username;
        private string userIpAddress;
        private string serverInviteIpAddress;

        /// <summary>
        /// Initializes a new instance of the ChatService class.
        /// </summary>
        /// <param name="username">The username of the current user.</param>
        /// <param name="serverInviteIpAddress">The IP address of the server host, or HOST_IP_FINDER if this user is the host.</param>
        /// <param name="uiDispatcherQueue">The dispatcher queue for the UI thread.</param>
        public ChatService(string username, string serverInviteIpAddress, DispatcherQueue uiDispatcherQueue)
        {
            username = username;
            userIpAddress = GetLocalIpAddress();
            serverInviteIpAddress = serverInviteIpAddress;
            uiDispatcherQueue = uiDispatcherQueue;
        }

        /// <summary>
        /// Connects the current user to the chat server.
        /// </summary>
        public async void ConnectToServer()
        {
            try
            {
                bool isHostUser = serverInviteIpAddress == ChatConstants.HOST_IP_FINDER;

                if (isHostUser)
                {
                    serverInviteIpAddress = userIpAddress;
                    // Create and start the server if this user is the host
                    networkServer = new NetworkServer(userIpAddress, username);
                    networkServer.Start();

                    // Connect to the server as a client
                    networkClient = new NetworkClient(serverInviteIpAddress, username, uiDispatcherQueue);
                    networkClient.SetAsHost();
                }
                else
                {
                    // Just connect as a client
                    networkClient = new NetworkClient(serverInviteIpAddress, username, uiDispatcherQueue);
                }

                // Connect to the server
                await networkClient.ConnectToServer();

                // Register event handlers
                networkClient.MessageReceived += HandleMessageReceived;
                networkClient.UserStatusChanged += HandleUserStatusChanged;
            }
            catch (Exception exception)
            {
                // Report the exception to the UI
                uiDispatcherQueue.TryEnqueue(() =>
                    ExceptionOccurred?.Invoke(this, new ExceptionEventArgs(exception)));
            }
        }

        /// <summary>
        /// Sends a message to all connected users.
        /// </summary>
        /// <param name="messageContent">The content of the message to send.</param>
        public void SendMessage(string messageContent)
        {
            try
            {
                // Validate message content
                if (string.IsNullOrEmpty(messageContent))
                {
                    throw new Exception("Message content cannot be empty");
                }

                // Check if client is connected
                if (!networkClient?.IsConnected() ?? true)
                {
                    throw new Exception("Client is not connected to server");
                }

                // Send the message
                networkClient?.SendMessageToServer(messageContent);

                // Check if server is still running (for host only)
                if (networkServer?.IsRunning() == false)
                {
                    throw new Exception("Server timeout has been reached!");
                }
            }
            catch (Exception exception)
            {
                // Report the exception to the UI
                uiDispatcherQueue.TryEnqueue(() =>
                    ExceptionOccurred?.Invoke(this, new ExceptionEventArgs(exception)));
            }
        }

        /// <summary>
        /// Handles a received message and processes it for display.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="messageEventArgs">The event arguments containing the message.</param>
        private void HandleMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            Message receivedMessage = messageEventArgs.Message;

            // Align messages based on sender (current user's messages on right, others on left)
            bool isCurrentUserMessage = receivedMessage.MessageSenderName == username;
            receivedMessage.MessageAligment = isCurrentUserMessage
                ? ChatConstants.ALIGNMENT_RIGHT
                : ChatConstants.ALIGNMENT_LEFT;

            // Notify UI of the new message
            uiDispatcherQueue.TryEnqueue(() =>
                MessageReceived?.Invoke(this, new MessageEventArgs(receivedMessage)));
        }

        /// <summary>
        /// Disconnects the client from the server.
        /// </summary>
        public void DisconnectFromServer()
        {
            networkClient?.Disconnect();
        }

        /// <summary>
        /// Attempts to change the mute status of the specified user.
        /// </summary>
        /// <param name="targetUsername">The username of the target user.</param>
        public void AttemptChangeMuteStatus(string targetUsername)
        {
            string command = $"<{username}>|{ChatConstants.MUTE_STATUS}|<{targetUsername}>";
            SendMessage(command);
        }

        /// <summary>
        /// Attempts to change the admin status of the specified user.
        /// </summary>
        /// <param name="targetUsername">The username of the target user.</param>
        public void AttemptChangeAdminStatus(string targetUsername)
        {
            string command = $"<{username}>|{ChatConstants.ADMIN_STATUS}|<{targetUsername}>";
            SendMessage(command);
        }

        /// <summary>
        /// Attempts to kick the specified user from the chat room.
        /// </summary>
        /// <param name="targetUsername">The username of the target user.</param>
        public void AttemptKickUser(string targetUsername)
        {
            string command = $"<{username}>|{ChatConstants.KICK_STATUS}|<{targetUsername}>";
            SendMessage(command);
        }

        /// <summary>
        /// Handles user status changes and forwards them to the UI.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="userStatusEventArgs">The event arguments containing the user status.</param>
        private void HandleUserStatusChanged(object sender, UserStatusEventArgs userStatusEventArgs)
        {
            // Forward the status change to the UI
            uiDispatcherQueue.TryEnqueue(() =>
                UserStatusChanged?.Invoke(this,
                    new UserStatusEventArgs(userStatusEventArgs.UserStatus)));
        }

        /// <summary>
        /// Gets the local IP address of the current machine.
        /// </summary>
        /// <returns>The local IP address as a string.</returns>
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