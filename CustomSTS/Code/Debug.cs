using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CustomSTS.Code
{
    public class Debug
    {
        public static void Log(string msg)
        {
            using (System.IO.StreamWriter w = System.IO.File.AppendText(@"c:\log\customsts.log")) { w.WriteLine(String.Format("{0} {1:yyyy-MM-dd hh:mm:ss.fff}", msg, DateTime.Now)); w.Flush(); w.Close(); }
        }
    }
}