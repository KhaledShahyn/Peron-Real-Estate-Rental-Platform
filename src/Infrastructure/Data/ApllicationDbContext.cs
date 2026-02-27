using FinalProject.src.Domain.Helper;
using FinalProject.src.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.src.Infrastructure.Data
{
    public class ApllicationDbContext : IdentityDbContext<ApplicatiopnUser>
    {
        public ApllicationDbContext(DbContextOptions<ApllicationDbContext> options) : base(options) { }
        public DbSet<Property> Properties { get; set; }
        public DbSet<PendingProperty> pendingProperties { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<BlacklistedToken> BlacklistedTokens { get; set; }
        public DbSet<PropertyImage> PropertiesImage { get; set; }
        public DbSet<PendingPropertyImage> pendingPropertyImages { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<AccessToken> AccessTokens { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Inquiry> Inquiries { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<AppRating> AppRatings { get; set; }
        public DbSet<ChatBotMessage> chatBotMessages { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<IdentityRole>().HasData(
        new IdentityRole { Id = "1", Name = "User", NormalizedName = "USER" },
        new IdentityRole { Id = "2", Name = "ADMIN", NormalizedName = "ADMIN" }
    ); 
            modelBuilder.Entity<Property>()
                .HasOne(p => p.Owner)
                .WithMany(o => o.Properties)
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Booking>()
                 .HasOne(b => b.User)
                 .WithMany(u => u.Bookings)
                 .HasForeignKey(b => b.UserId)
                 .OnDelete(DeleteBehavior.NoAction);
           
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Property)
                .WithMany(p => p.Bookings)
                .HasForeignKey(b => b.PropertyId)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Booking)
                .WithOne(b => b.Payment)
                .HasForeignKey<Payment>(p => p.BookingId)
                .OnDelete(DeleteBehavior.NoAction); // منع الحذف التلقائي

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany(u => u.ReceivedMessages)
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.Property)
                .WithMany()
                .HasForeignKey(f => f.PropertyId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Rating>()
                .HasOne(r => r.Property)
                .WithMany(p => p.ratings)
                .HasForeignKey(r => r.PropertyId)
                .OnDelete(DeleteBehavior.NoAction); 
           

            modelBuilder.Entity<Rating>()
                .HasOne(r => r.User)
                .WithMany(u => u.Ratings)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ChatMessage>()
        .Property(c => c.SenderId)
        .HasMaxLength(450); // تحديد حجم الـ string

            modelBuilder.Entity<ChatMessage>()
                .Property(c => c.ReceiverId)
                .HasMaxLength(450);
            modelBuilder.Entity<ChatMessage>()
        .HasOne(m => m.Sender)
        .WithMany()
        .HasForeignKey(m => m.SenderId)
        .OnDelete(DeleteBehavior.NoAction); 

            modelBuilder.Entity<ChatMessage>()
                .HasOne(m => m.Receiver)
                .WithMany()
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.NoAction);
         modelBuilder.Entity<Rating>()
            .HasOne(r => r.Property)
            .WithMany(p => p.ratings)
            .HasForeignKey(r => r.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Booking>()
           .HasOne(b => b.PendingProperty)
           .WithMany() 
           .HasForeignKey(b => b.PendingPropertyId);
         //modelBuilder.Entity<PendingProperty>()
         //   .HasOne(p => p.Booking)
         //   .WithOne(b => b.PendingProperty)
         //   .HasForeignKey<Booking>(b => b.PendingPropertyId);
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.PendingProperty)
            .WithMany(p => p.Bookings)
            .HasForeignKey(b => b.PendingPropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        }


    }
}
