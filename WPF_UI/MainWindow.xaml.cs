using DocumentFormat.OpenXml.Drawing;
using ExtendPaint;
using SharpGL.SceneGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;
using Shape = System.Windows.Shapes.Shape;

namespace WPF_UI
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private enum ToolType { Line, Ellipse, Rectangle, Triangle }
        ToolType currentTool;
        Point startPoint;
        Shape currentShape;
        Brush currentBrush = Brushes.Black;
        MouseButtonState previousMouseEvent = new MouseButtonState();
        UndoRedoProvider undoRedo = new UndoRedoProvider();
        FormatWorker worker;


        Geometry.IFigureParameters parameters = new Geometry.IFigureParameters();

        private void menuOpen_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "SVG (*.svg)|*.svg|VGF (*.vgf)|*.vgf|JSON (*.json)|*.json",
                DefaultExt = "svg"
            };
            var DialougeResult = dialog.ShowDialog();
            if (DialougeResult == false) return;

            var extension = System.IO.Path.GetExtension(dialog.FileName);
            switch (extension.ToLower())
            {
                case ".svg":
                    worker = new SVGWorker(inkPanel, dialog.FileName);
                    break;
                case ".vgf":
                    worker = new VGFWorker(inkPanel, dialog.FileName);
                    break;
                case ".json":
                    worker = new JSONWorker(inkPanel, dialog.FileName);
                    break;
                default:
                    worker = null;
                    break;

            }
            if (worker == null) throw new ArgumentOutOfRangeException(extension);
            worker.Load();

        }

        private void menuSaveAs_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = "SVG (*.svg)|*.svg|PNG (*.png)|*.png|VGF (*.vgf)|*.vgf|JSON (*.json)|*.json",
                FileName = "picture",
                DefaultExt = "svg"
            };
            var DialougeResult = dialog.ShowDialog();
            if (DialougeResult == false) return;

            var extension = System.IO.Path.GetExtension(dialog.FileName);
            switch (extension.ToLower())
            {
                case ".png":
                    worker = new PNGWorker(inkPanel, dialog.FileName);
                    break;
                case ".svg":
                    worker = new SVGWorker(inkPanel, dialog.FileName);
                    break;
                case ".vgf":
                    worker = new VGFWorker(inkPanel, dialog.FileName);
                    break;
                case ".json":
                    worker = new JSONWorker(inkPanel, dialog.FileName);
                    break;
                default:
                    worker = null;
                    break;
            }

            if (worker == null) throw new ArgumentOutOfRangeException(extension);
            worker.Save();
        }
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void menuUndo_Click(object sender, RoutedEventArgs e)
        {
            undoRedo.Undo(1);
        }
        private void menuRedo_Click(object sender, RoutedEventArgs e)
        {
            undoRedo.Redo(1);
        }
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            Help help = new Help();
            help.Show();
        }

        private void clrPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            Geometry.Color c = new Geometry.Color(                
                myColorPicker.SelectedColor.Value.R,
                myColorPicker.SelectedColor.Value.G,
                myColorPicker.SelectedColor.Value.B,
                myColorPicker.SelectedColor.Value.A);

            parameters.setColor(c);

            inkPanel.DefaultDrawingAttributes.Color = Color.FromArgb(
                myColorPicker.SelectedColor.Value.A,
                myColorPicker.SelectedColor.Value.R,
                myColorPicker.SelectedColor.Value.G,
                myColorPicker.SelectedColor.Value.B);
        }

      
        private void SelectThickNess_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            setMode();
        }

        private void setMode()
        {
            if (SelectThickNess != null)
            {
                switch (SelectThickNess.SelectedIndex)
                {
                    case 0:
                        parameters.Thickness = 1;
                        break;
                    case 1:
                        parameters.Thickness = 3;
                        break;
                    case 2:
                        parameters.Thickness = 5;
                        break;
                    default:
                        break;
                }
            }
            

            /*if (btnEraser != null && inkPanel != null)
            {
                inkPanel.DefaultDrawingAttributes.Width = parameters.Thickness;
                inkPanel.EraserShape = new EllipseStylusShape(20, 20);
                

                if (btnEraser.IsChecked.Value)
                {
                   //draw(MouseEventArgs e);
                        //ластик
                    //inkPanel.EditingMode = InkCanvasEditingMode.EraseByPoint;

                }
                
                else
                {                   
                    inkPanel.EditingMode = InkCanvasEditingMode.None;                    
                }
                
            }*/
        }
        

        private void draw(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                switch (currentTool)
                {
                    case ToolType.Line:
                        drawLine(e);
                        break;
                    case ToolType.Ellipse:
                        drawEllipse(e, false);
                        break;
                    case ToolType.Rectangle:
                        drawRectangle(e);
                        break;
                    case ToolType.Triangle:
                        drawTriangle(e);
                        break;
                    default:
                        break;
                }
            }
            else if (e.LeftButton == MouseButtonState.Released && previousMouseEvent == MouseButtonState.Pressed)
            {
                DrawCommand command = new DrawCommand(currentShape, inkPanel);
                undoRedo.InsertComand(command);
            }
            previousMouseEvent = e.LeftButton;

        }

        private void btnEraser_Click(object sender, RoutedEventArgs e)
        {
         //   setMode();
        }

        private void toolChange(object sender, RoutedEventArgs e)
        {
            switch((tool.SelectedItem as ComboBoxItem).Name)
            {
                case "Line":
                    currentTool = ToolType.Line;
                    break;
                case "Ellipse":
                    currentTool = ToolType.Ellipse;
                    break;
                case "Rectangle":
                    currentTool = ToolType.Rectangle;
                    break;
                case "Triangle":
                    currentTool = ToolType.Triangle;
                    break;
                default:
                    break;
            }

            setMode();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (box.SelectedIndex)
            {
                case 0:
                    parameters.LineType = 0;
                    break;
                case 1:
                    parameters.LineType = 1;
                    break;
                default:
                    break;
            }
        }

        private void inkPanel_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(inkPanel);
            inkPanel.EditingMode = InkCanvasEditingMode.None;
            currentShape = new Line();
        }

        private void inkPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!btnEraser.IsChecked.Value)
                {
                    inkPanel.Children.Remove(currentShape);
                    draw(e);
                }
            }
        }

        private void inkPanel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            DrawCommand command = new DrawCommand(currentShape, inkPanel);
            undoRedo.InsertComand(command);

            if (!btnEraser.IsChecked.Value)
            {
                draw(e);
            }
            else if(btnEraser.IsChecked.Value)
            {
                currentTool = ToolType.Ellipse;
                draw(e);
            }
            if (e.LeftButton == MouseButtonState.Pressed && !btnEraser.IsChecked.Value)
            {
                inkPanel.Children.Remove(currentShape);
            }
           /*else if (e.LeftButton == MouseButtonState.Released && previousMouseEvent == MouseButtonState.Pressed)
            {*/
               
            //}
            previousMouseEvent = e.LeftButton;
        }

        private void bUndo_Click(object sender, RoutedEventArgs e)
        {
            undoRedo.Undo(1);
        }

        private void bRedo_Click(object sender, RoutedEventArgs e)
        {
            undoRedo.Redo(1);
        }
        
        private void bFill_Click(object sender, RoutedEventArgs e)
        {
            FillCommand command = new FillCommand(this.currentBrush, this.inkPanel);
            undoRedo.InsertComand(command);
            command.Execute();
        }
        
        private void drawLine(MouseEventArgs e)
        {
            Polygon line = new Polygon();
            line.Stroke = new SolidColorBrush(Color.FromArgb(parameters.LineColor.a,
                parameters.LineColor.r, parameters.LineColor.g,
                parameters.LineColor.b));
            line.StrokeThickness = parameters.Thickness;
 
            PointCollection myPointCollection = new PointCollection();

            foreach (var item in new Geometry.Line(
                new Geometry.Point(startPoint.X, startPoint.Y),
                new Geometry.Point(e.GetPosition(inkPanel).X,
                e.GetPosition(inkPanel).Y)).Points(parameters))
            {
                myPointCollection.Add(new Point(item.x, item.y));
            }

            line.Points = myPointCollection;
            inkPanel.Children.Add(line);
            currentShape = line;
        }
        
        private void drawEllipse(MouseEventArgs e, bool isEraser)
        {
            double R1, R2, l, r, b, t;

            l = startPoint.X;
            t = startPoint.Y;
            r = e.GetPosition(inkPanel).X;
            b = e.GetPosition(inkPanel).Y;

            R1 = (r - l) / 2.0;
            R2 = (t - b) / 2.0;

            Geometry.Point center = new Geometry.Point((l + r) / 2.0, (b + t) / 2.0);

            Polygon el = new Polygon();
            el.Stroke = new SolidColorBrush(Color.FromArgb(parameters.LineColor.a,
                parameters.LineColor.r, parameters.LineColor.g,
                parameters.LineColor.b));
            el.StrokeThickness = parameters.Thickness;

            PointCollection myPointCollection = new PointCollection();
            /*
            foreach (var item in new Geometry.Ellipse(
                center, R1, R2).Points(parameters))
            {
                myPointCollection.Add(new Point(item.x, item.y));
            }*/

            var item = new Geometry.Ellipse(center, R1, R2).Points(parameters);
            for (double Angle = 0; Angle <= 2 * Math.PI; Angle = Angle + 0.01)
            {
                myPointCollection.Add(new Point(0.5 * (l + r) + R1 * Math.Cos(Angle), 0.5 * (b + t) + R2 * Math.Sin(Angle)));
            }

            el.Points = myPointCollection;

            if (checkBoxFill.IsChecked == true)
            {
                el.Fill = new SolidColorBrush(Color.FromArgb(
                parameters.FillColor.a,
                parameters.FillColor.r,
                parameters.FillColor.g,
                parameters.FillColor.b));
            }

            inkPanel.Children.Add(el);
            
            currentShape = el;
        }
        
        private void drawRectangle(MouseEventArgs e)
        {
            Polygon rect = new Polygon();
            rect.Stroke = new SolidColorBrush(Color.FromArgb(parameters.LineColor.a,
                parameters.LineColor.r, parameters.LineColor.g,
                parameters.LineColor.b));
            rect.StrokeThickness = parameters.Thickness;

            PointCollection myPointCollection = new PointCollection();
            foreach (var item in new Geometry.Rectangle(
                new Geometry.Point(startPoint.X, startPoint.Y),
                new Geometry.Point(e.GetPosition(inkPanel).X,
                e.GetPosition(inkPanel).Y)).Points(parameters))
            {
                myPointCollection.Add(new Point(item.x, item.y));
            }

            rect.Points = myPointCollection;

            if (checkBoxFill.IsChecked == true)
            {
                rect.Fill = new SolidColorBrush(Color.FromArgb(
                parameters.FillColor.a,
                parameters.FillColor.r,
                parameters.FillColor.g,
                parameters.FillColor.b));
            }

            inkPanel.Children.Add(rect);
            currentShape = rect;
        }

        private void drawTriangle(MouseEventArgs e)
        {
            Geometry.Point center = new Geometry.Point(0, 0);
            Geometry.Point r = new Geometry.Point(0, 0);

            Polygon tr = new Polygon();
            tr.Stroke = new SolidColorBrush(Color.FromArgb(parameters.LineColor.a,
                parameters.LineColor.r, parameters.LineColor.g,
                parameters.LineColor.b));
            tr.StrokeThickness = parameters.Thickness;
            
            PointCollection myPointCollection = new PointCollection();
            foreach (var item in new Geometry.Triangle(
                new Geometry.Point(startPoint.X, startPoint.Y),
                new Geometry.Point(e.GetPosition(inkPanel).X, e.GetPosition(inkPanel).Y))
                .Points(parameters))
            {
                myPointCollection.Add(new Point(item.x, item.y));
            }

            tr.Points = myPointCollection;

            if (checkBoxFill.IsChecked == true)
            {
                tr.Fill = new SolidColorBrush(Color.FromArgb(
                parameters.FillColor.a,
                parameters.FillColor.r,
                parameters.FillColor.g,
                parameters.FillColor.b));
            }

            inkPanel.Children.Add(tr);
            currentShape = tr;
        }
      
    }
}
