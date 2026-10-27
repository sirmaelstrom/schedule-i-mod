# Mechanics evidence

Installed game: Steam build `25796331`, IL2CPP.

Save version: `0.4.7f12`

## M1: Deal-offer eligibility (installed IL2CPP build 25796331)

Source: local Cpp2IL 2022.1.0-pre-release.21 ISIL dump of installed build 25796331: `ScheduleOne.Economy.Customer.OfferDealValid`, instructions 24–48; `CustomerData` constructor; `Customer.Load`, instructions 100–113; and the `get_TimeSinceLastDealCompleted` / `get_TimeSinceInstantDealOffered` getters. The constructor and load instructions map the saved elapsed fields to offsets 316 and 328. The method rejects the completed-deal value below 360, rejects a pending offer, and checks the instant-offer value against 360 with an exception controlled by another flag whose meaning is not established here. This is static analysis, not a runtime experiment. The unit is unproved; don't convert the threshold to real time or assume an eligible offer will succeed. The older [Mono analysis](../../docs/Game-Analysis.md) is separately build-unspecified.

## M2: Cash balance changes (Mono evidence only)

The same Mono analysis identifies `ScheduleOne.Money.MoneyManager.ChangeCashBalance` as accepting positive and negative changes. Its build is unspecified; applicability to installed IL2CPP build 25796331 is **unverified**.

## M3: Selling and growing

The developer's [Steam description](https://store.steampowered.com/app/3164500/Schedule_I/) describes manufacturing and distributing products to customers. This is a general public description, not a method-level verification of build 25796331; quantities, prices, acceptance and timing are **unverified** for that build.

## M4: Properties and workers

The developer's [Steam description](https://store.steampowered.com/app/3164500/Schedule_I/) describes purchasable properties and recruitable workers and dealers. This is a general public description, not a method-level verification of build 25796331; exact costs, unlocks and returns are **unverified** for that build.

## M5: Pot seed acceptance (installed IL2CPP build 25796331)

Source: local Cpp2IL ISIL dump of installed build 25796331, `ScheduleOne.ObjectScripts.Pot.CanAcceptSeed`, instructions 26–60. The method rejects a pot that is not fully filled with soil and one already containing a seed. This is static analysis; the saved item's ID-to-runtime-class mapping and any other planting requirements have not been verified.
