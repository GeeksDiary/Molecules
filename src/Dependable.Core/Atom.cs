using System;
using System.Collections.Generic;
using System.Linq.Expressions;
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
        public static Atom<TIn, TOut> Of<TIn, TOut>(Func<TIn, TOut> impl)
        {
            return new SimpleAtom<TIn, TOut>(i => Task.FromResult(impl(i)));
        }

        public static MapAtom<TIn, TOut> Of<TIn, TOut>(Func<IEnumerable<TIn>, IEnumerable<TOut>> impl)
        {
            return new MapAtom<TIn, TOut>(i => Task.FromResult(impl(i)));
        }

        public static LinkAtom<TIn, TIntermediate, TOut> Connect<TIn, TIntermediate, TOut>(this Atom<TIn, TIntermediate> first,
            Func<TIntermediate, TOut> second)
        {
            return first.Connect(new SimpleAtom<TIntermediate, TOut>(i => Task.FromResult(second(i))));
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