using System.Drawing;
using System.Windows.Forms;


namespace Graphs
{
    public class GraphCreator
    {
        public static Bitmap? Create(Dictionary<DateTime, double> values, int maxWidth = 1920, int maxHeight = 1080)
        {
            if (values != null)
            {
                Font font = new Font("Multiround Pro", 20);
                Graphics gr = Graphics.FromImage(new Bitmap(1, 1));
                SizeF dateTimeSize = gr.MeasureString("00.00.0000", font);
                float width = 20 + 15 + 2 + (dateTimeSize.Width + 15) * values.Count();
                float height = 20 + 12 + 10;

                double rawMinVal = values.Values.Min();
                double rawMaxVal = values.Values.Max();

                double minVal = rawMinVal - rawMinVal % 10;
                double maxVal = rawMaxVal + (35 - rawMaxVal % 10);

                double valDiff = maxVal - minVal;

                height += ((float)valDiff / 50f) * (dateTimeSize.Height + 50);

                if (width > maxWidth) width = maxWidth;
                if (height > maxHeight) height = maxHeight;

                Bitmap bitmap = new Bitmap((int)Math.Ceiling(width), (int)Math.Ceiling(height));
                gr = Graphics.FromImage(bitmap);

                Pen pen = new Pen(Color.Black, 10);

                gr.DrawLine(pen, new PointF(0, height - 20 - dateTimeSize.Height), new PointF(width, height - 20 - dateTimeSize.Height));
                gr.DrawLine(pen, new PointF(gr.MeasureString(maxVal.ToString(), font).Width + 10, 0), new PointF(gr.MeasureString(maxVal.ToString(), font).Width + 10, height));

                double valOffset = valDiff / Math.Ceiling((height - 45f - dateTimeSize.Height) / (dateTimeSize.Height + 50));
                if (valOffset % 10 > 5) valOffset += 10 - valOffset % 10; else valOffset -= valOffset % 10;
                Console.WriteLine((valDiff / ((height - 45f - dateTimeSize.Height) / (dateTimeSize.Height + 20))).ToString() + ": " + valOffset.ToString());
                Console.WriteLine(Math.Ceiling((height - 45f - dateTimeSize.Height) / (dateTimeSize.Height + 50)));
                for (int i = 0; i < Math.Ceiling((height - 45f - dateTimeSize.Height) / (dateTimeSize.Height + 50)) + 1; i++)
                {
                    gr.DrawString((minVal + (valOffset * i)).ToString(), font, new SolidBrush(pen.Color), new PointF(0, height - 175 - (dateTimeSize.Height + 45) * i + 45 + dateTimeSize.Height));
                    if (width == maxWidth) gr.DrawString((minVal + (valOffset * i)).ToString(), font, new SolidBrush(pen.Color), new PointF(width - gr.MeasureString(maxVal.ToString(), font).Width - 5, height - 175 - (dateTimeSize.Height + 45) * i + 45 + dateTimeSize.Height));
                    Console.WriteLine((minVal + (valOffset * i)).ToString());
                }
                float minY = height - 175 + 45 + dateTimeSize.Height;
                float maxY = height - 175 - (dateTimeSize.Height + 45) * (float)(Math.Ceiling((height - 45f - dateTimeSize.Height) / (dateTimeSize.Height + 50))) + 45 + dateTimeSize.Height;
                Console.WriteLine(nameof(minY) + ": " + minY + " " + nameof(maxY) + ": " + maxY);
                if (width < maxWidth)
                {
                    var valuesEnumerator = values.GetEnumerator();
                    PointF[] points = new PointF[values.Count];
                    for (int i = 0; i < values.Count; i++)
                    {
                        valuesEnumerator.MoveNext();
                        var item = valuesEnumerator.Current;
                        gr.DrawString(item.Key.ToString("dd.MM.yyyy"), font, new SolidBrush(pen.Color), new PointF(gr.MeasureString(maxVal.ToString(), font).Width + 25 + 2 + (dateTimeSize.Width + 5) * i, height - 25));
                        points[i] = new PointF(gr.MeasureString(maxVal.ToString(), font).Width + 25 + dateTimeSize.Width / 2 + (dateTimeSize.Width + 5) * (i), height - 30 - dateTimeSize.Height - item.Value == minVal ? 10 : (new NumInterval(0, height - 30 - dateTimeSize.Height).GetFromAnotherAgainstProportional((float)item.Value, new((float)rawMinVal, (float)rawMaxVal))));
                    }
                    gr.DrawCurve(pen, points);
                }
                else
                {
                    gr.DrawLine(pen, new PointF(width - gr.MeasureString(maxVal.ToString(), font).Width - 10, 0), new PointF(width - gr.MeasureString(maxVal.ToString(), font).Width - 10, height));
                    DateTime start = values.Keys.First();
                    DateTime end = values.Keys.Last();
                    TimeSpan dateSpan = end - start;
                    double offset = dateSpan.Days / 9d;
                    Console.WriteLine($"daysOffset: {offset}");
                    for (int i = 0; (i < 10); i++)
                    {
                        DateTime curr = start.AddDays(offset * i);
                        Console.WriteLine($"{i}. TotalOffset: {offset * i}");
                        gr.DrawString(curr.ToString("dd.MM.yyyy"), font, new SolidBrush(pen.Color), new PointF(gr.MeasureString(maxVal.ToString(), font).Width + 7 + 2 + 10 + (dateTimeSize.Width + 15 + (dateTimeSize.Width / 10f) - (20 / 10f)) * i, height - 40));
                    }
                    float step = (width - 40 - (gr.MeasureString(maxVal.ToString(), font).Width * 2)) / values.Count;
                    float prevoiusGroupLength = 0;
                    short barrier = values.Count > 365
                        ? (short)365 : (short)values.Count;
                    bool isNeededToCompact = values.Count > 365;
                    PointF[] pointsM = new PointF[barrier];
                    double[] valuesArray = values.Values.ToArray();
                    if (isNeededToCompact)
                    {
                        step = (width - 40 - (gr.MeasureString(maxVal.ToString(), font).Width * 2)) / 365f;
                        float currGroupLength = values.Count / 365f;
                        if (currGroupLength == Math.Ceiling(currGroupLength))
                        {
                            float[] GroupValues = new float[(int)Math.Ceiling(currGroupLength)];
                            float GroupValue = 0;
                            for (int i = 0; (i < 365); i++)
                            {
                                for (int j = 0; (j < currGroupLength); j++)
                                {
                                    GroupValues[j] = (float)valuesArray[j + i * (int)currGroupLength];
                                }
                                GroupValue = GroupValues.Sum() / GroupValues.Length;
                                pointsM[i] = new PointF(gr.MeasureString(maxVal.ToString(), font).Width + 30 + step * (i), 30 - dateTimeSize.Height - GroupValue == minVal ? 10 : (new NumInterval(maxY, minY).GetFromAnother(GroupValue, new((float)minVal, (float)maxVal))));
                            }
                        }
                        else
                        {
                            float[] GroupValues = new float[(int)Math.Truncate(currGroupLength) + 1];
                            float GroupValue = 0;
                            float daysRemnant = (float)Math.Truncate(((currGroupLength - (float)Math.Truncate(currGroupLength)) * 365f));
                            int RemnantDaysInclueded = 0;
                            for (int i = 0; (i < 365); i++)
                            {
                                for (int j = 0; (j < Math.Truncate(currGroupLength) + (i < daysRemnant ? 1 : 0)); j++)
                                {
                                    GroupValues[j] = (float)valuesArray[j + i * ((int)currGroupLength) + RemnantDaysInclueded];
                                }
                                if (i < daysRemnant) RemnantDaysInclueded++; else GroupValues[GroupValues.Length - 1] = 0;
                                GroupValue = GroupValues.Sum() / (GroupValues.Length - (i < daysRemnant ? 0 : 1));
                                pointsM[i] = new PointF(gr.MeasureString(maxVal.ToString(), font).Width + 30 + step * (i), new NumInterval(maxY, minY).GetFromAnother(GroupValue, new((float)rawMinVal, (float)rawMaxVal)));
                            }
                        }

                    }
                    else
                    {
                        for (int i = 0; i < barrier; i++)
                        {

                            var item = values.Values.ToArray()[i];
                            pointsM[i] = new PointF(gr.MeasureString(maxVal.ToString(), font).Width + 30 + step * (i), height - 30 - dateTimeSize.Height - item == minVal ? 10 : (new NumInterval(10, height - 40 - dateTimeSize.Height).GetFromAnother((float)item, new((float)minVal, (float)maxVal))));

                            Console.WriteLine(pointsM[i].X + " x " + pointsM[i].Y + " y value " + item);
                        }
                    }
                    gr.DrawCurve(new Pen(Brushes.Black, 3), pointsM, 0);
                }
                return bitmap;
            }
            int cock;
            throw new ArgumentNullException(nameof(values));


        }
    }

    

    public class NumInterval
    {
        public float min { get; private set; }
        public float max { get; private set; }
        public float length { get; private set; }
        public NumInterval(float min, float max)
        {
            this.min = min;
            this.max = max;
            length = max - min;
        }

        public float GetFromAnother(float value, NumInterval interval)
        { 
            float shiftedValue = value - interval.min;
            return shiftedValue / interval.length * length;
        }

        public float GetFromAnotherAgainstProportional(float value, NumInterval interval)
        {
            float shiftedValue = value - interval.min;
            return (1f - (shiftedValue / interval.length)) * length;
        }
    }

    public static class ArrayExctentions
    {
        public static string ArrayToString(this Array floats, string sep = "")
        {
            string res = "";
            foreach (var item in floats)
            {
                res += item.ToString() + sep;
            }
            return res;
        }
    }
}
