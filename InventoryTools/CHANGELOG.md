# Changelog

All notable changes to this project will be documented in this file.

The log versioning the plugin versioning will not match as 1.0.0.0 technically does not match semantic versioning but the headache of trying to change this would be too much.
Instead the changelog reader and automation surrounding plugin PRs will add the 1. back in 

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.html).

## [12.0.20] - 2025-06-15

### Added
- Added a new tree view mode for craft lists for quick configuration and visualization of each output item.

### Changed
- The Total Quantity Available column is now available in curated lists
- The "Total Quantity Available" column was renamed "Quantity/Total Quantity Available"

## [12.0.19] - 2025-06-11

### Fixed
- When a list was set to require HQs but individual items were set to be NQ, they'd still be counted as HQ
- Changed the ingredient preference loop detection logic to consider preferences further up the tree

## [12.0.18] - 2025-06-10

### Fixed
- Changed the default ingredient preference order making desynthesis & reduction less likely to be selected when there are better options available
- Added checks to stop sources being self-referential in craft lists

## [12.0.17] - 2025-06-06

### Fixed
- Fixed potential crash when loading if Chat2 is not installed

## [12.0.16] - 2025-06-06

### Added
- Chat2 integration for item context menu

### Fixed
- When a craft list was set to use HQ by default, items that cannot be HQ were being checked resulting in items not being considered for retrieval
- Fixed loggers being cleared

## [12.0.15] - 2025-06-05

### Added
- Sources for Card Packs, PoTD, HoH, Orthos, Anemos, Pagos, Pyros, Hydatos, Bozja, Logograms, Occult, PvP Series and Collectable Shops added (thanks to tracky's data for some of this!)
- Certain quests will now show rewards 
- Certain sources will now show the probability of dropping and min and max drop amounts.
- Setting to disable items when highlighting in shops(off by default)

### Fixed
- Crystals could sometimes ignore the retrieval order in craft lists
- The craft overlay will now display properly when using a different ui scale


## [12.0.14] - 2025-05-29

### Fixed
- Hotkeys should be working and stay working
- The collectable/hq icon should now display correctly in the craft overlay

## [12.0.13] - 2025-05-28

### Added
- Initial 7.25 data
- More coffer data should be available

## [12.0.12] - 2025-05-24

### Fixed 
- Fixed a bug where HQ items were not correctly handled by the acquisition tracker.

### Added
- Duties added as a source to craft lists.
- Added a ingredient patch filter/column/tooltip. This lets you determine the highest patch a ingredient is used in.

### Changed
- The equipment recommendation's select highest iLvl button now takes item stats into consideration.

## [12.0.11] - 2025-05-21

### Fixed
- Fixed an issue in craft lists where multiple items requiring the same item would want more than was actually needed. This only occurred on sub-items that had a Yield above 1.
- The off-hand item can now be unselected in the equipment suggestion window
- The context menu integration has been fixed when right clicking on an item in the hand-in npc in the firmament

### Added
- The equipment suggestion category drop down now has extra options for combat, Melee/Tank/Ranged/Caster
- A loading icon was added to the equipment suggestion window when results are being calculated.

### Changed
- The HQ stats in item tooltips are now absolute
- Large inventory files should load faster
- The equipment suggestion system will now suggest items outside the range shown if no items are available. An icon will be displayed indicating if this is the case.

## [12.0.10] - 2025-05-18

### Added
- Equipment Recommendations System was added, it's available from Windows inside any AT plugin window
  - This new window helps you find gear which you can then add to a craft/curated list
  - It has two modes
  - Class/Job - Find levelling gear for a specific class/level
  - Tool/Weapon - Find all the main hand/off hand items for a set of classes(Crafting, Gathering, Combat)
  - Can be opened with /atrecommend or /atr

### Changed
- Item tooltips inside plugin windows now include stats

## [12.0.9] - 2025-05-17

### Changed
- Collectable items are now supported in craft lists
- Retainer Retrieval can now be configured to only show NQ/Collectable items
- The acquisition tracker now counts collectables when crafted

### Fixed
- The market pricing column was not updating for items that were not specifically set to be bought off the market

### Added
- Added Is Collectable filter/column
- Added Can Be HQ filter/column

## [12.0.8] - 2025-05-08

### Added
- Custom Button and Custom Link Button columns added
- Troubleshooting section allowing you to tweak the acquisition tracker's timings
- Holding down CTRL and scrolling while viewing a grouped source will let you scroll through each tooltip

### Fixed
- Improvements to the acquisition tracker

## [12.0.7] - 2025-05-07

### Added
- The craft overlay will now show a uptime icon for nodes that have uptimes

### Changed
- More safety around the ODR scanner

### Fixed
- Improvements to the acquisition tracker, should hopefully be more accurate and take into account processing delays

## [12.0.6] - 2025-05-06

### Added
- Added sources/uses for Battle/Gathering/Crafting/Company leves
- Added sources for Triple Triad Cards
- The acquisition tracker now supports Market Board purchases and craft lists have a setting to toggle tracking these purchases

### Fixed
- Changes to the way grouped sources are displayed
- Fixed potential crash in the new acquisition tracker
- Improvements to the way certain sources are drawn

## [12.0.5] - 2025-05-03

### Added
- Changelog window added
- Acquiring items through Gathering/Crafting/Buying/Combat will now count towards craft completion for output items. These can be toggled per craft list in the 'Completion Tracking' section. 
  - This uses a new acquisition tracker, please report any issues with it 
  - It's possible to switch back to the old craft tracker, check settings, the old one only tracks crafts
- Sources/Uses added for quests
- The Acquisition/Use columns now have column filters allowing you to search for specific types, categories, rewards and requirements for items
- Overhaul to right click menus on sources/uses. 

### Fixed
- When a craft list is grouped by precraft craft type, grouping will attempt to adjust itself if 2 precrafts rely on each other and an item in the second group is required to be completed before the first.
- When a specific item was set in the ingredient preference list in a craft list, it was not being used over the default logic
- Fixed rare crash in universalis
- Fixed a bug that'd cause all items to be considered inside a gearset.
- Glamour chest parsing should be reliable and not lose track of items
- Added more safety around when free company credits are scanned
- Fixes to the way text is vertically aligned within table cells 
- Ingredient counts in the "Required" column will show the correct total
- Squashed some imgui asserts
- If there are no known locations of mobs, it'll display that instead of displaying nothing in the item window
- Creating a new list will actually open the configuration window to the list correctly
- Universalis requests will now contain a user agent matching the plugin's internal name and version

## [12.0.4] - 2025-04-17

### Added
- Added ability to enable verbose logging
 
## [12.0.3] - 2025-04-14

### Added
- Universalis button added to item window
- Attributes column added
- Each attribute added as a filter
- Physical Damage, Magical Damage, Delay column/filters added
- Fishing and spearfishing were split into 2 different ingredient sourcing types in craft lists, your ingredient sourcing lists will be migrated automatically to have both options.
- Desynthesis will now show up as different sources allowing you to pick which item you want to desynthesis
- When an item has multiple options to be sourced by reduction, gardening, desynthesis or by another item, the ingredient sourcing order will be taken into account to pick the default

### Fixed
- Special shop costs were not being collated properly
- Added more safety around opening the crafting log
- Added more logging around craft completion
- Fixed minor race issue with window restoration
- Closing the crafting overlay will disable highlighting if applicable now

## [12.0.2] - 2025-04-06

### Fixed
- Plugin shutdown time should hopefully be much faster

## [12.0.1] - 2025-04-02

### Fixed
- The tomestones required for certain items was wrong
- Fixed searching when multiple columns with the same name were added
- Clarified the name/help text on some of the buttons
- The teleporter integration was not working
- Optimisations to the internal message queue, things should pop up faster in the plugin

### Added
- Craft lists can be displayed in reverse order via the "Reverse Craft List Order?" filter setting
- The "Total Quantity Available" column can have a scope configured allowing you to pick which characters are considered when calculating the total
- Adding items from a craft list to another craft/curated list now lets you which part of the craft list you want to add
- Added a clear search button for the craft/curated item search bar

## [12.0.0] - 2025-03-28

### Changed
- API12 support
- Craft Window can now import garland tools group URLs
- Are recipes completed filter/column now only show recipes that can actually be completed in the log
- Thank you for your patience and the horses

## [11.1.4] - 2025-03-16

### Fixed
- Fix an issue stopping list configurations from being exported/copied
- Fix an issue with certain shops listing the wrong currency for their cost
- Fix an issue when opening the "More Information" menu/other context menus from the Search results of the marketboard

## [11.1.3] - 2025-03-12

### Fixed
- Fix duplicate craft list crash
- Fix 'Filter items when in retainer' setting not being respected

### Added
- Source/Use columns will output a summarized description when exporting to a CSV/json
- Right clicking on a source/use icon will now give you the ability to "Copy Source/Use Information" making it easier to share source/use information with others

## [11.1.2] - 2025-02-28

### Fixed
- Market cache data is now saved in the background
- Fixed an issue with the market tooltip causing lag
- Retainer market items should parse properly again
- The "Keep market prices for X hours" setting was not being respected
- Teleporting to vendors via the buy menu would sometimes send you to the wrong zone

## [11.1.1] - 2025-02-27

### Fixed
- If the an item is to be sourced by desynthesis in a craft list, it's ingredients cannot have their source set to crafting.
- A depth limit was added to craft lists in case something breaks free of the above limitation
- Allowed the glamour chest to scan for more items
- The use information configuration pane was showing sources and not uses

### Added
- All tooltips additions now have a default colour and a colour must be picked
- The item unlock tooltip can now be configured to group by acquired and also hide characters who have acquired the item already

## [11.1.0] - 2025-02-26

### Added
- Most coffers should now list what they contain

### Fixed
- Added missing NPCs
- Some shop items listed the wrong currency
- Fishing map markers when using Gather(Advanced) were in the wrong spot
- The wrong quantity was being calculated in stock mode in certain situations
- Sourcing items from Desynthesis works again in craft lists
- An edge case would cause FC points to be zerod

This release has a lot of backend changes to prepare for localization, if you notice any weirdness open a bug ticket or post on discord

## [11.0.12] - 2025-02-03

### Fixed
- Fixed hang on boot

## [11.0.11] - 2025-02-02

### Added
- Configuration window reorganised with more sub-sections
- Source/Use grouping is now configurable
- Each tooltip modification can now have it's own colour set(or will fallback to the default colour)
- Source/use tooltip added, will show you the sources/uses for an item inside the tooltip, fully configurable allowing for sources/uses to be hidden/grouped/ungrouped/reordered
- The `Add Item Locations` tooltip can now be sorted by Item quantity

### Fixed
- Worked around a bug in a third-party library that could cause the plugin to not load
- Fixed an issue in how certain craft yields are calculated

## [11.0.10] - 2025-01-14

### Added
- Items that can be crossbred will now show as a source/use
- Added basic shop highlighting(only gil shops so far)
- The required column now has a icon that when hovered will give you a breakdown of that particular item in relation to the craft(required, missing, etc)
- Add to curated list context menu feature
- New installs now come with a housing list
- The inventory data now saves on it's own thread to stop hitching when a user has a lot of items

### Fixed
- Exterior house storage should now be scanned properly
- Items that are considered currency will no longer be grouped as currency if they are to be purchased
- Fixed some bugs related to how craft numbers are calculated
- Fixed a few imgui asserts
- The can be equipped filter and favourites should work again
- The monster drop tooltip was showing a monster instead the map
- NPCs with no shop locations will now longer show up in the action menu in the craft overlay
- Window positions should save properly(they weren't saving if the window was open)
- Grand company shops had the wrong seal counts
- Spectacles will now be considered acquirable and will show in the acquired tooltip
- Added missing gathering points
- When crafting an item that relied on inspection, the subcrafts were not being generated properly

## [11.0.9] - 2024-12-19

### Added
- Added the gathering points required for dark matter clusters to be listed
- Added company craft prototypes as uses on related items
- Added /craftoverlay to toggle the craft overlay
- The craft overlay will be hidden in duties/deep dungeon/cutscenes, this can be configured in settings

## [11.0.8] - 2024-12-17

### Added
- The craft overlay has arrived, this collapsable overlay will show you the next steps in your current craft project.
    - Has a menu for each item for buying/gathering/crafting
    - Is retainer aware, so it'll only show you what you need to extract from each retainer while it's open
    - Provides quick switching between active craft list
- Item icons within the AT interface can now be hovered and a tooltip will show, this can be configured to show never/on icon hover/row hover
- Right clicking on any item within AT will now provide a Gather/Buy/Craft menu with the ability to teleport to specific nodes/shops
- New filter called "Glamour Ready Combined", shows if an item is part of a set that gets combined in the glamour chest
- Extra vendors have been included
- The item window will show if an item has been combined into a glamour ready set
- Initial 7.11 data

### Fixed
- Fishing/Spearfishing items should show more accurate locations
- Certain vendors were not showing due to having no name will now show their NPCs name instead
- Housing vendors have been de-deduplicated

## [11.0.7] - 2024-12-13

### Added
- Added a new craft list mode, stock mode allows you to enter how much of an item you want in total in your inventory, rather than how much you want to gather/craft/buy

## [11.0.6] - 2024-12-06

### Fixed
- Stopped the unlocks from being wiped on logout
- The characters in the unlock tooltip will be shown alphabetically

## [11.0.5] - 2024-12-05
### Fixed
- Certain tooltips were active even when disabled in the settings

## [11.0.4] - 2024-12-04

### Added
- Added a yes/no filter for every single source/use
- Added a yes/no filter for every single source/use category
- Added a item unlock tooltip(shows which characters have an item unlocked)
- Added a search for the list configuration(hopefully this helps configuring lists easier)
- The craft calculator now allows for a scope to be picked(you can choose which inventories you want to use)
- Added exterior house items and housing fixtures as uses

### Changed
- Unlocks are now tracked more reliably
- The way filters are displayed in the list configuration has been adjusted
- Certain filters were superceeded by the source/use system, any lists using those have been migrated
- Amount Owned was renamed to Item Locations

### Fixed
- Item searching in a few places was inconsistent
- Certain tabs of the list configuration page were causing massive FPS drops
- The sample lists did not have any columns by default

## [11.0.3] - 2024-11-28

### Fixed
- Hopefully squashed a bug with the retainer list and highlighting
- Improvements to the NPC parsing means more vendors should have locations/shops

## [11.0.2] - 2024-11-27

### Fixed
- Gemstones should now list in craft list sourcing again
- Houses that have had their interior design changed will now list the correct zone
- The glamour chest should be parsing again
- Hopefully squashed a bug in the retainer list that was causing crashes
- Issues with searching for items when adding to craft/curated lists are fixed

### Added
- Added "Open Crafting Log", "Open Gathering Log", "Open Fishing Log" context menu options
- Added "Open Crafting Log", "Open Gathering Log", "Open Fishing Log", "Open Log" hotkeys
- Added a "Expert Delivery Seal Count" column/filter
- The "Acquisition" column can now have which icons it shows configured(uses column to come in a later version)
- All sources/uses now have detailed tooltips(with more improvements to come), grouping, click actions, right click actions
- When uptimes are listed, it will show the soonest uptime along with an icon that shows all uptimes for that item
- Cash shop sources now have a price in USD

## [11.0.1] - 2024-11-18

### Fixed
- The plugin will now use the dalamud language instead of english
- Stopped certain gathering points from showing up that had bad data
- Tooltips were tweaked and should now show properly
- Shared models within the Item Window is back

### Added
- Added item uses: Chocobo Item, Indoor Furnishing
- Added item sources: Achievement

## [11.0.0] - 2024-11-17

### Changed
- With support for API11 comes a rewrite of AT's data layer that was required due to lumina changes(how we access the game's sheet data)
- There will definitely be some broken things and changes with how the sources/uses are displayed/used in craft lists so please bear with me
- Sources and uses will generally be more detailed now and hovering over them will provide you with seal costs/rewards/etc
- Memory/load time improvements, I've seen as little as 100MB not factoring in lumina's own caches, I've seen it load in under 2-3 seconds now too
- Any bugs, please jump on the xivlauncher discord or from AT's menu hit File -> Report a issue

## [7.1.2] - 2024-10-25

### Fixed
- Fix an issue with the craft button
- Fix highlighting in the free company chest not working

## [7.1.1] - 2024-10-17

### Fixed
- Fix an issue with the retainer list highlighting more than it should
- Fix an issue causing a migration to crash
- Fix an issue that stops "Remove from craft list" from showing in the right click menu

## [7.1.0] - 2024-10-15

### Added
- Curated lists(build your own lists with whatever you want in them)
- Added a new menu bar to the list/craft windows
- Added more options for copying/pasting
- Craft zone, dye count, materia count, are recipes completed and remove columns added
- Dye count, materia count, are recipes completed filters added
- The dye column can be configured to show stain 1/2/both
- Added a new layout(single)

### Fixed
- The second dye will now track (thanks emyxiv)
- Craft columns could not be edited
- Fixed market ordering
- The way inventory sorting is tracked should be faster

## [7.0.20] - 2024-08-04

### Fixed

- Expand the inventory scanner to cover missing currency types
- Fix alignment issue with lists

## [7.0.19] - 2024-07-30

### Added

- Added a new section to the item window that displays the possible recipes for an item and the ingredients for each
- Updated patch data for 7.05

### Fixed

- Attempting to open the craft log via AT will no longer be allowed while crafting
- Fixed a bug that would cause the Gather/Purchase/Buy column to break how right clicking interacted with the tables

## [7.0.18] - 2024-07-29

### Fixed

- Hopefully fully fixed column hiding not breaking the layout
- Craft/Gather button columns now work as intended
- Having an empty tooltip amount owned scope would sometimes make the tooltip show no owned items

## [7.0.17] - 2024-07-28

### Added

- Added a amount owned tooltip sorting option(by retainer name or by inventory category name)
- Added a outdated gear filter/column(will compare your current class/job levels to the gear in the specified inventories)

### Fixed

- Table columns can be hidden/shown using the built-in imgui menu without breaking the layout
- Fixed a bug that would cause right clicking on a list/craft table item to fail

## [7.0.16] - 2024-07-23

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

## [7.0.15] - 2024-07-21

### Added

- The craft window will warn you when a Universalis request failed, listing the date it happened, and to inform the user of a back off period. It will also warn the user if they make too many requests in a given time period(due to too many plugins making requests).

### Fixed

- Changing the "Retainer Retrieval" setting via the Retainer Bell icon in the craft settings column will refresh the craft list properly.
- Fixed caching of the "Columns" tab that meant that some available columns would not show up.
- The windows tab in the main configuration window had somehow been lost in the shuffle, it's back where it should be.
- Some of the vendors were not parsing due to a bug in LuminaSupplemental, those vendors should now show again.

## [7.0.14] - 2024-07-19

### Added

- Added a calamity salvager filter and column, also items that can be purchased from a calamity salvager will be listed within the item window for applicable items.

### Fixed

- Fixed a bug that would list missing ingredients for a craft even if they weren't missing
- Fixed duplicate patch data that was breaking the patch column

## [7.0.13] - 2024-07-18

### Added

- Added a new "Seach" context menu, provides similar functionality to the game's search but will search across whatever scope you define
- Bicolour Gem Vendors will now show NPCs and their respective locations
- Added new mob spawn data (thanks to Emma <3)

### Fixed

- The context menu shortcuts will now work correctly in the market board
- When using "Active Character" in any of the inventory scopes, this will now consider any "characters" owned by your logged in character as also active
- Removed some old incorrect mob spawn data

## [7.0.12] - 2024-07-14

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

## [7.0.11] - 2024-07-13

### Added
- Grand company turn in column/filter added
- Character owner column added
- Items that are grand company turn-ins will now display that in the Uses/Rewards section of the more item window
- Add in inventory scope picker for "Amount Owned" tooltip allowing you to pick which items are shown

### Fixed
- Labels in the wizard should no longer be cut off
- Tetris has returned!

More fixes and features to come, stay tuned

## [7.0.10] - 2024-07-09

### Fixed
- Fix a crash that wold occur when booting the plugin for the first time.

## [7.0.9] - 2024-07-08

### Changed
- Company Credit will now track again
- Import/Export of lists works properly again
- Trial Synthesis will no longer count towards craft lists
- Rolled back a fix applied to counter a bug in dalamud(those with inventory not scanning issues should hopefully be sorted)
- Stopped an old migration from running that would duplicate certain columns
- Console Games Wiki links for items with a # will now be correct

## [7.0.8] - 2024-07-03

### Fixed
- Tooltips are back in action

## [7.0.7] - 2024-07-02

### Changed
- API X support
- 7.0 patch data updated, this is still a WIP
- Tooltips have been disabled temporarily

## [7.0.6] - 2024-06-10

### Added
Added Ephemeral & Hidden Node columns/filters

### Fixed
Certain items were showing as being collected from ephemeral nodes when they were not
Columns in the columns/craft columns picker will be in alphabetical order

## [7.0.5] - 2024-06-04

### Added
Add to Active Craft List context menu feature added
Next uptime column added


### Fixed
Certain columns were not being saved/loaded properly when added to lists
The tooltip footer/header were not showing up in the correct position
The add to craft list context menu was showing up regardless of wanting it or not
When adding an item from certain windows to a craft list, no item would be added
When closing the crafts window, the active list will disable properly(assuming no other list window is open)

## [7.0.4] - 2024-06-03

### Added
- All columns can now be renamed and some can be configured, multiple copies of the same column can be added
- The market integration now supports multiple worlds, associated columns and craft lists can be configured to pick which worlds are applicable to you
- The more information window has a market tab listing the current prices
- Configuration wizard for when you first install the plugin and if you choose when new features come out
- Buy/craft/gather button columns added
- Favourites column added
- Add to craft list context menu added
- The plugin can be opened when not logged in
- A icon can be added to the main dalamud menu for easy access


### Changed
- Filters are now called Lists so there are Item Lists and Craft Lists
- Settings menus reworked
- Support .net 8(finally)

### Removed
- Some of the older Inventory Tools specific slash commands

Thanks to all the testers for their bug reports and patience <3

## [6.2.9] - 2024-03-17

### Fixed
- Stop some game calls being made in the plugin load
- The armoire should now highlight again
- The default highlighting colour for tabs was incorrect

### Changed
- Highlighting now uses the addon lifecycle service provided by Dalamud

## [6.2.8] - 2024-03-16

### Fixed
- Stutter fix thanks to Azure Gem, please submit feedback if you still have issues

## [6.2.7] - 2024-03-15

### Fixed
- Optimize inventory scanner further

## [6.2.6] - 2024-03-14

### Fixed
- The "Relative Item Level" column is no longer a debug only column, give it a try!
- The inventory scanner now runs on the main thread(prefix for new Dalamud version)

## [6.2.5] - 2024-02-22

### Fixed
- Skybuilder resource inspection needed quantity was not calculating correctly

## [6.2.4] - 2024-02-13

### Fixed
- Fix potential STG crash related to fonts

## [6.2.3] - 2024-02-11

### Added
- Ephemeral Nodes are now supported
- Can be Traded was split into Can be Traded and Can be placed on Market

### Fixed
- Character rename restored

## [6.2.2] - 2024-01-11

### Added
- Add "Is Dropped By Mob" column/filter
- Add "Can be Equipped" column/filter
- Orphaned inventories will be removed on plugin load
- Character management section has been updated
- New IPC methods, GetSearchFilters & GetRetrievalItems - thanks pikajude
- Gamer Escape/Console Games Wiki shorcuts in the item window and right click menus

### Fixed
- Fix certain costs for rewards at special shops not listing properly
- Fix "Is Timed Node" filter
- Fix craft lists not refreshing after an item is added/removed via IPC
- Fix an issue where history columns were not exporting any data to CSV
- Item level filter no longer restricts to equipment, if you want to replicate this filter, use the new "Can be Equipped" filter in combination with the existing filter

## [6.2.1] - 2023-12-17

### Added
- Add in some new IPC calls for getting inventory(thanks to emyxiv)
- Fix a bug with setting company craft phases(thanks to zhyupe)

## [6.2.0] - 2023-10-17

### Added
- Airships, Submarines, Mobs, Retainer Ventures should allow their source columns to be filtered
- New column/filter that lets you search for the total number of recipes an item is involved in

### Fixes
- Fix free company credit parsing
- Fixed some bad sub data due to SQ renaming things(thanks infi)

## [6.1.9] - 2023-10-06

### Added
- Added Ephemeral Craft Lists - Add the items you need and once you've crafted them the list deletes itself
- New craft lists will append a number on the end if a craft list with that name already exists
- Added "Name (Selector)" filter for picking items you want to show in a list as a stop gap until a favourites and/or other system is implemented

### Changed
- Minor tweaks to the UI for clarity

## [6.1.8] - 2023-10-05

### Fixes
- Fix crash that occurs due to duplicate item patch data

## [6.1.7] - 2023-10-04

### Addded
- Added a ItemCountOwned IPC, thanks nebel :)
- Store/Patch data added for 6.5, still missing items related to submarines and item sets, PM if you have anything to add
- Addded plugin installer main window button

## [6.1.6] - 2023-09-12

### Added
- Added in a gathered by filter and column

## [6.1.5] - 2023-09-05

### Added
- Added in a gathered by filter and column
- Display options added to craft filters(invert highlighting, etc)

### Fixed
- Catch failures to save market cache

## [6.1.4] - 2023-07-27

### Added
- Craft Completion Mode: Can choose to delete or leave items on completion
- Completed items will show a red X allowing for quickly removing them from a list
- The craft list "To Craft" list can now be shown as tabs or as it currently is(a giant table)

### Fixed
- Removing a craft item will be more consistent
- Completed items will show as "Completed" instead of "Waiting"
- When collapsing/expanding the "To Craft" and "Items in Retainers/Bags" sections, the table layout should stay consistent
- Output items were not checking against the HQRequireds list(Kiwikahawai)

## [6.1.3] - 2023-07-22

### Changed
- The acquisition icon column will display in a slightly nicer order(at least until it's configurable)
- Fixed the way in which shop locations are grouped (KiwiKahawai)
- Fixes to marked items as properly returned (rather than still used) (KiwiKahawai)
- Solves issues with items not appearing in filters if HQ required is set (KiwiKahawai)
- Minor changes to CriticalCommonLib to help support other plugins using it

## [6.1.2] - 2023-07-19

### Changed
- Company Craft phases should now show/switch correctly
- Add reduction data for 6.45 + previously missing reduction items
- Fix a crash that could occur on plugin unload
- Added a HQ Item count IPC method(thanks Taurenkey)

## [6.1.1] - 2023-07-17

### Changed
- Bicolour gem vendors will now show up and any vendors with no name will be listed as "Unknown Vendor" instead of not appearing at all
- Aetherial reduction will let you pick the item to reduce and will be factored into the craft
- Craft window splitter should be easier to see
- Gathering uptime text in the craft window will be red if it's down, green if it's up

## [6.1.0] - 2023-07-15

### Changed
This is the live release of the crafting update for Allagan Tools which brings it closer to being a full replacement of some of the existing external tools. The update includes the following changes:

- Improved handling of items with sources other than crafting. Sourcing can be configured via a priority system and then overridden per item
- There are now options to group the items in the craft list
    - Precrafts: Class, Depth, Together
    - Everything Else: Zone, Together
    - Crystals/Currency: Seperate/Together
- NQ/HQ can be configured per item
- Retainer Retrieval can be configured per item
- Any item can be added to a craft list(completion tracking for non-craft items will come later)
- Teleporation and zoning for vendors has been greatly improved
- There has been a lot of changes under the hood to accommodate these changes so any issues please head to the #plugin-help-forum
  A inventory history module has also been added, it's still very new and is opt in, the plugin will prompt you when you open the new "History" filter if you wish to turn it on.

Also massive thanks to KiwiKahawai for helping me test this thing and helping me reign in my constant feature creep :slight_smile:

## [5.0.11] - 2023-06-29

### Changed
Add search filter to acqusition icons column
Remove unrequired logging
Update lumina supplemental(Thanks to Emma for the mob spawn data)

## [5.0.10] - 2023-06-22

### Changed
Add an ingredient search filter(this will calculate the ingredients required to craft the items selected in the filters configuration and only show those ingredients)
Filters now have a reset button to quickly clear their settings
Multiple choice filters can now be searched from the setting interface + you can add all the items in the drop down list with a button
Added 6.4 submarine drops and unlocks (thanks Infi <3)

## [5.0.9] - 2023-06-16

### Changed
Adjust ItemCount IPC to use int instead of uint

## [5.0.8] - 2023-06-14

### Changed
Framers kit's will now count as items that can be tracked with the acquired column
Fixed some of the existing mob data that was missing decimals
Updated SQ store items list

## [5.0.7] - 2023-06-12

### Changed
Tetris has returned! Turn it on in the 'Fun' section within Settings -> General
The add item search field now accepts advanced filters (||,&&,!, etc)
Added an extra ~800 mob drops, the data should be far more complete and include drops from the latest expansion

## [5.0.6] - 2023-06-05

### Changed
Actually fix the housing crash, much appreciated to Laissabelle for helping me track it down

## [5.0.5] - 2023-06-05

### Changed
Fix 2 crashes that could stop the plugin from loading
Fix hotkey bug
Add mappy data, should have a huge percentage of mob spawns mapped out, still working on mob drops
Add Earthbreak Aethersand (thanks Faye Y.)

## [5.0.4] - 2023-05-29

### Changed
**
Fix a stackoverflow when generating company crafts
Fix Free Company Credit scanning(you need to open the FC window or FC shop in the workshop to get the value reflected in the plugin)

## [5.0.3] - 2023-05-28

### Changed
**
Stop a potential crash when generating craft materials
Correct the calculations for skybuilder recipes
Re-enable context menu integration
Free company credit of your active FC is now being parsed
Free company credit has it's own item now and a page of what can be purchased with it
The JSON export will now use lower case names for it's keys
The ventures table in the item window should display nicer

## [5.0.2] - 2023-05-24

### Changed
**6.4 - Tears of the Plogon**
Support for 6.4
Updated patch data for items
Updated coffer contents
Updated shop items
Hide the fabled Diadchos Sword
More Information context menu disabled for now
A good egg provided more NPC spawn data <3

## [5.0.1] - 2023-05-09

### Changed
**House Storage has arrived**
So this took a while but it has finally come to fruition. A few things to note:

- To have a house register with the plugin you must first enter it, have permission and then open the 'Indoor Furnishings' menu. This will allow for the plugin to see you own the house and add it to your 'Characters' list.
- Once the house is registered due to the way the inventory data of each section is provided, you must enter each section to have it be parsed by the plugin. For Indoor and Outdoor Furnishings you must enter the storeroom tab before that data is collected.
- For Interior Fixtures open the relevant section in the housing menu.
- There's a lot of moving parts so if you run into issues, bugs or crashes hit up the #plugin-help-forum on discord.
- I'll be working on making the 'Is Housing Item' filter a bit more reliable as this might be more important now.

Other Fixes:
Fix to workshop items not having the full set of materials in craft lists
Stopped the FC name from being wiped out
Added has been gathered column and filter
New /moreinfo or /itemwindow command added that will accept either an item's name or ID and show the more item information window

## [4.1.4] - 2023-04-19

### Changed
Retainer Venture Column/Filter
Real Money Shop Column/Filter
Added a window for viewing ventures + window for individual ventures
Added a new search operator, having a single ! will show all items that are not empty
Gil is now right aligned for easier reading
Added more mob spawn data(thanks users for contributing)
Fixed a copy json to clipboard crash for craft lists
Added a Item ID column
Added a Source World column

## [4.1.3] - 2023-04-13

### Changed
Map links should be point to the correct map and have the correct coordinates, especially subdivisions
The quantity and available columns should function faster when searching
Tooltip stability intensifies(<3 to Caraxi)
UI scaling fixes
Stop FC from being ignored even if the name fails to parse

## [4.1.2] - 2023-04-04

### Changed
Fix for lag when searching in certain circumstances
Added ability to copy filters/craft lists as JSON to your clipboard
Can be dyed filter/column added
Uses column added
Patch filter updated to finalise items for 6.35
Added more coffer contents

## [4.1.1] - 2023-03-29

### Changed
Item Patch data added + filter/column, fixed a bug with craft quantities underflowing, fixed an issue with certain data sheets not loading in, added some extra tooltip safety

## [4.1.0] - 2023-03-28

### Changed
New Duties, Mobs, Airships, Submarines Windows
Tabbed/Sidebar Layouts for Craft/Filters windows
Proper Free Company support
Hotkeys for all windows
UI overhaul
Filter and inventory saving speed ups
Craft CSV export
More player currencies are parsed

## [3.1.1] - 2023-03-07

### Changed
Update to support new CS changes.

## [3.1.0] - 2023-02-21

### Changed
Crafting calculation fixes
Reworked tooltips(new implementation + more display options)
Character/retainer world is now tracked + source world filter
Added wildcard searching
Added IPC service for getting item counts, enabling/disabling filters, managing craft lists and item add/remove events
Fixed an issue with the class job filter

## [2.0.18] - 2023-02-06

### Changed
This is purely a crash fix release, nothing else bar the crash has been fixed. You may still encounter a crash until you restart the game.

## [2.0.17] - 2023-01-29

### Changed
This is a bug fix release. Fixed an issue when you initially add in a craft list. Have put in more code to help mitigate a potential saving crash. If anyone is crashing reliably and knows their way around a debugger, can they attach it and get a stack trace please.

## [2.0.16] - 2023-01-26

### Changed
This is a bug fix release. Fixed some potential bugs with IPC initalisation, retainer sort scanning(rolled back to file monitoring for now) and an assembly related crash.

## [2.0.15] - 2023-01-21

### Changed
Fixed retainer sort order crashing
Fixed configuration not saving on game exit

## [2.0.14] - 2023-01-19

### Changed
Fixed Highlighting in Retainer & Main Character Bags
Fixed Gearset Parsing
Fixed context menu offsets - more information should work again
Fixed an issue with the help menu not showing in specific cases
Fixed a bug that would wipe certain inventories when logging in/out

## [2.0.13] - 2023-01-16

### Changed
While this also updates the plugin for 6.3 it's also a full release of the new parsing/scanning system, along with a plethora of new features and additions. Please post a message in the Allagan Tools help channel if you run into issues. See the changelog here https://github.com/Critical-Impact/InventoryTools/commit/5573f9a84ea714bb191d18e6744533a20119d306

## [2.0.5] - 2022-09-10

### Changed
Mini update, one new feature and a refresh on some of the data sourced from garland tools for 6.2
- Thanks to @sabrinaxiv we have a new setting for tooltips, 'Limit to items belonging to the current character?'

## [2.0.4] - 2022-09-03

### Changed
- Bug Fixes
- Stopped a potential memory leak
- Removed old commands from showing in help
- The hotkey check I had in place could have been causing lag, have tweaked it.
- Improved draw times of each window
- People with higher font sizes and ui scales should hopefully be able to see all the buttons
- Collapsing either of the craft window sections will have the other section take the available space.
- The inventory scanning process now runs in the thread pool, hopefully this should reduce stuttering when any item movement occurs(and a rescan needs to happen).

