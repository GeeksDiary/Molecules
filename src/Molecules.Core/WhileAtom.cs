using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Molecules.Core
{
    public class WhileAtom<TTest, TOut> : Atom<TOut>
    {
        readonly Predicate<TTest> _predicate;
        
        public Atom<TTest> Test { get; }
        
        public Atom<TOut> Body { get; }

        public WhileAtom(Atom<TTest> test, 
            Predicate<TTest> predicate,
            Atom<TOut> body            
            )
        {
            Test = test;
            Body = body;
            _predicate = predicate;
        }

        internal override async Task<TOut> ChargeCore(AtomContext atomContext)
        {
            var t = await Test.ChargeCore(atomContext);
            var r = default(TOut);

            while (_predicate(t))
            {
                r = await Body.ChargeCore(AtomContext.For(t));
                t = await Test.ChargeCore(atomContext);
            }            

            return r;
        }
    }

    public class WhileAtomBuilder<TTest>
    {
        readonly Atom<TTest> _test;
        readonly Predicate<TTest> _predicate;

        public WhileAtomBuilder(Atom<TTest> test, Predicate<TTest> predicate)
        {
            _test = test;
            _predicate = predicate;
        }

        public WhileAtom<TTest, TBody> Do<TBody>(Atom<TBody> body)
        {
            return new WhileAtom<TTest, TBody>(_test, _predicate, body);
        }

        public WhileAtom<TTest, TBody> Do<TBody>(Func<AtomContext<TTest>, Task<TBody>> body)
        {
            return Do(Atom.Func(body));
        }

        public WhileAtom<TTest, TBody> Do<TBody>(Func<AtomContext<TTest>, TBody> body)
        {
            return Do(Atom.Func(body));
        }

        public WhileAtom<TTest, TBody> Do<TBody>(Func<TBody> body)
        {
            return Do(Atom.Func(body));
        }

        public WhileAtom<TTest, TBody> Do<TBody>(Func<Task<TBody>> body)
        {
            return Do(Atom.Func(body));
        }
    }

    public static partial class Atom
    {
        public static WhileAtomBuilder<TTest> While<TTest>(this Atom<TTest> test,
            Predicate<TTest> predicate)
        {
            return new WhileAtomBuilder<TTest>(test, predicate);
        }
    }
}