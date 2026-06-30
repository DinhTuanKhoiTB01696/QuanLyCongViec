using System;

class Program
{
    static void Main()
    {
        string identifier = "tua4699@gmail.com";
        int hash = 0;
        foreach (char c in identifier)
        {
            hash = c + ((hash << 5) - hash);
        }
        
        string[] AvatarColors = new[]
        {
            "#579dff", "#c97cf4", "#00b8d9", "#22a06b", "#f5cd47", "#e2483d"
        };
        
        Console.WriteLine(hash);
        Console.WriteLine(Math.Abs(hash) % 6);
        Console.WriteLine(AvatarColors[Math.Abs(hash) % 6]);
    }
}
