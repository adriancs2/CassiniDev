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

#endregion

namespace CassiniDev.ServerLog
{
    /// <summary>
    /// TODO: get rid of status code and url in the database and simply parse the headers
    /// </summary>
    [Serializable]
    public class LogInfo : ICloneable
    {
        ///<summary>
        ///</summary>
        public byte[] Body { get; set; }

        ///<summary>
        ///</summary>
        public Guid ConversationId { get; set; }

        ///<summary>
        ///</summary>
        public DateTime Created { get; set; }

        ///<summary>
        ///</summary>
        public string Exception { get; set; }

        ///<summary>
        ///</summary>
        public string Headers { get; set; }

        ///<summary>
        ///</summary>
        public string Identity { get; set; }

        ///<summary>
        ///</summary>
        public string PathTranslated { get; set; }

        ///<summary>
        ///</summary>
        public string PhysicalPath { get; set; }

        ///<summary>
        ///</summary>
        public long RowId { get; set; }

        ///<summary>
        ///</summary>
        public long RowType { get; set; }

        ///<summary>
        ///</summary>
        public long? StatusCode { get; set; }

        ///<summary>
        ///</summary>
        public string Url { get; set; }

        #region ICloneable Members

        object ICloneable.Clone()
        {
            return MemberwiseClone();
        }

        #endregion

        ///<summary>
        ///</summary>
        ///<returns></returns>
        public LogInfo Clone()
        {
            LogInfo result = (LogInfo) ((ICloneable) this).Clone();
            if (Body != null)
            {
                result.Body = new byte[Body.Length];
                Body.CopyTo(result.Body, 0);
            }

            return result;
        }
    }
}