using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarWarsAPI
{
    class UserDataUtility
    {

        public string ReadFromFile(string path)
        {
            // Add a string variable called fileContents
            string DataContents = "";

            // Add a using statement
            // Inside of the using statment, create a StreamReader.  The stream read constructor will take the path property
            using (StreamReader reader = new StreamReader(path))
            {
                // Set the DataContents variable to the file using stream readers 'ReadToEnd' method
                DataContents = reader.ReadToEnd();
            }

            // Return the dataContents
            return DataContents;
        }

        public void WriteToFile(string value)
        {
            // Add a using statement
            // Inside of the using, create a new StreamWriter.  The stream writer's constructor take the private file path field
            using (StreamWriter writer = new StreamWriter("./JediArchives.txt"))
            {
                // Write the value parameter to the file using the writer's 'Write' method
                writer.Write(value);
            }
        }
    }
}