using ZolikKonzole;

Console.OutputEncoding = System.Text.Encoding.UTF8;

HerniLogika hra = new();
hra.InicializujHru();
bool hracNaRade = true;

while (!hra.KonecHry)
{
    if (hracNaRade)
    {
        hra.TahHrace();
    }
    else
    {
        hra.TahPocitace();
    }hracNaRade = !hracNaRade;
}

Console.Clear();
Console.WriteLine("=== KONEC HRY ===");

if (hra.Vitez == "Hrac")
{
    Console.WriteLine("\n🎉 GRATULUJI! Vyhrál jsi hru Žolík!");
}
else if (hra.Vitez == "Pocitac")
{
    Console.WriteLine("\n💻 Počítač vyhrál. Zkus to znovu!");
}
else
{
    Console.WriteLine("\nDošly karty v balíčku. Hra končí remízou!");
}

Console.WriteLine("\nStiskněte libovolnou klávesu pro ukončení programu...");
Console.ReadKey();