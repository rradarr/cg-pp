﻿using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using System.Diagnostics;

namespace GraphicsBook
{
    /// <summary>
    /// Display and interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        GraphPaper gp = null;

        Dot[] myDots = null;
        Circle myCircle = null;
        Random autoRand = new Random();

        // Are we ready for interactions like slider-changes to alter the 
        // parts of our display (like polygons or images or arrows)? Probably not until those things 
        // have been constructed!
        bool ready = false;

        // Code to create and display objects goes here.
        public Window1()
        {
            InitializeComponent();
            InitializeCommands();

            // Now add some graphical items in the main drawing area, whose name is "Paper"
            gp = this.FindName("Paper") as GraphPaper;
            

            // Track mouse activity in this window
            MouseLeftButtonDown += MyMouseButtonDown;
            MouseLeftButtonUp += MyMouseButtonUp;
            MouseMove += MyMouseMove;

            myDots = new Dot[3];
            myDots[0] = new Dot(new Point(-40, 60));
            myDots[1] = new Dot(new Point(40, 60));
            myDots[2] = new Dot(new Point(40, -60));

            for(int i = 0; i < 3; i++) {
                myDots[i].MakeDraggable(gp);
                gp.Children.Add(myDots[i]);
            }

            myCircle = new Circle(new Point(0,0), 0.0);
            updateCircle(myDots[0].Position, myDots[1].Position, myDots[2].Position);
            gp.Children.Add(myCircle);

            ready = true; // Now we're ready to have sliders and buttons influence the display.
        }

        protected void updateCircle(Point P, Point Q, Point R) {
            if ((P.X == Q.X) & (P.Y == Q.Y)) {
                P.X += .001 * (2 * autoRand.NextDouble() - 1);
                P.Y += .001 * (2 * autoRand.NextDouble() - 1);
            }
            if ((Q.X == R.X) & (Q.Y == R.Y)) {
                R.X += .001 * (2 * autoRand.NextDouble() - 1);
                R.Y += .001 * (2 * autoRand.NextDouble() - 1);
            }

            Point A = P + (Q - P) / 2.0;
            Point B = Q + (R - Q) / 2.0;

            Vector v = Q - P;
            double tmp = v.X;
            v.X = v.Y;
            v.Y = -tmp;     // clockwise vector rotation by 90 deg
            v.Normalize();
            
            Vector w = R - Q;
            w.Normalize();
            tmp = w.X;
            w.X = w.Y;
            w.Y = -tmp;

            double a = v.X; double b = w.X; double c = v.Y; double d = w.Y;
            double det = a * d - b * c;
            // If the det is too small (or zero), we can fix it by moving the xcoordinates of v and w by 1/1000,
            // which is too small to have any visual impact.
            if (Math.Abs(det) < 1e-9) {
                a += .001;
                c += .001;
            }
            Vector target = (P - R) / 2.0;
            double t = (d * target.X - b * target.Y);
            double s = (-c * target.X + a * target.Y);
            t /= det;

            Point C = A + t * v;
            double r = (P - C).Length;
            myCircle.Position = C;
            myCircle.Radius = r;
        }

        #region Interaction handling -- sliders and buttons

        /* Click-handling in the main graph-paper window */
        public void MyMouseButtonUp(object sender, RoutedEventArgs e)
        {
            if (sender != this) return;
            System.Windows.Input.MouseButtonEventArgs ee =
              (System.Windows.Input.MouseButtonEventArgs)e;
            Debug.Print("MouseUp at " + ee.GetPosition(this));
            e.Handled = true;
        }

        public void MyMouseButtonDown(object sender, RoutedEventArgs e)
        {
            if (sender != this) return;
            System.Windows.Input.MouseButtonEventArgs ee =
              (System.Windows.Input.MouseButtonEventArgs)e;
            Debug.Print("MouseDown at " + ee.GetPosition(this));
            e.Handled = true;
        }


        public void MyMouseMove(object sender, MouseEventArgs e)
        {
            if (sender != this) return;
            System.Windows.Input.MouseEventArgs ee =
              (System.Windows.Input.MouseEventArgs)e;
            // Uncommment following line to get a flood of mouse-moved messages. 
            // Debug.Print("MouseMove at " + ee.GetPosition(this));
            e.Handled = true;
        }

        public void DrawCircleClick(object sender, RoutedEventArgs e) {
            Debug.Print("RedrawCircle clicked!\n");
            e.Handled = true; // don't propagate the click any further
            updateCircle(myDots[0].Position, myDots[1].Position, myDots[2].Position);
        }
        public void ResetClick(object sender, RoutedEventArgs e) {
            Debug.Print("Reset button clicked!\n");
            e.Handled = true; // don't propagate the click any further
            myDots[0].Position = new Point(-40, 60);
            myDots[1].Position = new Point(40, 60);
            myDots[2].Position = new Point(40, -60);
            updateCircle(myDots[0].Position, myDots[1].Position, myDots[2].Position);
        }
        #endregion

        #region Menu, command, and keypress handling

        protected static RoutedCommand ExitCommand;

        protected void InitializeCommands()
        {
            InputGestureCollection inp = new InputGestureCollection();
            inp.Add(new KeyGesture(Key.X, ModifierKeys.Control));
            ExitCommand = new RoutedCommand("Exit", typeof(Window1), inp);
            CommandBindings.Add(new CommandBinding(ExitCommand, CloseApp));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, CloseApp));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.New, NewCommandHandler));
        }

        void NewCommandHandler(Object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("You selected the New command",
                                Title,
                                MessageBoxButton.OK,
                                MessageBoxImage.Exclamation);

        }

        // Announce keypresses, EXCEPT for CTRL, ALT, SHIFT, CAPS-LOCK, and "SYSTEM" (which is how Windows 
        // seems to refer to the "ALT" keys on my keyboard) modifier keys
        // Note that keypresses that represent commands (like ctrl-N for "new") get trapped and never get
        // to this handler.
        void KeyDownHandler(object sender, KeyEventArgs e)
        {
            if ((e.Key != Key.LeftCtrl) &&
                (e.Key != Key.RightCtrl) &&
                (e.Key != Key.LeftAlt) &&
                (e.Key != Key.RightAlt) &&
                (e.Key != Key.System) &&
                (e.Key != Key.Capital) &&
                (e.Key != Key.LeftShift) &&
                (e.Key != Key.RightShift))
            {
                MessageBox.Show(String.Format("[{0}]  {1} received @ {2}",
                                        e.Key,
                                        e.RoutedEvent.Name,
                                        DateTime.Now.ToLongTimeString()),
                                Title,
                                MessageBoxButton.OK,
                                MessageBoxImage.Exclamation);
            }
        }

        void CloseApp(Object sender, ExecutedRoutedEventArgs args)
        {
            if (MessageBoxResult.Yes ==
                MessageBox.Show("Really Exit?",
                                Title,
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question)
               ) Close();
        }
        #endregion //Menu, command and keypress handling

        #region Image, Mesh, and Quiver, construction helpers
        private byte[, ,] createStripeImageArray()
        {
            int width = 128;
            int height = 128;
            byte[ , , ] pixelArray = new byte[width, height, 4];

            for (int y = 0; y < height; ++y)
            {
                int yIndex = y * width;
                for (int x = 0; x < width; ++x)
                {
                    byte b = (byte)(32 * (Math.Round((x + 2 * y) / 32.0)));
                    pixelArray[x, y, 0] = b;
                    pixelArray[x, y, 1] = b;
                    pixelArray[x, y, 2] = b;
                    pixelArray[x, y, 3] = 255;
                }
            }
            return pixelArray;
        }

        private int vectorIndex(int row, int col, int nrows, int ncols)
        {
            return col + row * ncols;
        }

        private Mesh createSampleMesh()
        {
            int nrows = 4;
            int ncols = 6;
            int nverts = nrows * ncols;
            int nedges = nrows * (ncols - 1) + ncols * (nrows - 1);
            int baseX = -40;
            int baseY = 55;
            Point[] verts = new Point[nverts];
            int[,] edges = new int[nedges, 2];

            for (int y = 0; y < nrows; y++)
            {
                for (int x = 0; x < ncols; x++)
                {
                    verts[vectorIndex(y, x, nrows, ncols)] =
                        new Point(baseX + 10 * x, baseY + 10 * y + 5 * Math.Sin(2 * Math.PI * x / (ncols - 1)));
                }
            }

            int count = 0;
            for (int y = 0; y < nrows; y++)
            {
                for (int x = 0; x < ncols - 1; x++)
                {
                    edges[count, 0] = vectorIndex(y, x, nrows, ncols);
                    edges[count, 1] = vectorIndex(y, x + 1, nrows, ncols);
                    count++;
                }
            }
            for (int x = 0; x < ncols; x++)
            {
                for (int y = 0; y < nrows - 1; y++)
                {
                    edges[count, 0] = vectorIndex(y, x, nrows, ncols);
                    edges[count, 1] = vectorIndex(y + 1, x, nrows, ncols);
                    count++;
                }
            }
            Debug.Print("count = " + count + "\n");
            return new Mesh(nverts, count, verts, edges);
        }

        private Quiver makeQuiver()
        {
            int count = 10;
            Point[] verts = new Point[count];
            Vector[] arrows = new Vector[count];
            for (int i = 0; i < count; i++)
            {
                double th = 2 * Math.PI * i / count;
                verts[i] = new Point(-40 + 5 * Math.Cos(th), -40 + 5 * Math.Sin(th));
                arrows[i] = new Vector(20 * Math.Cos(th), 20 * Math.Sin(th));
            }
            return new Quiver(verts, arrows);
        }
        #endregion
    }
}