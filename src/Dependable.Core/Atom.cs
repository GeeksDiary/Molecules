using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dependable.Core
{
    public abstract class Atom<TIn, TOut>
    {
        public abstract Task<TOut> Charge(TIn input);
    }

    public class SimpleAtom<TIn, TOut> : Atom<TIn, TOut>
    {
        readonly Func<TIn, Task<TOut>> _impl;

        internal SimpleAtom(Func<TIn, Task<TOut>> impl)
        {
            _impl = impl;
        }

        public override Task<TOut> Charge(TIn input)
        {
            return _impl(input);
        }
    }

    public class LinkAtom<TIn, TIntermediate, TOut> : Atom<TIn, TOut>
    {
        readonly Atom<TIn, TIntermediate> _first;
        readonly Atom<TIntermediate, TOut> _second;

        public LinkAtom(Atom<TIn, TIntermediate> first, Atom<TIntermediate, TOut> second)
        {
            _first = first;
            _second = second;
        }

        public async override Task<TOut> Charge(TIn input)
        {
            var i = await _first.Charge(input);
            return await _second.Charge(i);
        }
    }

    public class JunctionAtom<TIn, TIntermediary, TOut> : Atom<TIn, TOut>
    {
        readonly Atom<TIn, TIntermediary> _source;
        readonly Predicate<TIntermediary> _predicate;
        readonly Atom<TIntermediary, TOut> _truthy;
        readonly Atom<TIntermediary, TOut> _falsey;

        public JunctionAtom(Atom<TIn, TIntermediary> source, 
            Predicate<TIntermediary> predicate, Atom<TIntermediary, TOut> truthy, Atom<TIntermediary, TOut> falsey)
        {
            _source = source;
            _predicate = predicate;
            _truthy = truthy;
            _falsey = falsey;
        }

        public async override Task<TOut> Charge(TIn input)
        {
            var i = await _source.Charge(input);
            return await (_predicate(i) ? _truthy.Charge(i) : _falsey.Charge(i));
        }
    }

    public class NullaryAtom<TOut> : SimpleAtom<Value, TOut>
    {
        internal NullaryAtom(Func<Task<TOut>> impl) : base(v => impl())
        {
        }

        public async Task<TOut> Charge()
        {
            return await base.Charge(Value.None);
        }
    }

    public class IgnoreAtom<TIn, TOut> : Atom<TIn, Value>
    {
        readonly Atom<TIn, TOut> _source;

        internal IgnoreAtom(Atom<TIn, TOut> source)
        {
            _source = source;
        }

        public async override Task<Value> Charge(TIn input)
        {
            await _source.Charge(input);
            return await Task.FromResult(Value.None);
        }
    }

    public class VoidAtom<TIn> : SimpleAtom<TIn, Value>
    {
        internal VoidAtom(Func<TIn, Task<Value>> impl) : base(impl)
        {
        }
    }

    public class NakedAtom : SimpleAtom<Value, Value>
    {
        internal NakedAtom(Func<Value, Task<Value>> impl) : base(impl)
        {
        }

        public async Task<Value> Charge()
        {
            return await base.Charge(Value.None);
        }
    }

    public class MapAtom<TIn, TOut> : Atom<IEnumerable<TIn>, IEnumerable<TOut>>
    {
        internal MapAtom(Func<IEnumerable<TIn>, Task<IEnumerable<TOut>>> impl)
        {
        }

        public override Task<IEnumerable<TOut>> Charge(IEnumerable<TIn> input)
        {
            throw new NotImplementedException();
        }
    }

    public static class Atom
    {
        public static SimpleAtom<TIn, TOut> Of<TIn, TOut>(Func<TIn, TOut> impl)
        {
            return new SimpleAtom<TIn, TOut>(i => Task.FromResult(impl(i)));
        }

        public static NullaryAtom<TOut> Of<TOut>(Func<TOut> impl)
        {
            return new NullaryAtom<TOut>(() => Task.FromResult(impl()));
        }

        public static VoidAtom<TIn> Of<TIn>(Action<TIn> impl)
        {
            return new VoidAtom<TIn>(i =>
            {
                impl(i);
                return Value.CompletedNone;
            });
        }

        public static NakedAtom Of(Action impl)
        {
            return new NakedAtom(i => {
                impl();
                return Value.CompletedNone;
            });
        }

        public static MapAtom<TIn, TOut> Of<TIn, TOut>(Func<IEnumerable<TIn>, IEnumerable<TOut>> impl)
        {
            return new MapAtom<TIn, TOut>(i => Task.FromResult(impl(i)));
        }

        public static IgnoreAtom<TIn, TOut> Ignore<TIn, TOut>(this Atom<TIn, TOut> source)
        {
            return new IgnoreAtom<TIn, TOut>(source);
        } 

        public static LinkAtom<TIn, TIntermediate, TOut> Connect<TIn, TIntermediate, TOut>(this Atom<TIn, TIntermediate> first,
            Func<TIntermediate, TOut> second)
        {
            return first.Connect(Of(second));
        }

        public static LinkAtom<TIn, Value, TOut> Connect<TIn, TIntermediate, TOut>(this Atom<TIn, TIntermediate> first,
            Func<TOut> second)
        {
            return first.Ignore().Connect(Of(second));
        }

        public static LinkAtom<TIn, TIntermediary, Value> Connect<TIn, TIntermediary>(
            this Atom<TIn, TIntermediary> first,
            Action<TIntermediary> second)
        {
            return first.Connect(Of(second));
        }

        public static LinkAtom<TIn, Value, Value> Connect<TIn, TIntermediary>(
            this Atom<TIn, TIntermediary> first,
            Action second)
        {
            return first.Ignore().Connect(Of(second));
        }

        public static LinkAtom<TIn, TIntermediate, TOut> Connect<TIn, TIntermediate, TOut>(this Atom<TIn, TIntermediate> first,
            Atom<TIntermediate, TOut> second)
        {
            return new LinkAtom<TIn, TIntermediate, TOut>(first, second);
        }

        public static JunctionAtom<TIn, TIntermediary, TOut> If<TIn, TIntermediary, TOut>(
            this Atom<TIn, TIntermediary> source,
            Predicate<TIntermediary> predicate,
            Func<TIntermediary, TOut> truthy,
            Func<TIntermediary, TOut> falsey)
        {
            return source.If(predicate, new SimpleAtom<TIntermediary, TOut>(i => Task.FromResult(truthy(i))),
                new SimpleAtom<TIntermediary, TOut>(i => Task.FromResult(falsey(i))));
        }

        public static JunctionAtom<TIn, TIntermediary, TOut> If<TIn, TIntermediary, TOut>(
            this Atom<TIn, TIntermediary> source,
            Predicate<TIntermediary> predicate, 
            Atom<TIntermediary, TOut> truthy,
            Atom<TIntermediary, TOut> falsey)
        {
            return new JunctionAtom<TIn, TIntermediary, TOut>(source, predicate, truthy, falsey);
        }
    }
}