using System.IO;

namespace StarWarsAPI
{
    // 2026 update: this class now actually implements the IUserDataUtility
    // interface (it had the interface file but never used it), and the file path
    // lives in one place instead of being typed in two different files.
    class UserDataUtility : IUserDataUtility
    {
        // The Jedi Archives: saved next to the program. (If an item does not
        // appear in our records, it does not exist.)
        public const string ArchivePath = "./JediArchives.txt";

        public string ReadFromFile()
        {
            // BUG FIX (2026): reading before anything was saved used to crash with
            // FileNotFoundException. Now it just returns an empty string.
            if (!File.Exists(ArchivePath))
            {
                return string.Empty;
            }

            // Inside of the using statment, create a StreamReader.  The stream read constructor will take the path property
            using (StreamReader reader = new StreamReader(ArchivePath))
            {
                // Set the DataContents variable to the file using stream readers 'ReadToEnd' method
                return reader.ReadToEnd();
            }
        }

        public void WriteToFile(string value)
        {
            // Inside of the using, create a new StreamWriter.  The stream writer's constructor take the private file path field
            using (StreamWriter writer = new StreamWriter(ArchivePath))
            {
                // Write the value parameter to the file using the writer's 'Write' method
                writer.Write(value);
            }
        }
    }
}
