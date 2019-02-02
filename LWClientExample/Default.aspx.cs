using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.IO;


namespace LWClientExample
{
    public partial class _Default : System.Web.UI.Page
    {
        // Source language is English
        private string srcLang = "eng";
        // Target Language is French
        private string tgtLang = "fra";
        // UserName
        private string user;
        // Type of encoding
        private string encoding;

        private static string fileName;
        private static string reciveFilename;

        protected void Page_Load(object sender, EventArgs e)
        {

            // TextArea2  is a textarea that shows the translated text (Target text). 
            TextArea2.Visible = false;
            TextArea1.InnerText = TextArea1.InnerText.Trim();
        }

        protected void Submit1_ServerClick(object sender, EventArgs e)
        {
            // LWTRANSLI connects to  LW web service 
            LWTRANSLI _client = new LWTRANSLI();

            // determin the username. (If you specify a username the job is related to this user.)
            user = "";


            bool unknown = true;
            string fileType;

            encoding = "";
            int jobID = 0;

            // This path is a path for saving data on server.
            if (!Directory.Exists("c:\\temp\\"))
                Directory.CreateDirectory("c:\\temp\\");

            string path = "c:\\temp\\";
            reciveFilename = "";


            // check if there is an input for translation
            if (TextArea1.InnerText.Length == 0)
            {
                Page.ClientScript.RegisterStartupScript(GetType(), @"startup", @"<script>alert('Please enter valid input before submiting translation.');</script>");
            }


            try
            {
                // The input is a text
                string filePath = path + "\\temp.txt";
                if (File.Exists(filePath))
                    File.Delete(filePath);
                StreamWriter file = new StreamWriter(filePath);
                try
                {
                    file.Write(TextArea1.InnerText);

                }
                catch (Exception)
                {
                    Page.ClientScript.RegisterStartupScript(GetType(), @"startup", @"<script>alert('Error 1: An exception has been caught');</script>");
                }
                finally
                {
                    file.Close();
                }

                // send a file contains the source text to translator.
                fileType = "text/plain";
                fileName = filePath;
                jobID = _client.translate_File(fileName, fileType, encoding, srcLang, tgtLang, user, unknown);

                // receive translated text and save it in a file.
                reciveFilename = path + "\\tempTransli.txt";
                _client.receive(reciveFilename, jobID);


                // show the translated text in textArea
                TextArea2.Visible = true;
                StreamReader StreamReader1 = new StreamReader(reciveFilename);
                TextArea2.InnerText = StreamReader1.ReadToEnd();
                StreamReader1.Close();

                // Remove the job from Queue.
                _client.removeJobUser(user, jobID);

            }
            catch
            {
                Page.ClientScript.RegisterStartupScript(GetType(), @"startup", @"<script>alert('An exception has been caught');</script>");
            }

        }
    }
}

