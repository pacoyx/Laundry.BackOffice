public interface ISymmetricEncryptionService
{
    string Encrypt(string plainText);

    string Decrypt(string cipherText);
}
