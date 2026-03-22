using Microsoft.EntityFrameworkCore;
using TaskManagement.Infrastructure.Data;
using System.Linq;
using System;

namespace TaskManagement.Diagnostics
{
    public class UserLister
    {
        public static void ListUsers(ApplicationDbContext context)
        {
            var users = context.Users.ToList();
            Console.WriteLine("--- Registered Users ---");
            foreach (var user in users)
            {
                Console.WriteLine($"ID: {user.Id}, Email: {user.Email}, Name: {user.FullName}");
            }
            Console.WriteLine("-------------------------");
        }
    }
}
