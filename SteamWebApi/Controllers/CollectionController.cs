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

        [HttpGet("{collectionId}/user/{userId}")]
        public IActionResult GetCollection(int collectionId, int userId)
        {
            var collection = collectionsService.GetCollectionByIdentifier(collectionId, userId);
            return Ok(collection);
        }

        [HttpGet("{collectionId}/games")]
        public IActionResult GetGamesInCollection(int collectionId)
        {
            var games = collectionsService.GetGamesInCollection(collectionId);
            return Ok(games);
        }

        [HttpGet("public/{userId}")]
        public IActionResult GetPublicCollections(int userId)
        {
            var collections = collectionsService.GetPublicCollectionsForUser(userId);
            return Ok(collections);
        }

        [HttpGet("{collectionId}/user/{userId}/games-not-in-collection")]
        public IActionResult GetGamesNotInCollection(int collectionId, int userId)
        {
            var games = collectionsService.GetGamesNotInCollection(collectionId, userId);
            return Ok(games);
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

        [HttpPut("{collectionId}")]
        public IActionResult UpdateCollection(int collectionId, [FromBody] UpdateCollectionRequest request)
        {
            collectionsService.UpdateCollection(
                collectionId,
                request.UserId,
                request.CollectionName,
                request.CoverPicture ?? string.Empty,
                request.IsPublic
            );
            return Ok();
        }

        [HttpDelete("{collectionId}/user/{userId}")]
        public IActionResult DeleteCollection(int collectionId, int userId)
        {
            collectionsService.DeleteCollection(collectionId, userId);
            return Ok();
        }

        [HttpPost("add-game")]
        public IActionResult AddGameToCollection([FromBody] AddGameRequest request)
        {
            collectionsService.AddGameToCollection(
                request.CollectionId,
                request.GameId,
                request.UserId
            );
            return Ok();
        }

        [HttpPost("remove-game")]
        public IActionResult RemoveGameFromCollection([FromBody] RemoveGameRequest request)
        {
            collectionsService.RemoveGameFromCollection(
                request.CollectionId,
                request.GameId
            );
            return Ok();
        }
    }

    public class UpdateCollectionRequest
    {
        public int UserId { get; set; }
        public string CollectionName { get; set; }
        public string CoverPicture { get; set; }
        public bool IsPublic { get; set; }
    }

    public class AddGameRequest
    {
        public int CollectionId { get; set; }
        public int GameId { get; set; }
        public int UserId { get; set; }
    }

    public class RemoveGameRequest
    {
        public int CollectionId { get; set; }
        public int GameId { get; set; }
    }
}