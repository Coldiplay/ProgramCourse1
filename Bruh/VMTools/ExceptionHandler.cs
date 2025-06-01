using System.Windows;

namespace Bruh.VMTools
{
    public static class ExceptionHandler
    {
        public static bool Try(Action action)
        {
            try
            {
                action();
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
        }
    }
}
