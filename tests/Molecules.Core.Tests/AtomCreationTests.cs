using System.Threading.Tasks;
using NSubstitute;
using Xunit;

namespace Molecules.Core.Tests
{   
    public class AtomCreationTests
    {
        readonly ISignature _signature = Substitute.For<ISignature>();

        [Fact]
        public async void FuncWithoutInput()
        {
            _signature.Func().Returns(1);
            Assert.Equal(1, await Atom.Of(() => _signature.Func()).Charge());
        }

        [Fact]
        public async void FuncWithInput()
        {
            _signature.Func(1).Returns(2);
            Assert.Equal(2, await Atom.Of((int i) => _signature.Func(i)).Charge(1));
        }

        [Fact]
        public async void AsyncFuncWithoutInput()
        {
            _signature.AsyncFunc().Returns(Task.FromResult(1));
            Assert.Equal(1, await Atom.Of(() => _signature.AsyncFunc()).Charge());
        }

        [Fact]
        public async void AsyncFuncWithInput()
        {
            _signature.AsyncFunc(1).Returns(Task.FromResult(2));
            Assert.Equal(2, await Atom.Of((int i) => _signature.AsyncFunc(i)).Charge(1));
        }

        [Fact]
        public async void ActionWithoutInput()
        {
            await Atom.Of(() => _signature.Action()).Charge();
            _signature.Received(1).Action();
        }

        [Fact]
        public async void ActionWithInput()
        {
            await Atom.Of((int i) => _signature.Action(i)).Charge(1);
            _signature.Received(1).Action(1);
        }

        [Fact]
        public async void AsyncAction()
        {
            _signature.AsyncAction().Returns(Task.FromResult(new object()));
            await Atom.Of(() => _signature.AsyncAction()).Charge();

#pragma warning disable 4014
            _signature.Received(1).AsyncAction();
#pragma warning restore 4014
        }

        [Fact]
        public async void AsyncActionWithInput()
        {
            _signature.AsyncAction(1).Returns(Task.FromResult(new object()));
            await Atom.Of((int i) => _signature.AsyncAction(i)).Charge(1);

#pragma warning disable 4014
            _signature.Received(1).AsyncAction(1);
#pragma warning restore 4014
        }
    }
}