﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Symbolism.Numerator;
using Symbolism.Denominator;

using static Symbolism.ListConstructor;

namespace Symbolism.CosFun
{
    public class Cos : Function
    {
        public static MathObject Mod(MathObject x, MathObject y)
        {
            if (x is Number && y is Number)
            {
                var result = Convert.ToInt32(Math.Floor(((x / y) as Number).ToDouble().val));

                return x - y * result;
            }

            throw new Exception();
        }

        Func<MathObject, MathObject> cos = obj => new Cos(obj).Simplify();

        MathObject CosProc(params MathObject[] ls)
        {
            var Pi = new Symbol("Pi");

            var half = new Integer(1) / 2;

            if (ls[0] == 0) return 1;

            if (ls[0] == new Symbol("Pi")) return -1;

            if (ls[0] is DoubleFloat)
                return new DoubleFloat(Math.Cos(((DoubleFloat)ls[0]).val));

            if (ls[0] is Number && ls[0] < 0) return new Cos(-ls[0]);

            if (ls[0] is Product &&
                (ls[0] as Product).elts[0] is Number &&
                ((ls[0] as Product).elts[0] as Number) < 0)
                return new Cos(-ls[0]).Simplify();

            // cos(a/b * Pi)
            // a/b > 1/2         

            if (ls[0] is Product &&
                (
                    (ls[0] as Product).elts[0] is Integer ||
                    (ls[0] as Product).elts[0] is Fraction
                ) &&
                ((ls[0] as Product).elts[0] as Number) > new Integer(1) / 2 &&
                (ls[0] as Product).elts[1] == Pi
                )
            {
                var n = (ls[0] as Product).elts[0];

                if (n > 2) return cos(Mod(n, 2) * Pi);

                if (n > 1) return -cos(n * Pi - Pi);

                if (n > half) return -cos(Pi - n * Pi);

                return new Cos(n * Pi);
            }

            // cos(k/n Pi)
            // n is one of 1 2 3 4 6

            if (ls[0] is Product &&

                List<MathObject>(1, 2, 3, 4, 6)
                    .Any(elt => elt == (ls[0] as Product).elts[0].Denominator()) &&

                (ls[0] as Product).elts[0].Numerator() is Integer &&
                (ls[0] as Product).elts[1] == Pi
                )
            {
                var k = (ls[0] as Product).elts[0].Numerator();
                var n = (ls[0] as Product).elts[0].Denominator();

                if (n == 1)
                {
                    if (Mod(k, 2) == 1) return -1;
                    if (Mod(k, 2) == 0) return 1;
                }

                if (n == 2)
                {
                    if (Mod(k, 2) == 1) return 0;
                }

                if (n == 3)
                {
                    if (Mod(k, 6) == 1) return half;
                    if (Mod(k, 6) == 5) return half;

                    if (Mod(k, 6) == 2) return -half;
                    if (Mod(k, 6) == 4) return -half;
                }

                if (n == 4)
                {
                    if (Mod(k, 8) == 1) return 1 / (2 ^ half);
                    if (Mod(k, 8) == 7) return 1 / (2 ^ half);

                    if (Mod(k, 8) == 3) return -1 / (2 ^ half);
                    if (Mod(k, 8) == 5) return -1 / (2 ^ half);
                }

                if (n == 6)
                {
                    if (Mod(k, 12) == 1) return (3 ^ half) / 2;
                    if (Mod(k, 12) == 11) return (3 ^ half) / 2;

                    if (Mod(k, 12) == 5) return -(3 ^ half) / 2;
                    if (Mod(k, 12) == 7) return -(3 ^ half) / 2;
                }
            }

            // cos(n Pi + x + y)

            // n * Pi where n is Exact && abs(n) >= 2

            Func<MathObject, bool> Product_n_Pi = elt =>
                    (elt is Product) &&
                    (
                        (elt as Product).elts[0] is Integer ||
                        (elt as Product).elts[0] is Fraction
                    ) &&
                    Math.Abs(((elt as Product).elts[0] as Number).ToDouble().val) >= 2.0 &&

                    (elt as Product).elts[1] == Pi;

            if (ls[0] is Sum && (ls[0] as Sum).elts.Any(Product_n_Pi))
            {
                var pi_elt = (ls[0] as Sum).elts.First(Product_n_Pi);

                var n = (pi_elt as Product).elts[0];

                return cos((ls[0] - pi_elt) + Mod(n, 2) * Pi);
            }

            Func<MathObject, bool> Product_n_div_2_Pi = elt =>
                elt is Product &&
                (
                    (elt as Product).elts[0] is Integer ||
                    (elt as Product).elts[0] is Fraction
                ) &&
                (elt as Product).elts[0].Denominator() == 2 &&
                (elt as Product).elts[1] == Pi;

            // cos(a + b + ... + n/2 * Pi) -> sin(a + b + ...)

            if (ls[0] is Sum && (ls[0] as Sum).elts.Any(Product_n_div_2_Pi))
            {
                var n_div_2_Pi = (ls[0] as Sum).elts.First(Product_n_div_2_Pi);

                var other_elts = ls[0] - n_div_2_Pi;

                var n = (n_div_2_Pi as Product).elts[0].Numerator();

                if (Mod(n, 4) == 1) return -new Sin(other_elts);
                if (Mod(n, 4) == 3) return new Sin(other_elts);
            }

            return new Cos(ls[0]);
        }

        public Cos(MathObject param)
        {
            name = "cos";
            args = new List<MathObject>() { param };
            proc = CosProc;
        }
    }
}
