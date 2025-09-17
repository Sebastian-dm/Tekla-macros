// ###########################################################################################
// ### Name           : Slope Measurer
// ### Date           : 2025
// ### Author         : Sebastian Meier
// ### Description    : 
// ###########################################################################################

using System;
using TSG3D = Tekla.Structures.Geometry3d;
using TSM = Tekla.Structures.Model;

namespace Tekla.Technology.Akit.UserScript
{
    public class Script
    {
        public static void Run(Tekla.Technology.Akit.IScript akit)
        {
            string Command_Interrupted = "Command interrupted by the user";
            
            try
            {
                // Pick points
                TSM.UI.Picker myPicker = new TSM.UI.Picker();
                var points = myPicker.PickPoints(TSM.UI.Picker.PickPointEnum.PICK_TWO_POINTS);
                TSG3D.Point point1 = points[0] as TSG3D.Point;
                TSG3D.Point point2 = points[1] as TSG3D.Point;
                
                // Calculate horizontal distance (in XY plane)
                double horizontalDistance = Math.Sqrt(Math.Pow(point2.X - point1.X, 2) + Math.Pow(point2.Y - point1.Y, 2));
                
                // Calculate vertical distance (Z difference)
                double verticalDistance = point2.Z - point1.Z;
                
                // Calculate slope
                if (Math.Abs(verticalDistance) > 0.001) // Avoid division by zero
                {
                    double slope = horizontalDistance / Math.Abs(verticalDistance);
                    double slopePerMille = Math.Abs(verticalDistance) / horizontalDistance * 1000;
                    
                    // Format the slope as 1:X.XX = Y.Y‰
                    string slopeMessage = string.Format("Slope: 1:{0:F2} = {1:F1}‰", slope, slopePerMille);
                    
                    // Display the result
                    TSM.Operations.Operation.DisplayPrompt(slopeMessage);
                    
                    // Draw a line between the two points
                    TSM.UI.GraphicsDrawer graphicsDrawer = new TSM.UI.GraphicsDrawer();
					var color = new TSM.UI.Color(0.0, 0.0, 1.0);
                    
                    // Create a list of points for the PolyLine
                    var linePoints = new System.Collections.Generic.List<TSG3D.Point>();
                    linePoints.Add(point1);
                    linePoints.Add(point2);
                    
                    // Create a PolyLine with the points
                    TSG3D.PolyLine polyLine = new TSG3D.PolyLine(linePoints);
                    
                    // Create a GraphicPolyLine object
                    TSM.UI.GraphicPolyLine graphicPolyLine = new TSM.UI.GraphicPolyLine(
                        polyLine, color,
                        3, // Line width
                        TSM.UI.GraphicPolyLine.LineType.Solid
                    );
                    
                    // Draw the polyline
                    graphicsDrawer.DrawPolyLine(graphicPolyLine);
                    
                    // Calculate midpoint for text placement
                    TSG3D.Point midpoint = new TSG3D.Point(
                        (point1.X + point2.X) / 2,
                        (point1.Y + point2.Y) / 2,
                        (point1.Z + point2.Z) / 2
                    );
                    
                    // Draw text at midpoint
                    string displayText = string.Format("1:{0:F1} = {1:F1}PM", slope, slopePerMille);
                    graphicsDrawer.DrawText(midpoint, displayText, color);
                }
                else
                {
                    TSM.Operations.Operation.DisplayPrompt("Points are vertically aligned - slope is infinite");
                }
            }
            catch
            {
                TSM.Operations.Operation.DisplayPrompt(Command_Interrupted);
            }
        }
    }
}