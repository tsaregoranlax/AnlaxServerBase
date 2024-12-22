using AnlaxPackage;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AnlaxBaseServer
{
    [Transaction(TransactionMode.Manual)]
    public class OpenWebHelp : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            string urlBase =AuthSettings.Initialize(true).URLBaseKnowledge;
            if (!string.IsNullOrEmpty(urlBase))
            {
                System.Diagnostics.Process.Start(urlBase);
            }
            else
            {
                MessageBox.Show("В файле json не задан путь к базе знаний. Пропишите его в свойстве URLBaseKnowledge");
            }
            
            return Result.Succeeded;
        }
    }
}
