## 🤖 — Telegram Bot for Derivatives & Interest Rate Calculations

This is a Telegram bot built with .NET that allows users to simulate and calculate various interest-based financial instruments, including:

- **Fixed Rate Loans** (Simple or Compound)
- **Floating Rate Loans** (with adjustable reset periods)
- **Overnight Index Swaps (OIS)** based on SOFR
- **Trade tracking** per user with persistent database storage
- **SOFR integration** with live data fetch from the FRED API

---

### 🧠 Main Features

- 🏦 **Fixed Rate Calculation** — supports both simple and compound interest logic  
- 🌊 **Floating Rate Calculation** — allows interest reset every 1, 3, 6, or 12 months  
- 🌙 **OIS (Overnight Index Swap)** — based on user-defined period and overnight rate  
- 🏢 **Company-based Trade Tracking** — all trades are saved with timestamp and company name  
- 📚 **Trade History** — view and reselect from your personal list of saved trades  
- 🌬️ **Live SOFR Fetching** — `/sofr` command gets the latest SOFR value via the FRED API  
- 📈 **Step-by-step UI** — bot guides user through data entry in a conversational style

---

### 🛠️ Technologies Used

- **.NET 8**  
- **Telegram.Bot** official API  
- **Entity Framework Core**  
- **PostgreSQL / SQLite** (via `AppDbContext`)  
- **HTTPClient + JSON Parsing** for SOFR API  
- **Strategy Design Pattern** for modular interest calculations  
- **LINQ, async/await, enums, and clean separation of concerns**

---

### 📦 Project Structure (simplified)

```
TradingSystem/
├── 🧮 CalculationService/           
│   ├── Controllers/                
│   ├── Core/                      
│   └── Program.cs
├── 💾 TradeManagementService/      
│   ├── Controllers/                 
│   ├── Models/                    
│   ├── Data/                      
│   └── Program.cs
└── 🌉 ApiGateway/                   
    ├── Controllers/               
    └── Program.cs

TG_Fitz_Bot/                         
├── Services/ApiGatewayService.cs   
└── Program.cs                       
```

### 📱 Try It in Telegram

You can interact with the bot directly here: https://t.me/iamfitz_bot

⚙️ The bot supports inline keyboards and conversational input to guide you through calculations with ease.

### 🧪 Future Improvements (Ideas)

- [ ] Multi-company support per user  
- [ ] Export trade history to CSV  
- [ ] Admin dashboard (web UI)  
- [ ] Custom amortization schedules  
- [ ] Visual charts of rate dynamics

