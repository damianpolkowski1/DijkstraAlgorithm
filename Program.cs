using System;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace projekt
{
    class Program
    {
        static readonly Stopwatch timer = new Stopwatch();

        static void Main(string[] args)
        {
            int n = 0; //liczba hipermiast
            int m = 0; //liczba korytarzy
            int k = 0; //maksymalna liczba tuneli, ktorych mozna uzyć
            List<List<List<int>>> krawedzie = new List<List<List<int>>>(); //tablica list incydencji

            string path;
            while (true)
            {
                Console.WriteLine("Podaj nazwe pliku wejsciowego: ");
                path = AppDomain.CurrentDomain.BaseDirectory + Console.ReadLine() + ".txt";

                if (File.Exists(path))
                {
                    break;
                }
                else
                {
                    Console.WriteLine("Plik nie istnieje\n");
                    continue;
                }
            }

            using (StreamReader file = new StreamReader(path))
            {
                timer.Start();

                string ln = file.ReadLine();
                String[] pierwsza = ln.Split(" ", StringSplitOptions.RemoveEmptyEntries);

                n = Convert.ToInt32(pierwsza[0]);
                m = Convert.ToInt32(pierwsza[1]);
                k = Convert.ToInt32(pierwsza[2]);

                for (int i = 0; i < n + 1; i++)
                {
                    List<List<int>> wierzcholek = new List<List<int>>();

                    krawedzie.Add(wierzcholek);
                }

                for (int i = 0; i < m; i++)
                {
                    if ((ln = file.ReadLine()) != null)
                    {
                        String[] linia = ln.Split(" ", StringSplitOptions.RemoveEmptyEntries);

                        int w1 = Convert.ToInt32(linia[0]);
                        int w2 = Convert.ToInt32(linia[1]);
                        int waga = Convert.ToInt32(linia[2]);

                        krawedzie[w1].Add(new List<int>(2) { w2, waga });
                        krawedzie[w2].Add(new List<int>(2) { w1, waga });
                    }
                }

            }

            Wierzcholek[][] kopiec = new Wierzcholek[n + 1][];

            for (int i = 0; i < kopiec.Length; i++)
            {
                kopiec[i] = new Wierzcholek[k + 1];
            }

            for (int i = 1; i < n + 1; i++)
            {
                for (int j = 0; j < k + 1; j++)
                {
                    kopiec[i][j] = new Wierzcholek(i);
                }
            }

            int[][][] T = new int[n + 1][][]; //tablica przechowująca dist, pred i t

            for (int i = 1; i < T.Length; i++)
            {
                T[i] = new int[k + 1][];

                for (int j = 0; j < T[i].Length; j++)
                {
                    T[i][j] = new int[3] { -1, -1, -1 };
                }
            }

            for (int i = 0; i < k + 1; i++)
            {
                kopiec[1][i].d = 0; //zaczynamy z hipermiasta 1, wiec odleglosc do 1 to zawsze 0
            }

            while (true) //Algorytm Dijkstry
            {
                for (int i = 0; i < k + 1; i++)
                {
                    T[kopiec[1][0].v][i][0] = kopiec[1][i].d;
                    T[kopiec[1][0].v][i][1] = kopiec[1][i].pred;
                    T[kopiec[1][0].v][i][2] = kopiec[1][i].t;
                }

                Wierzcholek[] s = kopiec[1]; //wierzcholek z ktorego przeprowadzamy relaksacje

                kopiec[1] = kopiec[kopiec.Length - 1]; //Zastąpienie elementu ze szczytu kopca ostatnim elementem
                Array.Resize(ref kopiec, kopiec.Length - 1); //Skrócenie kopca o 1
                if (kopiec.Length == 1) break; //Kopiec jest pusty, więc algorytm zostaje przerwany

                KopcujDol(kopiec, 1);

                for (int i = 0; i < krawedzie[s[0].v].Count(); i++)
                {
                    int w = 1; //Element według którego później rozpocznie się przywracanie własności kopca

                    for (int j = 1; j < kopiec.Length; j++)
                    {
                        if (kopiec[j][0].v == krawedzie[s[0].v][i][0])
                        {
                            for (int z = 0; z < k + 1; z++)
                            {
                                if (s[0].d != Int32.MaxValue && z == 0 && krawedzie[s[0].v][i][1] != 0)
                                {
                                    int temp = kopiec[j][0].d; //odległość do wierzchołka przed relaksacją

                                    kopiec[j][0].d = Math.Min(kopiec[j][0].d, s[0].d + krawedzie[s[0].v][i][1]);

                                    if (temp > kopiec[j][0].d)
                                    {
                                        kopiec[j][0].pred = s[0].v;
                                        w = j;
                                    }
                                }

                                if (z != 0 && s[z - 1].d != Int32.MaxValue && krawedzie[s[0].v][i][1] == 0)
                                {
                                    int temp = kopiec[j][z].d; //odległość do wierzchołka przed relaksacją

                                    kopiec[j][z].d = Math.Min(kopiec[j][z].d, s[z - 1].d + krawedzie[s[0].v][i][1]);

                                    if (temp > kopiec[j][z].d)
                                    {
                                        kopiec[j][z].pred = s[z].v;
                                        kopiec[j][z].t = s[z - 1].t + 1;
                                        w = j;
                                    }
                                }

                                if (s[z].d != Int32.MaxValue && z != 0 && krawedzie[s[0].v][i][1] != 0)
                                {
                                    int temp = kopiec[j][z].d; //odległość do wierzchołka przed relaksacją

                                    kopiec[j][z].d = Math.Min(kopiec[j][z].d, s[z].d + krawedzie[s[0].v][i][1]);

                                    if (temp > kopiec[j][z].d)
                                    {
                                        kopiec[j][z].pred = s[z].v;
                                        kopiec[j][z].t = s[z].t;
                                        w = j;
                                    }
                                }
                            }
                        }
                    }

                    Kopcuj(kopiec, w);
                }
            }

            int min = Int32.MaxValue;
            int indeks_optymalnego_rozwiazania = 0;

            for (int i = 0; i < T[n].Length; i++)
            {
                if (T[n][i][0] != -1 && T[n][i][0] < min)
                {
                    min = T[n][i][0];
                    indeks_optymalnego_rozwiazania = i;
                }
            }

            Console.WriteLine("\nWykorzystano " + T[n][indeks_optymalnego_rozwiazania][2] + " tuneli czasoprzestrzennych, można było " + k);
            Console.WriteLine("\nDo hipermiasta nr " + n + " można dostać się w czasie: " + min + " h");
            List<int> sciezka = new List<int>{n};

            while (sciezka[sciezka.Count - 1] != 1)
            {
                sciezka.Add(T[sciezka[sciezka.Count - 1]][indeks_optymalnego_rozwiazania][1]);

                int krawedz_z = Math.Min(sciezka[sciezka.Count - 1], sciezka[sciezka.Count - 2]);
                int krawedz_do = Math.Max(sciezka[sciezka.Count - 1], sciezka[sciezka.Count - 2]);

                for(int i = 0; i < krawedzie[krawedz_z].Count; i++)
                {
                    if (krawedzie[krawedz_z][i][0] == krawedz_do)
                    {
                        if (krawedzie[krawedz_z][i][1] == 0)
                        {
                            indeks_optymalnego_rozwiazania--;
                        }
                    }
                }
            }

            Console.Write("Optymalna droga to: ");
            for (int i = sciezka.Count - 1; i >= 0; i--)
            {
                Console.Write(sciezka[i]);
                if (i != 0) Console.Write(" --> ");
            }

            timer.Stop();
            Console.WriteLine("\n\nWykonano w czasie: " + timer.Elapsed.ToString());
        }

        static void Kopcuj(Wierzcholek[][] kopiec, int w) //przywraca własność kopca
        {
            int ojciec = w / 2;
            int indeks_kopcowania_ojciec = 0;
            int indeks_kopcowania_w = 0;

            if (ojciec > 0)
            {
                for (int i = 1; i < kopiec[1].Length - 1; i++)
                {
                    if (kopiec[ojciec][i].d != Int32.MaxValue && kopiec[ojciec][i].d > kopiec[ojciec][i - 1].d)
                    {
                        indeks_kopcowania_ojciec = i;
                    }
                    else if (kopiec[ojciec][i].d != Int32.MaxValue)
                    {
                        indeks_kopcowania_ojciec = i;
                    }
                }
            }

            for (int i = 1; i < kopiec[1].Length - 1; i++)
            {
                if (kopiec[w][i].d != Int32.MaxValue && kopiec[w][i].d > kopiec[w][i - 1].d)
                {
                    indeks_kopcowania_w = i;
                }
                else if(kopiec[w][i].d != Int32.MaxValue)
                {
                    indeks_kopcowania_w = i;
                }
            }

            Wierzcholek[] pom;

            if (ojciec > 0 && kopiec[ojciec][indeks_kopcowania_ojciec].d > kopiec[w][indeks_kopcowania_w].d)
            {
                pom = kopiec[w];
                kopiec[w] = kopiec[ojciec];
                kopiec[ojciec] = pom;
                Kopcuj(kopiec, ojciec);
            }
        }

        static void KopcujDol(Wierzcholek[][] kopiec, int w) //przywraca własność kopca
        {
            int k = Min(kopiec, w); //wyznacz index min elementu sposrod w i jego dzieci

            Wierzcholek[] pom;

            if (k != w)
            {
                pom = kopiec[w];
                kopiec[w] = kopiec[k];
                kopiec[k] = pom;
                KopcujDol(kopiec, k);
            }
        }

        static int Min(Wierzcholek[][] kopiec, int w) //zwraca index min elementu sposrod w i jego dzieci
        {
            int indeks_kopcowania_w = 0;
            int indeks_kopcowania_2w = 0;
            int indeks_kopcowania_2w1 = 0;

            for (int i = 1; i < kopiec[1].Length - 1; i++)
            {
                if (kopiec[w][i].d != Int32.MaxValue && kopiec[w][i].d < kopiec[w][i - 1].d)
                {
                    indeks_kopcowania_w = i;
                }
            }

            if(kopiec.Length > 2 * w)
            {
                for (int i = 1; i < kopiec[1].Length - 1; i++)
                {
                    if (kopiec[2*w][i].d != Int32.MaxValue && kopiec[2*w][i].d < kopiec[2*w][i - 1].d)
                    {
                        indeks_kopcowania_2w = i;
                    }
                }
            }

            if (kopiec.Length > 2 * w + 1)
            {
                for (int i = 1; i < kopiec[1].Length - 1; i++)
                {
                    if (kopiec[2 * w + 1][i].d != Int32.MaxValue && kopiec[2 * w + 1][i].d < kopiec[2 * w + 1][i - 1].d)
                    {
                        indeks_kopcowania_2w1 = i;
                    }
                }
            }


            int minimalne_d = kopiec[w][indeks_kopcowania_w].d;

            if (kopiec.Length > 2 * w + 1)
            {
                minimalne_d = Math.Min(kopiec[w][indeks_kopcowania_w].d, Math.Min(kopiec[2 * w][indeks_kopcowania_2w].d, kopiec[2 * w + 1][indeks_kopcowania_2w1].d));
            }
            else if (kopiec.Length > 2 * w)
            {
                minimalne_d = Math.Min(kopiec[w][indeks_kopcowania_w].d, kopiec[2 * w][indeks_kopcowania_2w].d);
            }

            if (minimalne_d == kopiec[w][indeks_kopcowania_w].d) return w;

            if (kopiec[2 * w][indeks_kopcowania_2w].d == minimalne_d) return (2 * w);
            else if (kopiec[2 * w + 1][indeks_kopcowania_2w1].d == minimalne_d) return (2 * w + 1);

            return w;
        }
    }

    class Wierzcholek
    {
        public int v;
        public int d;
        public int pred;
        public int t; //Liczba wykorzystanych dotąd tuneli

        public Wierzcholek(int v)
        {
            this.v = v;
            this.d = Int32.MaxValue; //nieskonczonosc (powiedzmy)
            this.pred = -1;
            this.t = 0;
        }
    }
}