using System;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace AnlaxBaseServer
{
    public class RequestHandler
    {
        private readonly RevitTask _revitTask = new RevitTask();

        private string TestFunction (UIApplication uiapp)
        {
            Application app = uiapp.Application;
            Document doc = null;

            try
            {
                // Создание нового документа
                string templatePath = "C:\\ProgramData\\Autodesk\\RVT 2022\\Templates\\Default_M_RUS.rte";
                doc = app.NewProjectDocument(templatePath);

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
    

        public async Task<string[]> HandleAsync(string request)
        {
            string[] result;

            Task<string[]> task;
            string decodedPath = Uri.UnescapeDataString(request);
            if (request == "FILEDIALOG")
            {
                task = _revitTask
                    .Run((app) =>
                    {
                        //// var document = app.Document;

                        var dialog = new FileOpenDialog("Revit Files (*.rvt)|*.rvt");

                        var dialogResult = dialog.Show();

                        var modelPath = dialog.GetSelectedModelPath();

                        var path = ModelPathUtils
                            .ConvertModelPathToUserVisiblePath(modelPath);

                        return new[] { path };
                    });
            }
            else if (request == "VIEWLIST")
            {
                task = _revitTask
                    .Run((uiapp) =>
                    {
                        if (uiapp.ActiveUIDocument?.Document == null)
                        {
                            return new[] { "No opened documents" };
                        }

                        var document = uiapp.ActiveUIDocument.Document;

                        var plans = new FilteredElementCollector(document)
                            .WhereElementIsNotElementType()
                            .OfClass(typeof(View))
                            .Select(x => x.Name)
                            .ToArray();

                        return plans;
                    });
            }
            else if (request.Contains("EXPORT"))
            {
                task = _revitTask
                    .Run((uiapp) =>
                    {
                        string Jsonrequest = request.Replace("EXPORT", "");
                        TestFunction(uiapp);
                        return new[] { decodedPath };
                    });
            }

            else
            {
                task = _revitTask
                .Run(uiapp =>
                {
                    //// TaskDialog.Show("Deb", $"Requested: {request}");

                    var command = (PostableCommand)Enum.Parse(
                        typeof(PostableCommand),
                        request,
                        true);

                    var id = RevitCommandId
                        .LookupPostableCommandId(command);

                    uiapp.PostCommand(id);

                    return new[] { $"Successfully posted command {command}" };
                });
            }


            try
            {
                result = await task;
            }
            catch (Exception e)
            {
                result = new[] { $"{e.Message} in {e.StackTrace}" };
            }

            return result;
        }
    }
}
