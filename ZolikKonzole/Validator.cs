namespace ZolikKonzole
{
    public static class Validator
    {
        public static bool JePlatnaKombinace(List<Karta> sada)
        {
            if (sada == null || sada.Count < 3) return false;

            var zolici = sada.Where(k => k.Hodnota == "Žolík").ToList();
            var beznatKarty = sada.Where(k => k.Hodnota != "Žolík").ToList();

            if (beznatKarty.Count == 0) return true;

            return JePlatnaSkupina(beznatKarty, zolici.Count) || JePlatnaPostupka(beznatKarty, zolici.Count);
        }

        public static bool JePlatnePrilozeni(List<Karta> stavajiciSada, Karta novaKarta)
        {
            List<Karta> novaSada = [.. stavajiciSada, novaKarta];
            return JePlatnaKombinace(novaSada);
        }

        private static bool JePlatnaSkupina(List<Karta> karty, int pocetZoliku)
        {
            string prvniHodnota = karty[0].Hodnota;
            if (karty.Any(k => k.Hodnota != prvniHodnota)) return false;

            var unikatniBarvy = karty.Select(k => k.Barva).Distinct().ToList();
            if (unikatniBarvy.Count != karty.Count) return false;

            if (karty.Count + pocetZoliku > 4) return false;

            return true;
        }

        private static bool JePlatnaPostupka(List<Karta> karty, int pocetZoliku)
        {
            string prvniBarva = karty[0].Barva;
            if (karty.Any(k => k.Barva != prvniBarva)) return false;

            var serazeneKarty = karty.OrderBy(k => k.Sila).ToList();

            if (ZkontrolujCiselneMezery(serazeneKarty, pocetZoliku)) return true;

            if (serazeneKarty.Any(k => k.Hodnota == "A"))
            {
                var kartySEsemJakoJednicka = serazeneKarty.Select(k => new Karta 
                { 
                    Barva = k.Barva, 
                    Hodnota = k.Hodnota, 
                    Sila = k.Hodnota == "A" ? 1 : k.Sila 
                }).OrderBy(k => k.Sila).ToList();

                return ZkontrolujCiselneMezery(kartySEsemJakoJednicka, pocetZoliku);
            }

            return false;
        }

        private static bool ZkontrolujCiselneMezery(List<Karta> seřadeneKarty, int dostupniZolici)
        {
            int potreneZoliky = 0;
            for (int i = 0; i < seřadeneKarty.Count - 1; i++)
            {
                int rozdil = seřadeneKarty[i + 1].Sila - seřadeneKarty[i].Sila;
                if (rozdil == 0) return false; 
                potreneZoliky += (rozdil - 1);
            }
            return potreneZoliky <= dostupniZolici;
        }
    }
}
