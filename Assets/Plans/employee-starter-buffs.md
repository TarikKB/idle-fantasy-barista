# Project Overview
- Game Title: Idle Fantasy Barista (project name).
- High-Level Concept: Grow a fantasy café by brewing and selling coffee, investing in upgrades, and collecting employees. Three assigned employees provide level- and rarity-scaled specialties that support different café builds.
- Players: Single player; local save system. No online services in scope.
- Inspiration / Reference Games: None specified; incremental café and collection-game conventions.
- Tone / Art Direction: Existing fantasy café UI and placeholder character portraits; preserve existing styling. No generated artwork.
- Target Platform: iOS; Unity 6000.3.6f1.
- Screen Orientation / Resolution: Existing portrait Canvas reference 1290 x 2796; retain it.
- Render Pipeline: URP.

# Game Mechanics
## Core Gameplay Loop
Keep the existing brew/sell/upgrade/summon loop and three assignment slots. Add storage, resource efficiency, ticket luck, tips, and machine-specific progression choices. User confirmed: create new employees AND redistribute existing specialties, preserving old character IDs, ownership, levels, copies, and assignments. Existing assigned characters may therefore grant a different specialty after this update.

### Scope and scaling
Implement only these nine additional buffs. Do not implement recycling, instant brewing, conditional/daily effects, role synergies, or continuous offline production.
Values below are tier-1 baselines; each uses base + (level - 1) * growth:
| Buff | Effect | Base | Growth per level | Target |
|---|---|---:|---:|---|
| Bottomless Pantry | Bean storage bonus | 10% | 5 percentage points | Global |
| Perfect Portions | Bean-cost reduction | 2% | 0.5 percentage points | Global |
| Lucky Service | Added ticket chance | 0.2 percentage points | 0.1 percentage points | Global |
| Generous Tips | Chance of 50% extra coffee-sale gold | 2% | 1 percentage point | Global |
| House Blend Expert | Coffee-sale value bonus | 10% | 4 percentage points | DefaultCM / basic_maker |
| Espresso Virtuoso | Brewing-speed bonus | 10% | 4 percentage points | EspressoMachine / espresso_maker |
| Mana Resonance | Coffee-sale value bonus | 10% | 4 percentage points | ManaInfusedCM / mana_maker |
| Arcane Efficiency | Bean-cost reduction | 4% | 0.5 percentage points | ManaInfusedCM / mana_maker |
| Signature Roast | Fame-XP bonus | 15% | 5 percentage points | EspressoMachine / espresso_maker |

Ordinary bonuses retain rarity multipliers 1 / 1.15 / 1.30 / 1.50 / 1.75. Chance effects and cost reductions use 1 / 1.10 / 1.20 / 1.30 / 1.40. Existing four enum values must keep their serialized numeric values; append new types, never reorder.

Matching bonuses add. For machine queries combine applicable global and specialist bonuses additively, then multiply the upgrade-derived stat. Bean reduction caps at 25% including global and target-specific reductions. Ticket chance is upgrade chance plus employee percentage-point bonus, capped by the existing 25% total cap. Tip chance caps at 25%; one independent roll per successful coffee sale, no stacking multiple tip awards. Tip payout is 50% of that sale's final buffed coffee value. No tips on machine refunds or other gold sources.

Bean costs remain fractional (resource balance is already a float); use the same effective cost for manual and automatic affordability checks and deductions. Do not round a one-bean cost back to one or down to zero. Effective storage is upgrade storage multiplied by employee storage; clamp surplus to new capacity when a storage employee is removed. Offline beans obey effective storage. Capture brewing duration at brew start using base duration / effective machine speed; employee changes do not rewrite a running brew. Save and restore that duration as currently supported. Sales/fame/tips use the team at sale time.

### Roster distribution
Preserve one buff per employee. Keep four common Tarik variants on their existing four introductory specialties.
Redistribute existing higher-tier employees:
- Gio: Bottomless Pantry; Gio 2: Perfect Portions; Gio 3: Lucky Service; Gio 4: Generous Tips.
- Matty: House Blend Expert; Ian: Espresso Virtuoso.
- Maid Tarik: Mana Resonance; Maid Matty (retain existing display spelling unless separately requested): Arcane Efficiency; Maid Ian: Signature Roast (espresso); Maid Gio: retain Word of Mouth.
Create nine named employee data assets with stable unique IDs, without new portrait art:
| New employee | Tier | ID | Specialty |
|---|---:|---|---|
| Pippa the Porter | 1 | t1_pippa_porter | Bottomless Pantry |
| Nessa the Alchemist | 2 | t2_nessa_alchemist | Perfect Portions |
| Felix the Fortune Teller | 2 | t2_felix_fortune | Lucky Service |
| Lyra the Server | 2 | t2_lyra_server | Generous Tips |
| Bram the Roaster | 3 | t3_bram_roaster | House Blend Expert |
| Sera the Barista | 3 | t3_sera_barista | Espresso Virtuoso |
| Aster the Mana Mage | 4 | t4_aster_mage | Mana Resonance |
| Mira the Arcanist | 4 | t4_mira_arcanist | Arcane Efficiency |
| Orin the Roastmaster | 5 | t5_orin_roastmaster | Signature Roast (espresso) |
Add them once to the shared database (14 existing + 9 new = 23). Do not grant free ownership or reset saves. Keep current tier probabilities and duplicate leveling costs unchanged; a larger pool reduces per-character pull probability within each tier.

## Controls and Input Methods
Existing New Input System and Canvas UI remain. Staff Assign/Remove keeps the three-slot limit. Add Previous/Next page controls to each tier-tab view, with a page indicator. No new keyboard/touch polling backend.

# UI
Retain the existing Staff hierarchy and card prefab; make targeted additions only.
```
[Existing tier tabs]
[Assigned names / compact active buff totals]
[Previous] [Page 1 / N] [Next]
[Existing employee card grid]
  Name / portrait placeholder / level
  Buff name + effect value + target where applicable
  [Assign | Remove | Team full | Not owned]
[Existing hire buttons]
```
Use paginated groups of up to four employees per tier (two selected tiers per regular page; mythical four per page), preserving usable existing card sizes. Remove the permanent Take(4) truncation; apply Skip/Take for the selected page instead. For two-tier tabs page count is the maximum page count of those tiers. Handle sparse final pages and reset page when switching tabs. Assignment changes refresh the current page, not page one. Summon results retain their existing independent grid and non-assignable cards.
Display Lucky Service as '+0.2 pp ticket chance', costs as '-2% beans', tips as '2% chance: +50% sale gold', and specialist targets explicitly. Summary must not imply machine-only buffs are global. Keep summary compact within the existing band; adjust only relevant spacing if needed, not the whole Canvas.

# Key Asset & Context
Inspected actual scripts and GameScene connections. ResourceManager.employeeRoster, GCDisplayTierManager.roster/database/unitDisplayGrid/employeeTeamText, and GCSummonScript.characterDatabase/roster are wired. Main Canvas uses 1290 x 2796. Shared GachaCell and character database also affect GachaScene.
- Modify Assets/Scripts/GachaCharacters/GCData.cs: append buff types, optional CMData specialist target, scaling and unified effect description.
- Modify Assets/Scripts/GachaCharacters/GCRosterManager.cs: target-aware aggregation and readable active-team summary; preserve assignment/save behavior.
- Modify Assets/Scripts/ResourceManagerScript.cs: effective storage, machine-specific cost/speed/sale/fame queries, capped ticket/tip chances. Keep upgrade bases separate from employee stats and retain roster-event cache refresh.
- Modify Assets/Scripts/CoffeeManagerScript.cs: effective cost in both brew paths, captured machine speed, sale-specific tips and fame.
- Modify Assets/Scripts/MobileSaveManager.cs: use effective capacity on load; preserve fractional bean balances and tip gold in numeric save fields (old integer JSON remains readable). No save reset.
- Modify Assets/Scripts/GachaCharacters/GCCell.cs and GCDisplayTierManager.cs: descriptions, paging, summary.
- Modify Assets/Scripts/GachaCharacters/GCSummonScript.cs: clear/initialize each tier catalogue before populating from database; prevent duplicates/biased weights while adding employees. Keep probabilities and ticket charges unchanged.
- Modify Assets/Scripts/GachaCharacters/Gacha Character Database.asset and the ten existing non-common employee data assets listed above as needed (Maid Gio unchanged).
- Create nine .asset files under existing tier folders or newly consistent Epic Characters / Legendary Characters folders. Do not rename existing assets or IDs.
- Modify Assets/Scenes/GameScene.unity and, after inspecting its hierarchy, Assets/Scenes/GachaScene.unity only for paging controls/references needed by shared UI. Inspect actual Canvas/EventSystem setup before additions.
- Modify Assets/Prefabs/GachaCell.prefab only if existing text bounds need a small adjustment for descriptions.
- Existing machine assets are read-only target references; no machine stat or package changes.
- Prefer target-aware query signatures such as GetEffectiveBeanCost(CMData), GetBrewingSpeedMultiplier(CMData), GetSellPriceMultiplier(CMData), GetFameXPMultiplier(CMData). Keep a compatibility global property where existing consumers require it.

# Implementation Steps
1. Recheck dependencies/dependents for each changed script, prefab, data asset, and scene immediately before edits. Verify clean compile and scene connections; inspect all relevant storage and multiplier consumers. Dependencies: none. Preserve local save files before runtime tests.
2. Extend buff schema, scaling, target references and formatting in GCData; implement target-aware roster aggregation without changing original enum indices or save keys. Dependencies: Step 1.
3. Integrate effective capacity, cost reduction, ticket/tip chances, and machine queries in ResourceManager. Ensure upgrade/reset/roster changes recompute without compounding base stats. Update bean clamps and save numeric precision for fractional cost/gold. Dependencies: Step 2.
4. Integrate both brewing entry paths and sale awards in CoffeeManager; preserve saved brew duration, prevent duplicate tip rolls, and keep refunds unmodified. Dependencies: Step 3.
5. Create nine employees, redistribute higher-tier specialties, and update shared database. Initialize summon catalogues from scratch to include every entry once; validate stable IDs and tier membership. Dependencies: Step 2.
6. Add paging and effect descriptions, wire Prev/Next/page indicator in existing scene hierarchies, refresh current page on assignments, and show target-qualified summary. Preserve summon result behavior and three slots. Dependencies: Steps 2, 5.
7. Run deterministic calculation tests and Play Mode integration/visual checks; restore protected saves and remove temporary test files. Dependencies: Steps 3–6. Report verification limits rather than claiming device/build success without running it.

# Verification & Testing
- Compile with zero new errors. Old four buffs produce unchanged values; level zero gives no effect; level 1/5/10 match table, with correct rarity family.
- Database contains 23 unique IDs, each in exactly one tier catalogue, with no silently hidden employees. Tier probabilities stay 60/28/9/2.5/0.5 percent.
- Test effective storage with upgrades, assignment/removal, generation, AddBeans/SetBeans, save/load, and offline accrual. Surplus clamps consistently; no overwritten base beanLimit.
- A one-bean recipe at 2% reduction costs 0.98; both auto/manual accept exactly enough effective beans and reject insufficient beans. Global+mana reduction respects 25% cap; non-mana recipes ignore mana reduction. Fractional save/load round-trip retains balance and fractional gold.
- Ticket chance increases by percentage points (5% + 0.2 pp = 5.2%); cap applies after combining upgrade and employees. No bonus from unassigned employees.
- Tips: deterministic chance-boundary tests using an injectable/internal roll evaluation helper rather than flaky random assertions. Winning roll gives exactly +50% final coffee value; losing roll adds nothing. One roll per completed sale; machine resale, purchases, and debug AddGold do not trigger it. Chance clamps at 25%.
- Each of five specialists works on the intended machine and not the other two. Matching specialist/global values add before applying upgrades. Captured duration survives team changes and offline restoration; old saves without durations use the unbuffed base safely instead of stale prior-machine values.
- Save/load preserves old IDs, copies, levels and assignment slots; new hires begin unowned. Unknown IDs, missing target, missing old fields, empty team, reset, max-level duplicates, and a full team are safe.
- Paging: visit every page in Common/Rare, Epic/Legendary and Mythical; all 23 entries reachable. Assign/remove on final pages, switch tabs, open/close Staff, and show single/multi summon results without losing page state or enabling result-card assignment.
- Capture Play Mode screenshots of long specialist descriptions, a sparse last page and active new buff summary at portrait aspect. Verify no overlap, clipping, unreadable values or blocked buttons. Confirm Canvas GraphicRaycaster and one correctly configured InputSystemUIInputModule/EventSystem per scene.
- Protect actual persistent saves for tests; restore original data afterward, leave Edit Mode, remove temporary tests/screenshots, inspect Console. iOS device touch/performance and long-term economy balance require follow-up and are not implied by Editor tests.

---

# Franchise Expansion Implementation — Current Review Scope

This extension refines the existing approved plan. The employee work above is historical context and regression coverage: do not recreate employees, redistribute buffs, or redo completed UI changes. This extension supersedes the earlier prohibition on continuous offline production ONLY for retired cafés. The active café retains its existing offline bean and captured-brew restoration, not continuous offline sales.

# Project Overview
- Game Title: Idle Fantasy Barista.
- High-Level Concept: Build a café to Fame 50, pay to open the next café, and leave previous cafés operating as passive-income locations. A shared gold wallet and permanent staff collection support franchise growth; production upgrades and Fame remain local.
- Players: Single player, local saves. No backend, authentication, IAP, or network features.
- Inspiration / Reference Games: No specific references provided; incremental franchise-management conventions.
- Tone / Art Direction: Preserve the existing fantasy café UI, sprites, accessible rarity palette, and uniform staff frames. Reuse existing machine icons and staff cards; no generated art or different café environments in this milestone.
- Target Platform: iOS; Unity 6000.3.6f1.
- Screen Orientation / Resolution: Existing portrait 1290 x 2796 reference Canvas, verified in the loaded scene.
- Render Pipeline: URP.

# Game Mechanics
## Core Gameplay Loop
1. Brew, sell, hire, and buy local upgrades at the newest café.
2. Reach local Fame >= 50 and afford the fixed opening cost.
3. Review what stays behind and confirm expansion.
4. The previous café becomes retired; its machines, upgrade levels, and staff assignments remain there.
5. The new café starts at Fame 1 with local upgrades at level 0 and zero beans. Older cafés generate passive gold into the shared wallet.
6. View past cafés and redistribute staff to balance new-café growth against franchise income.

### Confirmed rules and review defaults
- Fame gate: the ACTIVE café must reach level 50 on every expansion, not global highest-ever Fame.
- Opening cost: fixed across all expansions. Proposed review default: 50,000 gold, configurable in FranchiseConfig.asset. The user selected fixed pricing but did not explicitly specify the amount; approving this plan accepts this editable starting value.
- Gold carries over; deduct only the opening price. Tickets, staff ownership/levels/copies, automation preferences, permanent machine availability, and highest-ever Fame persist.
- Three staff slots per location. One character ID may be assigned to only one location across the entire franchise. Staff levels are global and dynamically affect whichever café employs that character.
- All five existing local upgrade tracks reset for the new café: bean rate, storage, sale value, ticket chance, Fame XP. Retired cafés retain their own levels.
- Initial meta progression is permanent unlocks, highest-ever Fame, staff advancement, and location count. No franchise-point currency, permanent stat-upgrade shop, or additional economy multipliers in this milestone.
- Proposed anti-softlock default: each new café includes ONE basic starter machine, configured by stable machine ID; remaining slots start empty. It is a newly supplied machine, not moved from the old café. This prevents an exactly affordable expansion leaving the player unable to brew. No extra gold or tickets are granted.
- Retired income efficiency: 30%. Offline retired-income cap: 8 hours per absence. Both are configurable; online earnings have no 8-hour session cap.
- Past cafés can be viewed and staffed, but not resumed as active gameplay or used to buy/sell machines or local upgrades. Only the newest café may expand.
- Retired cafés generate gold only, not tickets, Fame XP, or active-café beans. Unstaffed cafés retain base passive income when they own valid machines.

### Permanent machine unlocks
Existing CMListing currently checks local fameLevel against CMData.unlockLevel. Replace this gate with a franchise unlock query backed by highest-ever Fame and saved unlocked machine IDs. Unlock as soon as the threshold is reached, not only when purchased. Seed migration from current Fame and all already-owned machine IDs. Show explicit unlocked/locked catalogue status so resetting local Fame does not appear to relock equipment. Do not give unlocked high-tier machines for free or alter machine prices/stats.

### Deterministic retired income
Create a data-only evaluator using machine IDs/counts, local upgrade levels, and the location's assigned staff IDs with current global levels. Reuse the same base upgrade curves and employee stacking/caps as active gameplay; do not instantiate ResourceManager or brewing objects for retired cafés.
For each valid machine slot i:
- Cycle rate r_i = effective brewing-speed multiplier / base brew seconds.
- Bean demand d_i = r_i * effective beans per brew.
- Gold per cycle v_i = base sellPrice * effective sale multiplier * (1 + capped tip chance * 0.5).
Let B be the location's effective bean generation per second and D = sum(d_i). Apply a shared supply factor s = min(1, B / D) for positive D; if D is zero use s = 1. Then passive gold/sec = 0.30 * s * sum(r_i * v_i).
This deliberately uses proportional throughput under bean shortages, not an optimal profit allocator. It is a sustainable-rate approximation, with no customer-queue or stored-bean transient simulation. Storage upgrades remain visible but do not increase steady-state passive gold. Fame and ticket staff remain useful only for active play; UI explains that they do not generate retired Fame/tickets.
Match current SellCoffee semantics: one sellPrice payout per brew; do not multiply by coffeeProduced unless active sale behavior is separately changed (not in scope). Skip unknown/invalid machine IDs, non-finite or non-positive duration, and invalid staff targets safely. Keep fractional bean costs and cap cost reductions/tip chance as currently implemented.
Persist the last calculated rate for each retired café. Settle accrued income at the OLD rate before any staff transfer, global staff level change, retirement, or balance/config refresh; then update future rates. This prevents newly upgraded staff being applied retroactively to an offline interval.

### Income accounting and safety
- One coordinator owns settlement. Gold is not independently minted by UI cards, the save loader, and each location.
- Use double for wallet and passive calculations; migrate legacy numeric gold without integer rounding. Audit callers so comparisons, purchases, refunds, tips, upgrade deductions, and display remain correct. Existing float costs may widen safely.
- Maintain a persisted settled-through UTC timestamp and per-location saved rate. Online income uses monotonic session elapsed time; pausing settles live earnings and saves a baseline.
- On cold start/resume, credit clamp(elapsed absence, 0, 28800) seconds of retired income once. Advance the checkpoint through the WHOLE valid absence, discarding excess rather than paying it in repeated 8-hour chunks. Save credited gold and the new checkpoint together.
- Repeated LoadGame/resume must not reapply offline rewards or overwrite newer live state. Use a single initialization/resume lifecycle and guard reentrancy.
- Clock rollback gives no negative awards; retain a nondecreasing checkpoint. Forward jumps are capped. This local system is not secure against deliberate clock/save manipulation; do not claim anti-cheat.
- Attribute active coffee sales and retired payouts to their location totals separately from the shared wallet. Refunds, debug awards, opening purchases, and upgrades are not coffee revenue. Revenue totals are informational, NOT an additional expansion gate.

## Controls and Input Methods
Keep New Input System and current uGUI button-event workflow. Add Locations access, café selection, expansion confirmation, and staff transfer actions. No legacy input polling, package changes, or new gesture backend. Viewing a retired café changes view context only, never the active simulation/resource context.

# UI
Reuse the existing MenuManager panel pattern under the current Canvas. Main Canvas is Screen Space Camera, portrait reference 1290 x 2796; nested panels currently include World Space Canvases. Follow verified hierarchy/rendering conventions, retaining the existing EventSystem/InputSystemUIInputModule and GraphicRaycaster wiring.

Locations panel:
```
[Locations                         Close]
Shared gold: ...   Retired income: .../min
[Current: Café 3 | Fame 27/50 | View]
[Café 2 | Retired | ... gold/min | View]
[Café 1 | Retired | ... gold/min | View]
[Open next café: 50,000 gold]
Requires this café's Fame 50 + sufficient gold
```
Use a scrollable list with correct ScrollRect -> Viewport -> Content hierarchy, content sizing, and recycled/reused list rows where practical. Do not rebuild the gameplay Canvas.

Selected café detail:
```
[Café name | Active / Retired]
Fame / retained local upgrade levels
[Machine lineup using existing machine icons]
[Staff 1] [Staff 2] [Staff 3]   [Manage staff]
Income: .../min | Café lifetime revenue: ...
Retired: viewing only; production upgrades cannot be edited
```
Past café viewing is a data-driven inventory/overview, not a promise of a separate fully playable environment. Keep machine displays legible with pagination/scrolling for the existing 20 slots.

Staff picker: reuse tier paging/card visuals. Display "Working at Café N", "Assign", "Remove", "Team full", "Not owned", or "Transfer". A transfer confirmation previews source/destination and retired income changes; reject a full destination without silently replacing staff. Keep active-café summary bound to active assignments, not the viewed café. Existing summon results remain non-assignable; hiring always affects global ownership.

Expansion modal:
- Fame 50 requirement and fixed price, remaining shared balance.
- Old café's projected income and staff remaining there.
- New café: Fame 1, local upgrades 0, zero beans, one basic starter machine.
- Carry-over summary: gold remainder, tickets, permanent unlocks, staff collection.
- Confirm / Cancel, plus recoverable save-failure feedback. Revalidate conditions on confirmation and block double taps.
Offline summary shows earned gold and the 8-hour cap without requiring a second claim action; income is already credited once.
Use text/state labels alongside colors, readable numbers, consistent card dimensions, safe-area-aware placement and appropriately sized mobile touch targets. Preserve current rarity palette and existing font assets; no font regeneration.

# Key Asset & Context
### Verified context
- GameScene: MobileSaveManager references 20 machine slots and three machine definitions; AutoBrewSellManager references the same-sized machine list. ResourceManager.employeeRoster and the staff database/config are connected. MenuManager currently has six panels. One EventSystem uses InputSystemUIInputModule. Console query showed no errors.
- MobileSaveManager stores coffeesave.json, upgradesave.json, staffsave.json separately, reloads on resume, and ResetGame clears global staff/tickets. It must NOT be called for expansion.
- GCRosterManager has a single global assignedEmployees array with three slots. Change assignment storage to per-location while preserving existing active-team consumers.
- ResourceManager Fame currently advances only one level per AddFameXP call and does not reconstruct the threshold when loading a high level. Correct threshold calculation/loading and handle multiple level-ups for reliable Fame 50 gating.
- Upgrade curves are initialized inside UpgradeDataTable.Awake. Extract/read these through shared pure definitions so passive evaluation cannot disagree or depend on an inactive UI object's Awake order.
- Scripts/CMListing.cs purchase gate uses local Fame today; shared unlock query is required. AutoBrew/AutoSell preferences are in PlayerPrefs and should remain intact across expansion.
- Shared GachaCell and staff scripts also affect GachaScene. Inspect that scene before wiring changed contexts. Do not introduce a second persistent franchise authority in alternate scenes.

### Create (proposed names, follow existing project conventions)
- Assets/Scripts/Locations/FranchiseState.cs: versioned global save, LocationState, assignment IDs, lifetime revenue, accounting checkpoint/rates. Reuse/migrate legacy DTOs rather than duplicate ambiguous type names.
- Assets/Scripts/Locations/FranchiseConfig.cs and Assets/Scripts/Locations/FranchiseConfig.asset: Fame 50, fixed price 50,000, efficiency 0.30, offline cap 28800 seconds, starter-machine reference/ID.
- Assets/Scripts/Locations/FranchiseManager.cs: canonical locations, shared economy coordination, expansion gate/transaction, selected view context, assignment transfer and settlement.
- Assets/Scripts/Locations/LocationIncomeCalculator.cs: pure sustainable-rate evaluator.
- Assets/Scripts/Upgrades/UpgradeRules.cs: shared immutable upgrade curves consumed by active UI and calculator.
- Assets/Scripts/Locations/LocationsPanel.cs and StaffLocationPicker.cs if needed: panel/detail/transfer binding with fully qualified UnityEngine.UI types.
- Assets/Prefabs/Locations/LocationListItem.prefab and LocationsPanel.prefab (or scene-authored panel following project conventions), reusing existing graphics.
- Focused test files under Assets/Tests/EditMode/Locations and Assets/Tests/PlayMode/Locations using the project's verified test/assembly setup; do not add packages merely to satisfy a guessed test setup.

### Modify
- MobileSaveManager.cs: versioned unified save, atomic writes/recovery, migration, lifecycle rebind/settlement; full reset remains explicitly destructive.
- ResourceManagerScript.cs: double global wallet, local state capture/restore/reset independent of global tickets/gold; coherent Fame thresholds; unlock notifications; recompute bases without compounding.
- GCRosterManager.cs: global ownership plus per-location assignment queries; active-context compatibility helpers and pre-change settlement for level gains.
- GCCell.cs and GCDisplayTierManager.cs: selected assignment location, employment labels, guarded transfer UI, active/global context separation; preserve current paging and Mythical frame sizing.
- CoffeeManagerScript.cs: attributable coffee-sale award, safe slot clearing/restoration and count reconstruction. No retirement refunds or duplicate ready-coffee payouts.
- CMListing.cs/CMCatalogueScript.cs: permanent machine availability and clear catalogue messaging.
- UpgradeDataTable.cs: consume shared upgrade definitions and keep existing cost/value behavior.
- MenuManagerScript.cs: Locations panel/navigation and clear pending sell/catalogue targets during expansion.
- AutoBrewSellManager.cs only if needed for a temporary transition guard; preserve automation preferences.
- Assets/Scenes/GameScene.unity: wire configuration/coordinator and new panel access. GachaScene.unity only if inspection shows shared staff-context wiring is required. GachaCell.prefab only for employment/transfer label readability.
- Existing machine/character assets and IDs remain unchanged/read-only. No rebalancing individual machines, redoing employee assets, or installing packages.

# Implementation Steps
1. **Baseline and impact check.** Re-read relevant files/scene references; inspect dependencies and reverse references before each edit (use dependency tooling when available, otherwise AssetDatabase dependencies plus source-reference searches). Inspect alternate-scene lifecycle and input configuration. Protect existing persistent saves; establish compile baseline. Dependencies: none.
2. **Shared rules and versioned model.** Add FranchiseState/LocationState and config; extract UpgradeRules preserving numerical behavior. Define stable location IDs and ordered slot IDs/index mapping for 20 machine slots; separate viewedLocationId from activeLocationId. Implement double wallet compatibility and deterministic income evaluator. Dependencies: 1.
3. **Safe persistence and migration.** Refactor MobileSaveManager to franchise-save.json with schema version, a temporary write, verified replacement strategy and last-good backup. Load valid new save first; migrate legacy files only when no new save exists. Missing legacy upgrades/staff get defaults independently rather than blocking coffee load. Preserve wallet, tickets, machines, brewing duration, upgrades, Fame, global levels/copies and assignments in Café 1. Seed unlocks/highest Fame. No retired income before the first retirement. Retain legacy backups; never silently remigrate after new-save corruption. Rebind scene references when necessary and define initialization ordering explicitly. Dependencies: 2.
4. **Local progression and permanent unlocks.** Add capture/apply/new-café operations without using ResetData/ResetGame. Reconstruct Fame threshold as 100 + 150 * (level - 1), clamp level >= 1, process repeated level-ups with a loop, notify highest-ever Fame/unlocks after progression. Keep purchases available after local Fame reset. Recompute local upgrades and purchased-machine count from current slots. Dependencies: 2–3.
5. **Per-location staff.** Make the location records canonical for assignment IDs; global roster retains ownership/levels/copies. Preserve GetAssignedEmployee/GetBuffBonus compatibility for active café; add explicit location-aware queries. Enforce exclusive employment and three slots. Before transfer/level gain, settle all affected old rates; after change, recalculate. Reject invalid IDs/unowned staff/full destinations without side effects. Dependencies: 2–4.
6. **Passive accounting and resume.** Integrate online monotonic settlement, persisted old rates, and single offline settlement with 8-hour cap. Save awarded gold/checkpoint together. Handle repeated resume/load, rollback clocks, full-reset deletion and corruption recovery safely. Active-café offline restoration remains unchanged in scope. Dependencies: 3–5.
7. **Atomic expansion.** Revalidate local Fame >= 50 and wallet >= fixed price. Settle old income, capture current machines/upgrades/staff, prepare a deep-copied candidate save: deduct price once, retire old location, create new ID/state with Fame 1/XP 0/local upgrades 0/beans 0/empty team and one starter basic machine. Commit candidate durably before applying scene changes; failure keeps old gameplay/state usable. Clear brewing coroutines, sell mode/pending targets/catalogue, restore new slots, rebuild ownership count and shelf visuals, refresh active staff/upgrade/Fame UI. Guard against automation and double confirmations throughout. Dependencies: 3–6.
8. **Locations and transfer UI.** Create/wire list, past café overview, staff picker/transfer modal, expansion preview and offline summary in verified existing hierarchy. Past viewing must not redirect brewing, upgrades, summoning ownership or active staff caches. Preserve rare-color/size changes and existing tier paging. Dependencies: 5–7.
9. **Integration and recovery validation.** Run calculation, migration, resume, transfer, gate, transaction-failure and Play Mode tests below; capture portrait screenshots, check Console and inspect saved JSON. Restore protected user saves/test environments; finish in Edit Mode. Report iOS testing/build limits honestly. Dependencies: 1–8.

# Verification & Testing
- Gate boundaries: Fame 49 fails regardless of gold; Fame 50 with 49,999 fails; Fame 50 with exactly 50,000 succeeds. Negative/NaN/infinite config values rejected. Every new café must independently reach 50; highest-ever Fame cannot bypass gate.
- Expansion subtracts 50,000 once, retains any remainder/tickets/unlocks/global staff, preserves old machine/upgrades/team, creates Fame 1/XP 0/new local base stats and starter machine. Rapid double taps, closing modal, save failure, app pause and termination never produce half-retirement or duplicate charges.
- Verify threshold reconstruction at Fame 1/49/50, high saved levels and multi-level XP awards. Reset/new café displays correct XP bar and threshold, unlock IDs remain available even at Fame 1.
- Migration round-trip all legacy fields and missing optional files. Distinct IDs, slot ordering, duplicate assignments, unknown character/machine IDs, corrupted primary/valid backup, interrupted temporary write and migration retry are safe. No unsolicited reset; missing new fields receive explicit defaults.
- Staff: each owned character occupies at most one of three slots in one café. Transfer updates source/destination bonuses and income, removal updates old café, full-team rejection mutates neither café, leveling a character employed at a retired café settles old rate before increase. Viewing another café does not change active bonuses or running brew duration.
- Calculator fixtures: e.g. one machine with 5-second brew, cost 1 bean, sale 5 gold, bean rate 1/s and no staff yields 0.30 gold/sec at 30% efficiency. Bean bottleneck, zero supply, multiple identical machines, mixed demand, speed/sale/bean/cost specialists, capped costs/tips, unknown IDs and invalid durations match the documented formula. coffeeProduced does not spuriously multiply sale awards.
- Accounting: 1-hour offline gives rate * 3600, 8-hour gives rate * 28800, 24-hour also gives only rate * 28800. Repeated resumes cannot claim discarded time. A 10-hour online session earns all 10 hours. Backward clock gives no loss/repeated windfall, forward jumps are capped. Fractional rates and large double wallet balances retain meaningful awards through save/load.
- No passive gold from active café in addition to manual/auto sales; retired cafés give no tickets/Fame/beans. Active sales/refunds/debug gains classified correctly; retirement cancels old ready/brewing states without extra sale awards/refunds.
- Full reset still intentionally clears franchise/global/local data with confirmation, including new backups, so a fallback cannot resurrect deleted progress; expansion never calls it. Automation preferences persist across expansions.
- UI: all locations reachable with many cafés and 20-machine overviews; readable staff/source/destination labels, consistent frame dimensions, no clipped long names, no overlap or duplicated event listeners, exactly one working EventSystem, correct raycast/masking, blocked old-café upgrade/purchase actions. Confirm online income continues while viewing past cafés.
- Verify existing manual/auto brew, sales, ticket rolls, staff leveling, paging, upgrades and machine resale regressions. Editor test success does not imply iOS device performance, touch, filesystem replacement reliability, or long-term economy balance; those need actual device follow-up.

