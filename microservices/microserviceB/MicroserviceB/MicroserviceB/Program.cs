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
        Console.WriteLine($"Starting microservice B");
        byte[] key = Encoding.UTF8.GetBytes("artgerhklngertma");                                                                                                                       //Key needs to be 16 byte
        byte[] iv = Encoding.UTF8.GetBytes("1402983490453459");                                                                                                                        //Initilization Vector needs to be 16 byte                                
        string userPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "LocalLow");                                                       //Locates save data in Appdata
        string filePath = Path.Combine(userPath, "DefaultCompany", "My project", "data.game");
        string saveFileBackup = Path.Combine(userPath, "DefaultCompany", "My project", "saveDataBackup.txt");                                                                           //combine path with fileName
        string decryptedData;
        int encryptionKey = 23;                                                                                                            //Read encrypted data

        //Console.WriteLine($"File written successfully! Path: {Path.GetFullPath(saveFile)}");                                                                                          //can be used to ensure path is correct
        using (var responder = new ResponseSocket("@tcp://localhost:5252"))                                                                                                             //start connection
        {
            while (true)                                                                                                                                                                //connection loop, runs until server shuts down
            {
                string request = responder.ReceiveFrameString();                                                                                                                        //recive request
                try
                {
                    if (File.Exists(filePath))                                                                                                                                          //If first file exists and is accessible use it
                    {
                        Console.WriteLine($"Trigered by main program!");
                        byte[] encryptedData = File.ReadAllBytes(filePath);                                                                                                             //Read encrypted data
                        decryptedData = DecryptData(encryptedData, encryptionKey);                                                                                                       //Decrypt data
                        File.WriteAllBytes(filePath, encryptedData);

                        Console.WriteLine($"File read and decrypted successfully! Sending: {decryptedData}");
                        responder.SendFrame($"{decryptedData}");                                                                                                                        //Send decypted data
                    }
                    else if (File.Exists(saveFileBackup))                                                                                                                               //Try backup save if first one is not working
                    {
                        byte[] encryptedData = File.ReadAllBytes(saveFileBackup);
                        decryptedData = DecryptData(encryptedData, encryptionKey);
                        File.WriteAllBytes(filePath, encryptedData);
                        responder.SendFrame($"{decryptedData}");
                    }
                    else                                                                                                                                                                //Error handling 
                    {
                        Console.WriteLine("Error: File not found.");
                        responder.SendFrame("Failed to Load data - File not found");
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine("Error: You don't have permission to write to this file.");
                    responder.SendFrame("Failed to Load data");
                }
            }
        }
    }
    static string DecryptData(byte[] encryptedData, int key)
    {
        byte[] decryptedBytes = new byte[encryptedData.Length];

        for (int i = 0; i < encryptedData.Length; i++)
        {
            decryptedBytes[i] = (byte)(encryptedData[i] ^ key);
        }

        return Encoding.UTF8.GetString(decryptedBytes);
    }
}





