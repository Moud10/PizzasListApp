using Newtonsoft.Json;
using PizzaApp.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace PizzaApp
{
    public partial class MainPage : ContentPage
    {
        public enum e_tri
        {
            TRI_AUCUN,
            TRI_NOM,
            TRI_PRIX,
            Tri_FAV
        };
        List<Pizza> pizzas;
        List<String> pizzasFav= new List<String>();
        e_tri tri = e_tri.TRI_AUCUN;
        const string KEY_TRI = "tri";
        const string KEY_FAV = "fav";
        string jsonFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"pizzas.json");
        string tempFileName= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "temp");
        public MainPage()
        {
            InitializeComponent();

            if (Application.Current.Properties.ContainsKey(KEY_TRI))
            {
                tri = (e_tri)Application.Current.Properties[KEY_TRI];
                imgButton.Source = GetImageSourceFromTri(tri);
            }
            if (Application.Current.Properties.ContainsKey(KEY_FAV))
            {
                loadFavList();
            }
                Console.WriteLine("Étape 1");
            maListeView.RefreshCommand = new Command(() =>
            {
                Console.WriteLine("Refresh");
                DownLoadData((pizzas) => {
                    if (pizzas != null)
                    {
                        maListeView.ItemsSource = GetPizzaCells(GetPizzasFromtri(tri, pizzas), pizzasFav);
                    }
                    maListeView.IsRefreshing = false;
                });
            });
            if (File.Exists(jsonFileName))
            {
                string pizzaJson = File.ReadAllText(jsonFileName);
                if (!string.IsNullOrEmpty(pizzaJson)) 
                {
                    pizzas = JsonConvert.DeserializeObject<List<Pizza>>(pizzaJson);
                    maListeView.ItemsSource = GetPizzaCells(GetPizzasFromtri(tri, pizzas), pizzasFav);
                    maListeView.IsVisible = true;
                    indicator.IsVisible = false; 
                } 
            }
            DownLoadData((pizzas)=>{
                if (pizzas != null)
                {
                    pizzas = GetPizzasFromtri(tri, pizzas);
                    maListeView.ItemsSource = GetPizzaCells(GetPizzasFromtri(tri, pizzas), pizzasFav);
                }
                indicator.IsVisible = false;
            });
        }
        public string GetImageSourceFromTri(e_tri t)
        {
            switch (t)
            {
                case e_tri.TRI_NOM:
                    return "sort_nom.png";
                case e_tri.TRI_PRIX:
                    return "sort_prix.png";
                case e_tri.Tri_FAV:
                    return "sort_fav.png";
            }
            return "sort_none.png";
        }
        private void DownLoadData(Action <List<Pizza>> action)
        {
            string urlPizzaJson = "https://drive.google.com/uc?export=download&id=1JkrK8LCPCDfnpZ60GxGPeRb5TTGsyqDD";
            using (var webClient = new WebClient())
            {
                Console.WriteLine("Étape 2");
                webClient.DownloadFileCompleted += (object sender, AsyncCompletedEventArgs e) => {
                    Console.WriteLine("étape 5");
                    Exception ex = e.Error;
                    if (ex==null)
                    {
                        File.Copy(tempFileName,jsonFileName,true);
                        string pizzaJson = File.ReadAllText(jsonFileName);
                        pizzas = JsonConvert.DeserializeObject<List<Pizza>>(pizzaJson);
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            Console.WriteLine("test");
                            action.Invoke(pizzas);
                        });
                    }
                    else
                    {
                        Device.BeginInvokeOnMainThread(async() =>
                        {
                            await DisplayAlert("Erreur", "une erreur réseau s'est produite: " + ex.Message, "OK");
                            action.Invoke(null);
                        });
                    }
                };
                Console.WriteLine("Étape 3");
                webClient.DownloadFileAsync(new Uri(urlPizzaJson), tempFileName);
            }
        }
       
        private void imgButton_Clicked(object sender, EventArgs e)
        {
            if (tri == e_tri.TRI_AUCUN)
            {
                tri = e_tri.TRI_NOM;
            }
            else if (tri == e_tri.TRI_NOM)
            {
                tri = e_tri.TRI_PRIX;
            }
            else if (tri == e_tri.TRI_PRIX)
            {
                tri = e_tri.Tri_FAV;
            }
            else if(tri== e_tri.Tri_FAV)
            {
                tri = e_tri.TRI_AUCUN;
            }

            imgButton.Source = GetImageSourceFromTri(tri);
            maListeView.ItemsSource = GetPizzaCells(GetPizzasFromtri(tri, pizzas), pizzasFav);

            Application.Current.Properties[KEY_TRI] = (int)tri;
            Application.Current.SavePropertiesAsync();
        }
        private List<Pizza> GetPizzasFromtri(e_tri t,List<Pizza>l)
        {
            if (l == null)
            {
                return null;
            }
            if (t == e_tri.TRI_AUCUN)
            {
                return l;
            }
            else if(t==e_tri.TRI_NOM){
                l = l.OrderBy(x => x.titre).ToList();
            }
            else if (t== e_tri.Tri_FAV)
            {
                l = l.OrderBy(x => x.titre).ToList();
            }
            else
            {
                l = l.OrderByDescending(x => x.prix).ToList();
            }
            return l;
        }
        private void onFavPizzaChanged(PizzaCell pizzaCell)
        {
            bool isInFavList = pizzasFav.Contains(pizzaCell.pizza.nom);

            if (pizzaCell.isfavorite && !isInFavList)
            {
                pizzasFav.Add(pizzaCell.pizza.nom);
            }
            else if (!pizzaCell.isfavorite && isInFavList)
            {
                pizzasFav.Remove(pizzaCell.pizza.nom);
            }
            SaveFavList();
        }
        private List<PizzaCell> GetPizzaCells(List<Pizza>p, List<String>f)
        {
            List<PizzaCell> ret = new List<PizzaCell>();

            if (p == null)
            {
                return ret;
            }
            
            foreach (Pizza pizza in p)
            {
                bool isFav = f.Contains(pizza.nom);
                if (tri != e_tri.Tri_FAV)
                {
                    ret.Add(new PizzaCell { pizza = pizza, isfavorite = isFav, FavChangeAction = onFavPizzaChanged });
                }
                else if(isFav)
                {
                    ret.Add(new PizzaCell { pizza = pizza, isfavorite = isFav, FavChangeAction = onFavPizzaChanged });
                }
            }
            return ret;
        }
        private void SaveFavList()
        {
            var jsonValueToSave = JsonConvert.SerializeObject(pizzasFav);
            Application.Current.Properties[KEY_FAV] = jsonValueToSave;
            Application.Current.SavePropertiesAsync();
        }
        private void loadFavList()
        {
            var pfav = JsonConvert.DeserializeObject<List<String>>((string)Application.Current.Properties[KEY_FAV]);
            if (pfav != null)
            {
                pizzasFav = pfav;
            }
        }
    }
}
