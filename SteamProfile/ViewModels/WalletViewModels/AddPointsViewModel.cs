using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using BusinessLayer.Models;
using BusinessLayer.Repositories;
using BusinessLayer.Repositories.Interfaces;

namespace SteamProfile.ViewModels
{
    public partial class AddPointsViewModel : ObservableObject
    {
        private readonly WalletViewModel walletViewModel;
        private readonly IPointsOffersRepository offersRepository;
        private readonly Frame navigationFrame;

        [ObservableProperty]
        private int userPoints;

        [ObservableProperty]
        private bool isProcessing;

        public ObservableCollection<PointsOffer> PointsOffers { get; }
        public ICommand PurchasePointsCommand { get; }

        public AddPointsViewModel(WalletViewModel walletViewModel, IPointsOffersRepository offersRepository, Frame navigationFrame)
        {
            this.walletViewModel = walletViewModel ?? throw new ArgumentNullException(nameof(walletViewModel));
            this.offersRepository = offersRepository ?? throw new ArgumentNullException(nameof(offersRepository));
            this.navigationFrame = navigationFrame ?? throw new ArgumentNullException(nameof(navigationFrame));

            PointsOffers = new ObservableCollection<PointsOffer>(this.offersRepository.PointsOffers);
            UserPoints = this.walletViewModel.Points;
            IsProcessing = false;

            PurchasePointsCommand = new RelayCommand<PointsOffer>(BuyPoints, CanBuyPoints);
        }

        private bool CanBuyPoints(PointsOffer pointsOffer)
        {
            return pointsOffer != null && !IsProcessing;
        }

        private async void BuyPoints(PointsOffer pointsOffer)
        {
            if (pointsOffer == null)
            {
                return;
            }
            IsProcessing = true;

            try
            {
                bool success = await walletViewModel.PurchasePoints(pointsOffer);
                if (success)
                {
                    UserPoints = walletViewModel.Points;
                    walletViewModel.RefreshWalletData();
                    await ShowSuccessMessageAsync(pointsOffer);
                    NavigateBack();
                }
                else
                {
                    await ShowErrorMessageAsync("Insufficient funds to purchase these points.");
                }
            }
            catch (Exception exception)
            {
                await ShowErrorMessageAsync($"An error occurred: {exception.Message}");
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private async Task ShowSuccessMessageAsync(PointsOffer pointsOffer)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Points Added",
                Content = $"Successfully added {pointsOffer.Points} points to your wallet!",
                CloseButtonText = "OK",
                XamlRoot = navigationFrame.XamlRoot
            };

            await dialog.ShowAsync();
        }

        private async Task ShowErrorMessageAsync(string message)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Error",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = navigationFrame.XamlRoot
            };

            await dialog.ShowAsync();
        }

        private void NavigateBack()
        {
            if (navigationFrame.CanGoBack)
            {
                navigationFrame.GoBack();
            }
        }

        public void RefreshPoints()
        {
            UserPoints = walletViewModel.Points;
        }
    }
}