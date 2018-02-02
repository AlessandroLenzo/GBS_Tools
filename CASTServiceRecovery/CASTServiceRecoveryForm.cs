using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Globalization;

namespace CASTServiceRecovery
{
    public partial class CASTServiceRecoveryForm : Form
    {
        private qpmAttivita qpm;
 
        public CASTServiceRecoveryForm()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("it-IT");
 
            InitializeComponent();

            textURL.Text = Properties.Settings.Default.URL;

            qpm = new qpmAttivita();


 
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonCall_Click(object sender, EventArgs e)
        {
            qpm.Url = textURL.Text;

            Thread.CurrentThread.CurrentCulture = new CultureInfo("it-IT");
 
            DateTime timestamp = new DateTime(dateTimePickerDate.Value.Year, dateTimePickerDate.Value.Month, dateTimePickerDate.Value.Day, dateTimePickerTime.Value.Hour, dateTimePickerTime.Value.Minute, dateTimePickerTime.Value.Second);

            qpmResult res;
            string attCode = qpm.registraSottoAttivita(
                textIdRichiesta.Text, 
                textIdAttività.Text, 
                textAPP.Text,
                textSottoattività.Text,
                textDescr.Text,
                comboBoxEsito.Text,
                textDescrEsito.Text,
                comboTipoNotifica.Text,
                timestamp.ToString(),
                out res);

            textResult.AppendText(res.codResult + "\t" + res.descrResult + "\n");
        }

        private void buttonChiudiAttivita_Click(object sender, EventArgs e)
        {
            qpm.Url = textURL.Text;

            Thread.CurrentThread.CurrentCulture = new CultureInfo("it-IT");

            DateTime timestamp = new DateTime(
                dateTimeDataChiudiAttività.Value.Year, 
                dateTimeDataChiudiAttività.Value.Month, 
                dateTimeDataChiudiAttività.Value.Day, 
                dateTimeChiudiAttività.Value.Hour, 
                dateTimeChiudiAttività.Value.Minute, 
                dateTimeChiudiAttività.Value.Second);


            List<qpm_fileResult> filesList = new List<qpm_fileResult>();

            foreach (string line in textBoxFiles.Lines)
                AllegaFile(line, filesList);

            qpmResult res = qpm.chiudiAttivita(
                textIDR.Text,
                textIDA.Text,
                textAppl.Text,
                 timestamp.ToString(),
                 comboEsitoChiudi.Text,
                 textDescrEsitoChiudi.Text,
                 comboBoxAzione.Text,
                 filesList.ToArray()
                 );

            textResult.AppendText(res.codResult + "\t" + res.descrResult + "\n");
 
        }

        private void AllegaFile(string fpath, List<qpm_fileResult> filesList)
        {
            qpm_fileResult result = new qpm_fileResult();
            result.pathFile = fpath;
            result.tipo = Path.GetExtension(fpath);
            result.allegare = false;
            filesList.Add(result);
        }
    }
}
