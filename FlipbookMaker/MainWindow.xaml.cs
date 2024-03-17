using FlipbookMaker.Frontend.Viewmodels;
using System.Windows;

namespace FlipbookMaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainDataContext MainViewmodel => (MainDataContext)DataContext;

        public MainWindow()
        {
            InitializeComponent();

            MainViewmodel.PreviewImageControl = PreviewImage;
        }
    }
}
