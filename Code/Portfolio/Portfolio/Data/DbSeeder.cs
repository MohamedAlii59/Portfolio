using Microsoft.EntityFrameworkCore;
using Portfolio.Models;

namespace Portfolio.Data
{

    // There is no public registration endpoint. Run this once to create the client's
    // account, then remove the call to it — it should not run on every app start.
    public static class DbSeeder
    {
        public static void SeedInitialUser(AppDbContext db, string fullName, string email, string slug, string initialPassword, string phoneNumber, string title)
        {
            if (db.Users.IgnoreQueryFilters().Any(u => u.Email == email))
            {
                Console.WriteLine("A user with this email already exists — skipping seed.");
                return;
            }

            var user = new User
            {
                FullName = fullName,
                Email = email,
                Slug = slug,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(initialPassword),
                PhoneNumber = phoneNumber,
                Title = title,
                MustChangePasswordOnFirstLogin = true,
                CreatedAt = DateTime.UtcNow,
            };

            db.Users.Add(user);
            db.SaveChanges();

            Console.WriteLine($"Seeded user '{fullName}' <{email}> — initial password: {initialPassword}");
            Console.WriteLine("Share these credentials with the client. They'll be asked to set a new password on first login.");
        }
    }
}
