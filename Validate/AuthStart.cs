﻿using AnlaxBase.Validate;
using AnlaxPackage;
using AnlaxPackage.Auth;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnlaxBase
{
    [Transaction(TransactionMode.Manual)]
    public class AuthStart : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            AuthSettings auth =AuthSettings.Initialize(true);
            Document currentDoc = commandData.Application.ActiveUIDocument.Document;
            int numLiscence=StaticAuthorization.GetLiscence();
            if (numLiscence == 0 )
            {
                NewValidate postgresSQLValidate = new NewValidate(auth.Login, auth.Password, currentDoc);
                postgresSQLValidate.CheckLicense(true);
            }
            else
            {
                LiscenceManager liscenceManager = new LiscenceManager();
                liscenceManager.ShowDialog();
            }

            return Result.Succeeded;
        }
    }
}