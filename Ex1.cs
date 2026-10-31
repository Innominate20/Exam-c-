using System;
using System.Collections.Generic;

namespace Ex1
{
    public interface IDigital
    {
        string Url { get; }
    }

    public interface IBorrowable
    {
        bool IsBorrowed { get; }
        bool Borrow();
        void Return();
    }

    public abstract class LibraryItem
    {
        public string Title { get; }

        protected LibraryItem(string title)
        {
            Title = title;
        }

        public virtual string Info()
        {
            return Title;
        }
    }

    public abstract class PhysicalItem : LibraryItem, IBorrowable
    {
        public bool IsBorrowed { get; private set; }

        public abstract int LoanDays { get; }

        protected PhysicalItem(string title) : base(title)
        {
        }

        public bool Borrow()
        {
            if (IsBorrowed)
                return false;

            IsBorrowed = true;
            return true;
        }

        public void Return()
        {
            IsBorrowed = false;
        }

        public override string Info()
        {
            string result = $"{Title}: {LoanDays} days";
            if (IsBorrowed)
                result += " [borrowed]";
            return result;
        }
    }

    public class Book : PhysicalItem
    {
        public override int LoanDays => 21;

        public Book(string title) : base(title)
        {
        }
    }

    public class Dvd : PhysicalItem
    {
        public override int LoanDays => 7;

        public Dvd(string title) : base(title)
        {
        }

        public override string Info()
        {
            return base.Info() + " (DVD)";
        }
    }

    public class EBook : LibraryItem, IDigital
    {
        public string Url { get; }

        public EBook(string title, string url) : base(title)
        {
            Url = url;
        }

        public override string Info()
        {
            return base.Info() + ": online only";
        }
    }

    public static class Demo
    {
        public static void Run()
        {
            List<LibraryItem> items =
            [
                new Book("Clean Code"),
                new Dvd("Inception"),
                new EBook("C# Basics", "https://books.example.com/csharp")
            ];

            foreach (LibraryItem item in items)
                Console.WriteLine(item.Info());

            Console.WriteLine();
            foreach (LibraryItem item in new[] { items[0], items[0], items[2], items[1] })
            {
                if (item is IBorrowable b)
                    Console.WriteLine(b.Borrow()
                        ? $"{item.Title}: borrowed"
                        : $"{item.Title}: already borrowed");
                else
                    Console.WriteLine($"{item.Title}: cannot be borrowed");
            }

            Console.WriteLine();
            foreach (LibraryItem item in items)
                Console.WriteLine(item.Info());

            ((IBorrowable)items[0]).Return();
            Console.WriteLine($"After return: {items[0].Info()}");

            foreach (LibraryItem item in items)
                if (item is IDigital d)
                    Console.WriteLine($"Download: {d.Url}");
        }
    }
}