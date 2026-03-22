using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Domain.Entities;
using Microsoft.Extensions.Configuration;
using System.IO;

var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("src/TaskManagement.API/appsettings.json");

var configuration = builder.Build();
var connectionString = configuration.GetConnectionString("DefaultConnection");

var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
optionsBuilder.UseSqlServer(connectionString);

using var context = new ApplicationDbContext(optionsBuilder.Options);

var user1 = context.Users.FirstOrDefault(u => u.Email == "user1@test.com");
if (user1 == null) {
    Console.WriteLine("User user1@test.com not found!");
} else {
    Console.WriteLine($"User found: {user1.FullName} (ID: {user1.Id})");
    var memberships = context.ProjectMembers
        .Include(pm => pm.Project)
        .Where(pm => pm.UserId == user1.Id)
        .ToList();
    
    if (!memberships.Any()) {
        Console.WriteLine("User has NO memberships.");
    } else {
        foreach (var m in memberships) {
            Console.WriteLine($"Project: {m.Project.Name}, Status: {m.Status}, Role: {m.ProjectRole}");
        }
    }
}
