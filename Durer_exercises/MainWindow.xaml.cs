using System;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows.Media.Media3D;
using GraphicsBook;

namespace GraphicsBook
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        GraphPaper gp = null; 
        bool ready = false;  // Flag for allowing sliders, etc., to influence display. 
        Object3D baseModel;
        Object3D model;
        public MainWindow()
        {
            InitializeComponent();
            InitializeCommands();

            // Now add some graphical items in the main Canvas, whose name is "GraphPaper"
            gp = this.FindName("Paper") as GraphPaper;
            
            const bool renderFaces = false;
            /*Point3D[] vtable = new Point3D[] 
            {
           /*0/new Point3D(-0.5, -0.5, 2.5),
           /*1/new Point3D(-0.5, 0.5, 2.5),
           /*2/new Point3D(0.5, 0.5, 2.5),
           /*3/new Point3D(0.5, -0.5, 2.5),
           /*4/new Point3D(-0.5, -0.5, 3.5),
           /*5/new Point3D(-0.5, 0.5, 3.5),
            };
            EdgeIDs [] etable = new EdgeIDs[]{
                new EdgeIDs(0, 1), new EdgeIDs(1, 2), new EdgeIDs(2, 3), // one face
                new EdgeIDs(0, 4), new EdgeIDs(1, 5), new EdgeIDs(2, 6),  // joining edges
                new EdgeIDs(4, 5), new EdgeIDs(5, 6), new EdgeIDs(6, 7)}; // opposite face
            FaceIDs[] ftable = new FaceIDs[]
            {
                new FaceIDs(3, 2, 1, 0),   // front face
                new FaceIDs(4, 0, 1, 5),   // left face
                new FaceIDs(3, 7, 6, 2),   // right face
                new FaceIDs(1, 2, 6, 5),   // top face
                new FaceIDs(4, 7, 3, 0),   // bottom face
            };
            Object3D model = new Object3D(vtable, etable, ftable);*/
            baseModel = Object3D.CreateCube(new Point3D(0, 0, 3), 1, 1, 1);
            model = baseModel.rotatedParametric(0);

            render(renderFaces);
            
            ready = true; // Now we're ready to have sliders and buttons influence the display.
        }

        public void render(bool renderFaces) {
            double xmin = -0.5;
            double xmax = 0.5;
            double ymin = -0.5;
            double ymax = 0.5;

            Point[] pictureVertices = new Point[model.vertices.Length];
            double scale = 100;
            for (int i = 0; i < pictureVertices.Length; i++) {
                double x = model.vertices[i].X;
                double y = model.vertices[i].Y;
                double z = model.vertices[i].Z;
                double xprime = x / z;
                double yprime = y / z;
                pictureVertices[i].X = scale * (1 - (xprime - xmin) / (xmax - xmin));
                pictureVertices[i].Y = scale * (yprime - ymin) / (ymax - ymin); // x / z
                //gp.Children.Add(new Dot(pictureVertices[i].X, pictureVertices[i].Y));
            }

            if (renderFaces) {
                for (int i = 0; i < model.faces.Length; i++) {

                    Vector3D vec1 = model.GetFaceVertex(i, 1) - model.GetFaceVertex(i, 0);
                    Vector3D vec2 = model.GetFaceVertex(i, 2) - model.GetFaceVertex(i, 1);
                    Vector3D E = ((Vector3D)model.GetFaceVertex(i, 1));
                    Vector3D cross = Vector3D.CrossProduct(vec1, vec2);
                    double dot = Vector3D.DotProduct(E, cross);

                    if (dot > 0) {
                        for (int j = 0; j < 4; j++) {
                            gp.Children.Add(new Segment(pictureVertices[model.faces[i][j]], pictureVertices[model.faces[i][(j + 1) % 4]]));
                        }
                    }
                }
            } else {
                for (int i = 0; i < model.edges.Length; i++) {
                    int n1 = model.edges[i][0];
                    int n2 = model.edges[i][1];
                    gp.Children.Add(new Segment(pictureVertices[n1], pictureVertices[n2]));
                }
            }

            for (int i = 0; i < pictureVertices.Length; i++) {
                gp.Children.Add(new Dot(pictureVertices[i].X, pictureVertices[i].Y));
            }
        }

#region Interaction handling -- sliders and buttons
        /* Vestigial handling-code from Testbed2DApp -- unused in this project. */

        /* Event handler for a click on button one */
        public void b1Click(object sender, RoutedEventArgs e)
        {
            Debug.Print("Button one clicked!\n");
            e.Handled = true; // don't propagate the click any further
        }

        void slider1change(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Debug.Print("Slider changed, ready = " + ready + ", and val = " + e.NewValue + ".\n");
            e.Handled = true;
            if (ready)
            {
                model = baseModel.rotatedParametric(e.NewValue);
                render(false);
            }
        }
        public void b2Click(object sender, RoutedEventArgs e)
        {
            Debug.Print("Button two clicked!\n");
            e.Handled = true; // don't propagate the click any further
        }
#endregion
#region Menu, command, and keypress handling
        protected static RoutedCommand ExitCommand;
        protected void InitializeCommands()
        {
            InputGestureCollection inp = new InputGestureCollection();
            inp.Add(new KeyGesture(Key.X, ModifierKeys.Control));
            ExitCommand = new RoutedCommand("Exit", typeof(MainWindow), inp);
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
    }
}