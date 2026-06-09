using System;
using System.Windows.Forms;
using WixSharp;
using WixSharp.Forms;
namespace SalonSamochodowySetup
{
    public class Program
    {
        static void Main()
        {
            var project = new ManagedProject("SalonSamochodowy",
                             new Dir(@"%ProgramFiles%\ProjektBazyDanych\SalonSamochodowy",
                                 new Files(@"..\__bin\__Release\__net8.0-windows\__publish\*.*")));

            project.GUID = new Guid("cf69b212-9587-4d8f-9c6c-5de310aadff5");
            project.ManagedUI = ManagedUI.Empty;    //no standard UI dialogs
            project.ManagedUI = ManagedUI.Default;  //all standard UI dialogs

            project.Version = new Version("1.0.0.0");
            project.Description = "Car dealearship managment system";
            project.ControlPanelInfo.Manufacturer = "ProjektBazyDanych";
            project.ControlPanelInfo.Comments = "Car dealership managment app";
            project.ControlPanelInfo.HelpLink = "https://github.com/KBieszczad/Salon-Samochodowy";
            project.ControlPanelInfo.NoModify = false;
            project.ControlPanelInfo.NoRepair = false;
            //custom set of standard UI dialogs
            project.ManagedUI = new ManagedUI();

            project.ManagedUI.InstallDialogs
                .Add(Dialogs.Welcome)
                .Add(Dialogs.Licence)
                //.Add(Dialogs.SetupType)
                //.Add(Dialogs.Features)
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
            //project.SourceBaseDir = "<input dir path>";
            //project.OutDir = "<output dir path>";
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