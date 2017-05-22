using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

public enum StartFormState { WaitNewGame, None }

namespace Match3
{
    public partial class StartForm : Form
    {
        private StartFormState state;

        public StartFormState State
        {
            get { return state; }
        }

        public StartForm()
        {
            InitializeComponent();
            state = StartFormState.None;
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            state = StartFormState.WaitNewGame;
            this.Close();
        }
    }
}
