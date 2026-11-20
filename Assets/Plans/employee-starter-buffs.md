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
