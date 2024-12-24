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
using Autodesk.Revit.ApplicationServices;


namespace AnlaxBaseServer
{


    public class RemoteControl : IExternalApplication
    {
        private bool _started;
        private string _port;
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
        private string GetPortFromCommandLineArguments()
        {
            string[] args = Environment.GetCommandLineArgs();
            foreach (string arg in args)
            {
                if (arg.StartsWith("/path:", StringComparison.OrdinalIgnoreCase))
                {
                    return arg.Substring("/path:".Length);
                }
            }
            return null;
        }
        public Result OnStartup(UIControlledApplication application)
        {

            string DllName = Assembly.GetExecutingAssembly().GetName().Name;
            string assemblyLocation = Assembly.GetExecutingAssembly().Location;

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
            _port = GetPortFromCommandLineArguments();
            if (!string.IsNullOrEmpty(_port)) // Если ревит запушен вручную. То плагин не запускаем
            {
                RevitTask _revitTask = new RevitTask();
                var task = _revitTask
        .Run((uiapp) =>
        {

            TestFunction(uiapp);

        });
            }


            return Result.Succeeded;
        }
        private string TestFunction(UIApplication uiapp)
        {
            Application app = uiapp.Application;

            try
            {
                // Создание нового документа
                string templatePath = "C:\\Users\\tsare\\Desktop\\Temp\\1 вариант\\sd.rvt";
                Document doc = OpenDocumentDetach(templatePath, uiapp);
                using (Transaction trans = new Transaction(doc, "Create Wall"))
                {
                    trans.Start();

                    // Определение начальной и конечной точек стены
                    XYZ start = new XYZ(0, 0, 0);
                    XYZ end = new XYZ(1000 / 304.8, 0, 0); // Перевод 1000 мм в футы

                    // Поиск типа стены
                    WallType wallType = new FilteredElementCollector(doc)
                        .OfClass(typeof(WallType))
                        .Cast<WallType>()
                        .FirstOrDefault(wt => wt.Kind == WallKind.Basic);

                    if (wallType == null)
                    {
                        return "Не удалось найти базовый тип стены.";
                    }

                    // Получение уровня для стены
                    Level level = new FilteredElementCollector(doc)
                        .OfClass(typeof(Level))
                        .Cast<Level>()
                        .FirstOrDefault();

                    if (level == null)
                    {
                        return "Не удалось найти уровень.";
                    }

                    // Создание стены
                    Wall.Create(doc, Line.CreateBound(start, end), wallType.Id, level.Id, 3000 / 304.8, 0, false, false);

                    trans.Commit();
                }

                return "успех";
            }
            catch (Exception ex)
            {
                return "ошибка";
            }
        }
        private void ApplicationOnIdling(object sender, IdlingEventArgs e)
        {
            if (!_started)
            {
                Run();
            }

            _started = true;
        }
        public Document OpenDocumentDetach(string PathStart, UIApplication uiapp, bool saveworkset = true)
        {
            Document documentOut = null;
            OpenOptions openOptions = new OpenOptions();
            openOptions.AllowOpeningLocalByWrongUser = true;
            openOptions.DetachFromCentralOption = DetachFromCentralOption.DetachAndDiscardWorksets;
            if (saveworkset)
            {
                openOptions.DetachFromCentralOption = DetachFromCentralOption.DetachAndPreserveWorksets;
            }
            ModelPath modelPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(PathStart);
            IList<WorksetId> worksetIds = new List<WorksetId>();

            try //Попытка отключить рабочие наборы со связями для ускорения процесса
            {
                IList<WorksetPreview> worksets = WorksharingUtils.GetUserWorksetInfo(modelPath);
                foreach (WorksetPreview worksetPrev in worksets)
                {
                    if (!worksetPrev.Name.ToLower().Contains("link"))
                    {
                        worksetIds.Add(worksetPrev.Id);
                    }
                }
                WorksetConfiguration openConfig = new WorksetConfiguration(WorksetConfigurationOption.CloseAllWorksets);
                openConfig.Open(worksetIds);
                openOptions.SetOpenWorksetsConfiguration(openConfig);

            }
            catch
            {
            }
            try
            {
                documentOut = uiapp.OpenAndActivateDocument(ModelPathUtils.ConvertUserVisiblePathToModelPath(PathStart), openOptions, true).Document;

                return documentOut;
            }
            catch (Exception ex)
            {

                return documentOut;
            }

        }
        public void Run()
        {
            var handler = new RequestHandler();

            var listener = new RevitHttpListener(handler, _port);

            Task.Run(() => listener.Start());
        }
    }



}



