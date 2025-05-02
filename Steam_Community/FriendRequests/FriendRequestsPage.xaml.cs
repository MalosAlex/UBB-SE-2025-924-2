using System;
using App1.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace FriendRequests
{
    public sealed partial class FriendRequestsPage : Page
    {
        public ProfileViewModel ProfileViewModel { get; }
        public FriendRequestViewModel FriendRequestViewModel { get; }

        public FriendRequestsPage()
        {
            // Get view models from DI container (same as in ProfileControl)
            FriendRequestViewModel = Steam_Community.App.GetService<FriendRequestViewModel>();

            // Create profile view model (it will use FriendRequestViewModel internally)
            ProfileViewModel = new App1.ViewModels.ProfileViewModel();

            InitializeComponent();
        }
    }
}