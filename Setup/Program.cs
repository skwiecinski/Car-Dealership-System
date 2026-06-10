using System;
using System.Windows.Forms;
using WixSharp;
using WixSharp.Forms;

namespace WixSharp_Setup1
{
    public class Program
    {
        static void Main()
        {
            var project = new ManagedProject("SalonSamochodowy",
                             new Dir(@"%ProgramFiles%\Projekt\SalonSamochodowy",
                                 new Files(@"C:\Users\markz\OneDrive\Pulpit\salonik\Salon-Samochodowy\SalonSamochodowy\bin\Release\net8.0-windows\win-x64\publish\*.*")));

            project.GUID = new Guid("3186a88a-6367-4225-9572-4cb6fb7eb50b");

            project.ManagedUI = ManagedUI.Empty;    
            project.ManagedUI = ManagedUI.Default;  

            
            project.ManagedUI = new ManagedUI();

            project.ManagedUI.InstallDialogs.Add(Dialogs.Welcome)
                                            .Add(Dialogs.Licence)
                                            .Add(Dialogs.SetupType)
                                            .Add(Dialogs.Features)
                                            .Add(Dialogs.InstallDir)
                                            .Add(Dialogs.Progress)
                                            .Add(Dialogs.Exit);

            project.ManagedUI.ModifyDialogs.Add(Dialogs.MaintenanceType)
                                           .Add(Dialogs.Features)
                                           .Add(Dialogs.Progress)
                                           .Add(Dialogs.Exit);

            project.Load += Msi_Load;
            project.BeforeInstall += Msi_BeforeInstall;
            project.AfterInstall += Msi_AfterInstall;

            
            

            project.BuildMsi();
        }

        static void Msi_Load(SetupEventArgs e)
        {
            if (!e.IsUISupressed && !e.IsUninstalling)
                MessageBox.Show(e.ToString(), "Load");
        }

        static void Msi_BeforeInstall(SetupEventArgs e)
        {
            if (!e.IsUISupressed && !e.IsUninstalling)
                MessageBox.Show(e.ToString(), "BeforeInstall");
        }

        static void Msi_AfterInstall(SetupEventArgs e)
        {
            if (!e.IsUISupressed && !e.IsUninstalling)
                MessageBox.Show(e.ToString(), "AfterExecute");
        }
    }
}