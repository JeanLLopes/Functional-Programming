using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FunctionalCSharp
{
    public static class Disposable
    {
        public static TResult Using<TDisposable, TResult>
        (
            Func<TDisposable> factory,
            Func<TDisposable, TResult> fn
        ) where TDisposable : IDisposable
        {
            using (var disposabled = factory())
            {
                return fn(disposabled);
            }
        }
    }

    public static class StringBuilderExtensions
    {
        public static StringBuilder AppendFormattedLine(this StringBuilder @this, string format, params object[] arguments) =>
                @this.AppendFormat(format, arguments).AppendLine();

        public static StringBuilder AppendLineWhen(this StringBuilder @this, Func<bool> predicate, Func<StringBuilder, StringBuilder> fn) =>
            predicate() ? fn(@this) : @this;

        public static StringBuilder AppendSequence<T>(this StringBuilder @this, IEnumerable<T> sequence, Func<StringBuilder, T, StringBuilder> fn) =>
            sequence.Aggregate(@this, fn);

        public static TResult Map<TSource, TResult>(this TSource @this, Func<TSource, TResult> fn) =>
            fn(@this);

        public static T Tee<T>(this T @this, Action<T> act)
        {
            act(@this);
            return @this;
        }
            
    }

    internal class Program
    {
        private static Func<IDictionary<int, string>, string> BuildSelectBox(string id, bool includeUnknown) =>
            options =>
                new StringBuilder()
                    .AppendFormattedLine("<select id=\"{0}\" name=\"{0}\">", id)
                    .AppendLineWhen(
                        () => includeUnknown,
                        sb => sb.AppendLine("\t<option>Unknown</option>"))
                    .AppendSequence(options, (sb, opt) =>
                        sb.AppendFormattedLine("\t<option value=\"{0}\">{1}</option>", opt.Key, opt.Value))
                    .AppendLine("</select>")
                    .ToString();

        private static void Main(string[] args)
        {
            var selectBox = Disposable
                    .Using(
                        StreamFactory.GetStream,
                        steam => new byte[steam.Length].Tee(b => steam.Read(b, 0, (int)steam.Length)))
                    .Map(Encoding.UTF8.GetString)
                    .Split(new[] { Environment.NewLine, }, StringSplitOptions.RemoveEmptyEntries)
                    .Select((s, ix) => Tuple.Create(ix, s))
                    .ToDictionary(k => k.Item1, v => v.Item2)
                    .Map(BuildSelectBox("theDoctors", true))
                    .Tee(Console.WriteLine);

            Console.ReadLine();
        }
    }
}