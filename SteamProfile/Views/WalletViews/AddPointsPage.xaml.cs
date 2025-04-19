using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using BusinessLayer.Repositories;
using SteamProfile.ViewModels;
using BusinessLayer.Repositories.Interfaces;

namespace SteamProfile.Views.WalletViews
{
    public sealed partial class AddPointsPage : Page
    {
        private AddPointsViewModel addPointsViewModel;

        public AddPointsPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs eventArgs)
        {
            base.OnNavigatedTo(eventArgs);

            if (eventArgs.Parameter is WalletViewModel walletViewModel)
            {
                var repository = new PointsOffersRepository();
                addPointsViewModel = new AddPointsViewModel(walletViewModel, repository, this.Frame);
                this.DataContext = addPointsViewModel;
            }
            else
            {
                ShowErrorAndGoBack("Navigation error: Wallet data not found.");
            }
        }

        private async void ShowErrorAndGoBack(string message)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Error",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();

            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private void CancelAddPoints(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }
    }
}