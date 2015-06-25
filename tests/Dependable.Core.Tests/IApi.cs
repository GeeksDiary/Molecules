using System.Collections.Generic;
using System.Linq;

namespace Dependable.Core.Tests
{
    public interface IApi
    {
        void Naked();

        int Nullary();

        void Void(int value);

        int Call(int value);

        Profile LoadProfile(string email);

        IEnumerable<Tweet> RecentTweets(string handle);

        Mode PsychologicalAssessment(string thoughts);

        IEnumerable<Prescription> RecentlyAcquiredMedication(string medicareNumber);
    }

    public class Profile
    {
        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int Age { get; set; }

        public string TwitterHandle { get; set; }

        public string MedicareNumber { get; set; }
    }

    public class Tweet
    {
        public string Text { get; set; }
    }

    public class Prescription
    {
        public static IEnumerable<Prescription> NotRequired = Enumerable.Empty<Prescription>();
    }


    public enum Mode
    {
        Happy,
        Sad,
        Stressed,
        Aggravated,
        PassiveAggression
    }
}