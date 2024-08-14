using WinformTemplate.Serialize;
using Debug = WinformTemplate.Logger.Debug;

namespace WinformTemplate
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            // Load log4net system
            Debug.InitLog4Net();
            Debug.Info("Project start: " + Application.ProductVersion);

            // Load Config system
            GlobalProjectConfig.Instance.CheckConfigLoaded();

            // Load Locker system

            Application.Run(new MainForm());
        }
    }
}