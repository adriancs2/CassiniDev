//  **********************************************************************************
//  CassiniDev - http://cassinidev.codeplex.com
// 
//  Copyright (c) 2010 Sky Sanders. All rights reserved.
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  
//  This source code is subject to terms and conditions of the Microsoft Public
//  License (Ms-PL). A copy of the license can be found in the license.txt file
//  included in this distribution.
//  
//  You must not remove this notice, or any other, from this software.
//  
//  **********************************************************************************

#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Remoting;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using CassiniDev.Configuration;
using CassiniDev.ServerLog;


#endregion

namespace CassiniDev
{
    ///<summary>
    ///</summary>
    [PermissionSet(SecurityAction.LinkDemand, Name = "Everything"),
     PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    public class Server : MarshalByRefObject, IDisposable
    {
        private readonly bool _useLogger;
        ///<summary>
        ///</summary>
        public List<string> Plugins = new List<string>();
        ///<summary>
        ///</summary>
        public readonly ApplicationManager ApplicationManager;

        private readonly bool _disableDirectoryListing;

        private readonly string _hostName;

        private readonly IPAddress _ipAddress;

        private readonly object _lockObject;

        private readonly string _physicalPath;

        private readonly int _port;
        private readonly bool _requireAuthentication;
        //private readonly int _timeoutInterval;
        private readonly string _virtualPath;
        private bool _disposed;

        private Host _host;

        private IntPtr _processToken;

        private string _processUser;

        //private int _requestCount;

        private bool _shutdownInProgress;

        private Socket _socket;

        //private Timer _timer;

        private string _appId;
        ///<summary>
        ///</summary>
        public string AppId
		{
			get { return _appId; }
		}
        ///<summary>
        ///</summary>
        public AppDomain HostAppDomain
        {
            get
            {
                if (_host == null)
                {
                    GetHost();
                }
                if (_host != null)
                {
                    return _host.AppDomain;
                }
                return null;
            }
        }


        ///<summary>
        ///</summary>
        ///<param name="port"></param>
        ///<param name="virtualPath"></param>
        ///<param name="physicalPath"></param>
        public Server(int port, string virtualPath, string physicalPath)
            : this(port, virtualPath, physicalPath, false, false)
        {
        }

        ///<summary>
        ///</summary>
        ///<param name="port"></param>
        ///<param name="physicalPath"></param>
        public Server(int port, string physicalPath)
            : this(port, "/", physicalPath, IPAddress.Loopback)
        {
        }

        ///<summary>
        ///</summary>
        ///<param name="physicalPath"></param>
        public Server(string physicalPath)
            : this(CassiniNetworkUtils.GetAvailablePort(32768, 65535, IPAddress.Loopback, false), physicalPath)
        {
        }

        ///<summary>
        ///</summary>
        ///<param name="port"></param>
        ///<param name="virtualPath"></param>
        ///<param name="physicalPath"></param>
        ///<param name="ipAddress"></param>
        ///<param name="hostName"></param>
        ///<param name="requireAuthentication"></param>
        public Server(int port, string virtualPath, string physicalPath, IPAddress ipAddress, string hostName,
                      bool requireAuthentication)
            : this(port, virtualPath, physicalPath, ipAddress, hostName, requireAuthentication, false)
        {
        }

        ///<summary>
        ///</summary>
        ///<param name="port"></param>
        ///<param name="virtualPath"></param>
        ///<param name="physicalPath"></param>
        ///<param name="requireAuthentication"></param>
        public Server(int port, string virtualPath, string physicalPath, bool requireAuthentication)
            : this(port, virtualPath, physicalPath, requireAuthentication, false)
        {
        }

        ///<summary>
        ///</summary>
        ///<param name="port"></param>
        ///<param name="virtualPath"></param>
        ///<param name="physicalPath"></param>
        ///<param name="ipAddress"></param>
        ///<param name="hostName"></param>
        public Server(int port, string virtualPath, string physicalPath, IPAddress ipAddress, string hostName)
            : this(port, virtualPath, physicalPath, ipAddress, hostName, false, false)
        {
        }

        ///<summary>
        ///</summary>
        ///<param name="port"></param>
        ///<param name="virtualPath"></param>
        ///<param name="physicalPath"></param>
        ///<param name="ipAddress"></param>
        ///<param name="hostName"></param>
        ///<param name="requireAuthentication"></param>
        ///<param name="disableDirectoryListing"></param>
        public Server(int port, string virtualPath, string physicalPath, IPAddress ipAddress, string hostName,
                      bool requireAuthentication, bool disableDirectoryListing)
            : this(port, virtualPath, physicalPath, requireAuthentication, disableDirectoryListing)
        {
            _ipAddress = ipAddress;
            _hostName = hostName;
            //_timeoutInterval = timeout;
        }

        ///<summary>
        ///</summary>
        ///<param name="port"></param>
        ///<param name="virtualPath"></param>
        ///<param name="physicalPath"></param>
        ///<param name="ipAddress"></param>
        public Server(int port, string virtualPath, string physicalPath, IPAddress ipAddress)
            : this(port, virtualPath, physicalPath, ipAddress, null, false, false)
        {
        }

        ///<summary>
        ///</summary>
        ///<param name="port"></param>
        ///<param name="virtualPath"></param>
        ///<param name="physicalPath"></param>
        ///<param name="requireAuthentication"></param>
        ///<param name="disableDirectoryListing"></param>
        public Server(int port, string virtualPath, string physicalPath, bool requireAuthentication,
                      bool disableDirectoryListing)
        {
            try
            {
                Assembly.ReflectionOnlyLoad("Common.Logging");
                _useLogger = true;
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            // ReSharper restore EmptyGeneralCatchClause
            {
            }
            _ipAddress = IPAddress.Loopback;
            _requireAuthentication = requireAuthentication;
            _disableDirectoryListing = disableDirectoryListing;
            _lockObject = new object();
            _port = port;
            _virtualPath = virtualPath;
            _physicalPath = Path.GetFullPath(physicalPath);
            _physicalPath = _physicalPath.EndsWith("\\", StringComparison.Ordinal)
                                ? _physicalPath
                                : _physicalPath + "\\";
            ProcessConfiguration();

            ApplicationManager = ApplicationManager.GetApplicationManager();
            string uniqueAppString = string.Concat(virtualPath, physicalPath,":",_port.ToString()).ToLowerInvariant();
            _appId = (uniqueAppString.GetHashCode()).ToString("x", CultureInfo.InvariantCulture);
            ObtainProcessToken();
        }

        private void ProcessConfiguration()
        {
            // #TODO: how to identify profile to use?
            // current method is to either use default '*' profile or match port
            // port can be an arbitrary value, especially in testing scenarios so
            // perhaps a regex based path matching strategy can also be offered

            var config = CassiniDevConfigurationSection.Instance;
            if (config != null)
            {
                foreach (CassiniDevProfileElement profile in config.Profiles)
                {
                    if (profile.Port == "*" || Convert.ToInt64(profile.Port) == _port)
                    {
                        foreach (PluginElement plugin in profile.Plugins)
                        {
                            Plugins.Insert(0, plugin.Type);
                        }
                    }
                }
            }
        }

        ///<summary>
        ///</summary>
        ///<param name="physicalPath"></param>
        ///<param name="requireAuthentication"></param>
        public Server(string physicalPath, bool requireAuthentication)
            : this(
                CassiniNetworkUtils.GetAvailablePort(32768, 65535, IPAddress.Loopback, false), "/", physicalPath,
                requireAuthentication)
        {
        }

        

        ///<summary>
        ///</summary>
        public bool DisableDirectoryListing
        {
            get { return _disableDirectoryListing; }
        }

        ///<summary>
        ///</summary>
        public bool RequireAuthentication
        {
            get { return _requireAuthentication; }
        }

        /////<summary>
        /////</summary>
        //public int TimeoutInterval
        //{
        //    get { return _timeoutInterval; }
        //}

        ///<summary>
        ///</summary>
        public string HostName
        {
            get { return _hostName; }
        }

        ///<summary>
        ///</summary>
// ReSharper disable InconsistentNaming
        public IPAddress IPAddress
// ReSharper restore InconsistentNaming
        {
            get { return _ipAddress; }
        }

        ///<summary>
        ///</summary>
        public string PhysicalPath
        {
            get { return _physicalPath; }
        }

        ///<summary>
        ///</summary>
        public int Port
        {
            get { return _port; }
        }

        ///<summary>
        ///</summary>
        public string RootUrl
        {
            get
            {
                string hostname = _hostName;
                if (string.IsNullOrEmpty(_hostName))
                {
                    if (_ipAddress.Equals(IPAddress.Loopback) || _ipAddress.Equals(IPAddress.IPv6Loopback) ||
                        _ipAddress.Equals(IPAddress.Any) || _ipAddress.Equals(IPAddress.IPv6Any))
                    {
                        hostname = "localhost";
                    }
                    else
                    {
                        hostname = _ipAddress.ToString();
                    }
                }

                return _port != 80
                           ?
                               String.Format("http://{0}:{1}{2}", hostname, _port, _virtualPath)
                           :
                    //FIX: #12017 - TODO:TEST
                       string.Format("http://{0}{1}", hostname, _virtualPath);
            }
        }

        ///<summary>
        ///</summary>
        public string VirtualPath
        {
            get { return _virtualPath; }
        }

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            if (!_disposed)
            {
                ShutDown();
            }
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        #endregion

        ///<summary>
        ///</summary>
        public event EventHandler<RequestEventArgs> RequestComplete;

        /////<summary>
        /////</summary>
        //public event EventHandler TimedOut;

        ///<summary>
        ///</summary>
        ///<returns></returns>
        public IntPtr GetProcessToken()
        {
            return _processToken;
        }

        ///<summary>
        ///</summary>
        ///<returns></returns>
        public string GetProcessUser()
        {
            return _processUser;
        }

        ///<summary>
        ///</summary>
        public void HostStopped()
        {
            _host = null;
        }

        /// <summary>
        /// Obtains a lifetime service object to control the lifetime policy for this instance.
        /// </summary>
        /// <returns>
        /// An object of type <see cref="T:System.Runtime.Remoting.Lifetime.ILease"/> used to control the lifetime policy for this instance. This is the current lifetime service object for this instance if one exists; otherwise, a new lifetime service object initialized to the value of the <see cref="P:System.Runtime.Remoting.Lifetime.LifetimeServices.LeaseManagerPollTime"/> property.
        /// </returns>
        /// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. 
        ///                 </exception><filterpriority>2</filterpriority><PermissionSet><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="RemotingConfiguration, Infrastructure"/></PermissionSet>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public override object InitializeLifetimeService()
        {
            // never expire the license
            return null;
        }

        // called at the end of request processing
        // to disconnect the remoting proxy for Connection object
        // and allow GC to pick it up
        /// <summary>
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="userName"></param>
        public void OnRequestEnd(Connection conn, string userName)
        {
            try
            {
                LogInfo connRequestLogClone = conn.RequestLog.Clone();
                connRequestLogClone.Identity = userName;
                LogInfo connResponseLogClone = conn.ResponseLog.Clone();
                connResponseLogClone.Identity = userName;
                OnRequestComplete(conn.Id, connRequestLogClone, connResponseLogClone);
            }
            catch
            {
                // swallow - we don't want consumer killing the server
            }
            RemotingServices.Disconnect(conn);
            //DecrementRequestCount();
        }

        ///<summary>
        ///</summary>
        public void Start()
        {
            _socket = CreateSocketBindAndListen(AddressFamily.InterNetwork, _ipAddress, _port);

            //start the timer
            //DecrementRequestCount();

            ThreadPool.QueueUserWorkItem(delegate
                {
                    while (!_shutdownInProgress)
                    {
                        try
                        {
                            Socket acceptedSocket = _socket.Accept();

                            ThreadPool.QueueUserWorkItem(delegate
                                {
                                    if (!_shutdownInProgress)
                                    {
                                        Connection conn = new Connection(this, acceptedSocket);

                                        if (conn.WaitForRequestBytes() == 0)
                                        {
                                            conn.WriteErrorAndClose(400);
                                            return;
                                        }

                                        Host host = GetHost();

                                        if (host == null)
                                        {
                                            conn.WriteErrorAndClose(500);
                                            return;
                                        }

                                        //IncrementRequestCount();
                                        host.ProcessRequest(conn);
                                    }
                                });
                        }
                        catch
                        {
                            Thread.Sleep(100);
                        }
                    }
                });
        }


        /// <summary>
        /// Allows an <see cref="T:System.Object"/> to attempt to free resources and perform other cleanup operations before the <see cref="T:System.Object"/> is reclaimed by garbage collection.
        /// </summary>
        ~Server()
        {
            Dispose();
        }


        private static Socket CreateSocketBindAndListen(AddressFamily family, IPAddress address, int port)
        {
            Socket socket = new Socket(family, SocketType.Stream, ProtocolType.Tcp);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            socket.Bind(new IPEndPoint(address, port));
            socket.Listen((int)SocketOptionName.MaxConnections);
            return socket;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="virtualPath"></param>
        /// <param name="physicalPath"></param>
        /// <param name="hostType"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        /// <remarks>
        /// This is Dmitry's hack to enable running outside of GAC.
        /// There are some errors being thrown when running in proc
        /// </remarks>
        private object CreateWorkerAppDomainWithHost(string virtualPath, string physicalPath, Type hostType,int port)
        {


            // create BuildManagerHost in the worker app domain
            //ApplicationManager appManager = ApplicationManager.GetApplicationManager();
            Type buildManagerHostType = typeof(HttpRuntime).Assembly.GetType("System.Web.Compilation.BuildManagerHost");
            IRegisteredObject buildManagerHost = ApplicationManager.CreateObject(_appId, buildManagerHostType, virtualPath,
                                                                          physicalPath, false);

            // call BuildManagerHost.RegisterAssembly to make Host type loadable in the worker app domain
            buildManagerHostType.InvokeMember("RegisterAssembly",
                                              BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic,
                                              null,
                                              buildManagerHost,
                                              new object[] { hostType.Assembly.FullName, hostType.Assembly.Location });

            // create Host in the worker app domain
            // FIXME: getting FileLoadException Could not load file or assembly 'WebDev.WebServer20, Version=4.0.1.6, Culture=neutral, PublicKeyToken=f7f6e0b4240c7c27' or one of its dependencies. Failed to grant permission to execute. (Exception from HRESULT: 0x80131418)
            // when running dnoa 3.4 samples - webdev is registering trust somewhere that we are not
            return ApplicationManager.CreateObject(_appId, hostType, virtualPath, physicalPath, false);
        }

        //private void DecrementRequestCount()
        //{
        //    lock (_lockObject)
        //    {
        //        _requestCount--;

        //        if (_requestCount < 1)
        //        {
        //            _requestCount = 0;

        //            if (_timeoutInterval > 0 && _timer == null)
        //            {
        //                _timer = new Timer(TimeOut, null, _timeoutInterval, Timeout.Infinite);
        //            }
        //        }
        //    }
        //}

        private Host GetHost()
        {
            if (_shutdownInProgress)
                return null;
            Host host = _host;
            if (host == null)
            {
#if NET40
                object obj2 = new object();
                bool flag = false;
                try
                {
                    Monitor.Enter(obj2 = _lockObject, ref flag);
                    host = _host;
                    if (host == null)
                    {
                        host = (Host)CreateWorkerAppDomainWithHost(_virtualPath, _physicalPath, typeof(Host),Port);
                        host.Configure(this, _port, _virtualPath, _physicalPath, _requireAuthentication, _disableDirectoryListing);
                        _host = host;
                    }
                }
                finally
                {
                    if (flag)
                    {
                        Monitor.Exit(obj2);
                    }
                }
#else

                lock (_lockObject)
                {
                    host = _host;
                    if (host == null)
                    {
                        host = (Host)CreateWorkerAppDomainWithHost(_virtualPath, _physicalPath, typeof(Host),Port);
                        host.Configure(this, _port, _virtualPath, _physicalPath, _requireAuthentication, _disableDirectoryListing);
                        _host = host;
                    }
                }

#endif
            }

            return host;
        }

        //private void IncrementRequestCount()
        //{

        //    lock (_lockObject)
        //    {
        //        _requestCount++;

        //        if (_timer != null)
        //        {

        //            _timer.Dispose();
        //            _timer = null;
        //        }
        //    }
        //}


        private void ObtainProcessToken()
        {
            if (Interop.ImpersonateSelf(2))
            {
                Interop.OpenThreadToken(Interop.GetCurrentThread(), 0xf01ff, true, ref _processToken);
                Interop.RevertToSelf();
                // ReSharper disable PossibleNullReferenceException
                _processUser = WindowsIdentity.GetCurrent().Name;
                // ReSharper restore PossibleNullReferenceException
            }
        }

        private void OnRequestComplete(Guid id, LogInfo requestLog, LogInfo responseLog)
        {
            PublishLogToCommonLogging(requestLog);
            PublishLogToCommonLogging(responseLog);

            EventHandler<RequestEventArgs> complete = RequestComplete;


            if (complete != null)
            {
                complete(this, new RequestEventArgs(id, requestLog, responseLog));
            }
        }



        private void PublishLogToCommonLogging(LogInfo item)
        {
            if(!_useLogger )
            {
                return;
            }

            Common.Logging.ILog logger = Common.Logging.LogManager.GetCurrentClassLogger();

            var bodyAsString = String.Empty;
            try
            {
                bodyAsString = Encoding.UTF8.GetString(item.Body);
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch 
            // ReSharper restore EmptyGeneralCatchClause
            {
                /* empty bodies should be allowed */
            }

            var type = item.RowType == 0 ? "" : item.RowType == 1 ? "Request" : "Response";
            logger.Debug(type + " | " +
                          item.Created + " | " +
                          item.StatusCode + " | " +
                          item.Url + " | " +
                          item.PathTranslated + " | " +
                          item.Identity + " | " +
                          "\n===>Headers<====\n" + item.Headers +
                          "\n===>Body<=======\n" + bodyAsString
                );
        }


        ///<summary>
        ///</summary>
        public void ShutDown()
        {
            if (_shutdownInProgress)
            {
                return;
            }

            _shutdownInProgress = true;

            try
            {
                if (_socket != null)
                {
                    _socket.Close();
                }
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            // ReSharper restore EmptyGeneralCatchClause
            {
                // TODO: why the swallow?
            }
            finally
            {
                _socket = null;
            }

            try
            {
                if (_host != null)
                {
                    _host.Shutdown();
                }

                // the host is going to raise an event that this class uses to null the field.
                // just wait until the field is nulled and continue.

                while (_host != null)
                {
                    new AutoResetEvent(false).WaitOne(100);
                }
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            // ReSharper restore EmptyGeneralCatchClause
            {
                // TODO: what am i afraid of here?
            }
  
        }

        //private void TimeOut(object ignored)
        //{
        //    TimeOut();
        //}

        /////<summary>
        /////</summary>
        //public void TimeOut()
        //{
        //    ShutDown();
        //    OnTimeOut();
        //}

        //private void OnTimeOut()
        //{
        //    EventHandler handler = TimedOut;
        //    if (handler != null) handler(this, EventArgs.Empty);
        //}
    }
}