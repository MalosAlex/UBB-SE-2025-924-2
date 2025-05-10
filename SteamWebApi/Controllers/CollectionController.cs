using BusinessLayer.Models;
using BusinessLayer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace SteamWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CollectionController : ControllerBase
    {
        private readonly ICollectionsService collectionsService;

        public CollectionController(ICollectionsService collectionsService)
        {
            this.collectionsService = collectionsService;
        }

        [HttpGet("{userId}")]
        public IActionResult GetCollections(int userId)
        {
            var collections = collectionsService.GetAllCollections(userId);
            return Ok(collections);
        }

        [HttpPost]
        public IActionResult AddCollection(Collection collection)
        {
            collectionsService.CreateCollection(
                collection.UserId,
                collection.CollectionName,
                collection.CoverPicture ?? string.Empty,
                collection.IsPublic,
                collection.CreatedAt
            );
            return Ok();
        }
    }
}
