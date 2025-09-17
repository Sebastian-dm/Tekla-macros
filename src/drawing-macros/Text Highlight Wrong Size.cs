// Written by Sebastian Meier dec. 2021.
//
// Marks the text to print in red if the text size is smaller than specified

using Tekla.Structures.Drawing;
using Tekla.Technology.Akit;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;


namespace Tekla.Technology.Akit.UserScript
{
    public class Script
    {

        public static double textMinSize = 2.5;
        public static double textMaxSize = 99.0;

        public static void Run(IScript akit)
        {
            DrawingHandler dh = new DrawingHandler();
            DrawingObjectEnumerator objectsEnumerator = dh.GetActiveDrawing().GetSheet().GetAllObjects();

            int nText = 0;
            int nTextModified = 0;
            int nMark = 0;
            int nDim = 0;
            
            List<double> textSizes = new List<double>();

            while (objectsEnumerator.MoveNext()) {
                DrawingObject obj = objectsEnumerator.Current;

                if (obj as Text != null) {
                    Text TextObj = (Text)obj;
                    textSizes.Add(TextObj.Attributes.Font.Height);
                    nTextModified += CheckTextSize(TextObj, textMinSize, textMaxSize);
                    nText++;
                }
                //else if (obj as MarkBase != null) {
                //    MarkBase MarkObj = (MarkBase)obj;
                //    textSizes.Add(MarkObj.Attributes.Font.Height);
                //    nTextModified += CheckMarkSize(MarkObj, textMinSize, textMaxSize);
                //    nText++;
                //}


            }
            textSizes.Sort();
            MessageBox.Show(string.Concat("Checked:\n",
                nText.ToString(), " text objects (", nTextModified.ToString(), " were modified)\n",
            //    nMark.ToString()+ " mark objects\n",
            //    nDim.ToString() + " dimension objects\n"
                "Used Text sizs: ", string.Join("; ", textSizes.Distinct().ToArray())));
        }

        private static int CheckTextSize(Text textObject, double minSize, double maxSize)
        {
            if (textObject.Attributes.Font.Height < minSize || textObject.Attributes.Font.Height > maxSize) {
                textObject.Attributes.Font.Color = DrawingColors.Yellow;
                textObject.Modify();
                return 1;
            }
            else {
                return 0;
            }
        }

        //private static int CheckMarkSize(MarkBase MarkObject, double minSize, double maxSize) {
        //    DrawingObjectEnumerator ObjEnum = MarkObject.GetObjects();
        //    while (ObjEnum.MoveNext()) {
        //        DrawingObject MarkSubObj = ObjEnum.Current;

        //        if (MarkSubObj.Attributes.Font.Height < minSize || MarkObject.Attributes.Font.Height > maxSize) {
        //            MarkSubObj.Attributes.Font.Color = DrawingColors.Yellow;
        //            MarkSubObj.Modify();
        //        return 1;
        //    }
        //    else {
        //        return 0;
        //    }
        //}
    }
}
