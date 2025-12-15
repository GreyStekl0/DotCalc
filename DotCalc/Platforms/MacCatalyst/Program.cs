using ObjCRuntime;
using UIKit;

namespace DotCalc
{
    public class Program
    {
        // Главная точка входа MacCatalyst-приложения.
        static void Main(string[] args)
        {
            // Если нужно использовать другой AppDelegate, его можно указать здесь вместо AppDelegate.
            UIApplication.Main(args, null, typeof(AppDelegate));
        }
    }
}
