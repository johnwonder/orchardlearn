using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.IO;

namespace WebClientPost
{
    /// <summary>
    /// Summary description for handler
    /// </summary>
    public class handler : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            XmlDocument document = new XmlDocument();
          
            document.Load(context.Server.MapPath("~/ResponeCode.xml"));
            //XmlTextWriter writer =new XmlTextWriter()
            context.Response.Write(document.InnerXml);
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}