using Microsoft.Data.SqlClient;
using PPICancerRecognitionProject.domain;
using PPICancerRecognitionProject.repository;

namespace PPICancerRecognitionProject
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            

           

                ApplicationConfiguration.Initialize();


                

            // Continue to your main form
            Application.Run(new Form1());
        }
    }
}