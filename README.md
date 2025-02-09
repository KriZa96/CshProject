# Csh Seminarski

## Opis projekta
Csh Seminarski je sustav koji se sastoji od tri aplikacije: Admin, Client i Test. Koristi **SignalR** za komunikaciju između klijentske i administratorske aplikacije.

## Aplikacije projekta

### Admin
- **Opis**: SignalR aplikacija koja služi kao admin sučelje.
- **Funkcionalnosti**:
    - Prima screenshotove od klijentske aplikacije.
    - Sprema primljene screenshotove u zaseban folder za svakog korisnika.
    - Sprema username koji se spaja sa kontekstom spojene aplikacije
    - Preusmjerava poruke prema svim korisnicima

### Client
- **Opis**: Klijentska aplikacija koja šalje screenshotove admin aplikaciji.
- **Funkcionalnosti**:
    - Koristi **SignalR-Client** za komunikaciju s admin aplikacijom.
    - Pokreće **thread timer** koji svakih 5 sekundi screenshota ekran i šalje screenshot admin aplikaciji.
    - Implementiran je **import iz user DLL-a** kako bi se aplikacija mogla označiti kao **DPI aware**.  
      Ovo je bilo nužno jer prilikom korištenja **200% scalinga** standardne metode za dohvat fizičkih piksela nisu davale ispravne rezultate.

### Test
- **Opis**: Aplikacija za testiranje funkcionalnosti **Admin** i **Client** aplikacija.
- Definirano samo par testova
- Client i Admin aplikacija su analizirane uz pomoc sonarqube-a

## Tehnologije
- **C#**
- **.NET**
- **SignalR**

## Pokretanje projekta
1. **Admin aplikacija**
    - Pokrenuti aplikaciju koristeći `dotnet run` u direktoriju admin aplikacije
    Isprobano i developano u Rideru, pa se moze ui kroz IDE pokrenuti, nije isprobano na VS
2. **Client aplikacija**
    - Pokrenuti aplikaciju koristeći `dotnet run` u direktoriju klijentske aplikacije.
   Isprobano i developano u Rideru, pa se moze ui kroz IDE pokrenuti, nije isprobano na VS

