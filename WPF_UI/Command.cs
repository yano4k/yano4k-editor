using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace ExtendPaint
{
    interface ICommand
    {
        void Execute();
        void UnExecute();
    }

    public class DrawCommand : ICommand
    {
        private Shape shape;
        private InkCanvas inkcanvas;

        /// <summary>
        /// CANVAS
        /// </summary>
        /// <param name="shape"></param>
        /// <param name="inkcanvas"></param>
        public DrawCommand(Shape shape, InkCanvas inkcanvas)
        {
            this.shape = shape;
            this.inkcanvas = inkcanvas;
        }

        public void Execute()
        {
            inkcanvas.Children.Add(this.shape);
        }

        public void UnExecute()
        {
            inkcanvas.Children.Remove(this.shape);
        }
    }

    public class EffectCommand : ICommand
    {
        private Effect effect;
        private InkCanvas inkcanvas;

        public EffectCommand(Effect effect, InkCanvas inkcanvas)
        {
            this.inkcanvas = inkcanvas;
            this.effect = effect;
        }

        public void Execute()
        {
            foreach (Shape children in inkcanvas.Children)
            {
                children.Effect = effect;
            }
        }

        public void UnExecute()
        {
            foreach (Shape children in inkcanvas.Children)
            {
                children.Effect = null;
            }
        }
    }

    public class FillCommand : ICommand
    {
        private Brush previousBrush;
        private Brush currentBrush;
        private InkCanvas inkcanvas;

        public FillCommand(Brush brush, InkCanvas inkcanvas)
        {
            this.currentBrush = brush;
            this.inkcanvas = inkcanvas;
        }

        public void Execute()
        {

            previousBrush = inkcanvas.Background;
            inkcanvas.Background = currentBrush;
        }

        public void UnExecute()
        {
            inkcanvas.Background = previousBrush;
        }
    }
}
