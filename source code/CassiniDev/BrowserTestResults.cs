// /* **********************************************************************************
//  *
//  * Copyright (c) Sky Sanders. All rights reserved.
//  * 
//  * This source code is subject to terms and conditions of the Microsoft Public
//  * License (Ms-PL). A copy of the license can be found in the license.htm file
//  * included in this distribution.
//  *
//  * You must not remove this notice, or any other, from this software.
//  *
//  * **********************************************************************************/
using System;
using System.Text;

namespace CassiniDev
{
    
    ///<summary>
    ///</summary>
    public class BrowserTestResults
    {
        ///<summary>
        ///</summary>
        public BrowserTestResults()
        {
        }

        ///<summary>
        ///</summary>
        ///<param name="eventArgs"></param>
        public BrowserTestResults(RequestEventArgs eventArgs)
        {
            Id = eventArgs.Id;
            if (eventArgs.RequestLog.Body.Length > 0)
            {
                Log = Encoding.UTF8.GetString(eventArgs.RequestLog.Body);
            }
            Error = eventArgs.RequestLog.Exception;
            StatusCode = eventArgs.RequestLog.StatusCode;
            Url = eventArgs.RequestLog.Url;
            Success = Url.IndexOf("success", StringComparison.InvariantCultureIgnoreCase) > -1;
        }

        ///<summary>
        ///</summary>
        public bool Success { get; set; }

        ///<summary>
        ///</summary>
        public string Url { get; set; }

        ///<summary>
        ///</summary>
        public long? StatusCode { get; set; }

        ///<summary>
        ///</summary>
        public string Error { get; set; }

        ///<summary>
        ///</summary>
        public string Log { get; set; }

        ///<summary>
        ///</summary>
        public Guid Id { get; set; }
    }
}