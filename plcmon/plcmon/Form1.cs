using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using Sharp7;
using static System.Windows.Forms.LinkLabel;

namespace plcmon {
    public partial class Form1 : Form {
        public Form1()
        {
            InitializeComponent();
        }
        public S7Client client = new S7Client();
        public bool connected =false;
        public System.Timers.Timer timer;

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ShowFaults();
        }
        private void btnconnect_Click(object sender, EventArgs e)
        {
            string ipaddress = tipadress.Text;

            if(ipaddress != "")
            {
                if(!connected)
                {
                    int connectionResult = client.ConnectTo(ipaddress, 0, 1);
                    if (connectionResult == 0)
                    {
                        btnconnect.BackColor = Color.LightGreen;
                        btnconnect.Text = "Disconnect from PLC";
                        connected = true;
                        if (timer == null)
                        {
                            timer = new System.Timers.Timer();
                            timer.Elapsed += Timer_Elapsed;
                            timer.Start();
                        }

                    }
                } else
                {
                    lient.Disconnect();
                    btnconnect.BackColor = Color.Red;
                    btnconnect.Text = "Connect to PLC";
                    timer.Stop();
                    connected = false;
                }
            }
        }
        public void ShowFaults()
        {
            const int area = S7Consts.S7AreaDB; // DB area
            const int dbNumber = 1020; // DB number
            const int start = 30; // Byte offset of the integer within the DB
            const int amount = 30; // Number of bytes to read (2 bytes for an integer)
            Dictionary<string, object> tracker = new Dictionary<string, object>();

            byte[] dbBuffer = new byte[30];
            int result = client.DBRead(dbNumber, start, amount, dbBuffer);

            for (int i = 0; i < 30; i += 2)
            {
                // Convert the bytes to an integer
                int value = S7.GetIntAt(dbBuffer, i); // Adjust byte index if needed
                var bools = new BitArray(new int[] { value }).Cast<bool>().ToArray();
                for (int j = 0; j < bools.Length; j++)
                {
                    string key = $"DB1020.DBW{i + 30}.DBX{j}";
                    if (bools[j])
                    {
                        if (!tracker.ContainsKey(key))
                        {
                            tracker[key] = new List<string> { DateTime.Now.ToString() };
                        }
                    } else
                    {
                        if (tracker.ContainsKey(key))
                        {
                            tracker[key].Add(DateTime.Now.ToString());
                        }
                    }
                    
                }

            }

        }
    }
}
