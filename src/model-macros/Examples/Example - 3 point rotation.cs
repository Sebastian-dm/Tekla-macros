using System;
using TSG3D = Tekla.Structures.Geometry3d;
using TSM = Tekla.Structures.Model;

namespace Tekla.Technology.Akit.UserScript
{
    public class Script
    {
        public static void Run(Tekla.Technology.Akit.IScript akit)
        {
            string Pick_parts, Pick_Origin_Point_For_Rotation, Pick_Origin_Point, Pick_Destination_Point, Command_interrupted_180, Command_interrupted = string.Empty;

            try
            {
                TSM.Model myModel = new TSM.Model();
				
				Pick_parts = "Pick Parts then middle click to validate";
				Pick_Origin_Point_For_Rotation = "Pick origin point for rotation";
				Pick_Origin_Point = "Pick origin point";
				Pick_Destination_Point = "Pick destination point";
				Command_interrupted_180 = "Command interrupted. Angle is 180°";
				Command_interrupted = "Command interrupted by the user";

                TSM.UI.Picker myPicker2 = new TSM.UI.Picker();

                TSM.ModelObjectEnumerator SelectedObjects = myPicker2.PickObjects(TSM.UI.Picker.PickObjectsEnum.PICK_N_OBJECTS, Pick_parts);

                if (SelectedObjects.GetSize() > 0)
                {
                    TSM.UI.Picker myPicker = new TSM.UI.Picker();

                    TSG3D.Point pointRotation = myPicker.PickPoint(Pick_Origin_Point_For_Rotation);

                    TSG3D.Point pointOrigine = myPicker.PickPoint(Pick_Origin_Point);

                    TSG3D.Point pointDestination = myPicker.PickPoint(Pick_Destination_Point);

                    TSM.WorkPlaneHandler PlaneHandler = myModel.GetWorkPlaneHandler();

                    TSG3D.Vector vecteurOrigin = new TSG3D.Vector(pointRotation);
                    TSG3D.Vector vecteurXorigine = new TSG3D.Vector(pointOrigine.X - pointRotation.X, pointOrigine.Y - pointRotation.Y, pointOrigine.Z - pointRotation.Z);
                    TSG3D.Vector vecteurYorigine = new TSG3D.Vector(pointDestination.X - pointRotation.X, pointDestination.Y - pointRotation.Y, pointDestination.Z - pointRotation.Z);

                    double angle = (vecteurXorigine.GetAngleBetween(vecteurYorigine) * 180 / Math.PI);

                    if (angle < 2.5)
                    {
                        vecteurYorigine = new TSG3D.Vector(pointDestination.X - pointOrigine.X, pointDestination.Y - pointOrigine.Y, pointDestination.Z - pointOrigine.Z);
                    }

                    if (angle > 179)
                    {
                        TSM.Operations.Operation.DisplayPrompt(Command_interrupted_180);
                        return;
                    }

                    TSM.TransformationPlane scuPicked = new TSM.TransformationPlane(vecteurOrigin, vecteurXorigine, vecteurYorigine);
                    TSM.TransformationPlane currentPlane = PlaneHandler.GetCurrentTransformationPlane();
                    PlaneHandler.SetCurrentTransformationPlane(scuPicked);

                    TSG3D.Point pointRotationLocal = scuPicked.TransformationMatrixToLocal.Transform(currentPlane.TransformationMatrixToGlobal.Transform(pointRotation));
                    TSG3D.Point pointOrigineLocal = scuPicked.TransformationMatrixToLocal.Transform(currentPlane.TransformationMatrixToGlobal.Transform(pointOrigine));
                    TSG3D.Point pointDestinationLocal = scuPicked.TransformationMatrixToLocal.Transform(currentPlane.TransformationMatrixToGlobal.Transform(pointDestination));

                    TSG3D.Vector vecteurXorigineLocal = new TSG3D.Vector(pointOrigineLocal);
                    TSG3D.Vector vecteurYorigineLocal = new TSG3D.Vector(-(pointOrigineLocal.Y), pointOrigineLocal.X, 0);

                    TSG3D.Vector vecteurXDestinationLocal = new TSG3D.Vector(+(pointDestinationLocal.X - pointRotationLocal.X), pointDestinationLocal.Y - pointRotationLocal.Y, 0);
                    TSG3D.Vector vecteurYDestinationLocal = new TSG3D.Vector(-(vecteurXDestinationLocal.Y), vecteurXDestinationLocal.X, 0);

                    TSG3D.CoordinateSystem originCoorSys = new TSG3D.CoordinateSystem(pointRotationLocal, vecteurXorigineLocal, vecteurYorigineLocal);
                    TSG3D.CoordinateSystem destCoorSys = new TSG3D.CoordinateSystem(pointRotationLocal, vecteurXDestinationLocal, vecteurYDestinationLocal);

                    while (SelectedObjects.MoveNext())
                    {
                        if (SelectedObjects.Current != null)
                        {
                            TSM.Operations.Operation.MoveObject(SelectedObjects.Current, originCoorSys, destCoorSys);
                        }
                    }
                    PlaneHandler.SetCurrentTransformationPlane(currentPlane);

                    myModel.CommitChanges();
                }
            }
            catch
            {
                TSM.Operations.Operation.DisplayPrompt(Command_interrupted);
            }
        }
    }
}