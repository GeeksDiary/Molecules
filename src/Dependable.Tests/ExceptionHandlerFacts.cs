using Dependable.Dispatcher;
using Xunit;
using Xunit.Extensions;

namespace Dependable.Tests
{
    public class ExceptionFilterForMethodFacts
    {
        [Fact]
        public void CapturesConstantArguments()
        {
// ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            var handler = ExceptionFilter.From<string>((c, s) => s.IndexOf('a'));

            Assert.Equal('a', handler.Arguments[0]);
        }

        [Fact]
        public void CapturesLocalValiableReferences()
        {
// ReSharper disable once ConvertToConstant.Local
            var argument = 'a';

// ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            var handler = ExceptionFilter.From<string>((c, s) => s.IndexOf(argument));

            Assert.Equal(argument, handler.Arguments[0]);
        }

        [Theory]
        [InlineData('a')]
        public void CapturesArgumentReference(char argument)
        {
// ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            var handler = ExceptionFilter.From<string>((c, s) => s.IndexOf(argument));

            Assert.Equal(argument, handler.Arguments[0]);
        }

        [Fact]
        public void CapturesComplexPropertyReference()
        {
            var thisMethod = GetType().GetMethod("CapturesComplexPropertyReference");

// ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            var handler = ExceptionFilter.From<string>((c, s) => s.StartsWith(thisMethod.Name));

            Assert.Equal(thisMethod.Name, handler.Arguments[0]);
        }

        [Fact]
        public void CreatesAPlaceholderForExceptionContext()
        {
            var handler = ExceptionFilter.From<ExceptionFilterForMethodFacts>((c, h) => h.Log(c));

            Assert.IsType<ExceptionContext>(handler.Arguments[0]);
        }

        [Fact]
        public void CapturesMethodName()
        {
// ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            var handler = ExceptionFilter.From<string>((c, s) => s.IndexOf('c'));

            Assert.Equal("IndexOf", handler.Method);
        }

        [Fact]
        public void CapturesType()
        {
// ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            var handler = ExceptionFilter.From<string>((c, s) => s.IndexOf('c'));

            Assert.Equal(typeof (string), handler.Type);
        }

        public void Log(ExceptionContext context)
        {            
        }
    }
}