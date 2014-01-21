using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace EncryptionKeyGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            RandomNumberGenerator rnd = RandomNumberGenerator.Create();
            byte[] encryptionKey = new byte[32];
            byte[] validationKey = new byte[32];
            rnd.GetBytes(encryptionKey);
            rnd.GetBytes(validationKey);

            string encryptionKeyHex = BitConverter.ToString(encryptionKey).Replace("-", "");
            string validationKeyHex = BitConverter.ToString(validationKey).Replace("-", "");

            Console.WriteLine("<appSettings>");
	        Console.WriteLine("  <add key=\"EncryptionKey\" value=\"{0}\"/>", encryptionKeyHex);
            Console.WriteLine("  <add key=\"ValidationKey\" value=\"{0}\"/>", validationKeyHex);
            Console.WriteLine("</appSettings>");
        }
    }
}
