using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection.Emit;
using System.Runtime.Remoting.Messaging;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace LZWKompesija
{
    internal class Program
    {
        private static List<int> LZWKodiranje(string s,string putanja)
        {
            Dictionary <string,int> recnik = new Dictionary<string, int>();
            List<int> izlaz = new List<int>();
            int kod = 256;
            for (int i = 0; i < 256; i++)
            {
                recnik.Add(((char)i).ToString(),i);
            }
            
            string prvi = s[0].ToString();

            int index=1;
            while(index<s.Length)
            {
                string sledeci=s[index].ToString();
                if (recnik.ContainsKey(prvi+sledeci))
                {
                    prvi = prvi + sledeci;
                }
                else
                {
                    izlaz.Add(recnik[prvi]);
                    recnik.Add(prvi+sledeci,kod);
                    kod++;
                    prvi = sledeci;
                }
                index++;
            }
            using (StreamWriter writer = new StreamWriter(putanja))
            {
                foreach (var x in recnik)
                {
                    writer.WriteLine($"\"{x.Key}\" : {x.Value}");
                }
            }
            return izlaz;
        }
        private static string LZWDekodiranje(List<int> ulazniKod)
        {
            Dictionary<int,string> tablica=new Dictionary<int,string>();
            for (int i = 0; i < 256; i++)
            {
                tablica.Add(i, ((char)i).ToString());
            }
            string rez="";
            string prethodni;
            int kod = 256;

            int stariKod = ulazniKod[0];
            prethodni = tablica[stariKod];
            rez+= prethodni;
            int x = 1;
            while(x<ulazniKod.Count)
            {
                int noviKod = ulazniKod[x];
                string trenutni;

                if(tablica.ContainsKey(noviKod))
                {
                    trenutni = tablica[noviKod];
                }
                else
                {
                    trenutni = prethodni + prethodni[0];
                }
                rez+= trenutni;
                string prviKarakter = trenutni[0].ToString();
                tablica.Add(kod,prethodni+prviKarakter);
                kod++;

                prethodni = trenutni;
                x++;
            }
            return rez;
        }
        private static string GenerisiRandomString(int duzinaStringa)
        {
            Random random = new Random(); ;
            StringBuilder stringBuilder = new StringBuilder("", duzinaStringa);
            int randomVrednost;
            char karakter;
            for (int i = 0; i < duzinaStringa; i++)
            {
                randomVrednost = random.Next(32, 126);
                karakter = (char)randomVrednost;
                stringBuilder.Append(karakter);
            }
            return stringBuilder.ToString();
        }
        private static string GenerisiLoremIpsum(int duzinaStringa)
        {
            string loremIpsum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nulla dictum arcu ipsum, nec placerat quam.";//, consequat luctus leo. Ae acnean vel lorem eget elit rhoncus rhoncus a vitae lectus. Donec et mattis sapien, id vulputate augue. Proin ultrices semper quam a viverra. Fusce dignissim, leo sed tempor porta, massa ante bibendum massa, nec commodo sapien urna id metus. Aliquam at tristique turpis. In hac habitasse platea dictumst. Ut eget bibendum nisl. Mauris enim lorem, maximus ac nulla at, dignissim bibendum magna. Nam finibus finibus tortor ut commodo. Proin consectetur erat vitae lacus ornare laoreet. Suspendisse quis eros rutrum, tincidunt felis in. ";
            StringBuilder rezultat = new StringBuilder(duzinaStringa);
            while (rezultat.Length < duzinaStringa)
            {
                rezultat.Append(loremIpsum);
            }
            return rezultat.ToString(0, duzinaStringa);
        }
        private static void GenerisanjeFajlova()
        {
            int[] velicina = { 100, 1000, 10000, 100000, 1000000, 10000000 };

            foreach (int n in velicina)
            {
                string randomString = GenerisiRandomString(n);
                string loremIpsumString = GenerisiLoremIpsum(n);

                string randomImeFajla = $"RandomString_{n}.txt";
                string loremImeFajla = $"LoremIpsum_{n}.txt";
                File.WriteAllText(randomImeFajla, randomString);
                File.WriteAllText(loremImeFajla, loremIpsumString);
            }
        }
        private static void UpisiKodUFajl(string putanja, List<int> kodiranoRandom)
        {
            using (StreamWriter writer = new StreamWriter(putanja))
            {
                writer.WriteLine(string.Join(" ",kodiranoRandom));
            }
        }
        private static void StepenKompresije(string putanja, string naziv, long originalnaVelicina, long kompresovanaVelicina)
        {
            double stepenKompresije = 100 * (1 - ((double)kompresovanaVelicina / originalnaVelicina));
            using (StreamWriter writer = new StreamWriter(putanja, true))
            {
                writer.WriteLine($"{naziv}:");
                writer.WriteLine($"Originalna veličina: {originalnaVelicina} bajtova");
                writer.WriteLine($"Kompresovana veličina (bez rečnika): {kompresovanaVelicina} bajtova");
                writer.WriteLine($"Stepen kompresije: {stepenKompresije:F2}%");//Zaokruzuje broj na 2 decimale
                writer.WriteLine();
            }
        }
        static void Main(string[] args)
        {
            GenerisanjeFajlova();
            int[] velicina = { 100, 1000, 10000, 100000, 1000000, 10000000};
            foreach (int v in velicina)
            {
                string imeFajla1 = $"RandomString_{v}.txt";
                string imeFajla2 = $"LoremIpsum_{v}.txt";

                List<int> kodiranoRandom = new List<int>();
                List<int> kodiranoLorem = new List<int>();

                string rnd = File.ReadAllText(imeFajla1);
                string lorem = File.ReadAllText(imeFajla2);

                string recnikRnd = $"recnikRndZa{v}Elemenata.txt";
                string recnikLorem = $"recnikLoremZa{v}Elemenata.txt";

                kodiranoRandom = LZWKodiranje(rnd,recnikRnd);
                kodiranoLorem = LZWKodiranje(lorem,recnikLorem);

                string izlazniKodZaRandom = $"izlazniKodZa_{v}_RandomElemenata.txt";
                string izlazniKodZaLorem = $"izlazniKodZa_{v}_LoremElemenata.txt";

                UpisiKodUFajl(izlazniKodZaRandom, kodiranoRandom);
                UpisiKodUFajl(izlazniKodZaLorem, kodiranoLorem);

                //DEKODIRANJE RANDOM ELEMENATA
                string rndDekod = File.ReadAllText(izlazniKodZaRandom);
                List<int> kodRnd = new List<int>();
                string[] deloviRnd = rndDekod.Split(' ');
                foreach (string deo in deloviRnd)
                {
                    if (int.TryParse(deo, out int broj)) // Provera da li je broj
                    {
                        kodRnd.Add(broj);
                    }
                }
                string randomImeDekodiranogFajla = $"RandomStringDekodirano_{v}.txt";
                File.WriteAllText(randomImeDekodiranogFajla, LZWDekodiranje(kodRnd));

                //DEKODIRANJE LOREMA
                string loremDekod = File.ReadAllText(izlazniKodZaLorem);
                List<int> kodLorem = new List<int>();
                string[] deloviLorem = loremDekod.Split(' ');
                foreach (string deo in deloviLorem)
                {
                    if (int.TryParse(deo, out int broj)) // Provera da li je broj
                    {
                        kodLorem.Add(broj);
                    }
                }
                string loremImeDekodiranogFajla = $"LoremIpsumDekodirano_{v}.txt";
                File.WriteAllText(loremImeDekodiranogFajla, LZWDekodiranje(kodLorem));

                //STEPEN KOMPRESIJE
                long originalRandom = new FileInfo(imeFajla1).Length;
                long kompresovaniRandom = new FileInfo(izlazniKodZaRandom).Length;// + new FileInfo(recnikRnd).Length;

                long originalLorem = new FileInfo(imeFajla2).Length;
                long kompresovaniLorem = new FileInfo(izlazniKodZaLorem).Length;// + new FileInfo(recnikLorem).Length;

                StepenKompresije("Stepeni_Konverzije.txt", $"Random String ({v} elemenata)", originalRandom, kompresovaniRandom);
                StepenKompresije("Stepeni_Konverzije.txt", $"Lorem Ipsum ({v} elemenata)", originalLorem, kompresovaniLorem);
            }
        }
    }
}

