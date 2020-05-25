using System;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace SomeProject.Library.Client
{
    public class Client
    {
        public TcpClient tcpClient;
        public static string FileNm;




        //works but without binaryformat
        /* public OperationResult SendFileToServer(string FileName)
         {
             FileNm = FileName;
             int BufferSize = 1024;
             byte[] SendingBuffer = null;
             NetworkStream netstream = null;
             SendInfo si = new SendInfo();
             try
             {

                 tcpClient = new TcpClient("127.0.0.1", 8080);
                 netstream = tcpClient.GetStream();


                 FileStream Fs = new FileStream(FileName, FileMode.Open, FileAccess.Read);
                 int NoOfPackets = Convert.ToInt32
                 (Math.Ceiling(Convert.ToDouble(Fs.Length) / Convert.ToDouble(BufferSize)));
                 int TotalLength = (int)Fs.Length, CurrentPacketLength, counter = 0;
                 for (int i = 0; i < NoOfPackets; i++)
                 {
                     if (TotalLength > BufferSize)
                     {
                         CurrentPacketLength = BufferSize;
                         TotalLength = TotalLength - CurrentPacketLength;
                     }
                     else
                     CurrentPacketLength = TotalLength;
                     SendingBuffer = new byte[CurrentPacketLength];
                     Fs.Read(SendingBuffer, 0, CurrentPacketLength);
                     netstream.Write(SendingBuffer, 0, (int)SendingBuffer.Length);
                     tcpClient.Close();

                 }
                 Fs.Close();
                 netstream.Close();
                 return new OperationResult(Result.OK, "");
             }
             catch (Exception ex)
             {
                 Console.WriteLine(ex.Message);
                 return new OperationResult(Result.Fail, ex.Message);
             }

         }*/



        /* public OperationResult SendFileToServer(string FileName)
         {

             int bufferSize = 1024;
             byte[] buffer = null;
             byte[] header = null;

             try
             {
                 FileStream fs = new FileStream(FileName, FileMode.Open);
                 bool read = true;

                 int bufferCount = Convert.ToInt32(Math.Ceiling((double)fs.Length / (double)bufferSize));

                 FileInfo fi = new FileInfo(FileName);

                 TcpClient tcpClient = new TcpClient("127.0.0.1", 8080);
                // System.Threading.Interlocked.Increment(ref Server.Server.counter);
                 tcpClient.SendTimeout = 600000;
                 tcpClient.ReceiveTimeout = 600000;

                 string headerStr = "Content-length:" + fs.Length.ToString() + "\r\nFilename:" + fi.FullName + "\r\n";
                 header = new byte[bufferSize];
                 Array.Copy(Encoding.ASCII.GetBytes(headerStr), header, Encoding.ASCII.GetBytes(headerStr).Length);

                 tcpClient.Client.Send(header);

                 for (int i = 0; i < bufferCount; i++)
                 {
                     buffer = new byte[bufferSize];
                     int size = fs.Read(buffer, 0, bufferSize);

                     tcpClient.Client.Send(buffer, size, SocketFlags.Partial);

                 }

                 tcpClient.Client.Close();

                 fs.Close();
                 return new OperationResult(Result.OK, "");
             }
             catch (Exception e)
             {
                 return new OperationResult(Result.Fail, e.ToString());
             }
         }*/

        /// <summary>
        /// Присоединение к серверу. Отправляем на сервер сообщение new
        /// </summary>
        /// <param name="nam">Массив байтов с сообщением.</param>
        public OperationResult ConnectToServer(string str)
        {
            byte[] nam = null;
            try
            {
                TcpClient tcpClient = new TcpClient("127.0.0.1", 8080);
                NetworkStream stream = tcpClient.GetStream();
                nam = new byte[3];
                nam = Encoding.UTF8.GetBytes(str);
                stream.Write(nam,0,3);
                stream.Close();
                tcpClient.Close();
                return new OperationResult(Result.OK, "");
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.Message);
            }
        }
        /// <summary>
        /// Отправка файла на сервер. Принимает имя нового файла
        /// </summary>
        /// <param name="nam">Массив байтов с сообщением об отправке файла.</param>
       /// <param name="header">Массив байтов с сообщением.</param>
        public OperationResult SendFileToServer(string FileName)
          {
  
            int bufferSize = 1024;
              byte[] buffer = null;
              byte[] header = null;
            byte[] nam = null;

              try
              {
                  FileStream fs = new FileStream(FileName, FileMode.Open);
                  bool read = true;

                  int bufferCount = Convert.ToInt32(Math.Ceiling((double)fs.Length / (double)bufferSize));

                  FileInfo fi = new FileInfo(FileName);

                  TcpClient tcpClient = new TcpClient("127.0.0.1", 8080);
                  NetworkStream stream = tcpClient.GetStream();
                 
                  tcpClient.SendTimeout = 600000;
                  tcpClient.ReceiveTimeout = 600000;
                nam = new byte[3];
                nam = Encoding.UTF8.GetBytes("fil");
                  string headerStr = "Content-length:" + fs.Length.ToString() + "\r\nFilename:" + fi.FullName + "\r\n";
                  header = new byte[bufferSize];
                  Array.Copy(Encoding.ASCII.GetBytes(headerStr), header, Encoding.ASCII.GetBytes(headerStr).Length);
                stream.Write(nam, 0, 3);
                stream.Write(header, 0, bufferSize);

             
                  for (int i = 0; i < bufferCount; i++)
                  {

                      buffer = new byte[bufferSize];
                      int size = fs.Read(buffer, 0, bufferSize);
                     
                      stream.Write(buffer, 0, size);

                  }

                  //tcpClient.Client.Close();
                
                
                stream.Close();
                tcpClient.Close();
                fs.Close();
                return new OperationResult(Result.OK, "");
              }
              catch (Exception e)
              {
                  return new OperationResult(Result.Fail, e.ToString());
              }
          }



        public OperationResult ReceiveMessageFromServer()
        {
            try
            {
                tcpClient = new TcpClient("127.0.0.1", 8080);
                StringBuilder recievedMessage = new StringBuilder();
                byte[] data = new byte[256];
                NetworkStream stream = tcpClient.GetStream();

                do
                {
                    int bytes = stream.Read(data, 0, data.Length);
                    recievedMessage.Append(Encoding.UTF8.GetString(data, 0, bytes));
                }
                while (stream.DataAvailable);
                stream.Close();
                tcpClient.Close();

                return new OperationResult(Result.OK, recievedMessage.ToString());
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.ToString());
            }
        }
        /// <summary>
        /// Отправка сообщения на сервер. Принимает сообщение
        /// </summary>
        /// <param name="nam">Массив байтов с сообщением об отправке файла.</param>
        /// <param name="data">Массив байтов с сообщением.</param>
        public OperationResult SendMessageToServer(string message)
        {

            try
            {
                byte[] b = new byte[3];
                b = Encoding.UTF8.GetBytes("mesage");
                tcpClient = new TcpClient("127.0.0.1", 8080);
               // System.Threading.Interlocked.Increment(ref Server.Server.counter);
                NetworkStream stream = tcpClient.GetStream();
                byte[] data = Encoding.UTF8.GetBytes("mes"+message);
                //stream.Write(b,0,8);
                stream.Write(data, 0, data.Length);
                stream.Close();
                tcpClient.Close();
                //Check.c = 1;
                return new OperationResult(Result.OK, "");
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.Message);
            }
        }

          /* public OperationResult SendFileToServer(string FileName)
           {
               byte[] GetHeader(int length)
               {
                   string header = length.ToString();
                   if (header.Length < 9)
                   {
                       string zeros = null;
                       for (int i = 0; i < (9 - header.Length); i++)
                       {
                           zeros += "0";
                       }
                       header = zeros + header;
                   }

                   byte[] byteheader = Encoding.Default.GetBytes(header);


                   return byteheader;
               }

               SendInfo si = new SendInfo();
               try
               {
                   tcpClient = new TcpClient("127.0.0.1", 8080);
                   NetworkStream stream = tcpClient.GetStream();
                   BinaryFormatter bf = new BinaryFormatter();
                   MemoryStream ms = new MemoryStream();
                   //if (String.IsNullOrEmpty(SendFileName) == true) ;

                   if (FileName != null)
                   {
                       FileInfo fi = new FileInfo(FileName);
                       if (fi.Exists == true)
                       {
                           si.filesize = (int)fi.Length;
                           si.filename = fi.Name;
                       }
                       fi = null;
                   }


                   bf.Serialize(ms, si);
                   ms.Position = 0;
                   byte[] infobuffer = new byte[ms.Length];
                   int r = ms.Read(infobuffer, 0, infobuffer.Length);
                   ms.Close();

                   byte[] header = GetHeader(infobuffer.Length);
                   byte[] total = new byte[header.Length + infobuffer.Length + si.filesize];
                   //Console.WriteLine(total);
                   Buffer.BlockCopy(header, 0, total, 0, header.Length);
                   Buffer.BlockCopy(infobuffer, 0, total, header.Length, infobuffer.Length);

                   // Если путь файла указан, добавим его содержимое в отправляемый массив байтов
                   if (si.filesize > 0)
                   {
                       FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read);
                       fs.Read(total, header.Length + infobuffer.Length, si.filesize);
                       fs.Close();
                       fs = null;
                   }
                   stream.Write(total, 0, total.Length);
                   header = null;
                   infobuffer = null;
                   total = null;

                   GC.Collect();
                   GC.WaitForPendingFinalizers();
                   //  byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
                   // stream.Write(data, 0, data.Length);
                   //stream.Close();
                   tcpClient.Close();
                   return new OperationResult(Result.OK, "");
               }
               catch (Exception e)
               {
                   return new OperationResult(Result.Fail, e.Message);
               }
           }*/

          // Метод отправки файлов и других данных по сети

       
        [Serializable]
        internal class SendInfo
        {

            public string filename;
            public string filetype;
            public int filesize;
        }
       

    }
}
