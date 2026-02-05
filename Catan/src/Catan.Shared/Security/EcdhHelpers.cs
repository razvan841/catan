using System.Security.Cryptography;

namespace Catan.Shared.Security;

public static class EcdhHelpers
{
    public static byte[] ExportPublicKey(ECDiffieHellman ecdh)
    {
        return ecdh.PublicKey.ExportSubjectPublicKeyInfo();
    }

    public static byte[] DeriveSharedSecret(ECDiffieHellman local, byte[] remotePublicKey)
    {
        using var other = ECDiffieHellman.Create();
        other.ImportSubjectPublicKeyInfo(remotePublicKey, out _);
        return local.DeriveKeyMaterial(other.PublicKey);
    }
}
