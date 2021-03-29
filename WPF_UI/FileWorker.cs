using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace WPF_UI
{
    //общее представление фигуры в определенном формате
    class ConvertTarget
    {
        //SVG
        public XElement Element { get; set; }
        public Point Ratio { get; set; }

        //VGF
        public string[] Array { get; set; }

        public ConvertTarget(XElement element, Point ratio)
        {
            Element = element;
            Ratio = ratio;
            Array = new string[0];
        }

        public ConvertTarget(string[] array)
        {
            Element = null;
            Ratio = new Point(1.0, 1.0);
            Array = array;
        }
    }

    //преобразование фигуры
    abstract class Converter
    {
        public string Format { get; set; }

        public Converter(string format)
        {
            Format = format;
        }

        abstract public string ShapeToFormat(Shape shape);
        abstract public Shape FormatToShape(ConvertTarget Target);

        protected string brushToString(Brush brush)
        {
            //перевод из С# (ARGB) в HEX (#RRGGBBAA)
            var c = ((SolidColorBrush)brush).Color;
            return String.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", c.R, c.G, c.B, c.A);
        }
        protected string dobuleToString(double db)
        {
            return db.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
        protected double stringToDouble(string str)
        {
            return Convert.ToDouble(str, System.Globalization.CultureInfo.InvariantCulture);
        }

        protected SolidColorBrush stringToBrush(string str)
        {
            //#RRGGBBAA -> ARGB 
            Color c = ((SolidColorBrush)((new BrushConverter()).ConvertFromString(str))).Color;
            //B -> A
            //A -> R
            //R -> G
            //G -> B
            return new SolidColorBrush((Color.FromArgb(c.B, c.A, c.R, c.G)));
        }

    }

    class LineConverter : Converter
    {
        public LineConverter(string format) : base(format) { }

        public override string ShapeToFormat(Shape shape)
        {
            Line line = (Line)shape;
            if (Format == "SVG")
            {
                XElement svgLine = new XElement("line");
                svgLine.Add(new XAttribute("x1", dobuleToString(line.X1)));
                svgLine.Add(new XAttribute("y1", dobuleToString(line.Y1)));
                svgLine.Add(new XAttribute("x2", dobuleToString(line.X2)));
                svgLine.Add(new XAttribute("y2", dobuleToString(line.Y2)));
                svgLine.Add(new XAttribute("stroke", brushToString(line.Stroke)));
                svgLine.Add(new XAttribute("stroke-width", line.StrokeThickness.ToString()));
                return svgLine.ToString();
            }
            if (Format == "VGF")
            {
                double X1 = line.X1;
                double Y1 = line.Y1;
                double X2 = line.X2;
                double Y2 = line.Y2;
                string stroke = line.Stroke.ToString();
                double thikness = line.StrokeThickness;
                return $"Line {X1} {Y1} {X2} {Y2} {stroke} {thikness};";
            }
            return "";
        }
        public override Shape FormatToShape(ConvertTarget Target)
        {

            Line line = new Line();
            if (Format == "SVG")
            {
                line.StrokeStartLineCap = PenLineCap.Round;
                line.StrokeEndLineCap = PenLineCap.Round;

                foreach (XAttribute k in Target.Element.Attributes())
                {
                    if (k.Name == "x1") { line.X1 = Target.Ratio.X * stringToDouble(k.Value); continue; }// = 217
                    if (k.Name == "y1") { line.Y1 = Target.Ratio.Y * stringToDouble(k.Value); continue; }// = 153.490909090909
                    if (k.Name == "x2") { line.X2 = Target.Ratio.X * stringToDouble(k.Value); continue; }// = 506
                    if (k.Name == "y2") { line.Y2 = Target.Ratio.Y * stringToDouble(k.Value); continue; }// = 311.490909090909
                    if (k.Name == "stroke") { line.Stroke = stringToBrush(k.Value); continue; }// = #0000FF
                    if (k.Name == "stroke-width") { line.StrokeThickness = stringToDouble(k.Value); continue; }// = 6
                    //Console.WriteLine(k.Name + " = " + k.Value);
                }

            }
            if (Format == "VGF")
            {
                if (Target.Array.Length != 7) line = null;//Некорректное количество аргументов у Линии

                line.StrokeStartLineCap = PenLineCap.Round;
                line.StrokeEndLineCap = PenLineCap.Round;
                line.X1 = Convert.ToDouble(Target.Array[1]);
                line.Y1 = Convert.ToDouble(Target.Array[2]);
                line.X2 = Convert.ToDouble(Target.Array[3]);
                line.Y2 = Convert.ToDouble(Target.Array[4]);
                line.Stroke = (SolidColorBrush)(new BrushConverter().ConvertFrom(Target.Array[5]));
                line.StrokeThickness = Convert.ToDouble(Target.Array[6]);
            }
            return line;

        }
    }

    class EllipseConverter : Converter
    {
        public EllipseConverter(string format) : base(format) { }

        public override string ShapeToFormat(Shape shape)
        {
            Ellipse ellipse = (Ellipse)shape;
            if (Format == "SVG")
            {
                double radiusX = ellipse.Width / 2;
                double radiusY = ellipse.Height / 2;
                XElement svgEllipse = new XElement("ellipse");
                svgEllipse.Add(new XAttribute("cx", dobuleToString(InkCanvas.GetLeft(ellipse) + radiusX)));
                svgEllipse.Add(new XAttribute("cy", dobuleToString(InkCanvas.GetTop(ellipse) + radiusY)));
                svgEllipse.Add(new XAttribute("rx", dobuleToString(radiusX)));
                svgEllipse.Add(new XAttribute("ry", dobuleToString(radiusY)));
                svgEllipse.Add(new XAttribute("fill", ellipse.Fill == null ? "none" : brushToString(ellipse.Fill)));
                svgEllipse.Add(new XAttribute("stroke", brushToString(ellipse.Stroke)));
                svgEllipse.Add(new XAttribute("stroke-width", ellipse.StrokeThickness.ToString()));
                return svgEllipse.ToString();
            }
            if (Format == "VGF")
            {
                double width = ellipse.Width;
                double height = ellipse.Height;
                double left = InkCanvas.GetLeft(ellipse);
                double top = InkCanvas.GetTop(ellipse);
                string stroke = ellipse.Stroke.ToString();
                double thikness = ellipse.StrokeThickness;
                string fill = ellipse.Fill == null ? "noFill" : ellipse.Fill.ToString();
                return $"Ellipse {left} {top} {width} {height} {stroke} {thikness} {fill};";
            }
            return "";
        }
        public override Shape FormatToShape(ConvertTarget Target)
        {
            Ellipse ellipse = new Ellipse();
            if (Format == "SVG")
            {
                double cx = 0, cy = 0;
                foreach (XAttribute k in Target.Element.Attributes())
                {
                    if (k.Name == "cx") { cx = Target.Ratio.X * stringToDouble(k.Value); continue; }// = 168.5
                    if (k.Name == "cy") { cy = Target.Ratio.Y * stringToDouble(k.Value); continue; }// = 143.990909090909
                    if (k.Name == "rx") { ellipse.Width = 2.0 * Target.Ratio.X * stringToDouble(k.Value); continue; }// = 62.5
                    if (k.Name == "ry") { ellipse.Height = 2.0 * Target.Ratio.Y * stringToDouble(k.Value); continue; }// = 68.5
                    if (k.Name == "fill") { ellipse.Fill = k.Value == "none" ? null : stringToBrush(k.Value); continue; }// = none
                    if (k.Name == "stroke") { ellipse.Stroke = stringToBrush(k.Value); continue; }// = #008000
                    if (k.Name == "stroke-width") { ellipse.StrokeThickness = stringToDouble(k.Value); continue; }// = 1
                    //Console.WriteLine(k.Name + " = " + k.Value);
                }
                InkCanvas.SetLeft(ellipse, cx - ellipse.Width / 2);
                InkCanvas.SetTop(ellipse, cy - ellipse.Height / 2);
            }
            if (Format == "VGF")
            {
                if (Target.Array.Length != 8) ellipse = null; //Некорректное количество аргументов у Эллипса
                InkCanvas.SetLeft(ellipse, Convert.ToDouble(Target.Array[1]));
                InkCanvas.SetTop(ellipse, Convert.ToDouble(Target.Array[2]));
                ellipse.Width = Convert.ToDouble(Target.Array[3]);
                ellipse.Height = Convert.ToDouble(Target.Array[4]);
                ellipse.Stroke = (SolidColorBrush)(new BrushConverter().ConvertFrom(Target.Array[5]));
                ellipse.StrokeThickness = Convert.ToDouble(Target.Array[6]);
                ellipse.Fill = Target.Array[7] == "noFill" ? null : (SolidColorBrush)(new BrushConverter().ConvertFrom(Target.Array[7]));
            }
            return ellipse;
        }
    }

    class RectangleConverter : Converter
    {
        public RectangleConverter(string format) : base(format) { }

        public override string ShapeToFormat(Shape shape)
        {
            Rectangle rect = (Rectangle)shape;
            if (Format == "SVG")
            {
                XElement svgRect = new XElement("rect");
                svgRect.Add(new XAttribute("x", dobuleToString(InkCanvas.GetLeft(rect))));
                svgRect.Add(new XAttribute("y", dobuleToString(InkCanvas.GetTop(rect))));
                svgRect.Add(new XAttribute("width", dobuleToString(rect.Width)));
                svgRect.Add(new XAttribute("height", dobuleToString(rect.Height)));
                svgRect.Add(new XAttribute("fill", rect.Fill == null ? "none" : brushToString(rect.Fill)));
                svgRect.Add(new XAttribute("stroke", brushToString(rect.Stroke)));
                svgRect.Add(new XAttribute("stroke-width", rect.StrokeThickness.ToString()));
                return svgRect.ToString();
            }
            if (Format == "VGF")
            {
                double width = rect.Width;
                double height = rect.Height;
                double left = InkCanvas.GetLeft(rect);
                double top = InkCanvas.GetTop(rect);
                string stroke = rect.Stroke.ToString();
                double thikness = rect.StrokeThickness;
                string fill = rect.Fill == null ? "noFill" : rect.Fill.ToString();
                return $"Rectangle {left} {top} {width} {height} {stroke} {thikness} {fill};";
            }
            return "";
        }
        public override Shape FormatToShape(ConvertTarget Target)
        {
            Rectangle rect = new Rectangle();
            if (Format == "SVG")
            {
                foreach (XAttribute k in Target.Element.Attributes())
                {
                    if (k.Name == "x") { InkCanvas.SetLeft(rect, Target.Ratio.X * stringToDouble(k.Value)); continue; }// = 168.5
                    if (k.Name == "y") { InkCanvas.SetTop(rect, Target.Ratio.Y * stringToDouble(k.Value)); continue; }// = 143.990909090909
                    if (k.Name == "width") { rect.Width = Target.Ratio.X * stringToDouble(k.Value); continue; }// = 62.5
                    if (k.Name == "height") { rect.Height = Target.Ratio.Y * stringToDouble(k.Value); continue; }// = 68.5
                    if (k.Name == "fill") { rect.Fill = k.Value == "none" ? null : stringToBrush(k.Value); continue; }// = none
                    if (k.Name == "stroke") { rect.Stroke = stringToBrush(k.Value); continue; }// = #008000
                    if (k.Name == "stroke-width") { rect.StrokeThickness = stringToDouble(k.Value); continue; }// = 1                                                                                           //Console.WriteLine(k.Name + " = " + k.Value);
                }
            }
            if (Format == "VGF")
            {
                if (Target.Array.Length != 8) rect = null;//Некорректное количество аргументов у Прямоугольника
                InkCanvas.SetLeft(rect, Convert.ToDouble(Target.Array[1]));
                InkCanvas.SetTop(rect, Convert.ToDouble(Target.Array[2]));
                rect.Width = Convert.ToDouble(Target.Array[3]);
                rect.Height = Convert.ToDouble(Target.Array[4]);
                rect.Stroke = (SolidColorBrush)(new BrushConverter().ConvertFrom(Target.Array[5]));
                rect.StrokeThickness = Convert.ToDouble(Target.Array[6]);
                rect.Fill = Target.Array[7] == "noFill" ? null : (SolidColorBrush)(new BrushConverter().ConvertFrom(Target.Array[7]));
            }
            return rect;
        }
    }

    class PolygonConverter : Converter
    {
        public PolygonConverter(string format) : base(format) { }

        public override string ShapeToFormat(Shape shape)
        {
            Polygon poly = (Polygon)shape;
            if (Format == "SVG")
            {
                // C# points  -> [1,5;10 20;2,7 30;30]
                // SVG points -> "1.5,10 20,2.7 30,30"
                var points = string.Join(" ", poly.Points.Select(point => $"{dobuleToString(point.X)},{dobuleToString(point.Y)}"));
                XElement svgPoly = new XElement("polygon");
                svgPoly.Add(new XAttribute("points", points));
                svgPoly.Add(new XAttribute("fill", poly.Fill == null ? "none" : brushToString(poly.Fill)));
                svgPoly.Add(new XAttribute("stroke", brushToString(poly.Stroke)));
                svgPoly.Add(new XAttribute("stroke-width", poly.StrokeThickness.ToString()));
                return svgPoly.ToString();
            }
            if (Format == "VGF")
            {
                var points = string.Join(" ", poly.Points.Select(point => $"{dobuleToString(point.X)} {dobuleToString(point.Y)}"));
                string stroke = poly.Stroke.ToString();
                string thikness = poly.StrokeThickness.ToString();
                string fill = poly.Fill == null ? "noFill" : poly.Fill.ToString();
                return $"Polygon {poly.Points.Count} {points} {stroke} {thikness} {fill};";
            }
            return "";
        }
        public override Shape FormatToShape(ConvertTarget Target)
        {
            Polygon poly = new Polygon();
            if (Format == "SVG")
            {
                poly.HorizontalAlignment = HorizontalAlignment.Left;
                poly.VerticalAlignment = VerticalAlignment.Center;
                foreach (XAttribute k in Target.Element.Attributes())
                {
                    if (k.Name == "points")
                    {
                        poly.Points = new PointCollection(k.Value.Split(' ').Select(point => new Point(Target.Ratio.X * stringToDouble(point.Split(',')[0]), Target.Ratio.Y * stringToDouble(point.Split(',')[1]))));
                        continue;
                    }
                    if (k.Name == "fill") { poly.Fill = k.Value == "none" ? null : stringToBrush(k.Value); continue; }// = none
                    if (k.Name == "stroke") { poly.Stroke = stringToBrush(k.Value); continue; }// = #008000
                    if (k.Name == "stroke-width") { poly.StrokeThickness = stringToDouble(k.Value); continue; }// = 1
                    //Console.WriteLine(k.Name + " = " + k.Value);
                }
            }
            if (Format == "VGF")
            {
                poly.HorizontalAlignment = HorizontalAlignment.Left;
                poly.VerticalAlignment = VerticalAlignment.Center;
                int pointNumber = 2* Convert.ToInt32(Target.Array[1]) + 2;
                poly.Points = new PointCollection();
                for (int i = 2; i < pointNumber; i += 2)
                    poly.Points.Add(new Point(stringToDouble(Target.Array[i]), stringToDouble(Target.Array[i + 1])));

                poly.Stroke = (SolidColorBrush)(new BrushConverter().ConvertFrom(Target.Array[pointNumber]));
                poly.StrokeThickness = Convert.ToDouble(Target.Array[pointNumber + 1]);
                poly.Fill = Target.Array[pointNumber + 2] == "noFill" ? null : (SolidColorBrush)(new BrushConverter().ConvertFrom(Target.Array[pointNumber + 2]));
            }
            return poly;
        }
    }
    //-------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------

    //преобразование холста
    abstract class FormatWorker
    {

        public string Format { get; set; }

        public InkCanvas Canvas { get; set; }
        public string Path { get; set; }

        public FormatWorker(InkCanvas canvas, string path)
        {
            Canvas = canvas;
            Path = path;
        }
        abstract public void Save();
        abstract public void Load();

        public string ProcessCanvas()
        {
            string formatData = "";
            Converter converter;
            foreach (Shape shape in Canvas.Children)
            {
                switch (shape.DependencyObjectType.Name)
                {
                    case "Line": { converter = new LineConverter(Format); break; }
                    case "Ellipse": { converter = new EllipseConverter(Format); break; }
                    case "Rectangle": { converter = new RectangleConverter(Format); break; }
                    case "Polygon": { converter = new PolygonConverter(Format); break; }
                    default: { converter = null; break; }
                }
                if (converter != null) formatData += converter.ShapeToFormat(shape);
            }
            return formatData;
        }

        protected double stringToDouble(string str)
        {
            return Convert.ToDouble(str, System.Globalization.CultureInfo.InvariantCulture);
        }

    }

    class SVGWorker : FormatWorker
    {
        public SVGWorker(InkCanvas canvas, string path) : base(canvas, path) { Format = "SVG"; }

        public override void Save()
        {
            string svgData = String.Format("<svg height=\"{0}\" width=\"{1}\" viewBox=\"0 0 {1} {0}\" >", (int)Canvas.ActualHeight, (int)Canvas.ActualWidth);
            svgData += ProcessCanvas() + "</svg>";
            File.WriteAllText(Path, svgData);
        }
        public override void Load()
        {
            XDocument svgParsed = XDocument.Parse(File.ReadAllText(Path));
            if (svgParsed.Root.Name != "svg") return;
            double ratioX = 1.0, ratioY = 1.0; //для масштабирования значений в фигурах
            Converter converter;
            //Descendants возвращает коллекцию всех тегов в файле
            foreach (XElement e in svgParsed.Descendants())
            {
                switch (e.Name.ToString())
                {
                    case "svg":
                        {
                            converter = null;
                            foreach (XAttribute k in e.Attributes())
                            {
                                if (k.Name == "height") { Canvas.Height = stringToDouble(k.Value); continue; }
                                if (k.Name == "width") { Canvas.Width = stringToDouble(k.Value); continue; }
                                if (k.Name == "viewBox")
                                {
                                    var split = k.Value.Split(' ').Select(stringToDouble).ToArray();
                                    ratioX = (Canvas.Width) / split[2];
                                    ratioY = (Canvas.Height) / split[3];
                                    continue;
                                }
                                //Console.WriteLine(k.Name + " = " + k.Value);
                            }
                            break;
                        }
                    case "line": { converter = new LineConverter(Format); break; }
                    case "ellipse": { converter = new EllipseConverter(Format); break; }
                    case "rect": { converter = new RectangleConverter(Format); break; }
                    case "polygon": { converter = new PolygonConverter(Format); break; }
                    default: { converter = null; break; }
                }
                if (converter != null)
                {
                    ConvertTarget target = new ConvertTarget(e, new Point(ratioX, ratioY));
                    Canvas.Children.Add(converter.FormatToShape(target));
                }
            }
        }
    }

    class VGFWorker : FormatWorker
    {
        public VGFWorker(InkCanvas canvas, string path) : base(canvas, path) { Format = "VGF"; }

        public override void Save()
        {
            string vgfData = $"{Canvas.ActualWidth} {Canvas.ActualHeight}:";
            File.WriteAllText(Path, vgfData + ProcessCanvas());

        }
        public override void Load()
        {
            string[] vgfData = File.ReadAllText(Path).Split(':');
            Canvas.Width = Convert.ToDouble(vgfData[0].Split(' ')[0]);
            Canvas.Height = Convert.ToDouble(vgfData[0].Split(' ')[1]);

            if (vgfData[1].Length < 1) return; //если в файле нет фигур
            Converter converter;
            foreach (var e in vgfData[1].Split(';'))
            {
                if (e.Length == 0) continue; //фиктивный элемент массива после последней ; (результат split)
                string[] shapeArray = e.Split(' ');
                switch (shapeArray[0])
                {
                    case "Line": { converter = new LineConverter(Format); break; }
                    case "Ellipse": { converter = new EllipseConverter(Format); break; }
                    case "Rectangle": { converter = new RectangleConverter(Format); break; }
                    case "Polygon": { converter = new PolygonConverter(Format); break; }
                    default: { converter = null; break; }
                }
                if (converter != null)
                {
                    ConvertTarget target = new ConvertTarget(shapeArray);
                    Canvas.Children.Add(converter.FormatToShape(target));
                }
            }
        }
    }

    class PNGWorker : FormatWorker
    {
        public PNGWorker(InkCanvas canvas, string path) : base(canvas, path)
        {
            Format = "PNG";
        }

        public override void Save()
        {
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                (int)Canvas.ActualWidth, (int)Canvas.ActualHeight,
                96d, 96d, PixelFormats.Pbgra32);

            Canvas.Measure(new Size((int)Canvas.ActualWidth, (int)Canvas.ActualHeight));
            Canvas.Arrange(new Rect(new Size((int)Canvas.ActualWidth, (int)Canvas.ActualHeight)));

            renderBitmap.Render(Canvas);

            var extension = System.IO.Path.GetExtension(Path);

            using (FileStream file = File.Create(Path))
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                encoder.Save(file);
                Canvas.InvalidateVisual();
            }

        }
        public override void Load()
        {
            //не нужен
        }
    }
}
