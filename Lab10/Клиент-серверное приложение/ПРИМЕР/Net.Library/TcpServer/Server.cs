using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using System.Threading;


namespace SomeProject.Library.Server
{
    public class Server
    {
       static List<int> clients = new List<int>();
        static List<int> client = new List<int>();
        List<int> files = new List<int>();
        TcpListener serverListener;
        public static int _value = 0;
        private static int _value2 = 0;
        int increm;
        int maxClients = 5;
        int idNew;
       static int fID=0;
        public static int counter = 0;
        public Server()
        {
            serverListener = new TcpListener(IPAddress.Loopback, 8080);
            
        }
        public bool TurnOffListener()
        {
            try
            {
                if (serverListener != null)
                    serverListener.Stop();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot turn off listener: " + e.Message);
                return false;
            }
        }

        /// <summary>
        /// Запуск listener. В бесконечном цикле запускаем ReciveFromClient
        /// </summary>
        public async Task TurnOnListener()
        {
            try
            {
                if (serverListener != null)
                    serverListener.Start();
             

                while (true)
                {
                  
                     OperationResult result0 = await ReceiveFromClient();
                    if (result0.Result == Result.False)
                    {
                        Console.WriteLine("Unexpected error: " + result0.Message);
                 
                       
                    }
                    else if (result0.Result == Result.OK)
                    {
                        Console.WriteLine("Server: " + result0.Message);
                    }
                                       
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot turn on listener: " + e.Message);
            }
        }


        /// <summary>
        /// Принимает NetworkStream. В случае, если в первых 3 байтах сказано new добавляет клиента.
        /// mes-отправляет сообщение
        /// fil-отправляет файл
        /// dis -отключает клиента
        /// </summary>
        /// <param name="gotstr">Массив байтов с сообщением о необходимой операции.</param>
        /// <param name="header">Массив байтов с заголовком файла.</param>
        /// <param name="recievedMessage">Полученное сообщение.</param>
        /// <param name="filedate">Текущая дата.</param>
        /// <param name="stream">Новый поток.</param>
        /// <param name="client">Запрос на подключение нового клиента.</param>
        public async Task<OperationResult> ReceiveFromClient()
        {
            try
            {
                Console.WriteLine("Waiting for connections...");
                TcpClient client = serverListener.AcceptTcpClient();
                NetworkStream stream = client.GetStream();
                byte[] data = new byte[256];
                byte[] b = new byte[3];
                int gotbyte = stream.Read(b, 0, 3);
                string gotstr = (Encoding.UTF8.GetString(b, 0, gotbyte));
                if (gotstr == "dis")
                {
                     DeleteClient();
                     return new OperationResult(Result.OK, "User diconnected" );
                }
                if (gotstr == "new")
                {
                    int clinum = AddClient();
                    if (clinum <= 3)
                        return new OperationResult(Result.OK, "User " + clinum+" connected");
                    else
                    return new OperationResult(Result.False, "Too many users");
                }
                if (gotstr == "mes")
                {
                    try
                    {

                        StringBuilder recievedMessage = new StringBuilder();
                       
                        do
                        {
                            int bytes = stream.Read(data, 0, data.Length);
                            recievedMessage.Append(Encoding.UTF8.GetString(data, 0, bytes));
                        }
                        while (stream.DataAvailable);

                        return new OperationResult(Result.OK,"New message from client: " +recievedMessage.ToString());
                    }
                    catch (Exception e)
                    {
                        return new OperationResult(Result.Fail, e.Message);
                    }
                }
                if (gotstr=="fil")
                {
                    try
                    {
                        
                        string filedate = DateTime.Now.ToString("yyyy.MM.dd");
                        int bufferSize = 1024;
                        byte[] buffer = null;
                        byte[] header = null;
                        string headerStr = "";
                        string filename = "";
                        int filesize = 0;
                        string ext = "";

                       
                        header = new byte[bufferSize];
                        stream.Read(header, 0, bufferSize);
                        headerStr = Encoding.ASCII.GetString(header);
                        string[] splitted = headerStr.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        Dictionary<string, string> headers = new Dictionary<string, string>();
                        foreach (string s in splitted)
                        {
                            if (s.Contains(":"))
                            {
                                headers.Add(s.Substring(0, s.IndexOf(":")), s.Substring(s.IndexOf(":") + 1));
                            }

                        }
                        //Get filesize from header
                        filesize = Convert.ToInt32(headers["Content-length"]);
                        //Get filename from header
                        filename = headers["Filename"];
                        FileInfo fi = new FileInfo(filename);
                        //increm = System.Threading.Interlocked.Increment(ref _value);
                        increm = AddFile();
                        ext = fi.Extension;
                        var result = filedate + "File" + increm.ToString() + ext;
                        if (!Directory.Exists(result))
                        { Directory.CreateDirectory(result); }
                        int bufferCount = Convert.ToInt32(Math.Ceiling((double)filesize / (double)bufferSize));


                        FileStream fs = new FileStream(Path.Combine(result, fi.Name), FileMode.OpenOrCreate);

                        while (filesize > 0)
                        {
                            buffer = new byte[bufferSize];

                            int size = stream.Read(buffer, 0, bufferSize);

                            fs.Write(buffer, 0, size);

                            filesize -= size;
                        }
                      
                        fs.Close();
                        return new OperationResult(Result.OK,"New file from client: "+ filename);
                    }
                    catch (Exception e)
                    {
                        return new OperationResult(Result.Fail, e.Message);
                    }
                }

                stream.Close();
                client.Close();


                return new OperationResult(Result.False, gotstr);



            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.Message);
            }

        }

        public int AddAndCheckClient(int ID)
        {
            if (ID == 0 && clients.Count < maxClients)
            {
                clients.Add(idNew);
                Interlocked.Increment(ref idNew);
                return 0;
            }
            else if (clients.Contains(ID))
            {
                return ID;
            }

            return -1;
        }
        /// <summary>
        /// Добавляем новый файл в массив файлов
        /// </summary>
        /// <param name="fID">ID файла.</param>
        public int AddFile()
        {
            files.Add(fID);
            Interlocked.Increment(ref fID);
            return files.Count;
        }
        /// <summary>
        /// Добавляем нового клиента в массив клиентов
        /// </summary>
        /// <param name="fID">ID нового клиента.</param>

        public int AddClient()
        {
            
            client.Add(fID);       
            Interlocked.Increment(ref fID);
            //Console.WriteLine(clients.Count.ToString());
            return client.Count;
    
        }
        /// <summary>
        /// Удаляем клиента
        /// </summary>
        /// <param name="fID">ID файла.</param>
        public void DeleteClient()
        {
            client.Remove(1);
            Interlocked.Decrement(ref fID);
        }

        public async Task<OperationResult> ReceiveMessageFromClient()
        {
           
                try
                {
                    Console.WriteLine("Waiting for connections...");
                    StringBuilder recievedMessage = new StringBuilder();
                    TcpClient client = serverListener.AcceptTcpClient();

                    byte[] data = new byte[256];
                    byte[] b = new byte[3];

                    NetworkStream stream = client.GetStream();
                    int gotbyte = stream.Read(b, 0, 3);
                    string gotstr = (Encoding.UTF8.GetString(b, 0, gotbyte));
                //Console.WriteLine(gotstr);
                if (gotstr == "mes")
                {
                    do
                    {
                        int bytes = stream.Read(data, 0, data.Length);
                        recievedMessage.Append(Encoding.UTF8.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);
                }
                    stream.Close();
                    client.Close();

                    return new OperationResult(Result.OK, recievedMessage.ToString());
                }
                catch (Exception e)
                {
                    return new OperationResult(Result.Fail, e.Message);
                }
            
            
        }

        public OperationResult SendMessageToClient(string message)
        {
            try
            {
                TcpClient client = serverListener.AcceptTcpClient();
                NetworkStream stream = client.GetStream();

                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);

                stream.Close();
                client.Close();
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.Message);
            }
            return new OperationResult(Result.OK, "");
        }

      public async Task<OperationResult> ReceiveFileFromClient()
      {
            
            try
            {
                Console.WriteLine("Waiting for connections...");

                //Socket socket = serverListener.AcceptSocket();
                string filedate = DateTime.Now.ToShortDateString();
                int bufferSize = 1024;
                byte[] buffer = null;
                byte[] header = null;
                string headerStr = "";
                string filename = "";
                int filesize = 0;
                string ext = "";

                TcpClient client = serverListener.AcceptTcpClient();
                NetworkStream stream = client.GetStream();

                header = new byte[bufferSize];
                stream.Read(header, 0, bufferSize);
                headerStr = Encoding.ASCII.GetString(header);


                string[] splitted = headerStr.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                Dictionary<string, string> headers = new Dictionary<string, string>();
                foreach (string s in splitted)
                {
                    if (s.Contains(":"))
                    {
                        headers.Add(s.Substring(0, s.IndexOf(":")), s.Substring(s.IndexOf(":") + 1));
                    }

                }
                //Get filesize from header
                filesize = Convert.ToInt32(headers["Content-length"]);
                //Get filename from header
                filename = headers["Filename"];
                FileInfo fi = new FileInfo(filename);
                //increm = System.Threading.Interlocked.Increment(ref _value);
                increm = AddFile();
                ext = fi.Extension;
                var result = filedate + "File" + increm.ToString() + ext;
                if (!Directory.Exists(result))
                { Directory.CreateDirectory(result); }
                int bufferCount = Convert.ToInt32(Math.Ceiling((double)filesize / (double)bufferSize));


                FileStream fs = new FileStream(Path.Combine(result, fi.Name), FileMode.OpenOrCreate);

                while (filesize > 0)
                {
                    buffer = new byte[bufferSize];

                    int size = stream.Read(buffer, 0, bufferSize);

                    fs.Write(buffer, 0, size);

                    filesize -= size;
                }
                stream.Close();
                client.Close();

                fs.Close();
                return new OperationResult(Result.OK, filename);
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.Message);
            }
      }


  
      
    }

}