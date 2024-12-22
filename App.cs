using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;
using System.Diagnostics;
using System.Configuration.Assemblies;
using AW = Autodesk.Windows;
using Autodesk.Internal.Windows;
using ComboBox = Autodesk.Revit.UI.ComboBox;
using AnlaxPackage;
using System.Drawing;
using System.Windows.Media.Imaging;
using Mono.Cecil;
using System.Resources;


namespace AnlaxBaseServer
{


        public class RemoteControl : IExternalApplication
        {
            private bool _started;

            public Result Execute(
                ExternalCommandData commandData,
                ref string message,
                ElementSet elements)
            {
                Run();

                return Result.Succeeded;
            }

            public Result OnShutdown(UIControlledApplication application)
            {
                return Result.Succeeded;
            }

            public Result OnStartup(UIControlledApplication application)
            {
            string DllName = Assembly.GetExecutingAssembly().GetName().Name;
            string assemblyLocation = Assembly.GetExecutingAssembly().Location;
            application.Idling += ApplicationOnIdling;
            string TabName = "AnlaxServer";
            try
            {
                application.CreateRibbonTab(TabName);
            }
            catch (Exception ex)
            {

            }
            RibbonPanel panel = application.CreateRibbonPanel(TabName, "Серверные функции");
            PushButtonData buttonDataPlugSettings = new PushButtonData(nameof(OpenWebHelp), "Пакетный\nэкспорт", assemblyLocation, typeof(OpenWebHelp).FullName);
            buttonDataPlugSettings.LargeImage = new BitmapImage(new Uri($@"/{DllName};component/Icons/Day - Update.png", UriKind.RelativeOrAbsolute));
            panel.AddItem(buttonDataPlugSettings);


            return Result.Succeeded;
            }

            private void ApplicationOnIdling(object sender, IdlingEventArgs e)
            {
                if (!_started)
                {
                    Run();
                }

                _started = true;
            }

            public void Run()
            {
                var handler = new RequestHandler();

                var listener = new RevitHttpListener(handler);

                Task.Run(() => listener.Start());
            }
        }



    }



