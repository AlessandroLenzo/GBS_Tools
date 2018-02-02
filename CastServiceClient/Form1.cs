using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using CastServiceClient.CASTServiceReference;

namespace CastServiceClient
{
    public partial class Form1 : Form
    {
        CASTServiceClient client;

        public Form1()
        {
            InitializeComponent();

            client = new CASTServiceClient();

            comboBox1.SelectedIndex = 1;
        }

        private void buttonCall_Click(object sender, EventArgs e)
        {
            CASTRequestType req = new CASTRequestType();
            req.AMB = "1";
            req.APP = textBox2.Text;
            req.CDT = textBox3.Text;
            req.CHS = "";
            req.IDA = "CAST#17#1475";
            req.IDR = numericUpDown1.Value.ToString();
            req.LSV = "0";
            req.MAC = "I";
            req.MAT = "B";
            req.SNN = textBox2.Text + " " + req.CDT;
            req.TAT = comboBox1.Text;
            req.VBS = "QualityCheckApp 25/01/2023";
            req.VER = "UC_009";
            req.VPR = "N";
            
            CASTResponseType res = client.Call(req);


            textBox1.Text += "\r\nCODICE=" + res.CODICE + "\r\n\r\nMESSAGGIO=" + res.MESSAGGIO + "\r\n";
        }

       
    }
} 
