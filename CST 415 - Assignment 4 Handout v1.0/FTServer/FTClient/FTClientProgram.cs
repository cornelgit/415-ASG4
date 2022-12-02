// FTClientProgram.cs
//
// Pete Myers
// CST 415
// Fall 2019
// 

using System;
using PRSLib;
using FTLib;

namespace FTClient
{
    class FTClientProgram
    {
        private static void Usage()
        {
            /*
                -prs <PRS IP address>:<PRS port>
                -s <file transfer server IP address>
                -d <directory requested>
            */
            Console.WriteLine("Usage: FTClient -d <directory> [-prs <PRS IP>:<PRS port>] [-s <FT Server IP>]");
        }

        static void Main(string[] args)
        {
            // defaults
            string PRSSERVER_IPADDRESS = "127.0.0.1";
            ushort PRSSERVER_PORT = 30000;
            string FTSERVICE_NAME = "FT Server";
            string FTSERVER_IPADDRESS = "127.0.0.1";
            ushort FTSERVER_PORT = 40000;
            string DIRECTORY_NAME = null;

            // process the command line arguments
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
                            PRSSERVER_IPADDRESS = parts[0];
                            PRSSERVER_PORT = ushort.Parse(parts[1]);
                        }
                        else
                        {
                            throw new Exception("-prs requires a value!");
                        }
                    }
                    else if (args[i] == "-s")
                    {
                        if (i + 1 < args.Length)
                        {
                            FTSERVER_IPADDRESS = args[++i];
                        }
                        else
                        {
                            throw new Exception("-s requires a value!");
                        }
                    }
                    else if (args[i] == "-d")
                    {
                        if (i + 1 < args.Length)
                        {
                            DIRECTORY_NAME = args[++i];
                        }
                        else
                        {
                            throw new Exception("-d requires a value!");
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

            // print current parameters
            Console.WriteLine("PRS Address: " + PRSSERVER_IPADDRESS);
            Console.WriteLine("PRS Port: " + PRSSERVER_PORT);
            Console.WriteLine("FT Server Address: " + FTSERVER_IPADDRESS);
            Console.WriteLine("Directory: " + DIRECTORY_NAME);
            
            try
            {
                // contact the PRS and lookup port for "FT Server"
                PRSClient prs = new PRSClient(PRSSERVER_IPADDRESS, PRSSERVER_PORT, FTSERVICE_NAME);
                FTSERVER_PORT = prs.LookupPort();

                // create an FTClient and connect it to the server
                FTLib.FTClient ft = new FTLib.FTClient(FTSERVER_IPADDRESS, FTSERVER_PORT);
                ft.Connect();

                // get the contents of the specified directory
                ft.GetDirectory(DIRECTORY_NAME);

                // disconnect from the server
                ft.Disconnect();
                
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

            // wait for a keypress from the user before closing the console window
            Console.WriteLine("Press Enter to exit");
            Console.ReadKey();
        }
    }
}
