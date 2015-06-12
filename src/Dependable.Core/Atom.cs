using System;
using System.Collections.Generic;
using System.Linq;
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

    public abstract class JunctionAtom<TIn, TIntermediary, TOut> : Atom<TIn, TOut>
    {
        readonly Atom<TIn, TIntermediary> _source;
        readonly Predicate<TIntermediary> _predicate;

        protected JunctionAtom(Atom<TIn, TIntermediary> source,
            Predicate<TIntermediary> predicate)
        {
            _source = source;
            _predicate = predicate;        }

        public async override Task<TOut> Charge(TIn input)
        {
            var i = await _source.Charge(input);
            return await(_predicate(i) ? OnTruthy(i) : OnFalsey(i));
        }

        protected abstract Task<TOut> OnTruthy(TIntermediary intermediary);

        protected abstract Task<TOut> OnFalsey(TIntermediary intermediary);
    }

    public class SimpleAtomJunction<TIn, TIntermediary, TOut> : JunctionAtom<TIn, TIntermediary, TOut>
    {
        readonly Atom<TIntermediary, TOut> _truthy;
        readonly Atom<TIntermediary, TOut> _falsey;

        public SimpleAtomJunction(Atom<TIn, TIntermediary> source, 
            Predicate<TIntermediary> predicate, 
            Atom<TIntermediary, TOut> truthy, 
            Atom<TIntermediary, TOut> falsey) : base(source, predicate)
        {
            _truthy = truthy;
            _falsey = falsey;
        }

        protected async override Task<TOut> OnTruthy(TIntermediary intermediary)
        {
            return await _truthy.Charge(intermediary);
        }

        protected async override Task<TOut> OnFalsey(TIntermediary intermediary)
        {
            return await _falsey.Charge(intermediary);
        }
    }

    public class NullaryAtomJuction<TIn, TIntermediary, TOut> : JunctionAtom<TIn, TIntermediary, TOut>
    {
        readonly NullaryAtom<TOut> _truthy;
        readonly NullaryAtom<TOut> _falsey;

        public NullaryAtomJuction(
            Atom<TIn, TIntermediary> source, 
            Predicate<TIntermediary> predicate,
            NullaryAtom<TOut> truthy,
            NullaryAtom<TOut> falsey) : 
            base(source, predicate)
        {
            _truthy = truthy;
            _falsey = falsey;
        }

        protected async override Task<TOut> OnTruthy(TIntermediary intermediary)
        {
            return await _truthy.Charge();
        }

        protected override async Task<TOut> OnFalsey(TIntermediary intermediary)
        {
            return await _falsey.Charge();
        }
    }

    public class VoidAtomJunction<TIn, TIntermediary> : JunctionAtom<TIn, TIntermediary, Value>
    {
        readonly VoidAtom<TIntermediary> _truthy;
        readonly VoidAtom<TIntermediary> _falsey;

        public VoidAtomJunction(
            Atom<TIn, TIntermediary> source, 
            Predicate<TIntermediary> predicate,
            VoidAtom<TIntermediary> truthy,
            VoidAtom<TIntermediary> falsey) : base(source, predicate)
        {
            _truthy = truthy;
            _falsey = falsey;
        }

        protected async override Task<Value> OnTruthy(TIntermediary intermediary)
        {
            return await _truthy.Charge(intermediary);
        }

        protected async override Task<Value> OnFalsey(TIntermediary intermediary)
        {
            return await _falsey.Charge(intermediary);
        }
    }

    public class NakedAtomJunction<TIn, TIntermediary> : JunctionAtom<TIn, TIntermediary, Value>
    {
        readonly NakedAtom _truthy;
        readonly NakedAtom _falsey;

        public NakedAtomJunction(Atom<TIn, 
            TIntermediary> source, 
            Predicate<TIntermediary> predicate,
            NakedAtom truthy,
            NakedAtom falsey) : base(source, predicate)
        {
            _truthy = truthy;
            _falsey = falsey;
        }

        protected override async Task<Value> OnTruthy(TIntermediary intermediary)
        {
            return await _truthy.Charge();
        }

        protected override async Task<Value> OnFalsey(TIntermediary intermediary)
        {
            return await _falsey.Charge();
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

    public class SimpleAtomMap<TIn, TIntermediary, TOut> : Atom<TIn, IEnumerable<TOut>>
    {
        readonly Atom<TIn, IEnumerable<TIntermediary>> _source;
        readonly Atom<TIntermediary, TOut> _map;

        public SimpleAtomMap(Atom<TIn, IEnumerable<TIntermediary>> source, Atom<TIntermediary, TOut> map)
        {
            _source = source;
            _map = map;
        }
        
        public async override Task<IEnumerable<TOut>> Charge(TIn input)
        {
            var d = await _source.Charge(input);
            return await Task.WhenAll(d.Select(i => _map.Charge(i)));
        }
    }

    public class NullaryAtomMap<TIntermediary, TOut> : SimpleAtomMap<Value, TIntermediary, TOut>
    {
        public NullaryAtomMap(NullaryAtom<IEnumerable<TIntermediary>> source, Atom<TIntermediary, TOut> map) : 
            base(source, map)
        {
        }

        public async Task<IEnumerable<TOut>> Charge()
        {
            return await base.Charge(Value.None);
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

        public static IgnoreAtom<TIn, TOut> Ignore<TIn, TOut>(this Atom<TIn, TOut> source)
        {
            return new IgnoreAtom<TIn, TOut>(source);
        } 

        public static LinkAtom<TIn, TIntermediate, TOut> Connect<TIn, TIntermediate, TOut>(
            this Atom<TIn, TIntermediate> first,
            Func<TIntermediate, TOut> second)
        {
            return first.Connect(Of(second));
        }

        public static LinkAtom<TIn, Value, TOut> Connect<TIn, TIntermediate, TOut>(
            this Atom<TIn, TIntermediate> first,
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
            return source.If(predicate, Of(truthy), Of(falsey));
        }

        public static JunctionAtom<TIn, TIntermediary, TOut> If<TIn, TIntermediary, TOut>(
            this Atom<TIn, TIntermediary> source,
            Predicate<TIntermediary> predicate, 
            Atom<TIntermediary, TOut> truthy,
            Atom<TIntermediary, TOut> falsey)
        {
            return new SimpleAtomJunction<TIn, TIntermediary, TOut>(source, predicate, truthy, falsey);
        }

        public static JunctionAtom<TIn, TIntermediary, TOut> If<TIn, TIntermediary, TOut>(
            this Atom<TIn, TIntermediary> source,
            Predicate<TIntermediary> predicate,
            Func<TOut> truthy,
            Func<TOut> falsey)
        {
            return source.If(predicate, Of(truthy), Of(falsey));
        }

        public static JunctionAtom<TIn, TIntermediary, TOut> If<TIn, TIntermediary, TOut>(
            this Atom<TIn, TIntermediary> source,
            Predicate<TIntermediary> predicate,
            NullaryAtom<TOut> truthy,
            NullaryAtom<TOut> falsey)
        {
            return new NullaryAtomJuction<TIn, TIntermediary, TOut>(source, predicate, truthy, falsey);
        }

        public static VoidAtomJunction<TIn, TIntermediary> If<TIn, TIntermediary>(
            this Atom<TIn, TIntermediary> source,
            Predicate<TIntermediary> predicate,
            Action<TIntermediary> truthy,
            Action<TIntermediary> falsey)
        {
            return source.If(predicate, Of(truthy), Of(falsey));
        }

        public static VoidAtomJunction<TIn, TIntermediary> If<TIn, TIntermediary>(
            this Atom<TIn, TIntermediary> source,
            Predicate<TIntermediary> predicate,
            VoidAtom<TIntermediary> truthy,
            VoidAtom<TIntermediary> falsey)
        {
            return new VoidAtomJunction<TIn, TIntermediary>(source, predicate, truthy, falsey);
        }

        public static NakedAtomJunction<TIn, TIntermediary> If<TIn, TIntermediary>(
            this Atom<TIn, TIntermediary> source,
            Predicate<TIntermediary> predicate,
            Action truthy,
            Action falsey)
        {
            return source.If(predicate, Of(truthy), Of(falsey));
        }

        public static NakedAtomJunction<TIn, TIntermediary> If<TIn, TIntermediary>(
            this Atom<TIn, TIntermediary> source,
            Predicate<TIntermediary> predicate,
            NakedAtom truthy,
            NakedAtom falsey
            )
        {
            return new NakedAtomJunction<TIn, TIntermediary>(source, predicate, truthy, falsey);
        }

        public static SimpleAtomMap<TIn, TIntermediary, TOut> Map<TIn, TIntermediary, TOut>(
            this Atom<TIn, IEnumerable<TIntermediary>> source,
            Func<TIntermediary, TOut> map)
        {
            return new SimpleAtomMap<TIn, TIntermediary, TOut>(source, Of(map));
        }

        public static NullaryAtomMap<TIntermediary, TOut> Map<TIntermediary, TOut>(
            this NullaryAtom<IEnumerable<TIntermediary>> source,
            Func<TIntermediary, TOut> map)
        {
            return new NullaryAtomMap<TIntermediary, TOut>(source, Of(map));
        }
    }
}