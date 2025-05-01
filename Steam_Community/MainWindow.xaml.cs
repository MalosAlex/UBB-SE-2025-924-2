using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Steam_Community.DirectMessages.Views;
using Steam_Community.DirectMessages.Models;
using Search;

namespace Steam_Community
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            this.searchPage.ChatRoomOpened += HandleChatInvite;
            this.Closed += this.searchPage.OnClosing;
        }

        public void HandleChatInvite(object sender, ChatRoomOpenedEventArgs e)
        {
            ChatRoomWindow chatRoomWindow = new ChatRoomWindow(e.Username, e.IpAddress);
            if (e.IpAddress == ChatConstants.HOST_IP_FINDER)
            {
                chatRoomWindow.Closed += this.searchPage.StoppedHosting;
            }
            chatRoomWindow.Activate();
        }
    }
}