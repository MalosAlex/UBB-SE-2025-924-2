using BusinessLayer.Models;
using BusinessLayer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace SteamWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeatureController : ControllerBase
    {
        private readonly IFeaturesService featuresService;

        public FeatureController(IFeaturesService featuresService)
        {
            this.featuresService = featuresService;
        }

        [HttpGet]
        public IActionResult GetFeatures()
        {
            var featuresByCategories = featuresService.GetFeaturesByCategories();
            return Ok(featuresByCategories);
        }

        [HttpPost]
        public IActionResult AddFeature(Feature feature)
        {
            return BadRequest("AddFeature method is not implemented in IFeaturesService.");
        }
    }
}
