using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;

namespace CassiniDev.Tests
{
    [TestFixture]
    public class IssueResolutionFixture
    {
        [Test]
        public void ServerCanBeRestarted()
        {
            var server = new CassiniDev.Server(Environment.CurrentDirectory);
            server .Start();
            new AutoResetEvent(false).WaitOne(1000);
            server.ShutDown();
            new AutoResetEvent(false).WaitOne(1000);
            server.Start();
            new AutoResetEvent(false).WaitOne(1000);
            server.ShutDown();
            
        }

        [Test]
    public void ScratchPad()
        {
            string projectPath = Environment.CurrentDirectory;
            string webContentPath = System.IO.Path.Combine(Environment.CurrentDirectory, "WebContent");

            string[] args = { "\"/a:" + webContentPath + "\"", "/ip:192.168.10.4", "/ipMode:Specific", "/pm:Specific", "/p:8080" };
            //System.Diagnostics.Process.Start(System.IO.Path.Combine(projectPath, "CassiniDev.exe"), String.Join(" ", args));
        }
    }
}
