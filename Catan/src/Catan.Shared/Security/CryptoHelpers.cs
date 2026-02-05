using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class CryptoHelpers
{
    public static (byte[] AesKey, byte[] HmacKey) DeriveKeys(byte[] sharedSecret)
    {
        using var hkdf = new HMACSHA256(sharedSecret);

        byte[] aesKey = hkdf.ComputeHash(Encoding.UTF8.GetBytes("aes-key"));
        byte[] hmacKey = hkdf.ComputeHash(Encoding.UTF8.GetBytes("hmac-key"));

        return (aesKey, hmacKey);
    }

    public static byte[] EncryptAes(byte[] plaintext, byte[] key, byte[] iv)
    {
        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.Key = key;
        aes.IV = iv;

        using var ms = new MemoryStream();
        using var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
        cs.Write(plaintext, 0, plaintext.Length);
        cs.FlushFinalBlock();
        return ms.ToArray();
    }

    public static byte[] DecryptAes(byte[] ciphertext, byte[] key, byte[] iv)
    {
        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.Key = key;
        aes.IV = iv;

        using var ms = new MemoryStream(ciphertext);
        using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
        using var outMs = new MemoryStream();
        cs.CopyTo(outMs);
        return outMs.ToArray();
    }

    public static byte[] ComputeHmac(byte[] data, byte[] key)
    {
        using var hmac = new HMACSHA256(key);
        return hmac.ComputeHash(data);
    }

    public static void VerifyHmac(byte[] data, byte[] key, byte[] expected)
    {
        var actual = ComputeHmac(data, key);
        if (!CryptographicOperations.FixedTimeEquals(actual, expected))
            throw new CryptographicException("HMAC validation failed!");
    }

    public static byte[] GenerateRandomIV()
    {
        byte[] iv = new byte[16];
        RandomNumberGenerator.Fill(iv);
        return iv;
    }
}
