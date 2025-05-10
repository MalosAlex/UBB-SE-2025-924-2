using BusinessLayer.Models;
using BusinessLayer.Services;
using Microsoft.AspNetCore.Mvc;

namespace SteamWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService reviewService;

        public ReviewController(IReviewService reviewService)
        {
            this.reviewService = reviewService;
        }

        [HttpGet("{userId}")]
        public IActionResult GetReviews(int userId)
        {
            var allReviews = reviewService.GetAllReviewsForAGame(0);
            var userReviews = allReviews.Where(review => review.UserIdentifier == userId).ToList();
            return Ok(userReviews);
        }

        [HttpPost]
        public IActionResult AddReview(Review review)
        {
            var result = reviewService.SubmitReview(review);
            if (result)
            {
                return Ok();
            }
            return BadRequest("Failed to add review.");
        }
    }
}
