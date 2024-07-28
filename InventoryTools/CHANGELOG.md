# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

### Changed

### Removed

## [1.7.0.17] - 2024-07-28

### Added

- Added a amount owned tooltip sorting option(by retainer name or by inventory category name)
- Added a outdated gear filter/column(will compare your current class/job levels to the gear in the specified inventories)

### Fixed

- Table columns can be hidden/shown using the built-in imgui menu without breaking the layout
- Fixed a bug that would cause right clicking on a list/craft table item to fail


## [1.7.0.16] - 2024-07-23

### Added

- Added a "Is From Fate?" filter/column
- More data is available for the following:
  - Desynth results of items
  - Loot
  - Reduction
  - Gardening
  - Mob Drops
  - Submarine/Airship Drops


### Fixed

- Items should now should the show the correct type of scrip for their requirements

## [1.7.0.15] - 2024-07-21

### Added

- The craft window will warn you when a Universalis request failed, listing the date it happened, and to inform the user of a back off period. It will also warn the user if they make too many requests in a given time period(due to too many plugins making requests).

### Fixed

- Changing the "Retainer Retrieval" setting via the Retainer Bell icon in the craft settings column will refresh the craft list properly.
- Fixed caching of the "Columns" tab that meant that some available columns would not show up.
- The windows tab in the main configuration window had somehow been lost in the shuffle, it's back where it should be.
- Some of the vendors were not parsing due to a bug in LuminaSupplemental, those vendors should now show again.

## [1.7.0.14] - 2024-07-19

### Added

- Added a calamity salvager filter and column, also items that can be purchased from a calamity salvager will be listed within the item window for applicable items

### Fixed

- Fixed a bug that would list missing ingredients for a craft even if they weren't missing
- Fixed duplicate patch data that was breaking the patch column

## [1.7.0.13] - 2024-07-18

### Added

- Added a new "Seach" context menu, provides similar functionality to the game's search but will search across whatever scope you define
- Bicolour Gem Vendors will now show NPCs and their respective locations
- Added new mob spawn data (thanks to Emma <3)

### Fixed

- The context menu shortcuts will now work correctly in the market board
- When using "Active Character" in any of the inventory scopes, this will now consider any "characters" owned by your logged in character as also active
- Removed some old incorrect mob spawn data

## [1.7.0.12] - 2024-07-14

### Added

- The output items of craft lists can now be ordered based on the "Output Ordering" setting by class or name
- Added a "Is custom delivery hand in?" column/filter
- Added a new menu in craft lists that allows you to clear all items and import/export the contents of the list(to your clipboard)
- Added a new hotkey for opening the lists window
- The item window has a new "Owned" section showing all the locations of items within your characters that the plugin knows about
### Fixed

- Certain columns were not showing as available to add within craft lists
- The active search scopes were not fully working
- All slash commands that open AT windows will now toggle instead of only opening
- The configuration wizard's labels should no longer clip