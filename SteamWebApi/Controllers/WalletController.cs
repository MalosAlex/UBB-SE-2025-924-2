using BusinessLayer.Models;
using BusinessLayer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace SteamWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService walletService;

        public WalletController(IWalletService walletService)
        {
            this.walletService = walletService;
        }

        [HttpGet("{userId}")]
        public IActionResult GetWallet(int userId)
        {
            var balance = walletService.GetBalance();
            var points = walletService.GetPoints();

            var wallet = new
            {
                UserId = userId,
                Balance = balance,
                Points = points
            };

            return Ok(wallet);
        }

        [HttpPost("create/{userId}")]
        public IActionResult CreateWallet(int userId)
        {
            walletService.CreateWallet(userId);
            return Ok();
        }

        [HttpPost("add-money")]
        public IActionResult AddMoney([FromBody] AddMoneyRequest request)
        {
            walletService.AddMoney(request.Amount);
            return Ok();
        }

        [HttpPost("add-points")]
        public IActionResult AddPoints([FromBody] AddPointsRequest request)
        {
            walletService.AddPoints(request.Points);
            return Ok();
        }

        [HttpPost("purchase-points/{userId}")]
        public IActionResult PurchasePoints(int userId, [FromBody] PointsOffer offer)
        {
            walletService.PurchasePoints(offer);
            return Ok();
        }

        [HttpPost("try-purchase-points")]
        public IActionResult TryPurchasePoints([FromBody] TryPurchaseRequest request)
        {
            // Find the points offer by ID
            // For the API, we'd need to get the offer by ID
            var offer = new PointsOffer(request.Price, request.Points)
            {
                OfferId = request.OfferId
            };

            bool success = walletService.TryPurchasePoints(offer);
            return Ok(success);
        }
    }

    public class AddMoneyRequest
    {
        public int UserId { get; set; }
        public decimal Amount { get; set; }
    }

    public class AddPointsRequest
    {
        public int UserId { get; set; }
        public int Points { get; set; }
    }

    public class TryPurchaseRequest
    {
        public int UserId { get; set; }
        public int OfferId { get; set; }

        public int Price { get; set; }
        public int Points { get; set; }
    }
}