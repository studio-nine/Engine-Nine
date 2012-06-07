using System.Windows;

namespace Nine.Studio.Xna.Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += new RoutedEventHandler(MainWindow_Loaded);

        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //GameHost.Game = new Nine.Graphics.ParticleEffects.Design.DebuggerPrimitiveGame();
        }

        private void Panel_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {

        }
    }
}
