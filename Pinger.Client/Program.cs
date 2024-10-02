// See https://aka.ms/new-console-template for more information
using System.IO;
using System.Net.Sockets;
using System.Text;

Console.WriteLine("Hello, World!");

const int PORT_NO = 2000;
const string SERVER_IP = "127.0.0.1";

//---data to send to the server---
string textToSend = DateTime.Now.ToString();

//---create a TCPClient object at the IP and port no.---
TcpClient client = new TcpClient(SERVER_IP, PORT_NO);

using (var serverStreamWriter = new StreamWriter(client.GetStream()))
{
	serverStreamWriter.WriteLine(textToSend);
	serverStreamWriter.Flush();

}

client.Close();