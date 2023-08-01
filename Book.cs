using System;
using MongoDB.Bson.Serialization.Attributes;

namespace encrypt_test
{
	public class Book
	{
		[BsonId]
		public Guid Id { get; set; }
		public string Title { get; set; }
		public string Author { get; set; }
		public float Price { get; set; }

		public string Publisher {get;set;}

		public Book(string title, string author, float price, string publisher)
		{
			Id = Guid.NewGuid();

			Title = title;
			Author = author;
			Price = price;
            Publisher = publisher;
		}
	}
}

