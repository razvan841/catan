using System;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Catan.Shared.Networking.Messages;
using Catan.Shared.Networking.Serialization;
using Catan.Shared.Security;

namespace Catan.Client.Networking;

public class TcpClientConnection : IDisposable
{
    private readonly TcpClient _client;
    private readonly NetworkStream _stream;
    private ECDiffieHellman? _ecdh;
    private byte[]? _sharedSecret;
    public bool IsSecure => _sharedSecret != null;
    private byte[]? _aesKey;
    private byte[]? _hmacKey;

    
    private TcpClientConnection(string host, int port)
    {
        _client = new TcpClient();
        _client.Connect(host, port);
        _stream = _client.GetStream();
    }

    public static async Task<TcpClientConnection> ConnectAsync(string host, int port)
    {
        var conn = new TcpClientConnection(host, port);
        await conn.DoEcdhHandshake();
        return conn;
    }

    public async Task SendAsync(ClientMessage message)
    {
        if (_aesKey == null || _hmacKey == null)
            throw new InvalidOperationException("Secure channel not established.");

        byte[] plaintext = JsonMessageSerializer.Serialize(message);
        byte[] iv = CryptoHelpers.GenerateRandomIV();
        string utf8 = Encoding.UTF8.GetString(plaintext);

        byte[] ciphertext = CryptoHelpers.EncryptAes(plaintext, _aesKey, iv);
        byte[] hmac = CryptoHelpers.ComputeHmac(ciphertext, _hmacKey);

        // format:
        // [4 bytes total length]
        // [16 bytes IV]
        // [ciphertext]
        // [32 bytes HMAC]

        int totalLen = 16 + ciphertext.Length + 32;

        await _stream.WriteAsync(BitConverter.GetBytes(totalLen));
        await _stream.WriteAsync(iv);
        await _stream.WriteAsync(ciphertext);
        await _stream.WriteAsync(hmac);
    }


    public async Task<ServerMessage> ReceiveAsync()
    {
        if (_aesKey == null || _hmacKey == null)
            throw new InvalidOperationException("Secure channel not established.");

        byte[] lengthBuffer = new byte[4];
        await ReadExactAsync(lengthBuffer);
        int totalLen = BitConverter.ToInt32(lengthBuffer, 0);

        byte[] all = new byte[totalLen];
        await ReadExactAsync(all);

        byte[] iv = new byte[16];
        Buffer.BlockCopy(all, 0, iv, 0, 16);

        int cipherLen = totalLen - 16 - 32;
        byte[] ciphertext = new byte[cipherLen];
        byte[] hmac = new byte[32];

        Buffer.BlockCopy(all, 16, ciphertext, 0, cipherLen);
        Buffer.BlockCopy(all, 16 + cipherLen, hmac, 0, 32);

        CryptoHelpers.VerifyHmac(ciphertext, _hmacKey, hmac);

        byte[] plaintext = CryptoHelpers.DecryptAes(ciphertext, _aesKey, iv);
        
        return JsonMessageSerializer.Deserialize<ServerMessage>(plaintext);
    }


    private async Task ReadExactAsync(byte[] buffer)
    {
        int offset = 0;

        while (offset < buffer.Length)
        {
            int read = await _stream.ReadAsync(
                buffer.AsMemory(offset, buffer.Length - offset)
            );

            if (read == 0)
                throw new IOException("Connection closed.");

            offset += read;
        }
    }
    private async Task SendFramedAsync(byte[] data)
    {
        await _stream.WriteAsync(BitConverter.GetBytes(data.Length));
        await _stream.WriteAsync(data);
    }

    private async Task<byte[]> ReceiveFramedAsync()
    {
        var lenBuf = new byte[4];
        await ReadExactAsync(lenBuf);

        int length = BitConverter.ToInt32(lenBuf, 0);
        var data = new byte[length];
        await ReadExactAsync(data);
        return data;
    }

    private async Task DoEcdhHandshake()
    {
        byte[] serverPub = await ReceiveFramedAsync();

        _ecdh = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);

        byte[] clientPub = EcdhHelpers.ExportPublicKey(_ecdh);
        await SendFramedAsync(clientPub);
        await _stream.FlushAsync();

        Console.WriteLine($"Sent secret {clientPub}");

        _sharedSecret = EcdhHelpers.DeriveSharedSecret(_ecdh, serverPub);
        var (aes, hmac) = CryptoHelpers.DeriveKeys(_sharedSecret);
        _aesKey = aes;
        _hmacKey = hmac;
    }

    public void Dispose()
    {
        _stream.Dispose();
        _client.Dispose();
    }
}
