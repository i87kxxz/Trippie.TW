using System.Security.Principal;
using Trippie.TW.Categories;
using Trippie.TW.Core.Registry;
using Trippie.TW.UI;

namespace Trippie.TW;

class Program
{
    static void Main(string[] args)
    {
        ConsoleUI.Initialize();

        if (!IsAdministrator())
        {
            ConsoleUI.Clear();
            ConsoleUI.PrintHeader();
            ConsoleUI.WriteLine();
            ConsoleUI.PrintError("This application requires Administrator privileges.");
            ConsoleUI.PrintInfo("Please right-click and select 'Run as Administrator'.");
            ConsoleUI.WaitForKey();
            return;
        }

        var registry = new CategoryRegistry();
        RegisterCategories(registry);

        var menu = new MenuManager(registry);
        menu.Run();

        ConsoleUI.Clear();
        ConsoleUI.PrintHeader();
        ConsoleUI.WriteLine();
        ConsoleUI.PrintInfo("Thank you for using Trippie.TW!");
        ConsoleUI.PrintInfo("Changes may require a restart to take effect.");
        ConsoleUI.WaitForKey("Press any key to exit...");
    }

    private static void RegisterCategories(CategoryRegistry registry)
    {
        registry.RegisterRange(new Core.Interfaces.ITweakCategory[]
        {
            new PrivacyCategory(),
            new PerformanceCategory(),
            new UICategory(),
            new NetworkCategory(),
            new ServicesCategory(),
            new SecurityCategory()
        });
    }

    private static bool IsAdministrator()
    {
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }
}
