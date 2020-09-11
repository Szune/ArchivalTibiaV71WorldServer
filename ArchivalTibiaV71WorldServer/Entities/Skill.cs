using System;
using System.Linq.Expressions;

namespace ArchivalTibiaV71WorldServer.Entities
{
    public class Skill<TSize> where TSize : struct, IConvertible
    {
        private static readonly Func<TSize, TSize, TSize> Add;
        private static readonly Func<TSize, TSize, bool> LessThan;


        static Skill()
        {
            ParameterExpression paramA = Expression.Parameter(typeof(TSize), "a"),
                paramB = Expression.Parameter(typeof(TSize), "b");
            // Thanks to Jon Skeet: https://jonskeet.uk/csharp/genericoperators.html
            Expression addBody;
            if (typeof(TSize) == typeof(byte))
            { // byte has no addition operator implemented on it :(
                addBody = Expression.Convert(
                    Expression.Add(
                        Expression.Convert(paramA, typeof(int)),
                        Expression.Convert(paramB, typeof(int))),
                    typeof(TSize));
            }
            else
            {
                // but the others do!
                addBody =
                    Expression.Add(
                        paramA,
                        paramB);
            }
            Add = Expression.Lambda<Func<TSize, TSize, TSize>>(addBody, paramA, paramB).Compile();
            var lessThanBody = Expression.LessThan(paramA, paramB);
            LessThan = Expression.Lambda<Func<TSize, TSize, bool>>(lessThanBody, paramA, paramB).Compile();
        }


        public TSize Value;
        public uint Exp;
        public TSize Min;

        /// <summary>
        /// Recalculated on speed changing spell cast and at the end of the spell durations.
        /// </summary>
        public TSize MagicBonus;

        /// <summary>
        /// Recalculated on equipment change
        /// </summary>
        public TSize EquipmentBonus;

        public Skill(TSize value, TSize min)
        {
            Value = value;
            Min = min;
        }
        
        public Skill(TSize value, TSize min, uint exp)
        {
            Value = value;
            Min = min;
            Exp = exp;
        }

        public TSize Get()
        {
            var value = Value;
            if (LessThan(value, Min))
            {
                value = Min;
            }
            // value + MagicBonus + EquipmentBonus
            return Add(Add(value, MagicBonus), // value + MagicBonus
                EquipmentBonus); // + EquipmentBonus
        }
        
        public void Set(TSize value)
        {
            Value = value;
        }
        
        public void SetExp(uint exp)
        {
            Exp = exp;
        }
    }
}