using System;
using Microsoft.Data.SqlClient;

class Program
{
    static void Main()
    {
        string connectionString = "Server=.\\SQLEXPRESS;Database=TaskManagementDB;Trusted_Connection=True;TrustServerCertificate=True;";
        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Delete mock users
                string[] emails = { "admin@example.com", "test@example.com", "dev@sprinta.local" };
                foreach (var email in emails)
                {
                    using (SqlCommand cmd = new SqlCommand("DELETE FROM Users WHERE Email = @email", connection))
                    {
                        cmd.Parameters.AddWithValue("@email", email);
                        int rows = cmd.ExecuteNonQuery();
                        Console.WriteLine($"Deleted {rows} users for {email}");
                    }
                }

                // Delete mock workspace
                using (SqlCommand cmd = new SqlCommand("DELETE FROM Workspaces WHERE Slug = 'cybwf'", connection))
                {
                    int rows = cmd.ExecuteNonQuery();
                    Console.WriteLine($"Deleted {rows} mock workspaces (cybwf)");
                }

                // Delete mock project
                using (SqlCommand cmd = new SqlCommand("DELETE FROM Projects WHERE Identifier = 'CYBWF'", connection))
                {
                    int rows = cmd.ExecuteNonQuery();
                    Console.WriteLine($"Deleted {rows} mock projects (CYBWF)");
                }
            }
            Console.WriteLine("[SUCCESS] Cleaned up mock data!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] {ex.Message}");
        }
    }
}
