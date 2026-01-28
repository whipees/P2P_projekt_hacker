# P2P Bank Network (Hacker Node)

Tento projekt implementuje decentralizovaný bankovní uzel (node) v rámci sítě **Peer-to-Peer (P2P)**. Každý uzel v síti reprezentuje samostatnou banku, která umožňuje správu účtů, vklady, výběry a mezibankovní operace prostřednictvím TCP/IP protokolu.

**Autoři:** Sebastián Janíček, Dan Oujeský  
**Repozitář:** [https://github.com/whipees/P2P_projekt_hacker.git](https://github.com/whipees/P2P_projekt_hacker.git)

---

## Klíčové vlastnosti

- **P2P Architektura**: Každý uzel je zároveň serverem (naslouchá příkazům) i klientem (posílá dotazy).
- **Essentials Proxy**: Automatické přeposílání požadavků (AD, AW, AB), pokud účet patří jiné bance (jiná IP).
- **Hacker Modul (RP)**: Pokročilé plánování loupeží. Algoritmus analyzuje síť a optimalizuje loupež tak, aby zasáhla co nejméně klientů při dosažení cílové částky.
- **Asynchronní zpracování**: Využití `async/await` pro paralelní obsluhu klientů a skenování sítě (fyzická paralelizace).
- **Persistence**: Data jsou bezpečně ukládána do JSON souborů s mechanismem zálohování (`.bak`), aby nedošlo ke ztrátě při pádu aplikace.
- **Lokalizace**: Plná podpora češtiny a angličtiny pro chybové hlášky a výstupy.

---

## Komunikační Protokol

Komunikace probíhá textově (UTF-8) přes TCP na portech **65525 – 65535**. Příkazy jsou case-insensitive, ale standardem jsou velká písmena.

| Kód | Název | Volání | Úspěšná odpověď |
| :--- | :--- | :--- | :--- |
| **BC** | Bank Code | `BC` | `BC <ip>` |
| **AC** | Account Create | `AC` | `AC <account>/<ip>` |
| **AD** | Account Deposit | `AD <account>/<ip> <amount>` | `AD` |
| **AW** | Account Withdrawal | `AW <account>/<ip> <amount>` | `AW` |
| **AB** | Account Balance | `AB <account>/<ip>` | `AB <amount>` |
| **AR** | Account Remove | `AR <account>/<ip>` | `AR` |
| **BA** | Bank Total Amount | `BA` | `BA <amount>` |
| **BN** | Bank Clients | `BN` | `BN <number>` |
| **RP** | Robbery Plan | `RP <target_amount>` | `RP <message>` |
| **ER** | Error | - | `ER <message>` |

---

## Konfigurace a Spuštění

Aplikace po spuštění automaticky generuje konfigurační soubor v adresáři `/Config/config.json`.

- **Port**: Nastavitelný v rozsahu 65525-65535.
- **Timeout**: Výchozí hodnota 5000ms pro síťové operace i obsluhu klientů.
- **IP Adresa**: Možnost dynamické detekce nebo ručního nastavení.

### Spuštění:
1. Otevřete projekt v IDE (Visual Studio) nebo použijte CLI:
   ```bash
   dotnet build
   dotnet run
