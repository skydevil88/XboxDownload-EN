using System.Diagnostics;
using System.Reflection;

namespace XboxDownload
{
    public partial class FormAbout : Form
    {
        public FormAbout()
        {
            InitializeComponent();

            lbVersion.Text = string.Format(lbVersion.Text, Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version);
        }

        private void LinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string url = ((LinkLabel)sender).Text;
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }

        private void LinkCopyBTC_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Clipboard.SetDataObject(labBTC.Text);
        }

        private void LinkCopyETH_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Clipboard.SetDataObject(labETH.Text);
        }
    }
}