namespace ZolikKonzole
{
    public class Karta
    {
        public string Barva { get; set; } = "";    // Srdce, Káry, Trefy, Pik, Žolík
        public string Hodnota { get; set; } = "";  // 2-10, J, Q, K, A, Žolík
        public int Sila { get; set; }              // Pro řazení (2=2, ..., A=14, Žolík=100)

        public int BodovaHodnota
        {
            get
            {
                if (Hodnota == "Žolík") return 11; // Žolík za 11 bodů
                if (Hodnota == "A") return 11;     // Eso za 11 bodů (pro zjednodušení výpočtu 51 b.)
                if (Hodnota == "J" || Hodnota == "Q" || Hodnota == "K" || Hodnota == "10") return 10;
                
                return int.Parse(Hodnota); // Karty 2-9 mají svou nominální hodnotu
            }
        }

        public override string ToString()
        {
            // ANSI barvy: Červená (\u001b[31m), Šedá (\u001b[90m), Žlutá (\u001b[33m), Reset (\u001b[0m)
            if (Hodnota == "Žolík") 
                return "\u001b[33m[🃏 ŽOLÍK]\u001b[0m";
            
            string symbol = Barva switch 
            { 
                "Srdce" => "♥️", 
                "Káry" => "♦️", 
                "Trefy" => "♣️", 
                "Pik" => "♠️", 
                _ => "" 
            };

            string barevnyKod = (Barva == "Srdce" || Barva == "Káry") ? "\u001b[31m" : "\u001b[90m";

            return $"{barevnyKod}{symbol} {Hodnota}\u001b[0m";
        }
    }
}
