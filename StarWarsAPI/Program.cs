using System;
using System.Net;
using SharpTrooper.Core;
using SharpTrooper.Entities;


namespace StarWarsAPI
{
    // =========================================================================
    // BUG-FIX + SAFETY NOTES (2026)
    // =========================================================================
    // - The program used to crash with an unhandled exception if you typed a
    //   letter, a number with no film/character, or if the internet/API was
    //   down. Now it checks your input, explains what went wrong, and lets you
    //   try again. Do or do not, there is no crash.
    // - Only whole numbers in range are accepted, so nothing odd can end up in
    //   the web address the program calls.
    // - The film prompt said #1-#7, but the API only has films 1-6. Fixed.
    //   Character #17 doesn't exist in the API (a famous gap), so a "not found"
    //   answer now asks you to pick another number instead of crashing.
    // - Unused fields (Value, StarWarsFilm, StarWarsPeople properties) that were
    //   shadowed by local variables were removed.
    // =========================================================================
    class Program
    {
        private const int FilmCount = 6;
        private const int PeopleCount = 83;

        static void Main(string[] args)
        {
            SharpTrooperCore core = new SharpTrooperCore();

            try
            {
                //The console will ask the user to input a Starwars film #
                film f = AskUntilFound(
                    "Enter Star Wars Film #1-#" + FilmCount + " that is your favorite.",
                    FilmCount,
                    id => core.GetFilm(id));
                Console.WriteLine(f.title);

                People p = AskUntilFound(
                    "Enter your favorite number #1-#" + PeopleCount + " to see what Star Wars character is associated with that #",
                    PeopleCount,
                    id => core.GetPeople(id));
                Console.WriteLine(p.name);

                //Store the input in a variable called "userData"
                UserDataUtility userDataUtility = new UserDataUtility();

                //Store the userinput in a variable called data
                string data = f.title + " " + p.name;

                //Store the user data to the file using UserDataUtility
                userDataUtility.WriteToFile(data);

                //Print a message saying that the data has been added to the Jedi Archives for safe keeping
                Console.WriteLine("Your Jedi Code has safely been added to the Jedi Archives");

                //Show the new data
                Console.WriteLine("Jedi Code: " + userDataUtility.ReadFromFile());
            }
            catch (WebException ex)
            {
                // The API couldn't be reached at all (no internet, site down, blocked, ...).
                Console.WriteLine("Could not reach the Star Wars API: " + ex.Message);
                Console.WriteLine("Check your internet connection, or set SWAPI_BASE_URL to another mirror (see README).");
            }

            //Add Console.ReadKey to make sure that the window doesn't close at any point during the session.
            Console.WriteLine("Press any key to exit. May the Force be with you.");
            Console.ReadKey();
        }

        // Keeps asking until the user types a valid number that the API knows about.
        // "Not found" (HTTP 404) means try another number; any other network error
        // is passed up to Main to report.
        private static T AskUntilFound<T>(string prompt, int max, Func<string, T> lookup) where T : class
        {
            while (true)
            {
                Console.WriteLine(prompt);
                string input = Console.ReadLine();

                int number;
                if (!int.TryParse(input, out number) || number < 1 || number > max)
                {
                    Console.WriteLine("That's not a number between 1 and " + max + ". These aren't the digits we're looking for - try again.");
                    continue;
                }

                try
                {
                    T result = lookup(number.ToString());
                    if (result != null)
                    {
                        return result;
                    }
                }
                catch (WebException ex) when ((ex.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.NotFound)
                {
                    // fall through to the message below
                }

                Console.WriteLine("Nothing found for #" + number + " - it's a trap! Please pick another number.");
            }
        }
    }
}
