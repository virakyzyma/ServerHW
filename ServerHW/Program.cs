using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerHW
{
    internal class Program
    {
        static void Main(string[] args)
        {
            CurrencyServer currencyServer = new CurrencyServer();
            currencyServer.Start();
        }
    }
    public class CurrencyServer
    {
        private TcpListener _listener;
        private bool _isRunning;

        private static readonly Dictionary<string, double> exchangeRates = new Dictionary<string, double>()
        {
            {"USD_EURO", 0.91 },
            {"EURO_USD", 1.09 },
            {"UAH_USD", 0.02 },
            {"USD_UAH", 41.01 },
            {"UAH_EURO", 0.03 },
            {"EURO_UAH", 45.50 },
        };

        public CurrencyServer()
        {
            _listener = new TcpListener(IPAddress.Any, 8888);
            _isRunning = true;
        }

        public void Start()
        {
            _listener.Start();
            Console.WriteLine("Server is running now");

            while (_isRunning)
            {
                TcpClient tcpClient = _listener.AcceptTcpClient();
                Console.WriteLine($"Client connected to server: {DateTime.Now}");

                Thread clientThread = new Thread(HandleClient);
                clientThread.Start(tcpClient);
            }
        }

        private void HandleClient(object obj)
        {
            TcpClient client = (TcpClient)obj;

            NetworkStream networkStream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead;

            while ((bytesRead = networkStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Request: {request}.");

                if (exchangeRates.ContainsKey(request))
                {
                    double rate = exchangeRates[request];
                    string response = rate.ToString();
                    byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                    networkStream.Write(responseBytes, 0, responseBytes.Length);
                    Console.WriteLine($"Response: {response}.");
                }
                else
                {
                    string error = "Error";
                    byte[] errorBytes = Encoding.UTF8.GetBytes(error);
                    networkStream.Write(errorBytes, 0, errorBytes.Length);
                    Console.WriteLine($"Response: {error}");
                }
            }

            Console.WriteLine($"Client disconnected: {DateTime.Now}");
            client.Close();
        }

        public void Close()
        {
            _isRunning = false;
            _listener.Stop();
        }
    }
}
