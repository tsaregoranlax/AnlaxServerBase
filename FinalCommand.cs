using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AnlaxBase
{

    [Transaction(TransactionMode.Manual)]
    public class FinalCommand : IExternalCommand
    {
        public string strCommandName {  get; set; }
        public string fullPathDllName { get; set; }
        public FinalCommand(string strCommandName,string fullPathDllName) 
        {
            this.strCommandName = strCommandName;
            this.fullPathDllName = fullPathDllName;
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            InvokeRevitCommand(strCommandName, commandData, ref message, elements, fullPathDllName);
            return Result.Succeeded;
        }
        public static void InvokeRevitCommand(string strCommandName, ExternalCommandData commandData, ref string message, object elements, string fullPathDllName)
        {
            //Грузим нашу библиотеку в массив байтов.
            //Таким образом ревит ее не заблокирует на диске.
            try
            {
                byte[] assemblyBytes = File.ReadAllBytes(fullPathDllName);
                Assembly objAssembly = Assembly.Load(assemblyBytes);

                //Проходимся по сборке.
                foreach (Type objType in objAssembly.GetTypes())
                {
                    //Выбираем класс.
                    if (objType.IsClass)
                    {
                        if (objType.Name.ToLower() == strCommandName.ToLower())
                        {
                            object ibaseObject = Activator.CreateInstance(objType);

                            object[] arguments = new object[] { commandData, message, elements };

                            //MethodInfo? mbinfo = objType.GetMethod("Execute");
                            //mbinfo.Invoke(ibaseObject, arguments);

                            object result = objType.InvokeMember(
                                "Execute",
                                BindingFlags.Default | BindingFlags.InvokeMethod,
                                null,
                                ibaseObject,
                                arguments);

                            break;
                        }
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {

                StringBuilder sb = new StringBuilder();
                foreach (Exception loaderException in ex.LoaderExceptions)
                {
                    sb.AppendLine(loaderException.Message);
                }
            }
        }
    }
}
