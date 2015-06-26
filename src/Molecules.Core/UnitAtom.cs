using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Molecules.Core
{
    public class UnitAtom : FuncAtom<Unit>
    {
        internal UnitAtom(Func<object, Task<Unit>> impl, Expression body) :
            base(impl, body)
        {
        }
    }

    public static partial class Atom
    {
        static UnitAtom Of(Func<Task<Unit>> impl, Expression body)
        {
            return new UnitAtom(_ => impl(), body);
        }

        public static UnitAtom Of(Expression<Func<Task>> body)
        {
            var compiled = body.Compile();
            return Of(async () =>
            {
                await compiled();
                return Unit.Value;
            }, body);
        }

        public static UnitAtom Of(Expression<Action> body)
        {
            var compiled = body.Compile();            
            return Of(() => {
                compiled();
                return Unit.CompletedTask;
            }, body);
        }

        static UnitAtom Of<T>(Func<T, Task<Unit>> impl, Expression body)
        {
            return new UnitAtom(i => impl((T)i), body);
        }

        public static UnitAtom Of<T>(Expression<Action<T>> body)
        {
            var compiled = body.Compile();
            return Of<T>(i =>
            {
                compiled(i);
                return Unit.CompletedTask;
            }, body);
        }

        public static UnitAtom Of<T>(Expression<Func<T, Task>> body)
        {
            var compiled = body.Compile();
            return Of<T>(async i =>
            {
                await compiled(i);
                return Unit.Value;
            }, body);
        }
    }
}