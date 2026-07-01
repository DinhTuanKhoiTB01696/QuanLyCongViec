// Run: dotnet script generate-demo-hash.csx
// Or: dotnet run in a console project with BCrypt.Net-Next NuGet
// This prints the BCrypt hash for "Demo@123" to use in SQL seed

#r "nuget: BCrypt.Net-Next, 4.0.3"

var hash = BCrypt.Net.BCrypt.HashPassword("Demo@123");
Console.WriteLine($"BCrypt hash for Demo@123:");
Console.WriteLine(hash);
