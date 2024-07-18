# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

### Changed

### Removed

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