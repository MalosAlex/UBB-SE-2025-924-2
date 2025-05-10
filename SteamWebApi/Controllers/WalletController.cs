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

        [HttpPost("add-money")]
        public IActionResult AddMoney(int userId, decimal amount)
        {
            walletService.AddMoney(amount);
            return Ok();
        }
    }
}
