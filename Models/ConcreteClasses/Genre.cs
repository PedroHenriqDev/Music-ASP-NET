﻿using Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ConcreteClasses
{
    public class Genre : IEntityWithName<Genre>
    {
        public string Id {  get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Date { get; set; }

        public Genre(string id, string name, string description, string date)
        {
            Id = id;
            Name = name;
            Description = description;
            Date = date;
        }
    }
}