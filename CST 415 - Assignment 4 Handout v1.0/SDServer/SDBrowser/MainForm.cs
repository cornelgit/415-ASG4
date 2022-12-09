// MainForm.cs
//
// Pete Myers
// CST 415
// Fall 2019
// 

using System;
using System.Windows.Forms;

namespace SDBrowser
{
    public partial class MainForm : Form
    {
        private ContentFetcher fetcher;

        public MainForm()
        {
            // default command line values
            string prsIP = "127.0.0.1";
            ushort prsPort = 30000;

            // -prs < PRS IP address>:< PRS port >
            // NOTE: args[0] is the name of the program, first true argument is at args[1]
            // string[] args = Environment.GetCommandLineArgs();
            string[] args = Environment.GetCommandLineArgs();

            // process the command line arguments to get the PRS ip address and PRS port number
            try
            {

                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "-prs")
                    {
                        if (i + 1 < args.Length)
                        {
                            // split serverIP:port
                            string[] parts = args[++i].Split(':');
                            prsIP = parts[0];
                            prsPort = ushort.Parse(parts[1]);
                        }
                        else
                        {
                            throw new Exception("-prs requires a value!");
                        }
                    }
                    else
                    {
                        // error! unexpected cmd line arg
                        throw new Exception("Invalid cmd line arg: " + args[i]);
                    }
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine("Error! " + ex.Message);
                return;
            }

            // instantiate the fetcher and add the support SD and FT protocols
            fetcher = new ContentFetcher();
            fetcher.AddProtocol("FT", new FTProtocolClient(prsIP, prsPort));
            fetcher.AddProtocol("SD", new SDProtocolClient(prsIP, prsPort));

            InitializeComponent();
        }

        private void buttonGo_Click(object sender, EventArgs e)
        {
            // user clicked the Go! button

            try
            {
                // grab the address from the address bar
                string address = textboxAddress.Text;

                if (string.IsNullOrWhiteSpace(address))
                {
                    throw new Exception("Hey! Enter an address!");
                }

                // fetch the content
                string content = fetcher.Fetch(address);

                // put the content in the content box
                textboxContent.Text = content;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);                           
            }            
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            // close the fetcher so it can close it's sessions with the servers
            fetcher.Close();
        }
    }
}
