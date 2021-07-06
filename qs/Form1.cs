using System;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.IO.Compression;

namespace qs
{
    public partial class Form1 : Form
    {
		string tmpPath = Path.Combine(Path.GetTempPath(), "qs_temp");
		public Form1()
        {
            InitializeComponent();
			StartServer();
        }
		void StartServer()
		{
			if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "index.html")))
			{
				MessageBox.Show("File index.html not found in current directory!");
				Environment.Exit(1);
			}

			if (Directory.Exists(tmpPath))
			{
				Directory.Delete(tmpPath, true);
			}
			Directory.CreateDirectory(tmpPath);

			using (var client = new WebClient())
			{
				client.DownloadFile("https://www.ritlabs.com/download/tinyweb/tinyweb-1-94.zip", Path.Combine(tmpPath, "tinyweb.zip"));
			}
			string zipPath = Path.Combine(tmpPath, "tinyweb.zip");
			string extractPath = tmpPath;
			ZipFile.ExtractToDirectory(zipPath, extractPath);
			File.Move(Path.Combine(tmpPath, "tiny.exe"), Path.Combine(tmpPath, "tinywebserver.exe"));

			string args = "\"" + AppDomain.CurrentDomain.BaseDirectory + "\" 25577";
			using (Process process = new Process())
            {
				// Configure the process using the StartInfo properties.
				process.StartInfo.WorkingDirectory = tmpPath;
				process.StartInfo.FileName = "tinywebserver.exe";
				process.StartInfo.Arguments = args;
				process.Start();
			}
		}
		void MainFormFormClosing(object sender, FormClosingEventArgs e)
		{
			ShowMessageBox("Shutting down server...", "Please wait.");
			foreach (var process in Process.GetProcessesByName("tinywebserver"))
			{
				process.Kill();
			}
			Thread.Sleep(2000);
			if (Directory.Exists(tmpPath))
			{
				Directory.Delete(tmpPath, true);
			}
			Environment.Exit(0);
		}
		void ShowMessageBox(string text, string caption)
		{
			Thread t = new Thread(() => MyMessageBox(text, caption));
			t.Start();
		}
		void MyMessageBox(object text, object caption)
		{
			MessageBox.Show((string)text, (string)caption);
		}
        private void buttonOpen_Click(object sender, EventArgs e)
        {
			Process.Start("http://localhost:25577").Dispose();
		}
    }
}
