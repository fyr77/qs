using System;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.IO.Compression;
using System.Net.NetworkInformation;
using System.Reflection;

namespace qs
{
    public partial class Form1 : Form
    {
		string tmpPath;
		int port;
		public Form1()
        {
            InitializeComponent();
			this.Text += Assembly.GetExecutingAssembly().GetName().Version.Major.ToString() + "." + Assembly.GetExecutingAssembly().GetName().Version.Minor.ToString();

			Guid tmpGuid = Guid.NewGuid();
			tmpPath = Path.Combine(Path.GetTempPath(), "qs_temp_" + tmpGuid.ToString());

			if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "index.html")))
				StartServer(AppDomain.CurrentDomain.BaseDirectory);
        }
		void StartServer(string baseDir)
		{
			port = GeneratePort();
			while (!PortBindable(port))
			{
				port = GeneratePort();
			}

			label1.Text = "Directory is being served at\nhttp://localhost:" + port.ToString();
			buttonOpen.Enabled = true;

			if (Directory.Exists(tmpPath))
			{
				Directory.Delete(tmpPath, true);
			}
			Directory.CreateDirectory(tmpPath);

			using (var client = new WebClient())
			{
				client.DownloadFile("https://github.com/jakobsenkl/qs/raw/main/res/tinyweb-1-94.zip", Path.Combine(tmpPath, "tinyweb.zip"));
			}
			string zipPath = Path.Combine(tmpPath, "tinyweb.zip");
			string extractPath = tmpPath;
			ZipFile.ExtractToDirectory(zipPath, extractPath);
			File.Move(Path.Combine(tmpPath, "tiny.exe"), Path.Combine(tmpPath, "tinywebserver.exe"));

			string args = "\"" + baseDir + "\" " + port.ToString();
			using (Process process = new Process())
            {
				// Configure the process using the StartInfo properties.
				process.StartInfo.WorkingDirectory = tmpPath;
				process.StartInfo.FileName = "tinywebserver.exe";
				process.StartInfo.Arguments = args;
				process.Start();
			}
		}

		int GeneratePort()
        {
			int min = 1024;
			int max = 65535;

			Random random = new Random();
			int number = random.Next(min, max);

			return number;
		}

		bool PortBindable(int port)
        {
			bool isAvailable = true;

			// Evaluate current system tcp connections. This is the same information provided
			// by the netstat command line application, just in .Net strongly-typed object
			// form.  We will look through the list, and if our port we would like to use
			// in our TcpClient is occupied, we will set isAvailable to false.
			IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
			TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

			foreach (TcpConnectionInformation tcpi in tcpConnInfoArray)
			{
				if (tcpi.LocalEndPoint.Port == port)
				{
					isAvailable = false;
					break;
				}
			}
			return isAvailable;
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
			Process.Start("http://localhost:" + port.ToString()).Dispose();
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
