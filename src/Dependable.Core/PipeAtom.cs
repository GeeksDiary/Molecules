using System;
using System.Threading.Tasks;

namespace Dependable.Core
{
    public class PipeAtom<TIn, TIntermediate, TOut> : Atom<TIn, TOut>
    {
        readonly Atom<TIn, TIntermediate> _first;
        readonly Atom<TIntermediate, TOut> _second;

        public PipeAtom(Atom<TIn, TIntermediate> first, Atom<TIntermediate, TOut> second)
        {
            _first = first;
            _second = second;
        }

        public override async Task<TOut> Charge(TIn input)
        {
            var i = await _first.Charge(input);
            return await _second.Charge(i);
        }
    }

    public static partial class Atom
    {
        public static PipeAtom<TIn, TIntermediate, TOut> Pipe<TIn, TIntermediate, TOut>(
            this Atom<TIn, TIntermediate> first,
            Func<TIntermediate, TOut> second)
        {
            return first.Pipe(Of(second));
        }

        public static PipeAtom<TIn, Unit, TOut> Pipe<TIn, TIntermediate, TOut>(
            this Atom<TIn, TIntermediate> first,
            Func<TOut> second)
        {
            return first.Ignore().Pipe(Of(second));
        }

        public static PipeAtom<TIn, TIntermediary, Unit> Pipe<TIn, TIntermediary>(
            this Atom<TIn, TIntermediary> first,
            Action<TIntermediary> second)
        {
            return first.Pipe(Of(second));
        }

        public static PipeAtom<TIn, Unit, Unit> Pipe<TIn, TIntermediary>(
            this Atom<TIn, TIntermediary> first,
            Action second)
        {
            return first.Ignore().Pipe(Of(second));
        }

        public static PipeAtom<TIn, TIntermediate, TOut> Pipe<TIn, TIntermediate, TOut>(
            this Atom<TIn, TIntermediate> first,
            Atom<TIntermediate, TOut> second)
        {
            return new PipeAtom<TIn, TIntermediate, TOut>(first, second);
        }

        public static ConditionAtom<TIn, TIntermediary, TOut> If<TIn, TIntermediary, TOut>(
            this Atom<TIn, TIntermediary> source,
            Predicate<TIntermediary> predicate,
            Func<TIntermediary, TOut> truthy,
            Func<TIntermediary, TOut> falsey)
        {
            return source.If(predicate, Of(truthy), Of(falsey));
        }
    }
}