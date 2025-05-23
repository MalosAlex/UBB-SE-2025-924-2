﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using BusinessLayer.Models;
using BusinessLayer.Interfaces;

namespace SteamProfile.Services
{
    /// <summary>
    /// Implements network server functionality for managing chat connections and routing messages.
    /// </summary>
    public class NetworkServer : INetworkServer
    {
        private Socket serverSocket;
        private IPEndPoint ipEndPoint;
        private Timer serverTimeoutTimer;
        private readonly object timerLock = new object();

        private ConcurrentDictionary<string, string> ipAddressesToUsernames = new ConcurrentDictionary<string, string>();
        private ConcurrentDictionary<Socket, string> socketsToIpAddresses = new ConcurrentDictionary<Socket, string>();
        private ConcurrentDictionary<string, Socket> usernamesToSockets = new ConcurrentDictionary<string, Socket>();
        private ConcurrentDictionary<string, bool> mutedUsers = new ConcurrentDictionary<string, bool>();
        private ConcurrentDictionary<string, bool> adminUsers = new ConcurrentDictionary<string, bool>();

        private Regex muteCommandRegex;
        private Regex adminCommandRegex;
        private Regex kickCommandRegex;
        private Regex infoChangeCommandRegex;

        private string hostUsername;
        private readonly string muteCommandPattern = @"^<.*>\|" + ChatConstants.MUTE_STATUS + @"\|<.*>$";
        private readonly string adminCommandPattern = @"^<.*>\|" + ChatConstants.ADMIN_STATUS + @"\|<.*>$";
        private readonly string kickCommandPattern = @"^<.*>\|" + ChatConstants.KICK_STATUS + @"\|<.*>$";
        private readonly string infoCommandPattern = @"^<INFO>\|.*\|<INFO>$";

        private bool isRunning;

        /// <summary>
        /// Initializes a new instance of the NetworkServer class.
        /// </summary>
        /// <param name="hostIpAddress">The IP address of the host.</param>
        /// <param name="hostUsername">The username of the host.</param>
        /// <exception cref="Exception">Thrown when server creation fails.</exception>
        public NetworkServer(string hostIpAddress, string hostUsername)
        {
            hostUsername = hostUsername;

            // Initialize regex patterns for command matching
            muteCommandRegex = new Regex(muteCommandPattern);
            adminCommandRegex = new Regex(adminCommandPattern);
            kickCommandRegex = new Regex(kickCommandPattern);
            infoChangeCommandRegex = new Regex(infoCommandPattern);

            try
            {
                // Set up server socket
                ipEndPoint = new IPEndPoint(IPAddress.Parse(hostIpAddress), ChatConstants.PORT_NUMBER);
                serverSocket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Bind(ipEndPoint);
                serverSocket.Listen(ChatConstants.NUMBER_OF_QUEUED_CONNECTIONS);
            }
            catch (Exception exception)
            {
                throw new Exception($"Server creation failed: {exception.Message}");
            }

            isRunning = true;
        }

        /// <summary>
        /// Starts the server and begins listening for client connections.
        /// </summary>
        public async void Start()
        {
            // Accept connections until server is stopped
            while (isRunning)
            {
                try
                {
                    // Accept new client connection
                    Socket clientSocket = await serverSocket.AcceptAsync();
                    string defaultEndpointValue = "Not Connected";

                    // Get client IP address
                    string ipAddressWithPort = clientSocket.RemoteEndPoint?.ToString() ?? defaultEndpointValue;
                    string clientIpAddress = ipAddressWithPort.Substring(
                        ChatConstants.STARTING_INDEX,
                        ipAddressWithPort.IndexOf(ChatConstants.ADDRESS_SEPARATOR));

                    // Check if server is at capacity
                    if (socketsToIpAddresses.Count >= ChatConstants.MAXIMUM_NUMBER_OF_ACTIVE_CONNECTIONS)
                    {
                        // Notify client and close connection
                        SendMessageToClient(
                            CreateMessage(ChatConstants.SERVER_CAPACITY_REACHED, hostUsername),
                            clientSocket);

                        SendMessageToClient(
                            CreateMessage(ChatConstants.INFO_CHANGE_KICK_STATUS_COMMAND, hostUsername),
                            clientSocket);

                        continue;
                    }

                    // Store client connection information
                    socketsToIpAddresses.TryAdd(clientSocket, clientIpAddress);

                    // Check if minimum connections requirement is met
                    CheckAndStartTimeoutIfNeeded();

                    // Handle client in the background
                    _ = Task.Run(() => HandleClientConnection(clientSocket));
                }
                catch (Exception)
                {
                }
            }
        }

        /// <summary>
        /// Handles communication with a connected client.
        /// </summary>
        /// <param name="clientSocket">The socket connected to the client.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task HandleClientConnection(Socket clientSocket)
        {
            try
            {
                // Receive username from client
                byte[] usernameBuffer = new byte[ChatConstants.USER_NAME_MAXIMUM_SIZE];
                int usernameLength = await clientSocket.ReceiveAsync(usernameBuffer, SocketFlags.None);
                string clientUsername = Encoding.UTF8.GetString(
                    usernameBuffer,
                    ChatConstants.STARTING_INDEX,
                    usernameLength);

                // Get client IP address
                string clientIpAddress = socketsToIpAddresses.GetValueOrDefault(clientSocket) ?? string.Empty;

                // Store client information
                ipAddressesToUsernames.TryAdd(clientIpAddress, clientUsername);
                usernamesToSockets.TryAdd(clientUsername, clientSocket);
                adminUsers.TryAdd(clientUsername, false);
                mutedUsers.TryAdd(clientUsername, false);

                // Process client messages
                while (isRunning)
                {
                    // Check if client is still connected
                    if (clientSocket == null)
                    {
                        RemoveClientInformation(clientSocket, clientUsername, clientIpAddress);
                        break;
                    }

                    // Receive message from client
                    byte[] messageBuffer = new byte[ChatConstants.MESSAGE_MAXIMUM_SIZE];
                    int messageLength = await clientSocket.ReceiveAsync(messageBuffer, SocketFlags.None);

                    // Check if server is still running
                    if (!isRunning)
                    {
                        break;
                    }

                    // Convert message bytes to string
                    string messageContent = Encoding.UTF8.GetString(
                        messageBuffer,
                        ChatConstants.STARTING_INDEX,
                        messageLength);

                    // Ignore info change commands sent by clients
                    if (infoChangeCommandRegex.IsMatch(messageContent))
                    {
                        continue;
                    }

                    // Check for client disconnection
                    if (messageLength == ChatConstants.DISCONNECT_CODE)
                    {
                        // Handle disconnection differently for host vs regular users
                        if (IsHostIpAddress(clientIpAddress))
                        {
                            // Host disconnection shuts down the server
                            messageContent = "Host disconnected";
                            BroadcastMessageToAllClients(CreateMessage(messageContent, clientUsername));
                            ShutdownServer();
                        }
                        else
                        {
                            // Regular user disconnection
                            messageContent = "Disconnected";
                            BroadcastMessageToAllClients(CreateMessage(messageContent, clientUsername));
                            CheckAndStartTimeoutIfNeeded();
                        }

                        RemoveClientInformation(clientSocket, clientUsername, clientIpAddress);
                        break;
                    }

                    // Check if message is a command
                    bool isCommand = true;

                    if (muteCommandRegex.IsMatch(messageContent))
                    {
                        ProcessStatusChangeCommand(messageContent, ChatConstants.MUTE_STATUS, clientUsername, clientSocket, mutedUsers);
                    }
                    else if (adminCommandRegex.IsMatch(messageContent))
                    {
                        ProcessStatusChangeCommand(messageContent, ChatConstants.ADMIN_STATUS, clientUsername, clientSocket, adminUsers);
                    }
                    else if (kickCommandRegex.IsMatch(messageContent))
                    {
                        ProcessStatusChangeCommand(messageContent, ChatConstants.KICK_STATUS, clientUsername, clientSocket);
                    }
                    else
                    {
                        isCommand = false;
                    }

                    // If not a command, broadcast the message to all clients
                    if (!isCommand)
                    {
                        BroadcastMessageToAllClients(CreateMessage(messageContent, clientUsername));
                    }
                }
            }
            catch (Exception)
            {
                // Handle exceptions during client communication
            }
            finally
            {
                try
                {
                    clientSocket.Shutdown(SocketShutdown.Both);
                }
                catch (Exception)
                {
                    // Socket might already be closed
                }

                clientSocket.Close();
            }
        }

        /// <summary>
        /// Creates a message object from the provided content and sender name.
        /// </summary>
        /// <param name="messageContent">The content of the message.</param>
        /// <param name="senderUsername">The username of the message sender.</param>
        /// <returns>A new Message object.</returns>
        public Message CreateMessage(string messageContent, string senderUsername)
        {
            return new Message
            {
                MessageContent = messageContent,
                MessageDateTime = DateTime.Now.ToString(),
                MessageSenderName = senderUsername,
                MessageAligment = ChatConstants.ALIGNMENT_LEFT,
                MessageSenderStatus = GetHighestUserStatus(senderUsername)
            };
        }

        /// <summary>
        /// Broadcasts a message to all connected clients.
        /// </summary>
        /// <param name="message">The message to broadcast.</param>
        private void BroadcastMessageToAllClients(Message message)
        {
            foreach (KeyValuePair<Socket, string> socketAndIpAddress in socketsToIpAddresses)
            {
                try
                {
                    SendMessageToClient(message, socketAndIpAddress.Key);
                }
                catch (Exception)
                {
                    // Ignore exceptions for individual clients
                }
            }
        }

        /// <summary>
        /// Sends a message to a specific client.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="clientSocket">The socket of the client to send to.</param>
        private void SendMessageToClient(Message message, Socket clientSocket)
        {
            byte[] messageBytes = message.ToByteArray();
            _ = clientSocket.SendAsync(messageBytes, SocketFlags.None);
        }

        /// <summary>
        /// Checks if the given IP address belongs to the host.
        /// </summary>
        /// <param name="ipAddress">The IP address to check.</param>
        /// <returns>True if it is the host's IP address, false otherwise.</returns>
        private bool IsHostIpAddress(string ipAddress)
        {
            return ipAddress == ipEndPoint.Address.ToString();
        }

        /// <summary>
        /// Initializes the server timeout timer.
        /// </summary>
        private void InitializeServerTimeout()
        {
            lock (timerLock)
            {
                // Dispose of existing timer if any
                serverTimeoutTimer?.Dispose();

                // Create new timeout timer
                serverTimeoutTimer = new Timer(_ =>
                {
                    // Check if minimum connections requirement is still not met
                    if (socketsToIpAddresses.Count < ChatConstants.MINIMUM_CONNECTIONS_REQUIRED)
                    {
                        ShutdownServer();
                    }
                }, null, ChatConstants.SERVER_TIMEOUT_DURATION_MS, Timeout.Infinite);
            }
        }

        /// <summary>
        /// Shuts down the server.
        /// </summary>
        private void ShutdownServer()
        {
            try
            {
                if (serverSocket.Connected)
                {
                    serverSocket.Shutdown(SocketShutdown.Both);
                }
            }
            catch (Exception)
            {
                // Socket might already be closed
            }

            serverSocket.Close();
            isRunning = false;
        }

        /// <summary>
        /// Checks if the minimum connections requirement is met, and starts the timeout timer if not.
        /// </summary>
        private void CheckAndStartTimeoutIfNeeded()
        {
            if (socketsToIpAddresses.Count < ChatConstants.MINIMUM_CONNECTIONS_REQUIRED)
            {
                InitializeServerTimeout();
            }
        }

        /// <summary>
        /// Checks if the server is currently running.
        /// </summary>
        /// <returns>True if the server is running, false otherwise.</returns>
        public bool IsRunning()
        {
            return isRunning;
        }

        /// <summary>
        /// Gets the highest status of a user.
        /// </summary>
        /// <param name="username">The username of the user.</param>
        /// <returns>The highest status of the user.</returns>
        private string GetHighestUserStatus(string username)
        {
            if (hostUsername == username)
            {
                return ChatConstants.HOST_STATUS;
            }

            return adminUsers.GetValueOrDefault(username, false)
                ? ChatConstants.ADMIN_STATUS
                : ChatConstants.REGULAR_USER_STATUS;
        }

        /// <summary>
        /// Checks if a user is allowed to change the status of another user.
        /// </summary>
        /// <param name="requesterStatus">The status of the requesting user.</param>
        /// <param name="targetStatus">The status of the target user.</param>
        /// <returns>True if the status change is allowed, false otherwise.</returns>
        private bool CanChangeUserStatus(string requesterStatus, string targetStatus)
        {
            // Host can change status of any non-host user
            bool isHostChangingNonHost =
                requesterStatus == ChatConstants.HOST_STATUS &&
                targetStatus != ChatConstants.HOST_STATUS;

            // Admin can change status of regular users
            bool isAdminChangingRegularUser =
                requesterStatus == ChatConstants.ADMIN_STATUS &&
                targetStatus == ChatConstants.REGULAR_USER_STATUS;

            return isHostChangingNonHost || isAdminChangingRegularUser;
        }

        /// <summary>
        /// Extracts the target username from a command.
        /// </summary>
        /// <param name="command">The command to parse.</param>
        /// <returns>The target username.</returns>
        private string ExtractTargetUsernameFromCommand(string command)
        {
            // Command follows the pattern: <username>|Status|<username>
            int targetPartIndex = 2;
            char separator = '|';
            string targetPart = command.Split(separator)[targetPartIndex];

            // Extract username from <username>
            int nameStartIndex = 1;
            int nameLength = targetPart.Length - 2; // Remove < and >
            return targetPart.Substring(nameStartIndex, nameLength);
        }

        /// <summary>
        /// Processes a status change command.
        /// </summary>
        /// <param name="command">The command to process.</param>
        /// <param name="targetedStatus">The status to change.</param>
        /// <param name="requesterUsername">The username of the requesting user.</param>
        /// <param name="requesterSocket">The socket of the requesting user.</param>
        /// <param name="statusTracker">The dictionary tracking the status.</param>
        private void ProcessStatusChangeCommand(
            string command,
            string targetedStatus,
            string requesterUsername,
            Socket requesterSocket,
            ConcurrentDictionary<string, bool> statusTracker = null)
        {
            string targetUsername = ExtractTargetUsernameFromCommand(command);

            // Find target user's socket
            Socket targetSocket = null;
            foreach (KeyValuePair<string, Socket> usernameAndSocket in usernamesToSockets)
            {
                if (targetUsername == usernameAndSocket.Key)
                {
                    targetSocket = usernameAndSocket.Value;
                    break;
                }
            }

            // If target user not found, abort
            if (targetSocket == null)
            {
                return;
            }

            string targetIpAddress = socketsToIpAddresses.GetValueOrDefault(targetSocket) ?? "Not found";

            string requesterStatus = GetHighestUserStatus(requesterUsername);
            string targetStatus = GetHighestUserStatus(targetUsername);

            // Check if the requester is allowed to change the target's status
            if (CanChangeUserStatus(requesterStatus, targetStatus))
            {
                string statusChangeMessage;

                // Toggle mute status
                if (targetedStatus == ChatConstants.MUTE_STATUS)
                {
                    bool isNowMuted = statusTracker?.AddOrUpdate(targetUsername, false, (key, oldValue) => !oldValue) ?? false;
                    statusChangeMessage = isNowMuted
                        ? $"{targetUsername} has been muted"
                        : $"{targetUsername} has been unmuted";

                    SendMessageToClient(
                        CreateMessage(ChatConstants.INFO_CHANGE_MUTE_STATUS_COMMAND, targetUsername),
                        targetSocket);
                }
                // Toggle admin status
                else if (targetedStatus == ChatConstants.ADMIN_STATUS)
                {
                    bool isNowAdmin = statusTracker?.AddOrUpdate(targetUsername, false, (key, oldValue) => !oldValue) ?? false;
                    statusChangeMessage = isNowAdmin
                        ? $"{targetUsername} is now an admin"
                        : $"{targetUsername} is no longer an admin";

                    SendMessageToClient(
                        CreateMessage(ChatConstants.INFO_CHANGE_ADMIN_STATUS_COMMAND, targetUsername),
                        targetSocket);
                }
                else
                {
                    statusChangeMessage = $"{targetUsername} has been kicked";
                    SendMessageToClient(
                        CreateMessage(ChatConstants.INFO_CHANGE_KICK_STATUS_COMMAND, targetUsername),
                        targetSocket);

                    RemoveClientInformation(targetSocket, targetUsername, targetIpAddress);
                }

                // Broadcast the status change to all clients
                BroadcastMessageToAllClients(CreateMessage(statusChangeMessage, requesterUsername));
                return;
            }

            // Notify requester that the command was rejected
            SendMessageToClient(
                CreateMessage(ChatConstants.SERVER_REJECT_COMMAND, hostUsername),
                requesterSocket);
        }

        /// <summary>
        /// Removes client information from all tracking dictionaries.
        /// </summary>
        /// <param name="clientSocket">The socket of the client.</param>
        /// <param name="username">The username of the client.</param>
        /// <param name="ipAddress">The IP address of the client.</param>
        private void RemoveClientInformation(Socket clientSocket, string username, string ipAddress)
        {
            ipAddressesToUsernames.TryRemove(ipAddress, out _);
            socketsToIpAddresses.TryRemove(clientSocket, out _);
            usernamesToSockets.TryRemove(username, out _);
            adminUsers.TryRemove(username, out _);
            mutedUsers.TryRemove(username, out _);
        }
    }
}