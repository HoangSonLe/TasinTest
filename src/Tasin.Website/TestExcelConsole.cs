using Tasin.Website.Scripts;

namespace Tasin.Website
{
    /// <summary>
    /// Simple console app to test Excel UTF-8 functionality
    /// </summary>
    public class TestExcelConsole
    {
        public static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("=== Testing Excel UTF-8 Functionality ===");
            
            try
            {
                TestExcelUTF8.RunAllTests();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
            
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
