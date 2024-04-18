using Debug = WinformTemplate.Src.Logger.Debug;

namespace WinformTemplate
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            // Load log4net system
            Debug.InitLog4Net();

            // Load Config system


            // Load Locker system

            // Load Middleware

            Application.Run(new MainForm());
        }
    }
}