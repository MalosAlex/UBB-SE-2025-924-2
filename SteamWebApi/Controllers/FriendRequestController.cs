using BusinessLayer.Models;
using BusinessLayer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace SteamWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FriendRequestController : ControllerBase
    {
        private readonly IFriendRequestService friendRequestService;

        public FriendRequestController(IFriendRequestService friendRequestService)
        {
            this.friendRequestService = friendRequestService;
        }

        [HttpGet]
        public async Task<IActionResult> GetFriendRequests(string username)
        {
            var requests = await friendRequestService.GetFriendRequestsAsync(username);
            return Ok(requests);
        }

        [HttpPost]
        public async Task<IActionResult> SendFriendRequest(FriendRequest request)
        {
            var result = await friendRequestService.SendFriendRequestAsync(request);
            if (result)
            {
                return Ok();
            }
            return BadRequest("Failed to send friend request.");
        }
    }
}
