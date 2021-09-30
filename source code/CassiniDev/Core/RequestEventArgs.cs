//  **********************************************************************************
//  CassiniDev - http://cassinidev.codeplex.com
// 
//  Copyright (c) 2010 Sky Sanders. All rights reserved.
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
using CassiniDev.ServerLog;

#endregion

namespace CassiniDev
{
    ///<summary>
    ///</summary>
    public class RequestEventArgs : EventArgs
    {
        private readonly Guid _id;

        private readonly LogInfo _requestLog;

        private readonly LogInfo _responseLog;

        ///<summary>
        ///</summary>
        ///<param name="id"></param>
        ///<param name="requestLog"></param>
        ///<param name="responseLog"></param>
        public RequestEventArgs(Guid id, LogInfo requestLog, LogInfo responseLog)
        {
            _requestLog = requestLog;
            _responseLog = responseLog;
            _id = id;
        }

        ///<summary>
        ///</summary>
        public Guid Id
        {
            get { return _id; }
        }

        ///<summary>
        ///</summary>
        public LogInfo RequestLog
        {
            get { return _requestLog; }
        }

        ///<summary>
        ///</summary>
        public LogInfo ResponseLog
        {
            get { return _responseLog; }
        }
    }
}