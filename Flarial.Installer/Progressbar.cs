using System.Windows.Forms;

namespace Flarial.Installer
{
    public partial class Progressbar : Form
    {
        public Progressbar()
        {
            InitializeComponent();            
        }

        public ProgressBar GetProgressBar()
        {
            return progressBar1;
        }
    }
}
