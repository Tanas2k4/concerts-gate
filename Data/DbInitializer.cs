using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using concerts_gate.server.Common.Constants;
using concerts_gate.server.Common.Enums;
using concerts_gate.server.Entities;

namespace concerts_gate.server.Data;

/// <summary>
/// Initializes sample seed data and prepares database schemas on application startup.
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// Checks database creation, ensures migrations, and seeds initial data (Roles, Users, Concerts, Categories, Vouchers).
    /// </summary>
    /// <param name="serviceProvider">Dependency injection service provider.</param>
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        // 1. Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // 2. Initialize default system roles if not present
        string[] roleNames = { AppConstants.Roles.Admin, AppConstants.Roles.Operator, AppConstants.Roles.Customer };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new ApplicationRole(roleName));
            }
        }

        // 3. Initialize default Administrator account
        var adminEmail = "admin@concertsgate.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "System Administrator",
                Role = UserRole.Admin,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };
            var result = await userManager.CreateAsync(adminUser, "Admin@123456");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, AppConstants.Roles.Admin);
            }
        }

        // 4. Initialize default Event Operator account
        var operatorEmail = "operator@concertsgate.com";
        var operatorUser = await userManager.FindByEmailAsync(operatorEmail);
        if (operatorUser == null)
        {
            operatorUser = new ApplicationUser
            {
                UserName = operatorEmail,
                Email = operatorEmail,
                FullName = "Event Operator",
                Role = UserRole.Operator,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };
            var result = await userManager.CreateAsync(operatorUser, "Operator@123456");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(operatorUser, AppConstants.Roles.Operator);
            }
        }

        // 5. Initialize default Customer account
        var customerEmail = "customer@gmail.com";
        var customerUser = await userManager.FindByEmailAsync(customerEmail);
        if (customerUser == null)
        {
            customerUser = new ApplicationUser
            {
                UserName = customerEmail,
                Email = customerEmail,
                FullName = "John Doe",
                Role = UserRole.Customer,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };
            var result = await userManager.CreateAsync(customerUser, "Customer@123456");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(customerUser, AppConstants.Roles.Customer);
            }
        }

        // 6. Initialize sample Concerts and Ticket Categories if database is empty
        if (!await context.Concerts.AnyAsync())
        {
            var flashSaleConcert = new Concert
            {
                Id = Guid.NewGuid(),
                Title = "The Cyber Waves: Flash Sale Live 2026",
                Artist = "The Cyber Waves ft. DJ Horizon",
                Description = "The most sensational electronic music concert of 2026 featuring world-class hologram visuals and sound systems.",
                Venue = "National Stadium, Main Arena",
                BannerUrl = "https://images.unsplash.com/photo-1470225620780-dba8ba36b745?auto=format&fit=crop&w=1200&q=80",
                Genre = "EDM / Synthwave",
                EventDate = DateTime.UtcNow.AddDays(30),
                SaleStartDate = DateTime.UtcNow.AddMinutes(-10),
                SaleEndDate = DateTime.UtcNow.AddDays(20),
                Status = ConcertStatus.Published,
                IsFlashSale = true,
                CreatedAt = DateTime.UtcNow
            };

            flashSaleConcert.TicketCategories.Add(new TicketCategory
            {
                Id = Guid.NewGuid(),
                ConcertId = flashSaleConcert.Id,
                Name = "VIP Early Bird (Flash Sale)",
                Description = "Fast-track lane, front fanzone access, official event jersey, and light-up banner",
                Price = 1500000m,
                TotalQuantity = 100,
                RemainingQuantity = 100,
                ReservedQuantity = 0,
                SoldQuantity = 0,
                MaxPerOrder = 2
            });

            flashSaleConcert.TicketCategories.Add(new TicketCategory
            {
                Id = Guid.NewGuid(),
                ConcertId = flashSaleConcert.Id,
                Name = "Standard GA (Standing Zone A)",
                Description = "General admission ticket for center standing arena Zone A",
                Price = 650000m,
                TotalQuantity = 500,
                RemainingQuantity = 500,
                ReservedQuantity = 0,
                SoldQuantity = 0,
                MaxPerOrder = 4
            });

            flashSaleConcert.TicketCategories.Add(new TicketCategory
            {
                Id = Guid.NewGuid(),
                ConcertId = flashSaleConcert.Id,
                Name = "Standard Tier 2 (Grandstand B)",
                Description = "Panoramic view seating area in Tier 2 Grandstand B",
                Price = 450000m,
                TotalQuantity = 1000,
                RemainingQuantity = 1000,
                ReservedQuantity = 0,
                SoldQuantity = 0,
                MaxPerOrder = 4
            });

            var indieConcert = new Concert
            {
                Id = Guid.NewGuid(),
                Title = "Autumn Serenade Acoustic Night",
                Artist = "Vu & The Acoustic Ensemble",
                Description = "An intimate, soulful acoustic evening performing iconic ballad hits and melodies.",
                Venue = "Grand Concert Hall, City Center",
                BannerUrl = "https://images.unsplash.com/photo-1511671782779-c97d3d27a1d4?auto=format&fit=crop&w=1200&q=80",
                Genre = "Indie / Acoustic",
                EventDate = DateTime.UtcNow.AddDays(45),
                SaleStartDate = DateTime.UtcNow.AddDays(-2),
                SaleEndDate = DateTime.UtcNow.AddDays(40),
                Status = ConcertStatus.Published,
                IsFlashSale = false,
                CreatedAt = DateTime.UtcNow
            };

            indieConcert.TicketCategories.Add(new TicketCategory
            {
                Id = Guid.NewGuid(),
                ConcertId = indieConcert.Id,
                Name = "Gold Tier (Level 1)",
                Description = "Central Level 1 VIP seating with premium beverage service",
                Price = 900000m,
                TotalQuantity = 200,
                RemainingQuantity = 200,
                ReservedQuantity = 0,
                SoldQuantity = 0,
                MaxPerOrder = 4
            });

            indieConcert.TicketCategories.Add(new TicketCategory
            {
                Id = Guid.NewGuid(),
                ConcertId = indieConcert.Id,
                Name = "Silver Tier (Level 2)",
                Description = "Standard seated ticket on Level 2",
                Price = 500000m,
                TotalQuantity = 400,
                RemainingQuantity = 400,
                ReservedQuantity = 0,
                SoldQuantity = 0,
                MaxPerOrder = 4
            });

            context.Concerts.AddRange(flashSaleConcert, indieConcert);
            await context.SaveChangesAsync();
        }

        // 7. Initialize sample promotional Vouchers if database is empty
        if (!await context.Vouchers.AnyAsync())
        {
            var vouchers = new List<Voucher>
            {
                new Voucher
                {
                    Id = Guid.NewGuid(),
                    Code = "FLASHSALE20",
                    Description = "20% discount up to 300,000 VND for launch week",
                    DiscountType = DiscountType.Percentage,
                    DiscountValue = 20m,
                    MaxDiscountAmount = 300000m,
                    MinOrderAmount = 500000m,
                    MaxUsageCount = 500,
                    CurrentUsageCount = 0,
                    MaxUsagePerUser = 1,
                    ValidFrom = DateTime.UtcNow.AddDays(-1),
                    ValidTo = DateTime.UtcNow.AddDays(14),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Voucher
                {
                    Id = Guid.NewGuid(),
                    Code = "WELCOME100K",
                    Description = "Direct 100,000 VND discount for orders from 1,000,000 VND",
                    DiscountType = DiscountType.FixedAmount,
                    DiscountValue = 100000m,
                    MinOrderAmount = 1000000m,
                    MaxUsageCount = 1000,
                    CurrentUsageCount = 0,
                    MaxUsagePerUser = 1,
                    ValidFrom = DateTime.UtcNow.AddDays(-1),
                    ValidTo = DateTime.UtcNow.AddDays(30),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Voucher
                {
                    Id = Guid.NewGuid(),
                    Code = "VIPEXCLUSIVE",
                    Description = "50% discount up to 1,000,000 VND for loyal VIP members",
                    DiscountType = DiscountType.Percentage,
                    DiscountValue = 50m,
                    MaxDiscountAmount = 1000000m,
                    MinOrderAmount = 1500000m,
                    MaxUsageCount = 50,
                    CurrentUsageCount = 0,
                    MaxUsagePerUser = 1,
                    ValidFrom = DateTime.UtcNow.AddDays(-1),
                    ValidTo = DateTime.UtcNow.AddDays(7),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            context.Vouchers.AddRange(vouchers);
            await context.SaveChangesAsync();
        }
    }
}
