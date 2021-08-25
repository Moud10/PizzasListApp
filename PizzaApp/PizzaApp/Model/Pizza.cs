using PizzaApp.extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace PizzaApp.Model
{
    class Pizza
    {
        public string nom { set; get; }
        public string imageUrl { set; get; }
        public int prix { set; get; }
        public string[] ingredients { set; get; }
        public string PrixEuros { get { return prix + "€"; } }
        public string IngredientsStr { get { return string.Join(", ",ingredients); } }
        public string titre { get { return nom.PremiereLettreMajuscule(); } }

    }
}
