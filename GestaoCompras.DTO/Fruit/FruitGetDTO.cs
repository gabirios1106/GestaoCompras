using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestaoCompras.DTO.Fruit
{
    public class FruitGetDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public double Price { get; set; }

        public FruitGetDTO() { }

        public FruitGetDTO(string name) => Name = name;

        public FruitGetDTO(int id, string name, double price) : this(name)
        {
            Id = id;
            Price = price;
        }
    }
}
