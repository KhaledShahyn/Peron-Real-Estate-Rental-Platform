using FinalProject.src.Application.DTOs;
using FinalProject.src.Application.Interfaces;
using FinalProject.src.Domain.Entities;
using FinalProject.src.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Claims;
using System.Threading.Tasks;
using Stripe;
using Stripe.Checkout;
using GenerativeAI;


namespace FinalProject.src.Infrastructure.Services
{
    public class PropertyService : IPropertyService
    {
        private readonly ApllicationDbContext _context;
        private readonly string _imageUploadPath = "wwwroot/images/";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly IAuthService _authService;
        private readonly INotificationService _notificationService;
        private readonly IMapServices _mapServices;
        private readonly IStripeService _stripeService;
        private readonly IPaymobService _paymobService;

        private readonly IWebHostEnvironment _env;
        public PropertyService(ApllicationDbContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, IAuthService authService, IMapServices mapServices, INotificationService notificationService, IStripeService stripeService, IWebHostEnvironment env, IPaymobService paymobService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _authService = authService;
            _mapServices = mapServices;
            _notificationService = notificationService;
            _stripeService = stripeService;
            _env = env;
            _paymobService = paymobService;
        }

        public async Task<PropertyResponseDTO> CreatePropertyAsync(PropertyDTO propertyDTO)
        {
            
            var user = await _authService.ValidateUserAsync();

            var property = new Property
            {
                Title = propertyDTO.Title,
                Location = propertyDTO.Location,
                Price = propertyDTO.Price,
                OwnerId = user.Id,
                Bedrooms = propertyDTO.Bedrooms,
                Bathrooms = propertyDTO.Bathrooms,
                HasInternet = propertyDTO.HasInternet,
                AllowsPets = propertyDTO.AllowsPets,
                SmokingAllowed = propertyDTO.SmokingAllowed,
                AvailableFrom = propertyDTO.AvailableFrom,
                AvailableTo = propertyDTO.AvailableTo,
                Description = propertyDTO.Description,
                RentType = propertyDTO.RentType,
                MinBookingDays = propertyDTO.MinBookingDays,
                HasBalcony = propertyDTO.HasBalcony,
                HasElevator = propertyDTO.HasElevator,
                HasSecurity = propertyDTO.HasSecurity,
                Floor = propertyDTO.Floor,
                //Rating = propertyDTO.Rating,
                IsFurnished = propertyDTO.IsFurnished,
                Area = propertyDTO.Area,
                Longitude = propertyDTO.Longitude,
                Latitude = propertyDTO.Latitude,
                Images = new List<PropertyImage>()
            };

            if (propertyDTO.Images != null && propertyDTO.Images.Count > 0)
            {
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                foreach (var formFile in propertyDTO.Images)
                {
                    if (formFile.Length > 0)
                    {
                        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(formFile.FileName)}";
                        var filePath = Path.Combine(uploadPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await formFile.CopyToAsync(stream);
                        }

                        property.Images.Add(new PropertyImage
                        {
                            ImageUrl = $"/images/{fileName}",
                            Caption = "Property Image"
                        });
                    }
                }
            }

            _context.Properties.Add(property);
            await _context.SaveChangesAsync();
            //await _notificationService.CreateNotificationAsync("تم رفع الشقة بنجاح!");

            var baseUrl = $"{_httpContextAccessor.HttpContext?.Request.Scheme}://{_httpContextAccessor.HttpContext?.Request.Host}";
            var response = new PropertyResponseDTO
            {
                PropertyId = property.PropertyId,
                Title = property.Title,
                Location = property.Location,
                Price = property.Price,
                Images = property.Images.Select(img => $"{baseUrl}{img.ImageUrl}").ToList(),
                Longitude = property.Longitude,
                Latitude = property.Latitude,
            };

            return response;
        }

        public async Task<string> CreatePendingPropertyAsync(PropertyDTO propertyDTO)
        {
           var user = await _authService.ValidateUserAsync();

            var pending = new PendingProperty
            {
                Title = propertyDTO.Title,
                Location = propertyDTO.Location,
                Price = propertyDTO.Price,
                OwnerId = user.Id,
                Bedrooms = propertyDTO.Bedrooms,
                Bathrooms = propertyDTO.Bathrooms,
                HasInternet = propertyDTO.HasInternet,
                AllowsPets = propertyDTO.AllowsPets,
                SmokingAllowed = propertyDTO.SmokingAllowed,
                AvailableFrom = propertyDTO.AvailableFrom,
                AvailableTo = propertyDTO.AvailableTo,
                Description = propertyDTO.Description,
                RentType = propertyDTO.RentType,
                MinBookingDays = propertyDTO.MinBookingDays,
                HasBalcony = propertyDTO.HasBalcony,
                HasElevator = propertyDTO.HasElevator,
                HasSecurity = propertyDTO.HasSecurity,
                Floor = propertyDTO.Floor,
                //Rating = propertyDTO.Rating,
                IsFurnished = propertyDTO.IsFurnished,
                Area = propertyDTO.Area,
                Longitude = propertyDTO.Longitude,
                Latitude = propertyDTO.Latitude,
                Images = new List<PendingPropertyImage>()
            };

            var uploadPath = Path.Combine(_env.WebRootPath, "images");
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            if (propertyDTO.Images != null)
            {
                foreach (var file in propertyDTO.Images)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                    var filePath = Path.Combine(uploadPath, fileName);
                    using var stream = new FileStream(filePath, FileMode.Create);
                    await file.CopyToAsync(stream);

                    pending.Images.Add(new PendingPropertyImage
                    {
                        ImageUrl = $"/images/{fileName}"
                    });
                }
            }
          
            _context.pendingProperties.Add(pending);
            await _context.SaveChangesAsync();
            var booking = new Booking
            {
                UserId = user.Id,
                PendingPropertyId = pending.PropertyId, // ✅ Use correct FK
                StartDate = propertyDTO.AvailableFrom,
                EndDate = propertyDTO.AvailableTo,
                TotalPrice = propertyDTO.Price,
                Status = "Pending"
            };
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
            
            await _notificationService.CreateNotificationAsync(
                 "تم تعليق إعلان {0}",
                 "الإعلان الموجود في {0} تم تعليقه مؤقتًا بسبب عدم الدفع.",
                 pending.Title,
                 pending.Location
             );

            await _context.SaveChangesAsync();

            var paymobUrl = await _stripeService.CreateCheckoutSessionAsync(pending);
            return paymobUrl;

        }
        public async Task<string> ConfirmPaymentAsync(string session_id)
        {
            var user = await _authService.ValidateUserAsync();
            var sessionService = new SessionService();
            var session = await sessionService.GetAsync(session_id);

            if (session.PaymentStatus != "paid")
                return "الدفع لم يتم بنجاح.";
            if (!session.Metadata.TryGetValue("pendingPropertyId", out var pendingIdStr) ||
                !int.TryParse(pendingIdStr, out int pendingPropertyId))
            {
                return "بيانات الدفع غير صحيحة.";
            }
            var pending = await _context.pendingProperties
                .Include(p => p.Images)
                .Include(p => p.Booking)
                .FirstOrDefaultAsync(p => p.PropertyId == pendingPropertyId);

            if (pending == null)
                return "العقار غير موجود.";

            if (pending.IsPaid)
                return "تم التأكيد بالفعل لهذا العقار.";
            var property = new Property
            {
                Title = pending.Title,
                Location = pending.Location,
                Price = pending.Price,
                Description = pending.Description,
                Bedrooms = pending.Bedrooms,
                Bathrooms = pending.Bathrooms,
                OwnerId = pending.OwnerId,
                CreatedAt = DateTime.UtcNow,
                HasInternet = pending.HasInternet,
                SmokingAllowed = pending.SmokingAllowed,
                AllowsPets = pending.AllowsPets,
                MinBookingDays = pending.MinBookingDays,
                Area = pending.Area,
                AvailableFrom = pending.AvailableFrom,
                AvailableTo = pending.AvailableTo,
                HasSecurity = pending.HasSecurity,
                Floor = pending.Floor,
                Longitude = pending.Longitude,
                Latitude = pending.Latitude,
                HasBalcony = pending.HasBalcony,
                HasElevator = pending.HasElevator,
                RentType = pending.RentType,
                Images = pending.Images.Select(img => new PropertyImage
                {
                    ImageUrl = img.ImageUrl,
                    Caption = "Uploaded"
                }).ToList()
            };

            _context.Properties.Add(property);
            await _context.SaveChangesAsync();
            if (pending.Booking != null)
            {
                pending.Booking.PropertyId = property.PropertyId;
                pending.Booking.Status = "Accepted";
                _context.Bookings.Update(pending.Booking);
            }
            pending.IsPaid = true;
            _context.pendingProperties.Update(pending);

            await _context.SaveChangesAsync();
            await _notificationService.CreateNotificationAsync(
                $"اعلانك {property.Title} اصبح متاحا",
                $"تم الدفع بنجاح والموافقة على اعلان {property.Title}",
                property.Title,
                property.Location
            );

            return "تم تأكيد الدفع ونشر العقار بنجاح.";
        }


        public async Task<List<PropertyResponseDTO>> FilterPropertiesAsync(PropertyFilterDTO filter)
        {
            var query = _context.Properties.AsQueryable();

            if (!string.IsNullOrEmpty(filter.Location))
                query = query.Where(p => p.Location.Contains(filter.Location));

            if (filter.MinPrice.HasValue)
                query = query.Where(p => p.Price >= filter.MinPrice.Value);

            if (filter.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= filter.MaxPrice.Value);

            if (!string.IsNullOrEmpty(filter.RentType))
                query = query.Where(p => p.RentType == filter.RentType);

            if (filter.Area.HasValue)
                query = query.Where(p => p.Area >= filter.Area.Value);

            if (filter.Bedrooms.HasValue)
                query = query.Where(p => p.Bedrooms >= filter.Bedrooms.Value);

            if (filter.Bathrooms.HasValue)
                query = query.Where(p => p.Bathrooms >= filter.Bathrooms.Value);

            if (filter.Floor.HasValue)
                query = query.Where(p => p.Floor == filter.Floor.Value);

            if (filter.IsFurnished.HasValue)
                query = query.Where(p => p.IsFurnished == filter.IsFurnished.Value);

            if (filter.HasBalcony.HasValue)
                query = query.Where(p => p.HasBalcony == filter.HasBalcony.Value);

            if (filter.HasInternet.HasValue)
                query = query.Where(p => p.HasInternet == filter.HasInternet.Value);

            if (filter.HasSecurity.HasValue)
                query = query.Where(p => p.HasSecurity == filter.HasSecurity.Value);

            if (filter.HasElevator.HasValue)
                query = query.Where(p => p.HasElevator == filter.HasElevator.Value);

            if (filter.AllowsPets.HasValue)
                query = query.Where(p => p.AllowsPets == filter.AllowsPets.Value);

            if (filter.SmokingAllowed.HasValue)
                query = query.Where(p => p.SmokingAllowed == filter.SmokingAllowed.Value);

            if (filter.MinBookingDays.HasValue)
                query = query.Where(p => p.MinBookingDays <= filter.MinBookingDays.Value);

            if (filter.AvailableFrom.HasValue)
                query = query.Where(p => p.AvailableFrom <= filter.AvailableFrom.Value);

            if (filter.AvailableTo.HasValue)
                query = query.Where(p => p.AvailableTo >= filter.AvailableTo.Value);

            var baseUrl = $"{_httpContextAccessor.HttpContext?.Request.Scheme}://{_httpContextAccessor.HttpContext?.Request.Host}";

            var properties = await query.ToListAsync();

            return properties.Select(p => new PropertyResponseDTO
            {
                PropertyId = p.PropertyId,
                Title = p.Title,
                Location = p.Location,
                Price = p.Price,
                Longitude = p.Longitude,
                Latitude = p.Latitude,
                Area = p.Area,  
                HasBalcony = p.HasBalcony,
                HasElevator = p.HasElevator,
                HasInternet = p.HasInternet,
                HasSecurity = p.HasSecurity,
                AllowsPets = p.AllowsPets,
                SmokingAllowed=p.SmokingAllowed,
                Floor=p.Floor,
                Bathrooms=p.Bathrooms,
                Bedrooms=p.Bedrooms,
                Description=p.Description,
                ratings=p.ratings,
                RentType=p.RentType,    
                OwnerId=p.OwnerId,
                AvailableFrom = p.AvailableFrom,    
                AvailableTo = p.AvailableTo,
                IsFurnished = p.IsFurnished,
                MinBookingDays = p.MinBookingDays,
                Images = p.Images.Select(img => $"{baseUrl}{img.ImageUrl}").ToList(),
            }).ToList();
        }


        public async Task<PropertyResponseDTO?> GetPropertyByIdAsync(int propertyId)
        {
            var property = await _context.Properties
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.PropertyId == propertyId);
            if (property == null)
            {
                return null;
            }

            return MapToDTO(property);
        }
        public async Task<List<PendingPropertyDTO>> GetAllPendingPropertiesAsync()
        {
            string baseUrl = _configuration["BaseUrl"];
            return await _context.pendingProperties
                .Include(p => p.Images)
                .OrderByDescending(p => p.PropertyId)
                .Select(p => new PendingPropertyDTO
                {
                    Id = p.PropertyId,
                    Title = p.Title,
                    Location = p.Location,
                    Price = p.Price,
                    Bedrooms = p.Bedrooms,
                    Bathrooms = p.Bathrooms,
                    RentType = p.RentType,
                    IsFurnished = p.IsFurnished,
                    Area = p.Area,
                    ImageUrls = p.Images != null
                    ? p.Images.Select(i => $"{baseUrl}{i.ImageUrl}").ToList()
                    : new List<string>()
                }
                    )
                .ToListAsync();
        }

        public async Task<List<PropertyResponseDTO>> GetAllPropertiesAsync()
        {
            var properties = await _context.Properties.Include(p => p.Images).Include(p=>p.ratings).ToListAsync();

            return properties.Select(MapToDTO).ToList();
        }

        public async Task<bool> UpdatePropertyAsync(int propertyId, PropertyDTO propertyDTO)
        {
            var user = await _authService.ValidateUserAsync();
            var property = await _context.Properties.FindAsync(propertyId);
            if (property == null) return false;
            property.Title = propertyDTO.Title;
            property.Location = propertyDTO.Location;
            property.Price = propertyDTO.Price;
            property.Bedrooms = propertyDTO.Bedrooms;
            property.Bathrooms = propertyDTO.Bathrooms;
            property.HasInternet = propertyDTO.HasInternet;
            property.AllowsPets = propertyDTO.AllowsPets;
            property.SmokingAllowed = propertyDTO.SmokingAllowed;
            property.AvailableFrom = propertyDTO.AvailableFrom;
            property.AvailableTo = propertyDTO.AvailableTo;
            property.Description = propertyDTO.Description;
            property.HasBalcony = propertyDTO.HasBalcony;
            property.HasElevator = propertyDTO.HasElevator;
            property.HasSecurity = propertyDTO.HasSecurity;
            property.Floor = propertyDTO.Floor;
            property.MinBookingDays = propertyDTO.MinBookingDays;
            property.RentType = propertyDTO.RentType;
            //property.ratings = propertyDTO.ratings;
            property.IsFurnished = propertyDTO.IsFurnished;
            property.Area = propertyDTO.Area;


            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeletePropertyAsync(int propertyId)
        {
            var user = await _authService.ValidateUserAsync();
            var property = await _context.Properties
                .Include(p => p.ratings)
                .FirstOrDefaultAsync(p => p.PropertyId == propertyId);
            var favorites = await _context.Favorites
       .Where(f => f.PropertyId == propertyId)
       .ToListAsync();

            if (property == null)
                return false;

            _context.Ratings.RemoveRange(property.ratings); 
            _context.Favorites.RemoveRange(favorites); 
            _context.Properties.Remove(property);
            await _context.SaveChangesAsync();

            return true;
        }


        public async Task<List<PropertyResponseDTO>> GetPropertiesByLocationAsync(string location)
        {
            var properties = await _context.Properties
                .Where(p => p.Location.Contains(location))
                .Include(p => p.Images)
                .ToListAsync();

            return properties.Select(MapToDTO).ToList();
        }

        public async Task<List<PropertyResponseDTO>> GetPropertiesByPriceAsync(decimal minPrice, decimal maxPrice)
        {
            var properties = await _context.Properties
                .Where(p => p.Price >= minPrice && p.Price <= maxPrice)
                .Include(p => p.Images)
                .ToListAsync();

            return properties.Select(MapToDTO).ToList();
        }

        public async Task<List<PropertyResponseDTO>> GetPropertiesByDateRangeAsync(DateTime from, DateTime to)
        {
            var properties = await _context.Properties
                .Where(p => p.AvailableFrom >= from && p.AvailableTo <= to)
                .Include(p => p.Images)
                .ToListAsync();

            return properties.Select(MapToDTO).ToList();
        }

        private PropertyResponseDTO MapToDTO(Property property)
        {
            string baseUrl = _configuration["BaseUrl"];
            var averageRating = property.ratings != null && property.ratings.Any(r => r.Stars.HasValue)
                ? property.ratings.Where(r => r.Stars.HasValue).Average(r => r.Stars.Value)
                : 0;

            return new PropertyResponseDTO
            {
                PropertyId = property.PropertyId,
                Title = property.Title,
                Location = property.Location,
                Price = property.Price,
                Bedrooms = property.Bedrooms,
                Bathrooms = property.Bathrooms,
                HasInternet = property.HasInternet,
                AllowsPets = property.AllowsPets,
                SmokingAllowed = property.SmokingAllowed,
                AvailableFrom = property.AvailableFrom,
                AvailableTo = property.AvailableTo,
                Description = property.Description,
                HasBalcony = property.HasBalcony,
                HasElevator = property.HasElevator,
                HasSecurity = property.HasSecurity,
                Floor = property.Floor,
                RentType = property.RentType,
                MinBookingDays = property.MinBookingDays,
                IsFurnished = property.IsFurnished,
                Area = property.Area,
                AverageRating = averageRating,
                Images = property.Images?.Select(i => $"{baseUrl}{i.ImageUrl}").ToList() ?? new List<string>()
            };
        }


        public async Task<bool> AddImagesToPropertyAsync(int propertyId, List<IFormFile> images)
        {
            var user = await _authService.ValidateUserAsync();
            var property = await _context.Properties.Include(p => p.Images)
               .FirstOrDefaultAsync(p => p.PropertyId == propertyId);

            if (property == null) return false;

            if (!Directory.Exists(_imageUploadPath))
            {
                Directory.CreateDirectory(_imageUploadPath);
            }

            foreach (var image in images)
            {
                var fileName = $"{Guid.NewGuid()}_{image.FileName}";
                var filePath = Path.Combine(_imageUploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }
                property.Images.Add(new PropertyImage
                {
                    ImageUrl = $"/images/{fileName}",
                    PropertyId = propertyId,
                    Caption = "Default Caption"
                });

            }

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<List<Property>> GetNearestPropertiesAsync(double userLat, double userLon, int maxResults)
        {
            var properties = await _context.Properties
                .Where(p => p.Latitude.HasValue && p.Longitude.HasValue)
                .ToListAsync();

            var nearestProperties = properties
                .Select(p => new
                {
                    Property = p,
                    Distance = _mapServices.CalculateDistance(userLat, userLon, p.Latitude.Value, p.Longitude.Value)
                })
                .OrderBy(p => p.Distance)
                .Take(maxResults)
                .Select(p => p.Property)
                .ToList();

            return nearestProperties;
        }


    }
}
