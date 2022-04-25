using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace CielaCrawler
{
    public class Client
    {
        public string GetHtml(string url)
        {
            HttpWebRequest webRequest = WebRequest.Create(url) as HttpWebRequest;
            webRequest.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.8.0.6) Gecko/20060728 Firefox/1.5";
            webRequest.CookieContainer = new CookieContainer();
            WebResponse webResponse;
            try
            {
                webRequest.Accept = "*/*";
                webResponse = webRequest.GetResponse();
                Stream receiveStream = webResponse.GetResponseStream();

                StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);

                var result = readStream.ReadToEnd();

                webResponse.Close();
                readStream.Close();

                return result;
            }
            catch (WebException e)
            {
                return e.Message;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public void DownloadFile(string url, string filePathAndName)
        {
            int bytesProcessed = 0;

            Stream remoteStream = null;
            Stream localStream = null;

            HttpWebRequest webRequest = WebRequest.Create(url) as HttpWebRequest;
            webRequest.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.8.0.6) Gecko/20060728 Firefox/1.5";
            webRequest.CookieContainer = new CookieContainer();
            WebResponse webResponse = null;
            try
            {
                webRequest.Accept = "*/*";
                webResponse = webRequest.GetResponse();

                remoteStream = webResponse.GetResponseStream();

                Directory.CreateDirectory(".\\Images");
                localStream = File.Create(filePathAndName);

                byte[] buffer = new byte[1024];
                int bytesRead;

                do
                {
                    bytesRead = remoteStream.Read(buffer, 0, buffer.Length);

                    localStream.Write(buffer, 0, bytesRead);

                    bytesProcessed += bytesRead;
                } while (bytesRead > 0);
            }
            catch (WebException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                // Close the response and streams objects here 
                // to make sure they're closed even if an exception
                // is thrown at some point
                if (webResponse != null) webResponse.Close();
                if (remoteStream != null) remoteStream.Close();
                if (localStream != null) localStream.Close();
            }
        }
    }
}
