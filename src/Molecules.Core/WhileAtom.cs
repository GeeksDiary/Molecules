using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Molecules.Core
{
    public class WhileAtom<TIn, TTest, TOut> : Atom<TOut>
    {
        readonly Predicate<TTest> _predicate;
        readonly Func<TIn, TTest, TIn> _with;

        public Atom<TTest> Test { get; set; }

        public Expression<Predicate<TTest>> PredicateExpression { get; }

        public Expression<Func<TIn, TTest, TIn>> WithExpression { get; }

        public Atom<TOut> Body { get; set; }

        public WhileAtom<TIn, TTest, TOut> With(Expression<Func<TIn, TTest, TIn>> with)
        {
            return new WhileAtom<TIn, TTest, TOut>(Test, PredicateExpression, Body, with);
        }

        public WhileAtom(Atom<TTest> test, 
            Expression<Predicate<TTest>> predicate,
            Atom<TOut> body,
            Expression<Func<TIn, TTest, TIn>> with = null
            )
        {
            Test = test;
            PredicateExpression = predicate;
            Body = body;
            WithExpression = with ?? ((i, _) => i);
            _with = WithExpression.Compile();
            _predicate = predicate.Compile();
        }

        protected override async Task<TOut> OnCharge(object input = null)
        {
            var w = (TIn) input;
            var t = await Test.Charge(w);
            var r = default(TOut);

            while (_predicate(t))
            {
                r = await Body.Charge(t);
                w = _with(w, t);
                t = await Test.Charge(w);
            }

            return r;
        }
    }

    public class WhileAtomBuilder<TIn, TTest>
    {
        readonly FuncAtom<TIn, TTest> _test;
        readonly Expression<Predicate<TTest>> _predicate;
        
        public WhileAtomBuilder(FuncAtom<TIn, TTest> test, 
            Expression<Predicate<TTest>> predicate)
        {
            _test = test;
            _predicate = predicate;
        }

        public WhileAtom<TIn, TTest, TOut> Do<TOut>(Expression<Func<TTest, TOut>> body)
        {
            return new WhileAtom<TIn, TTest, TOut>(_test, _predicate, Atom.Of(body));
        }       
    }

    public static partial class Atom
    {
        public static WhileAtomBuilder<TIn, TTest> While<TIn, TTest>(this FuncAtom<TIn, TTest> test,
            Expression<Predicate<TTest>> predicate)
        {
            return new WhileAtomBuilder<TIn, TTest>(test, predicate);
        }
    }
}