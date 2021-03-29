using System;
using System.Collections.Generic;
using System.Text;

namespace Geometry
{
    public class Point
    {
        public double x, y;

        public Point(double x_, double y_)
        {
            x = x_;
            y = y_;
        }
    }

    public class Color
    {
        public byte r, g, b, a;

        public Color(int r_, int g_, int b_, int a_)
        {
            r = (byte)r_;
            g = (byte)g_;
            b = (byte)b_;
            a = (byte)a_;
        }

        public Color(byte r_, byte g_, byte b_, byte a_)
        {
            r = r_;
            g = g_;
            b = b_;
            a = a_;
        }
    }

    public class Material
    {
        public Color Color { get; set; }

        public Material(int r_, int g_, int b_, int a_)
        {
            Color = new Color(r_, g_, b_, a_);
        }

        public Material(byte r_, byte g_, byte b_, byte a_)
        {
            Color = new Color(r_, g_, b_, a_);
        }

        public Material(Color c)
        {
            Color = c;
        }
    }

    public class IFigureParameters
    {
        public Material Material { get; set; }
        public Color LineColor { get; set; }
        public double Thickness { get; set; }
        public int LineType { get; set; }
        public Color FillColor { get; set; }
        public Color BorderColor { get; set; }

        public IFigureParameters()
        {
            Geometry.Color c = new Geometry.Color(0, 0, 0, 255);

            Material = new Geometry.Material(c);
            LineColor = c;
            FillColor = c;
            BorderColor = c;
            Thickness = 1;
            LineType = 0;
        }

        public void setColor(Color c)
        {
            Material = new Geometry.Material(c);
            LineColor = c;
            FillColor = c;
            BorderColor = c;
        }
    }

    public interface IFigure
    {
        string Name { get; }
        IEnumerable<Point> Points(IFigureParameters parameters);
        void Scale(double x, double y);
        void MoveByVector(Point vector);
        void Rotate(double angle);
        bool IsIn(Point p);
        string ToJson();
    }

    class Line : IFigure
    {
        public string Id { get; set; }
        string IFigure.Name { get; }

        Point a, b, Center;
        double Angle;

        public Line(Point a, Point b)
        {
            this.a = a;
            this.b = b;
            Center = new Point(Math.Abs((a.x + b.x) / 2), Math.Abs((a.y + b.y) / 2));
        }

        public IEnumerable<Point> Points(IFigureParameters param)
        {
            return new[] { a, b };
        }

        public void Scale(double percentX, double percentY)
        {
            a = new Point(Center.x + percentX * (a.x - Center.x) * Math.Cos(Angle) - percentY * (a.y - Center.y) * Math.Sin(Angle), Center.y + percentX * (a.x - Center.x) * Math.Sin(Angle) + percentY * (a.y - Center.y) * Math.Cos(Angle));
            b = new Point(Center.x + percentX * (b.x - Center.x) * Math.Cos(Angle) - percentY * (b.y - Center.y) * Math.Sin(Angle), Center.y + percentX * (b.x - Center.x) * Math.Sin(Angle) + percentY * (b.y - Center.y) * Math.Cos(Angle));
        }

        public void MoveByVector(Point vector)
        {
            a = new Point(a.x + vector.x, a.y + vector.y);
            b = new Point(b.x + vector.x, b.y + vector.y);
            Center = new Point(Center.x + vector.x, Center.y + vector.y);
        }

        public void Rotate(double deltaAngle)
        {
            Angle += deltaAngle;
            a = new Point(Center.x + (a.x - Center.x) * Math.Cos(Angle) - (a.y - Center.y) * Math.Sin(Angle), Center.y + (a.x - Center.x) * Math.Sin(Angle) + (a.y - Center.y) * Math.Cos(Angle));
            b = new Point(Center.x + (b.x - Center.x) * Math.Cos(Angle) - (b.y - Center.y) * Math.Sin(Angle), Center.y + (b.x - Center.x) * Math.Sin(Angle) + (b.y - Center.y) *Math.Cos(Angle));
        }

        public bool IsIn(Point p)
        {
            double m = (b.y - a.y) / (b.x - a.x);
            if (Math.Abs(p.y - (m * p.x + a.y - m * a.x)) < 0.1)
                return true;
            else return false;
        }

        public string ToJson()
        {
            throw new NotImplementedException();
        }
    }

    class Rectangle : IFigure
    {
        public string Id { get; set; }
        string IFigure.Name { get; }

        Point lt, rb, lb, rt, Center; //lefttop, rightbottom, leftbottom, righttop
        double Angle;

        public Rectangle(Point lt, Point rb)
        {
            this.lt = lt;
            this.rb = rb;
            lb = new Point(lt.x, rb.y);
            rt = new Point(rb.x, lt.y);

            Angle = 0;
            Center = new Point((rb.x + lt.x) / 2, (rb.y + lt.y) / 2);
        }

        public IEnumerable<Point> Points(IFigureParameters param)
        {
            return new[] { lt, lb, rb, rt };
        }

        public void Scale(double percentX, double percentY)
        {
            lt = new Point(Center.x + percentX * (lt.x - Center.x) * Math.Cos(Angle) - percentY * (lt.y - Center.y) * Math.Sin(Angle), Center.y + percentX * (lt.x - Center.x) * Math.Sin(Angle) + percentY * (lt.y - Center.y) * Math.Cos(Angle));
            lb = new Point(Center.x + percentX * (lb.x - Center.x) * Math.Cos(Angle) - percentY * (lb.y - Center.y) * Math.Sin(Angle), Center.y + percentX * (lb.x - Center.x) * Math.Sin(Angle) + percentY * (lb.y - Center.y) * Math.Cos(Angle));
            rt = new Point(Center.x + percentX * (rt.x - Center.x) * Math.Cos(Angle) - percentY * (rt.y - Center.y) * Math.Sin(Angle), Center.y + percentX * (rt.x - Center.x) * Math.Sin(Angle) + percentY * (rt.y - Center.y) * Math.Cos(Angle));
            rb = new Point(Center.x + percentX * (rb.x - Center.x) * Math.Cos(Angle) - percentY * (rb.y - Center.y) * Math.Sin(Angle), Center.y + percentX * (rb.x - Center.x) * Math.Sin(Angle) + percentY * (rb.y - Center.y) * Math.Cos(Angle));
        }

        public void MoveByVector(Point vector)
        {
            lt = new Point(lt.x + vector.x, lt.y + vector.y);
            lb = new Point(lb.x + vector.x, lb.y + vector.y);
            rt = new Point(rt.x + vector.x, rt.y + vector.y);
            rb = new Point(rb.x + vector.x, rb.y + vector.y);
            Center = new Point((rb.x + lt.x) / 2, (rb.y + lt.y) / 2);
        }

        public void Rotate(double deltaAngle)
        {
            Angle += deltaAngle;
            lt = new Point(Center.x + (lt.x - Center.x) * Math.Cos(Angle) - (lt.y - Center.y) * Math.Sin(Angle), Center.y + (lt.x - Center.x) * Math.Sin(Angle) + (lt.y - Center.y) * Math.Cos(Angle));
            lb = new Point(Center.x + (lb.x - Center.x) * Math.Cos(Angle) - (lb.y - Center.y) * Math.Sin(Angle), Center.y + (lb.x - Center.x) * Math.Sin(Angle) + (lb.y - Center.y) * Math.Cos(Angle));
            rt = new Point(Center.x + (rt.x - Center.x) * Math.Cos(Angle) - (rt.y - Center.y) * Math.Sin(Angle), Center.y + (rt.x - Center.x) * Math.Sin(Angle) + (rt.y - Center.y) * Math.Cos(Angle));
            rb = new Point(Center.x + (rb.x - Center.x) * Math.Cos(Angle) - (rb.y - Center.y) * Math.Sin(Angle), Center.y + (rb.x - Center.x) * Math.Sin(Angle) + (rb.y - Center.y) * Math.Cos(Angle));
        }

        //(Bx - Ax) * (Py - Ay) - (By - Ay) * (Px - Ax)
        public bool IsIn(Point p)
        {
            double p1 = (lb.x - lt.x)*(p.y - lt.y) - (lb.y - lt.y)*(p.x - lt.x);
            double p2 = (rb.x - lb.x)*(p.y - lb.y) - (rb.y - lb.y)*(p.x - lb.x);
            double p3 = (rt.x - rb.x)*(p.y - rb.y) - (rt.y - rb.y)*(p.x - rb.x);
            double p4 = (lt.x - rt.x)*(p.y - rt.y) - (lt.y - rt.y)*(p.x - rt.x);
            if ((p1 < 0 && p2 < 0 && p3 < 0 && p4 < 0) || (p1 > 0 && p2 > 0 && p3 > 0 && p4 > 0))
            return true;
            else return false;
        }

        public string ToJson()
        {
            throw new NotImplementedException();
        }
    }

    class Ellipse : IFigure
    {
        public string Id { get; set; }
        string IFigure.Name { get; }

        Point center;
        double R1, R2, Angle;
        public Ellipse(Point center, double R1, double R2)
        {
            this.center = center;
            this.R1 = R1;
            this.R2 = R2;
            Angle = 0;
        }

        public IEnumerable<Point> Points(IFigureParameters param)
        {
            List<Point> list = new List<Point>();

            Point l = new Point(center.x - R1, center.y);
            Point r = new Point(center.x + R1, center.y);
            Point b = new Point(center.x, center.y - R2);
            Point t = new Point(center.x, center.y + R2);

            list.Add(l);
            list.Add(t);
            list.Add(r);
            list.Add(b);

            return list as IEnumerable<Point>;
        }

        public void Scale(double percentX, double percentY)
        {
            R1 = R1 * percentX;
            R2 = R2 * percentY;
        }

        public void MoveByVector(Point vector)
        {
            center = new Point(center.x + vector.x, center.y + vector.y);
        }

        public void Rotate(double deltaAngle)
        {
            Angle += deltaAngle;
        }

        public bool IsIn(Point p)
        {
            if (1 < Math.Pow((p.x - center.x) * Math.Cos(Angle) + (p.y - center.y) * Math.Sin(-Angle), 2) / (R1 * R1) + Math.Pow(-(p.x - center.x) * Math.Sin(-Angle) + (p.y - center.y) * Math.Cos(Angle), 2) / (R2 * R2))
                return false;
            else return true;
        }

        public string ToJson()
        {
            throw new NotImplementedException();
        }
    }

    class Triangle : IFigure
    {
        public string Id { get; set; }
        string IFigure.Name { get; }

        Point center, b, c, r;
        double Angle;
        public Triangle(Point center, Point r)
        {
            Angle = 0;
            this.center = center;
            this.r = r;
            b = new Point(center.x + Math.Sqrt(3) * (r.y - center.y) / 2, center.y - (r.y - center.y) / 2);
            c = new Point(center.x - Math.Sqrt(3) * (r.y - center.y) / 2, center.y - (r.y - center.y) / 2);
        }

        public IEnumerable<Point> Points(IFigureParameters param)
        {
            return new[] { r, b, c };
        }

        public void Scale(double percentX, double percentY) //меняем через радиус описанной окружности
        {
            r = new Point(center.x + percentX * (r.x - center.x) * Math.Cos(Angle) - percentY * (r.y - center.y) * Math.Sin(Angle), center.y + percentX * (r.x - center.x) * Math.Sin(Angle) + percentY * (r.y - center.y) * Math.Cos(Angle));
            b = new Point(center.x + percentX * (b.x - center.x) * Math.Cos(Angle) - percentY * (b.y - center.y) * Math.Sin(Angle), center.y + percentX * (b.x - center.x) * Math.Sin(Angle) + percentY * (b.y - center.y) * Math.Cos(Angle));
            c = new Point(center.x + percentX * (c.x - center.x) * Math.Cos(Angle) - percentY * (c.y - center.y) * Math.Sin(Angle), center.y + percentX * (c.x - center.x) * Math.Sin(Angle) + percentY * (c.y - center.y) * Math.Cos(Angle));
        }

        public void MoveByVector(Point vector)
        {
            center = new Point(center.x + vector.x, center.y + vector.y);
            r = new Point(r.x + vector.x, r.y + vector.y);
            b = new Point(b.x + vector.x, b.y + vector.y);
            c = new Point(c.x + vector.x, c.y + vector.y);
        }

        public void Rotate(double deltaAngle)
        {
            Angle += deltaAngle;
            r = new Point(center.x + (r.x - center.x) * Math.Cos(Angle) - (r.y - center.y) * Math.Sin(Angle), center.y + (r.x - center.x) * Math.Sin(Angle) + (r.y - center.y) * Math.Cos(Angle));
            b = new Point(center.x + (b.x - center.x) * Math.Cos(Angle) - (b.y - center.y) * Math.Sin(Angle), center.y + (b.x - center.x) * Math.Sin(Angle) + (b.y - center.y) * Math.Cos(Angle));
            c = new Point(center.x + (c.x - center.x) * Math.Cos(Angle) - (c.y - center.y) * Math.Sin(Angle), center.y + (c.x - center.x) * Math.Sin(Angle) + (c.y - center.y) * Math.Cos(Angle));
        }


        public bool IsIn(Point p) //сумма площадей треугольников abp, bcp, acp равняется площади abc
        { //площадь треугольника через его вершины S=1/2|(x2-x1)(y3-y1)-(x3-x1)(x2-x1)|

          double p1 = 0.5 * Math.Abs((r.x - b.x) * (c.y - b.y) - (c.x - b.x) * (r.y - b.y));
          double p2 = 0.5 * Math.Abs((r.x - b.x) * (p.y - b.y) - (p.x - b.x) * (r.y - b.y));
          double p3 = 0.5 * Math.Abs((r.x - p.x) * (c.y - p.y) - (c.x - p.x) * (r.y - p.y));
          double p4 = 0.5 * Math.Abs((p.x - b.x) * (c.y - b.y) - (c.x - b.x) * (p.y - b.y));

           if (p1 != p2 + p3 + p4)
            return false;
           else return true;
        }

        public string ToJson()
        {
            throw new NotImplementedException();
        }
    }

    public static class FigureAssistant
    {
        public static IFigure CreateFromJson(string json) => throw new NotImplementedException();
        public static IFigure CreateByName(string typename, string name) => throw new NotImplementedException();
        public static IEnumerable<string> AvailableFigures() => throw new NotImplementedException();
    }

    public static class UserInterface
    {
        // нужна очередь для хранения всех нарисованных фигр, чтоб пользователь имел возможность выбрать из списка какую фигуру он хочет выбрать
    }
}