namespace ZolikKonzole
{                 
    internal class HerniLogika
    {
        private List<Karta> balicek = [];
        private readonly List<Karta> odhazovaciBalicek = [];
        private List<Karta> rukaHrac = [];
        private readonly List<Karta> rukaPocitac = [];
        private readonly List<List<Karta>> stul = [];
        private readonly Random random = new();

        private bool jeVylozenyHrac = false;
        private bool jeVylozenyPocitac = false;

        public bool KonecHry { get; private set; } = false;
        public string Vitez { get; private set; } = "";

        public void InicializujHru()
        {
            string[] barvy = ["Srdce", "Káry", "Trefy", "Pik"];
            string[] hodnoty = ["2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A"];

            // 104 karet + 4 žolíci (dvojitý balíček)
            for (int balicekCislo = 0; balicekCislo < 2; balicekCislo++)
            {
                for (int b = 0; b < barvy.Length; b++) 
                {
                    for (int h = 0; h < hodnoty.Length; h++) 
                    {
                        balicek.Add(new Karta { Barva = barvy[b], Hodnota = hodnoty[h], Sila = h + 2 });
                    }
                }
                balicek.Add(new Karta { Barva = "Žolík", Hodnota = "Žolík", Sila = 100 });
                balicek.Add(new Karta { Barva = "Žolík", Hodnota = "Žolík", Sila = 100 });
            }

            balicek = [.. balicek.OrderBy(x => random.Next())];

            for (int i = 0; i < 14; i++) 
            {
                rukaHrac.Add(LizniKartu()!);
                rukaPocitac.Add(LizniKartu()!);
            }
            rukaHrac.Add(LizniKartu()!);
            odhazovaciBalicek.Add(LizniKartu()!);
        }

        private Karta? LizniKartu()
        {
            if (balicek.Count == 0)
            {
                if (odhazovaciBalicek.Count > 1)
                {
                    Karta vrchni = odhazovaciBalicek.Last();
                    odhazovaciBalicek.RemoveAt(odhazovaciBalicek.Count - 1);
                    balicek.AddRange(odhazovaciBalicek);
                    balicek = [.. balicek.OrderBy(x => random.Next())];
                    odhazovaciBalicek.Clear();
                    odhazovaciBalicek.Add(vrchni);
                }
                else
                {
                    return null;
                }
            }
            Karta k = balicek[0];
            balicek.RemoveAt(0);
            return k;
        }

        public void TahHrace()
        {
            VypisStavHrace();
        
            // 1. FÁZE: LÍZÁNÍ KARTY (Hráč líže vždy, pokud na úplném startu hry nedostal 15 karet)
            // Pokud má hráč na startu svého tahu méně než 15 karet, musí si povinně líznout!
            if (rukaHrac.Count < 15)
            {
                Console.WriteLine($"\n[VRCHNÍ KARTA NA KOPĚ]: [{odhazovaciBalicek.Last()}]");
                Console.Write("Odkud chceš líznout? (1 - Balíček, 2 - Kopa): ");
                string volba = Console.ReadLine()!;
                
                if (volba == "2") 
                {
                    rukaHrac.Add(odhazovaciBalicek.Last());
                    odhazovaciBalicek.RemoveAt(odhazovaciBalicek.Count - 1);
                } 
                else 
                {
                    Karta liznuta = LizniKartu()!;
                    if (liznuta != null) rukaHrac.Add(liznuta);
                }
                
                // Po líznutí ruku seřadíme
                rukaHrac = [.. rukaHrac.OrderBy(k => k.Sila).ThenBy(k => k.Barva)];
            }
        
            // 2. FÁZE: MEZITAH (Vykládání / Přikládání - hráč může akci opakovat nebo přeskočit k odhozu)
            bool akceDokoncena = false;
            while (!akceDokoncena)
            {
                VypisStavHrace();
                Console.WriteLine("\nCo chceš udělat?");
                Console.WriteLine("1 - Vyložit NOVÉ sady na stůl");
                Console.WriteLine("2 - Přiložit kartu do existující sady na stole");
                Console.WriteLine("3 - Ukončit tah odhozením karty");
                Console.Write("Tvoje volba: ");
                string volbaMezitahu = Console.ReadLine()!;
        
                if (volbaMezitahu == "1")
                {
                    VylozitKarty();
                }
                else if (volbaMezitahu == "2")
                {
                    PrilozitKartu();
                }
                else if (volbaMezitahu == "3")
                {
                    akceDokoncena = true;
                }
            }
        
            // 3. FÁZE: ODHOZENÍ KARTY (Hráč vybere jednu kartu a odhodí ji, čímž definitivně předá tah)
            VypisStavHrace();
            int indexOdhozu = -1;
            while (indexOdhozu < 0 || indexOdhozu >= rukaHrac.Count)
            {
                Console.Write($"\nVyber index karty, kterou CHCEŠ ODHODIT (0 až {rukaHrac.Count - 1}): ");
                _ = int.TryParse(Console.ReadLine(), out indexOdhozu);
            }
        
            Karta odhozena = rukaHrac[indexOdhozu];
            rukaHrac.RemoveAt(indexOdhozu);
            odhazovaciBalicek.Add(odhozena);
            Console.WriteLine($"Odhodil jsi: [{odhozena}]\n--------------------------------------");
        
            ZkontrolujKonecHry();
        }

        private void VylozitKarty()
        {
            List<List<Karta>> sadyKValidaci = [];
            List<int> vsechnyVybraneIndexy = [];
            int celkoveBodyZaTentoTah = 0;

            Console.WriteLine("\n--- FÁZE VYKLÁDÁNÍ ---");
            if (!jeVylozenyHrac)
            {
                Console.WriteLine("⚠️ Ještě nejsi vyložený. Musíš vyložit sady v celkové hodnotě MINIMÁLNĚ 51 bodů.");
            }

            while (true)
            {
                Console.WriteLine($"\nAktuálně připravené body k vyložení: {celkoveBodyZaTentoTah} b.");
                Console.WriteLine("Zadej indexy karet pro JEDNU sadu oddělené čárkou (např. 2,3,4) nebo stiskni ENTER pro odeslání:");
                string vstup = Console.ReadLine()!;

                if (string.IsNullOrWhiteSpace(vstup)) 
                    break;

                try
                {
                    List<int> vybraneIndexySady = [.. vstup.Split(',').Select(s => int.Parse(s.Trim())).Distinct()];

                    if (vybraneIndexySady.Any(i => i < 0 || i >= rukaHrac.Count || vsechnyVybraneIndexy.Contains(i)))
                    {
                        Console.WriteLine("⚠️ Neplatné indexy nebo jsi tyto karty už vybral!");
                        continue;
                    }

                    List<Karta> docasnaSada = [];
                    
                    foreach (int index in vybraneIndexySady) 
                    {
                        docasnaSada.Add(rukaHrac[index]);
                    }

                    if (Validator.JePlatnaKombinace(docasnaSada))
                    {
                        int bodySady = docasnaSada.Sum(k => k.BodovaHodnota);
                        celkoveBodyZaTentoTah += bodySady;
                        sadyKValidaci.Add(docasnaSada);
                        vsechnyVybraneIndexy.AddRange(vybraneIndexySady);
                        
                        Console.WriteLine($"✅ Sada schválena! ({bodySady} b.)");
                    }
                    else
                    {
                        Console.WriteLine("❌ Neplatná kombinace!");
                    }
                }
                catch 
                { 
                    Console.WriteLine("⚠️ Chyba formátu!"); 
                }
            }

            if (sadyKValidaci.Count == 0) 
                return;

            if (!jeVylozenyHrac && celkoveBodyZaTentoTah < 51)
            {
                Console.WriteLine($"\n❌ Neúspěch! Celkem pouze {celkoveBodyZaTentoTah} b. z požadovaných 51 b.");
                _ = Console.ReadLine();
                return;
            }

            foreach (int index in vsechnyVybraneIndexy.OrderByDescending(i => i)) 
                rukaHrac.RemoveAt(index);
            
            foreach (var sada in sadyKValidaci) 
                stul.Add([.. sada.OrderBy(k => k.Sila)]);

            if (!jeVylozenyHrac)
            {
                jeVylozenyHrac = true;
                
                Console.WriteLine($"\n🎉 Úspěšně ses vyložil s hodnotou {celkoveBodyZaTentoTah} bodů!");
            }

            _ = Console.ReadLine();
        }

        private void PrilozitKartu()
        {
            if (!jeVylozenyHrac)
            {
                Console.WriteLine("\n❌ Nemůžeš přikládat karty, dokud nejsi sám vyložený (první vyložení za 51 b.)!");
                _ = Console.ReadLine();
                return;
            }
        
            if (stul.Count == 0)
            {
                Console.WriteLine("\n❌ Na stole nejsou žádné sady, ke kterým by se dalo přiložit!");
                _ = Console.ReadLine();
                return;
            }
        
            try
            {
                Console.Write($"\nZadej index karty z ruky, kterou chceš přiložit (0 až {rukaHrac.Count - 1}): ");
                if (!int.TryParse(Console.ReadLine(), out int indexKarty) || indexKarty < 0 || indexKarty >= rukaHrac.Count)
                {
                    Console.WriteLine("⚠️ Neplatný index karty!");
                    _ = Console.ReadLine();
                    return;
                }
        
                Console.Write($"Zadej číslo sady na stole (1 až {stul.Count}): ");
                if (!int.TryParse(Console.ReadLine(), out int cisloSady) || cisloSady < 1 || cisloSady > stul.Count)
                {
                    Console.WriteLine("⚠️ Neplatné číslo sady!");
                    _ = Console.ReadLine();
                    return;
                }
        
                int indexSady = cisloSady - 1;
                Karta vybranaKarta = rukaHrac[indexKarty];
                List<Karta> cilovaSada = stul[indexSady];
        
                Karta? zolikKNahrazeni = null;
        
                // OCHRANA: Žolíka z ruky nelze použít k nahrazení jiného Žolíka na stole
                if (vybranaKarta.Hodnota != "Žolík")
                {
                    bool jeSkupinaStejnych = cilovaSada.Any(k => k.Hodnota != "Žolík") && 
                                             cilovaSada.Where(k => k.Hodnota != "Žolík").All(k => k.Hodnota == cilovaSada.First(x => x.Hodnota != "Žolík").Hodnota);
        
                    if (jeSkupinaStejnych && vybranaKarta.Hodnota == cilovaSada.First(k => k.Hodnota != "Žolík").Hodnota)
                    {
                        zolikKNahrazeni = cilovaSada.FirstOrDefault(k => k.Hodnota == "Žolík");
                    }
                    else if (!jeSkupinaStejnych)
                    {
                        Karta? potencionalniZolik = cilovaSada.FirstOrDefault(k => k.Hodnota == "Žolík");
                        if (potencionalniZolik != null)
                        {
                            List<Karta> testSada = [.. cilovaSada];
                            _ = testSada.Remove(potencionalniZolik);
                            testSada.Add(vybranaKarta);
                            testSada = [.. testSada.OrderBy(k => k.Sila)];
        
                            if (Validator.JePlatnaKombinace(testSada))
                            {
                                zolikKNahrazeni = potencionalniZolik;
                            }
                        }
                    }
                }
        
                if (zolikKNahrazeni != null)
                {
                    rukaHrac.RemoveAt(indexKarty);
                    _ = cilovaSada.Remove(zolikKNahrazeni);
                    cilovaSada.Add(vybranaKarta);
                    stul[indexSady] = [.. cilovaSada.OrderBy(k => k.Sila)];
                    
                    rukaHrac.Add(zolikKNahrazeni); 
                    rukaHrac = [.. rukaHrac.OrderBy(k => k.Sila).ThenBy(k => k.Barva)];
        
                    Console.WriteLine($"✅ Karta [{vybranaKarta}] byla úspěšně přiložena a Žolík byl přesunut do tvé ruky!");
                }
                else
                {
                    List<Karta> testSadaRozsirena = [.. cilovaSada, vybranaKarta];
                    if (Validator.JePlatnePrilozeni(stul[indexSady], vybranaKarta) || Validator.JePlatnaKombinace(testSadaRozsirena))
                    {
                        rukaHrac.RemoveAt(indexKarty);
                        stul[indexSady].Add(vybranaKarta);
                        stul[indexSady] = [.. stul[indexSady].OrderBy(k => k.Sila)];
                        
                        Console.WriteLine($"✅ Karta [{vybranaKarta}] byla úspěšně přiložena k sadě {cisloSady}!");
                    }
                    else
                    {
                        Console.WriteLine($"❌ Kartu [{vybranaKarta}] nelze k této sadě přiložit (porušuje pravidla).");
                    }
                }
            }
            catch 
            {
                Console.WriteLine("⚠️ Nastala chyba při zpracování vstupu."); 
            }

            _ = Console.ReadLine();
        }


        public void TahPocitace()
        {            
            Console.Clear();           
            Console.WriteLine("====================== STŮL ======================");
            VypisStolyPouze();
            
            Console.WriteLine("\n🤖 NA TAHU JE POČÍTAČ...");
        
            Karta kartaNaZemi = odhazovaciBalicek.Last();
            if (jeVylozenyPocitac && stul.Any(sada => Validator.JePlatnePrilozeni(sada, kartaNaZemi)))
            {
                rukaPocitac.Add(kartaNaZemi);
                odhazovaciBalicek.RemoveAt(odhazovaciBalicek.Count - 1);
                Console.WriteLine("🤖 Počítač si líznul KARTU Z KOPY, protože ji umí okamžitě přiložit!");
            }
            else
            {
                Karta liznuta = LizniKartu()!;
                if (liznuta != null) rukaPocitac.Add(liznuta);
                Console.WriteLine("🤖 Počítač si líznul kartu z balíčku.");
            }
        
            if (!jeVylozenyPocitac)
            {
                List<List<Karta>> nalezeneSady = [];
                var skupinyPodleHodnot = rukaPocitac.GroupBy(k => k.Hodnota);
                foreach (var skupina in skupinyPodleHodnot)
                {
                    List<Karta> moznaSada = [.. skupina];
                    if (moznaSada.Count >= 3 && Validator.JePlatnaKombinace(moznaSada))
                    {
                        nalezeneSady.Add(moznaSada);
                    }
                }
        
                int bodyCelkem = 0;
                foreach (var sada in nalezeneSady)
                {
                    foreach (var karta in sada)
                    {
                        bodyCelkem += karta.BodovaHodnota;
                    }
                }
                
                if (bodyCelkem >= 51)
                {
                    foreach (var sada in nalezeneSady)
                    {
                        foreach (Karta k in sada)
                            _ = rukaPocitac.Remove(k);
                        stul.Add([.. sada.OrderBy(k => k.Sila)]);
                    }
                    jeVylozenyPocitac = true;
                    Console.WriteLine($"🤖 Počítač se úspěšně VYLOŽIL na stůl! (Získal {bodyCelkem} b.)");
                }
            }
            
            if (jeVylozenyPocitac)
            {
                bool probehloPrilozeni = true;
        
                while (probehloPrilozeni && rukaPocitac.Count > 1)
                {
                    probehloPrilozeni = false;
                    for (int i = 0; i < rukaPocitac.Count; i++)
                    {
                        Karta k = rukaPocitac[i];
                        
                        // OCHRANA PROTI ZACYKLENÍ: Počítač nesmí brát Žolíka ze stolu pomocí Žolíka z ruky
                        if (k.Hodnota == "Žolík") 
                            continue;
        
                        for (int s = 0; s < stul.Count; s++)
                        {
                            List<Karta> cilovaSada = stul[s];
                            Karta? zolikKNahrazeni = null;
        
                            bool jeSkupinaStejnych = cilovaSada.Any(x => x.Hodnota != "Žolík") && 
                                                     cilovaSada.Where(x => x.Hodnota != "Žolík").All(x => x.Hodnota == cilovaSada.First(y => y.Hodnota != "Žolík").Hodnota);
        
                            if (jeSkupinaStejnych && k.Hodnota == cilovaSada.First(x => x.Hodnota != "Žolík").Hodnota)
                            {
                                zolikKNahrazeni = cilovaSada.FirstOrDefault(x => x.Hodnota == "Žolík");
                            }
                            else if (!jeSkupinaStejnych)
                            {
                                Karta? potencionalniZolik = cilovaSada.FirstOrDefault(x => x.Hodnota == "Žolík");
                                if (potencionalniZolik != null)
                                {
                                    List<Karta> testSada = [.. cilovaSada];
                                    _ = testSada.Remove(potencionalniZolik);
                                    testSada.Add(k);
                                    testSada = [.. testSada.OrderBy(x => x.Sila)];
        
                                    if (Validator.JePlatnaKombinace(testSada))
                                    {
                                        zolikKNahrazeni = potencionalniZolik;
                                    }
                                }
                            }
        
                            if (zolikKNahrazeni != null)
                            {
                                _ = cilovaSada.Remove(zolikKNahrazeni);
                                cilovaSada.Add(k);
                                stul[s] = [.. cilovaSada.OrderBy(karta => karta.Sila)];
                                rukaPocitac.RemoveAt(i);
                                rukaPocitac.Add(zolikKNahrazeni);
        
                                Console.WriteLine($"🤖 Počítač nahradil a vzal Žolíka ze sady {s + 1} pomocí karty [{k}]!");
                                probehloPrilozeni = true;
                                break;
                            }
                            else
                            {
                                List<Karta> testSadaRozsirena = [.. cilovaSada, k];
                                if (Validator.JePlatnePrilozeni(stul[s], k) || Validator.JePlatnaKombinace(testSadaRozsirena))
                                {
                                    stul[s].Add(k);
                                    stul[s] = [.. stul[s].OrderBy(kart => kart.Sila)];
                                    rukaPocitac.RemoveAt(i);
                                    
                                    Console.WriteLine($"🤖 Počítač přiložil kartu [{k}] k sadě {s + 1}!");
                                    probehloPrilozeni = true;
                                    break;
                                }
                            }
                        }
                        if (probehloPrilozeni) 
                            break;
                    }
                }
            }
        
            int indexOdhozu = 0;
            var kandidatiNaOdhoz = rukaPocitac.Where(k => k.Hodnota != "Žolík")
                .OrderBy(k => k.Sila)
                .ToList();
        
            if (kandidatiNaOdhoz.Count > 0)
            {
                Karta kOdhozeni = kandidatiNaOdhoz.First();
                indexOdhozu = rukaPocitac.IndexOf(kOdhozeni);
            }
        
            Karta odhozena = rukaPocitac[indexOdhozu];
            rukaPocitac.RemoveAt(indexOdhozu);
            odhazovaciBalicek.Add(odhozena);
        
            Console.WriteLine($"🤖 Počítač odhodil na kopu kartu: [{odhozena}]");
            Console.WriteLine($"🤖 Počítači zbývá {rukaPocitac.Count} karet.");
            Console.WriteLine("\nStiskněte Enter pro pokračování...");
            _ = Console.ReadLine();
        
            ZkontrolujKonecHry();
        }

        private void VypisStavHrace()
        {
            Console.Clear();
            Console.WriteLine();
            Console.WriteLine("====================== STŮL ======================");
            Console.WriteLine($" Stav vyložení -> Uživatel: {(jeVylozenyHrac ? "ANO" : "NE (potřebuje 51 b.)")} | Počítač: {(jeVylozenyPocitac ? "ANO" : "NE")}");
            Console.WriteLine("--------------------------------------------------");VypisStolyPouze();
            Console.WriteLine("==================================================\n");

            Console.WriteLine("=== TVOJE RUKA ===");
            for (int i = 0; i < rukaHrac.Count; i++)
            {
                Console.Write($"[{i}]: {rukaHrac[i]}   ");
                
                if ((i + 1) % 5 == 0) 
                Console.WriteLine();
            }
            Console.WriteLine($"\n\nPočet karet v balíčku: {balicek.Count} | Na kopě leží: [{odhazovaciBalicek.Last()}]");
        }

        private void VypisStolyPouze()
        {
            if (stul.Count == 0)
            {
                Console.WriteLine("               [ Na stole nic není ]");
            }
            else
            {
                for (int i = 0; i < stul.Count; i++)
                {
                    Console.Write($"Sada {i + 1}: ");
                    
                    foreach (Karta k in stul[i]) 
                        Console.Write($"[{k}] ");
                    Console.WriteLine();
                }
            }
        }

        private void ZkontrolujKonecHry()
        {
            if (rukaHrac.Count == 0)
            {
                KonecHry = true;
                Vitez = "Hrac";
            }
            else if (rukaPocitac.Count == 0)
            {
                KonecHry = true;
                Vitez = "Pocitac";
            }else if (balicek.Count == 0 && odhazovaciBalicek.Count <= 1)
            {
                KonecHry = true;
                Vitez = "Remiza";
            }
        }
    }
}