# Gameplay Onboarding

Last updated: 2026-06-05

Implementation status: applied. This document now matches the E2E first-run flow and the current `OnboardingObjectiveCatalog`.

This note documents the early-game guidance and Relaxed-mode tuning.

## Tutorial Flow

The first-run tutorial uses short Romanian steps:

1. Welcome the player and stock a shelf from storage.
2. Set one product price from `Prețuri`.
3. Open the store.
4. Watch customers enter and browse shelves.
5. Watch the cashier queue.
6. Read how money is earned and spent.
7. Read why reputation changes.
8. Follow the current objective.
9. Read the daily report.
10. Finish and continue with the task box/objectives.

The tutorial is intentionally lightweight: it teaches the existing loop without hiding mechanics or adding a separate mode.

## Objective Checklist

The early checklist is backed by `OnboardingObjectiveCatalog`:

- `Aprovizionează primul raft`
- `Setează prețul unui produs`
- `Servește primul client`
- `Cumpără sau plasează un raft nou`
- `Angajează un lucrător`
- `Menține reputația peste 60%`
- `Servește 5 clienți`
- `Finalizează prima zi`

These objectives are revealed as completed/current tasks so the first-time player is not shown the whole run at once. Completed items are computed from the current `GameManager` state.

## Relaxed Mode Tuning

Relaxed mode starts with:

- Cash: `3200 lei`
- Reputation: `65`
- Satisfaction: `60`

Relaxed-only early support:

- Grace period: first 2 days and the first 1200 business ticks of a day.
- Sales wave demand during grace: 45% of normal for the first wave, then 65%.
- Sales wave demand on day 3: 80%.
- Customer visual cap during grace: 2 customers before first service, then 3.
- Customer visual cap through day 3: 4.
- Payroll cost during grace: 50%; later Relaxed payroll: 80%.
- Queue pressure penalty during grace: 45% of the incoming queue penalty basis.
- Early reputation loss multipliers are reduced for stockouts, queue pressure, overflow, and events.
- Reputation cannot be pushed below 60 on day 1, 57 on day 2, or 54 on day 3 by Relaxed-mode penalties.
- Customer-service reputation gains are stronger during grace.
- Day 1 Relaxed shelves do not auto-restock; the player must stock the first shelf manually.

## Recovery Behavior

During the Relaxed grace period, if projected cash falls below `250 lei`, the game grants `450 lei` as starter support and records the support effect. This prevents very early unwinnable states without changing Normal or Hard.

## Feedback

The HUD alert summary and action notifications explain:

- Empty or low shelves.
- Long cashier waits.
- Missing cashier.
- Not enough money.
- Furniture purchase/place failure.
- Reputation risk from shelves and queue pressure.

## Verification

`dotnet test ChronoIndustrialist.sln` passes with 48 tests. Regression coverage checks Relaxed defaults, onboarding order/completion, first-checkout feedback, price validation/target binding, save/load cleanup, and UI refresh throttling.

## Adjusting Later

Balance values live in `GameManager` constants near the business tick settings. Prefer changing those constants and the related tests together. Keep Normal and Hard behavior unchanged unless a separate balance pass intentionally covers them.
