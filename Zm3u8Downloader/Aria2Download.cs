﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Zm3u8Downloader
{
    public class Aria2Download
    {
       
        public delegate void Aria2DownloadCallBack(Status status, int progress);

        public Status status;
        public Aria2DownloadCallBack callBack;
        public string uri;
        public string filename;

        public enum Status
        {
            NoStart,
            Paused,
            Running,
            Failed,
            Finished
        }

        public Aria2Download(string uri,string name, Aria2DownloadCallBack cb)
        {
            this.uri = uri;
            this.filename = name.Replace("\\","/");
            this.callBack = cb;
            status = Status.Paused;
        }


        public void Start()
        {
            //新建线程
            Process process = new Process();
            process.StartInfo.FileName = @".\aria2c\aria2c.exe";
            process.StartInfo.Arguments = " -o \"" + filename + "\" -c -x 2 " + uri;
 //           process.StartInfo.Arguments = " --all-proxy=w0102934:beenle1702@10.191.131.15:3128  -o \"" + filename + "\" -c -x 2 " + uri;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.ErrorDialog = false;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.EnableRaisingEvents = true;
            process.OutputDataReceived += process_OutputDataReceived;
            process.ErrorDataReceived += process_OutputDataReceived;
            process.Exited += process_Exited;
            status = Status.Running;
            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

        }



        private delegate void UpdateStatusDelegate(string status);

        private void process_Exited(object sender, System.EventArgs e)
        {
            Console.WriteLine("process_Exited");
        }

        private void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null) return;
            //debug
            //Console.WriteLine(e.Data);
            //end debug
            if (e.Data.Contains("completed"))
            {
                status = Status.Finished;
                callBack.Invoke(status, 100);
            }
            else if (e.Data.Contains("error occurred."))
            {
                status = Status.Failed;
                callBack.Invoke(status, 100);
            }
            
            Regex legalRegex = new Regex(@"[#S S S S S*]");
            if (!legalRegex.IsMatch(e.Data)) return;

            Regex rateRegex = new Regex(@"\d*%");
            string rate = rateRegex.Match(e.Data).Value;

            //TODO 需要增加显示速度功能

            if (!String.IsNullOrWhiteSpace(rate))
                callBack(status, int.Parse(rate.Remove(rate.Length - 1)));
            

        }
    }
}
