USE GameLibraryDB;
GO

INSERT INTO Users (Email, PasswordHash, Username, Role, ProfileImageURL) VALUES
('admin@test.com', '3b612c75a7b5048a435fb6ec81e52ff92d6d795a8b5a9c17070f6a63c97a53b2', 'The Judge', 'Admin', 'https://cdna.artstation.com/p/assets/images/images/053/917/268/large/so-riya-tevii179.jpg?1663322139'),
('user@test.com', 'a61a8adf60038792a2cb88e670b20540a9d6c2ca204ab754fc768950e79e7d36', 'Just testing', 'User', 'https://i.pinimg.com/736x/a9/3e/86/a93e8677cdf10a4c28691c3e8719401a.jpg'),
('gamer@test.com', 'a61a8adf60038792a2cb88e670b20540a9d6c2ca204ab754fc768950e79e7d36', 's2mple', 'User', 'https://esports-marketing-blog.com/wp-content/uploads/2022/10/S1mple.webp'),
('casual@test.com', 'a61a8adf60038792a2cb88e670b20540a9d6c2ca204ab754fc768950e79e7d36', 'im not a casual', 'User', 'https://i.pinimg.com/736x/80/17/86/80178693d1d0c7e0ec688707b02ecc0b.jpg'),
('collector@test.com', 'a61a8adf60038792a2cb88e670b20540a9d6c2ca204ab754fc768950e79e7d36', 'elixir_collector', 'User', 'https://static.wikia.nocookie.net/clashroyale/images/d/db/Elixir_Collector_card_render.png/revision/latest/scale-to-width-down/250?cb=20250903140357'),
('speedrunner@test.com', 'a61a8adf60038792a2cb88e670b20540a9d6c2ca204ab754fc768950e79e7d36', 'SPID', 'User', 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTYvpoaYza58kZ95AAXH4gtyyqgYndRRSzLXA&s'),
('rpgfan@test.com', 'a61a8adf60038792a2cb88e670b20540a9d6c2ca204ab754fc768950e79e7d36', 'THAT RPG fan', 'User', 'https://img.gta5-mods.com/q75/images/rpg-7/1ef4d9-20161112221237_1.jpg'),
('indiegamer@test.com', 'a61a8adf60038792a2cb88e670b20540a9d6c2ca204ab754fc768950e79e7d36', 'HK Enjoyer', 'User', 'https://images.cults3d.com/wSE5SgR6OFLeJ3J0Z6Vl6KzV9fg=/516x516/filters:no_upscale()/https://fbi.cults3d.com/uploaders/35661245/illustration-file/788ca194-218c-4e1e-941f-a51d552d8fe1/Hornet1.jpeg'),
('multiplayer@test.com', 'a61a8adf60038792a2cb88e670b20540a9d6c2ca204ab754fc768950e79e7d36', 'MultiplayerPro', 'User', 'https://blogger.googleusercontent.com/img/b/R29vZ2xl/AVvXsEj2ia5dqGXTfBRE_U8fxJLLChZ0fkFlHnZKZmDZhus1aoiBgSgTwfaA0AyN9vOnMc4F01tT6_zj68vfMlmWw8dMs0aZtOGIru2cX82EOHDBMxfv7De5I816exBqflLHbghKIS_IOGIR2ok/s1600/Hardcore+gamer+header.png'),
('retro@test.com', 'a61a8adf60038792a2cb88e670b20540a9d6c2ca204ab754fc768950e79e7d36', 'OG', 'User', 'https://ichef.bbci.co.uk/news/480/cpsprodpb/EAEE/production/_96724106_gettyimages-157437668.jpg'),
('newplayer@test.com', 'a61a8adf60038792a2cb88e670b20540a9d6c2ca204ab754fc768950e79e7d36', 'Newbie', 'User', 'https://voca-land.sgp1.cdn.digitaloceanspaces.com/0/1757672799171/29194d54.jpg'),
('hardcore@test.com', 'a61a8adf60038792a2cb88e670b20540a9d6c2ca204ab754fc768950e79e7d36', 'Harder than you think', 'User', 'https://media.tenor.com/kq7GyBPPIj0AAAAe/sweaty-speedrunner.png');
GO

INSERT INTO Platforms (PlatformName) VALUES
('PC'),
('PlayStation 4'),
('PlayStation 5'),
('Xbox One'),
('Xbox Series X'),
('Nintendo Switch'),
('Mobile');
GO

INSERT INTO Genres (GenreName) VALUES
('Action'),
('RPG'),
('Strategy'),
('Adventure'),
('FPS'),
('Horror'),
('Puzzle'),
('Racing'),
('Sports'),
('Simulation'),
('Indie'),
('Platformer'),
('Open World'),
('Survival'),
('Roguelike');
GO

DECLARE @AdminUserID INT = (SELECT UserID FROM Users WHERE Email = 'admin@test.com');

INSERT INTO Games (Title, Developer, Publisher, ReleaseYear, Description, CoverImageURL, CreatedBy) VALUES
('The Witcher 3: Wild Hunt', 'CD Projekt RED', 'CD Projekt', 2015, 'An open-world action RPG set in a fantasy universe. Play as Geralt of Rivia, a monster hunter for hire.', 'https://cdn.akamai.steamstatic.com/steam/apps/292030/library_600x900.jpg', @AdminUserID),
('Elden Ring', 'FromSoftware', 'Bandai Namco Entertainment', 2022, 'A fantasy action-RPG adventure set within a world created by Hidetaka Miyazaki and George R. R. Martin.', 'https://cdn.akamai.steamstatic.com/steam/apps/1245620/library_600x900.jpg', @AdminUserID),
('Cyberpunk 2077', 'CD Projekt RED', 'CD Projekt', 2020, 'An open-world, action-adventure story set in Night City, a megalopolis obsessed with power, glamour and body modification.', 'https://cdn.akamai.steamstatic.com/steam/apps/1091500/library_600x900.jpg', @AdminUserID),
('Red Dead Redemption 2', 'Rockstar Games', 'Rockstar Games', 2018, 'An epic tale of life in America''s unforgiving heartland. The game''s vast and atmospheric world will also provide the foundation for a brand new online multiplayer experience.', 'https://cdn.akamai.steamstatic.com/steam/apps/1174180/library_600x900.jpg', @AdminUserID),
('God of War', 'Santa Monica Studio', 'Sony Interactive Entertainment', 2018, 'His vengeance against the Gods of Olympus years behind him, Kratos now lives as a man in the realm of Norse Gods and monsters.', 'https://cdn.akamai.steamstatic.com/steam/apps/1593500/library_600x900.jpg', @AdminUserID),
('Hades', 'Supergiant Games', 'Supergiant Games', 2020, 'Defy the god of the dead as you hack and slash out of the Underworld in this rogue-like dungeon crawler.', 'https://cdn.akamai.steamstatic.com/steam/apps/1145360/library_600x900.jpg', @AdminUserID),
('Celeste', 'Maddy Makes Games', 'Maddy Makes Games', 2018, 'A single-player adventure about climbing a mountain. Help Madeline survive her journey to the top of Celeste Mountain.', 'https://cdn.akamai.steamstatic.com/steam/apps/504230/library_600x900.jpg', @AdminUserID),
('Stardew Valley', 'ConcernedApe', 'ConcernedApe', 2016, 'You''ve inherited your grandfather''s old farm plot in Stardew Valley. Armed with hand-me-down tools and a few coins, you set out to begin your new life.', 'https://cdn.akamai.steamstatic.com/steam/apps/413150/library_600x900.jpg', @AdminUserID),
('The Last of Us Part II', 'Naughty Dog', 'Sony Interactive Entertainment', 2020, 'Set five years after the events of The Last of Us, Ellie embarks on a journey of retribution when a violent event disrupts her peace.', 'https://cdn.akamai.steamstatic.com/steam/apps/1888930/library_600x900.jpg', @AdminUserID),
('Hollow Knight', 'Team Cherry', 'Team Cherry', 2017, 'Forge your own path in Hollow Knight! An epic action adventure through a vast ruined kingdom of insects and heroes.', 'https://cdn.akamai.steamstatic.com/steam/apps/367520/library_600x900.jpg', @AdminUserID),
('Disco Elysium', 'ZA/UM', 'ZA/UM', 2019, 'Disco Elysium is a groundbreaking role playing game. You''re a detective with a unique skill system at your disposal and a whole city block to carve your path across.', 'https://cdn.akamai.steamstatic.com/steam/apps/632470/library_600x900.jpg', @AdminUserID),
('Portal 2', 'Valve', 'Valve', 2011, 'The "Perpetual Testing Initiative" has been expanded to allow you to design co-op puzzles for you and your friends!', 'https://cdn.akamai.steamstatic.com/steam/apps/620/library_600x900.jpg', @AdminUserID),
('Half-Life: Alyx', 'Valve', 'Valve', 2020, 'Half-Life: Alyx is Valve''s VR return to the Half-Life series. It''s the story of an impossible fight against a vicious alien race known as the Combine.', 'https://cdn.akamai.steamstatic.com/steam/apps/546560/library_600x900.jpg', @AdminUserID),
('Outer Wilds', 'Mobius Digital', 'Annapurna Interactive', 2019, 'Named Game of the Year 2019 by Giant Bomb, Polygon, Eurogamer, and The Guardian, Outer Wilds is an open world mystery about a solar system trapped in an endless time loop.', 'https://cdn.akamai.steamstatic.com/steam/apps/753640/library_600x900.jpg', @AdminUserID),
('Dead Cells', 'Motion Twin', 'Motion Twin', 2018, 'Dead Cells is a rogue-lite, metroidvania-inspired, action-platformer, allowing you to explore a sprawling, ever-changing castle...', 'https://cdn.akamai.steamstatic.com/steam/apps/588650/library_600x900.jpg', @AdminUserID),
('Factorio', 'Wube Software', 'Wube Software', 2020, 'Factorio is a game about building and creating automated factories to produce items of increasing complexity, within an infinite 2D world.', 'https://cdn.akamai.steamstatic.com/steam/apps/427520/library_600x900.jpg', @AdminUserID),
('RimWorld', 'Ludeon Studios', 'Ludeon Studios', 2018, 'A sci-fi colony sim driven by an intelligent AI storyteller. Generates stories by simulating psychology, ecology, gunplay, melee combat, climate, biomes, diplomacy, interpersonal relationships, art, medicine, trade, and more.', 'https://cdn.akamai.steamstatic.com/steam/apps/294100/library_600x900.jpg', @AdminUserID),
('Subnautica', 'Unknown Worlds Entertainment', 'Unknown Worlds Entertainment', 2018, 'Descend into the depths of an alien underwater world filled with wonder and peril. Craft equipment, pilot submarines, and out-smart wildlife to explore lush coral reefs, volcanoes, cave systems, and more.', 'https://cdn.akamai.steamstatic.com/steam/apps/264710/library_600x900.jpg', @AdminUserID),
('Terraria', 'Re-Logic', 'Re-Logic', 2011, 'Dig, fight, explore, build! Nothing is impossible in this action-packed adventure game. The world is your canvas and the ground itself is your paint.', 'https://cdn.akamai.steamstatic.com/steam/apps/105600/library_600x900.jpg', @AdminUserID),
('Minecraft', 'Mojang Studios', 'Mojang Studios', 2011, 'Minecraft is a game about placing blocks and going on adventures. Build anything you can imagine with unlimited resources in Creative mode, or go on grand expeditions in Survival, journeying across mysterious lands and into the depths of your own infinite worlds.', 'https://i.redd.it/4lv9qrcjjui71.jpg', @AdminUserID),
('Among Us', 'InnerSloth', 'InnerSloth', 2018, 'An online and local party game of teamwork and betrayal for 4-15 players...in space!', 'https://cdn.akamai.steamstatic.com/steam/apps/945360/library_600x900.jpg', @AdminUserID),
('Fall Guys', 'Mediatonic', 'Devolver Digital', 2020, 'Fall Guys is a massively multiplayer party game with up to 60 players online in a free-for-all struggle through round after round of escalating chaos until one victor remains!', 'https://cdn.akamai.steamstatic.com/steam/apps/1097150/library_600x900.jpg', @AdminUserID),
('Valheim', 'Iron Gate AB', 'Coffee Stain Publishing', 2021, 'A brutal exploration and survival game for 1-10 players, set in a procedurally-generated purgatory inspired by viking culture.', 'https://cdn.akamai.steamstatic.com/steam/apps/892970/library_600x900.jpg', @AdminUserID),
('It Takes Two', 'Hazelight Studios', 'Electronic Arts', 2021, 'Embark on the craziest journey of your life in It Takes Two, a genre-bending platform adventure created purely for co-op.', 'https://cdn.akamai.steamstatic.com/steam/apps/1426210/library_600x900.jpg', @AdminUserID),
('Returnal', 'Housemarque', 'Sony Interactive Entertainment', 2021, 'After crash-landing on this shape-shifting world, Selene finds herself fighting tooth and nail for survival. Again and again, she''s defeated - forced to restart her journey when she dies.', 'https://image.api.playstation.com/vulcan/ap/rnd/202011/0415/FlDoLAOQRrIR6On9aIwBu3it.jpg', @AdminUserID),
('Ratchet & Clank: Rift Apart', 'Insomniac Games', 'Sony Interactive Entertainment', 2021, 'Blast your way through an interdimensional adventure with Ratchet and Clank.', 'https://image.api.playstation.com/vulcan/ap/rnd/202101/2921/DwVjpbKOsFOyPdNzmSTSWuxG.png', @AdminUserID),
('Forza Horizon 5', 'Playground Games', 'Xbox Game Studios', 2021, 'Your Ultimate Horizon Adventure awaits! Explore the vibrant open world landscapes of Mexico with limitless, fun driving action in the world''s greatest cars.', 'https://cdn.akamai.steamstatic.com/steam/apps/1551360/library_600x900.jpg', @AdminUserID),
('Animal Crossing: New Horizons', 'Nintendo EPD', 'Nintendo', 2020, 'Escape to a deserted island and create your own paradise as you explore, create, and customize in Animal Crossing: New Horizons.', 'https://assets.nintendo.com/image/upload/c_fill,w_600/q_auto:best/f_auto/dpr_2.0/ncom/software/switch/70010000027619/9989957eae3a6b545194c42fec2071675c34aadacd65e6b33fdfe7b3b6a86c3a', @AdminUserID),
('Super Mario Odyssey', 'Nintendo EPD', 'Nintendo', 2017, 'Join Mario on a massive, globe-trotting 3D adventure!', 'https://assets.nintendo.com/image/upload/c_fill,w_600/q_auto:best/f_auto/dpr_2.0/ncom/software/switch/70010000001130/c42553b4fd0312c31e70ec7468c6c9bccd739f340152925b9600631f2d29f8b5', @AdminUserID),
('Baldur''s Gate 3', 'Larian Studios', 'Larian Studios', 2023, 'Gather your party and return to the Forgotten Realms in a tale of fellowship and betrayal, sacrifice and survival, and the lure of absolute power.', 'https://cdn.akamai.steamstatic.com/steam/apps/1086940/library_600x900.jpg', @AdminUserID),
('Starfield', 'Bethesda Game Studios', 'Bethesda Softworks', 2023, 'Starfield is the first new universe in over 25 years from Bethesda Game Studios, the award-winning creators of The Elder Scrolls V: Skyrim and Fallout 4.', 'https://cdn.akamai.steamstatic.com/steam/apps/1716740/library_600x900.jpg', @AdminUserID),
('Lies of P', 'Neowiz Games', 'Neowiz Games', 2023, 'Lies of P is a soulslike action RPG based on the classic tale of Pinocchio. Guide Pinocchio on his unrelenting journey to become human.', 'https://cdn.akamai.steamstatic.com/steam/apps/1627720/library_600x900.jpg', @AdminUserID),
('Alan Wake 2', 'Remedy Entertainment', 'Epic Games Publishing', 2023, 'A string of ritualistic murders threatens Bright Falls, a small-town community surrounded by Pacific Northwest wilderness. Saga Anderson arrives to investigate, while Alan Wake pens a dark story to shape reality.', 'https://cdn1.epicgames.com/offer/c4763f236d08423eb47b4c3008779c84/EGS_AlanWake2_RemedyEntertainment_S1_2560x1440-ec44404c0b41bc457cb94cd72cf85872', @AdminUserID),
('Resident Evil 4', 'CAPCOM', 'CAPCOM', 2023, 'Survival is just the beginning. Six years have passed since the biological disaster in Raccoon City. Leon S. Kennedy, one of the survivors, tracks the president''s kidnapped daughter to a secluded European village.', 'https://cdn.akamai.steamstatic.com/steam/apps/2050650/library_600x900.jpg', @AdminUserID),
('Diablo IV', 'Blizzard Entertainment', 'Blizzard Entertainment', 2023, 'The endless battle between the High Heavens and the Burning Hells rages on as chaos threatens to consume Sanctuary. With ceaseless demons to slaughter, countless abilities to master, nightmarish Dungeons, and Legendary loot, this vast, open world brings the promise of adventure and devastation.', 'https://cdn.akamai.steamstatic.com/steam/apps/2344520/library_600x900.jpg', @AdminUserID),
('Final Fantasy VII Remake', 'Square Enix', 'Square Enix', 2021, 'Cloud Strife, an ex-SOLDIER operative, joins Avalanche, an eco-terrorist group, in their fight against the corrupt Shinra Electric Power Company. Experience a reimagining of the beloved RPG classic.', 'https://cdn.akamai.steamstatic.com/steam/apps/1462040/library_600x900.jpg', @AdminUserID),
('Marvel''s Spider-Man Remastered', 'Insomniac Games', 'PlayStation PC LLC', 2022, 'Experience the critically acclaimed Marvel''s Spider-Man, remastered for PC. Swing through New York City as Peter Parker and fight crime as the iconic web-slinger.', 'https://cdn.akamai.steamstatic.com/steam/apps/1817070/library_600x900.jpg', @AdminUserID),
('Horizon Zero Dawn', 'Guerrilla Games', 'PlayStation PC LLC', 2020, 'Experience Aloy''s legendary quest to unravel the mysteries of a future Earth ruled by Machines. Use devastating, tactical attacks against your prey and explore a majestic open world.', 'https://cdn.akamai.steamstatic.com/steam/apps/1151640/library_600x900.jpg', @AdminUserID),
('Sekiro: Shadows Die Twice', 'FromSoftware', 'Activision', 2019, 'Carve your own clever path to vengeance in the critically acclaimed adventure from developer FromSoftware, creators of Bloodborne and the Dark Souls series.', 'https://cdn.akamai.steamstatic.com/steam/apps/814380/library_600x900.jpg', @AdminUserID),
('Persona 5 Royal', 'ATLUS', 'SEGA', 2022, 'Don the mask and join the Phantom Thieves of Hearts as they stage grand heists, infiltrate the minds of the corrupt, and make them change their ways!', 'https://cdn.akamai.steamstatic.com/steam/apps/1687950/library_600x900.jpg', @AdminUserID),
('Mass Effect Legendary Edition', 'BioWare', 'Electronic Arts', 2021, 'The legendary sci-fi RPG trilogy remastered. Experience the complete story of Commander Shepard with enhanced graphics, modernized gameplay, and all DLC included.', 'https://cdn.akamai.steamstatic.com/steam/apps/1328670/library_600x900.jpg', @AdminUserID),
('DOOM Eternal', 'id Software', 'Bethesda Softworks', 2020, 'Hell''s armies have invaded Earth. Become the Slayer in an epic single-player campaign to conquer demons across dimensions and stop the final destruction of humanity.', 'https://cdn.akamai.steamstatic.com/steam/apps/782330/library_600x900.jpg', @AdminUserID),
('Assassin''s Creed Valhalla', 'Ubisoft Montreal', 'Ubisoft', 2020, 'Become Eivor, a legendary Viking raider on a quest for glory. Explore England''s dark ages as you raid your enemies, grow your settlement, and build your political power.', 'https://cdn.akamai.steamstatic.com/steam/apps/2208920/library_600x900.jpg', @AdminUserID),
('Ghost of Tsushima', 'Sucker Punch Productions', 'PlayStation PC LLC', 2024, 'Forge a new path and wage an unconventional war for the freedom of Tsushima. Challenge opponents with your katana, master the bow to eliminate distant threats, develop stealth tactics to ambush enemies, and explore a new story on Iki Island.', 'https://cdn.akamai.steamstatic.com/steam/apps/2215430/library_600x900.jpg', @AdminUserID),
('Star Wars Jedi: Survivor', 'Respawn Entertainment', 'Electronic Arts', 2023, 'The story of Cal Kestis continues in Star Wars Jedi: Survivor, a third-person, galaxy-spanning action-adventure game from Respawn Entertainment.', 'https://cdn.akamai.steamstatic.com/steam/apps/1774580/library_600x900.jpg', @AdminUserID),
('The Finals', 'Embark Studios', 'Embark Studios', 2023, 'The Finals is the ultimate free-to-play combat-centered game show. Join virtual arenas, build teams, and fight to win in a destructible world where strategy and skill collide.', 'https://cdn.akamai.steamstatic.com/steam/apps/2073850/library_600x900.jpg', @AdminUserID),
('Palworld', 'Pocketpair', 'Pocketpair', 2024, 'Fight, farm, build and work alongside mysterious creatures called "Pals" in this completely new multiplayer, open world survival and crafting game!', 'https://cdn.akamai.steamstatic.com/steam/apps/1623730/library_600x900.jpg', @AdminUserID),
('Lethal Company', 'Zeekerss', 'Zeekerss', 2023, 'A co-op horror about scavenging at abandoned moons to sell scrap to the Company.', 'https://cdn.akamai.steamstatic.com/steam/apps/1966720/library_600x900.jpg', @AdminUserID);
GO

DECLARE @Action INT = (SELECT GenreID FROM Genres WHERE GenreName = 'Action');
DECLARE @RPG INT = (SELECT GenreID FROM Genres WHERE GenreName = 'RPG');
DECLARE @Adventure INT = (SELECT GenreID FROM Genres WHERE GenreName = 'Adventure');
DECLARE @OpenWorld INT = (SELECT GenreID FROM Genres WHERE GenreName = 'Open World');
DECLARE @Horror INT = (SELECT GenreID FROM Genres WHERE GenreName = 'Horror');
DECLARE @Roguelike INT = (SELECT GenreID FROM Genres WHERE GenreName = 'Roguelike');
DECLARE @Platformer INT = (SELECT GenreID FROM Genres WHERE GenreName = 'Platformer');
DECLARE @Simulation INT = (SELECT GenreID FROM Genres WHERE GenreName = 'Simulation');
DECLARE @Indie INT = (SELECT GenreID FROM Genres WHERE GenreName = 'Indie');
DECLARE @FPS INT = (SELECT GenreID FROM Genres WHERE GenreName = 'FPS');
DECLARE @Puzzle INT = (SELECT GenreID FROM Genres WHERE GenreName = 'Puzzle');
DECLARE @Survival INT = (SELECT GenreID FROM Genres WHERE GenreName = 'Survival');
DECLARE @Strategy INT = (SELECT GenreID FROM Genres WHERE GenreName = 'Strategy');
DECLARE @Racing INT = (SELECT GenreID FROM Genres WHERE GenreName = 'Racing');

INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Action FROM Games WHERE Title = 'The Witcher 3: Wild Hunt';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @RPG FROM Games WHERE Title = 'The Witcher 3: Wild Hunt';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @OpenWorld FROM Games WHERE Title = 'The Witcher 3: Wild Hunt';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Adventure FROM Games WHERE Title = 'The Witcher 3: Wild Hunt';

INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Action FROM Games WHERE Title = 'Elden Ring';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @RPG FROM Games WHERE Title = 'Elden Ring';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @OpenWorld FROM Games WHERE Title = 'Elden Ring';

INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Action FROM Games WHERE Title = 'Cyberpunk 2077';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @RPG FROM Games WHERE Title = 'Cyberpunk 2077';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @OpenWorld FROM Games WHERE Title = 'Cyberpunk 2077';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @FPS FROM Games WHERE Title = 'Cyberpunk 2077';

INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Action FROM Games WHERE Title = 'Red Dead Redemption 2';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Adventure FROM Games WHERE Title = 'Red Dead Redemption 2';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @OpenWorld FROM Games WHERE Title = 'Red Dead Redemption 2';

INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Action FROM Games WHERE Title = 'God of War';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Adventure FROM Games WHERE Title = 'God of War';

INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Action FROM Games WHERE Title = 'Hades';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Roguelike FROM Games WHERE Title = 'Hades';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Indie FROM Games WHERE Title = 'Hades';

INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Platformer FROM Games WHERE Title = 'Celeste';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Indie FROM Games WHERE Title = 'Celeste';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Puzzle FROM Games WHERE Title = 'Celeste';

INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Simulation FROM Games WHERE Title = 'Stardew Valley';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Indie FROM Games WHERE Title = 'Stardew Valley';

INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Action FROM Games WHERE Title = 'The Last of Us Part II';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Adventure FROM Games WHERE Title = 'The Last of Us Part II';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Horror FROM Games WHERE Title = 'The Last of Us Part II';

INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Action FROM Games WHERE Title = 'Hollow Knight';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Platformer FROM Games WHERE Title = 'Hollow Knight';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Indie FROM Games WHERE Title = 'Hollow Knight';

INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @RPG FROM Games WHERE Title = 'Disco Elysium';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Indie FROM Games WHERE Title = 'Disco Elysium';

INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Puzzle FROM Games WHERE Title = 'Portal 2';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @FPS FROM Games WHERE Title = 'Portal 2';

INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @FPS FROM Games WHERE Title = 'Half-Life: Alyx';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Action FROM Games WHERE Title = 'Half-Life: Alyx';

INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Adventure FROM Games WHERE Title = 'Outer Wilds';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Puzzle FROM Games WHERE Title = 'Outer Wilds';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Indie FROM Games WHERE Title = 'Outer Wilds';

INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Action FROM Games WHERE Title = 'Dead Cells';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Roguelike FROM Games WHERE Title = 'Dead Cells';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Indie FROM Games WHERE Title = 'Dead Cells';

INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Strategy FROM Games WHERE Title = 'Factorio';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Simulation FROM Games WHERE Title = 'Factorio';

INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Strategy FROM Games WHERE Title = 'RimWorld';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Simulation FROM Games WHERE Title = 'RimWorld';

INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Adventure FROM Games WHERE Title = 'Subnautica';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Survival FROM Games WHERE Title = 'Subnautica';

INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Action FROM Games WHERE Title = 'Terraria';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Adventure FROM Games WHERE Title = 'Terraria';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Survival FROM Games WHERE Title = 'Terraria';

INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Adventure FROM Games WHERE Title = 'Minecraft';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Survival FROM Games WHERE Title = 'Minecraft';

INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Puzzle FROM Games WHERE Title = 'Among Us';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Indie FROM Games WHERE Title = 'Among Us';

INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Action FROM Games WHERE Title = 'Fall Guys';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Indie FROM Games WHERE Title = 'Fall Guys';

INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Survival FROM Games WHERE Title = 'Valheim';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Adventure FROM Games WHERE Title = 'Valheim';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Indie FROM Games WHERE Title = 'Valheim';

INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Action FROM Games WHERE Title = 'It Takes Two';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Adventure FROM Games WHERE Title = 'It Takes Two';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Platformer FROM Games WHERE Title = 'It Takes Two';

INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Action FROM Games WHERE Title = 'Returnal';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Roguelike FROM Games WHERE Title = 'Returnal';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Horror FROM Games WHERE Title = 'Returnal';

INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Action FROM Games WHERE Title = 'Ratchet & Clank: Rift Apart';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Platformer FROM Games WHERE Title = 'Ratchet & Clank: Rift Apart';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Adventure FROM Games WHERE Title = 'Ratchet & Clank: Rift Apart';

INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Racing FROM Games WHERE Title = 'Forza Horizon 5';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Simulation FROM Games WHERE Title = 'Forza Horizon 5';

INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Simulation FROM Games WHERE Title = 'Animal Crossing: New Horizons';

INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Platformer FROM Games WHERE Title = 'Super Mario Odyssey';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Adventure FROM Games WHERE Title = 'Super Mario Odyssey';

INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @RPG FROM Games WHERE Title = 'Baldur''s Gate 3';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Adventure FROM Games WHERE Title = 'Baldur''s Gate 3';

INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Action FROM Games WHERE Title = 'Starfield';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @RPG FROM Games WHERE Title = 'Starfield';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @OpenWorld FROM Games WHERE Title = 'Starfield';

INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Action FROM Games WHERE Title = 'Lies of P';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @RPG FROM Games WHERE Title = 'Lies of P';

INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Action FROM Games WHERE Title = 'Alan Wake 2';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Horror FROM Games WHERE Title = 'Alan Wake 2';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Adventure FROM Games WHERE Title = 'Alan Wake 2';

INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Action FROM Games WHERE Title = 'Resident Evil 4';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Horror FROM Games WHERE Title = 'Resident Evil 4';

INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Action FROM Games WHERE Title = 'Diablo IV';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @RPG FROM Games WHERE Title = 'Diablo IV';

INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Action FROM Games WHERE Title = 'Final Fantasy VII Remake';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @RPG FROM Games WHERE Title = 'Final Fantasy VII Remake';

INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Action FROM Games WHERE Title = 'Marvel''s Spider-Man Remastered';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Adventure FROM Games WHERE Title = 'Marvel''s Spider-Man Remastered';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @OpenWorld FROM Games WHERE Title = 'Marvel''s Spider-Man Remastered';

INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Action FROM Games WHERE Title = 'Horizon Zero Dawn';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @RPG FROM Games WHERE Title = 'Horizon Zero Dawn';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @OpenWorld FROM Games WHERE Title = 'Horizon Zero Dawn';

INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Action FROM Games WHERE Title = 'Sekiro: Shadows Die Twice';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Adventure FROM Games WHERE Title = 'Sekiro: Shadows Die Twice';

INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @RPG FROM Games WHERE Title = 'Persona 5 Royal';

INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Action FROM Games WHERE Title = 'Mass Effect Legendary Edition';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @RPG FROM Games WHERE Title = 'Mass Effect Legendary Edition';

INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Action FROM Games WHERE Title = 'DOOM Eternal';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @FPS FROM Games WHERE Title = 'DOOM Eternal';

INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Action FROM Games WHERE Title = 'Assassin''s Creed Valhalla';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @RPG FROM Games WHERE Title = 'Assassin''s Creed Valhalla';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @OpenWorld FROM Games WHERE Title = 'Assassin''s Creed Valhalla';

INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Action FROM Games WHERE Title = 'Ghost of Tsushima';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Adventure FROM Games WHERE Title = 'Ghost of Tsushima';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @OpenWorld FROM Games WHERE Title = 'Ghost of Tsushima';

INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Action FROM Games WHERE Title = 'Star Wars Jedi: Survivor';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Adventure FROM Games WHERE Title = 'Star Wars Jedi: Survivor';

INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Action FROM Games WHERE Title = 'The Finals';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @FPS FROM Games WHERE Title = 'The Finals';

INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Action FROM Games WHERE Title = 'Palworld';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Adventure FROM Games WHERE Title = 'Palworld';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Survival FROM Games WHERE Title = 'Palworld';

INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Horror FROM Games WHERE Title = 'Lethal Company';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Action FROM Games WHERE Title = 'Lethal Company';
INSERT INTO GameGenres (GameID, GenreID) SELECT GameID, @Indie FROM Games WHERE Title = 'Lethal Company';
GO

DECLARE @PC INT = (SELECT PlatformID FROM Platforms WHERE PlatformName = 'PC');
DECLARE @PS5 INT = (SELECT PlatformID FROM Platforms WHERE PlatformName = 'PlayStation 5');
DECLARE @XboxSX INT = (SELECT PlatformID FROM Platforms WHERE PlatformName = 'Xbox Series X');
DECLARE @Switch INT = (SELECT PlatformID FROM Platforms WHERE PlatformName = 'Nintendo Switch');
DECLARE @Mobile INT = (SELECT PlatformID FROM Platforms WHERE PlatformName = 'Mobile');
DECLARE @PS4 INT = (SELECT PlatformID FROM Platforms WHERE PlatformName = 'PlayStation 4');
DECLARE @XboxOne INT = (SELECT PlatformID FROM Platforms WHERE PlatformName = 'Xbox One');

INSERT INTO GamePlatforms (GameID, PlatformID) 
SELECT GameID, @PC FROM Games WHERE Title IN ('The Witcher 3: Wild Hunt', 'Elden Ring', 'Cyberpunk 2077', 'Red Dead Redemption 2', 'Hades', 'Celeste', 'Stardew Valley', 'Hollow Knight', 'Disco Elysium', 'Portal 2', 'Half-Life: Alyx', 'Outer Wilds', 'Dead Cells', 'Factorio', 'RimWorld', 'Subnautica', 'Terraria', 'Minecraft', 'Among Us', 'Fall Guys', 'Valheim', 'It Takes Two', 'Forza Horizon 5');

INSERT INTO GamePlatforms (GameID, PlatformID) 
SELECT GameID, @PS5 FROM Games WHERE Title IN ('Elden Ring', 'Cyberpunk 2077', 'Red Dead Redemption 2', 'God of War', 'The Last of Us Part II', 'It Takes Two', 'Returnal', 'Ratchet & Clank: Rift Apart', 'Forza Horizon 5');

INSERT INTO GamePlatforms (GameID, PlatformID) 
SELECT GameID, @XboxSX FROM Games WHERE Title IN ('Elden Ring', 'Cyberpunk 2077', 'Red Dead Redemption 2', 'Hades', 'It Takes Two', 'Forza Horizon 5');

INSERT INTO GamePlatforms (GameID, PlatformID) 
SELECT GameID, @Switch FROM Games WHERE Title IN ('Hades', 'Celeste', 'Stardew Valley', 'Hollow Knight', 'Dead Cells', 'Terraria', 'Minecraft', 'Among Us', 'Animal Crossing: New Horizons', 'Super Mario Odyssey', 'It Takes Two');

INSERT INTO GamePlatforms (GameID, PlatformID) 
SELECT GameID, @PS4 FROM Games WHERE Title IN ('The Witcher 3: Wild Hunt', 'Red Dead Redemption 2', 'God of War', 'The Last of Us Part II', 'Hollow Knight', 'Dead Cells', 'Terraria', 'Minecraft');

INSERT INTO GamePlatforms (GameID, PlatformID) 
SELECT GameID, @XboxOne FROM Games WHERE Title IN ('The Witcher 3: Wild Hunt', 'Red Dead Redemption 2', 'Hades', 'Hollow Knight', 'Dead Cells', 'Terraria', 'Minecraft', 'Forza Horizon 5');

INSERT INTO GamePlatforms (GameID, PlatformID) 
SELECT GameID, @Mobile FROM Games WHERE Title IN ('Stardew Valley', 'Minecraft', 'Among Us', 'Terraria');

INSERT INTO GamePlatforms (GameID, PlatformID) 
SELECT GameID, @PC FROM Games WHERE Title IN ('Baldur''s Gate 3', 'Starfield', 'Lies of P', 'Alan Wake 2', 'Resident Evil 4', 'Diablo IV', 'Final Fantasy VII Remake', 'Marvel''s Spider-Man Remastered', 'Horizon Zero Dawn', 'Sekiro: Shadows Die Twice', 'Persona 5 Royal', 'Mass Effect Legendary Edition', 'DOOM Eternal', 'Assassin''s Creed Valhalla', 'Ghost of Tsushima', 'Star Wars Jedi: Survivor', 'The Finals', 'Palworld', 'Lethal Company');

INSERT INTO GamePlatforms (GameID, PlatformID) 
SELECT GameID, @PS5 FROM Games WHERE Title IN ('Baldur''s Gate 3', 'Starfield', 'Lies of P', 'Alan Wake 2', 'Resident Evil 4', 'Diablo IV', 'Final Fantasy VII Remake', 'Marvel''s Spider-Man Remastered', 'Horizon Zero Dawn', 'Sekiro: Shadows Die Twice', 'Persona 5 Royal', 'DOOM Eternal', 'Assassin''s Creed Valhalla', 'Ghost of Tsushima', 'Star Wars Jedi: Survivor');

INSERT INTO GamePlatforms (GameID, PlatformID) 
SELECT GameID, @XboxSX FROM Games WHERE Title IN ('Baldur''s Gate 3', 'Starfield', 'Lies of P', 'Alan Wake 2', 'Resident Evil 4', 'Diablo IV', 'Final Fantasy VII Remake', 'Mass Effect Legendary Edition', 'DOOM Eternal', 'Assassin''s Creed Valhalla', 'Star Wars Jedi: Survivor');

INSERT INTO GamePlatforms (GameID, PlatformID) 
SELECT GameID, @PS4 FROM Games WHERE Title IN ('Resident Evil 4', 'Final Fantasy VII Remake', 'Horizon Zero Dawn', 'Sekiro: Shadows Die Twice', 'Persona 5 Royal', 'DOOM Eternal', 'Assassin''s Creed Valhalla', 'Ghost of Tsushima', 'Star Wars Jedi: Survivor');

INSERT INTO GamePlatforms (GameID, PlatformID) 
SELECT GameID, @XboxOne FROM Games WHERE Title IN ('Resident Evil 4', 'Final Fantasy VII Remake', 'DOOM Eternal', 'Assassin''s Creed Valhalla', 'Star Wars Jedi: Survivor');
GO



DECLARE @TestUserID INT = (SELECT UserID FROM Users WHERE Email = 'user@test.com');

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed, PersonalNotes) 
SELECT @TestUserID, GameID, 'Completed', 9.5, 120.0, 'Amazing game! One of the best RPGs ever made.' FROM Games WHERE Title = 'The Witcher 3: Wild Hunt';

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed) 
SELECT @TestUserID, GameID, 'Playing', 9.0, 45.5 FROM Games WHERE Title = 'Elden Ring';

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed) 
SELECT @TestUserID, GameID, 'Completed', 8.5, 60.0 FROM Games WHERE Title = 'Hades';

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed) 
SELECT @TestUserID, GameID, 'Completed', 10.0, 15.0 FROM Games WHERE Title = 'Celeste';

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed) 
SELECT @TestUserID, GameID, 'Playing', 8.0, 30.0 FROM Games WHERE Title = 'Stardew Valley';

INSERT INTO UserGames (UserID, GameID, Status) 
SELECT @TestUserID, GameID, 'Wishlist' FROM Games WHERE Title = 'Red Dead Redemption 2';

INSERT INTO UserGames (UserID, GameID, Status) 
SELECT @TestUserID, GameID, 'Backlog' FROM Games WHERE Title = 'Hollow Knight';

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating) 
SELECT @TestUserID, GameID, 'Completed', 9.0 FROM Games WHERE Title = 'Portal 2';

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed) 
SELECT @TestUserID, GameID, 'Playing', 7.5, 20.0 FROM Games WHERE Title = 'Dead Cells';

INSERT INTO UserGames (UserID, GameID, Status) 
SELECT @TestUserID, GameID, 'Wishlist' FROM Games WHERE Title = 'Disco Elysium';



DECLARE @ProGamerID INT = (SELECT UserID FROM Users WHERE Email = 'gamer@test.com');

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed, PersonalNotes) 
SELECT @ProGamerID, GameID, 'Completed', 10.0, 180.0, 'Masterpiece! Best RPG I''ve ever played. The combat system is incredible.' FROM Games WHERE Title = 'Baldur''s Gate 3';

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed) 
SELECT @ProGamerID, GameID, 'Playing', 8.5, 95.0 FROM Games WHERE Title = 'Starfield';

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed, PersonalNotes) 
SELECT @ProGamerID, GameID, 'Completed', 9.5, 65.0, 'Incredibly challenging but rewarding. The combat is flawless.' FROM Games WHERE Title = 'Sekiro: Shadows Die Twice';

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed) 
SELECT @ProGamerID, GameID, 'Completed', 9.0, 80.0 FROM Games WHERE Title = 'Elden Ring';

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed) 
SELECT @ProGamerID, GameID, 'Playing', 8.0, 45.0 FROM Games WHERE Title = 'Lies of P';

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed) 
SELECT @ProGamerID, GameID, 'Completed', 9.5, 120.0 FROM Games WHERE Title = 'The Witcher 3: Wild Hunt';

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed) 
SELECT @ProGamerID, GameID, 'Playing', 7.5, 30.0 FROM Games WHERE Title = 'DOOM Eternal';

INSERT INTO UserGames (UserID, GameID, Status) 
SELECT @ProGamerID, GameID, 'Wishlist' FROM Games WHERE Title = 'Ghost of Tsushima';



DECLARE @CasualPlayerID INT = (SELECT UserID FROM Users WHERE Email = 'casual@test.com');

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed, PersonalNotes) 
SELECT @CasualPlayerID, GameID, 'Playing', 9.0, 25.0, 'Such a relaxing game! Perfect for unwinding after work.' FROM Games WHERE Title = 'Stardew Valley';

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed) 
SELECT @CasualPlayerID, GameID, 'Completed', 8.5, 35.0 FROM Games WHERE Title = 'It Takes Two';

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed) 
SELECT @CasualPlayerID, GameID, 'Playing', 8.0, 15.0 FROM Games WHERE Title = 'Animal Crossing: New Horizons';

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed) 
SELECT @CasualPlayerID, GameID, 'Completed', 9.5, 40.0 FROM Games WHERE Title = 'Hades';

INSERT INTO UserGames (UserID, GameID, Status) 
SELECT @CasualPlayerID, GameID, 'Backlog' FROM Games WHERE Title = 'Celeste';

INSERT INTO UserGames (UserID, GameID, Status) 
SELECT @CasualPlayerID, GameID, 'Wishlist' FROM Games WHERE Title = 'Palworld';



DECLARE @CollectorID INT = (SELECT UserID FROM Users WHERE Email = 'collector@test.com');

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed, PersonalNotes) 
SELECT @CollectorID, GameID, 'Completed', 9.0, 150.0, 'Amazing collection of three incredible games. A must-have for any RPG fan.' FROM Games WHERE Title = 'Mass Effect Legendary Edition';

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed) 
SELECT @CollectorID, GameID, 'Completed', 8.5, 90.0 FROM Games WHERE Title = 'Persona 5 Royal';

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed) 
SELECT @CollectorID, GameID, 'Completed', 9.5, 100.0 FROM Games WHERE Title = 'Red Dead Redemption 2';

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed) 
SELECT @CollectorID, GameID, 'Playing', 8.0, 60.0 FROM Games WHERE Title = 'Final Fantasy VII Remake';

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed) 
SELECT @CollectorID, GameID, 'Completed', 9.0, 80.0 FROM Games WHERE Title = 'Horizon Zero Dawn';

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed) 
SELECT @CollectorID, GameID, 'Playing', 8.5, 55.0 FROM Games WHERE Title = 'Assassin''s Creed Valhalla';

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed) 
SELECT @CollectorID, GameID, 'Completed', 8.0, 45.0 FROM Games WHERE Title = 'Marvel''s Spider-Man Remastered';

INSERT INTO UserGames (UserID, GameID, Status) 
SELECT @CollectorID, GameID, 'Wishlist' FROM Games WHERE Title = 'Baldur''s Gate 3';

INSERT INTO UserGames (UserID, GameID, Status) 
SELECT @CollectorID, GameID, 'Backlog' FROM Games WHERE Title = 'Starfield';



DECLARE @SpeedRunnerID INT = (SELECT UserID FROM Users WHERE Email = 'speedrunner@test.com');

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed, PersonalNotes) 
SELECT @SpeedRunnerID, GameID, 'Completed', 10.0, 200.0, 'My main speedrun game. Current PB: 22:34!' FROM Games WHERE Title = 'Celeste';

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed) 
SELECT @SpeedRunnerID, GameID, 'Completed', 9.5, 350.0 FROM Games WHERE Title = 'Hollow Knight';

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed) 
SELECT @SpeedRunnerID, GameID, 'Playing', 9.0, 150.0 FROM Games WHERE Title = 'Hades';

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed) 
SELECT @SpeedRunnerID, GameID, 'Completed', 9.0, 180.0 FROM Games WHERE Title = 'Dead Cells';

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed) 
SELECT @SpeedRunnerID, GameID, 'Completed', 8.5, 120.0 FROM Games WHERE Title = 'Portal 2';



DECLARE @RPGFanID INT = (SELECT UserID FROM Users WHERE Email = 'rpgfan@test.com');

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed, PersonalNotes) 
SELECT @RPGFanID, GameID, 'Completed', 10.0, 200.0, 'Absolute masterpiece! The story, characters, and combat are all perfect.' FROM Games WHERE Title = 'Baldur''s Gate 3';

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed) 
SELECT @RPGFanID, GameID, 'Playing', 9.0, 120.0 FROM Games WHERE Title = 'Starfield';

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed) 
SELECT @RPGFanID, GameID, 'Completed', 9.5, 180.0 FROM Games WHERE Title = 'The Witcher 3: Wild Hunt';

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed) 
SELECT @RPGFanID, GameID, 'Completed', 9.0, 160.0 FROM Games WHERE Title = 'Elden Ring';

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed) 
SELECT @RPGFanID, GameID, 'Completed', 8.5, 150.0 FROM Games WHERE Title = 'Persona 5 Royal';

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed) 
SELECT @RPGFanID, GameID, 'Completed', 9.0, 140.0 FROM Games WHERE Title = 'Mass Effect Legendary Edition';

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed) 
SELECT @RPGFanID, GameID, 'Playing', 8.5, 80.0 FROM Games WHERE Title = 'Final Fantasy VII Remake';

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed) 
SELECT @RPGFanID, GameID, 'Completed', 8.0, 100.0 FROM Games WHERE Title = 'Disco Elysium';

INSERT INTO UserGames (UserID, GameID, Status) 
SELECT @RPGFanID, GameID, 'Wishlist' FROM Games WHERE Title = 'Diablo IV';



DECLARE @IndieGamerID INT = (SELECT UserID FROM Users WHERE Email = 'indiegamer@test.com');

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed, PersonalNotes) 
SELECT @IndieGamerID, GameID, 'Completed', 10.0, 40.0, 'Perfect indie game. The story really hit me emotionally.' FROM Games WHERE Title = 'Celeste';

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed) 
SELECT @IndieGamerID, GameID, 'Completed', 9.5, 60.0 FROM Games WHERE Title = 'Hollow Knight';

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed) 
SELECT @IndieGamerID, GameID, 'Completed', 9.0, 70.0 FROM Games WHERE Title = 'Hades';

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed) 
SELECT @IndieGamerID, GameID, 'Playing', 8.5, 35.0 FROM Games WHERE Title = 'Dead Cells';

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed) 
SELECT @IndieGamerID, GameID, 'Completed', 9.5, 50.0 FROM Games WHERE Title = 'Outer Wilds';

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed) 
SELECT @IndieGamerID, GameID, 'Completed', 9.0, 45.0 FROM Games WHERE Title = 'Disco Elysium';

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed) 
SELECT @IndieGamerID, GameID, 'Playing', 8.0, 20.0 FROM Games WHERE Title = 'Stardew Valley';

INSERT INTO UserGames (UserID, GameID, Status) 
SELECT @IndieGamerID, GameID, 'Wishlist' FROM Games WHERE Title = 'Lethal Company';



DECLARE @MultiplayerProID INT = (SELECT UserID FROM Users WHERE Email = 'multiplayer@test.com');

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed, PersonalNotes) 
SELECT @MultiplayerProID, GameID, 'Playing', 9.0, 150.0, 'Addictive! Play with friends every night.' FROM Games WHERE Title = 'The Finals';

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed) 
SELECT @MultiplayerProID, GameID, 'Completed', 9.5, 200.0 FROM Games WHERE Title = 'It Takes Two';

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed) 
SELECT @MultiplayerProID, GameID, 'Playing', 8.5, 80.0 FROM Games WHERE Title = 'Valheim';

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed) 
SELECT @MultiplayerProID, GameID, 'Playing', 8.0, 120.0 FROM Games WHERE Title = 'Palworld';

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed) 
SELECT @MultiplayerProID, GameID, 'Playing', 7.5, 50.0 FROM Games WHERE Title = 'Among Us';

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed) 
SELECT @MultiplayerProID, GameID, 'Completed', 8.0, 60.0 FROM Games WHERE Title = 'Fall Guys';

INSERT INTO UserGames (UserID, GameID, Status) 
SELECT @MultiplayerProID, GameID, 'Wishlist' FROM Games WHERE Title = 'Lethal Company';



DECLARE @RetroGamerID INT = (SELECT UserID FROM Users WHERE Email = 'retro@test.com');

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed, PersonalNotes) 
SELECT @RetroGamerID, GameID, 'Completed', 9.5, 50.0, 'Classic! Still holds up after all these years.' FROM Games WHERE Title = 'Portal 2';

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed) 
SELECT @RetroGamerID, GameID, 'Completed', 9.0, 200.0 FROM Games WHERE Title = 'Terraria';

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed) 
SELECT @RetroGamerID, GameID, 'Playing', 8.5, 300.0 FROM Games WHERE Title = 'Minecraft';

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed) 
SELECT @RetroGamerID, GameID, 'Completed', 8.0, 40.0 FROM Games WHERE Title = 'Half-Life: Alyx';

INSERT INTO UserGames (UserID, GameID, Status) 
SELECT @RetroGamerID, GameID, 'Backlog' FROM Games WHERE Title = 'Celeste';



DECLARE @NewPlayerID INT = (SELECT UserID FROM Users WHERE Email = 'newplayer@test.com');

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed, PersonalNotes) 
SELECT @NewPlayerID, GameID, 'Playing', 9.0, 15.0, 'My first RPG! Really enjoying it so far.' FROM Games WHERE Title = 'Stardew Valley';

INSERT INTO UserGames (UserID, GameID, Status) 
SELECT @NewPlayerID, GameID, 'Wishlist' FROM Games WHERE Title = 'Minecraft';

INSERT INTO UserGames (UserID, GameID, Status) 
SELECT @NewPlayerID, GameID, 'Wishlist' FROM Games WHERE Title = 'Among Us';

INSERT INTO UserGames (UserID, GameID, Status) 
SELECT @NewPlayerID, GameID, 'Backlog' FROM Games WHERE Title = 'Hades';



DECLARE @HardcoreGamerID INT = (SELECT UserID FROM Users WHERE Email = 'hardcore@test.com');

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed, PersonalNotes) 
SELECT @HardcoreGamerID, GameID, 'Completed', 10.0, 250.0, 'The ultimate challenge. NG+7 completed!' FROM Games WHERE Title = 'Elden Ring';

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed) 
SELECT @HardcoreGamerID, GameID, 'Completed', 9.5, 180.0 FROM Games WHERE Title = 'Sekiro: Shadows Die Twice';

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed) 
SELECT @HardcoreGamerID, GameID, 'Playing', 9.0, 120.0 FROM Games WHERE Title = 'Lies of P';

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed) 
SELECT @HardcoreGamerID, GameID, 'Completed', 9.5, 200.0 FROM Games WHERE Title = 'The Witcher 3: Wild Hunt';

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed) 
SELECT @HardcoreGamerID, GameID, 'Completed', 9.0, 150.0 FROM Games WHERE Title = 'DOOM Eternal';

INSERT INTO UserGames (UserID, GameID, Status, PersonalRating, HoursPlayed) 
SELECT @HardcoreGamerID, GameID, 'Playing', 8.5, 100.0 FROM Games WHERE Title = 'Starfield';

INSERT INTO UserGames (UserID, GameID, Status) 
SELECT @HardcoreGamerID, GameID, 'Wishlist' FROM Games WHERE Title = 'Baldur''s Gate 3';
GO

-- Seed reviewed reports
DECLARE @AdminUserIDForReports INT = (SELECT UserID FROM Users WHERE Email = 'admin@test.com');
DECLARE @User1ID INT = (SELECT UserID FROM Users WHERE Email = 'user@test.com');
DECLARE @User2ID INT = (SELECT UserID FROM Users WHERE Email = 'gamer@test.com');
DECLARE @User3ID INT = (SELECT UserID FROM Users WHERE Email = 'casual@test.com');
DECLARE @User4ID INT = (SELECT UserID FROM Users WHERE Email = 'collector@test.com');
DECLARE @User5ID INT = (SELECT UserID FROM Users WHERE Email = 'rpgfan@test.com');

INSERT INTO Reports (UserID, Type, Title, Content, Status, ReviewedBy, AdminNotes, CreatedAt, ReviewedAt) VALUES
(@User1ID, 'Suggestion', 'Add dark mode toggle', 'It would be great to have a dark mode toggle in the settings. The current dark theme is nice, but some users might prefer a light theme option.', 'Resolved', @AdminUserIDForReports, 'Great suggestion! We have added this feature to our roadmap and it will be available in the next update.', DATEADD(day, -15, GETDATE()), DATEADD(day, -12, GETDATE())),

(@User2ID, 'Report', 'Search functionality issue', 'When searching for games, sometimes the results don''t update immediately. I have to clear the search and type again for it to work properly.', 'Resolved', @AdminUserIDForReports, 'Fixed in the latest update. The search now updates in real-time as you type.', DATEADD(day, -10, GETDATE()), DATEADD(day, -8, GETDATE())),

(@User3ID, 'Suggestion', 'Export library to CSV', 'Would it be possible to add a feature to export my game library to a CSV file? This would be helpful for keeping backups or sharing with friends.', 'Reviewed', @AdminUserIDForReports, 'This is a good idea. We''ll consider adding this feature in a future release.', DATEADD(day, -8, GETDATE()), DATEADD(day, -6, GETDATE())),

(@User4ID, 'Report', 'Profile image not loading', 'My profile image URL is correct but the image is not displaying in my profile. It works fine when I open the URL directly in a browser.', 'Resolved', @AdminUserIDForReports, 'This was a CORS issue. We have updated the image loading mechanism and it should work now. Please try refreshing your profile image URL.', DATEADD(day, -7, GETDATE()), DATEADD(day, -5, GETDATE())),

(@User5ID, 'Suggestion', 'Add game completion percentage', 'It would be helpful to see a completion percentage for each game in my library, based on achievements or story progress if available.', 'Reviewed', @AdminUserIDForReports, 'Interesting feature request. We''ll look into integrating with achievement APIs where available.', DATEADD(day, -5, GETDATE()), DATEADD(day, -3, GETDATE())),

(@User1ID, 'Suggestion', 'Filter by release year', 'Could you add a filter option to sort or filter games by release year? This would help me find games from specific time periods.', 'Dismissed', @AdminUserIDForReports, 'We already have this feature in the advanced filters. Please check the filter panel on the library page.', DATEADD(day, -4, GETDATE()), DATEADD(day, -2, GETDATE())),

(@User2ID, 'Report', 'Slow loading on game details', 'When clicking on a game to view details, it takes a few seconds to load. This seems slower than it should be, especially for games with large descriptions.', 'Resolved', @AdminUserIDForReports, 'We have optimized the game details loading. The page should now load much faster. Thank you for reporting this!', DATEADD(day, -3, GETDATE()), DATEADD(day, -1, GETDATE())),

(@User3ID, 'Suggestion', 'Add wishlist notifications', 'It would be great to get notifications when games on my wishlist go on sale or get added to the catalog.', 'Reviewed', @AdminUserIDForReports, 'We are working on a notification system. This feature will be available soon.', DATEADD(day, -2, GETDATE()), DATEADD(hour, -12, GETDATE())),

-- Unreviewed reports and suggestions
(@User1ID, 'Suggestion', 'Add game tags system', 'It would be helpful to add custom tags to games in my library. For example, I could tag games as "co-op", "solo", "relaxing", etc. This would make organizing my collection much easier.', 'Unreviewed', NULL, NULL, DATEADD(day, -1, GETDATE()), NULL),

(@User2ID, 'Report', 'Rating system not saving', 'I tried to rate a game but the rating doesn''t seem to save. When I refresh the page, my rating is gone. This happens with multiple games.', 'Unreviewed', NULL, NULL, DATEADD(hour, -18, GETDATE()), NULL),

(@User4ID, 'Suggestion', 'Add playtime tracking per session', 'Could you add a feature to track individual play sessions? It would be nice to see when I played a game and for how long each time, not just the total hours.', 'Unreviewed', NULL, NULL, DATEADD(hour, -12, GETDATE()), NULL),

(@User5ID, 'Report', 'Export button not working', 'When I try to export my library, the button doesn''t do anything. No file dialog appears and no error message is shown. I''ve tried multiple times.', 'Unreviewed', NULL, NULL, DATEADD(hour, -6, GETDATE()), NULL);
GO