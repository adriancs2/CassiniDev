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
using System.Collections.Generic;

namespace CassiniDev
{
    ///<summary>
    ///</summary>
    [Serializable]
    public class BrowserTestResultItem
    {
        ///<summary>
        ///</summary>
        public bool Success { get; set; }
        ///<summary>
        ///</summary>
        public string Name { get; set; }
        ///<summary>
        ///</summary>
        public int Failures { get; set; }
        ///<summary>
        ///</summary>
        public int Total { get; set; }
        ///<summary>
        ///</summary>
        public List<string> Log { get; set; }
        ///<summary>
        ///</summary>
        ///<param name="log"></param>
        ///<exception cref="NotImplementedException"></exception>
        public virtual void Parse(string log)
        {
            throw new NotImplementedException();
        }
        ///<summary>
        ///</summary>
        public BrowserTestResultItem()
        {
            Items = new Dictionary<string, BrowserTestResultItem>();
            Log = new List<string>();
        }

        ///<summary>
        ///</summary>
        public Dictionary<string, BrowserTestResultItem> Items { get; set; }

    }
}