namespace StarWarsAPI
{
    public interface IUserDataUtility
    {
        string ReadFromFile();
        void WriteToFile(string value);
    }
}