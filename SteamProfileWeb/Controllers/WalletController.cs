using Microsoft.AspNetCore.Mvc;
using BusinessLayer.Services.Interfaces;
using BusinessLayer.Repositories.Interfaces;
using SteamProfileWeb.ViewModels;
using System;
using Microsoft.AspNetCore.Authorization;

namespace SteamProfileWeb.Controllers
{
    [Authorize]
    public class WalletController : Controller
    {
        private readonly IWalletService _walletService;

        public WalletController(IWalletService walletService)
        {
            _walletService = walletService ?? throw new ArgumentNullException(nameof(walletService));
        }

        public IActionResult Index()
        {
            var viewModel = new WalletViewModel(_walletService);
            viewModel.RefreshWalletData();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddFunds(WalletViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                if (viewModel.AmountToAdd.HasValue)
                {
                    _walletService.AddMoney(viewModel.AmountToAdd.Value);
                    TempData["SuccessMessage"] = $"Successfully added ${viewModel.AmountToAdd:F2} to your wallet using {viewModel.SelectedPaymentMethod}.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["AddFundsError"] = "Amount to add cannot be null.";
                }
            }

            TempData["AddFundsError"] = "Please correct the errors below.";
            var freshViewModel = new WalletViewModel(_walletService);
            freshViewModel.RefreshWalletData();
            freshViewModel.AmountToAdd = viewModel.AmountToAdd;
            freshViewModel.SelectedPaymentMethod = viewModel.SelectedPaymentMethod;
            freshViewModel.CardNumber = viewModel.CardNumber;
            freshViewModel.ExpiryDate = viewModel.ExpiryDate;
            freshViewModel.CVV = viewModel.CVV;
            freshViewModel.PayPalEmail = viewModel.PayPalEmail;

            return View("Index", freshViewModel);
        }

    }
}