# Mechanics evidence

Installed game: Steam build `25796331`, IL2CPP. Save version: `0.4.7f12`.

Save version: `0.4.7f12`

## M1: Deal-offer eligibility (Mono evidence only)

In `ScheduleOne.Economy.Customer.OfferDealValid`, the Mono decompilation in [Game-Analysis.md](../../docs/Game-Analysis.md) checks elapsed time since the last completed deal, an existing offered contract, and time since an instant deal offer. This source does not identify the Mono build and does not prove the installed IL2CPP build behaves the same way. Treat applicability to build 25796331 as **unverified**.

## M2: Cash balance changes (Mono evidence only)

The same Mono analysis identifies `ScheduleOne.Money.MoneyManager.ChangeCashBalance` as accepting positive and negative changes. Its build is unspecified; applicability to installed IL2CPP build 25796331 is **unverified**.

## M3: Selling and growing

The developer's [Steam description](https://store.steampowered.com/app/3164500/Schedule_I/) describes manufacturing and distributing products to customers. This is a general public description, not a method-level verification of build 25796331; quantities, prices, acceptance and timing are **unverified** for that build.

## M4: Properties and workers

The developer's [Steam description](https://store.steampowered.com/app/3164500/Schedule_I/) describes purchasable properties and recruitable workers and dealers. This is a general public description, not a method-level verification of build 25796331; exact costs, unlocks and returns are **unverified** for that build.
