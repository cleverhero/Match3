using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel;

namespace Match3
{
    class Game: ApplicationContext
    {
        private StartForm startform;
        private GameForm gameform;

        public Game()
        {
            startform = new StartForm();
            startform.Closed += new EventHandler(OnStartFormClosed);
            startform.Show();
        }

        private void OnStartFormClosed(object sender, EventArgs e)
        {
            if (this.startform.State == StartFormState.WaitNewGame)
            {
                gameform = new GameForm();
                gameform.Closed += new EventHandler(OnGameFormClosed);
                gameform.Show();
            }
            else
            {
                this.ExitThread();
            }
        }

        private void OnGameFormClosed(object sender, EventArgs e)
        {
            MessageBox.Show("Game Over");

            startform = new StartForm();
            startform.Closed += new EventHandler(OnStartFormClosed);
            startform.Show();
        }
    }
}
