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
			if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "index.html")))
				StartServer(AppDomain.CurrentDomain.BaseDirectory);
        }
		void StartServer(string baseDir)
		{
			label1.Text = "Directory is being served at\nhttp://localhost:25577";

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

			string args = "\"" + baseDir + "\" 25577";
			using (Process process = new Process())
            {
				// Configure the process using the StartInfo properties.
				process.StartInfo.WorkingDirectory = tmpPath;
				process.StartInfo.FileName = "tinywebserver.exe";
				process.StartInfo.Arguments = args;
				process.Start();
			}
		}
		void Cleanup()
        {
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
        private void ButtonOpen_Click(object sender, EventArgs e)
        {
			Process.Start("http://localhost:25577").Dispose();
		}

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
			label1.Text = "Shutting down server...";
			buttonOpen.Enabled = false;

			Thread t = new Thread(() => Cleanup());
			t.Start();
		}

        private void Form1_DragOver(object sender, DragEventArgs e)
        {
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
				e.Effect = DragDropEffects.Link;
			else
				e.Effect = DragDropEffects.None;
		}

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
			string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];
			if (files.Length != 0)
			{
				StartServer(new FileInfo(files[0]).Directory.FullName);
			}
		}
    }
}
