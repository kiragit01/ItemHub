﻿using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ItemHub.Models.OnlyItem
{
    public class Item
    {
        [Key]
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string Creator { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public List<string> PathImages { get; set; }
        public int Price { get; set; }
        public uint Views { get; set; }
        public bool Published { get; set; }

        public Item(Guid id, string title, string? description, string creator, List<string> pathImages, int price, bool published)
        {
            Id = id;
            Title = title;
            Description = description;
            Creator = creator;
            CreatedDate = DateTime.UtcNow;
            PathImages = pathImages;
            Price = price;
            Published = published;
        }

        public void Update(string title, string? description, List<string> pathImages, int price, bool published)
        {
            Title = title;
            Description = description;
            UpdatedDate = DateTime.UtcNow;
            PathImages = pathImages;
            Price = price;
            Published = published;
        }
        public void UpdateDate() => UpdatedDate = DateTime.UtcNow;
    }
}
