using System;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

class SessionManager
{
    private static readonly byte[] Key = Encoding.UTF8.GetBytes("anonymous");
    public static string sessionToken;
    public static string GenerateToken(string username)
    {
        byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
        byte[] timestampBytes = BitConverter.GetBytes(DateTime.UtcNow.ToBinary());
        byte[] data = new byte[usernameBytes.Length + timestampBytes.Length];

        Buffer.BlockCopy(usernameBytes, 0, data, 0, usernameBytes.Length);
        Buffer.BlockCopy(timestampBytes, 0, data, usernameBytes.Length, timestampBytes.Length);

        using (HMACSHA256 hmac = new HMACSHA256(Key))
        {
            byte[] signature = hmac.ComputeHash(data);
            byte[] tokenData = new byte[data.Length + signature.Length];

            Buffer.BlockCopy(data, 0, tokenData, 0, data.Length);
            Buffer.BlockCopy(signature, 0, tokenData, data.Length, signature.Length);

            return Convert.ToBase64String(tokenData);
        }
    }

    public static bool ValidateToken(string token, string username)
    {
        byte[] tokenData = Convert.FromBase64String(token);
        byte[] receivedSignature = new byte[32];
        byte[] expectedSignature;

        Buffer.BlockCopy(tokenData, tokenData.Length - 32, receivedSignature, 0, 32);

        byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
        byte[] timestampBytes = new byte[8];
        Buffer.BlockCopy(tokenData, usernameBytes.Length, timestampBytes, 0, 8);

        byte[] data = new byte[usernameBytes.Length + timestampBytes.Length];
        Buffer.BlockCopy(usernameBytes, 0, data, 0, usernameBytes.Length);
        Buffer.BlockCopy(timestampBytes, 0, data, usernameBytes.Length, timestampBytes.Length);

        using (HMACSHA256 hmac = new HMACSHA256(Key))
        {
            expectedSignature = hmac.ComputeHash(data);
            return hmac.ComputeHash(data).SequenceEqual(receivedSignature);
        }
    }
}
