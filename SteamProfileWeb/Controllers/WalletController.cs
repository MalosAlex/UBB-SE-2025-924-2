using Microsoft.AspNetCore.Mvc;
using BusinessLayer.Services.Interfaces;
using BusinessLayer.Repositories.Interfaces;
using SteamProfileWeb.ViewModels;
using System;
using Microsoft.AspNetCore.Authorization; // Added this line

namespace SteamProfileWeb.Controllers
{
    [Authorize] // Added this attribute
    public class WalletController : Controller
    {
        private readonly IWalletService _walletService;
        private readonly IPointsOffersRepository _pointsOffersRepository;

        public WalletController(IWalletService walletService, IPointsOffersRepository pointsOffersRepository)
        {
            _walletService = walletService ?? throw new ArgumentNullException(nameof(walletService));
            _pointsOffersRepository = pointsOffersRepository ?? throw new ArgumentNullException(nameof(pointsOffersRepository));
        }

        public IActionResult Index()
        {
            // Note: User context (e.g., User ID) might be needed for _walletService methods.
            // This would typically be retrieved from HttpContext.User and passed to the service
            // or the ViewModel. For now, we assume the services can handle this or it's a general view.
            var viewModel = new WalletViewModel(_walletService, _pointsOffersRepository);
            viewModel.RefreshWalletData(); // Populate data
            return View(viewModel);
        }

        // Placeholder for AddFunds action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddFunds(decimal amount) // Amount would come from a form
        {
            if (ModelState.IsValid && amount > 0)
            {
                // Again, user context is crucial here.
                // _walletService.AddMoney(userId, amount);
                // For now, let's assume the ViewModel's AddFunds handles it or it's simplified
                var viewModel = new WalletViewModel(_walletService, _pointsOffersRepository); // Re-create or retrieve
                viewModel.AddFunds(amount); // This method in VM also needs user context awareness
                TempData["SuccessMessage"] = "Funds added successfully!"; // Example feedback
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = "Invalid amount.";
            return RedirectToAction(nameof(Index)); // Or return to a specific view with error
        }

        // Placeholder for PurchasePoints action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PurchasePoints(int pointsOfferId) // ID would come from a form
        {
            // Fetch the PointsOffer object based on ID from _pointsOffersRepository
            // var pointsOffer = _pointsOffersRepository.GetById(pointsOfferId); // Assuming such a method exists
            // if (pointsOffer == null) return NotFound();

            // For demonstration, assuming pointsOffer is passed directly or reconstructed.
            // This is a simplification. In a real app, you'd likely pass an ID and fetch the offer.
            // var viewModel = new WalletViewModel(_walletService, _pointsOffersRepository);
            // bool success = await viewModel.PurchasePoints(pointsOffer);

            // This part needs a proper PointsOffer instance.
            // The original WPF app might pass the whole object. Web apps usually pass IDs.
            // For now, this action is highly conceptual.
            // bool success = _walletService.TryPurchasePoints(pointsOffer); // Needs user context and offer

            // if (success)
            // {
            //     TempData["SuccessMessage"] = "Points purchased successfully!";
            // }
            // else
            // {
            //     TempData["ErrorMessage"] = "Failed to purchase points.";
            // }
            return RedirectToAction(nameof(Index));
        }
    }
}