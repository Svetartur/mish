using ASP_P42.Services.Hash;

namespace ASP_P42.Services.Kdf
{
    public class PbKdf1Service(IHashService hashService) : IKdfService
    {
        private readonly IHashService _hashService = hashService;
        private const int iterationsCount = 1000000;
        private const int dkLength = 32;
        private const String filler = "B6915281DD9C4436963BB2970FD6DC93";

        public string Dk(string password, string salt)
        {
            String t = _hashService.Digest(password + salt);
            for (int i = 1; i < iterationsCount; i++)
            {
                t = _hashService.Digest(t);
            }
            return t.Length >= dkLength
                ? t[..dkLength]
                : t + filler[..(dkLength - t.Length)];
        }
    }
}
