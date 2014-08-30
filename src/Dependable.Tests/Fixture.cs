using System;

namespace Dependable.Tests
{
    public static class Fixture
    {
        public static DateTime Now
        {
            get { return new DateTime(2004, 01, 01, 0, 0, 0); }
        }

        public static DateTime InSeconds(int seconds)
        {
            return Now + TimeSpan.FromSeconds(seconds);
        }
    }
}