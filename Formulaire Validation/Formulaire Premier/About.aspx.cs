using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Formulaire_Premier
{
    public partial class About : System.Web.UI.Page
    {
        string prem, deux, trois;
        protected void Page_Load(object sender, EventArgs e)
        {
            StartTestButton.Click += new EventHandler(this.StartTestButton_Click);
        }

        void StartTestButton_Click(object sender, EventArgs e)
        {
            prem = this.Premier.Text;
            deux = this.Deuxieme.Text;
            trois = this.Troisieme.Text;

            if (prem == "Premier" && deux == "Deuxieme" && trois == "Troisieme")
            {
                StartTestButton.Text = "Test Passed";
            }

            else
            {
                StartTestButton.Text = "Test Failed";
            }
        }

        public bool TestPremier()
        {
            return (prem == "Premier");
        }

        public bool TestDeuxieme()
        {
            return (deux == "Deuxieme");
        }

        public bool TestTroisieme()
        {
            return (trois == "Troisieme");
        }
    }
}
