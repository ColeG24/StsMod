# Pending Workshop change note

Changes committed but not yet released. When releasing, condense this into `workshop.json`'s `changeNote`
(one paragraph, Steam BBCode allowed), run `upload.sh`, then clear this file.

Last release: 2026-09-18.

- Shot Glass reworked: your Brew still holds 1 fewer Ingredient, but Ingredients now cost 1 more Intoxication to play (stacks per copy) instead of granting Intoxication when you drink a Concoction. Upgrade makes it cost 0 Energy. Cards that add Ingredients straight to the pot are not taxed.
- Stir Crazy now deals its damage once, then again for each Ingredient in your Brew, so it is never a dead draw on an empty pot.
- Stockpot now costs 2 and also grants 2 (3) Strength and 2 (3) Dexterity; the upgrade no longer reduces its cost.
- Dregs removed. A Concoction you have not drunk when combat ends now simply vanishes; the potion and the Brew tooltip say so.
