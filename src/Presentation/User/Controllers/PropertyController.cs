using FinalProject.src.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FinalProject.src.Application.Interfaces;
using FinalProject.src.Application.DTOs;
using FinalProject.src.Infrastructure.Data;

namespace FinalProject.src.Presentation.User.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class PropertyController : ControllerBase
    {
        private readonly IPropertyService _propertyService;
        private readonly ApllicationDbContext _dbContext;


        public PropertyController(IPropertyService propertyService, ApllicationDbContext dbContext)
        {
            _propertyService = propertyService;
            _dbContext = dbContext;
        }
        [Authorize(Roles = "User")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateProperty([FromForm] PropertyDTO propertyDTO)
        {
            var ownerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
               ?? User.FindFirst("uid")?.Value;


            Console.WriteLine($"Extracted OwnerId: {ownerId}");


            if (string.IsNullOrEmpty(ownerId))
            {
                return Unauthorized("Owner ID is missing or invalid.");
            }

            propertyDTO.OwnerId = ownerId;

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _propertyService.CreatePropertyAsync(propertyDTO);
            return CreatedAtAction(nameof(GetPropertyById), new { id = result.PropertyId }, result);
        }
        [Authorize(Roles = "User")]
        [HttpPost("pending")]
        public async Task<IActionResult> CreatePending([FromForm] PropertyDTO dto)
        {
            try
            {
                var paypalurl = await _propertyService.CreatePendingPropertyAsync(dto);
                return Ok(new { paypalurl });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [Authorize(Roles = "User")]
        [HttpPost("confirm")]
        public async Task<IActionResult> ConfirmPayment([FromQuery] string session_id)
        {
            try
            {
                var result = await _propertyService.ConfirmPaymentAsync(session_id);
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("filter")]
        public async Task<IActionResult> FilterProperties([FromQuery] PropertyFilterDTO filter)
        {
            var results = await _propertyService.FilterPropertiesAsync(filter);
            return Ok(results);
        }

        [Authorize(Roles = "User")]
        [HttpGet("pending")]
        public async Task<IActionResult> GetAllPending()
        {
            var pendingList = await _propertyService.GetAllPendingPropertiesAsync();
            return Ok(pendingList);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProperties()
        {
            return Ok(await _propertyService.GetAllPropertiesAsync());
        }
        [Authorize(Roles = "User")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPropertyById(int id)
        {
            var property = await _propertyService.GetPropertyByIdAsync(id);
            return property == null ? NotFound() : Ok(property);
        }
        [Authorize(Roles = "User")]
        [HttpPost("{id}/add-images")]
        public async Task<IActionResult> AddImagesToProperty(int id, [FromForm] List<IFormFile> images)
        {
            bool result = await _propertyService.AddImagesToPropertyAsync(id, images);
            return result ? Ok("تمت إضافة الصور بنجاح.") : NotFound("الشقة غير موجودة.");
        }
        [Authorize(Roles = "User")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProperty(int id, [FromBody] PropertyDTO propertyDTO)
        {
            bool result = await _propertyService.UpdatePropertyAsync(id, propertyDTO);
            return result ? Ok("تم التحديث بنجاح.") : NotFound("الشقة غير موجودة.");
        }
        [Authorize(Roles = "User")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProperty(int id)
        {
            var result = await _propertyService.DeletePropertyAsync(id);

            if (!result)
                return NotFound("Property not found.");

            return NoContent(); // 204
        }


        [Authorize(Roles = "User")]
        [HttpGet("location/{location}")]
        public async Task<IActionResult> GetPropertiesByLocation(string location)
        {
            return Ok(await _propertyService.GetPropertiesByLocationAsync(location));
        }
        [Authorize(Roles = "User")]
        [HttpGet("price")]
        public async Task<IActionResult> GetPropertiesByPrice([FromQuery] decimal minPrice, [FromQuery] decimal maxPrice)
        {
            return Ok(await _propertyService.GetPropertiesByPriceAsync(minPrice, maxPrice));
        }
        [Authorize(Roles = "User")]
        [HttpGet("date")]
        public async Task<IActionResult> GetPropertiesByDateRange([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            return Ok(await _propertyService.GetPropertiesByDateRangeAsync(from, to));
        }
        [Authorize(Roles = "User")]
        [HttpPost("{id}/upload-images")]
        public async Task<IActionResult> UploadPropertyImages(int id, [FromForm] List<IFormFile> images)
        {
            var result = await _propertyService.AddImagesToPropertyAsync(id, images);
            return result ? Ok("Uploade Success!") : NotFound("Property Not Found !");
        }
        [Authorize(Roles = "User")]
        [HttpGet("nearest")]
        public async Task<IActionResult> GetNearestProperties(double lat, double lon, int maxResults = 10)
        {
            var properties = await _propertyService.GetNearestPropertiesAsync(lat, lon, maxResults);
            return Ok(properties);
        }


    }
}

