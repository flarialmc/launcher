using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
