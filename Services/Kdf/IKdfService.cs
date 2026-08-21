namespace ASP_P42.Services.Kdf
{
    public interface IKdfService
    {
        String Dk(String password, String salt);
    }
}
