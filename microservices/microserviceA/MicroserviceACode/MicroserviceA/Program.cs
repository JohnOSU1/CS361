using NetMQ;
using NetMQ.Sockets;
using System;
using System.IO;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;

class Program
{
    static void Main()
    {
        Console.WriteLine("Server started");
        byte[] key = Encoding.UTF8.GetBytes("artgerhklngertma");                                            //Key needs to be 16 byte
        byte[] iv = Encoding.UTF8.GetBytes("1402983490453459");                                             // Initilization Vector needs to be 16 byte
        string targetDir = Directory.GetCurrentDirectory();                                                 //currently saves in one directery above MicroserviceA. Can loop if you want it higher
        //for (int i = 0; i < 5; i++)
        //{
        targetDir = Directory.GetParent(targetDir).FullName;                                                //Saves one above microservice folder
        //}

        string saveFile = Path.Combine(targetDir, "saveData.txt");                                          //combine path with fileName

        //Console.WriteLine($"File written successfully! Path: {Path.GetFullPath(saveFile)}");              //Writes save path
        using (var responder = new ResponseSocket("@tcp://localhost:5454"))                                 //start connection
        {
            while (true)                                                                                    //connection loop, runs until server shuts down
            {
                string request = responder.ReceiveFrameString();                                            //recive request
                try
                {
                    string encrypt = EncryptString(request, key, iv);                                       //encrypt data 

                    File.WriteAllText(saveFile, encrypt);
                    Console.WriteLine("File written successfully!");
                    responder.SendFrame("Data saved successfully");

                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine("Error: You don't have permission to write to this file.");
                    responder.SendFrame("Failed to save data");
                }
            }
        }
    }
    static string EncryptString(string plainText, byte[] key, byte[] iv)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.IV = iv;

            using (MemoryStream ms = new MemoryStream())                                                                        //needed to store data from CryptoStream instead of writing it to file.                                    
            {
                using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))                   //Pass in needed data
                using (StreamWriter sw = new StreamWriter(cs))
                {
                    sw.Write(plainText);
                }

                return Convert.ToBase64String(ms.ToArray());                                                                    //Convert to string
            }
        }
    }
}
