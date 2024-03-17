using System.Windows.Controls;
using System.Windows.Input;

namespace FlipbookMaker.Frontend.Controls
{
    /// <summary>
    /// Interaction logic for MergeControl.xaml
    /// </summary>
    public partial class MergeControl : UserControl
    {
        public MergeControl()
        {
            InitializeComponent();
        }

        private void OnColumnTextPreview(object sender, TextCompositionEventArgs e)
        {
            if (!int.TryParse(e.Text, out int i))
            {
                e.Handled = true;
                return;
            }
        }
    }
}
