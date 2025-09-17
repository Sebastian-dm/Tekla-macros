// ###########################################################################################
// ### Name           : North Arrow
// ### Date           : 24.10.2023
// ### Author         : Sebastian Meier
// ### Description    : Inserts a north arrow symbolt at the location picked by the user.
// ###                  The rotation of the north arrow is set in relation to the rotation of
// ###                  the base point and the drawing view.
// ###########################################################################################


using System;
using System.Linq;
using System.Collections.Generic;
using Tekla.Structures.Model.Operations;
using System.Windows.Forms;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Drawing;

using View = Tekla.Structures.Drawing.View;

namespace Tekla.Technology.Akit.UserScript
{
    public class Script
    {
        private const string SymbolFile = "BRO";
        private const int SymbolIndex = 31;

        private static DrawingHandler drawingHandler;
        private static Drawing activeDrawing;

        private static Point pickedLocation;
        private static ViewBase pickedView;

        public static void Run(Tekla.Technology.Akit.IScript akit)
        {
            // Get drawing
            drawingHandler = new DrawingHandler();
            activeDrawing = drawingHandler.GetActiveDrawing();
            if (activeDrawing == null) {
                Operation.DisplayPrompt("You must have a drawing open first to use this tool.");
                return;
            }

            // Get picker
            var picker = drawingHandler.GetPicker();
            if (picker == null) {
                Operation.DisplayPrompt("Please open drawing editor.");
                return;
            }

            // Get location and view
            var pickings = picker.PickPoint("Pick location of north arrow.");
            pickedLocation = pickings.Item1;
            pickedView = pickings.Item2;
            if (pickedLocation == null || pickedView == null) {
                Operation.DisplayPrompt("Could not find picked location.");
                return;
            }

            InsertNorthArrow();
            activeDrawing.CommitChanges();
        }

        private static void InsertNorthArrow() {
            Symbol northArrowSymbol = new Symbol(pickedView, pickedLocation, new PointPlacing());
            northArrowSymbol.SymbolInfo.SymbolFile = SymbolFile;
            northArrowSymbol.SymbolInfo.SymbolIndex = SymbolIndex;
            northArrowSymbol.Attributes.Height = 70;
            northArrowSymbol.Attributes.Angle = AngleToNorth((View)pickedView);
            northArrowSymbol.Insert();
        }

        private static double AngleToNorth(View view) {

            if (!IsPlan(view)) {
                Operation.DisplayPrompt("North arrow angle only works in plan views. Arrow inserted with 0 angle.");
                return 0;
            }

            double angleBasePoint = RotationOfBasePoint();
            double angleviewRotation = RotationOfView(view);

            string debugMessage = "Base point angle to north: " + angleBasePoint.ToString()
                + ",   View rotation angle: " + angleviewRotation.ToString();
            Operation.DisplayPrompt(debugMessage);

            return angleBasePoint + angleviewRotation;
        }

        private static bool IsPlan(View view) {
            CoordinateSystem CoordSys = view.DisplayCoordinateSystem;
            Vector normalVector = CoordSys.AxisX.Cross(CoordSys.AxisY).GetNormal();
            double angleToUp = normalVector.GetAngleBetween(new Vector(0, 0, 1));
            if (angleToUp < 0.1)
                return true;
            else
                return false;
        }

        private static double RotationOfBasePoint() {

            BasePoint projectBasePoint = null;
            foreach (BasePoint basePoint in ProjectInfo.GetBasePoints()) {
                if (basePoint.IsProjectBasePoint)
                    projectBasePoint = basePoint;
            }

            if (projectBasePoint == null) {
                Operation.DisplayPrompt("No Project basepoint found. Assuming 0 basepoint rotation.");
                return 0;
            }

            double angleBasePoint = projectBasePoint.AngleToNorth * 57.295780; // Convert degrees

            return angleBasePoint;
        }

        private static double RotationOfView(View view) {
            CoordinateSystem viewCoordinateSystem = view.DisplayCoordinateSystem;
            Vector viewXVector = viewCoordinateSystem.AxisX;

            double angleviewRotation = viewXVector.GetAngleBetween(new Vector(1, 0, 0)) * 57.295780;
            angleviewRotation = viewXVector.Y >= 0 ? -angleviewRotation : angleviewRotation;

            return angleviewRotation;
        }
    }
}
