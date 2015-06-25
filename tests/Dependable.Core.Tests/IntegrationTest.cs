using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using Xunit;

namespace Dependable.Core.Tests
{
    public class IntegrationTest
    {
        readonly IApi _api = Substitute.For<IApi>();
        readonly Atom<Tuple<Profile, IEnumerable<Prescription>>> _workflow;

        readonly Profile _profile = new Profile
        {
            FirstName = "Alice",
            LastName = "Wonderland",
            Age = 25,
            TwitterHandle = "@awonderland",
            Email = "alice@wonderland.com"
        };

        public IntegrationTest()
        {
            /*
                A hypothetical workflow which consumes a few API services.

                It's executed with a string parameter indicating an email
                address of a person.
                
                First it attempts to load user profile for the given email.
                Then it uses the Twitter handle available in profile to retrieve 
                recent tweets by that person.
                Recent tweets are then piped into a psychological assessment 
                service for evaluation.
                If 5 or more recent tweets were evaluated as aggravated, 
                workflow will use medicate number in profile to retrieve 
                recent prescription medicine obtain by the user.
                Finally, it returns a result with user profile and the list 
                of recent prescription medication if there was any.
            */
            _workflow =
                from profile in Atom.Of((string s) => _api.LoadProfile(s))
                from tweets in Atom.Of(() => _api.RecentTweets(profile.TwitterHandle))
                    .Map(tweet => _api.PsychologicalAssessment(tweet.Text))
                    .If(
                        modes => modes.Count(mode => mode == Mode.Aggravated) >= 5,
                        Atom.Of(() => _api.RecentlyAcquiredMedication(profile.MedicareNumber)),
                        Atom.Of(() => Prescription.NotRequired))
                select Tuple.Create(profile, tweets);

            _api.LoadProfile(null).ReturnsForAnyArgs(_profile);
        }

        [Fact]
        public async void ReturnsOnlyProfileForPersonWithoutRecentTweets()
        {
            _api.RecentTweets(null).ReturnsForAnyArgs(Enumerable.Empty<Tweet>());
            _api.PsychologicalAssessment(null).Returns(Mode.Happy);

            var result = await _workflow.Charge("alice@wonderland.com");

            Assert.Equal(_profile, result.Item1);
            Assert.Equal(Prescription.NotRequired, result.Item2);
        }

        [Fact]
        public async void ReturnsOnlyProfileWithPositivePsychologicalAssessment()
        {
            _api.RecentTweets(null)
                .ReturnsForAnyArgs(Enumerable.Range(0, 5)
                .Select(i => new Tweet()));

            _api.PsychologicalAssessment(null).Returns(Mode.Happy);


            var result = await _workflow.Charge("alice@wonderland.com");

            Assert.Equal(_profile, result.Item1);
            Assert.Equal(Prescription.NotRequired, result.Item2);
        }

        [Fact]
        public async void ReturnsRecentlyAcquiredMedicationForProfileWithNegetivePsychologicalAssessment()
        {
            _api.RecentTweets(null)
                .ReturnsForAnyArgs(Enumerable.Range(0, 5)
                .Select(i => new Tweet()));

            _api.PsychologicalAssessment(null).Returns(Mode.Aggravated);

            var prescription = new Prescription();
            _api.RecentlyAcquiredMedication(null).Returns(new[] {prescription});

            var result = await _workflow.Charge("alice@wonderland.com");

            Assert.Equal(_profile, result.Item1);
            Assert.Contains(prescription, result.Item2);
        }
    }
}