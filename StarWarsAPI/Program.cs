using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpTrooper.Core;
using SharpTrooper.Entities;
using System.Net;
using System.IO;


namespace StarWarsAPI
{
    class Program
    {
        private const string Value = "Please enter the Title of your favorite Star Wars Film and the character associated with your favorite # for the file";
       

        public static object StarWarsFilm { get; private set; }
        public static object StarWarsPeople { get; private set; }


        static void Main(string[] args)
        {
            SharpTrooperCore core = new SharpTrooperCore();

            //The console will ask the user to input a Starwars film #
            Console.WriteLine("Enter Star Wars Film #1-#7 that is your favorite.");

            //Console will listen for user input
            var StarWarsFilm = Console.ReadLine();

            //Console looks in api for the # the user inputs and then prints the title that is associated with that number.
            film f = core.GetFilm(StarWarsFilm);
            Console.WriteLine(f.title);

            Console.WriteLine("Enter your favorite number #1-#80 to see what Star Wars character is associated with that #, After Displayed Press Enter to continue");
            var StarWarsPeople = Console.ReadLine();
            People p = core.GetPeople(StarWarsPeople);
            Console.WriteLine(p.name);

            //Store the input in a variable called "userData"
            UserDataUtility UserDataUtility = new UserDataUtility();

            //Store the userinput in a variable called data
            string data = f.title + " " + p.name;

            //Store the user data to the file using UserDataUtility
            UserDataUtility.WriteToFile(data);

            //Print a message saying that the data has been added to the Jedi Archives for safe keeping
            Console.WriteLine("Your Jedi Code has safely been added to the Jedi Archives");

            //Show the new data
            Console.WriteLine(value: "Jedi Code: " + UserDataUtility.ReadFromFile("./JediArchives.txt"));

            //Add Console.ReadKey to make sure that the window doesn't close at any point during the session.
            Console.ReadKey();

        }
    }
}
