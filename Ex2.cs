using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ex2
{
    public class Book
    {
        public string Title { get; set; } = "";
        public string Author { get; set; } = "";
        public int Year { get; set; }

        [JsonIgnore]
        public decimal Price { get; set; }
    }

    public static class Demo
    {
        public static void Run()
        {
            string path = "books.json";

            List<Book> books =
            [
                new Book { Title = "Vepkhistkaosani", Author = "Shota Rustaveli", Year = 1200, Price = 25.50m },
                new Book { Title = "Data Tutashkhia", Author = "Chabua Amirejibi", Year = 1975, Price = 30.00m },
                new Book { Title = "Clean Code", Author = "Robert C. Martin", Year = 2008, Price = 45.99m }
            ];

            // ჩაწერა: File.WriteAllText ფაილს თავად ქმნის, თუ ის არ არსებობს
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(books, options);
            File.WriteAllText(path, json);

            Console.WriteLine("books.json content:");
            Console.WriteLine(json);
            Console.WriteLine();

            // წაკითხვა
            string readJson = File.ReadAllText(path);
            List<Book> loaded = JsonSerializer.Deserialize<List<Book>>(readJson) ?? [];

            foreach (Book b in loaded)
            {
                // Price წაკითხვის შემდეგ ყველა წიგნისთვის 0-ია.
                // მიზეზი: [JsonIgnore]-ის გამო Price ფაილში საერთოდ არ ჩაწერილა,
                // ამიტომ Deserialize-ს მისი მნიშვნელობა არსაიდან აქვს და
                // property რჩება decimal-ის ნაგულისხმევ მნიშვნელობაზე, ანუ 0-ზე.
                Console.WriteLine($"{b.Title} - {b.Author} ({b.Year}), Price: {b.Price}");
            }
        }
    }
}