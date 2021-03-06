﻿using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    abstract class RedundantDeclaration
    {
        public RedundantDeclaration()
        {
            MyEvent += new EventHandler((a, b) => { }); // Noncompliant, needlessly verbose
            MyEvent += (a, b) => { };

            (new EventHandler((a, b) => { }))(1, null);

            MyEvent2 = new MyMethod((i, j) => { }); // Noncompliant
            MyEvent2 = new MyMethod(            // Noncompliant
                delegate (int i, int j) { });   // Noncompliant

            MyEvent2 = delegate (int i, int j) { Console.WriteLine(); }; //Noncompliant
            MyEvent = delegate { Console.WriteLine("fdsfs"); };

            var l = new List<int>() { }; // Noncompliant
            l = new List<int>();
            var o = new object() { }; // Noncompliant
            o = new object { };

            var ints = new int[] { 1, 2, 3 }; // Noncompliant
            ints = new[] { 1, 2, 3 };
            ints = new int[3] { 1, 2, 3 }; // Noncompliant

            var ddd = new double[] { 1, 2, 3.0 }; // Compliant the element types are not the same as the specified one

            var xxx = new int[,] { { 1, 1, 1 }, { 2, 2, 2 }, { 3, 3, 3 } };
            var yyy = new int[3 // Noncompliant, we report two issues on this to keep the comma unfaded
                , 3// Noncompliant
                ] { { 1, 1, 1 }, { 2, 2, 2 }, { 3, 3, 3 } };
            var zzz = new int[][] { new[] { 1, 2, 3 }, new int[0], new int[0] }; // Noncompliant
            var www = new int[][][] { new[] { new[] { 0 } } }; // Noncompliant

            int? xx = ((new int?(5))); // Noncompliant
            xx = new Nullable<int>(5); // Noncompliant
            var rr = new int?(5);

            NullableTest1(new int?(5));
            NullableTest1<int>(new int?(5)); // Noncompliant
            NullableTest2(new int?(5)); // Noncompliant

            Func<int, int?> f = new Func<int, int?>(// Noncompliant
                i => new int?(i)); // Noncompliant
            f = i =>
            {
                return new int?(i); // Noncompliant
            };

            Delegate d = new Action(() => { });
            Delegate d = new Func<double>(() => { return 1; });

            NullableTest2(f(5));

            var f2 = new Func<int, int?>(i => i);

            Func<int, int> f1 = (int i) => 1; //Noncompliant
            Func<int, int> f3 = (i) => 1;
            var transformer = Funcify((string x) => new { Original = x, Normalized = x.ToLower() });
            var transformer2 = Funcify2((string x) => new { Original = x, Normalized = x.ToLower() }); // Noncompliant

            RefDelegateMethod((ref int i) => { });
        }

        public void M()
        {
            dynamic d = new object();
            Test(d, new BoolDelegate(() => true)); // Special case, d is dynamic
            Test2(null, new BoolDelegate(() => true)); // Compliant
        }

        public abstract void Test(object o, BoolDelegate f);
        public abstract void Test2(object o, Delegate f);
        public delegate bool BoolDelegate();

        private event EventHandler MyEvent;
        private event MyMethod MyEvent2;

        private delegate void MyMethod(int i, int j);
        private delegate void MyMethod2(ref int i);
        private void RefDelegateMethod(MyMethod2 f) { }

        public static Func<T, TResult> Funcify<T, TResult>(Func<T, TResult> f) { return f; }
        public static Func<string, TResult> Funcify2<TResult>(Func<string, TResult> f) { return f; }

        public static void NullableTest1<T>(T? x) where T : struct { }
        public static void NullableTest2(int? x) { }
    }
}
