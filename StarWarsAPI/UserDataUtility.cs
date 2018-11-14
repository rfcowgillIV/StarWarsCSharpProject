//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace StarWarsAPI
//{
//    internal class UserDataUtility2
//    {
//        private readonly Stream _filePath;
//        private readonly Stream _userPath;
//        private string userData;
//        private readonly string ReadFromFile;

//        public UserDataUtility2(string userData)
//        {
//            this.userData = userData;
//        }

//        public UserDataUtility2(string UserData, Stream filePath, Stream userPath) : this(UserData)
//        {
//            _filePath = filePath;
//            _userPath = userPath;
//        }

//        public interface IUserData
//        {
//            // Add a interface method called WriteToFile here:
//            // It should return void and take a string parameter called value
//            void WriteToFile(string value);

//            // Add another interface method called ReadFromFile
//            // It should return a string and take no parameters
//            string ReadFromFile();
//        }

//        internal static string ReadFromFile(string v)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
