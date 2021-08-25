using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace PizzaApp.Model
{
    class PizzaCell: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public Pizza pizza { set; get; }
        public bool isfavorite { set; get; }
        public string ImageSourceFav { get { return isfavorite ? "star2.png" : "star1.png"; } }
        public ICommand FavClickCommand { set; get; }
        public Action<PizzaCell> FavChangeAction { set; get; }

        public PizzaCell()
        {
            FavClickCommand = new Command((obj) =>
            {
                isfavorite = !isfavorite;
                Console.WriteLine("FavClickCommand");
                OnPropertyChanged("ImageSourceFav"); //on remet à jour notre source image.
                FavChangeAction.Invoke(this);
            });
        }
        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
