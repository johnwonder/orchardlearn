using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;

namespace WebClientTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //WebClient webClient = new WebClient();
            //webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");//采取POST方式必须加的header，如果改为GET方式的话就去掉这句话即可  
            //byte[] responseData = webClient.UploadData(bankURL, "POST", postData);//得到返回字符流  
            //string srcString = Encoding.UTF8.GetString(responseData);//解码  
             string  tmp ="MERCHANTID=1&BRANCHID=2&POSID=3&ORDERDATE=4&BEGORDERTIME=00:00:00&ENDORDERTIME=23:59:59&QUPWD=&TXCODE=410404&SEL_TYPE=9&OPERATOR=";
             string xml = PostHelper.PostXml("http://localhost:14641/Default.aspx", tmp);

             XmlDocument xmlDoc = new XmlDocument();
             xmlDoc.LoadXml(xml);

             Console.WriteLine(xmlDoc.SelectSingleNode("//RESULT").InnerText);
             Console.Read();
        }
    }

    public class PostHelper
    {
        public string GetWebContent(string url)
        {
            Stream outstream = null;
            Stream instream = null;
            StreamReader sr = null;
            HttpWebResponse response = null;
            HttpWebRequest request = null;
            // 要注意的这是这个编码方式，还有内容的Xml内容的编码方式
            Encoding encoding = Encoding.GetEncoding("UTF-8");
            byte[] data = encoding.GetBytes(url);

            // 准备请求,设置参数
            request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = "POST";
            request.ContentType = "text/xml";
            //request.ContentLength = data.Length;

            outstream = request.GetRequestStream();
            outstream.Write(data, 0, data.Length);
            outstream.Flush();
            outstream.Close();
            //发送请求并获取相应回应数据

            response = request.GetResponse() as HttpWebResponse;
            //直到request.GetResponse()程序才开始向目标网页发送Post请求
            instream = response.GetResponseStream();

            sr = new StreamReader(instream, encoding);
            //返回结果网页(html)代码

            string content = sr.ReadToEnd();
            return content;
        }
       
        
        public static string PostXml(string url, string strPost)
        {
            string result = "";

            StreamWriter myWriter = null;
            HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
            objRequest.Method = "POST";
            //objRequest.ContentLength = strPost.Length;
            objRequest.ContentType = "text/xml";//提交xml 
            objRequest.ContentType = "application/x-www-form-urlencoded";//提交表单
            try
            {
                myWriter = new StreamWriter(objRequest.GetRequestStream());
                myWriter.Write(strPost);
            }
            catch (Exception e)
            {
                return e.Message;
            }
            finally
            {
                myWriter.Close();
            }

            HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
            using (StreamReader sr = new StreamReader(objResponse.GetResponseStream()))
            {
                result = sr.ReadToEnd();
                sr.Close();
            }
            return result;
        }
    }
}
