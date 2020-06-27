using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Nullspace
{
    public static class DESCrypto
    {
        public static byte[] Encrypt(byte[] ToEncrypt, byte[] Key)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider() { Key = Key, IV = Key };
            byte[] encrypted;
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(ToEncrypt, 0, ToEncrypt.Length);
                    cs.FlushFinalBlock();
                    encrypted = ms.ToArray();
                }
            }
            return encrypted;
        }
        public static byte[] Decrypt(byte[] ToDecrypt, byte[] Key)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider() { Key = Key, IV = Key };
            byte[] decrypted;
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(ToDecrypt, 0, ToDecrypt.Length);
                    cs.FlushFinalBlock();
                    decrypted = ms.ToArray();
                }
            }
            return decrypted;
        }
        public static string GenerateKey()
        {
            var desCrypto = DES.Create();
            return Encoding.ASCII.GetString(desCrypto.Key);
        }
    }

    public static class RSACrypto
    {
        public static byte[] Encrypt(byte[] ToEncrypt, String Key)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            des.Key = Encoding.ASCII.GetBytes(Key);
            des.IV = Encoding.ASCII.GetBytes(Key);
            byte[] encrypted;
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(ToEncrypt, 0, ToEncrypt.Length);
                    cs.FlushFinalBlock();
                    encrypted = ms.ToArray();
                }
            }
            return encrypted;
        }
        public static byte[] Decrypt(byte[] ToDecrypt, string Key)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            des.Key = Encoding.ASCII.GetBytes(Key);
            des.IV = Encoding.ASCII.GetBytes(Key);
            byte[] decrypted;
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(ToDecrypt, 0, ToDecrypt.Length);
                    cs.FlushFinalBlock();
                    decrypted = ms.ToArray();
                }
            }
            return decrypted;
        }
        public static void GenerateKey()
        {
            RSACryptoServiceProvider rcp = new RSACryptoServiceProvider();
            string keyString = rcp.ToXmlString(true);
            XmlFileUtils.SaveText("E:\\key.xml", keyString);
            //FileStream fs = File.Create("E:\\key.xml");
            //fs.Write(Encoding.UTF8.GetBytes(keyString), 0, Encoding.UTF8.GetBytes(keyString).Length);
            //fs.Flush();
            //fs.Close();
        }
    }
}
