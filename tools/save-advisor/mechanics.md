# Mechanics evidence

Installed game: Steam build `25796331`, IL2CPP. Save version: `0.4.7f12`.

Save version: `0.4.7f12`

## M1: Deal-offer eligibility (installed IL2CPP build 25796331)

Source: local Cpp2IL 2022.1.0-pre-release.21 ISIL dump of installed build 25796331, `ScheduleOne.Economy.Customer.OfferDealValid`, instructions 24–48, and `get_TimeSinceLastDealCompleted` / `get_TimeSinceInstantDealOffered`. The first getter reads offset 316, compared with 360; below 360 the method returns false with “Customer recently completed a deal”. It also rejects a pending offer, and checks offset 328 (the instant-offer getter) against 360, with an exception controlled by another flag whose meaning is not established here. This is a static analysis of this build, not a runtime experiment. The unit and exact save-to-runtime mapping have not been independently proved; don't convert the threshold to real time or assume an eligible offer will succeed. The older [Mono analysis](../../docs/Game-Analysis.md) is separately build-unspecified.

## M2: Cash balance changes (Mono evidence only)

The same Mono analysis identifies `ScheduleOne.Money.MoneyManager.ChangeCashBalance` as accepting positive and negative changes. Its build is unspecified; applicability to installed IL2CPP build 25796331 is **unverified**.

## M3: Selling and growing

The developer's [Steam description](https://store.steampowered.com/app/3164500/Schedule_I/) describes manufacturing and distributing products to customers. This is a general public description, not a method-level verification of build 25796331; quantities, prices, acceptance and timing are **unverified** for that build.

## M4: Properties and workers

The developer's [Steam description](https://store.steampowered.com/app/3164500/Schedule_I/) describes purchasable properties and recruitable workers and dealers. This is a general public description, not a method-level verification of build 25796331; exact costs, unlocks and returns are **unverified** for that build.
