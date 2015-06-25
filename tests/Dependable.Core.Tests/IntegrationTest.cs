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
            _workflow =
                from profile in Atom.Of<string, Profile>(s => _api.LoadProfile(s))
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