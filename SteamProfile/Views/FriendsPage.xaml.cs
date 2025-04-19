using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SteamProfile.ViewModels;

namespace SteamProfile.Views
{
    public sealed partial class FriendsPage : Page
    {
        private readonly FriendsViewModel friendsViewModel;
        private readonly UsersViewModel usersViewModel;

        public FriendsPage()
        {
            InitializeComponent();
            friendsViewModel = App.FriendsViewModel;
            usersViewModel = App.UsersViewModel;
            DataContext = friendsViewModel;

            // Load friends immediately when page is created
            friendsViewModel.LoadFriends();
        }

        private void RemoveFriend_Click(object sender, RoutedEventArgs eventArgs)
        {
            if (sender is Button button && button.Tag is int friendshipId)
            {
                friendsViewModel.RemoveFriend(friendshipId);
            }
        }

        private void ViewFriend_Click(object sender, RoutedEventArgs eventArgs)
        {
            if (sender is Button button && button.Tag is int friendId)
            {
                Frame.Navigate(typeof(ProfilePage), friendId);
            }
        }

        private void BackToProfileButton_Click(object sender, RoutedEventArgs eventArgs)
        {
            Frame.Navigate(typeof(ProfilePage), usersViewModel.GetCurrentUser().UserId);
        }
    }
}