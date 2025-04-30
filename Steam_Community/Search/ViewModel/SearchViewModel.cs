using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Steam_Community.DirectMessages.Views;
using Microsoft.UI.Xaml;
using System.Linq;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;

namespace Search
{
    public partial class SearchViewModel : ObservableObject
    {
        private readonly IService service;

        [ObservableProperty]
        private User currentUser;

        [ObservableProperty]
        private ObservableCollection<User> displayedUsers = new();

        [ObservableProperty]
        private ObservableCollection<User> chatInvitesFromUsers = new();

        [ObservableProperty]
        private ObservableCollection<User> friendRequestsFromUsers = new();

        [ObservableProperty]
        private string searchText;

        [ObservableProperty]
        private Visibility noUsersFoundMessage = Visibility.Collapsed;

        public const string SEND_MESSAGE_REQUEST_CONTENT = "Invite to Chat";
        public const string CANCEL_MESSAGE_REQUEST_CONTENT = "Cancel Invite";

        public IRelayCommand SearchCommand { get; }
        public IRelayCommand SortAscendingCommand { get; }
        public IRelayCommand SortDescendingCommand { get; }
        public IRelayCommand RefreshChatInvitesCommand { get; }

        public event EventHandler<ChatRoomOpenedEventArgs>? ChatRoomOpened;

        private bool isHosting = false;

        public SearchViewModel(IService service)
        {
            this.service = service;
            this.currentUser = new User(1, "JaneSmith", Steam_Community.DirectMessages.Models.ChatConstants.GET_IP_REPLACER);
            this.currentUser.IpAddress = service.UpdateCurrentUserIpAddress(currentUser.Id);

            SearchCommand = new RelayCommand(Search);
            SortAscendingCommand = new RelayCommand(SortAscending);
            SortDescendingCommand = new RelayCommand(SortDescending);
            RefreshChatInvitesCommand = new RelayCommand(FillInvites);

            FillInvites();
        }

        public void Search()
        {
            var foundUsers = service.GetFirst10UsersMatchedSorted(SearchText);
            DisplayedUsers.Clear();
            foreach (var user in foundUsers)
                DisplayedUsers.Add(user);

            NoUsersFoundMessage = DisplayedUsers.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public void SortAscending()
        {
            var sorted = service.SortAscending(DisplayedUsers.ToList());
            DisplayedUsers.Clear();
            foreach (var user in sorted)
                DisplayedUsers.Add(user);
            NoUsersFoundMessage = DisplayedUsers.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public void SortDescending()
        {
            var sorted = service.SortDescending(DisplayedUsers.ToList());
            DisplayedUsers.Clear();
            foreach (var user in sorted)
                DisplayedUsers.Add(user);
            NoUsersFoundMessage = DisplayedUsers.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public void FillInvites()
        {
            var senders = service.GetUsersWhoSentMessageRequest(CurrentUser.Id);
            ChatInvitesFromUsers.Clear();
            foreach (var user in senders)
                ChatInvitesFromUsers.Add(user);
        }

        public void HandleMessage(User user, Button button)
        {
            var result = service.MessageRequest(CurrentUser.Id, user.Id);

            switch (result)
            {
                case Service.MESSAGE_REQUEST_FOUND:
                    button.Content = SEND_MESSAGE_REQUEST_CONTENT;
                    return;
                case Service.MESSAGE_REQUEST_NOT_FOUND:
                    button.Content = CANCEL_MESSAGE_REQUEST_CONTENT;
                    break;
            }

            if (!isHosting)
            {
                ChatRoomOpened?.Invoke(this, new ChatRoomOpenedEventArgs
                {
                    Username = CurrentUser.UserName,
                    IpAddress = Steam_Community.DirectMessages.Models.ChatConstants.GET_IP_REPLACER
                });
                isHosting = true; // set to false
            }
        }

        public void AcceptInvite(User user)
        {
            ChatRoomOpened?.Invoke(this, new ChatRoomOpenedEventArgs
            {
                Username = CurrentUser.UserName,
                IpAddress = user.IpAddress
            });

            service.HandleMessageAcceptOrDecline(user.Id, CurrentUser.Id);
            ChatInvitesFromUsers.Remove(user);
        }

        public void DeclineInvite(User user)
        {
            service.HandleMessageAcceptOrDecline(user.Id, CurrentUser.Id);
            ChatInvitesFromUsers.Remove(user);
        }

        public void ToggleFriendRequest(User user, Button button)
        {
            switch (user.FriendshipStatus)
            {
                case FriendshipStatus.NotFriends:
                    user.FriendshipStatus = FriendshipStatus.RequestSent;
                    button.Content = "Cancel Request";
                    break;
                case FriendshipStatus.RequestSent:
                    user.FriendshipStatus = FriendshipStatus.NotFriends;
                    button.Content = "Add Friend";
                    break;
                case FriendshipStatus.RequestReceived:
                case FriendshipStatus.Friends:
                    break;
            }

            service.ToggleFriendRequest(user.FriendshipStatus, CurrentUser.Id, user.Id);
        }

        public void OnClosing()
        {
            service.OnCloseWindow(CurrentUser.Id);
        }

        public void StoppedHosting()
        {
            isHosting = false;
        }
    }
}
