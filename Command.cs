/* *********************************************************************************************
 * Create Foor
 * 
 * Create Build (API REVIT 2024, Revit 2024 SDK) 
 * Application (add-ins)
 * -------------------------------------------------------------------------------------------
 * Visual Studio 2022 
 * C# | .NET 4.8
 * -------------------------------------------------------------------------------------------
 * Revit API (Create Wall): 
 * https://www.revitapidocs.com/2020/4a42066c-bc44-0f99-566c-4e8327bc3bfa.htm
 * 
 * Writing sgiman @ 2023 
 * **********************************************************************************************/
//
// (C) Copyright 2003-2019 by Autodesk, Inc.
//
// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted,
// provided that the above copyright notice appears in all copies and
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting
// documentation.
//
// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS.
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE. AUTODESK, INC.
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.
//
// Use, duplication, or disclosure by the U.S. Government is subject to
// restrictions set forth in FAR 52.227-19 (Commercial Computer
// Software - Restricted Rights) and DFAR 252.227-7013(c)(1)(ii)
// (Rights in Technical Data and Computer Software), as applicable.
//

using System;
using System.Collections.Generic;
using Autodesk.Revit;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Revit.SDK.Samples.GenerateFloor.CS
{
    /// <summary>
    /// Implements the Revit add-in interface IExternalCommand
    /// </summary>
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]
    public class Command : IExternalCommand
    {
        #region IExternalCommand Members Implementation
        /// <summary>
        /// Реализуйте этот метод как внешнюю команду для Revit..
        /// </summary>
		
		/// <param name="commandData">Объект, передаваемый внешнему приложению
        /// который содержит данные, относящиеся к команде,
        /// например объект приложения и активное представление.</param>
        /// <param name="message">Сообщение, которое может быть установлено внешним приложением
        /// который будет отображаться, если возвращается ошибка или отмена
        /// внешняя команда.</param>
        /// <param name="elements">Набор элементов, к которым внешнее приложение
        /// можно добавлять элементы, которые будут подсвечены в случае сбоя или отмены.</param>
        /// <returns>Вернем статус внешней команды.
        /// Результат Succeeded означает, что внешний метод API сработал должным образом.
        /// Отменено может использоваться для обозначения того, что пользователь отменил внешнюю операцию
        /// в какой-то момент. Ошибка должна быть возвращена, если приложение не может продолжить работу.
        /// операция.</returns>
		
        public Autodesk.Revit.UI.Result Execute(Autodesk.Revit.UI.ExternalCommandData commandData,
            ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
           Transaction tran = new Transaction(commandData.Application.ActiveUIDocument.Document, "Generate Floor");
           tran.Start();

            try
            {
                if (null == commandData)
                {
                    throw new ArgumentNullException("commandData");
                }

                Data data = new Data();
                data.ObtainData(commandData);

                GenerateFloorForm dlg = new GenerateFloorForm(data);

                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    CreateFloor(data, commandData.Application.ActiveUIDocument.Document);

                    tran.Commit();
                    return Autodesk.Revit.UI.Result.Succeeded;
                }
                else
                {
                   tran.RollBack();
                    return Autodesk.Revit.UI.Result.Cancelled;
                }                
            }
            catch (Exception e)
            {
                message = e.Message;
                tran.RollBack();
                return Autodesk.Revit.UI.Result.Failed;
            }
        }

        #endregion IExternalCommand Members Implementation

        /// <summary>
        /// создайте пол по данным, полученным из Revit.
        /// </summary>
        /// <param name="data">Данные, включая профиль, уровень и т. д., необходимые для создания пола.</param>
        /// <param name="doc">Получает объект, представляющий текущий активный проект.</param>

        static public void CreateFloor(Data data, Document doc)
        {
            CurveLoop loop = new CurveLoop();
            foreach (Curve curve in data.Profile)
            {
                loop.Append(curve);
            }

            List<CurveLoop> floorLoops = new List<CurveLoop> { loop };

            Floor.Create(doc, floorLoops, data.FloorType.Id, data.Level.Id, data.Structural, null, 0.0);
        }
    }
}
